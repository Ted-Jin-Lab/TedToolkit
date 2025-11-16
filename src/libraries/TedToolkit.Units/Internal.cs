using System.Globalization;
using System.Text;

namespace TedToolkit.Units;

public static class Internal
{
    private static readonly Dictionary<char, char> Superscripts = new()
    {
        ['0'] = '⁰',
        ['1'] = '¹',
        ['2'] = '²',
        ['3'] = '³',
        ['4'] = '⁴',
        ['5'] = '⁵',
        ['6'] = '⁶',
        ['7'] = '⁷',
        ['8'] = '⁸',
        ['9'] = '⁹',
        ['-'] = '⁻'
    };

    private static string ToSuperscript(this int value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        var sb = new StringBuilder(s.Length);
        foreach (var c in s)
        {
#if NET6_0_OR_GREATER
            sb.Append(Superscripts.GetValueOrDefault(c, c));
#else
            sb.Append(Superscripts.TryGetValue(c, out var i) ? i : c);
#endif
        }

        return sb.ToString();
    }

    // public static string GetUnitString(
    //     int index, IFormatProvider? formatProvider,
    //     LengthUnit length, int lengthCount,
    //     MassUnit mass, int massCount,
    //     DurationUnit time, int timeCount,
    //     ElectricCurrentUnit current, int currentCount,
    //     TemperatureUnit temperature, int temperatureCount,
    //     AmountOfSubstanceUnit amount, int amountCount,
    //     LuminousIntensityUnit luminousIntensity, int luminousIntensityCount)
    // {
    //     var names = new List<string>(7);
    //     AddOne(lengthCount, length.ToString(index, formatProvider));
    //     AddOne(massCount, mass.ToString(index, formatProvider));
    //     AddOne(timeCount, time.ToString(index, formatProvider));
    //     AddOne(currentCount, current.ToString(index, formatProvider));
    //     AddOne(temperatureCount, temperature.ToString(index, formatProvider));
    //     AddOne(amountCount, amount.ToString(index, formatProvider));
    //     AddOne(luminousIntensityCount, luminousIntensity.ToString(index, formatProvider));
    //     return string.Join("·", names);
    //
    //     void AddOne(int count, string name)
    //     {
    //         if (count is 0) return;
    //         names.Add(name + count.ToSuperscript());
    //     }
    // }

    public static string GetUnitString(bool isSymbol, IFormatProvider? formatProvider,
        string symbol,
        string defaultLabel, params (string, string)[] labels)
    {
        if (isSymbol) return symbol;
        var culture = GetCulture(formatProvider);
        foreach (var (key, value) in labels)
        {
            if (key == culture) return value;
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

    public static string GetCulture(IFormatProvider? formatProvider)
    {
        return formatProvider is CultureInfo cultureInfo
            ? cultureInfo.TwoLetterISOLanguageName
            : CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }
}