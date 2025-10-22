using TedToolkit.Assertions.AssertionItems;
using TedToolkit.Assertions.Assertions;

namespace TedToolkit.Assertions.Execution;

internal class MergedAssertionStrategy(params IAssertionStrategy[] strategies)
{
    public IDictionary<IAssertionStrategy, object> HandleFailure(AssertionScope scope,
        IReadOnlyList<IAssertion> assertions)
    {
        return strategies.Select(strategy => (strategy, strategy.HandleFailure(scope, assertions)))
            .Where(pair => pair.Item2 is not null)
            .ToDictionary(pair => pair.strategy, pair => pair.Item2!);
    }

    public IDictionary<IAssertionStrategy, object> HandleFailure(AssertionScope scope, AssertionType assertionType,
        AssertionItem assertion,
        object? tag, CallerInfo callerInfo)
    {
        return strategies.Select(strategy =>
                (strategy, strategy.HandleFailure(scope, assertionType, assertion, tag, callerInfo)))
            .Where(pair => pair.Item2 is not null)
            .ToDictionary(pair => pair.strategy, pair => pair.Item2!);
    }
}