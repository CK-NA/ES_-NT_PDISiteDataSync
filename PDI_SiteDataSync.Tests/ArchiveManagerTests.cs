using PDI_SiteDataSync.Configuration;
using PDI_SiteDataSync.Utility;

namespace PDI_SiteDataSync.Tests;

public class ArchiveManagerTests : IDisposable
{
	private readonly Logger _logger;
	private readonly Mock<ILoggerFactory> _mockLoggerFactory;
	private readonly string _testArchiveFolder;
	private readonly string _testLogsFolder;

	public ArchiveManagerTests()
	{
		_logger = TestLoggerFactory.CreateTestLogger();
		_mockLoggerFactory = new Mock<ILoggerFactory>();
		var testRoot = Path.Combine(Path.GetTempPath(), "PDI_Archive_Test_" + Guid.NewGuid().ToString());
		_testArchiveFolder = Path.Combine(testRoot, "Archive");
		_testLogsFolder = Path.Combine(testRoot, "Logs");

		Directory.CreateDirectory(_testArchiveFolder);
		Directory.CreateDirectory(_testLogsFolder);
	}

	[Fact]
	public void ArchiveDataFile_CreatesZipFileWithTimestamp()
	{
		// Arrange
		var testDataFile = Path.Combine(_testLogsFolder, "test.xlsx");
		File.WriteAllText(testDataFile, "Test Excel Content");

		var config = new ArchiveConfiguration
		{
			ArchiveFolder = _testArchiveFolder,
			LogsFolder = _testLogsFolder
		};
		var archiveManager = new ArchiveManager(_logger, _mockLoggerFactory.Object, config);
		var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

		// Act
		var result = archiveManager.ArchiveDataFile(testDataFile, timestamp);

		// Assert
		result.Should().Contain($"Data_{timestamp}.zip");
		File.Exists(result).Should().BeTrue();
		File.Exists(testDataFile).Should().BeFalse("original file should be deleted");
	}

	[Fact]
	public void ArchiveDataFile_WhenFileDoesNotExist_ReturnsNull()
	{
		// Arrange
		var nonExistentFile = Path.Combine(_testLogsFolder, "does_not_exist.xlsx");

		var config = new ArchiveConfiguration
		{
			ArchiveFolder = _testArchiveFolder,
			LogsFolder = _testLogsFolder
		};
		var archiveManager = new ArchiveManager(_logger, _mockLoggerFactory.Object, config);
		var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

		// Act
		var result = archiveManager.ArchiveDataFile(nonExistentFile, timestamp);

		// Assert
		result.Should().BeNull();
		Directory.GetFiles(_testArchiveFolder, "Data_*.zip").Should().BeEmpty("no archive should be created");
	}

	[Fact]
	public void ArchiveLogFiles_WhenNoLogsExist_ReturnsNull()
	{
		// Arrange
		var config = new ArchiveConfiguration
		{
			ArchiveFolder = _testArchiveFolder,
			LogsFolder = _testLogsFolder
		};
		var archiveManager = new ArchiveManager(_logger, _mockLoggerFactory.Object, config);
		var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

		// Act
		var result = archiveManager.ArchiveLogFiles(timestamp);

		// Assert
		result.Should().BeNull();
		_mockLoggerFactory.Verify(x => x.ShutdownLogs(), Times.Never, "ShutdownLogs should not be called when no logs exist");
	}

	[Fact]
	public void ArchiveLogFiles_WhenLogsExist_CreatesZipAndDeletesOriginals()
	{
		// Arrange
		var logFile1 = Path.Combine(_testLogsFolder, "app.log");
		var logFile2 = Path.Combine(_testLogsFolder, "error.log");
		File.WriteAllText(logFile1, "Log content 1");
		File.WriteAllText(logFile2, "Log content 2");

		var config = new ArchiveConfiguration
		{
			ArchiveFolder = _testArchiveFolder,
			LogsFolder = _testLogsFolder
		};
		var archiveManager = new ArchiveManager(_logger, _mockLoggerFactory.Object, config);
		var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

		// Act
		var result = archiveManager.ArchiveLogFiles(timestamp);

		// Assert
		result.Should().Contain($"Log_{timestamp}.zip");
		File.Exists(result).Should().BeTrue();
		File.Exists(logFile1).Should().BeFalse("log files should be deleted after archiving");
		File.Exists(logFile2).Should().BeFalse("log files should be deleted after archiving");
		_mockLoggerFactory.Verify(x => x.ShutdownLogs(), Times.Once, "ShutdownLogs should be called before archiving");
	}

	[Fact]
	public void ArchiveAll_CreatesDataAndLogArchives()
	{
		// Arrange
		var testDataFile = Path.Combine(_testLogsFolder, "data.xlsx");
		var logFile = Path.Combine(_testLogsFolder, "app.log");
		File.WriteAllText(testDataFile, "Excel Data");
		File.WriteAllText(logFile, "Log Content");

		var config = new ArchiveConfiguration
		{
			ArchiveFolder = _testArchiveFolder,
			LogsFolder = _testLogsFolder
		};
		var archiveManager = new ArchiveManager(_logger, _mockLoggerFactory.Object, config);

		// Act
		var (dataArchive, logArchive) = archiveManager.ArchiveAll(testDataFile);

		// Assert
		dataArchive.Should().Contain("Data_");
		dataArchive.Should().EndWith(".zip");
		File.Exists(dataArchive).Should().BeTrue();

		logArchive.Should().NotBeNull();
		logArchive.Should().Contain("Log_");
		File.Exists(logArchive).Should().BeTrue();

		File.Exists(testDataFile).Should().BeFalse();
		File.Exists(logFile).Should().BeFalse();

		_mockLoggerFactory.Verify(x => x.ShutdownLogs(), Times.Once, "ShutdownLogs should be called during ArchiveAll");
	}

	[Fact]
	public void ArchiveLogsOnly_CreatesLogArchiveWithoutDataFile()
	{
		// Arrange
		var logFile = Path.Combine(_testLogsFolder, "app.log");
		File.WriteAllText(logFile, "Log Content");

		var config = new ArchiveConfiguration
		{
			ArchiveFolder = _testArchiveFolder,
			LogsFolder = _testLogsFolder
		};
		var archiveManager = new ArchiveManager(_logger, _mockLoggerFactory.Object, config);

		// Act
		archiveManager.ArchiveLogsOnly();

		// Assert
		var archiveFiles = Directory.GetFiles(_testArchiveFolder, "Log_*.zip");
		archiveFiles.Should().HaveCount(1);
		File.Exists(logFile).Should().BeFalse();

		_mockLoggerFactory.Verify(x => x.ShutdownLogs(), Times.Once, "ShutdownLogs should be called once during ArchiveLogsOnly");
	}

	public void Dispose()
	{
		var testRoot = Path.GetDirectoryName(_testArchiveFolder);
		if (testRoot != null && Directory.Exists(testRoot))
		{
			try
			{
				Directory.Delete(testRoot, true);
			}
			catch
			{
				// Ignore cleanup errors
			}
		}
	}
}
