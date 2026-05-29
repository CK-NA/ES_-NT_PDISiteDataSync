using PDI_SiteDataSync.Configuration;

namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Handles Excel file discovery and data extraction operations.
/// </summary>
public class ExcelDataReader : IExcelDataReader
{
	private readonly Logger _logger;
	private readonly ExcelReaderConfiguration _configuration;

	public ExcelDataReader(Logger logger, ExcelReaderConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
	}

	/// <summary>
	/// Discovers an Excel file, reads data from the specified worksheet and column, and returns the results.
	/// </summary>
	/// <returns>A tuple containing the Excel file path and the list of extracted values, or null if operation fails.</returns>
	public (string ExcelFilePath, List<string> Values)? ReadDataFromExcel()
	{
		_logger.Debug("Searching for Excel file in directory: {Directory}", _configuration.InputFolder);

		// Find the Excel file in the input directory
		string? excelFile = Directory.GetFiles(_configuration.InputFolder, "*.xlsx").FirstOrDefault()
						   ?? Directory.GetFiles(_configuration.InputFolder, "*.xls").FirstOrDefault();

		if (excelFile == null)
		{
			_logger.Info("No Excel file found in {InputFolder}. Processing will complete without data operations.", _configuration.InputFolder);
			return null;
		}

		_logger.Info("Reading Excel file: {ExcelFile}", excelFile);

		using var package = new ExcelPackage(new FileInfo(excelFile));

		// Log all available worksheet names for debugging
		_logger.Debug("Found {WorksheetCount} worksheets in file", package.Workbook.Worksheets.Count);
		foreach (var ws in package.Workbook.Worksheets)
		{
			_logger.Debug("  Worksheet: '{WorksheetName}' (Index: {Index})", ws.Name, ws.Index);
		}

		// Try to find worksheet by name (case-insensitive comparison)
		var worksheet = package.Workbook.Worksheets.FirstOrDefault(ws =>
			string.Equals(ws.Name, _configuration.WorksheetName, StringComparison.OrdinalIgnoreCase));

		if (worksheet == null)
		{
			_logger.Error("Worksheet '{WorksheetName}' not found in Excel file", _configuration.WorksheetName);
			_logger.Error("Available worksheets: {AvailableWorksheets}",
				string.Join(", ", package.Workbook.Worksheets.Select(ws => $"'{ws.Name}'")));
			return null;
		}

		_logger.Debug("Worksheet '{WorksheetName}' loaded successfully", _configuration.WorksheetName);

		// Find the column index for the specified column
		int columnIndex = FindColumnIndex(worksheet);
		if (columnIndex == -1)
		{
			_logger.Error("Column '{ColumnName}' not found in worksheet '{WorksheetName}'", _configuration.ColumnName, _configuration.WorksheetName);
			return null;
		}

		_logger.Debug("Column '{ColumnName}' found at index {ColumnIndex}", _configuration.ColumnName, columnIndex);

		// Read all values from the column (skipping header row)
		var values = ExtractColumnValues(worksheet, columnIndex);

		_logger.Info("Found {ValueCount} values in column '{ColumnName}'", values.Count, _configuration.ColumnName);

		return (excelFile, values);
	}

	/// <summary>
	/// Finds the column index by matching the header value.
	/// </summary>
	private int FindColumnIndex(ExcelWorksheet worksheet)
	{
		for (int col = 1; col <= worksheet.Dimension.Columns; col++)
		{
			var headerValue = worksheet.Cells[1, col].Value?.ToString();
			if (headerValue?.Trim() == _configuration.ColumnName)
			{
				return col;
			}
		}

		return -1;
	}

	/// <summary>
	/// Extracts non-empty values from the specified column, starting from row 2 (skipping header).
	/// </summary>
	private List<string> ExtractColumnValues(ExcelWorksheet worksheet, int columnIndex)
	{
		var values = new List<string>();

		for (int row = 2; row <= worksheet.Dimension.Rows; row++)
		{
			var cellValue = worksheet.Cells[row, columnIndex].Value?.ToString()?.Trim();
			if (!string.IsNullOrWhiteSpace(cellValue))
			{
				values.Add(cellValue);
			}
		}

		return values;
	}
}
