namespace TedToolkit.Quantities.Data;

public readonly record struct DataCollection(
    IReadOnlyList<Quantity> Quantities,
    IReadOnlyDictionary<string, Unit> Units);