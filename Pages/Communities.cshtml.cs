using System.Text.Json;
using LoraStatsNet.Database;
using LoraStatsNet.Database.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Pages;

class CommunitiesModel(LoraStatsNetDb db) : PageModel, IPageWithTitle
{
	public string Title => "Communities";
	public IReadOnlyCollection<Community> Communities { get; private set; } = default!;
	public string Nodes { get; set; } = default!;
	public string Areas { get; set; } = default!;
	public Dictionary<EntityRef<Community>, int> NodeCount { get; set; } = default!;

	public async Task OnGetAsync()
	{
		Communities = await db.Communities.OrderBy(community => community.Name).ToListAsync();

		var areas = await db.CommunityAreas.Include(communityArea => communityArea.Community).ToListAsync();
		var areasInfo = areas
			.Select(communityArea => new
			{
				name = communityArea.Community.Name,
				url = Url.Page("Nodes", new { Community = communityArea.Community.UrlName }),
				bounds = communityArea.Area.ForMap
			})
			.ToList();
		Areas = JsonSerializer.Serialize(areasInfo);

		var nodes = await db.Nodes.Where(node => node.LastLatitude.HasValue && node.LastLongitude.HasValue).ToListAsync();
		MapNode.Ungroup(nodes);
		Nodes = MapNode.JsonForNodes(nodes);
		NodeCount = nodes.Where(node => node.CommunityId.HasValue).GroupBy(node => node.CommunityId!.Value).ToDictionary(g => g.Key, g => g.Count());
	}
}
