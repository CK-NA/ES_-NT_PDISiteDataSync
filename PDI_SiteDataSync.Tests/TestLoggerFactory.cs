using NLog;
using NLog.Config;
using NLog.Targets;

namespace PDI_SiteDataSync.Tests;

/// <summary>
/// Helper to create test loggers that don't cause hangs.
/// </summary>
public static class TestLoggerFactory
{
    /// <summary>
    /// Creates a test logger that writes to memory (not files).
    /// Safe to use in tests without hanging.
    /// Configuration is handled by TestFrameworkInitializer.
    /// </summary>
    public static Logger CreateTestLogger()
    {
        // Simply return a logger - configuration is handled globally by TestFrameworkInitializer
        // Use a consistent name to avoid LogManager state issues
        return LogManager.GetLogger("TestLogger");
    }
}
