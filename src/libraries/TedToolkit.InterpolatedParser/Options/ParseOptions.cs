using TedToolkit.InterpolatedParser.Parsers;

namespace TedToolkit.InterpolatedParser.Options;

/// <summary>
///     The options
/// </summary>
public readonly record struct ParseOptions()
{
    private readonly Dictionary<string, ParseItemOptions> _options = [];

    /// <summary>
    ///     Patch at the beginning
    /// </summary>
    public bool Beginning { get; init; }

    /// <summary>
    ///     Format provider
    /// </summary>
    public IFormatProvider? FormatProvider { get; init; }

    /// <summary>
    /// </summary>
    public PreModifyOptions ProModification { get; init; } = new();

    /// <summary>
    ///     Your parameters options.
    /// </summary>
    public ParseItemOptions[] ParameterOptions
    {
        init { _options = value.ToDictionary(o => o.ParameterName); }
    }

    /// <summary>
    ///     Just your parsers. For the case it can't find the parser.
    /// </summary>
    public IParser[] Parsers { get; init; } = [];

    /// <summary>
    ///     Get the option.
    /// </summary>
    /// <param name="parameterName"></param>
    public ParseItemOptions this[string parameterName]
        => _options.TryGetValue(parameterName, out var options) ? options : ParseItemOptions.Default;

    /// <summary>
    ///     Converters.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static implicit operator ParseOptions(ParseItemOptions[] options)
    {
        return new ParseOptions
        {
            ParameterOptions = options
        };
    }

    /// <summary>
    ///     Converters.
    /// </summary>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static implicit operator ParseOptions(IParser[] parsers)
    {
        return new ParseOptions
        {
            Parsers = parsers
        };
    }
}