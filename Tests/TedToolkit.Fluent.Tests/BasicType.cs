namespace TedToolkit.Fluent.Tests;

public class BasicType<TAa, TBb>
    where TAa : class, new()
    where TBb : struct
{
    public string Name { get; set; } = string.Empty;
    public TAa DataA { get; set; } = null!;

    public TBb AMethod<T>(ref T type, out int sth) where T : unmanaged
    {
        Name = typeof(T).Name;
        sth = 1;
        return default;
    }
}

public static class BasicTypeExtensions
{
    public static TBb AMethod<TAa, TBb, T>(this BasicType<TAa, TBb> item, ref T type, out int sth)
        where TAa : class, new()
        where TBb : struct
    {
        item.Name = typeof(T).Name;
        sth = 1;
        return default;
    }

    public static TBb BMethod<TAa, TBb, T>(this BasicType<TAa, TBb> item, ref T type, out int sth)
        where TAa : class, new()
        where TBb : struct
    {
        item.Name = typeof(T).Name;
        sth = 1;
        return default;
    }
}