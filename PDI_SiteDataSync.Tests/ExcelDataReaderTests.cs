using PDI_SiteDataSync.Configuration;
using PDI_SiteDataSync.Utility;

namespace PDI_SiteDataSync.Tests;

public class ExcelDataReaderTests : IDisposable
{
	private readonly Logger _logger;
	private readonly string _testFolder;

	public ExcelDataReaderTests()
	{
		_logger = TestLoggerFactory.CreateTestLogger();
		_testFolder = Path.Combine(Path.GetTempPath(), "PDI_Test_" + Guid.NewGuid().ToString());
		Directory.CreateDirectory(_testFolder);
	}

	[Fact]
	public void ReadDataFromExcel_WhenNoFileExists_ReturnsNull()
	{
		// Arrange
		var config = new ExcelReaderConfiguration
		{
			InputFolder = _testFolder,
			WorksheetName = "Sheet1",
			ColumnName = "Column1"
		};
		var reader = new ExcelDataReader(_logger, config);

		// Act
		var result = reader.ReadDataFromExcel();

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void ReadDataFromExcel_WhenFileExists_WithInvalidWorksheet_ReturnsNull()
	{
		// Arrange
		var testFile = Path.Combine(_testFolder, "test.xlsx");
		CreateTestExcelFile(testFile, "ValidSheet", "TestColumn", new[] { "Value1", "Value2" });

		var config = new ExcelReaderConfiguration
		{
			InputFolder = _testFolder,
			WorksheetName = "InvalidSheet",
			ColumnName = "TestColumn"
		};
		var reader = new ExcelDataReader(_logger, config);

		// Act
		var result = reader.ReadDataFromExcel();

		// Assert
		result.Should().BeNull();

		// Cleanup
		File.Delete(testFile);
	}

	[Fact]
	public void ReadDataFromExcel_WhenFileExists_WithInvalidColumn_ReturnsNull()
	{
		// Arrange
		var testFile = Path.Combine(_testFolder, "test.xlsx");
		CreateTestExcelFile(testFile, "Sheet1", "ValidColumn", new[] { "Value1", "Value2" });

		var config = new ExcelReaderConfiguration
		{
			InputFolder = _testFolder,
			WorksheetName = "Sheet1",
			ColumnName = "InvalidColumn"
		};
		var reader = new ExcelDataReader(_logger, config);

		// Act
		var result = reader.ReadDataFromExcel();

		// Assert
		result.Should().BeNull();

		// Cleanup
		File.Delete(testFile);
	}

	[Fact]
	public void ReadDataFromExcel_WhenFileExists_WithValidData_ReturnsData()
	{
		// Arrange
		var testFile = Path.Combine(_testFolder, "test.xlsx");
		var expectedValues = new[] { "Store1", "Store2", "Store3" };
		CreateTestExcelFile(testFile, "Sheet1", "Store #", expectedValues);

		var config = new ExcelReaderConfiguration
		{
			InputFolder = _testFolder,
			WorksheetName = "Sheet1",
			ColumnName = "Store #"
		};
		var reader = new ExcelDataReader(_logger, config);

		// Act
		var result = reader.ReadDataFromExcel();

		// Assert
		result.Should().NotBeNull();
		result.Value.ExcelFilePath.Should().Be(testFile);
		result.Value.Values.Should().HaveCount(3);
		result.Value.Values.Should().BeEquivalentTo(expectedValues);

		// Cleanup
		File.Delete(testFile);
	}

	[Fact]
	public void ReadDataFromExcel_SkipsEmptyRows()
	{
		// Arrange
		var testFile = Path.Combine(_testFolder, "test.xlsx");
		var valuesWithEmpties = new[] { "Store1", "", "Store2", null, "Store3" };
		CreateTestExcelFile(testFile, "Sheet1", "Store #", valuesWithEmpties);

		var config = new ExcelReaderConfiguration
		{
			InputFolder = _testFolder,
			WorksheetName = "Sheet1",
			ColumnName = "Store #"
		};
		var reader = new ExcelDataReader(_logger, config);

		// Act
		var result = reader.ReadDataFromExcel();

		// Assert
		result.Should().NotBeNull();
		result.Value.Values.Should().HaveCount(3);
		result.Value.Values.Should().BeEquivalentTo(new[] { "Store1", "Store2", "Store3" });

		// Cleanup
		File.Delete(testFile);
	}

	private void CreateTestExcelFile(string filePath, string worksheetName, string columnName, string[] values)
	{
		OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("CK-NA");

		using var package = new OfficeOpenXml.ExcelPackage();
		var worksheet = package.Workbook.Worksheets.Add(worksheetName);

		// Add header
		worksheet.Cells[1, 1].Value = columnName;

		// Add data
		for (int i = 0; i < values.Length; i++)
		{
			worksheet.Cells[i + 2, 1].Value = values[i];
		}

		package.SaveAs(new FileInfo(filePath));
	}

	public void Dispose()
	{
		if (Directory.Exists(_testFolder))
		{
			Directory.Delete(_testFolder, true);
		}
	}
}
