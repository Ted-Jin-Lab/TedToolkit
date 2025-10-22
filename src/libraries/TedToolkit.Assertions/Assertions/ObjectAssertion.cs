using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TedToolkit.Assertions.Utils;
using TedToolkit.Assertions.AssertionItems;
using TedToolkit.Assertions.Constraints;
using TedToolkit.Assertions.Execution;
using TedToolkit.Assertions.Resources;

namespace TedToolkit.Assertions.Assertions;

/// <summary>
///     Just the Assertion
/// </summary>
/// <typeparam name="TValue"></typeparam>
public sealed class ObjectAssertion<TValue> : IAssertion
{
    private readonly DateTimeOffset _createTime;
    private readonly bool _isValid;
    private readonly List<AssertionItem> _items = [];
    private readonly AssertionScope _scope;
    private readonly AssertionType _type;
    private bool _reversed;

    internal ObjectAssertion(TValue subject, string valueName, AssertionType type, CallerInfo callerInfo,
        bool isValid = true)
        : this(subject, valueName, type, DateTimeOffset.Now, AssertionScope.Current, callerInfo, isValid)
    {
    }

    private ObjectAssertion(TValue subject, string valueName, AssertionType type, DateTimeOffset createTime,
        AssertionScope scope, CallerInfo callerInfo, bool isValid)
    {
        Subject = subject;
        SubjectName = string.IsNullOrEmpty(valueName) ? "Unknown" : valueName;
        _type = type;
        _createTime = createTime;
        _scope = scope;
        _isValid = isValid;
        _scope.AddAssertion(this);
        CallerInfo = callerInfo;
    }

    /// <summary>
    ///     The subject.
    /// </summary>
    public TValue Subject { get; }

    /// <summary>
    ///     The subject name
    /// </summary>
    public string SubjectName { get; }

    /// <summary>
    ///     Not
    /// </summary>
    /// <returns></returns>
    public ObjectAssertion<TValue> Not
    {
        get
        {
            _reversed = !_reversed;
            return this;
        }
    }

    /// <inheritdoc />
    IReadOnlyList<AssertionItem> IAssertion.Items => _items;

    /// <inheritdoc />
    AssertionType IAssertion.Type => _type;

    public CallerInfo CallerInfo { get; }

    /// <inheritdoc />
    DateTimeOffset IAssertion.CreatedTime => _createTime;

    internal ObjectAssertion<TValue> Duplicate(AssertionType type)
    {
        return type == _type
            ? this
            : new ObjectAssertion<TValue>(Subject, SubjectName, type, _createTime, _scope, CallerInfo, _isValid);
    }


    #region Match

    /// <summary>
    ///     Match the method
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public AndConstraint<TValue> Match(Expression<Func<TValue, bool>> predicate,
        AssertionParams? assertionParams = null)
    {
        return AssertCheck(predicate.Compile()(Subject),
            AssertionItemType.Match,
            new AssertMessage(AssertionLocalization.MatchAssertion, new Argument("Expression", predicate.Body)),
            assertionParams);
    }

    #endregion

    #region Range

    /// <summary>
    ///     Should be in range.
    /// </summary>
    /// <param name="minimumValue"></param>
    /// <param name="maximumValue"></param>
    /// <param name="comparer"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public AndConstraint<TValue> BeInRange(TValue minimumValue, TValue maximumValue,
        IComparer<TValue>? comparer = null,
        AssertionParams? assertionParams = null)
    {
        var realComparer = comparer ?? Comparer<TValue>.Default;

        return AssertCheck(
            realComparer.Compare(Subject, minimumValue) >= 0 && realComparer.Compare(Subject, maximumValue) <= 0,
            AssertionItemType.Range,
            new AssertMessage(AssertionLocalization.RangeAssertion, new Argument("MinimumValue", minimumValue),
                new Argument("MaximumValue", maximumValue)),
            assertionParams);
    }

    #endregion

    #region Null

    /// <summary>
    ///     The item is type of.
    /// </summary>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public AndConstraint<TValue> BeNull(
        AssertionParams? assertionParams = null)
    {
        return AssertCheck(Subject is null,
            AssertionItemType.Null, AssertionLocalization.NullAssertion,
            assertionParams);
    }

    #endregion

    #region Comparison

    /// <summary>
    ///     Should be greater.
    /// </summary>
    /// <param name="comparedValue"></param>
    /// <param name="comparer"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public AndConstraint<TValue> BeGreaterThanOrEqualTo(TValue comparedValue,
        IComparer<TValue>? comparer = null,
        AssertionParams? assertionParams = null)
    {
        var realComparer = comparer ?? Comparer<TValue>.Default;

        return AssertCheck(realComparer.Compare(Subject, comparedValue) >= 0,
            AssertionItemType.Comparison,
            new AssertMessage(AssertionLocalization.GreaterOrEqualAssertion,
                new Argument("ComparedValue", comparedValue)), assertionParams);
    }

    /// <summary>
    ///     Should be greater.
    /// </summary>
    /// <param name="comparedValue"></param>
    /// <param name="comparer"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public AndConstraint<TValue> BeGreaterThan(TValue comparedValue,
        IComparer<TValue>? comparer = null,
        AssertionParams? assertionParams = null)
    {
        var realComparer = comparer ?? Comparer<TValue>.Default;

        return AssertCheck(realComparer.Compare(Subject, comparedValue) > 0,
            AssertionItemType.Comparison,
            new AssertMessage(AssertionLocalization.GreaterAssertion, new Argument("ComparedValue", comparedValue)),
            assertionParams);
    }

    /// <summary>
    ///     Less or equal to
    /// </summary>
    /// <param name="comparedValue"></param>
    /// <param name="comparer"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public AndConstraint<TValue> BeLessThanOrEqualTo(TValue comparedValue,
        IComparer<TValue>? comparer = null,
        AssertionParams? assertionParams = null)
    {
        var realComparer = comparer ?? Comparer<TValue>.Default;

        return AssertCheck(realComparer.Compare(Subject, comparedValue) <= 0,
            AssertionItemType.Comparison,
            new AssertMessage(AssertionLocalization.LessOrEqualAssertion, new Argument("ComparedValue", comparedValue)),
            assertionParams);
    }

    /// <summary>
    ///     Should be less than.
    /// </summary>
    /// <param name="comparedValue"></param>
    /// <param name="comparer"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public AndConstraint<TValue> BeLessThan(TValue comparedValue, IComparer<TValue>? comparer = null,
        AssertionParams? assertionParams = null)
    {
        var realComparer = comparer ?? Comparer<TValue>.Default;
        return AssertCheck(realComparer.Compare(Subject, comparedValue) < 0,
            AssertionItemType.Comparison,
            new AssertMessage(AssertionLocalization.LessAssertion, new Argument("ComparedValue", comparedValue)),
            assertionParams);
    }

    #endregion

    #region Equality

    /// <summary>
    ///     The item should be.
    /// </summary>
    /// <param name="expectedValue"></param>
    /// <param name="equalityComparer"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public AndConstraint<TValue> Be(TValue expectedValue,
        IComparer<TValue>? equalityComparer = null,
        AssertionParams? assertionParams = null)
    {
        var realComparer = equalityComparer ?? Comparer<TValue>.Default;

        return AssertCheck(realComparer.Compare(Subject, expectedValue) == 0,
            AssertionItemType.Equality,
            new AssertMessage(AssertionLocalization.EqualityAssertion, new Argument("ExpectedValue", expectedValue)),
            assertionParams);
    }

    /// <summary>
    ///     The item should be.
    /// </summary>
    /// <param name="expectedValue"></param>
    /// <param name="equalityComparer"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public AndConstraint<TValue> Be(TValue expectedValue,
        IEqualityComparer<TValue>? equalityComparer = null,
        AssertionParams? assertionParams = null)
    {
        var comparer = equalityComparer ?? EqualityComparer<TValue>.Default;

        return AssertCheck(comparer.Equals(Subject, expectedValue),
            AssertionItemType.Equality,
            new AssertMessage(AssertionLocalization.EqualityAssertion, new Argument("ExpectedValue", expectedValue)),
            assertionParams);
    }

    /// <summary>
    ///     be one of.
    /// </summary>
    /// <param name="expectedValues"></param>
    /// <param name="equalityComparer"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public AndConstraint<TValue> BeOneOf(IEnumerable<TValue> expectedValues,
        IEqualityComparer<TValue>? equalityComparer = null,
        AssertionParams? assertionParams = null)
    {
        var comparer = equalityComparer ?? EqualityComparer<TValue>.Default;
        var values = expectedValues as TValue[] ?? expectedValues.ToArray();

        return AssertCheck(values.Contains(Subject, comparer),
            AssertionItemType.Equality,
            new AssertMessage(AssertionLocalization.OneOfAssertion, new Argument("ExpectedValues", values)),
            assertionParams);
    }

    #endregion

    #region Type

    /// <summary>
    ///     The item is type of.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public AndWhichConstraint<TValue, T?> BeAssignableTo<T>(AssertionParams? assertionParams = null)
    {
        return AssertCheck(() => Subject is T type ? type : default, string.Empty,
            Subject is T, AssertionItemType.DataType,
            new AssertMessage(AssertionLocalization.AssignableAssertion,
                new Argument("ValueType", Subject?.GetType().GetFullName()),
                new Argument("ExpectedType", typeof(T).GetFullName())), assertionParams);
    }

    /// <summary>
    ///     Be type of
    /// </summary>
    /// <param name="assertionParams"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public AndConstraint<TValue> BeTypeOf<T>(AssertionParams? assertionParams = null)
    {
        return BeTypeOf(typeof(T), assertionParams);
    }

    /// <summary>
    /// </summary>
    /// <param name="expectedType"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public AndConstraint<TValue> BeTypeOf(Type expectedType, AssertionParams? assertionParams = null)
    {
        var subjectType = Subject?.GetType();
        var succeed = expectedType.IsGenericTypeDefinition && (subjectType?.IsGenericType ?? false)
            ? subjectType.GetGenericTypeDefinition() == expectedType
            : subjectType == expectedType;

        return AssertCheck(succeed, AssertionItemType.DataType,
            new AssertMessage(AssertionLocalization.TypeAssertion,
                new Argument("ValueType", Subject?.GetType().GetFullName()),
                new Argument("ExpectedType", expectedType.GetFullName())), assertionParams);
    }

    #endregion

    #region Assertion Helper Methods

    /// <summary>
    /// </summary>
    /// <param name="succeed"></param>
    /// <param name="assertionItemType"></param>
    /// <param name="message"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public AndConstraint<TValue> AssertCheck(bool succeed, AssertionItemType assertionItemType,
        AssertMessage message, AssertionParams? assertionParams)
    {
        return AssertCheck(new AndConstraint<TValue>(this), succeed, assertionItemType, message, assertionParams);
    }

    /// <summary>
    ///     Just the check
    /// </summary>
    /// <param name="resultGetter"></param>
    /// <param name="suffix"></param>
    /// <param name="succeed"></param>
    /// <param name="assertionItemType"></param>
    /// <param name="message"></param>
    /// <param name="assertionParams"></param>
    /// <typeparam name="TMatchedElement"></typeparam>
    /// <returns></returns>
    public AndWhichConstraint<TValue, TMatchedElement> AssertCheck<TMatchedElement>(
        Func<TMatchedElement> resultGetter, string suffix, bool succeed, AssertionItemType assertionItemType,
        AssertMessage message, AssertionParams? assertionParams)
    {
        return AssertCheck(new AndWhichConstraint<TValue, TMatchedElement>(this, resultGetter, suffix), succeed,
            assertionItemType, message, assertionParams);
    }

    private TResult AssertCheck<TResult>(TResult result, bool succeed, AssertionItemType assertionItemType,
        AssertMessage message, AssertionParams? assertionParams)
        where TResult : IAndConstraint
    {
        if (IsSucceed(succeed, out var reverse)) return result;
        var msg = AddMoreArguments(message, assertionParams?.Reason ?? string.Empty, reverse);
        result.FailureReturnValues = AddAssertionItem(assertionItemType, msg, assertionParams?.Tag);
        return result;
    }

    private AssertMessage AddMoreArguments(AssertMessage message, string reason, bool reverse)
    {
        var basicFormat = message.StructuredFormat;
        List<Argument> basicArguments =
        [
            new(nameof(Subject), Subject),
            new("Subjects", Subject?.GetObjectString()),
            new(nameof(SubjectName), SubjectName),
            new(nameof(AssertionType), _type switch
            {
                AssertionType.Must => AssertionLocalization.Must,
                AssertionType.Should => AssertionLocalization.Should,
                AssertionType.Could => AssertionLocalization.Could,
                _ => "Unknown"
            }),
            new("Not", reverse ? AssertionLocalization.Not : string.Empty),
            ..message.StructuredArguments
        ];

        if (!string.IsNullOrWhiteSpace(reason))
        {
            basicFormat += $"\n{string.Format(AssertionLocalization.Reason, "{Reason}")}";
            basicArguments.Add(new Argument("Reason", reason));
        }

        return new AssertMessage(basicFormat, basicArguments.ToArray());
    }

    private bool IsSucceed(bool succeed, out bool reverse)
    {
        if (!_isValid)
        {
            reverse = false;
            return true;
        }

        try
        {
            reverse = _reversed;
            return _reversed ? !succeed : succeed;
        }
        finally
        {
            _reversed = false;
        }
    }

    private IDictionary<IAssertionStrategy, object> AddAssertionItem(AssertionItemType type, AssertMessage message,
        object? tag)
    {
        var item = new AssertionItem(type, message, DateTimeOffset.Now, tag);
        _items.Add(item);
        return _scope.PushAssertionItem(item, _type, tag, CallerInfo);
    }

    #endregion
}