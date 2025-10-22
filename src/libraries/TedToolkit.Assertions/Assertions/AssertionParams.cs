using System.Diagnostics.CodeAnalysis;

namespace TedToolkit.Assertions.Assertions;

/// <summary>
/// </summary>
public readonly struct AssertionParams()
{
    internal string Reason => string.Format(ReasonFormat, ReasonArgs);

    /// <summary>
    ///     The reason formating string
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
    public string ReasonFormat { get; init; } = string.Empty;

    /// <summary>
    ///     The reason format arguments
    /// </summary>
    public object?[] ReasonArgs { get; init; } = [];

    /// <summary>
    ///     Your custom Tag
    /// </summary>
    public object? Tag { get; init; } = null;

    /// <summary>
    ///     Create by the reason.
    /// </summary>
    /// <param name="reason"></param>
    /// <returns></returns>
    public static implicit operator AssertionParams(string reason)
    {
        return new AssertionParams { ReasonFormat = reason };
    }
}