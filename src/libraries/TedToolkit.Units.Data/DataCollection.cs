namespace TedToolkit.Units.Data;

public readonly record struct DataCollection(
    IReadOnlyList<Quantity> Quantities,
    IReadOnlyDictionary<string, Unit> Units);