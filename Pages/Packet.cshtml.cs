using LoraStatsNet.Database;
using LoraStatsNet.Database.Entities;
using Meshtastic.Protobufs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Pages;

class PacketModel(LoraStatsNetDb db) : PageModel, IPageWithTitle
{
	public string Title => "Packet " + Packet.PacketIdFmt;
	[FromRoute(Name = "id")] public EntityRef<Packet> PacketRef { get; set; }

	public Packet Packet { get; set; } = default!;

	public async Task<IActionResult> OnGetAsync()
	{
		if (PacketRef.IsNull) return BadRequest();
		var packet = await db.Packets
			.Include(packet => packet.Reports)
			.ThenInclude(report => report.Gateway)
			.Include(packet => packet.FromNode)
			.ThenInclude(node => node.Community)
			.Include(packet => packet.ToNode)
			.FirstOrDefaultAsync(packet => packet.Ref == PacketRef);
		if (packet == null) return NotFound();
		MapNode.Ungroup(packet.Reports.Select(report => report.Gateway).Distinct());
		RouteData.Values["community"] = packet.FromNode.Community;
		Packet = packet;
		return Page();
	}
}
