using System.Numerics;

namespace TedToolkit.Quantities;

/// <summary>
/// Quantity
/// </summary>
/// <typeparam name="TQuantity"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TUnit"></typeparam>
public interface IQuantity<TQuantity, TValue, in TUnit> :
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    TValue As(TUnit unit);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    string ToString(TUnit unit, string? format = null, IFormatProvider? formatProvider = null);
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
}

/// <summary>
/// Quantity
/// </summary>
/// <typeparam name="TQuantity"></typeparam>
public interface IQuantityQuantity<TQuantity> :
    IQuantity,
    IEquatable<TQuantity>,
    IComparable<TQuantity>
    where TQuantity : struct, IQuantityQuantity<TQuantity>
{
#if NET7_0_OR_GREATER
    /// <summary/>
    static abstract TQuantity Zero { get; }

    /// <summary/>
    static abstract TQuantity One { get; }
#endif

    /// <inheritdoc cref="Math.Abs(double)"/>
    TQuantity Abs();

    /// <inheritdoc cref="Math.Min(double, double)"/>
    TQuantity Min(TQuantity val2);

    /// <inheritdoc cref="Math.Max(double, double)"/>
    TQuantity Max(TQuantity val2);

    /// <inheritdoc cref="Math.Clamp(double, double, double)"/>
    TQuantity Clamp(TQuantity min, TQuantity max);
}

/// <summary>
/// Quantity
/// </summary>
public interface IQuantity :
    IFormattable,
    IComparable;