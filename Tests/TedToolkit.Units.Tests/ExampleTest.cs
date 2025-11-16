using System.Globalization;

namespace TedToolkit.Units.Tests;

public class ExampleTest
{
    [Test]
    public async Task DummyTest()
    {
        var meter = Length.FromMeters(5);

        var a = meter.ToString();
        var c = meter.As(LengthUnit.Centimeter);
        var b = meter.ToString(LengthUnit.Meter);
        var res = meter.As(LengthUnit.Meter);
        await Assert.That(meter.As(LengthUnit.Meter)).IsEqualTo(5);
    }
    
    [Test]
    public async Task CenterMeterTest()
    {
        var meter = Length.FromMeters(5);
        await Assert.That((int)meter.As(LengthUnit.Centimeter)).IsEqualTo(500);
    }
}