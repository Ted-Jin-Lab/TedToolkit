namespace TedToolkit.Units;

/// <summary>
/// Generate the Data.
/// </summary>
/// <typeparam name="TData"></typeparam>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class UnitsAttribute<TData>(
    LengthUnit length = LengthUnit.Meter,
    MassUnit mass = MassUnit.Kilogram,
    DurationUnit time = DurationUnit.Second,
    ElectricCurrentUnit current = ElectricCurrentUnit.Ampere,
    TemperatureUnit temperature = TemperatureUnit.Kelvin,
    AmountOfSubstanceUnit amount = AmountOfSubstanceUnit.Mole,
    LuminousIntensityUnit luminousIntensity = LuminousIntensityUnit.Candela) : Attribute 
    where TData : struct;