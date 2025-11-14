// See https://aka.ms/new-console-template for more information

using Microsoft.CodeAnalysis.CSharp;
using TedToolkit.RoslynHelper.Extensions;
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

foreach (var quantity in await Quantity.GetQuantitiesAsync())
{
    var enumGenerator = new UnitEnumGenerator(quantity);

    await File.WriteAllTextAsync(Path.Combine(unitsFolder.FullName, enumGenerator.FileName),
        enumGenerator.GenerateCode());
    toStringExtensionClass = toStringExtensionClass.AddMembers(enumGenerator.GenerateToString());
}

await File.WriteAllTextAsync(Path.Combine(unitFolder.FullName, "UnitToStringExtensions.g.cs"),
    NamespaceDeclaration("TedToolkit.Units").WithMembers([toStringExtensionClass]).NodeToString());

Console.WriteLine("Done");