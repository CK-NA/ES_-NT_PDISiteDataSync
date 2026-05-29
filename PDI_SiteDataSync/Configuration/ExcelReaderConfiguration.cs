namespace PDI_SiteDataSync.Configuration;

/// <summary>
/// Configuration settings for Excel data reading operations.
/// </summary>
public class ExcelReaderConfiguration
{
	public required string InputFolder { get; init; }
	public required string WorksheetName { get; init; }
	public required string ColumnName { get; init; }
}
