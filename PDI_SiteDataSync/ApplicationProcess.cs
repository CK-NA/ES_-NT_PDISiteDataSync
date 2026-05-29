using PDI_SiteDataSync.Configuration;
using PDI_SiteDataSync.Utility;

namespace PDI_SiteDataSync;

/// <summary>
/// Handles the core business logic for processing Excel data and executing database operations.
/// </summary>
public class ApplicationProcess
{
	private readonly Logger _logger;
	private readonly IExcelDataReader _excelReader;
	private readonly IArchiveManager _archiveManager;
	private readonly IDatabaseService _ckReportingService;
	private readonly IDatabaseService _commonService;
	private readonly ApplicationConfiguration _configuration;

	/// <summary>
	/// Constructor for production use - creates all dependencies from initializer.
	/// </summary>
	public ApplicationProcess(ProgramInitializer initializer)
		: this(
			initializer.Logger,
			new ExcelDataReader(initializer.Logger, initializer.ApplicationConfig.ExcelReaderConfig),
			new ArchiveManager(initializer.Logger, new NLogLoggerFactory(), initializer.ApplicationConfig.ArchiveConfig),
			new DatabaseService(initializer.Logger, initializer.ApplicationConfig.CK_ReportingConnectionString),
			new DatabaseService(initializer.Logger, initializer.ApplicationConfig.CommonConnectionString),
			initializer.ApplicationConfig)
	{
	}

	/// <summary>
	/// Constructor for testing - allows injection of all dependencies.
	/// </summary>
	public ApplicationProcess(
		Logger logger,
		IExcelDataReader excelReader,
		IArchiveManager archiveManager,
		IDatabaseService ckReportingService,
		IDatabaseService commonService,
		ApplicationConfiguration configuration)
	{
		_logger = logger;
		_excelReader = excelReader;
		_archiveManager = archiveManager;
		_ckReportingService = ckReportingService;
		_commonService = commonService;
		_configuration = configuration;
	}

	/// <summary>
	/// Executes the main application workflow: read Excel data, execute stored procedures, and archive files.
	/// </summary>
	public async Task ExecuteAsync()
	{
		// Read data from Excel file
		var excelResult = _excelReader.ReadDataFromExcel();

		if (excelResult == null)
		{
			// No file found - this is a valid scenario
			_logger.Info("No input file to process. Completing successfully.");

			// Archive logs only (no data file to archive)
			_archiveManager.ArchiveLogsOnly();
			return;
		}

		var (excelFile, storeNumbers) = excelResult.Value;

		// Create comma-delimited string
		string commaDelimitedString = string.Join(",", storeNumbers);

		_logger.Debug("Comma-delimited string: {StoreList}", commaDelimitedString);
		Console.WriteLine($"Comma-delimited string:\n{commaDelimitedString}");

		// Execute first stored procedure: AddHolidaySitesAndOrganizations (CK_Reporting database)
		_logger.Info("Executing stored procedure: {StoredProcedure}", _configuration.AddHolidaySitesProc);
		var siteIds = await _ckReportingService.AddHolidaySitesAndOrganizationsAsync(_configuration.AddHolidaySitesProc, commaDelimitedString);
		_logger.Info("CK_Reporting: Received {Count} Site_IDs from stored procedure", siteIds.Count);

		// Only execute second stored procedure if we have results from the first
		int commonCount = 0;
		if (siteIds.Count > 0)
		{
			// Convert Site_IDs to comma-delimited string
			string siteIdString = string.Join(",", siteIds);
			_logger.Debug("Site_ID string for SiteXRef: {SiteIdString}", siteIdString);

			// Execute second stored procedure: AddSitesToSiteXRef (Common database)
			_logger.Info("Executing stored procedure: {StoredProcedure}", _configuration.AddSitesToSiteXRefProc);
			commonCount = await _commonService.AddSitesToSiteXRefAsync(_configuration.AddSitesToSiteXRefProc, siteIdString);
			_logger.Info("Common: Added {AddedCount} sites to SiteXRef", commonCount);
		}
		else
		{
			_logger.Info("No Site_IDs returned from first stored procedure. Skipping SiteXRef update.");
		}

		Console.WriteLine($"✓ Database operations completed successfully");
		Console.WriteLine($"  - CK_Reporting: {siteIds.Count} Site_IDs returned");
		Console.WriteLine($"  - Common: {commonCount} sites added to SiteXRef");
		Console.WriteLine();

		// Archive data and log files
		Console.WriteLine($"Starting archival process for: {excelFile}");
		_archiveManager.ArchiveAll(excelFile);
		Console.WriteLine($"✓ Archival completed successfully");
	}
}
