using OfficeOpenXml;
using NLog;
using PDI_SiteDataSync;
using PDI_SiteDataSync.Utility;

// Set the license context for EPPlus (required for version 5.0+)
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Initialize program and configuration
ProgramInitializer? initializer = null;

try
{
    initializer = new ProgramInitializer();
    initializer.Initialize();

    // Create logger factory
    var loggerFactory = new NLogLoggerFactory();

    // Create database services
    var ckReportingService = new DatabaseService(initializer.Logger, initializer.CK_ReportingConnectionString);
    var commonService = new DatabaseService(initializer.Logger, initializer.CommonConnectionString);

    // Create Excel reader
    var excelReader = new ExcelDataReader(
        initializer.Logger,
        initializer.InputFolder,
        initializer.WorksheetName,
        initializer.ColumnName);

    // Create archive manager
    var archiveManager = new ArchiveManager(
        initializer.Logger,
        loggerFactory,
        initializer.ArchiveFolder,
        initializer.LogsFolder);

    // Create and execute the application process
    var applicationProcess = new ApplicationProcess(
        initializer.Logger,
        excelReader,
        archiveManager,
        ckReportingService,
        commonService,
        initializer.AddHolidaySitesAndOrganizationsProc,
        initializer.AddSitesToSiteXRefProc);

    await applicationProcess.ExecuteAsync();
}
catch (Exception ex)
{
    if (initializer?.Logger != null)
    {
        initializer.Logger.Fatal(ex, "Application terminated unexpectedly");
        Console.WriteLine($"FATAL ERROR: Application terminated unexpectedly - {ex.Message}");
    }
    else
    {
        Console.WriteLine($"Fatal Error: {ex.Message}");
    }
    throw;
}
finally
{
    initializer?.Logger?.Info("Application shutting down");
    LogManager.Shutdown();
}