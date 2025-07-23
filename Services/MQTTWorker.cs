using System.Buffers;
using System.Text.RegularExpressions;

using MQTTnet;

namespace LoraStatsNet.Services;

internal class MQTTWorker(ILogger<MQTTWorker> logger, IServiceProvider serviceProvider, MeshCrypto meshCrypto) : IngressWorker(logger, serviceProvider, meshCrypto)
{
	public async Task RunAsync(MQTT mqtt, CancellationToken cancellationToken)
	{
		logger.LogInformation("MQTT worker for {server} started", mqtt);

		var ctsWatchdog = new CancellationTokenSource();
		cancellationToken.Register(ctsWatchdog.Cancel);
		ResetWatchdog();

		var mqttFactory = new MqttClientFactory();
		var mqttClientOptionsBuilder = new MqttClientOptionsBuilder();
		mqttClientOptionsBuilder.WithTcpServer(mqtt.Server);
		if (!String.IsNullOrEmpty(mqtt.Login)) mqttClientOptionsBuilder.WithCredentials(mqtt.Login, mqtt.Password);
		var mqttClientOptions = mqttClientOptionsBuilder.Build();

		while (!cancellationToken.IsCancellationRequested)
		{
			using var mqttClient = mqttFactory.CreateMqttClient();

			mqttClient.ApplicationMessageReceivedAsync += HandleReceived;

			logger.LogDebug("Connecting to {server}", mqtt.Server);
			var connectResponse = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
			logger.LogDebug("Connection result: {result}", connectResponse);

			foreach (var topic in mqtt.Topics)
			{
				logger.LogDebug("Subscribing to {topic}", topic);
				var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder().WithTopicFilter(topic).Build();
				var subscribeResponse = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
				logger.LogDebug("Subscription result: {result}", subscribeResponse);
			}

			await Task.Delay(Timeout.InfiniteTimeSpan, ctsWatchdog.Token);
			if (cancellationToken.IsCancellationRequested) break;
			logger.LogWarning("Watchdog expired");
			await mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
			logger.LogDebug("Disconnected");
			await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
			logger.LogDebug("Restarting");
		}

		logger.LogWarning("MQTT worker for {server} stopped", mqtt);

		void ResetWatchdog() => ctsWatchdog.CancelAfter(TimeSpan.FromSeconds(120));

		async Task HandleReceived(MqttApplicationMessageReceivedEventArgs arg)
		{
			if (Regex.IsMatch(arg.ApplicationMessage.Topic, "msh/.*/2/map/"))
			{
				logger.LogDebug("Ignoring message from {server} for {topic}", mqtt, arg.ApplicationMessage.Topic);
				return;
			}
			logger.LogDebug("Received message from {server} for {topic}", mqtt, arg.ApplicationMessage.Topic);
			var packetBlob = arg.ApplicationMessage.Payload.ToArray();
			await ProcessServiceEnvelope(packetBlob);
			ResetWatchdog();
		}
	}
}