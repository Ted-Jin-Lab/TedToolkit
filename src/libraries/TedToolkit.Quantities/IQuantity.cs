namespace TedToolkit.Quantities;

/// <summary>
/// Quantity
/// </summary>
public interface IQuantity<TQuantity, TValue>:
    IFormattable, 
    IEquatable<TQuantity>,
    IComparable<TQuantity>
    where TValue : struct, IConvertible
    where TQuantity : IQuantity<TQuantity, TValue>;