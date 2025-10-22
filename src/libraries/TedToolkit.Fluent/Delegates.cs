namespace TedToolkit.Fluent;

/// <summary>
///     The delegate to modify the value
/// </summary>
/// <typeparam name="T"></typeparam>
public delegate T ModifyDelegate<T>(T input);

/// <summary>
///     The modify property delegate
/// </summary>
/// <typeparam name="T"></typeparam>
public delegate void PropertyDelegate<T>(ref T target);

/// <summary>
///     Method delegate
/// </summary>
/// <typeparam name="TTarget"></typeparam>
/// <typeparam name="TResult"></typeparam>
public delegate TResult MethodDelegate<TTarget, out TResult>(ref TTarget target);

/// <summary>
/// </summary>
/// <typeparam name="TTarget"></typeparam>
public delegate void MethodDelegate<TTarget>(ref TTarget target);