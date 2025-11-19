using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Quantities;

/// <summary>
/// It can generate the operators for you.
/// </summary>
/// <param name="operator"></param>
/// <typeparam name="TRight"></typeparam>
/// <typeparam name="TResult"></typeparam>
[Conditional("CODE_ANALYSIS")]
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
[SuppressMessage("ReSharper", "UnusedTypeParameter")]
public class QuantityOperatorAttribute<TRight, TResult>(Operator @operator) : Attribute
    where TRight : struct
    where TResult : struct;

/// <summary>
/// Operator types.
/// </summary>
public enum Operator : byte
{
    /// <summary>
    /// +
    /// </summary>
    Add,
    /// <summary>
    /// -
    /// </summary>
    Subtract,
    /// <summary>
    /// *
    /// </summary>
    Multiply,
    /// <summary>
    ///  /
    /// </summary>
    Divide,
}