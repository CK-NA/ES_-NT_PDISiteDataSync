# PDI_SiteDataSync.Tests

Unit test suite for the PDI Site Data Sync application.

## Test Framework

- **xUnit** - Testing framework
- **Moq** - Mocking framework for dependencies
- **FluentAssertions** - Fluent assertion library for better test readability

## Test Coverage

### ExcelDataReaderTests
Tests for Excel file reading and data extraction:
- ✅ No file scenario
- ✅ Invalid worksheet handling
- ✅ Invalid column handling
- ✅ Valid data extraction
- ✅ Empty row filtering

### ArchiveManagerTests
Tests for file archiving functionality with **mocked NLog flush**:
- ✅ Data file archiving with timestamp
- ✅ Log file archiving (mocked FlushLogs)
- ✅ Combined data and log archiving (verifies FlushLogs called)
- ✅ Logs-only archiving (no input file scenario, verifies FlushLogs called twice)
- ✅ Original file deletion after archiving

**Note:** `ILoggerFactory.FlushLogs()` is mocked to prevent real `LogManager.Flush()` blocking operations.

### LogHelperTests
Tests for dual logging (logger + console):
- ✅ Info level logging
- ✅ Error level logging
- ✅ Warning level logging
- ✅ Fatal level logging
- ✅ Parameter formatting
- ✅ Exception logging

### ProgramInitializerTests
Tests for application initialization with **mocked dependencies**:
- ✅ Configuration loading (file system and logger mocked)
- ✅ Folder creation (verifies mocked CreateDirectory calls)
- ✅ Missing configuration handling (validates error handling)

**Note:** All dependencies (`IFileSystemService`, `ILoggerFactory`) are mocked to prevent real I/O and NLog initialization.

### ApplicationProcessTests
Tests for main business workflow with **fully mocked dependencies**:
- ✅ No input file scenario (verifies ArchiveLogsOnly called, no DB calls)
- ✅ Valid Excel file processing (verifies all mocked operations called in order)
- ✅ Invalid worksheet handling (verifies early return, no DB calls)

**Note:** All dependencies (ExcelReader, ArchiveManager, DatabaseServices) are mocked using Moq to create true isolated unit tests.

## Running Tests

### Visual Studio
1. Open Test Explorer (Test → Test Explorer)
2. Click "Run All" to execute all tests

### Command Line
```powershell
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with coverage (requires coverlet)
dotnet test /p:CollectCoverage=true
```

## Test Structure

Each test follows the **Arrange-Act-Assert** (AAA) pattern:

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - Set up test data and mocks
    var mockLogger = new Mock<Logger>();

    // Act - Execute the method being tested
    var result = method.Execute();

    // Assert - Verify the outcome
    result.Should().NotBeNull();
}
```

## Mocking Strategy

- **Logger**: Mocked using Moq to verify logging calls without actual log output
- **File System**: Uses temporary folders that are cleaned up after each test
- **Database**: Tests that require database connections will fail (integration tests needed)

## Notes

### Complete Mocking Strategy
All external dependencies are mocked to ensure fast, isolated unit tests:
- **Logger**: Mocked using Moq (NLog Logger class)
- **IExcelDataReader**: Mocked to simulate file reading without actual I/O
- **IArchiveManager**: Mocked to prevent zip file creation and LogManager.Flush() blocking
- **IDatabaseService**: Mocked to prevent database connections
- **IFileSystemService**: Mocked to prevent real directory creation/checking
- **ILoggerFactory**: Mocked to prevent real NLog initialization

### Why Tests Are Now Fast
The original test suite was hanging because the code created real instances of:
1. **ExcelDataReader** - Real file I/O operations
2. **ArchiveManager** - `LogManager.Flush()` blocking + real zip file creation
3. **DatabaseService** - 30+ second SQL connection timeouts
4. **ProgramInitializer** - Real NLog setup via `LogManager.GetCurrentClassLogger()`
5. **FileSystem** - Real directory creation operations

**Complete refactoring solution:**
1. Created interfaces for all external dependencies:
   - `IExcelDataReader`, `IArchiveManager`, `IDatabaseService`
   - `IFileSystemService`, `ILoggerFactory`
2. Updated concrete classes to implement interfaces
3. Refactored all classes to use dependency injection
4. Updated `Program.cs` to create and inject real implementations
5. Tests inject fully mocked dependencies using Moq
6. **No real I/O, logging, database, or file system operations occur during tests**

### Integration Tests Recommended
For full end-to-end testing, create a separate integration test project that:
- Uses real `DatabaseService` instances
- Connects to test databases
- Has longer timeouts
- Runs separately from unit tests
- Uses real connection strings

### Temporary Files
Tests create temporary files and folders that are automatically cleaned up in the `Dispose()` method.

### EPPlus License
Tests set `ExcelPackage.LicenseContext = LicenseContext.NonCommercial` for Excel operations.

## Future Improvements

- Add integration tests for database operations
- Add code coverage reporting
- Add performance/load tests
- Mock file system operations using abstractions
- Add tests for configuration validation edge cases
