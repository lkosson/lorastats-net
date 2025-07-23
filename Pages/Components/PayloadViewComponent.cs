using LoraStatsNet.Database.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LoraStatsNet.Pages.Components;

public class PayloadViewComponent : ViewComponent
{
	public IViewComponentResult Invoke(Packet packet)
	{
		return View(packet);
	}
}
