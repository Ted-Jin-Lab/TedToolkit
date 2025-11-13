namespace TedToolkit.Units.Json;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public struct Unit()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    public BaseUnits BaseUnits { get; set; } = new();
    public string FromBaseToUnitFunc { get; set; } = string.Empty;
    public string FromUnitToBaseFunc { get; set; }= string.Empty;
    public Localization[] Localization { get; set; } = [];
    public string PluralName { get; set; }= string.Empty;
    public Prefix[] Prefixes { get; set; } = [];
    public string SingularName { get; set; } = null!;
    public string XmlDocRemarks { get; set; }= string.Empty;
    public string XmlDocSummary { get; set; } = null!;
    public string? ObsoleteText { get; set; }= string.Empty;
    public bool SkipConversionGeneration { get; set; } = false;
    public bool AllowAbbreviationLookup { get; set; } = true;
}