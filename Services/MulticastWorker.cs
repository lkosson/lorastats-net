using System.Net;
using System.Net.Sockets;


namespace LoraStatsNet.Services;
internal class MulticastWorker(ILogger<MulticastWorker> logger, IServiceProvider serviceProvider, MeshCrypto meshCrypto, Configuration configuration) : IngressWorker(logger, serviceProvider, meshCrypto)
{
	public async Task RunAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Multicast worker started");

		using var udpClient = new UdpClient(4403);
		udpClient.JoinMulticastGroup(IPAddress.Parse("224.0.0.69"));
		while (!cancellationToken.IsCancellationRequested)
		{
			var received = await udpClient.ReceiveAsync(cancellationToken);
			logger.LogDebug("Received message from {ip}, len: {len}", received.RemoteEndPoint, received.Buffer.Length);
			uint gatewayId = configuration.Multicast.GetNodeIdByIP(received.RemoteEndPoint.Address);
			if (gatewayId == 0)
			{
				logger.LogWarning("Unmapped IP address {ip}", received.RemoteEndPoint);
				continue;
			}
			await ProcessMeshPacket(received.Buffer, gatewayId);
		}

		logger.LogWarning("Multicast worker stopped");
	}
}