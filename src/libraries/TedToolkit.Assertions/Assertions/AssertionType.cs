namespace TedToolkit.Assertions.Assertions;

/// <summary>
///     The type of the assertion.
/// </summary>
public enum AssertionType : byte
{
    /// <summary>
    ///     Could way.
    /// </summary>
    Could,

    /// <summary>
    ///     Should way.
    /// </summary>
    Should,

    /// <summary>
    ///     Must way.
    /// </summary>
    Must
}