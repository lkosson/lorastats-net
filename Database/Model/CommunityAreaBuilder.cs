using LoraStatsNet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoraStatsNet.Database.Model;

static class CommunityAreaBuilder
{
	public static void Configure(EntityTypeBuilder<CommunityArea> builder)
	{
		builder.ToTable(nameof(CommunityArea));
		builder.HasKey(e => e.Ref);

		builder.Property(e => e.Ref).HasColumnName("Id").ValueGeneratedOnAdd().HasConversion<EntityRefValueConverter<CommunityArea>>().IsRequired();
		builder.Property(e => e.CommunityId).HasConversion<EntityRefValueConverter<Community>>().IsRequired();
		builder.Property(e => e.LatitudeMin).IsRequired();
		builder.Property(e => e.LatitudeMax).IsRequired();
		builder.Property(e => e.LongitudeMin).IsRequired();
		builder.Property(e => e.LongitudeMax).IsRequired();

		builder.HasOne(e => e.Community).WithMany(e => e.Areas).HasForeignKey(e => e.CommunityId).OnDelete(DeleteBehavior.Cascade);
	}
}
