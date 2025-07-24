using System.Text.Json;
using Google.Protobuf;
using LoraStatsNet.Database;
using Meshtastic.Protobufs;

namespace LoraStatsNet.Services;
internal class LiamWorker(ILogger<LiamWorker> logger, IServiceProvider serviceProvider, MeshCrypto meshCrypto) : IngressWorker(logger, serviceProvider, meshCrypto)
{
	private const string API = "https://meshtastic.liamcottle.net/api/v1/";
	private long lastMessageId = 0;

	public async Task RunAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Liam worker started");

		using var client = new HttpClient();
		client.DefaultRequestHeaders.UserAgent.ParseAdd("LoraStats.net");

		while (!cancellationToken.IsCancellationRequested)
		{
			await RunMessages(client, cancellationToken);
			await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
		}

		logger.LogWarning("Liam worker stopped");
	}

	private async Task<JsonDocument?> RunAPI(HttpClient client, string query, CancellationToken cancellationToken)
	{
		var response = await client.GetAsync(API + query, cancellationToken);
		if (!response.IsSuccessStatusCode)
		{
			logger.LogWarning("API request {query} failed: {status}", query, response.StatusCode);
			return null;
		}
		var responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
		var json = await JsonDocument.ParseAsync(responseContent, cancellationToken: cancellationToken);
		return json;
	}

	private async Task RunMessages(HttpClient client, CancellationToken cancellationToken)
	{
		var json = await RunAPI(client, lastMessageId == 0 ? "text-messages?order=desc&count=50" : $"text-messages?last_id={lastMessageId}", cancellationToken);
		if (json == null) return;
		var jsonMessages = json.RootElement.GetProperty("text_messages");
		foreach (var jsonMessage in jsonMessages.EnumerateArray())
		{
			if (!Int64.TryParse(jsonMessage.GetProperty("id").GetString(), out var id)) continue;
			if (!UInt32.TryParse(jsonMessage.GetProperty("gateway_id").GetString(), out var gatewayId)) continue;
			if (!UInt32.TryParse(jsonMessage.GetProperty("to").GetString(), out var toId)) continue;
			if (!UInt32.TryParse(jsonMessage.GetProperty("from").GetString(), out var fromId)) continue;
			var channel = jsonMessage.GetProperty("channel").GetByte();
			if (!UInt32.TryParse(jsonMessage.GetProperty("packet_id").GetString(), out var packetId)) continue;
			var hopLimit = jsonMessage.GetProperty("hop_limit").GetByte();
			var text = jsonMessage.GetProperty("text").GetString();
			if (!UInt32.TryParse(jsonMessage.GetProperty("rx_time").GetString(), out var rxTime)) continue;
			if (!Double.TryParse(jsonMessage.GetProperty("rx_snr").GetString(), out var rxSnr)) continue;
			var rxRssi = jsonMessage.GetProperty("rx_rssi").GetInt32();
			if (id > lastMessageId) lastMessageId = id;

			var packet = new MeshPacket();
			packet.Id = packetId;
			packet.From = fromId;
			packet.To = toId;
			packet.HopLimit = hopLimit;
			packet.HopStart = (uint)(hopLimit + 1);
			packet.RxSnr = (float)rxSnr;
			packet.RxRssi = rxRssi;
			packet.Channel = channel;
			packet.RxTime = rxTime;
			var data = new Data();
			data.Payload = ByteString.CopyFromUtf8(text);
			data.Portnum = PortNum.TextMessageApp;
			packet.Decoded = data;
			await ProcessPacket(packet, gatewayId, new DateTime(1970, 1, 1).AddSeconds(rxTime).ToLocalTime());
		}
	}
}