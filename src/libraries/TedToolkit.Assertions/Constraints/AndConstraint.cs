using TedToolkit.Assertions.Assertions;
using TedToolkit.Assertions.Execution;

namespace TedToolkit.Assertions.Constraints;

/// <summary>
///     Just the And Constraint
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class AndConstraint<TValue> : IAndConstraint
{
    internal AndConstraint(ObjectAssertion<TValue> assertion)
    {
        And = assertion;
    }

    /// <summary>
    ///     And things.
    /// </summary>
    public ObjectAssertion<TValue> And { get; }

    /// <summary>
    ///     And it.
    /// </summary>
    public PronounConstraint<TValue> AndIt => new(And);

    /// <inheritdoc />
    public IReadOnlyDictionary<IAssertionStrategy, object>? FailureReturnValues { get; set; }

    /// <summary>
    /// Get the Value
    /// </summary>
    public TValue Value => And.Subject;
}