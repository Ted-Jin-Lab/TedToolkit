namespace TedToolkit.Quantities;

/// <summary>
/// Quantity
/// </summary>
/// <typeparam name="TQuantity"></typeparam>
/// <typeparam name="TValue"></typeparam>
public interface IQuantity<TQuantity, TValue> : IQuantity<TQuantity>
    where TValue : struct, IConvertible
    where TQuantity : IQuantity<TQuantity, TValue>;

/// <summary>
/// Quantity
/// </summary>
/// <typeparam name="TQuantity"></typeparam>
public interface IQuantity<TQuantity> :
    IQuantity,
    IEquatable<TQuantity>,
    IComparable<TQuantity>;

/// <summary>
/// Quantity
/// </summary>
public interface IQuantity :
    IFormattable,
    IComparable;