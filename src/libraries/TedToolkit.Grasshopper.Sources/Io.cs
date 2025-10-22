namespace TedToolkit.Grasshopper;

/// <summary>
///     For the case you want more data
/// </summary>
/// <typeparam name="T"></typeparam>
public struct Io<T>
{
    /// <summary>
    ///     If it was got.
    /// </summary>
    public bool HasGot { get; }

    /// <summary>
    ///     The index in the param.
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///     The value of it
    /// </summary>
    public T Value { get; set; }

    internal Io(bool got, int index, T value)
    {
        HasGot = got;
        Index = index;
        Value = value;
    }

    public static implicit operator T(Io<T> t)
    {
        return t.Value;
    }
}