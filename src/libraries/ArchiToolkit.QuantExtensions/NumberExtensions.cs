namespace ArchiToolkit.QuantExtensions;

public static class NumberExtensions
{
    #region double

    extension(double d)
    {
        /// <inheritdoc cref="Math.Sign(double)"/>
        public int Sign => Math.Sign(d);

        /// <inheritdoc cref="double.IsNaN(double)"/>
        public bool IsNaN => double.IsNaN(d);
    }

    /// <inheritdoc cref="Math.Abs(double)"/>
    public static double Abs(this double value) => Math.Abs(value);

    /// <inheritdoc cref="Math.Pow(double, double)"/>
    public static double Pow(this double x, double y) => Math.Pow(x, y);


    /// <inheritdoc cref="Math.Sqrt(double)"/>
    public static double Sqrt(this double d) => Math.Sqrt(d);

    /// <inheritdoc cref="Math.Floor(double)"/>
    public static double Floor(this double a) => Math.Floor(a);

    /// <inheritdoc cref="Math.Ceiling(double)"/>
    public static double Ceiling(this double a) => Math.Ceiling(a);

    /// <inheritdoc cref="Math.Round(double)"/>
    public static double Round(this double a) => Math.Round(a);

    /// <inheritdoc cref="Math.Floor(double)"/>
    public static int FloorToInt(this double a) => (int)Math.Floor(a);

    /// <inheritdoc cref="Math.Ceiling(double)"/>
    public static int CeilingToInt(this double a) => (int)Math.Ceiling(a);

    /// <inheritdoc cref="Math.Round(double)"/>
    public static int RoundToInt(this double a) => (int)Math.Round(a);

    #endregion
}