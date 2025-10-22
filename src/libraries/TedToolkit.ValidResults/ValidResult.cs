using System.Diagnostics.Contracts;
using FluentResults;

namespace TedToolkit.ValidResults;

public class ValidResult(ValidResult.Data data) : IValidResult
{
    public Result Result => data.Result;

    [Pure]
    public static implicit operator ValidResult(Data data)
    {
        return new ValidResult(data);
    }

    public static bool operator true(ValidResult message)
    {
        return message.Result.IsSuccess;
    }

    public static bool operator false(ValidResult message)
    {
        return message.Result.IsFailed;
    }

    public override string ToString()
    {
        return Result.GetString();
    }

    public record Data(Result Result)
    {
        [Pure]
        public static Data Ok(Data value)
        {
            return value;
        }

        [Pure]
        public static Data Ok(params IReadOnlyCollection<ISuccess> successes)
        {
            var result = Result.Ok().WithSuccesses(successes.RemoveDuplicated());
            return new Data(result);
        }

        [Pure]
        public static Data? Fail(IReadOnlyCollection<IReason> reasons)
        {
            return reasons.OfType<IError>().Any()
                ? new Data(new Result().WithReasons(reasons.RemoveDuplicated()))
                : null;
        }

        [Pure]
        public static implicit operator Data(Result result)
        {
            return new Data(result);
        }
    }
}

public class ValidResult<TValue>(ValidResult<TValue>.Data data) : IValidResult<TValue>
{
    public Result Result => data.Result;
    public TValue ValueOrDefault => data.ValueOrDefault;

    public TValue Value
    {
        get
        {
            if (Result.IsFailed) throw new InvalidOperationException("This is failed, you can't get the value.");
            return ValueOrDefault;
        }
    }

    object? IValidObjectResult.ValueOrDefault => ValueOrDefault;

    [Pure]
    public static implicit operator ValidResult<TValue>(Data data)
    {
        return new ValidResult<TValue>(data);
    }

    [Pure]
    public static implicit operator ValidResult<TValue>(TValue value)
    {
        return Data.Ok(value);
    }

    public override string ToString()
    {
        var result = Result.GetString();
        if (Result.IsSuccess) result += " " + ValueOrDefault;
        return result;
    }

    public static bool operator true(ValidResult<TValue> message)
    {
        return message.Result.IsSuccess;
    }

    public static bool operator false(ValidResult<TValue> message)
    {
        return message.Result.IsFailed;
    }

    protected ValidResult<T>.Data GetProperty<T>(Func<T> getter)
    {
        return GetProperty(() => new ValidResult<T>.Data(Result, Result.IsFailed ? default! : getter()));
    }

    protected ValidResult<T>.Data GetProperty<T>(Func<ValidResult<T>.Data> getter)
    {
        return Bind<T>(_ => getter());
    }

    protected void SetProperty<T>(ValidResult<T> value, Action<T> setter)
    {
        if (Result.IsFailed) return;
        if (value.Result.IsFailed) return;
        setter(value.Value);
    }

    public ValidResult<TOut>.Data Map<TOut>(Func<TValue, TOut> mapper)
    {
        return Bind<TOut>(v => mapper(v));
    }

    public ValidResult<TOut>.Data Bind<TOut>(Func<TValue, ValidResult<TOut>.Data> next)
    {
        if (Result.IsFailed) return Result;

        if (ValidResultsConfig.ExceptionHandler is not { } handle)
            return next(ValueOrDefault);
        try
        {
            return next(ValueOrDefault);
        }
        catch (Exception ex)
        {
            return new ValidResult<TOut>.Data(Result.Fail(handle(ex)), default!);
        }
    }

    public record Data(Result Result, TValue ValueOrDefault)
    {
        [Pure]
        public static Data Ok(Data value)
        {
            return value;
        }

        [Pure]
        public static Data Ok(TValue? value, params IReadOnlyCollection<ISuccess> successes)
        {
            if (value is null) return Result.Fail(new NullReferenceError(nameof(value)));
            var validationResult = ValidResultsConfig.ValidateObject(value);
            var result = Result.Ok().WithSuccesses(successes.RemoveDuplicated());

            return validationResult.IsValid
                ? new Data(result, value)
                : new Data(result.WithError(new ObjectValidationError(validationResult)), default!);
        }

        [Pure]
        public static Data? Fail(IReadOnlyCollection<IReason> reasons)
        {
            return reasons.OfType<IError>().Any()
                ? new Data(new Result().WithReasons(reasons.RemoveDuplicated()), default!)
                : null;
        }

        [Pure]
        public static implicit operator Data(TValue value)
        {
            return Ok(value);
        }

        [Pure]
        public static implicit operator Data(Result result)
        {
            return new Data(result, default!);
        }
    }
}