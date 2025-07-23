namespace LoraStatsNet.Database.Entities;

public class PacketData : Entity<PacketData>
{
	public EntityRef<PacketReport> PacketReportId { get; set; }
	public required byte[] RawData { get; set; }

	public PacketReport PacketReport { get; set; } = default!;
}
