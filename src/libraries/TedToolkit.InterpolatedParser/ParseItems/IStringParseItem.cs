namespace TedToolkit.InterpolatedParser.ParseItems;

/// <summary>
///     The item to parse the string.
/// </summary>
public interface IStringParseItem : IParseItem
{
    /// <summary>
    ///     Parse it.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="provider"></param>
    void Parse(string s, IFormatProvider? provider);

    /// <summary>
    ///     Try to parse it.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="provider"></param>
    /// <returns></returns>
    ParseResult TryParse(string s, IFormatProvider? provider);
}