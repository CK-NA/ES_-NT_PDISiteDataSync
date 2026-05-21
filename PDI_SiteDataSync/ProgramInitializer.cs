using Microsoft.Extensions.Configuration;
using NLog;
using PDI_SiteDataSync.Utility;

namespace PDI_SiteDataSync;

public class ProgramInitializer
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _configBasePath;

    public string InputFolder { get; private set; } = string.Empty;
    public string LogsFolder { get; private set; } = string.Empty;
    public string ArchiveFolder { get; private set; } = string.Empty;
    public string WorksheetName { get; private set; } = string.Empty;
    public string ColumnName { get; private set; } = string.Empty;
    public string CK_ReportingConnectionString { get; private set; } = string.Empty;
    public string CommonConnectionString { get; private set; } = string.Empty;
    public string AddHolidaySitesAndOrganizationsProc { get; private set; } = string.Empty;
    public string AddSitesToSiteXRefProc { get; private set; } = string.Empty;
    public Logger Logger { get; private set; } = null!;

    public ProgramInitializer() 
        : this(new FileSystemService(), new NLogLoggerFactory(), Directory.GetCurrentDirectory())
    {
    }

    public ProgramInitializer(IFileSystemService fileSystemService, ILoggerFactory loggerFactory, string configBasePath)
    {
        _fileSystemService = fileSystemService;
        _loggerFactory = loggerFactory;
        _configBasePath = configBasePath;
    }

    public void Initialize()
    {
        // Load configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(_configBasePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Read folder settings from configuration
        string baseFolder = configuration["FolderSettings:BaseFolder"] 
            ?? throw new InvalidOperationException("FolderSettings:BaseFolder not configured");
        string inputSubfolder = configuration["FolderSettings:InputSubfolder"] ?? "Input";
        string logsSubfolder = configuration["FolderSettings:LogsSubfolder"] ?? "Logs";
        string archiveSubfolder = configuration["FolderSettings:ArchiveSubfolder"] ?? "Archive";

        // Construct full paths
        InputFolder = Path.Combine(baseFolder, inputSubfolder);
        LogsFolder = Path.Combine(baseFolder, logsSubfolder);
        ArchiveFolder = Path.Combine(baseFolder, archiveSubfolder);

        // Ensure all required directories exist
        EnsureDirectoryExists(InputFolder, "Input");
        EnsureDirectoryExists(LogsFolder, "Logs");
        EnsureDirectoryExists(ArchiveFolder, "Archive");

        // Configure NLog to use the logs folder
        _loggerFactory.ConfigureLogDirectory(LogsFolder);

        // Setup NLog
        Logger = _loggerFactory.CreateLogger();
        Logger.Info("Application started");
        Logger.Debug("Configuration loaded successfully");
        Logger.Info("Folder configuration: Base={BaseFolder}, Input={InputFolder}, Logs={LogsFolder}, Archive={ArchiveFolder}",
                    baseFolder, InputFolder, LogsFolder, ArchiveFolder);

        // Read other settings from configuration
        WorksheetName = configuration["ExcelSettings:WorksheetName"] 
            ?? throw new InvalidOperationException("ExcelSettings:WorksheetName not configured");
        ColumnName = configuration["ExcelSettings:ColumnName"] 
            ?? throw new InvalidOperationException("ExcelSettings:ColumnName not configured");
        CK_ReportingConnectionString = configuration.GetConnectionString("CK_Reporting") 
            ?? throw new InvalidOperationException("ConnectionString 'CK_Reporting' not configured");
        CommonConnectionString = configuration.GetConnectionString("Common") 
            ?? throw new InvalidOperationException("ConnectionString 'Common' not configured");
        AddHolidaySitesAndOrganizationsProc = configuration["StoredProcedures:AddHolidaySitesAndOrganizations"] 
            ?? throw new InvalidOperationException("StoredProcedure 'AddHolidaySitesAndOrganizations' not configured");
        AddSitesToSiteXRefProc = configuration["StoredProcedures:AddSitesToSiteXRef"] 
            ?? throw new InvalidOperationException("StoredProcedure 'AddSitesToSiteXRef' not configured");

        Logger.Info("Configuration settings: Worksheet={Worksheet}, Column={Column}", WorksheetName, ColumnName);
    }

    private void EnsureDirectoryExists(string path, string folderName)
    {
        if (!_fileSystemService.DirectoryExists(path))
        {
            Console.WriteLine($"Creating {folderName} folder: {path}");
            _fileSystemService.CreateDirectory(path);
            Console.WriteLine($"{folderName} folder created successfully");
        }
        else
        {
            Console.WriteLine($"{folderName} folder exists: {path}");
        }
    }
}
