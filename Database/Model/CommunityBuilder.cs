using LoraStatsNet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoraStatsNet.Database.Model;

static class CommunityBuilder
{
	public static void Configure(EntityTypeBuilder<Community> builder)
	{
		builder.ToTable(nameof(Community));
		builder.HasKey(e => e.Ref);

		builder.Property(e => e.Ref).HasColumnName("Id").ValueGeneratedOnAdd().HasConversion<EntityRefValueConverter<Community>>().IsRequired();
		builder.Property(e => e.Name).IsRequired();
		builder.Property(e => e.UrlName).IsRequired();
	}
}
