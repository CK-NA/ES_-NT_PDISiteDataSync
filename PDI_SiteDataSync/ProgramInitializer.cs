using PDI_SiteDataSync.Configuration;
using PDI_SiteDataSync.Utility;

namespace PDI_SiteDataSync;

public class ProgramInitializer : AppInitializerBase
{
	public string WorksheetName { get; private set; } = string.Empty;
	public string ColumnName { get; private set; } = string.Empty;
	public string CK_ReportingConnectionString { get; private set; } = string.Empty;
	public string CommonConnectionString { get; private set; } = string.Empty;
	public string AddHolidaySitesAndOrganizationsProc { get; private set; } = string.Empty;
	public string AddSitesToSiteXRefProc { get; private set; } = string.Empty;

	// Configuration objects
	public ExcelReaderConfiguration ExcelReaderConfig { get; private set; } = null!;
	public ApplicationConfiguration ApplicationConfig { get; private set; } = null!;

	public ProgramInitializer()
		: base(new FileSystemService(), new NLogAppLoggerFactory(), Directory.GetCurrentDirectory())
	{
	}

	public ProgramInitializer(IFileSystemService fileSystemService, IAppLoggerFactory loggerFactory, string configBasePath)
		: base(fileSystemService, loggerFactory, configBasePath)
	{
	}

	protected override void LoadAppConfiguration(IConfigurationRoot configuration)
	{
		// Read Excel settings
		WorksheetName = configuration["ExcelSettings:WorksheetName"]
			?? throw new InvalidOperationException("ExcelSettings:WorksheetName not configured");
		ColumnName = configuration["ExcelSettings:ColumnName"]
			?? throw new InvalidOperationException("ExcelSettings:ColumnName not configured");

		// Read connection strings
		CK_ReportingConnectionString = configuration.GetConnectionString("CK_Reporting")
			?? throw new InvalidOperationException("ConnectionString 'CK_Reporting' not configured");
		CommonConnectionString = configuration.GetConnectionString("Common")
			?? throw new InvalidOperationException("ConnectionString 'Common' not configured");

		// Read stored procedure names
		AddHolidaySitesAndOrganizationsProc = configuration["StoredProcedures:AddHolidaySitesAndOrganizations"]
			?? throw new InvalidOperationException("StoredProcedure 'AddHolidaySitesAndOrganizations' not configured");
		AddSitesToSiteXRefProc = configuration["StoredProcedures:AddSitesToSiteXRef"]
			?? throw new InvalidOperationException("StoredProcedure 'AddSitesToSiteXRef' not configured");

		Logger.Info("Configuration settings: Worksheet={Worksheet}, Column={Column}", WorksheetName, ColumnName);

		// Create configuration objects
		ExcelReaderConfig = new ExcelReaderConfiguration
		{
			InputFolder = InputFolder,
			WorksheetName = WorksheetName,
			ColumnName = ColumnName
		};

		var archiveConfig = new CK_NA.ConsoleApp.Core.Configuration.ArchiveConfiguration
		{
			ArchiveFolder = ArchiveFolder,
			LogsFolder = LogsFolder
		};

		ApplicationConfig = new ApplicationConfiguration
		{
			CK_ReportingConnectionString = CK_ReportingConnectionString,
			CommonConnectionString = CommonConnectionString,
			AddHolidaySitesProc = AddHolidaySitesAndOrganizationsProc,
			AddSitesToSiteXRefProc = AddSitesToSiteXRefProc,
			ExcelReaderConfig = ExcelReaderConfig,
			ArchiveConfig = archiveConfig
		};
	}
}
