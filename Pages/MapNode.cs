using System.Text.Json;
using LoraStatsNet.Database.Entities;

namespace LoraStatsNet.Pages;

struct MapNode
{
	public required string shortName { get; set; }
	public required string longName { get; set; }
	public required string foreColor { get; set; }
	public required string backColor { get; set; }
	public required double[]? coordinates { get; set; }
	public required string lastSeen { get; set; }
	public required string lastPositionUpdate { get; set; }

	public static MapNode ForNode(Node node) => new MapNode
	{
		shortName = node.ShortNameOrDefault,
		longName = node.LongNameOrDefault,
		foreColor = node.NodeColors.foreColor,
		backColor = node.NodeColors.backColor,
		coordinates = node.Coordinates?.ForMap,
		lastSeen = node.LastSeen.ToString("yyyy-MM-dd HH:mm:ss"),
		lastPositionUpdate = node.LastPositionUpdate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-"
	};

	public static string JsonForNodes(IEnumerable<Node> nodes) => JsonSerializer.Serialize(nodes.Select(ForNode));
}
