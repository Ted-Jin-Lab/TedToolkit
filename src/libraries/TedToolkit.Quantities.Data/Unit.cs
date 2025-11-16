using Newtonsoft.Json;
using PeterO.Numbers;

namespace TedToolkit.Quantities.Data;

public readonly record struct Unit(
    string Key,
    string Name,
    string Description,
    IReadOnlyList<Link> Links,
    string Symbol,
    Dictionary<string, string> Labels,
    string Multiplier,
    string Offset,
    IReadOnlyList<FactorUnit> FactorUnits,
    int ApplicableSystem)
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

    [JsonIgnore] public Conversion Conversion => new(EDecimal.FromString(Multiplier), EDecimal.FromString(Offset));
    [JsonIgnore]
    public double DistanceToDefault
    {
        get
        {
            var result = 0.0;
            if (!string.IsNullOrEmpty(Multiplier))
            {
                if (double.TryParse(Multiplier, out var value))
                {
                    result += Math.Abs(value - 1);
                }
                else
                {
                    return double.MaxValue;
                }
            }

            if (!string.IsNullOrEmpty(Offset))
            {
                if (double.TryParse(Offset, out var value))
                {
                    result += Math.Abs(value);
                }
                else
                {
                    return double.MaxValue;
                }
            }

            return result - ApplicableSystem;
        }
    }
}