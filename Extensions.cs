namespace LoraStatsNet;

static class Extensions
{
	public static string LongitudeFmt(this int longitude) => ((double?)Math.Round(longitude * 1e-7, 7)).LongitudeFmt();
	public static string LatitudeFmt(this int latitude) => ((double?)Math.Round(latitude * 1e-7, 7)).LatitudeFmt();
	public static string LongitudeFmt(this double? longitude) => longitude is null ? "" : Math.Abs(Math.Round(longitude.Value, 7)).ToString("0.000000") + "° " + (longitude > 0 ? "E" : "W");
	public static string LatitudeFmt(this double? latitude) => latitude is null ? "" : Math.Abs(Math.Round(latitude.Value, 7)).ToString("0.000000") + "° " + (latitude > 0 ? "N" : "S");
}
