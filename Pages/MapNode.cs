using System.Text.Json;
using LoraStatsNet.Database.Entities;

namespace LoraStatsNet.Pages;

struct MapNode
{
	public required string shortName { get; set; }
	public required string longName { get; set; }
	public required string foreColor { get; set; }
	public required string backColor { get; set; }
	public required string url { get; set; }
	public required double[]? coordinates { get; set; }
	public required string lastSeen { get; set; }
	public required string lastPositionUpdate { get; set; }
	public object? extra { get; set; }

	public static MapNode ForNode(Node node, object? extra = null) => new MapNode
	{
		shortName = node.ShortNameOrDefault,
		longName = node.LongNameOrDefault,
		foreColor = node.NodeColors.foreColor,
		backColor = node.NodeColors.backColor,
		url = $"/Node/{node.Ref.Id}",
		coordinates = node.Coordinates?.ForMap,
		lastSeen = node.LastSeen.ToString("yyyy-MM-dd HH:mm:ss"),
		lastPositionUpdate = node.LastPositionUpdate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-",
		extra = extra
	};

	public static string JsonForNode(Node node) => Formatter.FormatJSON(node);
	public static string JsonForNodes(IEnumerable<Node> nodes) => Formatter.FormatJSON(nodes.Select(node => ForNode(node)));
	public static string JsonForNodes<TElement>(IEnumerable<TElement> elements, Func<TElement, Node> nodeSelector, Func<TElement, object> extraSelector)
		=> Formatter.FormatJSON(elements.Select(element => ForNode(nodeSelector(element), extraSelector(element))));

	public static void Ungroup(IEnumerable<Node> nodes)
	{
		var nodeGroups = nodes
			.Where(node => node.HasValidLocation)
			.GroupBy(node => (node.LastLatitude, node.LastLongitude))
			.Where(g => g.Count() > 1)
			.SelectMany(g => g.Select((node, i) => (node, i)));

		foreach (var (node, i) in nodeGroups)
			node.LastLongitude += i * 0.0002;
	}
}
