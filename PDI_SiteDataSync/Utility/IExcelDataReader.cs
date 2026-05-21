namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Interface for Excel data reading operations to enable testability.
/// </summary>
public interface IExcelDataReader
{
    (string ExcelFilePath, List<string> Values)? ReadDataFromExcel();
}
