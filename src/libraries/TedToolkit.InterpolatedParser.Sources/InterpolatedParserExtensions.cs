using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TedToolkit.InterpolatedParser.Options;

namespace TedToolkit.InterpolatedParser;

/// <summary>
///     The extensions
/// </summary>
internal static class InterpolatedParserExtensions
{
    /// <summary>
    ///     The basic Parse
    /// </summary>
    /// <param name="input"></param>
    /// <param name="template"></param>
    public static void Parse(this string input,
        [StringSyntax(StringSyntaxAttribute.Regex)] [InterpolatedStringHandlerArgument(nameof(input))]
        ParseStringHandler template)
    {
        template.Solve();
    }

    /// <summary>
    /// </summary>
    /// <param name="input"></param>
    /// <param name="options"></param>
    /// <param name="template"></param>
    public static void Parse(this string input, ParseOptions options,
        [StringSyntax(StringSyntaxAttribute.Regex)] [InterpolatedStringHandlerArgument(nameof(input), nameof(options))]
        ParseStringHandler template)
    {
        template.Solve();
    }

    /// <summary>
    /// </summary>
    /// <param name="input"></param>
    /// <param name="template"></param>
    /// <returns></returns>
    public static ParseResult[] TryParse(this string input,
        [StringSyntax(StringSyntaxAttribute.Regex)] [InterpolatedStringHandlerArgument(nameof(input))]
        TryParseStringHandler template)
    {
        return template.Solve();
    }

    /// <summary>
    /// </summary>
    /// <param name="input"></param>
    /// <param name="options"></param>
    /// <param name="template"></param>
    /// <returns></returns>
    public static ParseResult[] TryParse(this string input, ParseOptions options,
        [StringSyntax(StringSyntaxAttribute.Regex)] [InterpolatedStringHandlerArgument(nameof(input), nameof(options))]
        TryParseStringHandler template)
    {
        return template.Solve();
    }
}