namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Application-specific database service that delegates to the shared generic DatabaseService.
/// </summary>
public class SiteDataDatabaseService : ISiteDataDatabaseService
{
	private readonly CK_NA.ConsoleApp.Core.Data.IDatabaseService _dbService;

	public SiteDataDatabaseService(Logger logger, string connectionString)
	{
		_dbService = new CK_NA.ConsoleApp.Core.Data.DatabaseService(logger, connectionString);
	}

	/// <summary>
	/// Executes a stored procedure that adds holiday sites and organizations.
	/// </summary>
	public async Task<List<int>> AddHolidaySitesAndOrganizationsAsync(string storedProcedureName, string storeList)
	{
		var parameters = new Dictionary<string, object> { { "siteList", storeList } };
		return await _dbService.ExecuteReaderScalarListAsync<int>(storedProcedureName, parameters);
	}

	/// <summary>
	/// Executes a stored procedure that adds sites to the SiteXRef table.
	/// </summary>
	public async Task<int> AddSitesToSiteXRefAsync(string storedProcedureName, string storeList)
	{
		var parameters = new Dictionary<string, object> { { "siteList", storeList } };
		return await _dbService.ExecuteScalarAsync<int>(storedProcedureName, parameters) is int result ? result : 0;
	}

	/// <summary>
	/// Tests the database connection.
	/// </summary>
	public async Task<bool> TestConnectionAsync()
	{
		return await _dbService.TestConnectionAsync();
	}
}
