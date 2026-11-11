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
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Types.JsonTypeConverters;


/// <summary>
/// Converts an <see cref="double[]"/> of strings to a single delimited string
/// and vice versa.
/// </summary>
public class JsonArrayToZoomFactorConverter : JsonConverter<double[]>
{
    private readonly string _delimiter = ";";

    public override void Write(Utf8JsonWriter writer, double[] arr, JsonSerializerOptions options)
    {
        // convert double[] to string
        var numbers = arr.Select(v => v * 100f);
        var str = string.Join(_delimiter, numbers);

        writer.WriteStringValue(str);
    }

    public override double[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        var arr = str
            ?.Split(_delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(v =>
            {
                if (double.TryParse(v, out var numberValue))
                {
                    return numberValue / 100f;
                }

                return double.NaN;
            })
            .Where(v => !double.IsNaN(v))
            .ToArray() ?? [];

        return arr;
    }
}
