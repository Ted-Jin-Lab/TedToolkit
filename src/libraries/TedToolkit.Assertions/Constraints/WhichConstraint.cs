using TedToolkit.Assertions.Assertions;

namespace TedToolkit.Assertions.Constraints;

/// <summary>
/// </summary>
public class WhichConstraint<TValue> : IConstraint
{
    private readonly CallerInfo _callerInfo;
    private readonly string _name;
    private readonly Lazy<TValue> _value;

    internal WhichConstraint(Lazy<TValue> value, string name, CallerInfo callerInfo)
    {
        _value = value;
        _name = name;
        _callerInfo = callerInfo;
    }

    /// <summary>
    /// </summary>
    public ObjectAssertion<TValue> Must => CreateAssertion(AssertionType.Must);

    /// <summary>
    /// </summary>
    public ObjectAssertion<TValue> Should => CreateAssertion(AssertionType.Should);

    /// <summary>
    /// </summary>
    public ObjectAssertion<TValue> Could => CreateAssertion(AssertionType.Could);

    private ObjectAssertion<TValue> CreateAssertion(AssertionType assertionType)
    {
        TValue value = default!;
        var isValid = true;
        try
        {
            value = _value.Value;
        }
        catch
        {
            isValid = false;
        }

        return new ObjectAssertion<TValue>(value, _name, assertionType, _callerInfo, isValid);
    }
}