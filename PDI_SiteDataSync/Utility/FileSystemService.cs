namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Default file system service implementation.
/// </summary>
public class FileSystemService : IFileSystemService
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
}
