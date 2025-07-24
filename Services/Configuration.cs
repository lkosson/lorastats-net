using System.Globalization;
using System.Net;
using System.Text;
using Meshtastic;

namespace LoraStatsNet.Services;

public class Configuration(IConfiguration configuration)
{
	public IConfiguration ConfigurationSource => configuration;
	public string DataDir => configuration.GetValue<string>("DataDir") ?? ".";
	public string[] BlockedIPs => configuration.GetSection("BlockedIPs").Get<string[]>() ?? [];
	public string[] AdminIPs => configuration.GetSection("AdminIPs").Get<string[]>() ?? [];
	public string DbPath => configuration.GetValue<string>("DbPath") ?? Path.Combine(DataDir, "database.sqlite3");
	public Channels Channels => new Channels(configuration.GetSection("Channels"));
	public MQTTs MQTTs => new MQTTs(configuration.GetSection("MQTT"));
	public bool Liam => configuration.GetValue("Liam", false);
	public Multicast Multicast => new Multicast(configuration.GetSection("Multicast"));
	public int DataRetentionHours => configuration.GetValue<int>("DataRetentionHours", 24);
	public int ReportingHours => configuration.GetValue<int>("ReportingHours", 24);
}

public class Channels(IConfiguration configuration)
{
	public IReadOnlyCollection<Channel> All => configuration.GetChildren().Select(section => GetForSection(section)!).ToList();
	public Channel? GetByName(string name) => GetForSection(configuration.GetSection(name));
	public IReadOnlyCollection<Channel> GetByHash(byte hash) => All.Where(channel => channel.Hash == hash).ToList();
	private static Channel? GetForSection(IConfigurationSection configurationSection) => String.IsNullOrEmpty(configurationSection.Key) || String.IsNullOrEmpty(configurationSection.Value) ? null : new Channel(configurationSection.Key, configurationSection.Value);
}

public class Channel
{
	public string Name { get; init; }
	public string PSKString { get; init; }
	public byte[] PSK { get; init; }
	public byte Hash { get; init; }

	public Channel(string name, string pskString)
	{
		Name = name;
		PSKString = pskString;
		PSK = GetPSKForKey(PSKString);
		Hash = ChannelHash(Name, PSK);
	}

	private static byte[] GetPSKForKey(string key)
	{
		var byteKey = Convert.FromBase64String(key);
		if (byteKey.Length != 1) return byteKey;
		var index = byteKey[0];
		if (index == 0) return [];
		var psk = Resources.DEFAULT_PSK.ToArray();
		psk[psk.Length - 1] += (byte)(index - 1);
		return psk;
	}

	private static byte ChannelHash(string channelName, byte[] psk)
	{
		var bytes = Encoding.UTF8.GetBytes(channelName);
		byte hash = 0;
		for (var i = 0; i < bytes.Length; i++) hash ^= bytes[i];
		for (var i = 0; i < psk.Length; i++) hash ^= psk[i];
		return hash;
	}

	public override string ToString() => Name;
}

public class MQTTs(IConfiguration configuration)
{
	public IReadOnlyCollection<MQTT> All => configuration.GetChildren().Select(section => new MQTT(section)).ToList();
}

public class MQTT(IConfiguration configuration)
{
	public string Server => configuration.GetValue<string>("Server") ?? "";
	public string? Login => configuration.GetValue<string>("Login");
	public string? Password => configuration.GetValue<string>("Password");
	public string[] Topics => configuration.GetSection("Topics").Get<string[]>() ?? [];

	public override string ToString() => Server;
}

public class Multicast(IConfiguration configuration)
{
	public IReadOnlyCollection<(IPAddress ip, uint nodeId)> IpToNodeMapping => configuration
		.GetChildren()
		.Select(e => IPAddress.TryParse(e.Key, out var ip) && UInt32.TryParse(e.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var nodeId) ? (ip, nodeId) : default)
		.Where(e => e != default)
		.ToList();

	public UInt32 GetNodeIdByIP(IPAddress ip) => IpToNodeMapping.FirstOrDefault(e => e.ip.Equals(ip)).nodeId;
}
