namespace LoraStatsNet.Database.Entities;

public class PacketReport : Entity<PacketReport>
{
	public EntityRef<Packet> PacketId { get; set; }
	public EntityRef<Node> GatewayId { get; set; }
	public DateTime ReceptionTime { get; set; }
	public float SNR { get; set; }
	public byte RSSI { get; set; }
	public byte HopLimit { get; set; }
	public byte RelayNode { get; set; }
	public byte NextHop { get; set; }

	public Packet Packet { get; set; } = default!;
	public Node Gateway { get; set; } = default!;
	public PacketData Data { get; set; } = default!;
}
