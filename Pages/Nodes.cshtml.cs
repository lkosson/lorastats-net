using LoraStatsNet.Database;
using LoraStatsNet.Database.Entities;
using LoraStatsNet.Services;
using Meshtastic.Protobufs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Pages;

class NodesModel(LoraStatsNetDb db, Configuration configuration) : PageModel, IPageWithTitle
{
	public string Title => $"Nodes - {Community}";
	[FromRoute(Name = "Community")] public string CommunityUrl { get; set; } = default!;
	[FromRoute(Name = "Role")] public string? RoleName { get; set; }

	public Community Community { get; set; } = default!;
	public List<(int nr, Node node)> ActiveNodes { get; private set; } = default!;
	public List<(int nr, Node node, int packetCount, int posCount, int infoCount, int neighCount, int tracerouteCount, int telemetryCount, int textCount)> SpammingNodes { get; private set; } = default!;
	public List<(int nr, Node node)> TopUptime { get; private set; } = default!;
	public int TotalPackets { get; set; }
	public List<(Node node, DateTime lastReception, int packetCount)> ActiveGateways { get; private set; } = default!;
	public List<(string role, int count)> NodesByRole { get; private set; } = default!;
	public List<(string model, int count)> NodesByModel { get; private set; } = default!;
	public Dictionary<EntityRef<Node>, int[]> NodeActivity { get; private set; } = default!;
	public int HistoryHours => configuration.ReportingHours;
	public int ActivityBuckets => 8;
	public int HoursPerBucket => HistoryHours / ActivityBuckets;
	public string[] BucketHours { get; private set; } = default!;

	public async Task<IActionResult> OnGetAsync()
	{
		var community = await db.Communities.FirstOrDefaultAsync(community => community.UrlName == CommunityUrl);
		if (community == null) return NotFound();
		Community = community;

		var gatewayStats = await db.PacketReports
			.Where(report => report.Gateway.CommunityId == Community && report.ReceptionTime >= DateTime.Now.AddHours(-HistoryHours))
			.GroupBy(report => report.GatewayId)
			.Select(g => new { Gateway = g.Key, LastReport = g.Max(report => report.ReceptionTime), PacketCount = g.Count() })
			.ToListAsync();
		var packets = await db.Packets
			.Where(packet => packet.FromNode.CommunityId == Community && packet.FirstSeen >= DateTime.Now.AddHours(-HistoryHours))
			.Select(packet => new { packet.FromNodeId, packet.Port, packet.FirstSeen })
			.ToListAsync();
		var nodes = await db.Nodes.Where(node => node.CommunityId == Community && node.LastSeen >= DateTime.Now.AddHours(-HistoryHours)).ToListAsync();
		if (Enum.TryParse<Config.Types.DeviceConfig.Types.Role>(RoleName, out var role)) nodes = nodes.Where(e => e.Role == role).ToList();

		var nodesByRef = nodes.ToDictionary(node => node.Ref);
		var packetsByNode = packets.ToLookup(packet => packet.FromNodeId, packet => packet.Port);
		var packetsByNodeHour = new Dictionary<EntityRef<Node>, int[]>();

		BucketHours = Enumerable.Range(0, ActivityBuckets)
			.Select(n => DateTime.Now.AddHours(-n * HoursPerBucket))
			.Select(endTime => (startTime: endTime.AddHours(-HoursPerBucket), endTime))
			.Select(range => $"{range.startTime:H:mm} - {range.endTime:H:mm}")
			.ToArray();

		foreach (var packet in packets)
		{
			if (!packetsByNodeHour.TryGetValue(packet.FromNodeId, out var stats))
			{
				stats = new int[ActivityBuckets];
				packetsByNodeHour[packet.FromNodeId] = stats;
			}
			var bucket = (int)((DateTime.Now - packet.FirstSeen).TotalHours / HoursPerBucket);
			if (bucket >= stats.Length) continue;
			stats[bucket]++;
		}
		NodeActivity = packetsByNodeHour;

		ActiveNodes = nodes
			.Where(node => packetsByNodeHour.ContainsKey(node))
			.OrderByDescending(node => node.LastSeen)
			.Select((e, i) => (nr: i + 1, node: e))
			.ToList();
		TopUptime = nodes
			.Where(node => node.LastBoot.HasValue)
			.OrderBy(node => node.LastBoot!.Value)
			.Take(25)
			.Select((node, i) => (nr: i + 1, node))
			.ToList();
		SpammingNodes = nodes
			.Where(node => packetsByNodeHour.ContainsKey(node))
			.Select(node => (node, packets: packetsByNode[node]))
			.OrderByDescending(e => e.packets.Count())
			.Select((e, i) => (
				nr: i + 1,
				e.node,
				packetCount: e.packets.Count(),
				posCount: e.packets.Where(packet => packet == PortNum.PositionApp).Count(),
				infoCount: e.packets.Where(packet => packet == PortNum.NodeinfoApp).Count(),
				neighCount: e.packets.Where(packet => packet == PortNum.NeighborinfoApp).Count(),
				tracerouteCount: e.packets.Where(packet => packet == PortNum.TracerouteApp).Count(),
				telemetryCount: e.packets.Where(packet => packet == PortNum.TelemetryApp).Count(),
				textCount: e.packets.Where(packet => packet == PortNum.TextMessageApp).Count()
			))
			.Take(25)
			.ToList();
		TotalPackets = packets.Count;
		ActiveGateways = gatewayStats
			.Select(gateway => (
				node: nodesByRef.GetValueOrDefault(gateway.Gateway)!,
				lastReception: gateway.LastReport,
				packetCount: gateway.PacketCount))
			.Where(e => e.node != null)
			.OrderByDescending(e => e.packetCount)
			.Where(e => e.node != null)
			.ToList();
		NodesByRole = nodes
			.Select(node => node.Role)
			.Where(role => role.HasValue)
			.GroupBy(role => role!.Value)
			.Select(role => (role: role.Key.ToString(), count: role.Count()))
			.OrderByDescending(e => e.count)
			.ToList();
		NodesByModel = nodes
			.Select(node => node.HwModel)
			.Where(model => model.HasValue)
			.GroupBy(model => model!.Value)
			.Select(model => (role: model.Key.ToString(), count: model.Count()))
			.OrderByDescending(e => e.count)
			.ToList();

		return Page();
	}
}
