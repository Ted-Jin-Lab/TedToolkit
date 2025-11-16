using TedToolkit.Units.Data;

namespace TedToolkit.Units.Analyzer;

/// <summary>
/// 
/// </summary>
public readonly struct UnitSystem(Dictionary<string, string> unitDictionary, DataCollection collection)
{
    public IReadOnlyList<string> Keys { get; } = unitDictionary.Keys.ToList();
    
    public Quantity GetQuantity(string key) => collection.Quantities.First(q => q.Name == key);

    private Unit GetUnit(string key)
    {
        var units = collection.Units.Values.ToArray();
        var unitKey = unitDictionary[key];
        return units.First(q => q.GetUnitName(units) == unitKey);
    }
}