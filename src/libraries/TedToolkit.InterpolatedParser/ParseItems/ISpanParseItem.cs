#if NETCOREAPP

namespace TedToolkit.InterpolatedParser.ParseItems;

/// <summary>
///     The span parse item.
/// </summary>
public interface ISpanParseItem : IParseItem
{
    /// <summary>
    ///     Parse it.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="provider"></param>
    void Parse(ReadOnlySpan<char> s, IFormatProvider? provider);

    /// <summary>
    ///     Try parse it.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="provider"></param>
    /// <returns></returns>
    ParseResult TryParse(ReadOnlySpan<char> s, IFormatProvider? provider);
}
#endif