namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Interface for logger factory to enable testability.
/// </summary>
public interface ILoggerFactory
{
	void ConfigureLogDirectory(string logDirectory);
	Logger CreateLogger();
	void FlushLogs();
	void ShutdownLogs();
}
