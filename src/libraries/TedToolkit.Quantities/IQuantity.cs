using System.Numerics;

namespace TedToolkit.Quantities;

/// <summary>
/// Quantity
/// </summary>
/// <typeparam name="TQuantity"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TUnit"></typeparam>
public interface IQuantity<TQuantity, TValue, TUnit> :
    IQuantityQuantity<TQuantity>,
    IQuantityValue<TValue>
    where TValue : struct,
#if NET7_0_OR_GREATER
    INumber<TValue>,
#endif
    IConvertible
    where TQuantity : struct, IQuantity<TQuantity, TValue, TUnit>
    where TUnit : Enum
{
#if NET7_0_OR_GREATER
    /// <summary/>
    static abstract TQuantity From(TValue value, TUnit unit);
#endif
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface IQuantityValue<out TValue> :
    IQuantity
    where TValue : struct,
#if NET7_0_OR_GREATER
    INumber<TValue>,
#endif
    IConvertible
{
    /// <summary>
    /// The Value. In the most case, you don't need it.
    /// </summary>
    TValue Value { get; }

#if NET7_0_OR_GREATER
    /// <summary/>
    static abstract TValue Zero { get; }

    /// <summary/>
    static abstract TValue One { get; }
#endif
}

/// <summary>
/// Quantity
/// </summary>
/// <typeparam name="TQuantity"></typeparam>
public interface IQuantityQuantity<TQuantity> :
    IQuantity,
    IEquatable<TQuantity>,
    IComparable<TQuantity>
    where TQuantity : struct, IQuantityQuantity<TQuantity>;

/// <summary>
/// Quantity
/// </summary>
public interface IQuantity :
    IFormattable,
    IComparable;