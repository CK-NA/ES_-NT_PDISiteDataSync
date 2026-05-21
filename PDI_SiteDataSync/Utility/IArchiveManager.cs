namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Interface for archive operations to enable testability.
/// </summary>
public interface IArchiveManager
{
    string ArchiveDataFile(string excelFilePath, string timestamp);
    string? ArchiveLogFiles(string timestamp);
    (string DataArchive, string? LogArchive) ArchiveAll(string excelFilePath);
    void ArchiveLogsOnly();
}
