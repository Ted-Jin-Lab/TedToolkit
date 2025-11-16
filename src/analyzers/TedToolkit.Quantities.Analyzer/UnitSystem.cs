using TedToolkit.Quantities.Data;

namespace TedToolkit.Quantities.Analyzer;

/// <summary>
/// 
/// </summary>
public readonly struct UnitSystem(Dictionary<string, string> unitDictionary, DataCollection collection)
{
    public IReadOnlyList<string> Keys { get; } = unitDictionary.Keys.ToList();

    public Quantity GetQuantity(string key) => collection.Quantities.First(q => q.Name == key);

    public Unit GetUnit(string key)
    {
        var quantity = GetQuantity(key);

        var data = collection;
        var allUnits = data.Units.Values.ToArray();
        var quantityUnits =quantity.Units
            .Select(u => data.Units[u]);
        if (unitDictionary.TryGetValue(key, out var unitKey))
        {
            return quantityUnits.First(q => q.GetUnitName(allUnits) == unitKey);
        }

        return quantityUnits
            .OrderBy(i => i.DistanceToDefault)
            .First();
    }
}