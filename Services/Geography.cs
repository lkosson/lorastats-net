using LoraStatsNet.Database.Entities;

namespace LoraStatsNet.Services;

static class Geography
{
	public static double Distance(this Coordinates coordinates1, Coordinates coordinates2)
	{
		var dlat = (double)(coordinates1.Latitude - coordinates2.Latitude);
		var dlon = (double)(coordinates1.Longitude - coordinates2.Longitude);
		var midlat = (double)(coordinates1.Latitude + coordinates2.Latitude) / 2 * Math.PI / 180;
		var k1 = 111.13209 - 0.56605 * Math.Cos(2 * midlat) + 0.00120 * Math.Cos(4 * midlat);
		var k2 = 111.41513 * Math.Cos(midlat) - 0.09455 * Math.Cos(3 * midlat) + 0.00012 * Math.Cos(5 * midlat);
		var p1 = k1 * dlat;
		var p2 = k2 * dlon;
		var d = Math.Sqrt(p1 * p1 + p2 * p2);
		return Math.Round(d, 3);
	}

	public static double? DistanceTo(this Node node1, Node node2)
	{
		if (!node1.HasValidLocation) return null;
		if (!node2.HasValidLocation) return null;
		return Distance(node1.Coordinates!.Value, node2.Coordinates!.Value);
	}
}
