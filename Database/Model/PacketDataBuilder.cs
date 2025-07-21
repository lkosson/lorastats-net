using LoraStatsNet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoraStatsNet.Database.Model;

static class PacketDataBuilder
{
	public static void Configure(EntityTypeBuilder<PacketData> builder)
	{
		builder.ToTable(nameof(PacketData));
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id).ValueGeneratedOnAdd().IsRequired();
		builder.Property(e => e.PacketReportId).IsRequired();
		builder.Property(e => e.RawData).IsRequired();

		builder.Ignore(e => e.PacketReportRef);
	}
}
