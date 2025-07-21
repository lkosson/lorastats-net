using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace LoraStatsNet.Services;

public class FileLoggerProvider(string fileName, bool logCategory, bool logEventId, int rotate = -1) : ILoggerProvider
{
	private readonly FileWriter writer = new(fileName, logCategory, logEventId, rotate);
	private readonly ConcurrentDictionary<string, ILogger> loggers = [];

	ILogger ILoggerProvider.CreateLogger(string categoryName) => loggers.GetOrAdd(categoryName, key => new FileLogger(key, writer));

	void IDisposable.Dispose()
	{
		loggers.Clear();
		writer.Dispose();
		GC.SuppressFinalize(this);
	}
}

static class FileLoggerProviderExtensions
{
	public static void AddFileLoggerProvider(this IServiceCollection services, Configuration configuration)
	{
		var loggingSection = configuration.ConfigurationSource.GetSection("Logging");
		var subsections = loggingSection.GetChildren();

		ModuleBuilder? module = null;

		foreach (var subsection in subsections)
		{
			var name = subsection.Key;
			var file = subsection.GetValue<string>("File");
			if (String.IsNullOrWhiteSpace(file)) continue;
			if (!Path.IsPathRooted(file)) file = Path.Combine(configuration.DataDir, "logs", file);
			var logCategory = subsection.GetValue<bool>("LogCategory", true);
			var logEventId = subsection.GetValue<bool>("LogEventId", false);
			var rotate = subsection.GetValue<int>("Rotate", -1);

			if (module == null) module = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("FileLoggers"), AssemblyBuilderAccess.Run).DefineDynamicModule("FileLoggers");
			var type = module.DefineType(name, TypeAttributes.Class | TypeAttributes.Public, typeof(FileLoggerProvider));
			var parentConstructor = typeof(FileLoggerProvider).GetConstructors().First();
			var parentParameters = parentConstructor.GetParameters();
			var constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parentParameters.Select(parameter => parameter.ParameterType).ToArray());
			var il = constructor.GetILGenerator();
			for (int i = 0; i <= parentParameters.Length; i++)
			{
				il.Emit(OpCodes.Ldarg, i);
			}
			il.Emit(OpCodes.Call, parentConstructor);
			il.Emit(OpCodes.Ret);
			type.SetCustomAttribute(new CustomAttributeBuilder(typeof(ProviderAliasAttribute).GetConstructors().First(), [name]));
			var impl = type.CreateType();
			services.AddSingleton<ILoggerProvider>(services => (ILoggerProvider)ActivatorUtilities.CreateInstance(services, impl!, file, logCategory, logEventId, rotate));
		}
	}
}
