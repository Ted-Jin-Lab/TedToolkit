using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using TedToolkit.InterpolatedParser.Options;
using TedToolkit.InterpolatedParser.ParseItems;
using TedToolkit.InterpolatedParser.Parsers;

namespace TedToolkit.InterpolatedParser;

public delegate void ParseDelegate(IParseItem item, string text, int start, int? length);

public struct MainParser(string input, int count, bool isTry, ParseOptions options)
{
    private readonly List<ParseResult> _results = new(count);

    private IParseItem? _item;
    private int _startIndex;

    private bool _isFirst = true;

    public void AppendRegex([StringSyntax(StringSyntaxAttribute.Regex)] string regex)
    {
        if (_isFirst && options.Beginning && !regex.StartsWith("^"))
        {
            _isFirst = false;
            regex = "^" + regex;
        }

        var match = new Regex(regex).Match(input, _startIndex);
        if (!match.Success) throw new FormatException("Invalid format to get the parsed string.");
        if (_item != null) Handle(_startIndex, match.Index - _startIndex);

        _startIndex = match.Index + match.Length;
    }

    #region Item

    public void AppendCollection<TCollection, TValue>(in TCollection t, string? format, string callerName,
        IParser? collectionParser, IParser? itemParser)
        where TCollection : ICollection<TValue>, new()
    {
        var option = options[callerName];
        var preModify = option.PreModification ?? options.ProModification;
        if (option.DataType is not DataType.List)
        {
            var realParser = GetParser(t, format, collectionParser, option) ?? FindParser<TValue>();
            SetFormat(ref realParser, format);
            AppendObjectRaw(t, realParser, preModify);
        }
        else
        {
            var realParser = GetParser(t, format, itemParser, option) ?? FindParser<TCollection>();
            SetFormat(ref realParser, format);
            AppendCollectionRaw<TCollection, TValue>(t, realParser, option.Separator, preModify);
        }
    }

    public void AppendObject<T>(in T t, string? format, string callerName, IParser? parser)
    {
        var option = options[callerName];
        var preModify = option.PreModification ?? options.ProModification;
        var realParser = GetParser(t, format, parser, option) ?? FindParser<T>();
        SetFormat(ref realParser, format);
        AppendObjectRaw(t, realParser, preModify);
    }

    private static void SetFormat(ref IParser? parser, string? format)
    {
        if (parser is null) return;
        parser.Format = format;
    }

    private void AppendCollectionRaw<TCollection, TValue>(in TCollection t, IParser? parser, string separator,
        PreModifyOptions preModify)
        where TCollection : ICollection<TValue>, new()
    {
        if (string.IsNullOrEmpty(separator))
            throw new ArgumentNullException(nameof(separator), "Separator cannot be empty.");
        switch (parser)
        {
#if NETCOREAPP
            case ISpanParser<TValue> spanParser:
                AppendItem(
                    new CollectionSpanParseItem<TCollection, TValue>(t, spanParser, separator,
                        preModify));
                break;
#endif
            case IStringParser<TValue> stringParser:
                AppendItem(new CollectionStringParseItem<TCollection, TValue>(t, stringParser, separator, preModify));
                break;
            case not null:
                throw new InvalidDataException($"Invalid parser type, which is {parser.GetType()}.");
        }
    }

    private void AppendObjectRaw<T>(in T t, IParser? parser, PreModifyOptions type)
    {
        switch (parser)
        {
#if NETCOREAPP
            case ISpanParser<T> spanParser:
                AppendItem(new SpanParseItem<T>(t, spanParser, type));
                break;
#endif
            case IStringParser<T> stringParser:
                AppendItem(new StringParseItem<T>(t, stringParser, type));
                break;
            case not null:
                throw new InvalidDataException($"Invalid parser type, which is {parser.GetType()}.");
        }
    }

    private bool IsInput<T>(T t, ParseItemOptions option, string? format)
    {
        if (option.ParseType is not ParseType.In) return false;
        var provider = options.FormatProvider;
        if (option.DataType is DataType.List && t is IEnumerable list)
            AppendRegex(AddBeginning(string.Join(option.Separator,
                from object? item in list select option.FormatToString(item, format, provider))));
        else
            AppendRegex(AddBeginning(option.FormatToString(t, format, provider)));

        return true;

        string AddBeginning(string s)
        {
            return option.Beginning ? "^" + s : s;
        }
    }

    private void AppendItem(IParseItem item)
    {
        if (_item is not null) throw new FormatException("Item is already there!");
        _item = item;
    }

    #endregion

    #region Parser

    private IParser? GetParser<T>(in T t, string? format, IParser? parser, ParseItemOptions option)
    {
        if (IsInput(t, option, format)) return null;
        var realParser = option.Parser ?? parser;
        if (realParser is null) throw new NullReferenceException("Parser is null!");
        return realParser;
    }

    private IParser? FindParser<T>()
    {
        foreach (var parser in options.Parsers)
            if (parser.TargetType == typeof(T))
                return parser;

        foreach (var parser in options.Parsers)
            if (parser.TargetType.IsAssignableFrom(typeof(T)))
                return parser;

        return null;
    }

    #endregion

    public ParseResult[] Solve()
    {
        if (_startIndex < input.Length) Handle(_startIndex, null);
        return _results.ToArray();
    }

    private void Handle(int start, int? length)
    {
        if (_item is null) return;
        var provider = options.FormatProvider;
        switch (_item)
        {
            case IStringParseItem si:
                var subString = length.HasValue ? input.Substring(start, length.Value) : input[start..];
                var modifiedString = _item.PreModification.ModifyString(subString);
                if (isTry)
                    _results.Add(si.TryParse(modifiedString, provider));
                else
                    si.Parse(modifiedString, provider);
                break;
#if NETCOREAPP
            case ISpanParseItem si:
                var subSpan = length.HasValue ? input.AsSpan(start, length.Value) : input.AsSpan(start);
                var modifiedSpan = _item.PreModification.ModifyString(subSpan);
                if (isTry)
                    _results.Add(si.TryParse(modifiedSpan, provider));
                else
                    si.Parse(modifiedSpan, provider);
                break;
#endif
            default:
                throw new StrongTypingException("Unexpected IParseItem");
        }

        _item = null;
    }
}