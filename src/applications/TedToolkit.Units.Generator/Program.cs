// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using TedToolkit.Units.Generator;
using TedToolkit.Units.Generator.JsonTypes;

Console.WriteLine("Hello, World!");

var assembly = Assembly.GetExecutingAssembly();

var regex = new Regex(@"TedToolkit\.Units\.Generator\.Json\..*\.json");
var options = new JsonSerializerOptions
{
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
};
foreach (var manifestResourceName in assembly.GetManifestResourceNames())
{
    if (!regex.IsMatch(manifestResourceName)) continue;
    Quantity quantity;
    await using (var stream = assembly.GetManifestResourceStream(manifestResourceName))
    {
        if (stream is null) continue;
        quantity = await JsonSerializer.DeserializeAsync<Quantity>(stream, options);
    }

    Console.WriteLine(quantity.Name);
}

Console.WriteLine("Done");