using System.Text;
using Meshtastic;

namespace LoraStatsNet.Services;

class Configuration(IConfiguration configuration)
{
	public IConfiguration ConfigurationSource => configuration;
	public string DataDir => configuration.GetValue<string>("DataDir") ?? ".";
	public string[] BlockedIPs => configuration.GetSection("BlockedIPs").Get<string[]>() ?? [];
	public string[] AdminIPs => configuration.GetSection("AdminIPs").Get<string[]>() ?? [];
	public string DbPath => configuration.GetValue<string>("DbPath") ?? Path.Combine(DataDir, "database.sqlite3");
	public Channels Channels => new Channels(configuration.GetSection("Channels"));
	public MQTTs MQTTs => new MQTTs(configuration.GetSection("MQTT"));
}

class Channels(IConfiguration configuration)
{
	public IReadOnlyCollection<Channel> All => configuration.GetChildren().Select(section => GetForSection(section)!).ToList();
	public Channel? GetByName(string name) => GetForSection(configuration.GetSection(name));
	public IReadOnlyCollection<Channel> GetByHash(byte hash) => All.Where(channel => channel.Hash == hash).ToList();
	private static Channel? GetForSection(IConfigurationSection configurationSection) => String.IsNullOrEmpty(configurationSection.Key) || String.IsNullOrEmpty(configurationSection.Value) ? null : new Channel(configurationSection.Key, configurationSection.Value);
}

class Channel(string name, string pskString)
{
	public string Name { get; init; } = name;
	public string PSKString { get; init; } = pskString;
	public byte[] PSK => GetPSKForKey(PSKString);
	public byte Hash => ChannelHash(Name, PSK);

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

class MQTTs(IConfiguration configuration)
{
	public IReadOnlyCollection<MQTT> All => configuration.GetChildren().Select(section => new MQTT(section)).ToList();
}

class MQTT(IConfiguration configuration)
{
	public string Server => configuration.GetValue<string>("Server") ?? "";
	public string? Login => configuration.GetValue<string>("Login");
	public string? Password => configuration.GetValue<string>("Password");
	public string Topic => configuration.GetValue<string>("Topic") ?? "";

	public override string ToString() => Server;
}