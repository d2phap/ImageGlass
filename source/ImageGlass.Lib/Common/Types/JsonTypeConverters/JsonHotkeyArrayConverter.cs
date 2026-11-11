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
/// Converts an array of <see cref="Hotkey"/> to a JSON array of strings and vice versa.
/// Entries that do not describe a usable hotkey are dropped instead of read as <c>null</c>.
/// </summary>
public class JsonHotkeyArrayConverter : JsonConverter<Hotkey[]>
{
    public override void Write(Utf8JsonWriter writer, Hotkey[]? arr, JsonSerializerOptions options)
    {
        if (arr is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (var hotkey in arr)
        {
            if (hotkey is null) continue;
            writer.WriteStringValue(hotkey.InvariantKeyString);
        }
        writer.WriteEndArray();
    }

    public override Hotkey[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var hotkeys = new List<Hotkey>();

        if (reader.TokenType != JsonTokenType.StartArray) return [.. hotkeys];

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                reader.Skip();
                continue;
            }

            var hotkey = Hotkey.ParseFrom(reader.GetString());
            if (hotkey is not null) hotkeys.Add(hotkey);
        }

        return [.. hotkeys];
    }
}
