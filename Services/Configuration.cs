namespace LoraStatsNet.Services;

public class Configuration(IConfiguration configuration)
{
	public IConfiguration ConfigurationSource => configuration;
	public string DataDir => configuration.GetValue<string>("DataDir") ?? ".";
	public string[] BlockedIPs => configuration.GetSection("BlockedIPs").Get<string[]>() ?? [];
}

