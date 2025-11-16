namespace TedToolkit.Units.Data;

public readonly record struct Unit(
    string Key,
    string Name,
    string Description,
    IReadOnlyList<Link> Links,
    string Symbol,
    Dictionary<string, string> Labels,
    string Multiplier,
    string Offset,
    IReadOnlyList<FactorUnit> FactorUnits)
{
    public string GetUnitName(IEnumerable<Unit> allUnits)
    {
        var name = Name;
        return MakeSafe(allUnits.Count(u => u.Name == name) is not 1
            ? name + "_" + Key
            : name);

        static string MakeSafe(string value)
        {
            return value
                .Replace("(", "_")
                .Replace("-", "_")
                .Replace(")", "_")
                .Replace(",", "")
                .Replace(".", "")
                .Replace("°", "");
        }
    }
}