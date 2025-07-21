using System.Text;

namespace LoraStatsNet.Services;

class FileWriter(string logPathTemplate, bool logCategoryName, bool logEventId, int rotate) : IDisposable
{
	private StreamWriter? cachedWriter;
	private string? cachedWriterPath;
	private Task? flushTask;
	private bool disposeAfterFlush;
	private bool disposed;

	private StreamWriter EnsureWriterOpen()
	{
		var expectedWriterPath = String.Format(logPathTemplate, DateTime.Now);
		var pathChanged = cachedWriterPath != expectedWriterPath;
		if (!pathChanged && cachedWriter != null) return cachedWriter;
		cachedWriter?.Dispose();
		Directory.CreateDirectory(Path.GetDirectoryName(expectedWriterPath)!);
		var checkRotate = !File.Exists(expectedWriterPath);
		cachedWriter = new StreamWriter(expectedWriterPath, true, Encoding.UTF8);
		cachedWriterPath = expectedWriterPath;
		if (checkRotate) RotateFiles();
		return cachedWriter;
	}

	public void Write(LogLevel logLevel, string categoryName, int eventId, string message)
	{
		lock (this)
		{
			try
			{
				if (disposed) return;
				var writer = EnsureWriterOpen();
				var lines = message.Split('\r', '\n');
				var dateTime = DateTime.Now;
				foreach (var line in lines)
				{
					if (String.IsNullOrWhiteSpace(line)) continue;
					writer.Write("[{0:yyyy-MM-dd HH:mm:ss.fff}]\t", dateTime);
					if (logEventId) writer.Write("[{0:D8}]\t", eventId);
					if (logLevel == LogLevel.Critical) writer.Write("[CRT]\t");
					else if (logLevel == LogLevel.Debug) writer.Write("[DEB]\t");
					else if (logLevel == LogLevel.Error) writer.Write("[ERR]\t");
					else if (logLevel == LogLevel.Information) writer.Write("[INF]\t");
					else if (logLevel == LogLevel.Trace) writer.Write("[TRC]\t");
					else if (logLevel == LogLevel.Warning) writer.Write("[WRN]\t");
					if (logCategoryName)
					{
						writer.Write("[");
						writer.Write(categoryName);
						writer.Write("] ");
					}
					writer.Write(line);
					writer.WriteLine();
				}
				if (flushTask == null)
				{
					flushTask = Task.Delay(100).ContinueWith(Flush);
					disposeAfterFlush = true;
				}
				else if (disposeAfterFlush)
				{
					disposeAfterFlush = false;
				}
			}
			catch
			{
				// ignored
			}
		}
	}

	private void Flush(Task _)
	{
		lock (this)
		{
			flushTask = null;
			if (disposed) return;
			if (cachedWriter == null) return;
			cachedWriter.Flush();
			if (disposeAfterFlush)
			{
				cachedWriter.Dispose();
				cachedWriter = null;
				cachedWriterPath = null;
			}
			else
			{
				disposeAfterFlush = true;
				flushTask = Task.Delay(100).ContinueWith(Flush);
			}
		}
	}

	private void RotateFiles()
	{
		if (rotate < 0) return;
		var logFiles = String.Format(logPathTemplate, "*");
		var path = Path.GetDirectoryName(logFiles) ?? ".";
		var filter = Path.GetFileName(logFiles);
		var files = Directory.GetFiles(path, filter);
		var oldFiles = files.OrderByDescending(e => e)
			.Skip(rotate + 1)
			.ToArray();
		foreach (var file in oldFiles)
		{
			try
			{
				File.Delete(file);
			}
			catch
			{
			}
		}
	}

	public void Dispose()
	{
		lock (this)
		{
			disposed = true;
			if (cachedWriter != null)
			{
				cachedWriter.Dispose();
				cachedWriter = null;
				cachedWriterPath = null;
			}
		}
		GC.SuppressFinalize(this);
	}
}
