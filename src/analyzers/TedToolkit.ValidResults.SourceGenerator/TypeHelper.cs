using TedToolkit.RoslynHelper.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace TedToolkit.ValidResults.SourceGenerator;

public static class TypeHelper
{
    public static TypeSyntax GetResultDataType(ITypeSymbol type)
    {
        if (GetParentDataType(type) is { } dataType) type = dataType;

        if (type.GetName().FullName is "global::TedToolkit.ValidResults.ValidResult.Data")
            return IdentifierName(Identifier("global::TedToolkit.ValidResults.ValidResult.Data"));

        NameSyntax childType = type.SpecialType is SpecialType.System_Void
            ? IdentifierName(Identifier("global::TedToolkit.ValidResults.ValidResult"))
            : GenericName(
                    Identifier("global::TedToolkit.ValidResults.ValidResult"))
                .WithTypeArgumentList(TypeArgumentList(
                [
                    IdentifierName(type.GetName().FullName)
                ]));
        return QualifiedName(childType, IdentifierName("Data"));
    }

    public static bool IsDisposable(ITypeSymbol type)
    {
        return type.AllInterfaces.Any(i => i.GetName().FullName is "global::System.IDisposable");
    }

    public static TypeSyntax FindValidResultType(Dictionary<ISymbol?, SimpleType> dictionary,
        ITypeSymbol target, out bool shouldDispose, out ITypeSymbol? dataTypeSymbol, bool containSelf)
    {
        dataTypeSymbol = null;
        if (target.SpecialType is SpecialType.System_Void)
        {
            shouldDispose = false;
            return IdentifierName(Identifier("global::TedToolkit.ValidResults.ValidResult"));
        }

        if (GetParentDataType(target) is { } dataType) target = dataType;

        if (target.GetName().FullName is "global::TedToolkit.ValidResults.ValidResult.Data")
        {
            shouldDispose = false;
            return IdentifierName(Identifier("global::TedToolkit.ValidResults.ValidResult"));
        }

        var isDataDispose = IsDisposable(target);
        if (FindValidResultType(dictionary, target, containSelf) is { } pair)
        {
            var (dataSymbol, resultTypeSymbol) = pair;
            shouldDispose = isDataDispose && !IsDisposable(dataSymbol);
            dataTypeSymbol = dataSymbol;
            var type = IdentifierName(resultTypeSymbol.FullName);
            return type;
        }

        shouldDispose = isDataDispose;
        return GenericName(
                Identifier("global::TedToolkit.ValidResults.ValidResult"))
            .WithTypeArgumentList(TypeArgumentList(
            [
                IdentifierName(target.GetName().FullName)
            ]));
    }

    private static (ITypeSymbol DataSymbol, SimpleType TargetSymbol)? FindValidResultType(
        Dictionary<ISymbol?, SimpleType> dictionary,
        ITypeSymbol data, bool containSelf)
    {
        var loopTarget = containSelf ? data : data.BaseType;
        while (loopTarget is not null)
        {

            if (dictionary.TryGetValue(loopTarget, out var symbol))
            {
                if (loopTarget is INamedTypeSymbol { IsGenericType: true } targetSymbol
                    && targetSymbol.TypeParameters.Any())
                {
                    return (loopTarget, symbol.WithGenericTypes(targetSymbol.TypeParameters
                        .Select(i => i.GetName().FullName)));
                }
                return (loopTarget, symbol);
            }
            loopTarget = loopTarget.BaseType;
        }

        loopTarget = containSelf ? data : data.BaseType;
        while (loopTarget is not null)
        {
            if (loopTarget is INamedTypeSymbol { IsGenericType: true } targetSymbol &&
                dictionary.TryGetValue(loopTarget.OriginalDefinition, out var symbol))
            {
                return (loopTarget, symbol.WithGenericTypes(targetSymbol.TypeArguments
                    .Select(i => i.GetName().FullName)));
            }

            loopTarget = loopTarget.BaseType;
        }

        return null;
    }

    private static ITypeSymbol? GetParentDataType(ITypeSymbol type)
    {
        if (type.ContainingType is not { } containingType) return null;
        var resultInterface = containingType.AllInterfaces.FirstOrDefault(i =>
        {
            if (!i.IsGenericType) return false;
            return i.ConstructedFrom.GetName().FullName is "global::TedToolkit.ValidResults.IValidResult<TValue>";
        });
        return resultInterface?.TypeArguments.FirstOrDefault();
    }
}