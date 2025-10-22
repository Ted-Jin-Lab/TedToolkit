using TedToolkit.Assertions.Assertions;

namespace TedToolkit.Assertions.Constraints;

/// <summary>
///     The assertion it.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class PronounConstraint<TValue> : IConstraint
{
    private readonly ObjectAssertion<TValue> _assertion;

    internal PronounConstraint(ObjectAssertion<TValue> assertion)
    {
        _assertion = assertion;
    }

    /// <summary>
    /// </summary>
    public ObjectAssertion<TValue> Must => _assertion.Duplicate(AssertionType.Must);

    /// <summary>
    /// </summary>
    public ObjectAssertion<TValue> Should => _assertion.Duplicate(AssertionType.Should);

    /// <summary>
    /// </summary>
    public ObjectAssertion<TValue> Could => _assertion.Duplicate(AssertionType.Could);
}