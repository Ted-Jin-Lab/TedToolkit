namespace TedToolkit.Units;

public readonly struct Length<TData> where TData : struct
{
    private readonly TData _value;

    public Length(TData value, LengthUnit unit)
    {
        
    }

    public TData As(LengthUnit unit)
    {
        return _value;
    }
}