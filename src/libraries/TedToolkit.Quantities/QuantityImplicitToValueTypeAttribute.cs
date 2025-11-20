namespace TedToolkit.Quantities;

/// <summary>
/// Implicit convert to the value type
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class QuantityImplicitToValueTypeAttribute : Attribute;