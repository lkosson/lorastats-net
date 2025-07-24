using System.Text.Json;
using LoraStatsNet.Services;

namespace LoraStatsNet.Pages;

class Formatter
{
	public static string FormatRSSI(byte rssi) => ((sbyte)rssi).ToString("0") + " dBm";
	public static string FormatSNR(int snr) => FormatSNR(snr * 0.25f);
	public static string FormatSNR(float snr) => snr.ToString("0.00") + " dB";
	public static string FormatDistance(double? distance) => distance is null ? "" : distance < 1 ? (distance.Value * 1000).ToString("0") + " m" : distance.Value.ToString("0.0") + " km";
	public static string FormatJSONCoordinates(Coordinates? coordinates) => coordinates is null ? "[]" : JsonSerializer.Serialize(coordinates.Value.ForMap);
	public static string FormatJSON<T>(T? obj) => obj is null ? "{}" : JsonSerializer.Serialize(obj);
}
