using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoraStatsNet.Pages;

class IndexModel : PageModel, IPageWithTitle
{
	public string Title => "Home page";

	public void OnGet()
	{
	}
}
