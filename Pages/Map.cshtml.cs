using System.Globalization;
using LoraStatsNet.Database;
using LoraStatsNet.Database.Entities;
using Meshtastic.Protobufs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Pages;

class MapModel(LoraStatsNetDb db) : PageModel, IPageWithTitle
{
	public string Title => $"Map - {Community}";
	[FromRoute(Name = "Community")] public string CommunityUrl { get; set; } = default!;

	public Community Community { get; set; } = default!;
	public string Nodes { get; set; } = default!;
	public IReadOnlyCollection<(Node fromNode, Node toNode, string color, int traceroutes, int reports, int neighbors)> Links { get; private set; } = default!;
	public int HistoryHours => 24;

	public async Task<IActionResult> OnGetAsync()
	{
		var community = await db.Communities.FirstOrDefaultAsync(community => community.UrlName == CommunityUrl);
		if (community == null) return NotFound();
		Community = community;

		var activeNodes = await db.Nodes.Where(node => node.CommunityId == Community && node.LastLatitude.HasValue && node.LastLongitude.HasValue && node.LastSeen >= DateTime.Now.AddHours(-HistoryHours)).ToListAsync();
		var reports = await db.PacketReports
			.Include(report => report.Packet)
			.Where(report => (report.Packet.FromNode.CommunityId == Community || report.Gateway.CommunityId == Community) && report.ReceptionTime >= DateTime.Now.AddHours(-HistoryHours))
			.Select(report => new { PacketRef = report.Packet.Ref, report.Packet.Port, report.Packet.ParsedPayload, report.Packet.FromNodeId, report.GatewayId, report.HopLimit, report.Packet.HopStart })
			.ToListAsync();
		MapNode.Ungroup(activeNodes);
		Nodes = MapNode.JsonForNodes(activeNodes);

		var activeNodesByNodeId = activeNodes.ToDictionary(node => node.NodeId);
		var activeNodesByRef = activeNodes.ToDictionary(node => node.Ref);

		IEnumerable<(uint fromNodeId, uint toNodeId)> RouteNodes(string? route)
		{
			if (String.IsNullOrEmpty(route)) yield break;
			var nodeIds = route.Split(',')
				.Select(f => UInt32.TryParse(f, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var nodeId) ? nodeId : 0)
				.Where(f => f != 0 && f != UInt32.MaxValue)
				.ToList();

			for (var i = 1; i < nodeIds.Count; i++)
			{
				yield return (nodeIds[i - 1], nodeIds[i]);
			}
		}

		var linksByTraceroute = reports
			.Where(report => report.Port == PortNum.TracerouteApp)
			.GroupBy(report => report.PacketRef)
			.Select(g => g.First())
			.SelectMany(e => RouteNodes(e.ParsedPayload))
			.Select(e => (fromNode: activeNodesByNodeId.GetValueOrDefault(e.fromNodeId), toNode: activeNodesByNodeId.GetValueOrDefault(e.toNodeId), source: MapLinkSource.Traceroute));

		var linksReports = reports
			.Where(report => report.HopStart == report.HopLimit)
			.Select(report => (fromNode: activeNodesByRef.GetValueOrDefault(report.FromNodeId), toNode: activeNodesByRef.GetValueOrDefault(report.GatewayId), source: MapLinkSource.Report));

		var linksByNeighbor = reports
			.Where(report => report.Port == PortNum.NeighborinfoApp)
			.GroupBy(report => report.PacketRef)
			.Select(g => g.First())
			.SelectMany(e => RouteNodes(e.ParsedPayload))
			.Select(e => (fromNode: activeNodesByNodeId.GetValueOrDefault(e.fromNodeId), toNode: activeNodesByNodeId.GetValueOrDefault(e.toNodeId), source: MapLinkSource.Neighbor));

		Links =
			linksByTraceroute
			.Concat(linksReports)
			.Concat(linksByNeighbor)
			.Where(e => e.fromNode != null && e.toNode != null && e.fromNode.HasValidLocation && e.toNode.HasValidLocation)
			.Select(e => e.fromNode!.Ref.Id < e.toNode!.Ref.Id ? (e.fromNode, e.toNode, e.source) : (fromNode: e.toNode, toNode: e.fromNode, e.source))
			.GroupBy(e => (e.fromNode, e.toNode))
			.Select(e => (
				e.Key.fromNode,
				e.Key.toNode,
				color: ColorPalette.Value(e.Count(f => f.source != MapLinkSource.Neighbor), 0, 50, 10),
				traceroutes: e.Count(f => f.source == MapLinkSource.Traceroute),
				reports: e.Count(f => f.source == MapLinkSource.Report),
				neighbors: e.Count(f => f.source == MapLinkSource.Neighbor))
			).ToList();

		return Page();
	}

	public enum MapLinkSource { Traceroute, Report, Neighbor }
}
