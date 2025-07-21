namespace LoraStatsNet.Database.Entities;

class PacketData : Entity<PacketData>
{
	public long PacketReportId { get; set; }
	public required byte[] RawData { get; set; }

	public EntityRef<Packet> PacketReportRef { get => PacketReportId; set => PacketReportId = value; }

	public PacketReport PacketReport { get; set; } = default!;
}
