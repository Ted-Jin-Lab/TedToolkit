using TedToolkit.Assertions.Assertions;

namespace TedToolkit.Assertions.Constraints;

/// <summary>
///     The constraint
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TMatchedElement"></typeparam>
public class AndWhichConstraint<TValue, TMatchedElement> : AndConstraint<TValue>
{
    private readonly Lazy<TMatchedElement> _itemGetter;
    private readonly string _suffix;

    internal AndWhichConstraint(ObjectAssertion<TValue> assertion, Func<TMatchedElement> itemGetter, string suffix) :
        base(assertion)
    {
        _itemGetter = new Lazy<TMatchedElement>(itemGetter);
        _suffix = suffix;
    }

    /// <summary>
    ///     The which thing.
    /// </summary>
    public WhichConstraint<TMatchedElement> Which => new(_itemGetter, And.SubjectName + _suffix, And.CallerInfo);
}