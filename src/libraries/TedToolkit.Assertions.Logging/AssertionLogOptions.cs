using TedToolkit.Assertions.Assertions;

namespace TedToolkit.Assertions.Logging;

/// <summary>
///     The assertion log options
/// </summary>
/// <param name="ShowTag"></param>
/// <param name="ShowCallerInfo"></param>
/// <param name="CallerInfoFormat"></param>
public readonly record struct AssertionLogOptions(
    bool ShowTag,
    bool ShowCallerInfo,
    Func<CallerInfo, string>? CallerInfoFormat = null);