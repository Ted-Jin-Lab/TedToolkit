namespace TedToolkit.Assertions.AssertionItems;

/// <summary>
///     The type of the assertion.
/// </summary>
public enum AssertionItemType : byte
{
    /// <summary>
    ///     For the data type.
    /// </summary>
    DataType,

    /// <summary>
    ///     For the equality
    /// </summary>
    Equality,

    /// <summary>
    ///     The comparison
    /// </summary>
    Comparison,

    /// <summary>
    ///     The custom match.
    /// </summary>
    Match,

    /// <summary>
    ///     The in range.
    /// </summary>
    Range,

    /// <summary>
    ///     Null check.
    /// </summary>
    Null,

    /// <summary>
    ///     The equality of the items.
    /// </summary>
    ItemEquality,

    /// <summary>
    ///     The Count
    /// </summary>
    ItemCount,

    /// <summary>
    ///     Enum have flag.
    /// </summary>
    Flag,

    /// <summary>
    /// </summary>
    Defined,

    /// <summary>
    ///     Empty
    /// </summary>
    Empty,

    /// <summary>
    ///     Valid
    /// </summary>
    Valid,

    /// <summary>
    /// All satisfy
    /// </summary>
    AllSatisfy,
}