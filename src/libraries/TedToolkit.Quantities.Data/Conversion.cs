using Newtonsoft.Json;
using PeterO.Numbers;

namespace TedToolkit.Quantities.Data;

public readonly record struct Conversion(ERational Multiplier, ERational Offset)
{
    [JsonIgnore] public bool IsValid => !Multiplier.IsInfinity() && !Multiplier.IsZero && !Offset.IsInfinity();
    public static Conversion Unit { get; } = new(ERational.One, ERational.Zero);

    public Conversion? TransformTo(Conversion? unit)
    {
        if (unit is null) return null;
        var multiplier = Multiplier / unit.Value.Multiplier;
        var offset = (Offset - unit.Value.Offset) / unit.Value.Multiplier;
        return new Conversion(multiplier, offset);
    }

    public Conversion? Pow(int exponent)
    {
        switch (exponent)
        {
            case 0:
                return Unit;
            case 1:
                return this;
            case -1:
                var multiplier = ERational.One / Multiplier;
                var offset = -Offset / Multiplier;
                return new Conversion(multiplier, offset);
        }

        if (Multiplier.IsZero || Offset.IsZero)
        {
            return new Conversion(Pow(Multiplier, exponent), Pow(Offset, exponent));
        }

        //TODO: Can't convert.
        return null;
    }

    private static ERational Pow(ERational rational, int exponent)
    {
        if (exponent == 0) return ERational.One;
        var one = rational;
        for (var i = 1; i < Math.Abs(exponent); i++)
        {
            rational *= one;
        }

        if (exponent < 0) rational = ERational.One / rational;
        return rational;
    }

    public Conversion? Merge(Conversion? other)
    {
        if (other is null) return null;
        if (Multiplier.IsZero || Offset.IsZero)
        {
            return new Conversion(Multiplier * other.Value.Multiplier,
                Offset * other.Value.Offset);
        }

        //TODO: Can't convert.
        return null;
    }
}