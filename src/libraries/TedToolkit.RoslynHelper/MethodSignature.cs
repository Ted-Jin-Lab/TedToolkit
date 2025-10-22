using Microsoft.CodeAnalysis;

namespace TedToolkit.RoslynHelper;

/// <summary>
///     The signature of a method
/// </summary>
/// <param name="methodSymbol"></param>
public readonly struct MethodSignature(IMethodSymbol methodSymbol) : IEquatable<MethodSignature>
{
    /// <summary>
    ///     Should check the equality with containing type.
    /// </summary>
    public static bool EqualityWithContainingType { get; set; } = true;

    /// <summary>
    ///     the name of the method.
    /// </summary>
    public string MethodName { get; } = methodSymbol.Name;

    /// <summary>
    ///     The containing type.
    /// </summary>
    public ITypeSymbol ContainingType { get; } = methodSymbol.IsExtensionMethod
        ? methodSymbol.Parameters[0].Type.OriginalDefinition
        : methodSymbol.ContainingType.OriginalDefinition;

    /// <summary>
    ///     Tye parameter types.
    /// </summary>
    public ITypeSymbol[] ParameterTypes { get; } = methodSymbol.Parameters
        .Skip(methodSymbol.IsExtensionMethod ? 1 : 0)
        .Select(p => p.Type.OriginalDefinition)
        .ToArray();

    /// <summary>
    ///     The Ref Kinds.
    /// </summary>
    public RefKind[] RefKinds { get; } = methodSymbol.Parameters
        .Skip(methodSymbol.IsExtensionMethod ? 1 : 0)
        .Select(i => i.RefKind)
        .ToArray();

    /// <summary>
    ///     The type Argument counts.
    /// </summary>
    public int TypeArgumentsCount { get; } =
        methodSymbol.TypeArguments.Length + methodSymbol.ContainingType.TypeArguments.Length;

    /// <inheritdoc />
    public bool Equals(MethodSignature other)
    {
        if (!MethodName.Equals(other.MethodName)) return false;
        if (!TypeArgumentsCount.Equals(other.TypeArgumentsCount)) return false;
        if (EqualityWithContainingType &&
            !ContainingType.Equals(other.ContainingType, SymbolEqualityComparer.Default)) return false;
        if (!ParameterTypes.Length.Equals(other.ParameterTypes.Length)) return false;
        for (var i = 0; i < ParameterTypes.Length; i++)
        {
            var thisType = ParameterTypes[i];
            var otherType = other.ParameterTypes[i];
            if (thisType.TypeKind == TypeKind.TypeParameter
                && otherType.TypeKind == TypeKind.TypeParameter)
                continue;

            if (RefKinds[i] != other.RefKinds[i]) return false;
            if (!thisType.Equals(otherType, SymbolEqualityComparer.Default)) return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is MethodSignature other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = MethodName.GetHashCode();
            hashCode = (hashCode * 397) ^ ParameterTypes.Length.GetHashCode();
            hashCode = (hashCode * 397) ^ TypeArgumentsCount.GetHashCode();
            return hashCode;
        }
    }
}