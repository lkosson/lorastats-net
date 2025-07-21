using LoraStatsNet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoraStatsNet.Database.Model;

static class CommunityAreaBuilder
{
	public static void Configure(EntityTypeBuilder<CommunityArea> builder)
	{
		builder.ToTable(nameof(CommunityArea));
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id).ValueGeneratedOnAdd().IsRequired();
		builder.Property(e => e.CommunityId).IsRequired();
		builder.Property(e => e.LatitudeMin).IsRequired();
		builder.Property(e => e.LatitudeMax).IsRequired();
		builder.Property(e => e.LongitudeMin).IsRequired();
		builder.Property(e => e.LongitudeMax).IsRequired();

		builder.Ignore(e => e.CommunityRef);

		builder.HasOne(e => e.Community).WithMany(e => e.Areas).HasForeignKey(e => e.CommunityId).OnDelete(DeleteBehavior.Cascade);
	}
}
