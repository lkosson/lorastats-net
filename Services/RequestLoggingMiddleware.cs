using System.Diagnostics;

namespace LoraStatsNet.Services;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		var stopWatch = Stopwatch.StartNew();
		try
		{
			await next(context);
		}
		finally
		{
			try
			{
				stopWatch.Stop();
				logger.LogInformation("{ip}\t{user}\t{status}\t{method}\t{path}\t{query}\t{time}",
					context.Connection.RemoteIpAddress,
					context.User.Identity?.Name ?? "",
					context.Response.StatusCode,
					context.Request.Method,
					context.Request.Path,
					context.Request.QueryString,
					stopWatch.ElapsedMilliseconds);
			}
			catch
			{
				// ignored
			}
		}
	}
}