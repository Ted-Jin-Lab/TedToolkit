using System.Diagnostics.CodeAnalysis;

namespace TedToolkit.InterpolatedParser.Parsers;

/// <inheritdoc cref="IStringParser{T}" />
public abstract class StringParser<T> : Parser, IStringParser<T>
{
    /// <inheritdoc />
    public sealed override Type TargetType => typeof(T);

    /// <inheritdoc />
    public abstract T Parse(string s, IFormatProvider? provider);

    /// <inheritdoc />
    public abstract bool TryParse(string s, IFormatProvider? provider, [MaybeNullWhen(false)] out T result);
}