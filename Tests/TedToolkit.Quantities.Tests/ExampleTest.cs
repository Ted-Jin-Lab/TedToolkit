using System.Globalization;
using TedToolkit.Quantities;

namespace TedToolkit.Quantities.Tests;

public class ExampleTest
{
    [Test]
    public async Task DummyTest()
    {
        var meter = Length.FromMetre(5);

        var a = meter.ToString();
        var c = meter.As(LengthUnit.Centimetre);
        var b = meter.ToString(LengthUnit.Metre);
        var res = meter.As(LengthUnit.Metre);
        await Assert.That(meter.As(LengthUnit.Metre)).IsEqualTo(5);
    }
    
    [Test]
    public async Task CenterMeterTest()
    {
        var meter = Length.FromMetre(5);
        await Assert.That((int)meter.As(LengthUnit.Centimetre)).IsEqualTo(500);
    }
}