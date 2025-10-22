namespace TedToolkit.Fluent;

/// <summary>
///     The basic fluent item
/// </summary>
/// <typeparam name="TTarget"></typeparam>
public class Fluent<TTarget> : IDisposable
{
    private readonly Queue<Action> _actions = new();
    private readonly FluentType _type;
    private bool _canContinue = true;
    private TTarget _target;

    internal Fluent(in TTarget target, FluentType type)
    {
        _target = target;
        _type = type;
    }

    /// <summary>
    ///     Get the result of it, actually it is not necessary, because it already modified the original one for you.
    /// </summary>
    public ref TTarget Result => ref Execute();

    /// <inheritdoc />
    public void Dispose()
    {
        Execute();
        GC.SuppressFinalize(this);
    }

    public static implicit operator TTarget(Fluent<TTarget> fluent)
    {
        return fluent.Result;
    }

    private ref TTarget Execute()
    {
        while (_canContinue && _actions.Count > 0) _actions.Dequeue().Invoke();
        return ref _target;
    }

    internal Fluent<TTarget> AddCondition(Func<bool> condition, FluentType? type)
    {
        return AddAction(() => _canContinue = condition(), type);
    }

    /// <summary>
    ///     Continue when.
    /// </summary>
    /// <param name="canContinue"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Fluent<TTarget> ContinueWhen(Predicate<TTarget> canContinue, FluentType? type = null)
    {
        return AddAction(() => _canContinue = canContinue(_target), type);
    }

    /// <summary>
    ///     Add the property to this fluent
    /// </summary>
    /// <param name="property"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Fluent<TTarget> AddProperty(PropertyDelegate<TTarget> property, FluentType? type)
    {
        return AddAction(() => property(ref _target), type);
    }

    /// <summary>
    ///     Invoke the method you want.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="type"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public DoResult<TTarget, TResult> InvokeMethod<TResult>(MethodDelegate<TTarget, TResult> method, FluentType? type)
    {
        var lazy = InvokeMethod(b => b ? (b, method(ref _target)) : (b, default!));
        return new DoResult<TTarget, TResult>(AddAction(() => { _ = lazy.Value; }, type), lazy);
    }

    /// <summary>
    ///     Invokes.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public DoResult<TTarget> InvokeMethod(MethodDelegate<TTarget> method, FluentType? type)
    {
        var lazy = InvokeMethod(b =>
        {
            if (b) method(ref _target);
            return b;
        });
        return new DoResult<TTarget>(AddAction(() => { _ = lazy.Value; }, type), lazy);
    }

    private Lazy<T> InvokeMethod<T>(Func<bool, T> method)
    {
        var actions = _actions.ToArray();
        _actions.Clear();
        return new Lazy<T>(() =>
        {
            foreach (var action in actions)
                if (_canContinue) action();
                else return method(false);

            return method(true);
        });
    }

    private Fluent<TTarget> AddAction(Action action, FluentType? type)
    {
        type ??= _type;
        switch (type)
        {
            case FluentType.Immediate:
                Execute();
                if (_canContinue) action.Invoke();
                break;
            case FluentType.Lazy:
                _actions.Enqueue(action);
                break;
            default:
                throw new NotSupportedException($"Unsupported fluent type {type}");
        }

        return this;
    }
}