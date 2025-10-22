using System.Globalization;

namespace TedToolkit.InterpolatedParser.Parsers;

/// <inheritdoc />
public abstract class Parser : IParser
{
    private static readonly Dictionary<char, NumberStyles> ShorthandMappings = new()
    {
        { 'C', NumberStyles.Currency },
        { 'N', NumberStyles.Number },
        { 'X', NumberStyles.HexNumber },
        { 'G', NumberStyles.Any },
        { 'D', NumberStyles.Integer },
        { 'E', NumberStyles.Float },
        { 'F', NumberStyles.Float },
        { 'P', NumberStyles.Number },
        { 'I', NumberStyles.Integer },
        { 'H', NumberStyles.HexNumber },
        { 'A', NumberStyles.Any }
    };

    private Lazy<NumberStyles> _getNumberStyle;

    protected Parser()
    {
        _getNumberStyle = new Lazy<NumberStyles>(GetStyle);
    }

    public NumberStyles NumberStyle => _getNumberStyle.Value;

    /// <inheritdoc />
    public abstract Type TargetType { get; }

    /// <inheritdoc />
    public string? Format
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            _getNumberStyle = new Lazy<NumberStyles>(GetStyle);
        }
    }

    private NumberStyles GetStyle()
    {
        return Format is null ? NumberStyles.None : GetStyle(Format);
    }

    private static NumberStyles GetStyle(string format)
    {
        if (Enum.TryParse(format, true, out NumberStyles parsedStyle)) return parsedStyle;

        NumberStyles result = 0;

        foreach (var ch in format.ToUpper())
            if (ShorthandMappings.TryGetValue(ch, out var style))
                result |= style;

        return result;
    }
}