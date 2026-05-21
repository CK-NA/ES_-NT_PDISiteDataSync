namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Interface for file system operations to enable testability.
/// </summary>
public interface IFileSystemService
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
}
