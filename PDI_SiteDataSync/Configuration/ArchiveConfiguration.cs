namespace PDI_SiteDataSync.Configuration;

/// <summary>
/// Configuration settings for archive operations.
/// </summary>
public class ArchiveConfiguration
{
	public required string ArchiveFolder { get; init; }
	public required string LogsFolder { get; init; }
}
