using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using FluentResults;

namespace TedToolkit.ValidResults;

public static class ValidResultsExtensions
{
    [Pure]
    [OverloadResolutionPriority(1)]
    public static ResultTracker<ValidResult<TValue>> Track<TValue>(this ValidResult<TValue> value,
        [CallerArgumentExpression(nameof(value))]
        string valueName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int fileLineNumber = 0)
    {
        return new ResultTracker<ValidResult<TValue>>(value, GetCallerInfo(valueName, filePath, fileLineNumber));
    }

    [Pure]
    public static ResultTracker<ValidResult<TValue>> Track<TValue>(this TValue value,
        [CallerArgumentExpression(nameof(value))]
        string valueName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int fileLineNumber = 0)
    {
        return new ResultTracker<ValidResult<TValue>>(ValidResult<TValue>.Data.Ok(value),
            GetCallerInfo(valueName, filePath, fileLineNumber));
    }

    [Pure]
    public static IEnumerable<IReason> GetReasons<TValue>(this ResultTracker<TValue> tracker,
        [CallerArgumentExpression(nameof(tracker))]
        string methodArgumentName = "",
        [CallerMemberName] string memberName = "") where TValue : IValidResult
    {
        return GetReasons(tracker.Value, tracker.CallerInfo, memberName, methodArgumentName);
    }

    [Pure]
    public static IEnumerable<IReason> GetReasons<TValue>(this TValue value,
        string valueName,
        string filePath,
        int fileLineNumber,
        [CallerArgumentExpression(nameof(value))]
        string methodArgumentName = "",
        [CallerMemberName] string memberName = "") where TValue : IValidResult
    {
        var callerInfo = GetCallerInfo(valueName, filePath, fileLineNumber);
        return GetReasons(value, callerInfo, memberName, methodArgumentName);
    }

    [Pure]
    private static IEnumerable<IReason> GetReasons<TValue>(TValue value, string callerInfo, string methodName,
        string methodArgumentName) where TValue : IValidResult
    {
        var reasons = value.Result.Reasons
            .Select(i => i is ObjectValidationError validation ? validation.WithInstanceName(callerInfo) : i);

        if (value.Result.IsFailed || value is not IValidObjectResult { ValueOrDefault: { } targetObject })
            return reasons;
        var argumentValidationResult =
            ValidResultsConfig.ValidateArgument(targetObject, methodName, methodArgumentName);
        if (argumentValidationResult.IsValid) return reasons;
        return reasons.Append(new ObjectValidationError(argumentValidationResult, callerInfo));
    }

    [Pure]
    public static ResultTracker<TValue> AsTracker<TValue>(this TValue value,
        [CallerArgumentExpression(nameof(value))]
        string valueName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int fileLineNumber = 0)
    {
        return new ResultTracker<TValue>(value, GetCallerInfo(valueName, filePath, fileLineNumber));
    }

    [Pure]
    public static string GetCallerInfo(string valueName, string filePath, int fileLineNumber)
    {
        var fileInfo = ValidResultsConfig.FileInfoFormater?.Invoke((filePath, fileLineNumber))
                       ?? $"{Path.GetFileName(filePath)}:{fileLineNumber}";
        return $"{valueName} in {fileInfo}";
    }

    [Pure]
    internal static IReadOnlyCollection<T> RemoveDuplicated<T>(this IReadOnlyCollection<T> collection) where T : IReason
    {
        var result = new List<T>(collection.Count);
        var validationErrors = new Dictionary<ObjectValidationError, ObjectValidationError>(collection.Count);
        foreach (var item in collection)
            if (item is ObjectValidationError { Owner: { } owner } error)
            {
                if (!validationErrors.TryGetValue(owner, out var oldError)
                    || GetLineNumber(error) < GetLineNumber(oldError))
                    validationErrors[owner] = error;
            }
            else
            {
                result.Add(item);
            }

        return [..result, ..validationErrors.Values.OfType<T>()];

        static int GetLineNumber(ObjectValidationError error)
        {
            return int.TryParse(error.CallerInfo.Split(':').LastOrDefault(), out var count) ? count : int.MaxValue;
        }
    }

    [Pure]
    internal static string GetString(this Result result)
    {
        if (result.IsFailed)
            return (ValidResultsConfig.ToStringWithEmoji ? "❌" : "[Failure]") + " " +
                   string.Join(", ", result.Reasons.Select(i => $"{{{i.GetString()}}}"));
        return ValidResultsConfig.ToStringWithEmoji ? "✅" : "[Success]";
    }

    [Pure]
    private static string? GetString(this IReason reason)
    {
        return ValidResultsConfig.SimplifyObjectValidationReasonToString
            ? $"<{reason.GetType().Name}> {reason.Message}"
            : reason.ToString();
    }
}