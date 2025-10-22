namespace TedToolkit.Fluent;

/// <summary>
///     Do Result
/// </summary>
/// <param name="fluent"></param>
/// <typeparam name="TValue"></typeparam>
public abstract class DoResultBase<TValue>(Fluent<TValue> fluent)
{
    private protected Fluent<TValue> Fluent { get; } = fluent;

    /// <summary>
    ///     Did it run the method.
    /// </summary>
    public abstract bool DidIt { get; }

    /// <inheritdoc cref="Fluent{TTarget}.Result" />
    public TValue Result => Continue.Result;

    /// <summary>
    ///     Continue to do sth.
    /// </summary>
    public Fluent<TValue> Continue => Fluent;

    /// <summary>
    ///     Convert it to bool.
    /// </summary>
    /// <param name="doResult"></param>
    /// <returns></returns>
    public static implicit operator bool(DoResultBase<TValue> doResult)
    {
        return doResult.DidIt;
    }
}

/// <inheritdoc />
public class DoResult<TValue>(Fluent<TValue> fluent, Lazy<bool> lazy) : DoResultBase<TValue>(fluent)
{
    public DoResult(Fluent<TValue> fluent) : this(fluent, new Lazy<bool>(() => true))
    {
    }

    /// <inheritdoc />
    public override bool DidIt => lazy.Value;
}

/// <inheritdoc />
public sealed class DoResult<TValue, TResult>(Fluent<TValue> fluent, Lazy<(bool, TResult)> lazy)
    : DoResultBase<TValue>(fluent)
{
    public DoResult(Fluent<TValue> fluent, TResult result)
        : this(fluent, new Lazy<(bool, TResult)>(() => (true, result)))
    {
    }

    /// <inheritdoc />
    public override bool DidIt => lazy.Value.Item1;

    /// <summary>
    ///     The return value
    /// </summary>
    public TResult ReturnValue => lazy.Value.Item2;

    /// <summary>
    ///     Continue when can continue.
    /// </summary>
    /// <param name="canContinue"></param>
    /// <param name="type"></param>
    /// <returns>fluent type</returns>
    public Fluent<TValue> ContinueWhen(Predicate<TResult> canContinue, FluentType? type = null)
    {
        return Fluent.AddCondition(() => canContinue(lazy.Value.Item2), type);
    }

    /// <summary>
    ///     Convert to the result.
    /// </summary>
    /// <param name="doResult"></param>
    /// <returns></returns>
    public static implicit operator TResult(DoResult<TValue, TResult> doResult)
    {
        return doResult.ReturnValue;
    }
}