// ReSharper disable once CheckNamespace

namespace TedToolkit.Fluent.Tests.Another;

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
}