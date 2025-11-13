// Licensed under MIT No Attribution, see LICENSE file at the root.
// Copyright 2013 Andreas Gullberg Larsen (andreas.larsen84@gmail.com). Maintained at https://github.com/angularsen/UnitsNet.

using System.Text.Json;
using System.Text.Json.Serialization;

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
    public override Dictionary<Prefix, string[]>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dict = new Dictionary<Prefix, string[]>();

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return dict;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            var keyString = reader.GetString()!;
            if (!Enum.TryParse<Prefix>(keyString, out var key))
                throw new JsonException($"Invalid prefix: {keyString}");

            reader.Read();

            string[] values;

            if (reader.TokenType == JsonTokenType.String)
            {
                values = [reader.GetString()!];
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                var list = new List<string>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    list.Add(reader.GetString()!);
                }
                values = list.ToArray();
            }
            else
            {
                throw new JsonException("Expected string or array");
            }

            dict[key] = values;
        }

        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<Prefix, string[]> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key.ToString());
            writer.WriteStartArray();
            foreach (var v in kvp.Value)
                writer.WriteStringValue(v);
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }
}