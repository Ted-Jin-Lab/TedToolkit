using UnitsNet;
using UnitsNet.Units;

namespace ArchiToolkit.QuantExtensions;

public static class UnitsExtensions
{
    extension(Area area)
    {
        /// <inheritdoc cref="Math.Sqrt"/>
        public Length Sqrt()
        {
            var lengthUnit = area.Unit switch
            {
                AreaUnit.SquareMeter => LengthUnit.Meter,
                AreaUnit.SquareKilometer => LengthUnit.Kilometer,
                AreaUnit.SquareCentimeter => LengthUnit.Centimeter,
                AreaUnit.SquareMillimeter => LengthUnit.Millimeter,
                AreaUnit.SquareMicrometer => LengthUnit.Micrometer,
                AreaUnit.SquareMile => LengthUnit.Mile,
                AreaUnit.SquareYard => LengthUnit.Yard,
                AreaUnit.SquareFoot => LengthUnit.Foot,
                AreaUnit.SquareInch => LengthUnit.Inch,
                AreaUnit.SquareDecimeter => LengthUnit.Decimeter,
                AreaUnit.SquareNauticalMile => LengthUnit.NauticalMile,
                AreaUnit.UsSurveySquareFoot => LengthUnit.UsSurveyFoot,
                _ => throw new NotSupportedException($"unsupported AreaUnit: {area.Unit}")
            };
            var sqrt = Math.Sqrt(area.Value);
            return new Length(sqrt, lengthUnit);
        }
    }

    #region trigonometric functions

    extension(Angle angle)
    {
        /// <summary>
        /// Returns the complement of this angle (π/2 - angle).
        /// The complement is the angle that, when added to this angle, equals 90 degrees (π/2 radians).
        /// </summary>
        public Angle Complement => Angle.FromDegrees(90).ToUnit(angle.Unit) - angle;

        /// <summary>
        /// Returns the supplement of this angle (π - angle).
        /// The supplement is the angle that, when added to this angle, equals 180 degrees (π radians).
        /// </summary>
        public Angle Supplement => Angle.FromDegrees(180).ToUnit(angle.Unit) - angle;

        /// <inheritdoc cref="Math.Tan"/>
        public double Tan => Math.Tan(angle.Radians);

        /// <inheritdoc cref="Math.Sin"/>
        public double Sin => Math.Sin(angle.Radians);

        /// <inheritdoc cref="Math.Cos"/>
        public double Cos => Math.Cos(angle.Radians);

        /// <summary>
        /// Normalizes an angle into the range [min, min + 2π).
        /// Useful when you want to wrap angles into a custom range starting at <paramref name="min"/>.
        /// </summary>
        /// <param name="min"></param>
        /// <returns></returns>
        public Angle NormalizeToRange(Angle min) => angle.NormalizePositive() + min;

        /// <summary>
        /// Normalizes an angle into the range [-π, π).
        /// The result is symmetric around zero, useful for applications where both clockwise and counter-clockwise directions are needed.
        /// </summary>
        public Angle NormalizeSymmetric() => angle.NormalizePositive() - Angle.FromRevolutions(0.5);

        /// <summary>
        /// Normalizes an angle into the range [0, 2π).
        /// The result is always a positive angle, useful for applications requiring unsigned angles.
        /// </summary>
        /// <returns></returns>
        public Angle NormalizePositive()
        {
            var revolution = Angle.FromRevolutions(1);
            var round = (angle / revolution).FloorToInt();
            return angle - round * revolution;
        }
    }

    /// <inheritdoc cref="Math.Atan"/>
    public static Angle Atan(this double d) => Angle.FromRadians(Math.Atan(d));


    /// <inheritdoc cref="Math.Atan2"/>
    public static Angle Atan2(this double y, double x) => Angle.FromRadians(Math.Atan2(y, x));


    /// <inheritdoc cref="Math.Atan2"/>
    public static Angle Atan2(this Length y, Length x)
        => Angle.FromRadians(Math.Atan2(y.As(UnitSystem.SI), x.As(UnitSystem.SI)));


    /// <inheritdoc cref="Math.Acos"/>
    public static Angle Acos(this double d) => Angle.FromRadians(Math.Acos(d));


    /// <inheritdoc cref="Math.Asin"/>
    public static Angle Asin(this double d) => Angle.FromRadians(Math.Asin(d));

    #endregion
}