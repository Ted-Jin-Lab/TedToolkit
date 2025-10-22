using System.Runtime.CompilerServices;
using TedToolkit.InterpolatedParser.Parsers;

namespace TedToolkit.InterpolatedParser.Options;

/// <summary>
///     The option to one specific parameter
/// </summary>
/// <param name="ParameterName">Please use <see langword="nameof" /></param>
/// to get the symbol word
public readonly record struct ParseItemOptions([CallerMemberName] string ParameterName = "")
{
    /// <summary>
    ///     The delegate about what you want to get the string.
    /// </summary>
    public delegate string ToStringDelegate(object? value, string? format);

    /// <summary>
    ///     The default value.
    /// </summary>
    // ReSharper disable once ExplicitCallerInfoArgument
    public static readonly ParseItemOptions Default = new("");

    /// <summary>
    ///     In or Out?
    /// </summary>
    public ParseType ParseType { get; init; }

    /// <summary>
    ///     List or Item?
    /// </summary>
    public DataType DataType { get; init; }

    /// <summary>
    /// </summary>
    public PreModifyOptions? PreModification { get; init; }

    /// <summary>
    ///     Patch at the beginning
    /// </summary>
    public bool Beginning { get; init; }

    /// <summary>
    ///     Your custom parser?
    /// </summary>
    public IParser? Parser { get; init; }

    /// <summary>
    ///     Your separator for the case you set <see cref="DataType" /> to <see cref="DataType.List" />.
    /// </summary>
    public string Separator { get; init; } = ",";

    /// <summary>
    ///     Your custom way to make it as string.
    /// </summary>
    public ToStringDelegate? CustomToString { get; init; }

    /// <summary>
    ///     To string the custom way
    /// </summary>
    /// <param name="value">the target</param>
    /// <param name="format">format</param>
    /// <param name="provider"></param>
    /// <returns></returns>
    public string FormatToString(object? value, string? format, IFormatProvider? provider)
    {
        if (CustomToString is { } toString)
            return toString(value, format);
        if (value is IFormattable formattable && format is not null)
            return formattable.ToString(format, provider);
        return value?.ToString() ?? "<null>";
    }

    public static implicit operator ParseItemOptions(string parameterName)
    {
        return new ParseItemOptions(parameterName)
        {
            ParseType = ParseType.In
        };
    }
}