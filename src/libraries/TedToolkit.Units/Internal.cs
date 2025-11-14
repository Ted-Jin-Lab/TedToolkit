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
            sb.Append(Superscripts.TryGetValue(c, out var i) ? i : c);
        }

        return sb.ToString();
    }

    public static string GetUnitString(
        int index, IFormatProvider? formatProvider,
        LengthUnit length, int lengthCount,
        MassUnit mass, int massCount,
        DurationUnit time, int timeCount,
        ElectricCurrentUnit current, int currentCount,
        TemperatureUnit temperature, int temperatureCount,
        AmountOfSubstanceUnit amount, int amountCount,
        LuminousIntensityUnit luminousIntensity, int luminousIntensityCount)
    {
        var names = new List<string>(7);
        AddOne(lengthCount, length.ToString(index, formatProvider));
        AddOne(massCount, mass.ToString(index, formatProvider));
        AddOne(timeCount, time.ToString(index, formatProvider));
        AddOne(currentCount, current.ToString(index, formatProvider));
        AddOne(temperatureCount, temperature.ToString(index, formatProvider));
        AddOne(amountCount, amount.ToString(index, formatProvider));
        AddOne(luminousIntensityCount, luminousIntensity.ToString(index, formatProvider));
        return string.Join("·", names);

        void AddOne(int count, string name)
        {
            if (count is 0) return;
            names.Add(name + count.ToSuperscript());
        }
    }

    public static string GetString(int index, params string[] format)
    {
        return format[index % format.Length];
    }

    public static string? GetFormat(string? format, out int index)
    {
        if (string.IsNullOrEmpty(format))
        {
            index = 0;
            return null;
        }

        var splitFormat = format!.Split('|');
        if (!int.TryParse(splitFormat[0], out index)) index = 0;
        return splitFormat[^1];
    }

    public static string GetCulture(IFormatProvider? formatProvider)
    {
        return formatProvider is CultureInfo cultureInfo 
            ? cultureInfo.Name 
            : CultureInfo.CurrentCulture.Name;
    }
}