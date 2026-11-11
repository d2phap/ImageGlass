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
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Types.JsonTypeConverters;


/// <summary>
/// Converts a <see cref="HashSet{String}"/> of strings to a JSON array of strings and vice versa.
/// The resulting set uses <see cref="StringComparer.OrdinalIgnoreCase"/>.
/// </summary>
public class JsonHashSetToArrayConverter : JsonConverter<HashSet<string>>
{
    public override void Write(Utf8JsonWriter writer, HashSet<string> arr, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in arr)
        {
            writer.WriteStringValue(item);
        }
        writer.WriteEndArray();
    }

    public override HashSet<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (reader.TokenType != JsonTokenType.StartArray) return set;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            var item = reader.GetString();
            if (!string.IsNullOrWhiteSpace(item)) set.Add(item.Trim());
        }

        return set;
    }
}
