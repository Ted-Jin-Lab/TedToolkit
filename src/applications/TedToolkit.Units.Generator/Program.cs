// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using TedToolkit.Units.Generator;
using VDS.RDF;
using VDS.RDF.Parsing;

Console.WriteLine("Hello, World!");

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
await File.WriteAllTextAsync(Path.Combine(unitFolder.FullName, "qudt.json"), JsonConvert.SerializeObject(data));