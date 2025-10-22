using TedToolkit.Assertions.AssertionItems;

namespace TedToolkit.Assertions.Assertions;

/// <summary>
///     The Assertion
/// </summary>
public interface IAssertion
{
    /// <summary>
    ///     The assertion items.
    /// </summary>
    IReadOnlyList<AssertionItem> Items { get; }

    /// <summary>
    ///     Created time
    /// </summary>
    DateTimeOffset CreatedTime { get; }

    /// <summary>
    ///     Assertion Type
    /// </summary>
    AssertionType Type { get; }

    /// <summary>
    ///     The Caller Info
    /// </summary>
    CallerInfo CallerInfo { get; }
}