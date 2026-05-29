namespace PDI_SiteDataSync.Configuration;

/// <summary>
/// Configuration settings for application-level operations.
/// </summary>
public class ApplicationConfiguration
{
	public required string CK_ReportingConnectionString { get; init; }
	public required string CommonConnectionString { get; init; }
	public required string AddHolidaySitesProc { get; init; }
	public required string AddSitesToSiteXRefProc { get; init; }
	public required ExcelReaderConfiguration ExcelReaderConfig { get; init; }
	public required ArchiveConfiguration ArchiveConfig { get; init; }
}
