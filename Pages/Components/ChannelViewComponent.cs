using LoraStatsNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoraStatsNet.Pages.Components;

public class ChannelViewComponent(Configuration configuration) : ViewComponent
{
	public IViewComponentResult Invoke(byte? value)
	{
		if (value == null || value == 0) return View(("", ""));
		var channel = configuration.Channels.GetByHash(value.Value).FirstOrDefault();
		var num = value.Value.ToString("X2");
		if (channel == null) return View(("", num));
		return View((num, channel.Name));
	}
}
