[assembly: Xunit.TestFramework("PDI_SiteDataSync.Tests.TestFrameworkInitializer", "PDI_SiteDataSync.Tests")]

namespace PDI_SiteDataSync.Tests;

/// <summary>
/// Custom test framework initializer that configures NLog before any tests run.
/// This prevents hanging caused by auto-discovery of nlog.config with file targets.
/// </summary>
public class TestFrameworkInitializer : XunitTestFramework
{
	public TestFrameworkInitializer(IMessageSink messageSink)
		: base(messageSink)
	{
		// Force NLog configuration to memory-only targets before any test discovery
		var config = new LoggingConfiguration();

		var memoryTarget = new MemoryTarget("memory")
		{
			Layout = "${longdate} ${level:uppercase=true} ${message} ${exception:format=tostring}"
		};

		config.AddTarget(memoryTarget);
		config.AddRule(LogLevel.Trace, LogLevel.Fatal, memoryTarget);

		// Set this as the global NLog configuration
		LogManager.Configuration = config;

		messageSink.OnMessage(new DiagnosticMessage("NLog initialized with memory-only target for tests"));
	}
}
