using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace LoraStatsNet.Services;

public class RequestFilteringMiddleware(RequestDelegate next, ILogger<RequestFilteringMiddleware> logger, Configuration configuration)
{
	public async Task InvokeAsync(HttpContext context)
	{
		foreach (var mask in configuration.BlockedIPs)
		{
			if (context.Connection.RemoteIpAddress != null && IsInSubnet(context.Connection.RemoteIpAddress, mask))
			{
				context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				logger.LogInformation("Blocked {ip} (matching {mask})", context.Connection.RemoteIpAddress, mask);
				return;
			}
		}
		await next(context);
	}

	public static bool IsInSubnet(IPAddress address, string subnetMask)
	{
		var slashIdx = subnetMask.IndexOf("/");
		if (slashIdx == -1) return address.ToString() == subnetMask;

		var maskAddress = IPAddress.Parse(subnetMask.Substring(0, slashIdx));

		if (maskAddress.AddressFamily != address.AddressFamily) return false;

		if (!Int32.TryParse(subnetMask.Substring(slashIdx + 1), out var maskLength)) return false;
		if (maskLength == 0) return true;
		if (maskLength < 0) return false;

		if (maskAddress.AddressFamily == AddressFamily.InterNetwork)
		{
			var maskAddressBits = BitConverter.ToUInt32(maskAddress.GetAddressBytes().Reverse().ToArray(), 0);
			var ipAddressBits = BitConverter.ToUInt32(address.GetAddressBytes().Reverse().ToArray(), 0);
			uint mask = UInt32.MaxValue << (32 - maskLength);
			return (maskAddressBits & mask) == (ipAddressBits & mask);
		}

		if (maskAddress.AddressFamily == AddressFamily.InterNetworkV6)
		{
			var maskAddressBits = new BitArray(maskAddress.GetAddressBytes().Reverse().ToArray());
			var ipAddressBits = new BitArray(address.GetAddressBytes().Reverse().ToArray());
			var ipAddressLength = ipAddressBits.Length;

			if (maskAddressBits.Length != ipAddressBits.Length) return false;

			for (var i = ipAddressLength - 1; i >= ipAddressLength - maskLength; i--)
				if (ipAddressBits[i] != maskAddressBits[i]) return false;

			return true;
		}

		return false;
	}
}