namespace TedToolkit.Quantities;

/// <summary>
/// The Quantity Display Unit Attribute.
/// </summary>
/// <param name="enum"></param>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class QuantityDisplayUnitAttribute<TEnum>(TEnum @enum) : Attribute
    where TEnum : struct, Enum;