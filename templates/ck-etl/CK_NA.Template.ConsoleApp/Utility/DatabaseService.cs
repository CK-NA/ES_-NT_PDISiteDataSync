//#if (includeDatabase)
namespace CK_NA.Template.ConsoleApp.Utility;

public interface IDatabaseService
{
    Task<bool> TestConnectionAsync();
}

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            _logger.Info("Database connection successful.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Database connection failed.");
            return false;
        }
    }

    // TODO: Add your database operations here.
}
//#endif
