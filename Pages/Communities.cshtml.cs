using LoraStatsNet.Database;
using LoraStatsNet.Database.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Pages;

class CommunitiesModel(LoraStatsNetDb db) : PageModel, IPageWithTitle
{
	public string Title => "Communities";
	public IReadOnlyCollection<Community> Communities { get; private set; } = default!;

	public async Task OnGetAsync()
	{
		Communities = await db.Communities.ToListAsync();
	}
}
