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
    private readonly bool _isPush;

    private bool _handledFailure;

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="tag"></param>
    /// <param name="isPush">Assert when push</param>
    public AssertionScope(string context = "", object? tag = null, bool isPush = false)
        : this(AssertionService.MergedStrategy, context, tag, isPush) //TODO: Type
    {
    }

    internal AssertionScope(MergedAssertionStrategy strategy, string context, object? tag, bool isPush)
    {
        _strategy = strategy;
        Context = context;
        Tag = tag;
        _isPush = isPush;
    }

    /// <summary>
    ///     Add the assertion.
    /// </summary>
    /// <param name="strategy"></param>
    /// <param name="context"></param>
    /// <param name="tag"></param>
    /// <param name="isPush">Assert when push</param>
    public AssertionScope(IAssertionStrategy strategy, string context = "", object? tag = null, bool isPush = false)
        : this(new MergedAssertionStrategy(strategy), context, tag, isPush)
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

    /// <inheritdoc />
    protected override void OnDispose()
    {
        if (!_handledFailure) HandleFailure();
    }

    /// <summary>
    ///     Manually handle failure
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<IAssertionStrategy, object> HandleFailure()
    {
        _handledFailure = true;
        if (_isPush) return new Dictionary<IAssertionStrategy, object>();
        return _strategy.HandleFailure(this, _assertions);
    }

    internal void AddAssertion(IAssertion assertion)
    {
        _assertions.Add(assertion);
    }

    private Func<IReadOnlyDictionary<IAssertionStrategy, object>>? _lastAssertion;

    internal IReadOnlyDictionary<IAssertionStrategy, object> GetLastItem()
    {
        return _lastAssertion?.Invoke() ?? throw new InvalidOperationException();
    }

    internal IReadOnlyDictionary<IAssertionStrategy, object> PushAssertionItem(AssertionItem assertionItem,
        AssertionType assertionType, object? tag, CallerInfo callerInfo)
    {
        if (!_isPush)
        {
            _lastAssertion = () => _strategy.HandleFailure(this, assertionType, assertionItem, tag, callerInfo);
            return new Dictionary<IAssertionStrategy, object>();
        }

        return _strategy.HandleFailure(this, assertionType, assertionItem, tag, callerInfo);
    }
}