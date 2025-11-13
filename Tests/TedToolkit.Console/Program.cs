// ReSharper disable LocalizableElement

using System.Text.RegularExpressions;
using MathNet.Symbolics;
using TedToolkit.Assertions;
using TedToolkit.Assertions.Assertions.Extensions;
using TedToolkit.Assertions.Execution;
using TedToolkit.Assertions.FluentValidation;
using TedToolkit.Console;
using TedToolkit.Console.Wrapper;
using TedToolkit.QuantExtensions;
using TedToolkit.Units.Json;
using TedToolkit.ValidResults;

foreach (var quantity in await Quantity.Quantities)
{
    var regex = new Regex(@"[-]?\d+(\.\d+)?[eE][+-]?\d+");

    foreach (var quantityUnit in quantity.Units)
    {
        var formula = quantityUnit.BaseToUnit;

        var origin = formula.SetExpressionValue("value");
         var result = SymbolicExpression.Parse(origin);
         Console.WriteLine(origin + "\n\t" + result.ToString());
    }
}


// unsafe
// {
//     double a = 15;
//     var ptr = &a;
//     var lengthPtr = (Length<double>*)ptr;
//     var value = lengthPtr->Value;
//     Console.WriteLine(value);
// }
//
// Console.WriteLine(90.Degrees.NormalizePositive());
// Console.WriteLine(360.Degrees.NormalizePositive());
// Console.WriteLine((-Math.Tau.Radians).NormalizePositive());
// return;
//
// ValidResultsConfig.AddValidator(new DoubleValidator(), (methodName, argumentName) =>
// {
//     var result = methodName is "AddDays" && argumentName is "value";
//     return result;
// });
// var datetimeResult = DateTime.Now.ToValidResult();
// var result = datetimeResult.AddDays(-10);
//
// Console.WriteLine(result);
// var validator = new Validator();
// using (new AssertionScope())
// {
//     new Item().Must().BeValidBy(validator);
// }

var task = Wait();
Console.WriteLine("Starting task");
await task;
return;

async Task Wait()
{
    Console.WriteLine("Waiting...");
    await Task.Delay(1000);
}