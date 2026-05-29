namespace CK_NA.Template.ConsoleApp;

public class ProgramInitializer : AppInitializerBase
{
    public ApplicationConfiguration AppConfig { get; private set; } = null!;

    protected override void LoadAppConfiguration(IConfigurationRoot configuration)
    {
        // TODO: Load your app-specific configuration from appsettings.json here.
        // Example:
        // var connectionString = configuration.GetConnectionString("DefaultConnection")
        //     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        var archiveConfig = new ArchiveConfiguration(
            ArchiveFolder: Configuration!["ArchiveFolder"] ?? "Archive",
            DataFilePattern: Configuration!["DataFilePattern"] ?? "*.xlsx",
            LogFilePattern: Configuration!["LogFilePattern"] ?? "*.log",
            RetentionDays: int.TryParse(Configuration!["RetentionDays"], out var days) ? days : 30);

        AppConfig = new ApplicationConfiguration
        {
            ArchiveConfig = archiveConfig
        };
    }
}
