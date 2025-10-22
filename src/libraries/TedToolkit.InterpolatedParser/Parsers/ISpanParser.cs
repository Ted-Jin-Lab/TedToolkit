#if NETCOREAPP
using System.Diagnostics.CodeAnalysis;

namespace TedToolkit.InterpolatedParser.Parsers;

/// <summary>
///     The span parser.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISpanParser<T> : IParser
{
    /// <summary>
    ///     Parse it
    /// </summary>
    /// <param name="s"></param>
    /// <param name="provider"></param>
    /// <returns></returns>
    T Parse(ReadOnlySpan<char> s, IFormatProvider? provider);

    /// <summary>
    ///     Try parse it.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="provider"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out T result);
}
#endif