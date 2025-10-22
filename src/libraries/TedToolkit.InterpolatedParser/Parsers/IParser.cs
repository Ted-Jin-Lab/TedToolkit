namespace TedToolkit.InterpolatedParser.Parsers;

/// <summary>
///     This should be one of <see cref="ISpanParser{T}" /> or <see cref="IStringParser{T}" />
///     You'd better create <see cref="SpanParser{T}" /> or <see cref="StringParser{T}" />
/// </summary>
public interface IParser
{
    /// <summary>
    ///     The format string
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    ///     The target type to parse.
    /// </summary>
    public Type TargetType { get; }
}