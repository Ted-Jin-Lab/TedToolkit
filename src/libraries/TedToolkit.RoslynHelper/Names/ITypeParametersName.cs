namespace TedToolkit.RoslynHelper.Names;

/// <summary>
/// </summary>
public interface ITypeParametersName
{
    /// <summary>
    ///     Has the type parameters.
    /// </summary>
    bool HasTypeParameters { get; }

    /// <summary>
    ///     Get the type parameters symbol
    /// </summary>
    TypeParamName[] TypeParameters { get; }
}