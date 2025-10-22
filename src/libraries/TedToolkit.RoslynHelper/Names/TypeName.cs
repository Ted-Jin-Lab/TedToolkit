using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace TedToolkit.RoslynHelper.Names;

/// <summary>
///     Get the type name.
/// </summary>
public class TypeName : TypeParametersName<ITypeSymbol>
{
    private readonly Lazy<string> _lazySafeName;

    internal TypeName(ITypeSymbol typeSymbol) : base(typeSymbol)
    {
        _lazySafeName = new Lazy<string>(() => Regex.Replace(FullNameNoGlobal,
            @"[.\[\]<>,\s:]", "_") + "_" + GetHashName(FullName, 8));
    }

    /// <summary>
    ///     The safe name.
    /// </summary>
    public string SafeName => _lazySafeName.Value;

    private static string GetHashName(string input, int count)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return string.Concat(hashBytes.Take(count).Select(b => chars[b % chars.Length]));
    }

    private protected override IEnumerable<ITypeParameterSymbol> GetTypeParameters(ITypeSymbol symbol)
    {
        return GetTypeParameterSymbols(symbol);

        static IEnumerable<ITypeParameterSymbol> GetTypeParameterSymbols(ITypeSymbol symbol)
        {
            if (symbol is ITypeParameterSymbol typeParameterSymbol) yield return typeParameterSymbol;
            if (symbol is not INamedTypeSymbol namedTypeSymbol) yield break;
            foreach (var typeParameter in namedTypeSymbol.TypeArguments.SelectMany(GetTypeParameterSymbols))
                yield return typeParameter;
        }
    }
}