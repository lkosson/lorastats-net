namespace LoraStatsNet.Services;

public class FileLogger(string categoryName, FileWriter fileWriter) : ILogger
{
	IDisposable ILogger.BeginScope<TState>(TState state) => null!;
	bool ILogger.IsEnabled(LogLevel logLevel) => true;

	void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		var message = formatter == null ? "" : formatter(state, exception);
		fileWriter.Write(logLevel, categoryName, eventId.Id, message);
	}
}
