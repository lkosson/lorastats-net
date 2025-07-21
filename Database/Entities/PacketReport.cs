namespace LoraStatsNet.Database.Entities;

class PacketReport : Entity<PacketReport>
{
	public long PacketId { get; set; }
	public long GatewayId { get; set; }
	public DateTime ReceptionTime { get; set; }
	public float SNR { get; set; }
	public byte RSSI { get; set; }
	public byte HopLimit { get; set; }
	public byte RelayNode { get; set; }
	public byte NextHop { get; set; }

	public EntityRef<Packet> PacketRef { get => PacketId; set => PacketId = value; }
	public EntityRef<Node> GatewayRef { get => GatewayId;set => GatewayId = value; }

	public Packet Packet { get; set; } = default!;
	public Node Gateway { get; set; } = default!;
	public PacketData Data { get; set; } = default!;
}
