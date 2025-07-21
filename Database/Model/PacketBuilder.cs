using LoraStatsNet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoraStatsNet.Database.Model;

static class PacketBuilder
{
	public static void Configure(EntityTypeBuilder<Packet> builder)
	{
		builder.ToTable(nameof(Packet));
		builder.HasKey(e => e.Ref);

		builder.Property(e => e.Ref).HasColumnName("Id").ValueGeneratedOnAdd().HasConversion<EntityRefValueConverter<Packet>>().IsRequired();
		builder.Property(e => e.PacketId).IsRequired();
		builder.Property(e => e.FromNodeId).HasConversion<EntityRefValueConverter<Node>>().IsRequired();
		builder.Property(e => e.ToNodeId).HasConversion<EntityRefValueConverter<Node>>();
		builder.Property(e => e.FirstSeen).IsRequired();
		builder.Property(e => e.HopStart).IsRequired();
		builder.Property(e => e.WantAck).IsRequired();
		builder.Property(e => e.WantResponse).IsRequired();
		builder.Property(e => e.Port).IsRequired();
		builder.Property(e => e.BitField).IsRequired();
		builder.Property(e => e.ParsedPayload);
		builder.Property(e => e.Channel).IsRequired();
		builder.Property(e => e.RequestId);

		builder.HasOne(e => e.FromNode).WithMany(e => e.SentPackets).HasForeignKey(e => e.FromNodeId).OnDelete(DeleteBehavior.Cascade);
		builder.HasOne(e => e.ToNode).WithMany().HasForeignKey(e => e.ToNodeId).OnDelete(DeleteBehavior.Cascade);
	}
}
