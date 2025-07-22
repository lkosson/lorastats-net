using LoraStatsNet.Database;
using LoraStatsNet.Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Pages;

class CommunityModel(LoraStatsNetDb db) : PageModel, IPageWithTitle
{
	public string Title => Community == null ? "New community" : $"Community {Community}";
	[FromRoute(Name = "Id")] public EntityRef<Community> CommunityRef { get; set; }
	[FromForm] public Community Community { get; set; }
	public IReadOnlyCollection<CommunityArea> Areas { get; set; } = [];

	public async Task<IActionResult> OnGetAsync()
	{
		if (!User?.Identity?.IsAuthenticated ?? false) return Unauthorized();
		var community = CommunityRef.IsNull ? new Community { Name = "", UrlName = "" } : await db.GetAsync(CommunityRef);
		if (community is null) return NotFound();
		Community = community;
		Areas = await db.CommunityAreas.Where(communityArea => communityArea.CommunityId == CommunityRef).ToListAsync();
		return Page();
	}

	public async Task<IActionResult> OnPostSaveAsync()
	{
		if (!User?.Identity?.IsAuthenticated ?? false) return Unauthorized();
		var community = CommunityRef.IsNull ? new Community { Name = "", UrlName = "" } : await db.GetAsync(CommunityRef);
		if (community is null) return NotFound();
		await TryUpdateModelAsync(community, nameof(Community), community => community.Name, community => community.UrlName);
		using var tx = await db.BeginTransactionAsync();
		await db.StoreAsync(community);
		await tx.CommitAsync();
		return RedirectToPage("Communities");
	}

	public async Task<IActionResult> OnPostDeleteAsync()
	{
		if (!User?.Identity?.IsAuthenticated ?? false) return Unauthorized();
		var community = await db.GetAsync(CommunityRef);
		if (community is null) return NotFound();
		using var tx = await db.BeginTransactionAsync();
		await db.DeleteAsync(community);
		await tx.CommitAsync();
		return RedirectToPage("Communities");
	}
}
