namespace CK_NA.Template.ConsoleApp.Tests;

public class ApplicationProcessTests
{
    [Fact]
    public async Task ExecuteAsync_CompletesSuccessfully()
    {
        // Arrange
        var initializer = new ProgramInitializer();
        // Note: Initialize() requires appsettings.json in the working directory.
        // For unit testing, consider mocking or providing a test configuration.

        var process = new ApplicationProcess(initializer);

        // Act
        await process.ExecuteAsync();

        // Assert - no exception means success for the default template
    }
}
