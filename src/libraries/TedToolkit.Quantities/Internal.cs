using System.Globalization;
using System.Text;

namespace TedToolkit.Quantities;

/// <summary>
/// Some internal methods.
/// </summary>
public static class Internal
{
    /// <summary>
    /// Easy to compare.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="other"></param>
    /// <typeparam name="TQuantity"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static int CompareTo<TQuantity>(TQuantity self, object? other) where TQuantity : IComparable<TQuantity>
    {
        return other switch
        {
            null => 1,
            TQuantity otherQuantity => self.CompareTo(otherQuantity),
            _ => throw new ArgumentException($"Object must be of type {typeof(TQuantity)}")
        };
    }

    /// <summary>
    /// Get the unit string.
    /// </summary>
    /// <param name="isSymbol"></param>
    /// <param name="formatProvider"></param>
    /// <param name="symbol"></param>
    /// <param name="defaultLabel"></param>
    /// <param name="labels"></param>
    /// <returns></returns>
    public static string GetUnitString(bool isSymbol, IFormatProvider? formatProvider,
        string symbol,
        string defaultLabel, params (string, string)[] labels)
    {
        if (isSymbol) return symbol;
        var culture = formatProvider as CultureInfo ?? CultureInfo.CurrentCulture;
        var letter = culture.Name.ToLower();
        foreach (var (key, value) in labels)
        {
            if (key == letter) return value;
        }

        letter = culture.TwoLetterISOLanguageName.ToLower();
        foreach (var (key, value) in labels)
        {
            if (key == letter) return value;
        }

        return defaultLabel;
    }

    /// <summary>
    /// Parse the format.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="isSymbol"></param>
    /// <returns></returns>
    public static string? ParseFormat(string? format, out bool isSymbol)
    {
        if (string.IsNullOrEmpty(format))
        {
            isSymbol = true;
            return null;
        }

        var splitFormat = format!.Split('|');
        isSymbol = splitFormat[0].Contains('s') || splitFormat[0].Contains('S'); //TODO: L for label?
        return splitFormat.First();
    }
}