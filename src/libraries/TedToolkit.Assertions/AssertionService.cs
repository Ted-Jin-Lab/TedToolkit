using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using TedToolkit.Assertions.AssertionItems;
using TedToolkit.Assertions.Assertions;
using TedToolkit.Assertions.Exceptions;
using TedToolkit.Assertions.Execution;

namespace TedToolkit.Assertions;

[DebuggerNonUserCode]
file class DefaultPushStrategy(
    AssertionType minRaisingExceptionType,
    [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
    string timeFormat) : IAssertionStrategy
{
    public object? HandleFailure(AssertionScope scope, AssertionType assertionType, AssertionItem assertion,
        object? tag, CallerInfo callerInfo)
    {
        if (assertionType < minRaisingExceptionType) return null;
        var message = $"{assertion.Message}\nwhen [{assertion.Time.ToString(timeFormat)}]{scope.Context}";
        throw new AssertionException(message);
    }

    public object? HandleFailure(AssertionScope scope, IReadOnlyList<IAssertion> assertions)
    {
        return null;
    }
}

[DebuggerNonUserCode]
file class DefaultScopeStrategy(
    AssertionType minRaisingExceptionType,
    [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
    string timeFormat,
    Func<CallerInfo, string>? callerInfoFormate) : IAssertionStrategy
{
    public object? HandleFailure(AssertionScope scope, IReadOnlyList<IAssertion> assertions)
    {
        var stringBuilder = new StringBuilder();
        var messageCount = 0;
        foreach (var assertion in assertions)
        {
            if (assertion.Type < minRaisingExceptionType) continue;
            foreach (var assertionItem in assertion.Items)
            {
                stringBuilder.AppendLine(
                    $"{++messageCount:D2}. [{assertionItem.Time.ToString(timeFormat)}] {assertionItem.Message}");
                stringBuilder.AppendLine(callerInfoFormate?.Invoke(assertion.CallerInfo) ??
                                         $"  {assertion.CallerInfo}");
            }
        }

        if (messageCount is 0) return null;
        stringBuilder.Insert(0,
            $"[{DateTimeOffset.Now.ToString(timeFormat)}][{messageCount} message(s)]{scope.Context}\n");
        throw new AssertionException(stringBuilder.ToString());
    }

    public object? HandleFailure(AssertionScope scope, AssertionType assertionType, AssertionItem assertion,
        object? tag, CallerInfo callerInfo)
    {
        return null;
    }
}

/// <summary>
///     The service of the assertions.
/// </summary>
public readonly struct AssertionService(IAssertionStrategy pushStrategy, IAssertionStrategy scopeStrategy)
{
    private static readonly ConcurrentBag<AssertionService> CurrentServices = [];
    private static readonly AsyncLocal<MergedAssertionStrategy> MergedPushStrategyAsyncLocal = new();
    private static readonly AsyncLocal<MergedAssertionStrategy> MergedScopeStrategyAsyncLocal = new();

    internal static MergedAssertionStrategy MergedPushStrategy =>
        MergedPushStrategyAsyncLocal.Value ??= CalculateMergedPushStrategy();

    internal static MergedAssertionStrategy MergedScopeStrategy =>
        MergedScopeStrategyAsyncLocal.Value ??= CalculateMergedScopeStrategy();


    private static void CheckCurrentServices()
    {
        if (CurrentServices.Count > 0) return;
        CurrentServices.Add(new AssertionService(AssertionType.Must));
    }

    private static MergedAssertionStrategy CalculateMergedPushStrategy()
    {
        CheckCurrentServices();
        return new MergedAssertionStrategy([..CurrentServices.Select(s => s.PushStrategy)]);
    }

    private static MergedAssertionStrategy CalculateMergedScopeStrategy()
    {
        CheckCurrentServices();
        return new MergedAssertionStrategy([..CurrentServices.Select(s => s.ScopeStrategy)]);
    }

    /// <summary>
    ///     Create the service by the min raising exception type
    /// </summary>
    /// <param name="minRaisingExceptionType"></param>
    /// <param name="timeFormat"></param>
    /// <param name="callerInfoFormat"></param>
    public AssertionService(AssertionType minRaisingExceptionType, string timeFormat = "yyyy-MM-dd HH:mm:ss.fff zzz",
        Func<CallerInfo, string>? callerInfoFormat = null)
        : this(new DefaultPushStrategy(minRaisingExceptionType, timeFormat),
            new DefaultScopeStrategy(minRaisingExceptionType, timeFormat, callerInfoFormat))
    {
    }

    /// <summary>
    ///     Set the service
    /// </summary>
    /// <param name="service"></param>
    public static void Add(AssertionService service)
    {
        CurrentServices.Add(service);
        MergedPushStrategyAsyncLocal.Value = CalculateMergedPushStrategy();
        MergedScopeStrategyAsyncLocal.Value = CalculateMergedScopeStrategy();
    }

    /// <summary>
    ///     Clear the services
    /// </summary>
    public static void Clear()
    {
        while (CurrentServices.TryTake(out _))
        {
        }

        MergedPushStrategyAsyncLocal.Value = CalculateMergedPushStrategy();
        MergedScopeStrategyAsyncLocal.Value = CalculateMergedScopeStrategy();
    }

    /// <summary>
    ///     The default push strategy.
    /// </summary>
    public IAssertionStrategy PushStrategy { get; } = pushStrategy;

    /// <summary>
    ///     The default scope strategy
    /// </summary>
    public IAssertionStrategy ScopeStrategy { get; } = scopeStrategy;
}