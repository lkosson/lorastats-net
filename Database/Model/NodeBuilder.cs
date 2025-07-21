using LoraStatsNet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoraStatsNet.Database.Model;

static class NodeBuilder
{
	public static void Configure(EntityTypeBuilder<Node> builder)
	{
		builder.ToTable(nameof(Node));
		builder.HasKey(e => e.Ref);

		builder.Property(e => e.Ref).HasColumnName("Id").ValueGeneratedOnAdd().HasConversion<EntityRefValueConverter<Node>>().IsRequired();
		builder.Property(e => e.CommunityId).HasConversion<EntityRefValueConverter<Community>>();
		builder.Property(e => e.NodeId).IsRequired();
		builder.Property(e => e.ShortName);
		builder.Property(e => e.LongName);
		builder.Property(e => e.Role);
		builder.Property(e => e.HwModel);
		builder.Property(e => e.LastSeen).IsRequired();
		builder.Property(e => e.LastLongitude);
		builder.Property(e => e.LastLatitude);
		builder.Property(e => e.LastElevation);
		builder.Property(e => e.LastPositionPrecision);
		builder.Property(e => e.LastPositionUpdate);
		builder.Property(e => e.LastBoot);
		builder.Property(e => e.PublicKey);

		builder.HasOne(e => e.Community).WithMany(e => e.Nodes).HasForeignKey(e => e.CommunityId).OnDelete(DeleteBehavior.Cascade);
	}
}
