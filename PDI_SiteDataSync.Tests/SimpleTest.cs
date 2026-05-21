using Xunit;

namespace PDI_SiteDataSync.Tests;

public class SimpleTest
{
    [Fact]
    public void SimpleTest_ShouldPass()
    {
        // Arrange & Act & Assert
        var result = 1 + 1;
        Assert.Equal(2, result);
    }
}
