namespace TedToolkit.Quantities;

/// <summary>
/// Implicit convert from the value type
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class QuantityImplicitFromValueTypeAttribute: Attribute;