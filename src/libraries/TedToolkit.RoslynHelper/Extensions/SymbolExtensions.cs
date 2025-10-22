using Microsoft.CodeAnalysis;
using TedToolkit.RoslynHelper.Names;

namespace TedToolkit.RoslynHelper.Extensions;

/// <summary>
///     Extensions for symbol
/// </summary>
public static class SymbolExtensions
{
    /// <summary>
    ///     Get the type name.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static TypeName GetName(this ITypeSymbol symbol)
    {
        return new TypeName(symbol);
    }

    /// <summary>
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static TypeParamName GetName(this ITypeParameterSymbol symbol)
    {
        return new TypeParamName(symbol);
    }

    /// <summary>
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static MethodName GetName(this IMethodSymbol symbol)
    {
        return new MethodName(symbol);
    }

    /// <summary>
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static ParameterName GetName(this IParameterSymbol symbol)
    {
        return new ParameterName(symbol);
    }

    /// <summary>
    /// </summary>
    /// <param name="symbols"></param>
    /// <returns></returns>
    public static IEnumerable<TypeParamName> GetNames(this IEnumerable<ITypeParameterSymbol> symbols)
    {
        return symbols.Select(symbol => symbol.GetName());
    }

    /// <summary>
    /// </summary>
    /// <param name="symbols"></param>
    /// <returns></returns>
    public static IEnumerable<ParameterName> GetNames(this IEnumerable<IParameterSymbol> symbols)
    {
        return symbols.Select(symbol => symbol.GetName());
    }

    /// <summary>
    ///     Get the extension methods for
    /// </summary>
    /// <param name="compilation"></param>
    /// <returns></returns>
    public static IReadOnlyDictionary<ISymbol?, IMethodSymbol[]> GetAllExtensionMethods(this Compilation compilation)
    {
        return compilation.GlobalNamespace.GetAllStaticClasses()
            .SelectMany(c => c.GetMembers())
            .OfType<IMethodSymbol>()
            .Where(m => m is { IsStatic: true, IsExtensionMethod: true, Parameters.Length: > 0 })
            .GroupBy(m => m.Parameters[0].Type.ReplaceWithNestedOriginalDefinition(), SymbolEqualityComparer.Default)
            .ToDictionary(m => m.Key, m => m.ToArray(), SymbolEqualityComparer.Default);
    }

    /// <summary>
    ///     Get all static classes in a namespace.
    /// </summary>
    /// <param name="namespaceSymbol"></param>
    /// <returns></returns>
    public static IEnumerable<INamedTypeSymbol> GetAllStaticClasses(this INamespaceSymbol namespaceSymbol)
    {
        return namespaceSymbol.GetAllTypes().Where(t => t.IsStatic && t.TypeKind == TypeKind.Class);
    }

    /// <summary>
    ///     Get all types in a namespace.
    /// </summary>
    /// <param name="namespaceSymbol"></param>
    /// <returns></returns>
    public static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamespaceSymbol namespaceSymbol)
    {
        var typeMembers = namespaceSymbol.GetTypeMembers();

        foreach (var typeMember in typeMembers) yield return typeMember;

        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        foreach (var nestTypeMember in GetAllTypes(nestedNamespace))
            yield return nestTypeMember;
    }

    /// <summary>
    ///     Replace the type with nested original definition.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static ITypeSymbol ReplaceWithNestedOriginalDefinition(this ITypeSymbol symbol)
    {
        if (symbol is not INamedTypeSymbol { IsGenericType: true } named) return symbol;
        if (named.TypeArguments.All(t => t.TypeKind is TypeKind.TypeParameter)) return symbol.OriginalDefinition;
        var newArgs = named.TypeArguments
            .Select(ReplaceWithNestedOriginalDefinition)
            .ToArray();
        return named.OriginalDefinition.Construct(newArgs);
    }
}