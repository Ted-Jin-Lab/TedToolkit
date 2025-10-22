using TedToolkit.InterpolatedParser.Options;
using TedToolkit.InterpolatedParser.Parsers;

namespace TedToolkit.InterpolatedParser.ParseItems;

/// <summary>
///     The collection parser.
/// </summary>
/// <param name="value"></param>
/// <param name="parser"></param>
/// <param name="separator"></param>
/// <param name="preModify"></param>
/// <typeparam name="TCollection"></typeparam>
/// <typeparam name="TValue"></typeparam>
public sealed class CollectionStringParseItem<TCollection, TValue>(
    in TCollection value,
    IStringParser<TValue> parser,
    string separator,
    PreModifyOptions preModify)
    : ParseItem<TCollection>(value, preModify), IStringParseItem
    where TCollection : ICollection<TValue>, new()
{
    /// <inheritdoc />
    public void Parse(string s, IFormatProvider? provider)
    {
        var item = new TCollection();
        while (true)
        {
            var index = s.IndexOf(separator, StringComparison.CurrentCulture);
            if (index < 0)
            {
                item.Add(parser.Parse(s, provider));
                break;
            }

            item.Add(parser.Parse(s[..index], provider));
            s = s[(index + separator.Length)..];
        }

        SetValue(item);
    }

    /// <inheritdoc />
    public ParseResult TryParse(string s, IFormatProvider? provider)
    {
        var item = new TCollection();
        List<bool> result = [];
        while (true)
        {
            var index = s.IndexOf(separator, StringComparison.CurrentCulture);
            if (index < 0)
            {
                if (parser.TryParse(s, provider, out var resultValue))
                {
                    result.Add(true);
                    item.Add(resultValue);
                }
                else
                {
                    result.Add(false);
                }

                break;
            }
            else
            {
                if (parser.TryParse(s[..index], provider, out var resultValue))
                {
                    result.Add(true);
                    item.Add(resultValue);
                }
                else
                {
                    result.Add(false);
                }
            }

            s = s[(index + separator.Length)..];
        }

        SetValue(item);
        return new ParseResult(result);
    }
}