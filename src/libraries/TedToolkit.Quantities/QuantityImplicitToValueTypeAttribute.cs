using System.Diagnostics.CodeAnalysis;

namespace TedToolkit.Quantities;
[AttributeUsage(AttributeTargets.Struct)]
[SuppressMessage("ReSharper", "UnusedTypeParameter")]
public sealed class QuantityImplicitToValueTypeAttribute: Attribute;