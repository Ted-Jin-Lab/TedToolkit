using TedToolkit.Assertions.AssertionItems;
using TedToolkit.Assertions.Assertions;

namespace TedToolkit.Assertions.Execution;

/// <summary>
/// </summary>
public class AssertionScope : IDisposable
{
    private static readonly AsyncLocal<AssertionScope?> CurrentScope = new();
    private readonly List<IAssertion> _assertions = [];
    private readonly AssertionScope? _parent;
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
        _parent = CurrentScope.Value;
        Context = context;
        Tag = tag;

        CurrentScope.Value = this;
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

    internal static AssertionScope Current
    {
        get => CurrentScope.Value ?? new AssertionScope(AssertionService.MergedPushStrategy);
        set => CurrentScope.Value = value;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_handledFailure) HandleFailure();

        CurrentScope.Value = _parent;
        GC.SuppressFinalize(this);
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