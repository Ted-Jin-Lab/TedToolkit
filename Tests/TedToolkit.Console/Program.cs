// ReSharper disable LocalizableElement

using System.Globalization;
using System.Text.RegularExpressions;
using PeterO.Numbers;
using TedToolkit.Assertions;
using TedToolkit.Assertions.Assertions.Extensions;
using TedToolkit.Assertions.Execution;
using TedToolkit.Assertions.FluentValidation;
using TedToolkit.Console;
using TedToolkit.Console.Wrapper;
using TedToolkit.QuantExtensions;
using TedToolkit.ValidResults;
using UnitsNet;
using UnitsNet.Units;

// var pi = ERational.FromEDecimal(EDecimal.FromString("3.1415926535897932384626433832795028841971693993751058209749445923"));
// var result = pi * ERational.FromInt32(2);
// var ctx = EContext.ForPrecision(50) // 精度50位
//     .WithExponentRange(-1000, 1000) // 指数在 [-1000, 1000]
//     .WithRounding(ERounding.HalfEven);
// Console.WriteLine(result.ToEDecimal(ctx));

var rational = ERational.Create(EInteger.FromInt16(1), EInteger.FromInt16(10));
Console.WriteLine(rational);
var powered = Pow(rational, 4);
Console.WriteLine(powered);

return;
var length = (TedToolkit.Quantities.Length)6.0;
Console.WriteLine(length);
unsafe
{
    var ptr = (double*)&length;
    Console.WriteLine(*ptr);

    Console.WriteLine(sizeof(TedToolkit.Quantities.Length));
    Console.WriteLine(sizeof(double));
}

return;
var ratio = new AmplitudeRatio(5, AmplitudeRatioUnit.DecibelVolt);
var b = ratio.DecibelMicrovolts;
return;

static ERational Pow(ERational rational, int exponent)
{
    if (exponent == 0) return ERational.One;
    var one = rational;
    for (var i = 1; i < Math.Abs(exponent); i++)
    {
        rational *= one;
    }

    if (exponent < 0) rational = ERational.One / rational;
    return rational;
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

// var task = Wait();
// Console.WriteLine("Starting task");
// await task;
// return;

// async Task Wait()
// {
//     Console.WriteLine("Waiting...");
//     await Task.Delay(1000);
// }