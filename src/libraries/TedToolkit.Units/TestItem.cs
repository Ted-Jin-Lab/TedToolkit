using System.Globalization;

namespace TedToolkit.Units;

internal sealed class UnitsAttribute<TData> : Attribute
{
    public TData What { get; init; } = default;
};

[Units<double>(What = default)]
class MyClass
{
}