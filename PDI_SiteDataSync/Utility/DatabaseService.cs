using Microsoft.Data.SqlClient;
using NLog;
using System.Data;

namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Handles database operations including stored procedure execution and data access.
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly Logger _logger;
    private readonly string _connectionString;

    public DatabaseService(Logger logger, string connectionString)
    {
        _logger = logger;
        _connectionString = connectionString;
    }

    /// <summary>
    /// Executes a stored procedure that adds holiday sites and organizations.
    /// </summary>
    /// <param name="storedProcedureName">Name of the stored procedure to execute</param>
    /// <param name="storeList">Comma-delimited list of store numbers</param>
    /// <returns>Number of sites/organizations added</returns>
    public async Task<int> AddHolidaySitesAndOrganizationsAsync(string storedProcedureName, string storeList)
    {
        try
        {
            _logger.Debug("Opening database connection");
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            _logger.Debug("Database connection opened successfully");

            await using var command = new SqlCommand(storedProcedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add the comma-delimited string as a parameter
            command.Parameters.AddWithValue("@siteList", storeList);
            _logger.Debug("Executing stored procedure '{StoredProcedure}' with parameter @siteList containing {StoreCount} stores", 
                          storedProcedureName, storeList.Split(',').Length);

            // Execute and get the return value
            var result = await command.ExecuteScalarAsync();
            int returnValue = result != null ? Convert.ToInt32(result) : 0;

            _logger.Debug("Stored procedure returned: {ReturnValue}", returnValue);
            return returnValue;
        }
        catch (SqlException ex)
        {
            _logger.Error(ex, "SQL error executing stored procedure '{StoredProcedure}': {ErrorMessage}", 
                          storedProcedureName, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error executing stored procedure '{StoredProcedure}'", storedProcedureName);
            throw;
        }
    }

    /// <summary>
    /// Executes a stored procedure that adds sites to the SiteXRef table.
    /// </summary>
    /// <param name="storedProcedureName">Name of the stored procedure to execute</param>
    /// <param name="storeList">Comma-delimited list of store numbers</param>
    /// <returns>Number of sites added to SiteXRef</returns>
    public async Task<int> AddSitesToSiteXRefAsync(string storedProcedureName, string storeList)
    {
        try
        {
            _logger.Debug("Opening database connection for SiteXRef update");
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            _logger.Debug("Database connection opened successfully");

            await using var command = new SqlCommand(storedProcedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add the comma-delimited string as a parameter
            command.Parameters.AddWithValue("@siteList", storeList);
            _logger.Debug("Executing stored procedure '{StoredProcedure}' with parameter @siteList containing {StoreCount} stores", 
                          storedProcedureName, storeList.Split(',').Length);

            // Execute and get the return value
            var result = await command.ExecuteScalarAsync();
            int returnValue = result != null ? Convert.ToInt32(result) : 0;

            _logger.Debug("Stored procedure returned: {ReturnValue}", returnValue);
            return returnValue;
        }
        catch (SqlException ex)
        {
            _logger.Error(ex, "SQL error executing stored procedure '{StoredProcedure}': {ErrorMessage}", 
                          storedProcedureName, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error executing stored procedure '{StoredProcedure}'", storedProcedureName);
            throw;
        }
    }

    /// <summary>
    /// Tests the database connection.
    /// </summary>
    /// <returns>True if connection is successful, false otherwise</returns>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            _logger.Debug("Testing database connection");
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            _logger.Debug("Database connection test successful");
            return true;
        }
        catch (SqlException ex)
        {
            _logger.Error(ex, "Database connection test failed: {ErrorMessage}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Database connection test failed");
            return false;
        }
    }
}
