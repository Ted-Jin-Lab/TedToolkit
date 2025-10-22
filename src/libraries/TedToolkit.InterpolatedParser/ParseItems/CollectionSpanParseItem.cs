#if NETCOREAPP
using TedToolkit.InterpolatedParser.Options;
using TedToolkit.InterpolatedParser.Parsers;

namespace TedToolkit.InterpolatedParser.ParseItems;

/// <summary>
///     The parse item for collections.
/// </summary>
/// <param name="value"></param>
/// <param name="parser"></param>
/// <param name="separator"></param>
/// <param name="preModify"></param>
/// <typeparam name="TCollection"></typeparam>
/// <typeparam name="TValue"></typeparam>
public sealed class CollectionSpanParseItem<TCollection, TValue>(
    in TCollection value,
    ISpanParser<TValue> parser,
    string separator,
    PreModifyOptions preModify)
    : ParseItem<TCollection>(value, preModify), ISpanParseItem
    where TCollection : ICollection<TValue>, new()
{
    /// <inheritdoc />
    public void Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        var item = new TCollection();
        var separatorSpan = separator.AsSpan();
        while (true)
        {
            var index = s.IndexOf(separatorSpan);
            if (index < 0)
            {
                item.Add(parser.Parse(s, provider));
                break;
            }

            item.Add(parser.Parse(s[..index], provider));
            s = s[(index + separatorSpan.Length)..];
        }

        SetValue(item);
    }

    /// <inheritdoc />
    public ParseResult TryParse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        var item = new TCollection();
        var separatorSpan = separator.AsSpan();
        List<bool> result = [];
        while (true)
        {
            var index = s.IndexOf(separatorSpan);
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

            s = s[(index + separatorSpan.Length)..];
        }

        SetValue(item);
        return new ParseResult(result);
    }
}
#endif