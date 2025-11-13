namespace TedToolkit.Units.Json;

/// <summary>
/// 
/// </summary>
/// <param name="N">AmountOfSubstance</param>
/// <param name="I">ElectricCurrent</param>
/// <param name="L">Length</param>
/// <param name="J">LuminousIntensity</param>
/// <param name="M">Mass</param>
/// <param name="Θ">Temperature</param>
/// <param name="T">Time</param>
public readonly record struct BaseDimensions(int N, int I, int L, int J, int M, int Θ, int T);