using TedToolkit.InterpolatedParser.Options;
using TedToolkit.InterpolatedParser.Parsers;

namespace TedToolkit.InterpolatedParser.ParseItems;

/// <summary>
///     Parse by the string.
/// </summary>
/// <param name="value"></param>
/// <param name="index"></param>
/// <param name="parser"></param>
/// <param name="preModify"></param>
/// <typeparam name="T"></typeparam>
public sealed class StringParseItem<T>(in T value, IStringParser<T> parser, PreModifyOptions preModify)
    : ParseItem<T>(value, preModify), IStringParseItem
{
    /// <inheritdoc />
    public void Parse(string s, IFormatProvider? provider)
    {
        SetValue(parser.Parse(s, provider));
    }

    /// <inheritdoc />
    public ParseResult TryParse(string s, IFormatProvider? provider)
    {
        if (!parser.TryParse(s, provider, out var result)) return false;
        SetValue(result);
        return true;
    }
}