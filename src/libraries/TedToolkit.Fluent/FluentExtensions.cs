namespace TedToolkit.Fluent;

/// <summary>
///     Make the one as fluent.
/// </summary>
public static class FluentExtensions
{
    private static FluentOptions _options;

    /// <summary>
    ///     The options.
    /// </summary>
    public static ref FluentOptions Options => ref _options;

    /// <summary>
    ///     Make this value as a fluent one.
    ///     <example>
    ///         Create your own data type.
    ///         <code>
    ///  class Point
    /// {
    ///     public double X { get; set; }
    ///     public double Y { get; set; }
    ///     public void AddX(double x) => X += x;
    /// }
    ///  </code>
    ///         And then, us it like this. The api will be auto created.
    ///         <code>
    ///  var point = new Point().AsFluent()
    ///     .WithX(5)
    ///     .DoAddX(6)
    ///     .Result;
    ///  </code>
    ///     </example>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Fluent<T> AsFluent<T>(this T value, FluentType? type = null) where T : notnull
    {
        return new Fluent<T>(value, type ?? _options.DefaultType);
    }
}