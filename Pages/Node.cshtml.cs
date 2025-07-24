using System.Linq;
using LoraStatsNet.Database;
using LoraStatsNet.Database.Entities;
using LoraStatsNet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Pages;

class NodeModel(LoraStatsNetDb db) : PageModel, IPageWithTitle
{
	public string Title => $"Node \"{Node?.LongNameOrDefault}\"";
	[FromRoute(Name = "id")] public EntityRef<Node> NodeRef { get; set; }

	public Node Node { get; set; } = default!;
	public List<Packet> PacketsSent { get; set; } = default!;
	public List<Packet> PacketsReceived { get; set; } = default!;
	public List<(Node node, double? distance, int[] hopStats, float hopAvg, PacketReport lastReport, int packetPct)> NodesReached { get; set; } = default!;
	public List<(Node node, double? distance, byte rssi, float snr)> NodesReachedDirectly { get; set; } = default!;

	public async Task<IActionResult> OnGetAsync()
	{
		if (NodeRef.IsNull) return BadRequest();
		var node = await db.Nodes.Include(node => node.Community).FirstOrDefaultAsync(node => node.Ref == NodeRef);
		if (node == null) return NotFound();
		var reports = await db.PacketReports
			.Include(report => report.Packet)
			.Include(report => report.Gateway)
			.Where(report => report.Packet.FromNodeId == NodeRef || report.Packet.ToNodeId == NodeRef)
			.ToListAsync();
		var packets = reports.Select(report => report.Packet).Distinct().ToList();

		RouteData.Values["community"] = node.Community?.UrlName;

		(int[] pct, float avg) HopStats(IEnumerable<PacketReport> reports)
		{
			int[] hops = new int[8];
			int count = 0;
			var sum = 0;
			foreach (var report in reports)
			{
				int hopsAway = report.Packet.HopStart - report.HopLimit;
				if (hopsAway < 0) hopsAway = 0;
				if (hopsAway > 7) hopsAway = 7;
				hops[hopsAway]++;
				count++;
				sum += hopsAway;
			}
			for (int i = 0; i < hops.Length; i++)
				hops[i] = hops[i] * 100 / count;
			return (hops, 1.0f * sum / count);
		}

		Node = node;
		PacketsSent = packets
			.Where(packet => packet.FromNodeId == node)
			.OrderByDescending(packet => packet.FirstSeen)
			.ToList();

		PacketsReceived = packets
			.Where(packet => packet.ToNodeId == node)
			.OrderByDescending(packet => packet.FirstSeen)
			.ToList();

		NodesReached = reports
			.Where(report => report.Packet.FromNodeId == node)
			.GroupBy(report => report.Gateway)
			.Select(gateway => (
				gateway: gateway.Key,
				hopStats: HopStats(gateway),
				lastReport: gateway.OrderBy(report => report.ReceptionTime).Last(),
				packetCount: gateway.Select(report => report.PacketId).Distinct().Count()))
			.Distinct()
			.Where(e => e.gateway != null && e.gateway != node)
			.Select(e => (
				node: e.gateway!,
				distance: e.gateway!.DistanceTo(node),
				hopStats: e.hopStats.pct,
				hopAvg: e.hopStats.avg,
				e.lastReport,
				packetPct: 100 * e.packetCount / PacketsSent.Count))
			.OrderByDescending(e => e.distance)
			.ThenByDescending(e => e.packetPct)
			.ThenBy(e => e.node.Ref.Id)
			.ToList();

		NodesReachedDirectly = reports
			.Where(report => report.Packet.FromNodeId == node)
			.Where(report => report.HopLimit == report.Packet.HopStart)
			.GroupBy(report => report.Gateway)
			.Select(gateway => (gateway: gateway.Key, reports: gateway))
			.Where(e => e.gateway != node)
			.Select(e => (
				node: e.gateway,
				distance: e.gateway.DistanceTo(node),
				rssi: e.reports.Max(f => f.RSSI),
				snr: e.reports.Max(f => f.SNR)))
			.OrderByDescending(e => e.distance)
			.ThenBy(e => e.snr)
			.ThenBy(e => e.node.Ref.Id)
			.ToList();

		MapNode.Ungroup(NodesReached.Select(e => e.node));

		return Page();
	}
}
