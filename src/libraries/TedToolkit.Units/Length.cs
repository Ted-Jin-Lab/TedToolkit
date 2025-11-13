namespace TedToolkit.Units;

public readonly partial struct Length
{
    private readonly double _value;
    public Length(double value, LengthUnit unit)
    {
        switch (unit)
        {
            case LengthUnit.Meter:
                _value = value;
                return;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
        }
    }

    public double As(LengthUnit unit)
    {
        return _value;
    }
}