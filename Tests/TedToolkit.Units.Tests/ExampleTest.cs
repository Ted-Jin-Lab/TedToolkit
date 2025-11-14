namespace TedToolkit.Units.Tests;

public class ExampleTest
{
    [Test]
    public async Task DummyTest()
    {
        var meter = Length.FromMeters(5);
        await Assert.That(meter.As(LengthUnit.Meter)).IsEqualTo(5);
    }
}