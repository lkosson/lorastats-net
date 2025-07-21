namespace LoraStatsNet.Services;

internal class MQTTService(ILogger<MQTTService> logger, IServiceProvider serviceProvider, Configuration configuration) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		List<Task> tasks = [];
		foreach (var mqtt in configuration.MQTTs.All)
		{
			tasks.Add(ExecuteAsync(mqtt, stoppingToken));
		}
		await Task.WhenAll(tasks);
	}

	private async Task ExecuteAsync(MQTT mqtt, CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = serviceProvider.CreateScope();
				var worker = scope.ServiceProvider.GetRequiredService<MQTTWorker>();
				await worker.RunAsync(mqtt, stoppingToken);
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
