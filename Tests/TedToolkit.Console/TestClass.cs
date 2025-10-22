using TedToolkit.Fluent;
using FluentValidation;

namespace TedToolkit.Console;

/// <summary>
///     Test summary.
/// </summary>
[FluentApi(typeof(int))]
public static class TestClass
{
    public static void AddIt(this int i)
    {
        i += 1;

        new Validator().Validate(new Item(), a => { });
    }
}

public class Item
{
    public int Value { get; set; }

    public static Item TestMethod(bool a)
    {
        return new Item { Value = 1 };
    }
}

public class Validator : AbstractValidator<Item>
{
    public Validator()
    {
        RuleFor(i => i.Value).GreaterThan(0);
    }
}

public class DoubleValidator : AbstractValidator<double>
{
    public DoubleValidator()
    {
        RuleFor(i => i).GreaterThan(0).WithSeverity(Severity.Info);
    }
}