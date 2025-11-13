using System.Text;

namespace TedToolkit.Units.Json;

public struct PrefixInfo
{
    private const string Russian = "ru-RU";
    private const string Chinese = "zh-CN";

    internal static readonly IReadOnlyDictionary<Prefix, PrefixInfo> Entries = new[]
    {
        // SI prefixes
        new PrefixInfo(Prefix.Yocto, -24, PrefixType.SI, "y", (Chinese, "夭")),
        new PrefixInfo(Prefix.Zepto, -21, PrefixType.SI, "z", (Chinese, "仄")),
        new PrefixInfo(Prefix.Atto, -18, PrefixType.SI, "a", (Russian, "а"), (Chinese, "阿")),
        new PrefixInfo(Prefix.Femto, -15, PrefixType.SI, "f", (Russian, "ф"), (Chinese, "飞")),
        new PrefixInfo(Prefix.Pico, -12, PrefixType.SI, "p", (Russian, "п"), (Chinese, "皮")),
        new PrefixInfo(Prefix.Nano, -9, PrefixType.SI, "n", (Russian, "н"), (Chinese, "纳")),
        new PrefixInfo(Prefix.Micro, -6, PrefixType.SI, "µ", (Russian, "мк"), (Chinese, "微")),
        new PrefixInfo(Prefix.Milli, -3, PrefixType.SI, "m", (Russian, "м"), (Chinese, "毫")),
        new PrefixInfo(Prefix.Centi, -2, PrefixType.SI, "c", (Russian, "с"), (Chinese, "厘")),
        new PrefixInfo(Prefix.Deci, -1, PrefixType.SI, "d", (Russian, "д"), (Chinese, "分")),
        new PrefixInfo(Prefix.Deca, 1, PrefixType.SI, "da", (Russian, "да"), (Chinese, "十")),
        new PrefixInfo(Prefix.Hecto, 2, PrefixType.SI, "h", (Russian, "г"), (Chinese, "百")),
        new PrefixInfo(Prefix.Kilo, 3, PrefixType.SI, "k", (Russian, "к"), (Chinese, "千")),
        new PrefixInfo(Prefix.Mega, 6, PrefixType.SI, "M", (Russian, "М"), (Chinese, "兆")),
        new PrefixInfo(Prefix.Giga, 9, PrefixType.SI, "G", (Russian, "Г"), (Chinese, "吉")),
        new PrefixInfo(Prefix.Tera, 12, PrefixType.SI, "T", (Russian, "Т"), (Chinese, "太")),
        new PrefixInfo(Prefix.Peta, 15, PrefixType.SI, "P", (Russian, "П"), (Chinese, "拍")),
        new PrefixInfo(Prefix.Exa, 18, PrefixType.SI, "E", (Russian, "Э"), (Chinese, "艾")),
        new PrefixInfo(Prefix.Zetta, 21, PrefixType.SI, "Z", (Chinese, "泽")),
        new PrefixInfo(Prefix.Yotta, 24, PrefixType.SI, "Y", (Chinese, "尧")),

        // Binary prefixes
        new PrefixInfo(Prefix.Kibi, 1, PrefixType.Binary, "Ki"),
        new PrefixInfo(Prefix.Mebi, 2, PrefixType.Binary, "Mi"),
        new PrefixInfo(Prefix.Gibi, 3, PrefixType.Binary, "Gi"),
        new PrefixInfo(Prefix.Tebi, 4, PrefixType.Binary, "Ti"),
        new PrefixInfo(Prefix.Pebi, 5, PrefixType.Binary, "Pi"),
        new PrefixInfo(Prefix.Exbi, 6, PrefixType.Binary, "Ei")
    }.ToDictionary(prefixInfo => prefixInfo.Prefix);

    private PrefixInfo(Prefix prefix, int factor, PrefixType type, string siPrefix,
        params (string cultureName, string prefix)[] cultureToPrefix)
    {
        Prefix = prefix;
        SiPrefix = siPrefix;
        CultureToPrefix = cultureToPrefix.ToDictionary(i => i.cultureName, i => i.prefix);
        Factor = factor;
        Type = type;
    }

    /// <summary>
    ///     The unit prefix.
    /// </summary>
    public Prefix Prefix { get; }

    /// <summary>
    ///     C# expression for the multiplier to prefix the conversion function.
    /// </summary>
    /// <example>Kilo has "1e3" in order to multiply by 1000.</example>
    public int Factor { get; }

    public PrefixType Type { get; }

    /// <summary>
    ///     The unit prefix abbreviation, such as "k" for kilo or "m" for milli.
    /// </summary>
    private string SiPrefix { get; }

    public string BaseToUnit
    {
        get
        {
            switch (Type)
            {
                case PrefixType.SI:
                    return $"{{x}} {(Factor > 0 ? "*" : "/")} 1{new string('0', Math.Abs(Factor))}";
                case PrefixType.Binary:
                    var builder = new StringBuilder().Append("{x}");
                    for (var i = 0; i < Factor; i++)
                    {
                        builder.Append(" * 1024");
                    }
                    return builder.ToString();
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public string UnitToBase 
    {
        get
        {
            switch (Type)
            {
                case PrefixType.SI:
                    return $"{{x}} {(Factor > 0 ? "/" : "*")} 1{new string('0', Math.Abs(Factor))}";
                case PrefixType.Binary:
                    var builder = new StringBuilder().Append("{x}");
                    for (var i = 0; i < Factor; i++)
                    {
                        builder.Append(" / 1024");
                    }
                    return builder.ToString();
                default:
                    throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    ///     Mapping from culture name to localized prefix abbreviation.
    /// </summary>
    private Dictionary<string, string> CultureToPrefix { get; }
}

public enum PrefixType : byte
{
    SI,
    Binary,
}