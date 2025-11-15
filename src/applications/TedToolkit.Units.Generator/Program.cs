// See https://aka.ms/new-console-template for more information

using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.Units.Generator;
using VDS.RDF;
using VDS.RDF.Parsing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

Console.WriteLine("Hello, World!");

var path = @"E:\PartTime\Code\TedToolkit\externals\QUDT\QUDT-all-in-one-OWL.ttl";

var g = new Graph();
var parser = new TurtleParser();
parser.Load(g, path);
var quantities = new QudtAnalyzer(g).Analyze()
    .GroupBy(i => i.Dimension)
    .Where(i => i.Count() is not 1)
    .Select(i => i.OrderByDescending(i => i.IsDimensionDefault).ToArray())
    .ToArray();

return;
// var unitFolder = new DirectoryInfo(AppContext.BaseDirectory).Parent?.Parent?.Parent?.Parent?.Parent
//     ?.CreateSubdirectory("libraries").CreateSubdirectory("TedToolkit.Units");
//
// if (unitFolder is null) return;
// var unitsFolder = unitFolder.CreateSubdirectory("Units");
//
// var toStringExtensionClass = ClassDeclaration("UnitToStringExtensions")
//     .WithModifiers(
//         TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
//     .WithAttributeLists([GeneratedCodeAttribute(typeof(Program))]);
//
// foreach (var quantity in await Quantity.GetQuantitiesAsync())
// {
//     var enumGenerator = new UnitEnumGenerator(quantity);
//
//     await File.WriteAllTextAsync(Path.Combine(unitsFolder.FullName, enumGenerator.FileName),
//         enumGenerator.GenerateCode());
//     toStringExtensionClass = toStringExtensionClass.AddMembers(enumGenerator.GenerateToString());
// }
//
// await File.WriteAllTextAsync(Path.Combine(unitFolder.FullName, "UnitToStringExtensions.g.cs"),
//     NamespaceDeclaration("TedToolkit.Units").WithMembers([toStringExtensionClass]).NodeToString());
//
// Console.WriteLine("Done");