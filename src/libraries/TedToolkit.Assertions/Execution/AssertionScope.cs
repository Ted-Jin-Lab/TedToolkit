using TedToolkit.Assertions.AssertionItems;
using TedToolkit.Assertions.Assertions;
using TedToolkit.Scopes;

namespace TedToolkit.Assertions.Execution;

/// <summary>
/// </summary>
public class AssertionScope : ScopeBase<AssertionScope>
{
    private readonly List<IAssertion> _assertions = [];
    private readonly MergedAssertionStrategy _strategy;

    private bool _handledFailure;

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="tag"></param>
    public AssertionScope(string context = "", object? tag = null)
        : this(AssertionService.MergedScopeStrategy, context, tag)
    {
    }

    internal AssertionScope(MergedAssertionStrategy strategy, string context = "", object? tag = null)
    {
        _strategy = strategy;
        Context = context;
        Tag = tag;
    }

    /// <summary>
    ///     Add the assertion.
    /// </summary>
    /// <param name="strategy"></param>
    /// <param name="context"></param>
    /// <param name="tag"></param>
    public AssertionScope(IAssertionStrategy strategy, string context = "", object? tag = null)
        : this(new MergedAssertionStrategy(strategy), context, tag)
    {
    }

    /// <summary>
    ///     The Context.
    /// </summary>
    public string Context { get; }

    /// <summary>
    ///     The tag to show.
    /// </summary>
    public object? Tag { get; }

    protected override void OnDispose()
    {
        if (!_handledFailure) HandleFailure();
    }

    /// <summary>
    ///     Manually handle failure
    /// </summary>
    /// <returns></returns>
    public IDictionary<IAssertionStrategy, object> HandleFailure()
    {
        _handledFailure = true;
        return _strategy.HandleFailure(this, _assertions);
    }

    internal void AddAssertion(IAssertion assertion)
    {
        _assertions.Add(assertion);
    }

    internal IDictionary<IAssertionStrategy, object> PushAssertionItem(AssertionItem assertionItem,
        AssertionType assertionType, object? tag, CallerInfo callerInfo)
    {
        return _strategy.HandleFailure(this, assertionType, assertionItem, tag, callerInfo);
    }
}