using Microsoft.AspNetCore.Mvc;

namespace LoraStatsNet.Pages.Components;

public class AbsTimeViewComponent : ViewComponent
{
	public IViewComponentResult Invoke(DateTime? value)
	{
		return View((value, RelTimeViewComponent.RelTime(value)));
	}
}
