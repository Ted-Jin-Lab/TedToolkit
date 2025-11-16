using Newtonsoft.Json;

namespace TedToolkit.Units.Data;

public readonly record struct Quantity(
    string Name,
    string Description,
    IReadOnlyList<Link> Links,
    bool IsBasic,
    Dimension Dimension,
    bool IsDimensionDefault,
    IReadOnlyList<string> Units)
{
    [JsonIgnore]
    public string UnitName => Name + "Unit";

    public bool IsNoDimensions => Dimension.Dimensionless is not 0;
}