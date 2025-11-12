namespace TedToolkit.Units.Generator.JsonTypes;

internal struct Quantity()
{
    public BaseDimensions BaseDimensions { get; set; } = new(); // Default to empty
    public string BaseUnit { get; set; } = string.Empty;
    public string AffineOffsetType { get; set; } = string.Empty;
    public bool IsAffine => !string.IsNullOrEmpty(AffineOffsetType);
    public bool IsLogarithmic => bool.Parse(Logarithmic);
    public string Logarithmic { get; set; } = string.Empty;
    public int LogarithmicScalingFactorValue => int.Parse(LogarithmicScalingFactor);
    public string LogarithmicScalingFactor { get; set; } = "1";
    public string Name { get; set; } = string.Empty;
    public Unit[] Units { get; set; } = [];
    public string XmlDocRemarks { get; set; } = string.Empty;
    public string XmlDocSummary { get; set; } = null!;
    public string ObsoleteText { get; set; } = string.Empty;
}