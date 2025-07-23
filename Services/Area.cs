using System.Diagnostics.CodeAnalysis;

namespace LoraStatsNet.Services;

public readonly struct Area
{
	public Coordinates CoordinatesMin { get; init; }
	public Coordinates CoordinatesMax { get; init; }
	public Coordinates MidPoint => new((CoordinatesMin.Latitude + CoordinatesMax.Latitude) / 2, (CoordinatesMin.Longitude + CoordinatesMax.Longitude) / 2);
	public double LatitudeExtentKm => Geography.Distance(new(CoordinatesMin.Latitude, MidPoint.Longitude), new(CoordinatesMax.Latitude, MidPoint.Longitude));
	public double LongitudeExtentKm => Geography.Distance(new(MidPoint.Latitude, CoordinatesMin.Longitude), new(MidPoint.Latitude, CoordinatesMax.Longitude));
	public double AreaKm => LatitudeExtentKm * LongitudeExtentKm;
	public double[][] ForMap => [CoordinatesMin.ForMap, CoordinatesMax.ForMap];

	public Area(Coordinates coordinatesMin, Coordinates coordinatesMax)
	{
		CoordinatesMin = coordinatesMin;
		CoordinatesMax = coordinatesMax;
	}

	public readonly bool IsInside(Coordinates probe)
	{
		return CoordinatesMin.Latitude <= probe.Latitude
			&& CoordinatesMax.Latitude >= probe.Latitude
			&& CoordinatesMin.Longitude <= probe.Longitude
			&& CoordinatesMax.Longitude >= probe.Longitude;
	}

	public override string ToString() => $"{CoordinatesMin.LatitudeFmt}-{CoordinatesMax.LatitudeFmt}, {CoordinatesMin.LongitudeFmt}-{CoordinatesMax.LongitudeFmt}";
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Area other && CoordinatesMin == other.CoordinatesMin && CoordinatesMax == other.CoordinatesMax;
	public override int GetHashCode() => HashCode.Combine(CoordinatesMin, CoordinatesMax);
	public static bool operator ==(Area area1, Area area2) => area1.CoordinatesMin == area2.CoordinatesMin && area1.CoordinatesMax == area2.CoordinatesMax;
	public static bool operator !=(Area area1, Area area2) => area1.CoordinatesMin != area2.CoordinatesMin || area1.CoordinatesMax != area2.CoordinatesMax;
}
