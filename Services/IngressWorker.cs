using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using Google.Protobuf;
using LoraStatsNet.Database;
using LoraStatsNet.Database.Entities;
using Meshtastic.Crypto;
using Meshtastic.Protobufs;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Services;

internal class IngressWorker(ILogger logger, IServiceProvider serviceProvider, MeshCrypto meshCrypto)
{
	public static SemaphoreSlim MUTEX = new(1);

	private static async Task<Node> GetOrCreateNodeAsync(LoraStatsNetDb db, uint nodeId, Node? gatewayNode = null)
	{
		if (nodeId == UInt32.MaxValue) return Node.BROADCAST;
		var changed = false;
		var node = await db.Nodes.FirstOrDefaultAsync(node => node.NodeId == nodeId);
		if (node == null)
		{
			node = new Node { NodeId = nodeId, LastSeen = DateTime.Now };
			changed = true;
		}
		if (gatewayNode != null)
		{
			if ((DateTime.Now - node.LastSeen).TotalSeconds > 60)
			{
				node.LastSeen = DateTime.Now;
				changed = true;
			}
			if (gatewayNode.HasValidLocation && !node.HasValidLocation && node.CommunityId != gatewayNode.CommunityId)
			{
				node.CommunityId = gatewayNode.CommunityId;
				changed = true;
			}
		}
		if (changed) await db.StoreAsync(node);
		return node;
	}

	protected async Task ProcessServiceEnvelope(byte[] blob)
	{
		var serviceEnvelope = ServiceEnvelope.Parser.ParseFrom(blob);
		var meshPacket = serviceEnvelope.Packet;
		if (meshPacket == null) return;

		if (!UInt32.TryParse(serviceEnvelope.GatewayId.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var gatewayId) || gatewayId == 0 || gatewayId == UInt32.MaxValue)
		{
			logger.LogWarning("Invalid gateway {id} for packet {blob}", serviceEnvelope.GatewayId, Convert.ToBase64String(blob));
			return;
		}

		await ProcessPacket(meshPacket, gatewayId);
	}

	protected async Task ProcessMeshPacket(byte[] blob, uint gatewayId)
	{
		var meshPacket = MeshPacket.Parser.ParseFrom(blob);
		await ProcessPacket(meshPacket, gatewayId);
	}

	private async Task ProcessPacket(MeshPacket meshPacket, uint gatewayId)
	{
		try
		{
			await MUTEX.WaitAsync();
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<LoraStatsNetDb>();
			await ProcessPacketUnsafe(db, meshPacket, gatewayId);
		}
		catch (Exception exc)
		{
			logger.LogError("Error processing packet {blob}\n{exc}", Convert.ToBase64String(meshPacket.ToByteArray()), exc);
		}
		finally
		{
			MUTEX.Release();
		}
	}

	private async Task ProcessPacketUnsafe(LoraStatsNetDb db, MeshPacket meshPacket, uint gatewayId)
	{
		using var tx = await db.BeginTransactionAsync();

		var gatewayNode = await GetOrCreateNodeAsync(db, gatewayId);
		var fromNode = await GetOrCreateNodeAsync(db, meshPacket.From, gatewayNode);
		var toNode = await GetOrCreateNodeAsync(db, meshPacket.To);
		var packet = await db.Packets.FirstOrDefaultAsync<Packet>(e => e.PacketId == meshPacket.Id && e.FromNode == fromNode && e.FirstSeen > DateTime.Now.AddMinutes(-15));
		if (packet == null || packet.Port == PortNum.TracerouteApp)
		{
			packet ??= new Packet
			{
				PacketId = meshPacket.Id,
				FromNode = fromNode,
				ToNode = toNode,
				HopStart = (byte)meshPacket.HopStart,
				WantAck = meshPacket.WantAck,
				Channel = (byte)meshPacket.Channel,
				FirstSeen = DateTime.Now
			};

			var channel = meshCrypto.TryDecode(meshPacket);

			if (meshPacket.Decoded == null)
			{
				logger.LogDebug("Unable to decode packet {packetId}", packet.PacketIdFmt);
			}
			else
			{
				var fromNodeDirty = false;
				var fromNodePositionUpdated = false;
				var parsedPayload = packet.ParsedPayload ?? "";
				packet.Port = meshPacket.Decoded.Portnum;
				packet.WantResponse = meshPacket.Decoded.WantResponse;
				packet.BitField = (byte)meshPacket.Decoded.Bitfield;
				packet.RequestId = meshPacket.Decoded.RequestId;

				if (meshPacket.Decoded.Portnum == PortNum.TelemetryApp) ProcessTelemetry(fromNode, ref fromNodeDirty, ref parsedPayload, meshPacket.Decoded);
				else if (meshPacket.Decoded.Portnum == PortNum.PositionApp) ProcessPosition(fromNode, ref fromNodePositionUpdated, ref parsedPayload, meshPacket.Decoded);
				else if (meshPacket.Decoded.Portnum == PortNum.NodeinfoApp) ProcessNodeInfo(fromNode, ref fromNodeDirty, ref parsedPayload, meshPacket.Decoded);
				else if (meshPacket.Decoded.Portnum == PortNum.TracerouteApp) ProcessTraceroute(fromNode, toNode, ref fromNodeDirty, ref parsedPayload, meshPacket.Decoded);
				else if (meshPacket.Decoded.Portnum == PortNum.NeighborinfoApp) ProcessNeighborInfo(fromNode, ref fromNodeDirty, ref parsedPayload, meshPacket.Decoded);
				else if (meshPacket.Decoded.Portnum == PortNum.RoutingApp) ProcessRouting(fromNode, ref fromNodeDirty, ref parsedPayload, meshPacket.Decoded);
				else if (meshPacket.Decoded.Portnum == PortNum.TextMessageApp) ProcessTextMessage(fromNode, ref fromNodeDirty, ref parsedPayload, meshPacket.Decoded);
				else if (meshPacket.Decoded.Portnum == PortNum.MapReportApp) { }
				else if (meshPacket.Decoded.Portnum == PortNum.RangeTestApp) ProcessRangeTest(fromNode, ref fromNodeDirty, ref parsedPayload, meshPacket.Decoded);
				else if (meshPacket.Decoded.Portnum == PortNum.AdminApp) { }
				else { }

				if (fromNodePositionUpdated)
				{
					var matchingAreas = await db.CommunityAreas
						.Where(communityArea => communityArea.LatitudeMin <= fromNode.LastLatitude && communityArea.LatitudeMax >= fromNode.LastLatitude
							&& communityArea.LongitudeMin <= fromNode.LastLongitude && communityArea.LongitudeMax >= fromNode.LastLongitude)
						.ToListAsync();
					var smallestArea = matchingAreas.OrderBy(communityArea => communityArea.Area.AreaKm).FirstOrDefault();
					
					fromNode.CommunityId = smallestArea?.CommunityId ?? default;
					fromNodeDirty = true;
				}

				if (fromNodeDirty) await db.StoreAsync(fromNode);
			}

			if (fromNode.CommunityId.IsNull)
			{
				logger.LogDebug("Ignoring packet from node \"{node}\" belonging unknown community.", fromNode);
				return;
			}

			await db.StoreAsync(packet);
		}

		var packetReport = await db.PacketReports.FirstOrDefaultAsync(e => e.Packet == packet && e.Gateway == gatewayNode);
		if (packetReport != null) return;

		packetReport = new PacketReport
		{
			Packet = packet,
			Gateway = gatewayNode,
			ReceptionTime = DateTime.Now,
			SNR = meshPacket.RxSnr,
			RSSI = (byte)meshPacket.RxRssi,
			HopLimit = (byte)meshPacket.HopLimit,
			RelayNode = (byte)meshPacket.RelayNode,
			NextHop = (byte)meshPacket.NextHop,
		};

		await db.StoreAsync(packetReport);

		var packetData = new PacketData
		{
			PacketReport = packetReport,
			RawData = meshPacket.ToByteArray()
		};
		await db.StoreAsync(packetData);

		logger.LogInformation("Stored packet {id} from {fromNode} reported by {gateway} as {ref} for {packet}", packet.PacketIdFmt, fromNode.NodeIdFmt, gatewayNode.NodeIdFmt, packetReport.Ref, packet.Ref);

		await tx.CommitAsync();
	}

	private static void ProcessDeviceMetrics(Node fromNode, ref bool fromNodeDirty, ref string parsedPayload, DeviceMetrics deviceMetrics)
	{
		if (deviceMetrics.UptimeSeconds > 0)
		{
			fromNode.LastBoot = DateTime.Now.AddSeconds(-deviceMetrics.UptimeSeconds);
			fromNodeDirty = true;
		}
		if (deviceMetrics.ChannelUtilization > 0)
		{
			if (parsedPayload.Length > 0) parsedPayload += ", ";
			parsedPayload += deviceMetrics.ChannelUtilization.ToString("0.00") + "% ChUtil";
		}
		if (deviceMetrics.AirUtilTx > 0)
		{
			if (parsedPayload.Length > 0) parsedPayload += ", ";
			parsedPayload += deviceMetrics.AirUtilTx.ToString("0.00") + "% AirUtilTx";
		}
	}

	private static void ProcessEnvironmentMetrics(ref string parsedPayload, EnvironmentMetrics environmentMetrics)
	{
		if (environmentMetrics.HasTemperature)
		{
			if (parsedPayload.Length > 0) parsedPayload += ", ";
			parsedPayload += environmentMetrics.Temperature.ToString("0.0") + " °C";
		}
		if (environmentMetrics.HasBarometricPressure)
		{
			if (parsedPayload.Length > 0) parsedPayload += ", ";
			parsedPayload += environmentMetrics.BarometricPressure.ToString("0") + " hPa";
		}
		if (environmentMetrics.HasIaq)
		{
			if (parsedPayload.Length > 0) parsedPayload += ", ";
			parsedPayload += environmentMetrics.Iaq.ToString("0") + " IAQ";
		}
	}

	private static void ProcessTelemetry(Node fromNode, ref bool fromNodeDirty, ref string parsedPayload, Data data)
	{
		var telemetry = Telemetry.Parser.ParseFrom(data.Payload);
		if (telemetry.DeviceMetrics != null) ProcessDeviceMetrics(fromNode, ref fromNodeDirty, ref parsedPayload, telemetry.DeviceMetrics);
		if (telemetry.EnvironmentMetrics != null) ProcessEnvironmentMetrics(ref parsedPayload, telemetry.EnvironmentMetrics);
	}

	private static void ProcessPosition(Node fromNode, ref bool fromNodePositionUpdated, ref string parsedPayload, Data data)
	{
		var position = Position.Parser.ParseFrom(data.Payload);
		if (fromNode.LastPositionPrecision > position.PrecisionBits && fromNode.HasRecentLocation) return;
		if (position.HasLatitudeI) fromNode.LastLatitude = Math.Round(position.LatitudeI * 1e-7, 7);
		if (position.HasLongitudeI) fromNode.LastLongitude = Math.Round(position.LongitudeI * 1e-7, 7);
		if (position.HasAltitude) fromNode.LastElevation = position.Altitude;
		if (fromNode.LastLatitude.HasValue && fromNode.LastLongitude.HasValue)
		{
			fromNode.LastPositionUpdate = DateTime.Now;
			fromNode.LastPositionPrecision = (int)position.PrecisionBits;
			fromNodePositionUpdated = true;
			parsedPayload = fromNode.Coordinates.ToString()!;
		}
	}

	private static void ProcessNodeInfo(Node fromNode, ref bool fromNodeDirty, ref string parsedPayload, Data data)
	{
		var user = User.Parser.ParseFrom(data.Payload);
		fromNode.ShortName = user.ShortName;
		fromNode.LongName = user.LongName;
		fromNode.Role = user.Role;
		fromNode.HwModel = user.HwModel;
		fromNode.PublicKey = user.PublicKey?.ToBase64();
		fromNodeDirty = true;
	}

	private static void ProcessTraceroute(Node fromNode, Node toNode, ref bool fromNodeDirty, ref string parsedPayload, Data data)
	{
		var route = RouteDiscovery.Parser.ParseFrom(data.Payload);
		var sb = new StringBuilder();
		if (data.RequestId == 0)
		{
			sb.Append(fromNode.NodeIdFmt);
			for (int i = 0; i < route.Route.Count; i++)
			{
				sb.Append(",");
				sb.Append(route.Route[i].ToString("x8"));
			}
			if (route.Route.Count == route.SnrTowards.Count - 1) sb.Append(toNode.NodeIdFmt);
		}
		else
		{
			sb.Append(toNode.NodeIdFmt);
			for (int i = 0; i < route.Route.Count; i++)
			{
				sb.Append(",");
				sb.Append(route.Route[i].ToString("x8"));
			}
			if (route.Route.Count == route.SnrTowards.Count - 1)
			{
				sb.Append(",");
				sb.Append(fromNode.NodeIdFmt);
			}
			for (int i = 0; i < route.RouteBack.Count; i++)
			{
				sb.Append(",");
				sb.Append(route.RouteBack[i].ToString("x8"));
			}
		}
		if (parsedPayload.Length < sb.Length) parsedPayload = sb.ToString();
	}

	private static void ProcessNeighborInfo(Node fromNode, ref bool fromNodeDirty, ref string parsedPayload, Data data)
	{
		var neighbors = NeighborInfo.Parser.ParseFrom(data.Payload);
		parsedPayload = String.Join(",", neighbors.Neighbors.Select(e => e.NodeId.ToString("x8")));
	}

	private static void ProcessRouting(Node fromNode, ref bool fromNodeDirty, ref string parsedPayload, Data data)
	{
		var routing = Routing.Parser.ParseFrom(data.Payload);
		parsedPayload = routing.ErrorReason.ToString();
	}

	private static void ProcessTextMessage(Node fromNode, ref bool fromNodeDirty, ref string parsedPayload, Data data)
	{
		parsedPayload = Encoding.UTF8.GetString(data.Payload.ToByteArray());
	}

	private static void ProcessRangeTest(Node fromNode, ref bool fromNodeDirty, ref string parsedPayload, Data data)
	{
		parsedPayload = Encoding.UTF8.GetString(data.Payload.ToByteArray());
	}
}
