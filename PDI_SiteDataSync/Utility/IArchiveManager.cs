namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Interface for archive operations to enable testability.
/// </summary>
public interface IArchiveManager
{
	string? ArchiveDataFile(string filePath, string timestamp);
	string? ArchiveLogFiles(string timestamp);
	(string? DataArchive, string? LogArchive) ArchiveAll(string filePath);
	void ArchiveLogsOnly();
}
