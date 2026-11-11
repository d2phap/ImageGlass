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
using Avalonia;
using ImageGlass.Common.Extensions;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Types.JsonTypeConverters;


/// <summary>
/// Converts an <see cref="PixelRect"/> of strings to a single delimited string
/// and vice versa.
/// </summary>
public class JsonArrayToPixelRectConverter : JsonConverter<PixelRect>
{
    private readonly string _delimiter = ";";

    public override void Write(Utf8JsonWriter writer, PixelRect rect, JsonSerializerOptions options)
    {
        // convert Rect to string
        var str = rect.ToRect(1).ToStringDelimiter();

        writer.WriteStringValue(str);
    }

    public override PixelRect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        var arr = str
            ?.Split(_delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(v =>
            {
                if (double.TryParse(v, out var numberValue)) return numberValue;
                return double.NaN;
            })
            .Where(v => !double.IsNaN(v))
            .ToArray() ?? [];

        var x = arr.Length > 0 ? arr[0] : 0;
        var y = arr.Length > 1 ? arr[1] : 0;
        var width = arr.Length > 2 ? arr[2] : 0;
        var height = arr.Length > 3 ? arr[3] : 0;

        width = Math.Max(0, width);
        height = Math.Max(0, height);

        var rect = new PixelRect((int)x, (int)y, (int)width, (int)height);

        return rect;
    }
}
