using LoraStatsNet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoraStatsNet.Database.Model;

static class CommunityBuilder
{
	public static void Configure(EntityTypeBuilder<Community> builder)
	{
		builder.ToTable(nameof(Community));
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id).ValueGeneratedOnAdd().IsRequired();
		builder.Property(e => e.Name).IsRequired();
		builder.Property(e => e.UrlName).IsRequired();
	}
}
