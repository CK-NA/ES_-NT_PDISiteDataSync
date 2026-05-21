using NLog;

namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Default NLog logger factory implementation.
/// </summary>
public class NLogLoggerFactory : ILoggerFactory
{
    public void ConfigureLogDirectory(string logDirectory)
    {
        LogManager.Configuration.Variables["logDirectory"] = logDirectory;
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
