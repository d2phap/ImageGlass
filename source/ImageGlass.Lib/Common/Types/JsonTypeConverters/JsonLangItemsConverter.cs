/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using ImageGlass.Common.Localization;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Types.JsonTypeConverters;


/// <summary>
/// Reads/writes a language pack's <c>Items</c> dictionary keyed by <see cref="LangId"/> (by name).
/// </summary>
/// <remarks>
/// <para>
/// Unlike a generic enum-key converter, an unknown property name (a key removed/renamed in a newer
/// app version, or added in a newer pack) is SKIPPED rather than mapped to <c>default(LangId)</c>.
/// The default fallback would silently collide every unknown key onto the first enum member
/// (<c>_OK</c>), overwriting its translation - the cause of "OK" showing an unrelated string.
/// </para>
/// </remarks>
public sealed class JsonLangItemsConverter : JsonConverter<IDictionary<LangId, string>>
{
    public override IDictionary<LangId, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

        var result = new Dictionary<LangId, string>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) return result;
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();

            var name = reader.GetString();
            reader.Read(); // advance to the value

            // keep only string values whose key maps to a known LangId member; skip the rest
            if (reader.TokenType == JsonTokenType.String
                && Enum.TryParse<LangId>(name, ignoreCase: true, out var id))
            {
                result[id] = reader.GetString() ?? string.Empty;
            }
            else
            {
                reader.Skip();
            }
        }

        throw new JsonException();
    }


    public override void Write(Utf8JsonWriter writer, IDictionary<LangId, string> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key.ToString());
            writer.WriteStringValue(kvp.Value);
        }

        writer.WriteEndObject();
    }
}
