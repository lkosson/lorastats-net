using System.Diagnostics.CodeAnalysis;

namespace LoraStatsNet.Services;

readonly struct Coordinates
{
	public double Latitude { get; init; }
	public double Longitude { get; init; }

	public string LatitudeFmt => Math.Abs(Math.Round(Latitude, 7)).ToString("0.000000") + "° " + (Latitude > 0 ? "N" : "S");
	public string LongitudeFmt => Math.Abs(Math.Round(Longitude, 7)).ToString("0.000000") + "° " + (Longitude > 0 ? "E" : "W");

	public Coordinates(double latitude, double longitude)
	{
		Latitude = latitude;
		Longitude = longitude;
	}

	public override string ToString() => $"{LatitudeFmt}, {LongitudeFmt}";
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Coordinates other && other.Latitude == Latitude && other.Longitude == Longitude;
	public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);
	public static bool operator ==(Coordinates coordinates1, Coordinates coordinates2) => coordinates1.Latitude == coordinates2.Latitude && coordinates1.Longitude == coordinates2.Longitude;
	public static bool operator !=(Coordinates coordinates1, Coordinates coordinates2) => coordinates1.Latitude != coordinates2.Latitude || coordinates1.Longitude != coordinates2.Longitude;
}
