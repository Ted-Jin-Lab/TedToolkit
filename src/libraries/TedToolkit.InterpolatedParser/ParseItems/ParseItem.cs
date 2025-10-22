using System.Runtime.CompilerServices;
using TedToolkit.InterpolatedParser.Options;

namespace TedToolkit.InterpolatedParser.ParseItems;

/// <inheritdoc />
public abstract unsafe class ParseItem<T> : IParseItem
{
    private readonly void* _ptr;

    private protected ParseItem(in T value, PreModifyOptions preModify)
    {
        ref var t = ref Unsafe.AsRef(in value);
        _ptr = Unsafe.AsPointer(ref t);
        PreModification = preModify;
    }

    /// <inheritdoc />
    public PreModifyOptions PreModification { get; }

    private protected void SetValue(in T value)
    {
        Unsafe.AsRef<T>(_ptr) = value;
    }
}