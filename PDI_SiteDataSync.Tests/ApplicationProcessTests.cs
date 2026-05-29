using PDI_SiteDataSync.Configuration;
using PDI_SiteDataSync.Utility;

namespace PDI_SiteDataSync.Tests;

public class ApplicationProcessTests : IDisposable
{
	private readonly Logger _logger;
	private readonly Mock<IExcelDataReader> _mockExcelReader;
	private readonly Mock<IArchiveManager> _mockArchiveManager;
	private readonly Mock<ISiteDataDatabaseService> _mockCkReportingService;
	private readonly Mock<ISiteDataDatabaseService> _mockCommonService;
	private readonly string _testFolder;

	public ApplicationProcessTests()
	{
		Console.WriteLine($"=== CONSTRUCTOR START: {DateTime.Now:HH:mm:ss.fff} ===");
		System.Diagnostics.Debug.WriteLine($"=== CONSTRUCTOR START: {DateTime.Now:HH:mm:ss.fff} ===");

		_logger = TestLoggerFactory.CreateTestLogger();
		Console.WriteLine($"=== Logger created: {DateTime.Now:HH:mm:ss.fff} ===");

		_mockExcelReader = new Mock<IExcelDataReader>();
		_mockArchiveManager = new Mock<IArchiveManager>();
		_mockCkReportingService = new Mock<ISiteDataDatabaseService>();
		_mockCommonService = new Mock<ISiteDataDatabaseService>();

		_testFolder = Path.Combine(Path.GetTempPath(), "PDI_App_Test_" + Guid.NewGuid().ToString());
		Directory.CreateDirectory(_testFolder);

		// Setup default mock returns for database services
		_mockCkReportingService.Setup(x => x.AddHolidaySitesAndOrganizationsAsync(It.IsAny<string>(), It.IsAny<string>()))
			.ReturnsAsync(new List<int> { 1001, 1002, 1003, 1004, 1005 });
		_mockCommonService.Setup(x => x.AddSitesToSiteXRefAsync(It.IsAny<string>(), It.IsAny<string>()))
			.ReturnsAsync(3);

		Console.WriteLine($"=== CONSTRUCTOR END: {DateTime.Now:HH:mm:ss.fff} ===");
		System.Diagnostics.Debug.WriteLine($"=== CONSTRUCTOR END: {DateTime.Now:HH:mm:ss.fff} ===");
	}

	[Fact]
	public async Task ExecuteAsync_WithNoInputFile_CompletesSuccessfully()
	{
		System.Diagnostics.Debug.WriteLine("=== START: ExecuteAsync_WithNoInputFile_CompletesSuccessfully ===");
		Console.WriteLine("=== START: ExecuteAsync_WithNoInputFile_CompletesSuccessfully ===");

		// Arrange
		_mockExcelReader.Setup(x => x.ReadDataFromExcel()).Returns((ValueTuple<string, List<string>>?) null);

		var config = new ApplicationConfiguration
		{
			CK_ReportingConnectionString = "TestConnectionString1",
			CommonConnectionString = "TestConnectionString2",
			AddHolidaySitesProc = "Proc1",
			AddSitesToSiteXRefProc = "Proc2",
			ExcelReaderConfig = new ExcelReaderConfiguration { InputFolder = "test", WorksheetName = "test", ColumnName = "test" },
			ArchiveConfig = new ArchiveConfiguration { ArchiveFolder = "test", LogsFolder = "test" }
		};

		var process = new ApplicationProcess(
			_logger,
			_mockExcelReader.Object,
			_mockArchiveManager.Object,
			_mockCkReportingService.Object,
			_mockCommonService.Object,
			config);

		// Act
		await process.ExecuteAsync();

		// Assert - verify behavior
		_mockExcelReader.Verify(x => x.ReadDataFromExcel(), Times.Once);
		_mockArchiveManager.Verify(x => x.ArchiveLogsOnly(), Times.Once);
		_mockArchiveManager.Verify(x => x.ArchiveAll(It.IsAny<string>()), Times.Never);

		// Verify database services were NOT called
		_mockCkReportingService.Verify(x => x.AddHolidaySitesAndOrganizationsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		_mockCommonService.Verify(x => x.AddSitesToSiteXRefAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

		Console.WriteLine("=== END: ExecuteAsync_WithNoInputFile_CompletesSuccessfully ===");
	}

	[Fact]
	public async Task ExecuteAsync_WithValidExcelFile_ProcessesSuccessfully()
	{
		System.Diagnostics.Debug.WriteLine("=== START: ExecuteAsync_WithValidExcelFile_ProcessesSuccessfully ===");
		Console.WriteLine("=== START: ExecuteAsync_WithValidExcelFile_ProcessesSuccessfully ===");

		// Arrange
		var testFilePath = Path.Combine(_testFolder, "test.xlsx");
		var testData = new List<string> { "101", "102", "103" };
		_mockExcelReader.Setup(x => x.ReadDataFromExcel()).Returns((testFilePath, testData));

		var config = new ApplicationConfiguration
		{
			CK_ReportingConnectionString = "TestConnectionString1",
			CommonConnectionString = "TestConnectionString2",
			AddHolidaySitesProc = "Proc1",
			AddSitesToSiteXRefProc = "Proc2",
			ExcelReaderConfig = new ExcelReaderConfiguration { InputFolder = "test", WorksheetName = "test", ColumnName = "test" },
			ArchiveConfig = new ArchiveConfiguration { ArchiveFolder = "test", LogsFolder = "test" }
		};

		var process = new ApplicationProcess(
			_logger,
			_mockExcelReader.Object,
			_mockArchiveManager.Object,
			_mockCkReportingService.Object,
			_mockCommonService.Object,
			config);

		// Act
		await process.ExecuteAsync();

		// Assert - verify all operations were called correctly
		_mockExcelReader.Verify(x => x.ReadDataFromExcel(), Times.Once);
		_mockCkReportingService.Verify(x => x.AddHolidaySitesAndOrganizationsAsync("Proc1", "101,102,103"), Times.Once);
		_mockCommonService.Verify(x => x.AddSitesToSiteXRefAsync("Proc2", "1001,1002,1003,1004,1005"), Times.Once);
		_mockArchiveManager.Verify(x => x.ArchiveAll(testFilePath), Times.Once);
		_mockArchiveManager.Verify(x => x.ArchiveLogsOnly(), Times.Never);

		Console.WriteLine("=== END: ExecuteAsync_WithValidExcelFile_ProcessesSuccessfully ===");
	}

	[Fact]
	public async Task ExecuteAsync_WithInvalidWorksheet_HandlesGracefully()
	{
		// Arrange - Excel reader returns null when worksheet is invalid
		_mockExcelReader.Setup(x => x.ReadDataFromExcel()).Returns((ValueTuple<string, List<string>>?) null);

		var config = new ApplicationConfiguration
		{
			CK_ReportingConnectionString = "TestConnectionString1",
			CommonConnectionString = "TestConnectionString2",
			AddHolidaySitesProc = "Proc1",
			AddSitesToSiteXRefProc = "Proc2",
			ExcelReaderConfig = new ExcelReaderConfiguration { InputFolder = "test", WorksheetName = "test", ColumnName = "test" },
			ArchiveConfig = new ArchiveConfiguration { ArchiveFolder = "test", LogsFolder = "test" }
		};

		var process = new ApplicationProcess(
			_logger,
			_mockExcelReader.Object,
			_mockArchiveManager.Object,
			_mockCkReportingService.Object,
			_mockCommonService.Object,
			config);

		// Act
		await process.ExecuteAsync();

		// Assert - verify early return behavior
		_mockExcelReader.Verify(x => x.ReadDataFromExcel(), Times.Once);
		_mockArchiveManager.Verify(x => x.ArchiveLogsOnly(), Times.Once);

		// Verify database services were NOT called
		_mockCkReportingService.Verify(x => x.AddHolidaySitesAndOrganizationsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		_mockCommonService.Verify(x => x.AddSitesToSiteXRefAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task ExecuteAsync_WhenFirstProcReturnsNoSiteIds_SkipsSecondProc()
	{
		// Arrange - First stored procedure returns empty list
		var testFilePath = Path.Combine(_testFolder, "test.xlsx");
		var testData = new List<string> { "101", "102", "103" };
		_mockExcelReader.Setup(x => x.ReadDataFromExcel()).Returns((testFilePath, testData));
		_mockCkReportingService.Setup(x => x.AddHolidaySitesAndOrganizationsAsync(It.IsAny<string>(), It.IsAny<string>()))
			.ReturnsAsync(new List<int>());

		var config = new ApplicationConfiguration
		{
			CK_ReportingConnectionString = "TestConnectionString1",
			CommonConnectionString = "TestConnectionString2",
			AddHolidaySitesProc = "Proc1",
			AddSitesToSiteXRefProc = "Proc2",
			ExcelReaderConfig = new ExcelReaderConfiguration { InputFolder = "test", WorksheetName = "test", ColumnName = "test" },
			ArchiveConfig = new ArchiveConfiguration { ArchiveFolder = "test", LogsFolder = "test" }
		};

		var process = new ApplicationProcess(
			_logger,
			_mockExcelReader.Object,
			_mockArchiveManager.Object,
			_mockCkReportingService.Object,
			_mockCommonService.Object,
			config);

		// Act
		await process.ExecuteAsync();

		// Assert
		_mockExcelReader.Verify(x => x.ReadDataFromExcel(), Times.Once);
		_mockCkReportingService.Verify(x => x.AddHolidaySitesAndOrganizationsAsync("Proc1", "101,102,103"), Times.Once);
		_mockCommonService.Verify(x => x.AddSitesToSiteXRefAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		_mockArchiveManager.Verify(x => x.ArchiveAll(testFilePath), Times.Once);
	}

	public void Dispose()
	{
		if (Directory.Exists(_testFolder))
		{
			try
			{
				Directory.Delete(_testFolder, true);
			}
			catch
			{
				// Ignore cleanup errors
			}
		}
	}
}
