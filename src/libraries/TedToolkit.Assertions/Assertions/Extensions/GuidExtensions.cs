using TedToolkit.Assertions.AssertionItems;
using TedToolkit.Assertions.Constraints;
using TedToolkit.Assertions.Resources;

namespace TedToolkit.Assertions.Assertions.Extensions;

/// <summary>
///     Extension for the guid
/// </summary>
public static class GuidExtensions
{
    /// <summary>
    ///     Empty/
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="assertionParams"></param>
    /// <returns></returns>
    public static AndConstraint<Guid> BeEmpty(this ObjectAssertion<Guid> assertion,
        AssertionParams? assertionParams = null)
    {
        return assertion.AssertCheck(assertion.Subject == Guid.Empty, AssertionItemType.Empty,
            new AssertMessage(AssertionLocalization.EmptyAssertion),
            assertionParams);
    }
}