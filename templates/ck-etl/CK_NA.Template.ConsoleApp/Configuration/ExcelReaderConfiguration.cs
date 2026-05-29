//#if (includeExcel)
namespace CK_NA.Template.ConsoleApp.Configuration;

public class ExcelReaderConfiguration
{
    public string InputFolder { get; set; } = string.Empty;
    public string FilePattern { get; set; } = "*.xlsx";
    public string WorksheetName { get; set; } = "Sheet1";
    public string ColumnName { get; set; } = string.Empty;
}
//#endif
