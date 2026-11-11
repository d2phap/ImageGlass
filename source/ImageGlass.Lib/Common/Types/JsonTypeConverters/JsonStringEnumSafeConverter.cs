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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Types.JsonTypeConverters;


/// <summary>
/// A resilient drop-in replacement for <see cref="System.Text.Json.Serialization.JsonStringEnumConverter{TEnum}"/>
/// that reads/writes enum values as their string names, but falls back to <c>default(TEnum)</c>
/// when the stored value cannot be matched to a member of <typeparamref name="TEnum"/>.
/// <para>
/// The framework converter throws on unknown values; this keeps a single stale or renamed enum
/// value — e.g. left over from an older app version — from aborting the entire settings load.
/// </para>
/// </summary>
public sealed class JsonStringEnumSafeConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            // string name, e.g. "LinearMipmapNearest" (case-insensitive)
            case JsonTokenType.String:
                if (Enum.TryParse<TEnum>(reader.GetString(), ignoreCase: true, out var named))
                    return named;
                break;

            // numeric underlying value, e.g. 4 (kept for backward compatibility)
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var raw))
                    return (TEnum)Enum.ToObject(typeof(TEnum), raw);
                break;
        }

        // unknown/stale value -> use the enum's default instead of throwing
        return default;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }


    /// <summary>
    /// Reads the enum as a dictionary key, falling back to <c>default(TEnum)</c> on an unknown name.
    /// </summary>
    public override TEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (Enum.TryParse<TEnum>(reader.GetString(), ignoreCase: true, out var named))
            return named;

        return default;
    }


    /// <summary>
    /// Writes the enum as a dictionary key (its string name).
    /// </summary>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.ToString());
    }
}
