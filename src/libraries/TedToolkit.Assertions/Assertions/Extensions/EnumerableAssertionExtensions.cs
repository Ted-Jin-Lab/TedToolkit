using System.Collections;
using System.Linq.Expressions;
using TedToolkit.Assertions.AssertionItems;
using TedToolkit.Assertions.Constraints;
using TedToolkit.Assertions.Resources;

namespace TedToolkit.Assertions.Assertions.Extensions;

/// <summary>
///     For the enumerable things
/// </summary>
public static class EnumerableAssertionExtensions
{
    #region Items

    /// <summary>
    /// All satisfy
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="predicate"></param>
    /// <param name="assertionParams"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public static AndConstraint<TValue>
        AllSatisfy<TValue, TItem>(this ObjectAssertion<TValue> assertion,
            Expression<Func<TItem, bool>> predicate,
            AssertionParams? assertionParams = null)
        where TValue : IEnumerable<TItem>
    {
        List<int> indexes = [];
        var index = 0;
        var func = predicate.Compile();
        foreach (var item in assertion.Subject)
        {
            try
            {
                if (!func(item)) indexes.Add(index);
            }
            finally
            {
                index++;
            }
        }

        return assertion.AssertCheck(assertion.Subject.All(predicate.Compile()),
            AssertionItemType.AllSatisfy,
            new AssertMessage(AssertionLocalization.AllSatisfyAssertion,
                new Argument("Indexes", indexes), new Argument("Expression", predicate.Body)),
            assertionParams);
    }

    #endregion

    #region ItemEquality

    /// <summary>
    ///     Get if the item contain single.
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="predicate"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public static AndWhichConstraint<TValue, TItem>
        ContainSingle<TValue, TItem>(this ObjectAssertion<TValue> assertion,
            Expression<Func<TItem, bool>> predicate,
            AssertionParams? assertionParams = null)
        where TValue : IEnumerable<TItem>
    {
        var func = predicate.Compile();
        var items = assertion.Subject.Where(func).ToArray();

        return assertion.AssertCheck(() => items.First(), $".SingleBy[{predicate.Body}]", items.Length is 1,
            AssertionItemType.ItemEquality,
            new AssertMessage(AssertionLocalization.ContainSingleExpressionAssertion,
                new Argument("MatchedCount", items.Length), new Argument("Expression", predicate.Body)),
            assertionParams);
    }

    /// <summary>
    ///     Get if the item contain single.
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="expectedValue"></param>
    /// <param name="equalityComparer"></param>
    /// <param name="assertionParams"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public static AndWhichConstraint<TValue, TItem>
        ContainSingle<TValue, TItem>(this ObjectAssertion<TValue> assertion,
            TItem expectedValue,
            IEqualityComparer<TItem>? equalityComparer = null,
            AssertionParams? assertionParams = null)
        where TValue : IEnumerable<TItem>
    {
        var comparer = equalityComparer ?? EqualityComparer<TItem>.Default;

        var items = assertion.Subject.Where(item => comparer.Equals(item, expectedValue)).ToArray();

        return assertion.AssertCheck(() => items.First(), $".SingleBy<{expectedValue}>", items.Length is 1,
            AssertionItemType.ItemEquality,
            new AssertMessage(AssertionLocalization.ContainSingleAssertion, new Argument("MatchedCount", items.Length),
                new Argument("ExpectedValue", expectedValue)), assertionParams);
    }

    /// <summary>
    ///     Contains items
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="expectedValue"></param>
    /// <param name="equalityComparer"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public static AndConstraint<TValue> Contain<TValue, TItem>(this ObjectAssertion<TValue> assertion,
        TItem expectedValue,
        IEqualityComparer<TItem>? equalityComparer = null,
        AssertionParams? assertionParams = null)
        where TValue : IEnumerable<TItem>
    {
        var comparer = equalityComparer ?? EqualityComparer<TItem>.Default;

        return assertion.AssertCheck(assertion.Subject.Contains(expectedValue, comparer),
            AssertionItemType.ItemEquality,
            new AssertMessage(AssertionLocalization.ContainAssertion, new Argument("ExpectedValue", expectedValue)),
            assertionParams);
    }

    /// <summary>
    ///     Contains items
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="predicate"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public static AndConstraint<TValue> Contain<TValue, TItem>(this ObjectAssertion<TValue> assertion,
        Expression<Func<TItem, bool>> predicate,
        AssertionParams? assertionParams = null)
        where TValue : IEnumerable<TItem>
    {
        var func = predicate.Compile();
        return assertion.AssertCheck(assertion.Subject.Any(func), AssertionItemType.ItemEquality,
            new AssertMessage(AssertionLocalization.ContainExpressionAssertion,
                new Argument("Expression", predicate.Body)), assertionParams);
    }

    /// <summary>
    ///     contain items
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="expectedValues"></param>
    /// <param name="equalityComparer"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public static AndConstraint<TValue> Contain<TValue, TItem>(this ObjectAssertion<TValue> assertion,
        IEnumerable<TItem> expectedValues,
        IEqualityComparer<TItem>? equalityComparer = null,
        AssertionParams? assertionParams = null)
        where TValue : IEnumerable<TItem>
    {
        var comparer = equalityComparer ?? EqualityComparer<TItem>.Default;
        var values = expectedValues as TItem[] ?? expectedValues.ToArray();

        return assertion.AssertCheck(values.Except(assertion.Subject, comparer).Any(), AssertionItemType.ItemEquality,
            new AssertMessage(AssertionLocalization.ContainAssertion, new Argument("ExpectedValues", values)),
            assertionParams);
    }

    #endregion

    #region Item Count

    /// <summary>
    ///     be empty
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="assertionParams"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static AndConstraint<TValue> BeEmpty<TValue>(this ObjectAssertion<TValue> assertion,
        AssertionParams? assertionParams = null)
        where TValue : IEnumerable
    {
        return assertion.AssertCheck(GetCount(assertion.Subject) > 0, AssertionItemType.Empty,
            AssertionLocalization.EmptyAssertion,
            assertionParams);
    }

    /// <summary>
    ///     have the count.
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="expectedCount"></param>
    /// <param name="assertionParams"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static AndConstraint<TValue> HaveCount<TValue>(this ObjectAssertion<TValue> assertion, int expectedCount,
        AssertionParams? assertionParams = null)
        where TValue : IEnumerable
    {
        var actualCount = GetCount(assertion.Subject);


        return assertion.AssertCheck(actualCount == expectedCount, AssertionItemType.ItemCount,
            new AssertMessage(AssertionLocalization.CountAssertion, new Argument("ActualCount", actualCount),
                new Argument("ExpectedCount", expectedCount)), assertionParams);
    }

    /// <summary>
    ///     greater than.
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="expectedCount"></param>
    /// <param name="assertionParams"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static AndConstraint<TValue> HaveCountGreaterThan<TValue>(this ObjectAssertion<TValue> assertion,
        int expectedCount,
        AssertionParams? assertionParams = null)
        where TValue : IEnumerable
    {
        var actualCount = GetCount(assertion.Subject);

        return assertion.AssertCheck(actualCount > expectedCount, AssertionItemType.ItemCount,
            new AssertMessage(AssertionLocalization.CountGreaterAssertion, new Argument("ActualCount", actualCount),
                new Argument("ExpectedCount", expectedCount)),
            assertionParams);
    }


    /// <summary>
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="expectedCount"></param>
    /// <param name="assertionParams"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static AndConstraint<TValue> HaveCountGreaterThanOrEqualTo<TValue>(this ObjectAssertion<TValue> assertion,
        int expectedCount,
        AssertionParams? assertionParams = null)
        where TValue : IEnumerable

    {
        var actualCount = GetCount(assertion.Subject);

        return assertion.AssertCheck(actualCount >= expectedCount, AssertionItemType.ItemCount,
            new AssertMessage(AssertionLocalization.CountGreaterOrEqualAssertion,
                new Argument("ActualCount", actualCount), new Argument("ExpectedCount", expectedCount)),
            assertionParams);
    }

    /// <summary>
    ///     The count less than.
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="expectedCount"></param>
    /// <param name="assertionParams"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static AndConstraint<TValue> HaveCountLessThan<TValue>(this ObjectAssertion<TValue> assertion,
        int expectedCount,
        AssertionParams? assertionParams = null)
        where TValue : IEnumerable

    {
        var actualCount = GetCount(assertion.Subject);

        return assertion.AssertCheck(actualCount < expectedCount, AssertionItemType.ItemCount,
            new AssertMessage(AssertionLocalization.CountLessAssertion, new Argument("ActualCount", actualCount),
                new Argument("ExpectedCount", expectedCount)),
            assertionParams);
    }

    /// <summary>
    ///     The count less than or equal to.
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="expectedCount"></param>
    /// <param name="assertionParams"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static AndConstraint<TValue> HaveCountLessThanOrEqualTo<TValue>(this ObjectAssertion<TValue> assertion,
        int expectedCount,
        AssertionParams? assertionParams = null)
        where TValue : IEnumerable
    {
        var actualCount = GetCount(assertion.Subject);
        return assertion.AssertCheck(actualCount <= expectedCount, AssertionItemType.ItemCount,
            new AssertMessage(AssertionLocalization.CountLessOrEqualAssertion, new Argument("ActualCount", actualCount),
                new Argument("ExpectedCount", expectedCount)),
            assertionParams);
    }

    private static int GetCount(IEnumerable enumerable)
    {
        return enumerable switch
        {
            ICollection collection => collection.Count,
            string str => str.Length,
            _ => enumerable.Cast<object?>().Count()
        };
    }

    #endregion
}