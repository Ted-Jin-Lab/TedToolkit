using TedToolkit.Units.Json;

namespace TedToolkit.Units.Analyzer;

/// <summary>
/// 
/// </summary>
public readonly record struct UnitSystem
{
    public UnitInfo AmountOfSubstance { get; }
    public UnitInfo ElectricCurrent { get; }
    public UnitInfo Length { get; }
    public UnitInfo LuminousIntensity { get; }
    public UnitInfo Mass { get; }
    public UnitInfo Temperature { get; }
    public UnitInfo Time { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="n">AmountOfSubstance</param>
    /// <param name="i">ElectricCurrent</param>
    /// <param name="l">Length</param>
    /// <param name="j">LuminousIntensity</param>
    /// <param name="m">Mass</param>
    /// <param name="θ">Temperature</param>
    /// <param name="t">Time</param>
    /// <param name="quantities"></param>
    public UnitSystem(byte n, byte i, byte l, byte j, byte m, byte θ, byte t, Quantity[] quantities)
    {
        AmountOfSubstance = GetUnitInfo(quantities, "AmountOfSubstance", n);
        ElectricCurrent = GetUnitInfo(quantities, "ElectricCurrent", i);
        Length = GetUnitInfo(quantities, "Length", l);
        LuminousIntensity = GetUnitInfo(quantities, "LuminousIntensity", j);
        Mass = GetUnitInfo(quantities, "Mass", m);
        Temperature = GetUnitInfo(quantities, "Temperature", θ);
        Time = GetUnitInfo(quantities, "Duration", t);
    }

    private static UnitInfo GetUnitInfo(Quantity[] quantities, string unit, byte info)
    {
        return quantities.First(q => q.Name == unit).UnitsInfos.ElementAt(info - 1);
    }
}