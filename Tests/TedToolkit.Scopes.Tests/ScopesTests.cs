namespace TedToolkit.Scopes.Tests;

public class ScopesTests
{
    [Test]
    [MatrixDataSource]
    public async Task ScopeCreateTest([MatrixRange<int>(-1000, 1000)]int value)
    {
        using var scope = new TestScope(value);
        await Task.Delay(1000);
        await Assert.That(TestScope.Current?.Value).IsEqualTo(value);
    }
}