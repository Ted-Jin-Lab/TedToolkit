namespace TedToolkit.Scopes;

/// <summary>
/// Scope base
/// </summary>
/// <typeparam name="TScope"></typeparam>
public abstract class ScopeBase<TScope> :
    IDisposable
    where TScope : ScopeBase<TScope>
{
    private static readonly AsyncLocal<TScope?> CurrentScope = new();
    private readonly TScope? _parent;

    /// <summary>
    ///  Current Value
    /// </summary>
    public static TScope? Current => CurrentScope.Value;

    /// <summary>
    /// 
    /// </summary>
    protected ScopeBase()
    {
        _parent = CurrentScope.Value;
        CurrentScope.Value = (TScope)this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        CurrentScope.Value = _parent;
        OnDispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// On Dispose
    /// </summary>
    protected virtual void OnDispose()
    {
        
    }
}