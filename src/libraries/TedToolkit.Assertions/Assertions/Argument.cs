namespace TedToolkit.Assertions.Assertions;

/// <summary>
///     The string format argument.
/// </summary>
/// <param name="Name"></param>
/// <param name="Value"></param>
public readonly record struct Argument(string Name, object? Value);