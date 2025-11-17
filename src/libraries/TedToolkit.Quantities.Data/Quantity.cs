using Newtonsoft.Json;

namespace TedToolkit.Quantities.Data;

public readonly record struct Quantity(
    string Name,
    string Description,
    IReadOnlyList<Link> Links,
    bool IsBasic,
    string Dimension,
    bool IsDimensionDefault,
    IReadOnlyList<string> Units)
{
    public string Denominator { get; init; }
    public string Numerator { get; init; }
    public string ExactMatch { get; init; }
    
    [JsonIgnore]
    public string UnitName => Name + "Unit";

    [JsonIgnore] public bool IsNoDimensions => Dimension.Contains("A0E0L0I0M0H0T0");
}