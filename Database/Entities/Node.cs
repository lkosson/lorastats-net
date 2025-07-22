using LoraStatsNet.Services;
using Meshtastic.Protobufs;

namespace LoraStatsNet.Database.Entities;

class Node : Entity<Node>
{
	public static readonly Node BROADCAST = new Node { NodeId = 0xffffffff, ShortName = "****", LongName = "BROADCAST" };

	public EntityRef<Community>? CommunityId { get; set; }
	public uint NodeId { get; set; }
	public string? ShortName { get; set; }
	public string? LongName { get; set; }
	public Config.Types.DeviceConfig.Types.Role? Role { get; set; }
	public HardwareModel? HwModel { get; set; }
	public DateTime LastSeen { get; set; }
	public double? LastLongitude { get; set; }
	public double? LastLatitude { get; set; }
	public double? LastElevation { get; set; }
	public int? LastPositionPrecision { get; set; }
	public DateTime? LastPositionUpdate { get; set; }
	public DateTime? LastBoot { get; set; }
	public string? PublicKey { get; set; }

	public Community Community { get; set; } = default!;
	public List<Packet> SentPackets { get; set; } = default!;
	public List<PacketReport> ReportedPackets { get; set; } = default!;

	public string NodeIdFmt => NodeId.ToString("x8");
	public string RoleFmt => Role.HasValue ? Role.Value.ToString() : "";
	public string HwModelFmt => HwModel.HasValue ? HwModel.Value.ToString() : "";
	public string ShortNameOrDefault => ShortName ?? NodeIdFmt[^4..];
	public string LongNameOrDefault => LongName ?? (NodeId == 0 || NodeId == 0xffffffff ? "" : $"Meshtastic {ShortNameOrDefault}");
	public bool HasValidLocation => LastLatitude.HasValue && LastLongitude.HasValue && LastPositionUpdate.HasValue && HasRecentLocation;
	public bool HasRecentLocation => LastPositionUpdate.HasValue && LastPositionUpdate.Value >= DateTime.Now.AddDays(-1);
	public Coordinates? Coordinates => HasValidLocation ? new(LastLatitude!.Value, LastLongitude!.Value) : null;
	public (string foreColor, string backColor) NodeColors
	{
		get
		{
			var r = (NodeId & 0xFF0000) >> 16;
			var g = (NodeId & 0x00FF00) >> 8;
			var b = (NodeId & 0x0000FF);
			var brightness = ((r * 0.299) + (g * 0.587) + (b * 0.114)) / 255;
			var foreColor = brightness > 0.5 ? "#000" : "#fff";
			var backColor = $"#{r:X2}{g:X2}{b:X2}";
			return (foreColor, backColor);
		}
	}

	public override string ToString() => ShortNameOrDefault;
}
