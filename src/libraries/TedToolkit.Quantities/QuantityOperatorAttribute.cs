using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Quantities;

[Conditional("CODE_ANALYSIS")]
[AttributeUsage(AttributeTargets.Struct)]
[SuppressMessage("ReSharper", "UnusedTypeParameter")]
public class QuantityOperatorAttribute<TRight, TResult>(Operator @operator) : Attribute
    where TRight : struct
    where TResult : struct;

public enum Operator : byte
{
    Add,
    Subtract,
    Multiply,
    Divide,
}