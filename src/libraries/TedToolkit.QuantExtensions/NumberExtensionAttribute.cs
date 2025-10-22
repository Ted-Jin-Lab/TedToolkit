using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnitsNet;

namespace TedToolkit.QuantExtensions;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[Conditional(Constant.KeepAttributes)]
[SuppressMessage("ReSharper", "UnusedTypeParameter")]
public sealed class NumberExtensionAttribute<TValue, TUnit> : Attribute
    where TValue : struct
    where TUnit : struct, IQuantity;