#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace TedToolkit.InterpolatedParser.Parsers;

/// <summary>
///     For the span parsable items.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SpanParseableParser<T> : SpanParser<T> where T : ISpanParsable<T>
{
    /// <inheritdoc />
    public sealed override T Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return T.Parse(s, provider);
    }

    /// <inheritdoc />
    public sealed override bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider,
        [MaybeNullWhen(false)] out T result)
    {
        return T.TryParse(s, provider, out result);
    }
}
#endif