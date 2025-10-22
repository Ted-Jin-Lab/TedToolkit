using Microsoft.CodeAnalysis;

namespace TedToolkit.RoslynHelper.Names;

/// <summary>
/// </summary>
public interface IName
{
    /// <summary>
    ///     Full Name
    /// </summary>
    string FullName { get; }

    /// <summary>
    ///     Full Name with Null
    /// </summary>
    string FullNameNull { get; }

    /// <summary>
    ///     Summary Name
    /// </summary>
    string SummaryName { get; }

    /// <summary>
    ///     Name
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     MiniName
    /// </summary>
    string MiniName { get; }

    /// <summary>
    ///     Full Name without global
    /// </summary>
    string FullNameNoGlobal { get; }
}

/// <summary>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IName<out T> : IName where T : ISymbol
{
    /// <summary>
    ///     Symbol
    /// </summary>
    public T Symbol { get; }
}