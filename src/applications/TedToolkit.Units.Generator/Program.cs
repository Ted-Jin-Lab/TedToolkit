// See https://aka.ms/new-console-template for more information

using Microsoft.CodeAnalysis.CSharp;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.Units.Generator;
using TedToolkit.Units.Json;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

Console.WriteLine("Hello, World!");

var unitFolder = new DirectoryInfo(AppContext.BaseDirectory).Parent?.Parent?.Parent?.Parent?.Parent
    ?.CreateSubdirectory("libraries").CreateSubdirectory("TedToolkit.Units");

if (unitFolder is null) return;
var unitsFolder = unitFolder.CreateSubdirectory("Units");

var toStringExtensionClass = ClassDeclaration("UnitToStringExtensions")
    .WithModifiers(
        TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
    .WithAttributeLists([GeneratedCodeAttribute(typeof(Program))]);

foreach (var quantity in await Quantity.Quantities)
{
    var enumGenerator = new UnitEnumGenerator(quantity);
    enumGenerator.GenerateCode(unitsFolder.FullName);
    toStringExtensionClass = toStringExtensionClass.AddMembers(enumGenerator.GenerateToString());
    if (string.IsNullOrEmpty(quantity.BaseUnit))
    {
        Console.WriteLine(quantity.Name);
    }

    var baseUnit = quantity.UnitsInfos.FirstOrDefault(i => 
        string.Equals(i.Name, quantity.BaseUnit, StringComparison.InvariantCultureIgnoreCase));
    if(baseUnit.Unit is null)
    {
        Console.WriteLine(quantity.Name);
    }
}

File.WriteAllText(Path.Combine(unitFolder.FullName, "UnitToStringExtensions.g.cs"),
    NamespaceDeclaration("TedToolkit.Units").WithMembers([toStringExtensionClass]).NodeToString());

// foreach (var quantity in await Quantity.Quantities)
// {
//     if (!quantity.IsNoDimensions) continue;
//     Console.WriteLine(quantity.Name);
// }

Console.WriteLine("Done");