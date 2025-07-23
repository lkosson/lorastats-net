using System.Globalization;
using Meshtastic.Protobufs;

namespace LoraStatsNet.Database.Entities;

public class Packet : Entity<Packet>
{
	public uint PacketId { get; set; }
	public EntityRef<Node> FromNodeId { get; set; }
	public EntityRef<Node>? ToNodeId { get; set; }
	public DateTime FirstSeen { get; set; }
	public byte HopStart { get; set; }
	public bool WantAck { get; set; }
	public bool WantResponse { get; set; }
	public PortNum Port { get; set; }
	public byte BitField { get; set; }
	public string? ParsedPayload { get; set; }
	public byte Channel { get; set; }
	public uint RequestId { get; set; }

	public Node FromNode { get; set; } = default!;
	public Node ToNode { get; set; } = default!;
	public List<PacketReport> Reports { get; set; } = default!;

	public uint[] ParsedPayloadNodeIds => (ParsedPayload?.Split(',') ?? [])
		.Select(stringId => UInt32.TryParse(stringId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var nodeId) && nodeId != 0 && nodeId != 0xffffffff ? nodeId : 0)
		.Where(nodeId => nodeId != 0)
		.ToArray();

	public string PacketIdFmt => PacketId.ToString("x8");
	public string PortFmt => Port == PortNum.UnknownApp ? "" : Port.ToString();
	public string ChannelFmt => Channel == 0 ? "" : Channel.ToString("X2");
	public string BitFieldFmt => BitField.ToString("X2");
	public string RequestIdFmt => RequestId == 0 ? "" : RequestId.ToString("x8");
}