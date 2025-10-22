using System.Diagnostics;
using TedToolkit.Assertions.AssertionItems;
using TedToolkit.Assertions.Assertions;
using TedToolkit.Assertions.Constraints;

namespace TedToolkit.Assertions.Execution;

/// <summary>
///     The strategy to handle the exceptions.
///     <remarks>
///         It is recommended to use the attribute <see cref="DebuggerHiddenAttribute" /> to all the method that this
///         has.
///     </remarks>
/// </summary>
public interface IAssertionStrategy
{
    /// <summary>
    ///     handel the assertion.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="assertions"></param>
    /// <returns></returns>
    object? HandleFailure(AssertionScope scope, IReadOnlyList<IAssertion> assertions);

    /// <summary>
    ///     Handle the assertions items.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="assertionType"></param>
    /// <param name="assertion"></param>
    /// <param name="tag"></param>
    /// <param name="callerInfo"></param>
    /// <returns>This value will push to <see cref="IAndConstraint.FailureReturnValues" /></returns>
    object? HandleFailure(AssertionScope scope, AssertionType assertionType, AssertionItem assertion, object? tag,
        CallerInfo callerInfo);
}