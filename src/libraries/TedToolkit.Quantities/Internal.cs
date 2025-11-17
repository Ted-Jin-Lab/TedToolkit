using System.Globalization;
using System.Text;

namespace TedToolkit.Quantities;

public static class Internal
{
    public static string GetUnitString(bool isSymbol, IFormatProvider? formatProvider,
        string symbol,
        string defaultLabel, params (string, string)[] labels)
    {
        if (isSymbol) return symbol;
        var culture = formatProvider is CultureInfo cultureInfo ? cultureInfo : CultureInfo.CurrentCulture;
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

    public static string? GetFormat(string? format, out bool isSymbol)
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