using LoraStatsNet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoraStatsNet.Database.Model;

static class PacketDataBuilder
{
	public static void Configure(EntityTypeBuilder<PacketData> builder)
	{
		builder.ToTable(nameof(PacketData));
		builder.HasKey(e => e.Ref);

		builder.Property(e => e.Ref).HasColumnName("Id").ValueGeneratedOnAdd().HasConversion<EntityRefValueConverter<PacketData>>().IsRequired();
		builder.Property(e => e.PacketReportId).HasConversion<EntityRefValueConverter<PacketReport>>().IsRequired();
		builder.Property(e => e.RawData).IsRequired();
	}
}
