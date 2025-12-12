using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TedToolkit.Assertions;

/// <summary>
/// Some exceptions extensions.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Throw ArgumentNullException if obj is null.
    /// </summary>
    /// <param name="obj">The object</param>
    /// <param name="objName">the name of obj</param>
    /// <exception cref="ArgumentNullException">exceptions</exception>
    [return: NotNull]
    public static T ThrowIfNull<T>(
        [NotNull] this T obj,
        [CallerArgumentExpression(nameof(obj))]
        string objName = "")
        where T : class
    {
        return obj ?? throw new ArgumentNullException(objName);
    }

    /// <summary>
    /// Throw ArgumentNullException if obj is null.
    /// </summary>
    /// <param name="obj">The object</param>
    /// <param name="objName">the name of obj</param>
    /// <exception cref="ArgumentNullException">exceptions</exception>
    public static T ThrowIfNull<T>(
        [NotNull] this T? obj,
        [CallerArgumentExpression(nameof(obj))]
        string objName = "")
        where T : struct
    {
        return obj ?? throw new ArgumentNullException(objName);
    }
}