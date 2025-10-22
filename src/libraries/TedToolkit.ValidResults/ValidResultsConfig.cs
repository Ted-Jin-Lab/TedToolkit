using FluentResults;
using FluentValidation;
using FluentValidation.Results;

namespace TedToolkit.ValidResults;

public static class ValidResultsConfig
{
    public delegate bool ShouldUseDelegate(string methodName, string methodArgumentName);

    private static readonly Dictionary<Type, ValidResultsValidator> Validators = new();
    public static bool SimplifyObjectValidationReasonToString { get; set; } = true;
    public static bool ToStringWithEmoji { get; set; } = true;

    public static Func<Exception, IError>? ExceptionHandler { get; set; } = ex => new ExceptionalError(ex.Message, ex);
    public static Func<(string FilePath, int FileLineNumber), string>? FileInfoFormater { get; set; } = null;

    internal static ValidationResult ValidateObject<T>(T value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return new ValidationResult(GetValidators(typeof(T)).Select(f => f.ValidateObject(value)));
    }

    internal static ValidationResult ValidateArgument(object value, string methodName, string methodArgumentName)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return new ValidationResult(GetValidators(value.GetType())
            .Select(f => f.ValidateArgument(value, methodName, methodArgumentName)));
    }

    private static IEnumerable<ValidResultsValidator> GetValidators(Type type)
    {
        return Validators.Where(kvp => kvp.Key.IsAssignableFrom(type)).Select(kvp => kvp.Value).ToArray();
    }

    public static Guid AddValidator<T>(IValidator<T> validator, ShouldUseDelegate? shouldUse = null)
    {
        return AddValidator<T>(validator.Validate, shouldUse);
    }

    public static Guid AddValidator<T>(Func<T, ValidationResult> validator, ShouldUseDelegate? shouldUse = null)
    {
        var type = typeof(T);
        if (!Validators.TryGetValue(type, out var resultValidator))
            resultValidator = Validators[type] = new ValidResultsValidator(type);
        return resultValidator.AddValidator(o => validator((T)o), shouldUse);
    }

    public static void RemoveValidator<T>()
    {
        Validators.Remove(typeof(T));
    }

    public static bool RemoveValidator(Guid key)
    {
        return Validators.Values.Any(i => i.RemoveValidator(key));
    }
}