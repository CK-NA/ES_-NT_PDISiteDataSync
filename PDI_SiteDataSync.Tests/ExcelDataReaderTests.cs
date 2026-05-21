using FluentAssertions;
using Moq;
using NLog;
using PDI_SiteDataSync.Utility;
using Xunit;

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
        var reader = new ExcelDataReader(_logger, _testFolder, "Sheet1", "Column1");

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

        var reader = new ExcelDataReader(_logger, _testFolder, "InvalidSheet", "TestColumn");

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

        var reader = new ExcelDataReader(_logger, _testFolder, "Sheet1", "InvalidColumn");

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

        var reader = new ExcelDataReader(_logger, _testFolder, "Sheet1", "Store #");

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

        var reader = new ExcelDataReader(_logger, _testFolder, "Sheet1", "Store #");

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
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

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
