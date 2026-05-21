using NLog;
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
    private readonly string _addHolidaySitesProc;
    private readonly string _addSitesToSiteXRefProc;

    public ApplicationProcess(
        Logger logger,
        IExcelDataReader excelReader,
        IArchiveManager archiveManager,
        IDatabaseService ckReportingService,
        IDatabaseService commonService,
        string addHolidaySitesProc,
        string addSitesToSiteXRefProc)
    {
        _logger = logger;
        _excelReader = excelReader;
        _archiveManager = archiveManager;
        _ckReportingService = ckReportingService;
        _commonService = commonService;
        _addHolidaySitesProc = addHolidaySitesProc;
        _addSitesToSiteXRefProc = addSitesToSiteXRefProc;
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
        _logger.Info("Executing stored procedure: {StoredProcedure}", _addHolidaySitesProc);
        var ckReportingCount = await _ckReportingService.AddHolidaySitesAndOrganizationsAsync(_addHolidaySitesProc, commaDelimitedString);
        _logger.Info("CK_Reporting: Added {AddedCount} holiday sites/organizations", ckReportingCount);

        // Execute second stored procedure: AddSitesToSiteXRef (Common database)
        _logger.Info("Executing stored procedure: {StoredProcedure}", _addSitesToSiteXRefProc);
        var commonCount = await _commonService.AddSitesToSiteXRefAsync(_addSitesToSiteXRefProc, commaDelimitedString);
        _logger.Info("Common: Added {AddedCount} sites to SiteXRef", commonCount);
        Console.WriteLine($"✓ Database operations completed successfully");
        Console.WriteLine($"  - CK_Reporting: {ckReportingCount} sites/organizations added");
        Console.WriteLine($"  - Common: {commonCount} sites added to SiteXRef");
        Console.WriteLine();

        // Archive data and log files
        Console.WriteLine($"Starting archival process for: {excelFile}");
        _archiveManager.ArchiveAll(excelFile);
        Console.WriteLine($"✓ Archival completed successfully");
    }
}
