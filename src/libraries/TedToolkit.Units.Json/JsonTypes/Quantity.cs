using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TedToolkit.Units.Json;

public struct Quantity()
{
    private static Quantity[]? _quantities;

    private static async Task<Quantity[]> GetQuantities()
    {
        if (_quantities is not null) return _quantities;

        var regex = new Regex(@"TedToolkit\.Units\.Json\.Json\..*\.json");
        var settings = new JsonSerializerSettings
        {
            Converters =
            {
                new StringEnumConverter(namingStrategy: new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy(),
                    allowIntegerValues: false)
            }
        };

        var assembly = typeof(Quantity).Assembly;

        var quantities = await Task.WhenAll(
            assembly.GetManifestResourceNames()
                .Where(name => regex.IsMatch(name))
                .Select(async manifestResourceName =>
                {
#if NET6_0_OR_GREATER
                    await
#endif
                    using var stream = assembly.GetManifestResourceStream(manifestResourceName);
                    using var reader = new StreamReader(stream!);
                    var str = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<Quantity>(str, settings);
                })
        );

        var deltaEntities = quantities.Where(i => i.IsAffine).Select(i => i.AffineOffsetType).ToArray();

        return _quantities = quantities.Where(q => !deltaEntities.Contains(q.Name)).ToArray();
    }

    public static Task<Quantity[]> Quantities => GetQuantities();

    public bool IsNoDimensions => BaseDimensions == default;
    public BaseDimensions BaseDimensions { get; set; } = new(); // Default to empty
    public string BaseUnit { get; set; } = string.Empty;
    public string AffineOffsetType { get; set; } = string.Empty;
    public bool IsAffine => !string.IsNullOrEmpty(AffineOffsetType);
    public bool IsLogarithmic => bool.Parse(Logarithmic);
    public string Logarithmic { get; set; } = string.Empty;
    public int LogarithmicScalingFactorValue => int.Parse(LogarithmicScalingFactor);
    public string LogarithmicScalingFactor { get; set; } = "1";
    public string Name { get; set; } = string.Empty;
    public string UnitName => Name + "Unit";
    public Unit[] Units { get; set; } = [];
    public string XmlDocRemarks { get; set; } = string.Empty;
    public string XmlDocSummary { get; set; } = null!;
    public string ObsoleteText { get; set; } = string.Empty;

    public IEnumerable<UnitInfo> UnitsInfos => Units.SelectMany(u => (IEnumerable<UnitInfo>)
    [
        new UnitInfo(u, Prefix.None),
        ..u.Prefixes.OrderBy(p => p).Select(p => new UnitInfo(u, p))
    ]);
}