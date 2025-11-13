namespace TedToolkit.Units;

/// <summary>
/// Generate the Data.
/// </summary>
/// <typeparam name="TData"></typeparam>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class UnitsAttribute<TData>(
    LengthUnit length = LengthUnit.Meter,
    MassUnit mass = MassUnit.KiloGram,
    DurationUnit time = DurationUnit.Second,
    ElectricCurrentUnit current = ElectricCurrentUnit.Ampere,
    TemperatureUnit temperature = TemperatureUnit.Kelvin,
    AmountOfSubstanceUnit amount = AmountOfSubstanceUnit.Mole,
    LuminousIntensityUnit luminousIntensity = LuminousIntensityUnit.Candela,
    Access access = Access.Public) : Attribute
    where TData : struct,
#if NET8_0_OR_GREATER
    System.Numerics.INumber<TData>;
#else
    IConvertible;
#endif