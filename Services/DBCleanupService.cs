namespace LoraStatsNet.Services;
internal class DBCleanupService(ILogger<DBCleanupService> logger, IServiceProvider serviceProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("DBCleanup service started");
		await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await IngressWorker.MUTEX.WaitAsync(CancellationToken.None);

				using var scope = serviceProvider.CreateScope();
				var worker = scope.ServiceProvider.GetRequiredService<DBCleanupWorker>();
				await worker.RunAsync(stoppingToken);
			}
			catch (OperationCanceledException exc)
			{
				logger.LogDebug("Operation cancelled: {exc}", exc);
			}
			catch (Exception exc)
			{
				logger.LogError("DBCleanup error: {exc}", exc);
			}
			finally
			{
				IngressWorker.MUTEX.Release();
			}
			await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
		}

		logger.LogWarning("DBCleanup service stopped");



	}
}
