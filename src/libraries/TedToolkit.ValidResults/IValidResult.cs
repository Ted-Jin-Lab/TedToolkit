using FluentResults;

namespace TedToolkit.ValidResults;

public interface IValidObjectResult : IValidResult
{
    object? ValueOrDefault { get; }
}

public interface IValidResult<out TValue> : IValidObjectResult
{
    new TValue ValueOrDefault { get; }
    TValue Value { get; }
}

public interface IValidResult
{
    Result Result { get; }
}