using NLog.Extensions.Logging;

namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Default NLog logger factory implementation.
/// </summary>
public class NLogLoggerFactory : ILoggerFactory
{
	public NLogLoggerFactory()
	{
		// Load configuration from appsettings.json
		var config = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.Build();

		LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));
	}

	public void ConfigureLogDirectory(string logDirectory)
	{
		LogManager.Configuration.Variables["logDirectory"] = logDirectory;
		LogManager.ReconfigExistingLoggers();
	}

	public Logger CreateLogger()
	{
		return LogManager.GetCurrentClassLogger();
	}

	public void FlushLogs()
	{
		LogManager.Flush();
	}

	public void ShutdownLogs()
	{
		LogManager.Shutdown();
	}
}
