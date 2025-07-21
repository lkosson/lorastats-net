using LoraStatsNet.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Database.Model;

static class LoraStatsNetModelBuilder
{
	public static void Configure(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<CommunityArea>(CommunityAreaBuilder.Configure);
		modelBuilder.Entity<Community>(CommunityBuilder.Configure);
		modelBuilder.Entity<Node>(NodeBuilder.Configure);
		modelBuilder.Entity<Packet>(PacketBuilder.Configure);
		modelBuilder.Entity<PacketData>(PacketDataBuilder.Configure);
		modelBuilder.Entity<PacketReport>(PacketReportBuilder.Configure);
	}
}
