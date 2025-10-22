using System.Diagnostics;
using TedToolkit.Assertions.AssertionItems;
using TedToolkit.Assertions.Assertions;
using TedToolkit.Assertions.Execution;
using Microsoft.Extensions.Logging;

namespace TedToolkit.Assertions.Logging;

[DebuggerNonUserCode]
file class LoggerStrategy(AssertionLogOptions options) : IAssertionStrategy
{
    public object? HandleFailure(AssertionScope scope, IReadOnlyList<IAssertion> assertions)
    {
        return null;
    }

    public object? HandleFailure(AssertionScope scope, AssertionType assertionType, AssertionItem assertion,
        object? tag, CallerInfo callerInfo)
    {
        if (scope.Tag is not ILogger logger) return null;

        var format = assertion.Message.StructuredFormat;
        IEnumerable<object?> arguments = assertion.Message.Arguments;

        if (options.ShowTag && tag is not null && tag is not EventId)
        {
            format += "\nTag: {Tag}";
            arguments = arguments.Append(tag);
        }

        if (options.ShowCallerInfo)
        {
            format += "\nCallerInfo: {CallerInfo}";
            arguments = arguments.Append(options.CallerInfoFormat?.Invoke(callerInfo) ?? callerInfo.ToString());
        }

        var eventId = tag is EventId id ? id : 0;

        logger.Log(assertionType switch
        {
            AssertionType.Must => LogLevel.Error,
            AssertionType.Should => LogLevel.Warning,
            AssertionType.Could => LogLevel.Information,
            _ => LogLevel.Debug
        }, eventId, format, [..arguments]);
        return null;
    }
}

file class LogAssertionScope(AssertionScope scope, IDisposable logScope) : IDisposable
{
    public void Dispose()
    {
        scope.Dispose();
        logScope.Dispose();
    }
}

/// <summary>
///     The extensions for the logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    ///     Add the logging scope
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="messageFormat"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IDisposable BeginAssertionScope(this ILogger logger, string? messageFormat = null,
        params object?[] args)
    {
        var logScope = logger.BeginScope(messageFormat, args);
        return new LogAssertionScope(new AssertionScope(string.Empty, logger), logScope);
    }

    /// <summary>
    ///     Create the scope with the strategy.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="strategy"></param>
    /// <param name="messageFormat"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IDisposable BeginAssertionScope(this ILogger logger, IAssertionStrategy strategy,
        string? messageFormat = null,
        params object?[] args)
    {
        var logScope = logger.BeginScope(messageFormat, args);
        return new LogAssertionScope(new AssertionScope(strategy, string.Empty, logger), logScope);
    }

    /// <summary>
    ///     Add the assertions to the <see cref="ILogger" />.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="changeOptions"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddTedToolkitAssertion(this ILoggingBuilder builder,
        Func<AssertionLogOptions, AssertionLogOptions>? changeOptions = null)
    {
        var defaultOptions = new AssertionLogOptions(true, true);
        var loggerStrategy = new LoggerStrategy(changeOptions?.Invoke(defaultOptions) ?? defaultOptions);
        AssertionService.Add(new AssertionService(loggerStrategy, loggerStrategy));

        //Exceptions.
        AssertionService.Add(new AssertionService(AssertionType.Must));
        return builder;
    }
}