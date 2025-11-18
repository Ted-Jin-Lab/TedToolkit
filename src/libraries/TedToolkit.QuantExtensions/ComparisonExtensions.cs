using UnitsNet;
using UnitsNet.Units;

namespace TedToolkit.QuantExtensions;

public static class ComparisonExtensions
{
    public static T Max<T>(this T self, T other) where T : struct, IComparable<T>
    {
        return self.CompareTo(other) >= 0 ? self : other;
    }
    
    public static T Min<T>(this T self, T other) where T : struct, IComparable<T>
    {
        return self.CompareTo(other) <= 0 ? self : other;
    }
}