namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Application-specific database operations interface.
/// </summary>
public interface ISiteDataDatabaseService
{
	Task<List<int>> AddHolidaySitesAndOrganizationsAsync(string storedProcedureName, string commaDelimitedSiteNumbers);
	Task<int> AddSitesToSiteXRefAsync(string storedProcedureName, string commaDelimitedSiteNumbers);
	Task<bool> TestConnectionAsync();
}
