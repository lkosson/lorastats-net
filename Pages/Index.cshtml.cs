using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoraStatsNet.Pages;

public class IndexModel : PageModel, IPageWithTitle
{
	public string Title => "Home page";

	public void OnGet()
	{
	}
}
