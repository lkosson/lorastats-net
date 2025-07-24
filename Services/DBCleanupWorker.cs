using System.Net;
using System.Net.Sockets;
using LoraStatsNet.Database;
using Microsoft.EntityFrameworkCore;

namespace LoraStatsNet.Services;
internal class DBCleanupWorker(ILogger<DBCleanupWorker> logger, LoraStatsNetDb db, Configuration configuration)
{
	public async Task RunAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("DBCleanup starting");
		var deadline = DateTime.Now.AddHours(-configuration.DataRetentionHours);
		using (var tx = await db.BeginTransactionAsync())
		{
			var reports = await db.PacketReports.Where(report => report.ReceptionTime < deadline).ToListAsync();
			await db.DeleteAsync(reports);
			await tx.CommitAsync();
			if (reports.Count > 0) logger.LogInformation("Deleted {count} reports", reports.Count);
		}
		using (var tx = await db.BeginTransactionAsync())
		{
			var packets = await db.Packets.Where(packet => packet.FirstSeen < deadline && !packet.Reports.Any()).ToListAsync();
			await db.DeleteAsync(packets);
			await tx.CommitAsync();
			if (packets.Count > 0) logger.LogInformation("Deleted {count} packets", packets.Count);
		}
		using (var tx = await db.BeginTransactionAsync())
		{
			var nodes = await db.Nodes.Where(node => node.LastSeen < deadline && !node.SentPackets.Any() && !node.ReportedPackets.Any()).ToListAsync();
			await db.DeleteAsync(nodes);
			await tx.CommitAsync();
			if (nodes.Count > 0) logger.LogInformation("Deleted {count} nodes", nodes.Count);
		}
	}
}