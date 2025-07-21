using LoraStatsNet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoraStatsNet.Database.Model;

static class PacketReportBuilder
{
	public static void Configure(EntityTypeBuilder<PacketReport> builder)
	{
		builder.ToTable(nameof(PacketReport));
		builder.HasKey(e => e.Ref);

		builder.Property(e => e.Ref).HasColumnName("Id").ValueGeneratedOnAdd().HasConversion<EntityRefValueConverter<PacketReport>>().IsRequired();
		builder.Property(e => e.PacketId).HasConversion<EntityRefValueConverter<Packet>>().IsRequired();
		builder.Property(e => e.GatewayId).HasConversion<EntityRefValueConverter<Node>>().IsRequired();
		builder.Property(e => e.ReceptionTime).IsRequired();
		builder.Property(e => e.SNR).IsRequired();
		builder.Property(e => e.RSSI).IsRequired();
		builder.Property(e => e.HopLimit).IsRequired();
		builder.Property(e => e.RelayNode).IsRequired();
		builder.Property(e => e.NextHop).IsRequired();

		builder.HasOne(e => e.Packet).WithMany(e => e.Reports).HasForeignKey(e => e.PacketId).OnDelete(DeleteBehavior.Cascade);
		builder.HasOne(e => e.Gateway).WithMany(e => e.ReportedPackets).HasForeignKey(e => e.GatewayId).OnDelete(DeleteBehavior.Cascade);
		builder.HasOne(e => e.Data).WithOne(e => e.PacketReport).HasForeignKey<PacketData>(e => e.PacketReportId).OnDelete(DeleteBehavior.Cascade);
	}
}
