using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using TedToolkit.Assertions.AssertionItems;
using TedToolkit.Assertions.Constraints;
using TedToolkit.Assertions.Resources;

namespace TedToolkit.Assertions.Assertions.Extensions;

/// <summary>
///     For string extensions.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     The expression
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="regularExpression"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public static AndConstraint<string> MatchRegex(this ObjectAssertion<string> assertion,
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string regularExpression,
        AssertionParams? assertionParams = null)
    {
        var regex = new Regex(regularExpression);

        return assertion.AssertCheck(
            regex.IsMatch(assertion.Subject), AssertionItemType.Match,
            new AssertMessage(AssertionLocalization.MatchAssertion, new Argument("Expression", regularExpression)),
            assertionParams);
    }

    /// <summary>
    ///     match regex.
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="regularExpression"></param>
    /// <param name="expectedMatchCount"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public static AndConstraint<string> MatchRegex(this ObjectAssertion<string> assertion,
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string regularExpression,
        int expectedMatchCount,
        AssertionParams? assertionParams = null)
    {
        var actualMatchCount = new Regex(regularExpression).Matches(assertion.Subject).Count;
        return assertion.AssertCheck(actualMatchCount == expectedMatchCount, AssertionItemType.Match,
            new AssertMessage(AssertionLocalization.MatchCountAssertion, new Argument("Expression", regularExpression),
                new Argument("ActualMatchCount", actualMatchCount),
                new Argument("ExpectedMatchCount", expectedMatchCount)), assertionParams);
    }
}