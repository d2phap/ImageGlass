/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageGlass.Base;

public class IgTool
{
    /// <summary>
    /// Id of the tool.
    /// </summary>
    public string ToolId { get; set; } = string.Empty;


    /// <summary>
    /// Name of the tool.
    /// </summary>
    public string ToolName { get; set; } = string.Empty;


    /// <summary>
    /// Executable file / command to launch the tool.
    /// </summary>
    public string Executable { get; set; } = string.Empty;


    /// <summary>
    /// Argument to pass to the <see cref="Executable"/>.
    /// </summary>
    public string? Argument { get; set; } = string.Empty;


    /// <summary>
    /// Gets, sets value indicates that the tool is integrated with <c>ImageGlass.Tools</c>.
    /// </summary>
    public bool? IsIntegrated { get; set; } = false;


    /// <summary>
    /// Gets, sets tool hotkeys.
    /// </summary>
    [JsonConverter(typeof(HotkeyListJsonConverter))]
    public List<Hotkey> Hotkeys { get; set; } = new List<Hotkey>();


    /// <summary>
    /// Checks if the current <see cref="IgTool"/> instance is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(ToolId) || string.IsNullOrEmpty(Executable);

}


/// <summary>
/// Converts <see cref="IgTool.Hotkeys"/> to <see cref="string[]"/> and vice versa.
/// </summary>
public class HotkeyListJsonConverter : JsonConverter<List<Hotkey>>
{
    public override List<Hotkey>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray) return null;

        reader.Read();

        var elements = new Stack<string>();
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            elements.Push(JsonSerializer.Deserialize<string>(ref reader, options)!);
            reader.Read();
        }

        return elements.Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(i => new Hotkey(i))
            .ToList();
    }


    public override void Write(Utf8JsonWriter writer, List<Hotkey> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        var reversed = new Stack<Hotkey>(value);

        foreach (var item in reversed)
        {
            JsonSerializer.Serialize(writer, item.ToString(), options);
        }

        writer.WriteEndArray();
    }
}

