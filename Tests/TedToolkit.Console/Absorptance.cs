namespace TedToolkit.Quantities;

[QuantityDisplayUnit<AbsorbedDoseUnit>(AbsorbedDoseUnit.Centigray)]
[QuantityImplicitFromValueType]
[QuantityImplicitToValueType]
[QuantityOperator<AbsorbedDose, AbsorbedDose, double>(Operator.Divide)]
partial struct AbsorbedDose
{
    public static void Test()
    {
        var a = new AbsorbedDose(10, AbsorbedDoseUnit.Centigray);
        var b = new AbsorbedDose(10, AbsorbedDoseUnit.Centigray);
        var c = a / b;
        double d = a;
        AbsorbedDose e = 10.0;
        
        var list = new List<AbsorbedDose> {a, b};
        var f = list.Sum();
    }
}

[QuantityDisplayUnit<DimensionlessUnit>(DimensionlessUnit.Number)]
partial struct Dimensionless
{
    
}