using System.Globalization;
using System.Text;

namespace TedToolkit.Units.Json;

internal static class Helpers
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

    public static string ToSuperscript(this int value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        var sb = new StringBuilder(s.Length);
        foreach (var c in s)
        {
            sb.Append(Superscripts.GetValueOrDefault(c, c));
        }
        return sb.ToString();
    }
}