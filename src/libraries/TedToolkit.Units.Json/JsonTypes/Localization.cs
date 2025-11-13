using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TedToolkit.Units.Json;

/// <summary>
///     Localization of a unit, such as unit abbreviations in different languages.
/// </summary>
public struct Localization()
{
    /// <summary>
    ///     The unit abbreviations. Can be empty for dimensionless units like Ratio.DecimalFraction.
    /// </summary>
    public string[] Abbreviations { get; set; } = [];
    
    /// <summary>
    ///     Explicit configuration of unit abbreviations for prefixes.
    ///     This is typically used for languages or special unit abbreviations where you cannot simply prepend SI prefixes like
    ///     "k" for kilo
    ///     to the abbreviations defined in <see cref="Localization.Abbreviations" />.
    /// </summary>
    /// <example>
    ///     Energy.ThermEc unit has "Abbreviations": "th (E.C.)" and "AbbreviationsForPrefixes": { "Deca": "Dth (E.C.)" } since
    ///     the SI prefix for Deca is "Da" and "Dath (E.C.)" is not the conventional form.
    /// </example>
    /// <remarks>
    ///     The unit abbreviation value can either be a string or an array of strings. Typically the number of abbreviations
    ///     for a prefix matches that of "Abbreviations" array, but this is not required.
    /// </remarks>
    [JsonConverter(typeof(DictionaryPrefixToStringArrayConverter))]
    public Dictionary<Prefix, string[]> AbbreviationsForPrefixes { get; set; } = [];

    /// <summary>
    ///     The name of the culture this is a localization for.
    /// </summary>
    public string Culture { get; set; } = string.Empty;
}

internal class DictionaryPrefixToStringArrayConverter : JsonConverter<Dictionary<Prefix, string[]>>
{
    public override Dictionary<Prefix, string[]>? ReadJson(JsonReader reader, Type objectType, Dictionary<Prefix, string[]>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.StartObject)
            throw new JsonException("Expected StartObject");

        var dict = new Dictionary<Prefix, string[]>();
        var jObject = JObject.Load(reader);

        foreach (var property in jObject.Properties())
        {
            if (!Enum.TryParse<Prefix>(property.Name, out var key))
                throw new JsonException($"Invalid prefix: {property.Name}");

            var values = property.Value.Type switch
            {
                JTokenType.String => [property.Value.ToString()!],
                JTokenType.Array => property.Value.ToObject<string[]>()!,
                _ => throw new JsonException("Expected string or array")
            };

            dict[key] = values;
        }

        return dict;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<Prefix, string[]>? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        if (value != null)
        {
            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key.ToString());
                writer.WriteStartArray();
                foreach (var v in kvp.Value)
                {
                    writer.WriteValue(v);
                }
                writer.WriteEndArray();
            }
        }

        writer.WriteEndObject();
    }
}