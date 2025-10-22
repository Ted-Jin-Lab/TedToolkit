using System.Diagnostics.CodeAnalysis;

namespace TedToolkit.InterpolatedParser.Parsers;

/// <summary>
///     The string parser
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IStringParser<T> : IParser
{
    /// <summary>
    ///     Parse it.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="provider"></param>
    /// <returns></returns>
    T Parse(string s, IFormatProvider? provider);

    /// <summary>
    ///     Try parse it.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="provider"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    bool TryParse(string s, IFormatProvider? provider, [MaybeNullWhen(false)] out T result);
}