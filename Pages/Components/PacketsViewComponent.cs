using LoraStatsNet.Database.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LoraStatsNet.Pages.Components;

public class PacketsViewComponent : ViewComponent
{
	public IViewComponentResult Invoke(IEnumerable<Packet> packets, string? title = null, bool includeFrom = true, bool includeTo = true)
	{
		if (packets.Any()) return View((packets, title ?? "Pakiety", includeFrom, includeTo));
		else return View("Empty");
	}
}
