using LoraStatsNet.Database;
using LoraStatsNet.Database.Entities;
using Meshtastic.Protobufs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Pages;

class PacketsModel(LoraStatsNetDb db) : PageModel, IPageWithTitle
{
	public string Title => $"Packets - {Community}";
	[FromRoute(Name = "Community")] public string CommunityUrl { get; set; } = default!;
	[FromRoute(Name = "Type")] public string? TypeName { get; set; }

	public Community Community { get; set; } = default!;
	public List<(string type, int count)> PacketsByType { get; private set; } = default!;
	public List<(int ttl, int count)> PacketsByTTL { get; private set; } = default!;
	public List<Packet> LastPackets { get; private set; } = default!;

	public async Task<IActionResult> OnGetAsync()
	{
		var community = await db.Communities.FirstOrDefaultAsync(community => community.UrlName == CommunityUrl);
		if (community == null) return NotFound();
		Community = community;

		var packets = await db.Packets
			.Include(packet => packet.FromNode)
			.Include(packet => packet.ToNode)
			.Where(packet => packet.FromNode.CommunityId == Community && packet.FirstSeen >= DateTime.Now.AddHours(-24))
			.ToListAsync();

		if (Enum.TryParse<PortNum>(TypeName, out var type)) packets = packets.Where(e => e.Port == type).ToList();
		PacketsByType = packets.GroupBy(e => e.Port).Select(e => (type: e.Key.ToString(), count: e.Count())).OrderByDescending(e => e.count).ToList();
		PacketsByTTL = packets.Where(e => e.HopStart <= 7).GroupBy(e => e.HopStart).Select(e => (ttl: (int)e.Key, count: e.Count())).OrderBy(e => e.ttl).ToList();
		LastPackets = packets.OrderByDescending(e => e.FirstSeen).Take(100).ToList();

		return Page();
	}
}
