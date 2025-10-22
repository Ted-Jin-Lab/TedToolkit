using Microsoft.CodeAnalysis;

namespace TedToolkit.RoslynHelper.Names;

/// <inheritdoc />
public abstract class BaseName<T> : IName<T> where T : ISymbol
{
    private readonly Lazy<string> _lazyFullName,
        _lazySummaryName,
        _lazyFullNameNoGlobal,
        _lazyFullNameNull,
        _lazyMiniName;

    private protected BaseName(T symbol)
    {
        Symbol = symbol;
        _lazyFullName = new Lazy<string>(() => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        _lazyFullNameNull = new Lazy<string>(() => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
            .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier)));
        _lazyFullNameNoGlobal = new Lazy<string>(() => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)));
        _lazyMiniName = new Lazy<string>(() => symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        _lazySummaryName = new Lazy<string>(GetSummaryName);
    }

    /// <inheritdoc />
    public T Symbol { get; }

    /// <inheritdoc />
    public string Name => Symbol.Name;

    /// <inheritdoc />
    public string MiniName => _lazyMiniName.Value;

    /// <inheritdoc />
    public string FullNameNoGlobal => _lazyFullNameNoGlobal.Value;

    /// <inheritdoc />
    public string FullName => _lazyFullName.Value;

    /// <inheritdoc />
    public string FullNameNull => _lazyFullNameNull.Value;

    /// <inheritdoc />
    public string SummaryName => _lazySummaryName.Value;

    private protected virtual string GetSummaryName()
    {
        return ToSummary(Symbol.OriginalDefinition
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }

    private protected static string ToSummary(string name)
    {
        return name.Replace('<', '{').Replace('>', '}');
    }
}