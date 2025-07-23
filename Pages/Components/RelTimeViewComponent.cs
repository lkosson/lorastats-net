using Microsoft.AspNetCore.Mvc;

namespace LoraStatsNet.Pages.Components;

public class RelTimeViewComponent : ViewComponent
{
	public IViewComponentResult Invoke(DateTime? value)
	{
		return View((value, RelTime(value)));
	}

	public static string RelTime(DateTime? timestamp)
	{
		if (timestamp == null) return "";
		var diff = DateTime.Now - timestamp.Value;
		if (diff.TotalSeconds < 1) return "now";
		if (diff.TotalSeconds < 60) return $"{diff.TotalSeconds:0}s ago";
		if (diff.TotalMinutes < 60) return $"{Math.Floor(diff.TotalMinutes):0}min {diff.Seconds:0}s ago";
		if (diff.TotalHours < 24) return $"{Math.Floor(diff.TotalHours):0}h {diff.Minutes}min ago";
		if (timestamp.Value.Date == DateTime.Now.Date.AddDays(-1)) return $"yesterday {timestamp:HH:mm}";
		return timestamp.Value.ToString("yyyy-MM-dd HH:mm:ss");
	}
}
