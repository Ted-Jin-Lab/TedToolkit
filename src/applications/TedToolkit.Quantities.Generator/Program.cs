// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using PeterO.Numbers;
using TedToolkit.Quantities.Generator;
using VDS.RDF;
using VDS.RDF.Parsing;

Console.WriteLine("Hello, World!");

var srcFolder = new DirectoryInfo(AppContext.BaseDirectory).Parent?.Parent?.Parent?.Parent?.Parent;
if (srcFolder is null) return;
var solutionFolder = srcFolder.Parent;
var quantityFolder = srcFolder.CreateSubdirectory("libraries").CreateSubdirectory("TedToolkit.Quantities");
var unitFolder = srcFolder.CreateSubdirectory("analyzers").CreateSubdirectory("TedToolkit.Quantities.Analyzer").CreateSubdirectory("QUDT");
var qudtFolder = solutionFolder?.CreateSubdirectory("externals").CreateSubdirectory("QUDT");
if (qudtFolder is null ) return;
var path = Path.Combine(qudtFolder.FullName, "QUDT-all-in-one-OWL.ttl");

var g = new Graph();
var parser = new TurtleParser();
parser.Load(g, path);

List<(string name, string description)> names = [("ALL", "All quantities.")];
foreach (var uriNode in g.GetTriplesWithPredicateObject(
                 g.CreateUriNode("rdf:type"),
                 g.CreateUriNode("qudt:SystemOfQuantityKinds"))
             .Select(t => t.Subject)
             .OfType<IUriNode>())
{
    var name = uriNode.GetUrlName();
    var desc = uriNode.GetLabels(g)
        .OrderBy(l => l.Language.Length)
        .ThenBy(l => l.Language != "en")
        .FirstOrDefault()?.Value ?? "";
    names.Add((name, desc));
    var data = new QudtAnalyzer(g, uriNode).Analyze();
    await File.WriteAllTextAsync(Path.Combine(unitFolder.FullName, name + ".json"),
        JsonConvert.SerializeObject(data));
}

var allData = new QudtAnalyzer(g, null).Analyze();
await File.WriteAllTextAsync(Path.Combine(unitFolder.FullName, "ALL.json"), JsonConvert.SerializeObject(allData));

QuantitySystemGenerator.GenerateQuantitySystem(quantityFolder.FullName, names);
Console.WriteLine("Done!");