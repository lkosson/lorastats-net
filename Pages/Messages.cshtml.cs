using LoraStatsNet.Database;
using LoraStatsNet.Database.Entities;
using LoraStatsNet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Pages;

class MessagesModel(LoraStatsNetDb db) : PageModel, IPageWithTitle
{
	public string Title => $"Messages - {Community}";
	[FromRoute(Name = "Community")] public string CommunityUrl { get; set; } = default!;
	[FromRoute(Name = "Channel")] public byte? Channel { get; set; }

	public IReadOnlyCollection<Packet> Messages { get; set; } = default!;
	public Community Community { get; set; } = default!;

	public async Task<IActionResult> OnGetAsync()
	{
		var community = await db.Communities.FirstOrDefaultAsync(community => community.UrlName == CommunityUrl);
		if (community == null) return NotFound();
		Community = community;

		var packets = await db.Packets
			.Include(packet => packet.FromNode)
			.Include(packet => packet.ToNode)
			.Where(packet => packet.FromNode.CommunityId == Community && packet.Port == Meshtastic.Protobufs.PortNum.TextMessageApp && packet.ToNodeId == null)
			.OrderByDescending(e => e.FirstSeen)
			.ToListAsync();

		if (Channel.HasValue) packets = packets.Where(packet => packet.Channel == Channel).ToList();

		Messages = packets.ToList();
		return Page();
	}
}
