namespace LoraStatsNet.Pages;

public class ColorPalette
{
	private static readonly string[] VALUES = ["#440154", "#481467", "#482576", "#453781", "#404688", "#39558c", "#33638d", "#2d718e", "#287d8e", "#238a8d", "#1f968b", "#20a386", "#29af7f", "#3dbc74", "#56c667", "#75d054", "#95d840", "#bade28", "#dde318", "#fde725"];

	private static int Lerp(int value, int scale, int min, int max)
	{
		if (value < min) return 0;
		if (value >= max) return scale;
		return (value - min) * scale / (max - min);
	}

	public static string Value(int value, int max) => Value(value, 0, max, 0);

	public static string Value(int value, int min, int max, int opacityMin)
	{
		if (value < opacityMin)
		{
			var opacityVal = 20 + Lerp(value, 230, min, opacityMin);
			return VALUES[0] + opacityVal.ToString("X2");
		}
		else
		{
			var idx = Lerp(value, VALUES.Length - 1, min, max);
			return VALUES[idx];
		}
	}
}
