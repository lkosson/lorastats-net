using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using LoraStatsNet.Database;
using LoraStatsNet.Database.Entities;
using LoraStatsNet.Services;
using Meshtastic;
using Meshtastic.Crypto;
using Meshtastic.Protobufs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg;

namespace LoraStatsNet.Pages;

class ReportModel(LoraStatsNetDb db, MeshCrypto meshCrypto) : PageModel, IPageWithTitle
{
	public string Title => "Packet content for " + PacketReport.Packet.PacketIdFmt;
	[FromRoute(Name = "id")] public EntityRef<PacketReport> PacketReportRef { get; set; }

	public PacketReport PacketReport { get; set; } = default!;
	public string? HeaderData { get; set; } = default!;
	public string? BodyData { get; set; } = default!;
	public string? ParsedData { get; set; } = default!;
	public Telemetry? Telemetry { get; set; }
	public Position? Position { get; set; }
	public new User? User { get; set; }
	public RouteDiscovery? RouteDiscovery { get; set; }
	public NeighborInfo? NeighborInfo { get; set; }
	public Routing? Routing { get; set; }
	public MapReport? MapReport { get; set; }
	public AdminMessage? AdminMessage { get; set; }

	public async Task<IActionResult> OnGetAsync()
	{
		if (PacketReportRef.IsNull) return BadRequest();
		var report = await db.PacketReports
			.Include(report => report.Packet)
			.ThenInclude(packet => packet.FromNode)
			.ThenInclude(packet => packet.Community)
			.Where(report => report.Ref == PacketReportRef)
			.FirstOrDefaultAsync();
		if (report == null) return NotFound();
		var data = await db.PacketDatas.FirstOrDefaultAsync(data => data.PacketReportId == PacketReportRef);
		if (data == null) return NotFound();
		PacketReport = report;
		RouteData.Values["community"] = report.Packet.FromNode.Community?.UrlName;

		try
		{
			var meshPacket = MeshPacket.Parser.ParseFrom(data.RawData);

			meshCrypto.TryDecode(meshPacket);

			BodyData = meshPacket.Decoded?.ToString() ?? "";
			if (meshPacket.Decoded == null)
			{
				if (meshPacket.Encrypted != null) BodyData = Convert.ToBase64String(meshPacket.Encrypted.ToByteArray());
			}
			else if (meshPacket.Decoded.Portnum == PortNum.TelemetryApp)
			{
				Telemetry = Telemetry.Parser.ParseFrom(meshPacket.Decoded.Payload);
				ParsedData = Telemetry.ToString();
			}
			else if (meshPacket.Decoded.Portnum == PortNum.PositionApp)
			{
				Position = Position.Parser.ParseFrom(meshPacket.Decoded.Payload);
				ParsedData = Position.ToString();
			}
			else if (meshPacket.Decoded.Portnum == PortNum.NodeinfoApp)
			{
				User = User.Parser.ParseFrom(meshPacket.Decoded.Payload);
				ParsedData = User.ToString();
			}
			else if (meshPacket.Decoded.Portnum == PortNum.TracerouteApp)
			{
				RouteDiscovery = RouteDiscovery.Parser.ParseFrom(meshPacket.Decoded.Payload);
				ParsedData = RouteDiscovery.ToString();
			}
			else if (meshPacket.Decoded.Portnum == PortNum.NeighborinfoApp)
			{
				NeighborInfo = NeighborInfo.Parser.ParseFrom(meshPacket.Decoded.Payload);
				ParsedData = NeighborInfo.ToString();
			}
			else if (meshPacket.Decoded.Portnum == PortNum.RoutingApp)
			{
				Routing = Routing.Parser.ParseFrom(meshPacket.Decoded.Payload);
				ParsedData = Routing.ToString();
			}
			else if (meshPacket.Decoded.Portnum == PortNum.TextMessageApp)
			{
				ParsedData = Encoding.UTF8.GetString(meshPacket.Decoded.Payload.ToByteArray());
			}
			else if (meshPacket.Decoded.Portnum == PortNum.MapReportApp)
			{
				MapReport = MapReport.Parser.ParseFrom(meshPacket.Decoded.Payload);
				ParsedData = MapReport.ToString();
			}
			else if (meshPacket.Decoded.Portnum == PortNum.RangeTestApp)
			{
				ParsedData = Encoding.UTF8.GetString(meshPacket.Decoded.Payload.ToByteArray());
			}
			else if (meshPacket.Decoded.Portnum == PortNum.AdminApp)
			{
				AdminMessage = AdminMessage.Parser.ParseFrom(meshPacket.Decoded.Payload);
				ParsedData = AdminMessage.ToString();
			}

			meshPacket.Decoded = null;
			HeaderData = meshPacket.ToString();
		}
		catch
		{
		}

		string FormatJson(string? input)
		{
			if (String.IsNullOrEmpty(input)) return "";
			try
			{
				return JsonSerializer.Serialize(JsonDocument.Parse(input), new JsonSerializerOptions { WriteIndented = true });
			}
			catch
			{
				return input;
			}
		}

		HeaderData = FormatJson(HeaderData);
		BodyData = FormatJson(BodyData);
		ParsedData = FormatJson(ParsedData);

		return Page();
	}
}
