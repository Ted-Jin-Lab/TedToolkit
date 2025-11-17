namespace TedToolkit.Quantities.Data;

public readonly record struct DataCollection(
    IReadOnlyDictionary<string, Quantity> Quantities,
    IReadOnlyDictionary<string, Unit> Units,
    IReadOnlyDictionary<string, Dimension> Dimensions);