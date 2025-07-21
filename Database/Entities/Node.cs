using Meshtastic.Protobufs;

namespace LoraStatsNet.Database.Entities;

class Node : Entity<Node>
{
	public static readonly Node BROADCAST = new Node { Id = 0, NodeId = 0xffffffff, ShortName = "****", LongName = "BROADCAST" };

	public long? CommunityId { get; set; }
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

	public EntityRef<Community> CommunityRef { get => CommunityId; set => CommunityId = value; }

	public Community Community { get; set; } = default!;
	public List<Packet> SentPackets { get; set; } = default!;
	public List<PacketReport> ReportedPackets { get; set; } = default!;

	public string NodeIdFmt => NodeId.ToString("x8");
	public string RoleFmt => Role.HasValue ? Role.Value.ToString() : "";
	public string HwModelFmt => HwModel.HasValue ? HwModel.Value.ToString() : "";
	public string ShortNameOrDefault => ShortName ?? NodeIdFmt[^4..];
	public string LongNameOrDefault => LongName ?? (NodeId == 0 || NodeId == 0xffffffff ? "" : $"Meshtastic {ShortNameOrDefault}");
	public bool HasValidLocation => LastLatitude.HasValue && LastLongitude.HasValue && LastPositionUpdate.HasValue && LastPositionUpdate.Value >= DateTime.Now.AddDays(-1);

	public override string ToString() => ShortNameOrDefault;
}
