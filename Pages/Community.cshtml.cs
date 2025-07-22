using System.Text.Json;
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
	[FromForm] public Community Community { get; set; } = default!;
	[FromForm] public string Areas { get; set; } = default!;

	public async Task<IActionResult> OnGetAsync()
	{
		if (!User?.Identity?.IsAuthenticated ?? false) return Unauthorized();
		var community = CommunityRef.IsNull ? new Community { Name = "", UrlName = "" } : await db.GetAsync(CommunityRef);
		if (community is null) return NotFound();
		Community = community;
		var areas = await db.CommunityAreas.Where(communityArea => communityArea.CommunityId == CommunityRef).ToListAsync();
		Areas = JsonSerializer.Serialize(areas.Select(communityArea => new[] { new[] { communityArea.LatitudeMin, communityArea.LongitudeMin }, new[] { communityArea.LatitudeMax, communityArea.LongitudeMax } }));
		return Page();
	}

	public async Task<IActionResult> OnPostSaveAsync()
	{
		if (!User?.Identity?.IsAuthenticated ?? false) return Unauthorized();
		var community = CommunityRef.IsNull ? new Community { Name = "", UrlName = "" } : await db.GetAsync(CommunityRef);
		if (community is null) return NotFound();
		await TryUpdateModelAsync(community, nameof(Community), community => community.Name, community => community.UrlName);
		var areas = JsonSerializer.Deserialize<double[][][]>(Areas)!;
		using var tx = await db.BeginTransactionAsync();
		var existingAreas = await db.CommunityAreas.Where(communityArea => communityArea.CommunityId == CommunityRef).ToListAsync();
		await db.StoreAsync(community);
		await db.DeleteAsync(existingAreas);
		foreach (var area in areas)
		{
			var communityArea = new CommunityArea
			{
				CommunityId = community,
				LatitudeMin = Math.Min(area[0][0], area[1][0]),
				LongitudeMin = Math.Min(area[0][1], area[1][1]),
				LatitudeMax = Math.Max(area[0][0], area[1][0]),
				LongitudeMax = Math.Max(area[0][1], area[1][1])
			};
			await db.StoreAsync(communityArea);
		}
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
