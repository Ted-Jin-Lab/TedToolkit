#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace TedToolkit.InterpolatedParser.Parsers;

/// <summary>
///     For the parsable items.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class StringParsableParser<T> : StringParser<T> where T : IParsable<T>
{
    /// <inheritdoc />
    public sealed override T Parse(string s, IFormatProvider? provider)
    {
        return T.Parse(s, provider);
    }

    /// <inheritdoc />
    public sealed override bool TryParse(string s, IFormatProvider? provider, [MaybeNullWhen(false)] out T result)
    {
        return T.TryParse(s, provider, out result);
    }
}
#endif