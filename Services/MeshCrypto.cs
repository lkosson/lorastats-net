using Meshtastic;
using Meshtastic.Crypto;
using Meshtastic.Protobufs;

namespace LoraStatsNet.Services;

class MeshCrypto(Configuration configuration)
{
	public Channel? TryDecode(MeshPacket meshPacket)
	{
		var channels = configuration.Channels.GetByHash((byte)meshPacket.Channel);
		foreach (var channel in channels)
		{
			if (!TryDecode(meshPacket, channel)) continue;
			return channel;
		}
		return null;
	}

	private static bool TryDecode(MeshPacket meshPacket, Channel channel)
	{ 
		if (meshPacket.Encrypted.Length == 0) return true;
		var nonce = new NonceGenerator(meshPacket.From, meshPacket.Id).Create();
		var blob = meshPacket.Encrypted.ToByteArray();
		Data? decoded = null;
		if (channel != null) decoded ??= TryDecode(blob, nonce, channel.PSK);
		decoded ??= TryDecode(blob);
		decoded ??= TryDecode(blob, nonce, Resources.DEFAULT_PSK);
		if (decoded == null) return false;
		meshPacket.Decoded = decoded;
		return true;
	}

	private static Data? TryDecode(byte[] blob, byte[] nonce, byte[] psk)
	{
		try
		{
			return TryDecode(PacketEncryption.TransformPacket(blob, nonce, psk));
		}
		catch
		{
			return null;
		}
	}

	private static Data? TryDecode(byte[] blob)
	{
		try
		{
			return Data.Parser.ParseFrom(blob);
		}
		catch
		{
			return null;
		}
	}
}
