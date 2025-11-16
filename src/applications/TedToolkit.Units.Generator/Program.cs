// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using PeterO.Numbers;
using TedToolkit.Units.Generator;
using VDS.RDF;
using VDS.RDF.Parsing;

Console.WriteLine("Hello, World!");

Console.WriteLine(ERational.One);
// var i =ERational.FromEDecimal( EDecimal.FromInt64(14959787069160));
// var re = ERational.One / i;
// Console.WriteLine(re.ToInt64Unchecked() );
// Console.WriteLine(re.Denominator);
// Console.WriteLine(re.Numerator);
return;

var srcFolder = new DirectoryInfo(AppContext.BaseDirectory).Parent?.Parent?.Parent?.Parent?.Parent;
var solutionFolder = srcFolder?.Parent;
var unitFolder = srcFolder?.CreateSubdirectory("analyzers").CreateSubdirectory("TedToolkit.Units.Analyzer");
var qudtFolder = solutionFolder?.CreateSubdirectory("externals").CreateSubdirectory("QUDT");
if (qudtFolder is null || unitFolder is null) return;
var path = Path.Combine(qudtFolder.FullName, "QUDT-all-in-one-OWL.ttl");

var g = new Graph();
var parser = new TurtleParser();
parser.Load(g, path);
var data = new QudtAnalyzer(g).Analyze();
foreach (var unitsValue in data.Units.Values)
{
    Console.WriteLine(unitsValue.Conversion);
}
await File.WriteAllTextAsync(Path.Combine(unitFolder.FullName, "qudt.json"), JsonConvert.SerializeObject(data));