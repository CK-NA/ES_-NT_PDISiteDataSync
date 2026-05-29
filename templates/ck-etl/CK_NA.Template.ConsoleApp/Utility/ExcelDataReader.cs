//#if (includeExcel)
namespace CK_NA.Template.ConsoleApp.Utility;

public interface IExcelDataReader
{
    (string ExcelFilePath, List<string> Values)? ReadDataFromExcel();
}

public class ExcelDataReader : IExcelDataReader
{
    private readonly ExcelReaderConfiguration _config;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public ExcelDataReader(ExcelReaderConfiguration config)
    {
        _config = config;
    }

    public (string ExcelFilePath, List<string> Values)? ReadDataFromExcel()
    {
        var files = Directory.GetFiles(_config.InputFolder, _config.FilePattern);
        if (files.Length == 0)
        {
            _logger.Info("No input files found matching pattern '{Pattern}' in '{Folder}'.",
                _config.FilePattern, _config.InputFolder);
            return null;
        }

        var filePath = files[0];
        _logger.Info("Reading data from '{FilePath}'.", filePath);

        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets[_config.WorksheetName];
        if (worksheet == null)
        {
            _logger.Warn("Worksheet '{Name}' not found in '{File}'.", _config.WorksheetName, filePath);
            return null;
        }

        var values = new List<string>();
        var colIndex = FindColumnIndex(worksheet, _config.ColumnName);
        if (colIndex < 0)
        {
            _logger.Warn("Column '{Column}' not found in worksheet.", _config.ColumnName);
            return null;
        }

        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
        {
            var cellValue = worksheet.Cells[row, colIndex].Text?.Trim();
            if (!string.IsNullOrEmpty(cellValue))
                values.Add(cellValue);
        }

        return (filePath, values);
    }

    private static int FindColumnIndex(ExcelWorksheet worksheet, string columnName)
    {
        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
        {
            if (string.Equals(worksheet.Cells[1, col].Text?.Trim(), columnName, StringComparison.OrdinalIgnoreCase))
                return col;
        }
        return -1;
    }
}
//#endif
