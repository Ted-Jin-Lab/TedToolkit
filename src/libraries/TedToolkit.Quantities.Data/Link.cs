using Newtonsoft.Json;

namespace TedToolkit.Quantities.Data;

public readonly record struct Link(string Name, string Url)
{
    [JsonIgnore]
    public string Remarks => $"<see href=\"{Url}\">{Name}</see>";
}