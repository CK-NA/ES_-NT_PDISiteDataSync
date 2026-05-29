using PDI_SiteDataSync.Utility;

namespace PDI_SiteDataSync.Tests;

public class ProgramInitializerTests : IDisposable
{
	private readonly string _testConfigFolder;
	private readonly string _testConfigFile;
	private readonly Mock<IFileSystemService> _mockFileSystem;
	private readonly Mock<IAppLoggerFactory> _mockLoggerFactory;
	private readonly Logger _logger;

	public ProgramInitializerTests()
	{
		_testConfigFolder = Path.Combine(Path.GetTempPath(), "PDI_Config_Test_" + Guid.NewGuid().ToString());
		Directory.CreateDirectory(_testConfigFolder);
		_testConfigFile = Path.Combine(_testConfigFolder, "appsettings.json");

		_mockFileSystem = new Mock<IFileSystemService>();
		_mockLoggerFactory = new Mock<IAppLoggerFactory>();
		_logger = TestLoggerFactory.CreateTestLogger();

		// Setup default mock behaviors - base folder exists, subfolders do not
		_mockFileSystem.Setup(x => x.DirectoryExists(_testConfigFolder)).Returns(true);
		_mockFileSystem.Setup(x => x.DirectoryExists(It.Is<string>(p => p != _testConfigFolder))).Returns(false);
		_mockLoggerFactory.Setup(x => x.CreateLogger()).Returns(_logger);
	}

	[Fact]
	public void Initialize_WithValidConfig_LoadsAllSettings()
	{
		// Arrange
		CreateTestConfigFile(_testConfigFile);

		var initializer = new ProgramInitializer(_mockFileSystem.Object, _mockLoggerFactory.Object, _testConfigFolder);

		// Act
		initializer.Initialize();

		// Assert
		initializer.Logger.Should().NotBeNull();
		initializer.InputFolder.Should().NotBeEmpty();
		initializer.LogsFolder.Should().NotBeEmpty();
		initializer.ArchiveFolder.Should().NotBeEmpty();
		initializer.WorksheetName.Should().Be("Store List");
		initializer.ColumnName.Should().Be("Store #");
		initializer.CK_ReportingConnectionString.Should().Contain("CK_Reporting");
		initializer.CommonConnectionString.Should().Contain("Common");
		initializer.AddHolidaySitesAndOrganizationsProc.Should().Be("SSIS_AddHolidaySitesAndOrganizations");
		initializer.AddSitesToSiteXRefProc.Should().Be("SSIS_AddSitesToSiteXRef");

		// Verify logger factory was called
		_mockLoggerFactory.Verify(x => x.ConfigureLogDirectory(It.IsAny<string>()), Times.Once);
		_mockLoggerFactory.Verify(x => x.CreateLogger(), Times.Once);
	}

	[Fact]
	public void Initialize_CreatesRequiredFolders()
	{
		// Arrange
		CreateTestConfigFile(_testConfigFile);

		var initializer = new ProgramInitializer(_mockFileSystem.Object, _mockLoggerFactory.Object, _testConfigFolder);

		// Act
		initializer.Initialize();

		// Assert - verify CreateDirectory was called for each folder
		_mockFileSystem.Verify(x => x.CreateDirectory(It.Is<string>(p => p.Contains("Input"))), Times.Once);
		_mockFileSystem.Verify(x => x.CreateDirectory(It.Is<string>(p => p.Contains("Logs"))), Times.Once);
		_mockFileSystem.Verify(x => x.CreateDirectory(It.Is<string>(p => p.Contains("Archive"))), Times.Once);
	}

	[Fact]
	public void Initialize_WithMissingBaseFolder_ThrowsException()
	{
		// Arrange
		CreateIncompleteConfigFile(_testConfigFile);

		var initializer = new ProgramInitializer(_mockFileSystem.Object, _mockLoggerFactory.Object, _testConfigFolder);

		// Act
		Action act = () => initializer.Initialize();

		// Assert
		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*BaseFolder*");
	}

	private void CreateTestConfigFile(string filePath)
	{
		var config = @"{
  ""ConnectionStrings"": {
    ""CK_Reporting"": ""Server=testserver;Database=CK_Reporting;Integrated Security=true;"",
    ""Common"": ""Server=testserver;Database=Common;Integrated Security=true;""
  },
  ""FolderSettings"": {
    ""BaseFolder"": """ + _testConfigFolder.Replace("\\", "\\\\") + @""",
    ""InputSubfolder"": ""Input"",
    ""LogsSubfolder"": ""Logs"",
    ""ArchiveSubfolder"": ""Archive""
  },
  ""ExcelSettings"": {
    ""WorksheetName"": ""Store List"",
    ""ColumnName"": ""Store #""
  },
  ""StoredProcedures"": {
    ""AddHolidaySitesAndOrganizations"": ""SSIS_AddHolidaySitesAndOrganizations"",
    ""AddSitesToSiteXRef"": ""SSIS_AddSitesToSiteXRef""
  }
}";
		File.WriteAllText(filePath, config);
	}

	private void CreateIncompleteConfigFile(string filePath)
	{
		var config = @"{
  ""ConnectionStrings"": {},
  ""FolderSettings"": {},
  ""ExcelSettings"": {}
}";
		File.WriteAllText(filePath, config);
	}

	public void Dispose()
	{
		if (Directory.Exists(_testConfigFolder))
		{
			try
			{
				Directory.Delete(_testConfigFolder, true);
			}
			catch
			{
				// Ignore cleanup errors
			}
		}
	}
}
