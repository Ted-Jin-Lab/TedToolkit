using TedToolkit.RoslynHelper.Extensions;
using Microsoft.CodeAnalysis;

namespace TedToolkit.RoslynHelper.Names;

/// <summary>
///     For the one has type parameters.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class TypeParametersName<T> : BaseName<T>, ITypeParametersName
    where T : ISymbol
{
    private readonly Lazy<TypeParamName[]> _lazyTypeParameters;

    private protected TypeParametersName(T symbol) : base(symbol)
    {
        _lazyTypeParameters = new Lazy<TypeParamName[]>(() => GetTypeParameters(symbol).GetNames().ToArray());
    }

    /// <inheritdoc />
    public bool HasTypeParameters => TypeParameters.Length > 0;

    /// <inheritdoc />
    public TypeParamName[] TypeParameters => _lazyTypeParameters.Value;

    private protected abstract IEnumerable<ITypeParameterSymbol> GetTypeParameters(T symbol);
}