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
	public string Nodes { get; set; } = default!;
	[FromRoute(Name = "Id")] public EntityRef<Community> CommunityRef { get; set; }
	[FromForm] public Community Community { get; set; } = default!;
	[FromForm] public string Areas { get; set; } = default!;

	public async Task<IActionResult> OnGetAsync()
	{
		if (!User?.Identity?.IsAuthenticated ?? false) return Unauthorized();
		var community = CommunityRef.IsNull ? new Community { Name = "", UrlName = "" } : await db.GetAsync(CommunityRef);
		if (community is null) return NotFound();
		var nodes = await db.Nodes.Where(node => node.LastLatitude.HasValue && node.LastLongitude.HasValue).ToListAsync();
		Community = community;
		Nodes = MapNode.JsonForNodes(nodes);
		var areas = await db.CommunityAreas.Where(communityArea => communityArea.CommunityId == CommunityRef).ToListAsync();
		Areas = JsonSerializer.Serialize(areas.Select(communityArea => communityArea.Area.ForMap));
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

	public async Task<IActionResult> OnPostReassignAsync()
	{
		if (!User?.Identity?.IsAuthenticated ?? false) return Unauthorized();
		using var tx = await db.BeginTransactionAsync();
		var communityAreas = (await db.CommunityAreas.ToListAsync()).OrderBy(area => area.Area.AreaKm).ToList();
		var nodesWithPosition = await db.Nodes.Where(node => node.LastLatitude.HasValue && node.LastLongitude.HasValue).ToListAsync();
		var communityByNode = new Dictionary<EntityRef<Node>, EntityRef<Community>>();
		foreach (var node in nodesWithPosition)
		{
			if (!node.HasValidLocation) continue;

			foreach (var area in communityAreas)
			{
				if (!area.Area.IsInside(node.Coordinates!.Value)) continue;
				if (node.CommunityId != area.CommunityId)
				{
					node.CommunityId = area.CommunityId;
					await db.StoreAsync(node);
				}
				communityByNode[node] = area.CommunityId;
				break;
			}
		}
		var reportedNodes = await db.PacketReports
			.Where(report => report.Gateway.LastLatitude.HasValue && report.Gateway.LastLongitude.HasValue)
			.Select(report => new { report.GatewayId, report.Packet.FromNode })
			.Distinct()
			.ToListAsync();

		foreach (var entry in reportedNodes)
		{
			if (entry.FromNode.HasValidLocation) continue;
			if (!communityByNode.TryGetValue(entry.GatewayId, out var communityRef)) continue;
			if (entry.FromNode.CommunityId == communityRef) continue;
			entry.FromNode.CommunityId = communityRef;
			await db.StoreAsync(entry.FromNode);
		}

		await tx.CommitAsync();
		return RedirectToPage();
	}
}
