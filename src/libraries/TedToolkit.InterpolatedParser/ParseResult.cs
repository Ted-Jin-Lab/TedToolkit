using System.Collections;

namespace TedToolkit.InterpolatedParser;

/// <summary>
///     The parse result
/// </summary>
public readonly struct ParseResult(params IEnumerable<bool> results) : IEnumerable<bool>
{
    /// <summary>
    ///     All results
    /// </summary>
    public bool[] Results { get; init; } = results.ToArray();

    /// <summary>
    ///     The result.
    /// </summary>
    public bool Result => Results.Length > 0 && Results.All(b => b);

    /// <summary>
    ///     Conversion
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static implicit operator bool(ParseResult result)
    {
        return result.Result;
    }

    /// <summary>
    ///     Convert from a bool
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static implicit operator ParseResult(bool result)
    {
        return new ParseResult(result);
    }

    public IEnumerator<bool> GetEnumerator()
    {
        return ((IEnumerable<bool>)Results).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Results.GetEnumerator();
    }
}