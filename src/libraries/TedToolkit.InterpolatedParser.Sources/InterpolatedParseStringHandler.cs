using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TedToolkit.InterpolatedParser.Options;
using TedToolkit.InterpolatedParser.Parsers;

namespace TedToolkit.InterpolatedParser;

// ReSharper disable once PartialTypeWithSinglePart
internal partial class InterpolatedParseStringHandler
{
    private MainParser _parser;


    protected InterpolatedParseStringHandler(int formattedCount, string input, bool tryIt, ParseOptions options)
    {
        _parser = new MainParser(input, formattedCount, tryIt, options);
    }

    public void AppendLiteral([StringSyntax(StringSyntaxAttribute.Regex)] string regex)
    {
        _parser.AppendRegex(regex);
    }

    internal ParseResult[] Solve()
    {
        return _parser.Solve();
    }

    #region Format

    [OverloadResolutionPriority(-1)]
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public void AppendFormatted(object t, string format, [CallerArgumentExpression(nameof(t))] string callerName = "")
    {
        AppendObject();
    }

    [OverloadResolutionPriority(-1)]
    public void AppendFormatted(object t, [CallerArgumentExpression(nameof(t))] string callerName = "")
    {
        AppendObject();
    }

    private static void AppendObject()
    {
        throw new NotImplementedException(
            "The method or operation is not implemented. Please check the source generator.");
    }

    #endregion

    #region Append

    public void AppendCollection<TCollection, TValue>(in TCollection t, string? format, string callerName,
        IParser? collectionParser, IParser? itemParser)
        where TCollection : ICollection<TValue>, new()
    {
        _parser.AppendCollection<TCollection, TValue>(t, format, callerName, collectionParser, itemParser);
    }

    public void AppendObject<T>(in T t, string? format, string callerName, IParser? parser)
    {
        _parser.AppendObject(t, format, callerName, parser);
    }

    #endregion
}