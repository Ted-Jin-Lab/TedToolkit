namespace TedToolkit.Assertions.Exceptions;

/// <summary>
///     The default assertion Exception.
/// </summary>
/// <param name="message"></param>
public class AssertionException(string? message) : Exception(message);