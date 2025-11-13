namespace TedToolkit.Units.Tests;

public class ExampleTest
{
    [Test]
    public async Task DummyTest()
    {
        var meter = new Length(5, LengthUnit.Meter);
        await Assert.That(meter.As(LengthUnit.Meter)).IsEqualTo(5);
    }
}