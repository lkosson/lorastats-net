namespace LoraStatsNet.Services;

class ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (ApplicationException exc)
		{
			logger.LogWarning("{exception}", exc);
			throw;
		}
		catch (Exception exc)
		{
			logger.LogError("{exception}", exc);
			throw;
		}
	}
}