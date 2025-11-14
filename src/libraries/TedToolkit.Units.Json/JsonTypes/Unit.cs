using System.Text.RegularExpressions;

namespace TedToolkit.Units.Json;

public sealed class Unit
{
    public BaseUnits BaseUnits { get; set; } = new();
    public string FromBaseToUnitFunc { get; set; } = string.Empty;
    public string FromUnitToBaseFunc { get; set; } = string.Empty;

    public string BaseToUnit => GetFunc(FromBaseToUnitFunc);
    public string UnitToBase => GetFunc(FromUnitToBaseFunc);

    public Localization[] Localization { get; set; } = [];
    public string PluralName { get; set; } = string.Empty;
    public Prefix[] Prefixes { get; set; } = [];
    public string SingularName { get; set; } = null!;
    public string XmlDocRemarks { get; set; } = string.Empty;
    public string XmlDocSummary { get; set; } = null!;
    public string? ObsoleteText { get; set; } = string.Empty;
    public bool SkipConversionGeneration { get; set; } = false;
    public bool AllowAbbreviationLookup { get; set; } = true;


    private static string GetFunc(string func)
    {
        return RemovePoint(RemoveSci(func
            .Replace("d", "")
            .Replace("Math.", "")));
    }

    private static string RemovePoint(string str)
    {
        return Regex.Replace(str,
            @"\d+(\.\d+)?",
            match =>
            {
                var matchValue = match.Value;
                var index = matchValue.IndexOf('.');
                if (index < 0) return matchValue;
                var count = matchValue.Length - index - 1;
                matchValue = matchValue.Replace(".", "");
                while (matchValue.StartsWith("0", StringComparison.InvariantCulture))
                {
                    matchValue = matchValue[1..];
                }
                return matchValue + " / 1" + new string('0', count);
            });
    }

    public static string RemoveSci(string str)
    {
        return Regex.Replace(str,
            @"[-]?\d+(\.\d+)?[eE][+-]?\d+(\.\d+)?",
            match => SciToString(match.Value));
    }

    private static string SciToString(string sci)
    {
        // 分解符号
        var sign = "";
        if (sci.StartsWith("+") || sci.StartsWith("-"))
        {
            sign = sci[..1];
            sci = sci[1..];
        }

        var eIndex = sci.IndexOfAny(['e', 'E']);
        var mantissa = sci[..eIndex];
        var exponent = (int)double.Parse(sci[(eIndex + 1)..]);

        var intPart = mantissa;
        var fracPart = "";

        var dotIndex = mantissa.IndexOf('.');
        if (dotIndex >= 0)
        {
            intPart = mantissa[..dotIndex];
            fracPart = mantissa[(dotIndex + 1)..];
        }

        if (exponent >= 0)
        {
            if (fracPart.Length <= exponent)
            {
                return sign + intPart + fracPart + new string('0', exponent - fracPart.Length);
            }
            else
            {
                return sign + intPart + fracPart[..exponent] + "." + fracPart[exponent..];
            }
        }
        else
        {
            var negExp = -exponent;
            if (intPart.Length >= negExp)
            {
                return sign + intPart[..^negExp] + "." + intPart[^negExp..] + fracPart;
            }
            else
            {
                return sign + "0." + new string('0', negExp - intPart.Length) + intPart + fracPart;
            }
        }
    }
}

public static class Extensions
{
    public static string SetExpressionValue(this string func, string value)
    {
        return func.Replace("{x}", "(" + value + ")");
    }
}