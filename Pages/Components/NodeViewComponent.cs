using Microsoft.AspNetCore.Mvc;

namespace LoraStatsNet.Pages.Components;

public class NodeViewComponent : ViewComponent
{
	public IViewComponentResult Invoke(Database.Entities.Node? value, bool shortOnly = false)
	{
		var id = value?.NodeId ?? 0;
		var r = (id & 0xFF0000) >> 16;
		var g = (id & 0x00FF00) >> 8;
		var b = (id & 0x0000FF);
		var brightness = ((r * 0.299) + (g * 0.587) + (b * 0.114)) / 255;
		return View((value, brightness > 0.5 ? "#000" : "#fff", $"#{r:X2}{g:X2}{b:X2}", shortOnly));
	}
}
