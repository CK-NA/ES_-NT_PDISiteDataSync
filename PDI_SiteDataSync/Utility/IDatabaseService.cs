namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Interface for database operations to enable testability.
/// </summary>
public interface IDatabaseService
{
    Task<int> AddHolidaySitesAndOrganizationsAsync(string storedProcedureName, string commaDelimitedSiteNumbers);
    Task<int> AddSitesToSiteXRefAsync(string storedProcedureName, string commaDelimitedSiteNumbers);
    Task<bool> TestConnectionAsync();
}
