namespace LoraStatsNet.Services;
internal class LiamService(ILogger<LiamService> logger, IServiceProvider serviceProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = serviceProvider.CreateScope();
				var worker = scope.ServiceProvider.GetRequiredService<LiamWorker>();
				await worker.RunAsync(stoppingToken);
			}
			catch (OperationCanceledException exc)
			{
				logger.LogDebug("Operation cancelled: {exc}", exc);
			}
			catch (Exception exc)
			{
				logger.LogError("Unhandled exception: {exc}", exc);
				await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
			}
		}
	}
}
