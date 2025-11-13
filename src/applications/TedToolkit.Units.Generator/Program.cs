// See https://aka.ms/new-console-template for more information

using TedToolkit.Units.Generator;
using TedToolkit.Units.Json;

Console.WriteLine("Hello, World!");

var unitFolder = new DirectoryInfo(AppContext.BaseDirectory).Parent?.Parent?.Parent?.Parent?.Parent
    ?.CreateSubdirectory("libraries").CreateSubdirectory("TedToolkit.Units");

if (unitFolder is null) return;
var quantitiesFolder = unitFolder.CreateSubdirectory("Quantities");
var unitsFolder = unitFolder.CreateSubdirectory("Units");

foreach (var quantity in await Quantity.Quantities)
{
    var enumGenerator = new UnitEnumGenerator(quantity);
    enumGenerator.GenerateCode(unitsFolder.FullName);
}

foreach (var quantity in await Quantity.Quantities)
{
    if(!quantity.IsNoDimensions) continue;
    Console.WriteLine(quantity.Name);
}
Console.WriteLine("Done");