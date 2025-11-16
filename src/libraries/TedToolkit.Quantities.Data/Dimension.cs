namespace TedToolkit.Quantities.Data;

public readonly record struct Dimension(
    int AmountOfSubstance,
    int ElectricCurrent,
    int Length,
    int LuminousIntensity,
    int Mass,
    int ThermodynamicTemperature,
    int Time,
    int Dimensionless)
{
    public int GetExponent(string key) => key switch
    {
        nameof(AmountOfSubstance) => AmountOfSubstance,
        nameof(ElectricCurrent) => ElectricCurrent,
        nameof(Length) => Length,
        nameof(LuminousIntensity) => LuminousIntensity,
        nameof(Mass) => Mass,
        nameof(ThermodynamicTemperature) => ThermodynamicTemperature,
        nameof(Time) => Time,
        nameof(Dimensionless) => Dimensionless,
        _ => 0,
    };
    
    public static Dimension operator *(int left, Dimension right) => new(
        left * right.AmountOfSubstance,
        left * right.ElectricCurrent,
        left * right.Length,
        left * right.LuminousIntensity,
        left * right.Mass,
        left * right.ThermodynamicTemperature,
        left * right.Time,
        left * right.Dimensionless);

    public static Dimension operator *(Dimension left, int right) => right * left;
}