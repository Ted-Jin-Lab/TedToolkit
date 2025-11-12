namespace TedToolkit.Units.Generator.JsonTypes;

internal struct BaseDimensions()
{
    /// <summary>AmountOfSubstance.</summary>
    public int N { get; set; } = 0;

    /// <summary>ElectricCurrent.</summary>
    public int I { get; set; } = 0;

    /// <summary>Length.</summary>
    public int L { get; set; } = 0;

    /// <summary>LuminousIntensity.</summary>
    public int J { get; set; } = 0;

    /// <summary>Mass.</summary>
    public int M { get; set; } = 0;

    /// <summary>Temperature.</summary>
    public int Θ { get; set; } = 0;

    /// <summary>Time.</summary>
    public int T { get; set; } = 0;
}