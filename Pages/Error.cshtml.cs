using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoraStatsNet.Pages;

class ErrorModel : PageModel, IPageWithTitle
{
	public string Title => "Error";

	public void OnGet()
	{
	}
}
