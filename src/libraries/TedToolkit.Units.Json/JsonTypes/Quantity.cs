using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace TedToolkit.Units.Json;

public struct Quantity()
{
    public static async Task<Quantity[]> GetQuantitiesAsync(params IEnumerable<string> otherJsonObjects)
    {
        var regex = new Regex(@"TedToolkit\.Units\.Json\..*\.json");
        var assembly = typeof(Quantity).Assembly;
        
        var objects = await Task.WhenAll(
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
                    return JObject.Parse(str);
                })
        );
        
        var serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            Converters =
            {
                new StringEnumConverter(namingStrategy: new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy(),
                    allowIntegerValues: false)
            }
        });
        
        var quantities = objects.Concat(otherJsonObjects.Select(JObject.Parse))
            .GroupBy(i => (string?)i["Name"] ?? "_")
            .Where(i => i.Key is not "_")
            .Select(i =>
            {
                var first = i.First();
                foreach (var jObject in i.Skip(1))
                {
                    first.Merge(jObject, new JsonMergeSettings
                    {
                        MergeNullValueHandling = MergeNullValueHandling.Merge,
                        MergeArrayHandling = MergeArrayHandling.Replace
                    });
                }
                return first;
            })
            .Select(j => j.ToObject<Quantity>(serializer)).ToArray();

        var deltaEntities = quantities.Where(i => i.IsAffine).Select(i => i.AffineOffsetType).ToArray();

        return  quantities.Where(q => !deltaEntities.Contains(q.Name)).ToArray();
    }


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