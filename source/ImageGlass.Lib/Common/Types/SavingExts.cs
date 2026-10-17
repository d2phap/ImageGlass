/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ImageGlass.Common.Types;


public static class SavingExts
{
    /// <summary>
    /// The formats the app itself offers, in a hand-tuned order the picker keeps.
    /// </summary>
    private static IReadOnlyCollection<KeyValuePair<string, string>> BuiltInExtensions =>
    [
        new(".png",   "PNG"),
        new(".jpg",   "JPG"),
        new(".jxl",   "JXL"),
        new(".webp",  "WEBP"),
        new(".avif",  "AVIF"),

        new(".bmp",   "BMP"),
        new(".gif",   "GIF"),
        new(".tiff",  "TIFF"),

        new(".emf",   "EMF"),
        new(".exif",  "EXIF"),
        new(".ico",   "ICO"),
        new(".wmf",   "WMF"),
    ];


    /// <summary>
    /// Gets the file picker choices for save file dialog, including the formats contributed by
    /// loaded codec plugins. Rebuilt on every get, so enabling a plugin takes effect immediately.
    /// </summary>
    public static ImmutableList<FilePickerFileType> FilePickerFileTypeChoices => BuildChoices();


    /// <summary>
    /// Gets, sets the last Save As extension, e.g. <c>.jxl</c>; text because the choices are rebuilt per get.
    /// </summary>
    public static string? LastSavedExtension { get; set; }


    /// <summary>
    /// Finds the choice that writes <paramref name="ext"/>, or <c>null</c> when none does.
    /// </summary>
    public static FilePickerFileType? FindChoice(IEnumerable<FilePickerFileType> choices, string? ext)
    {
        if (string.IsNullOrEmpty(ext)) return null;

        var pattern = $"*{ext}";
        var choice = choices.FirstOrDefault(c => c.Patterns?.Contains(pattern, StringComparer.OrdinalIgnoreCase) is true);

        return choice;
    }


    /// <summary>
    /// Builds the picker list: the built-ins in their curated order, then any plugin-only format,
    /// highest encode priority first. Exactly one entry per extension.
    /// </summary>
    private static ImmutableList<FilePickerFileType> BuildChoices()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = new List<FilePickerFileType>(BuiltInExtensions.Count + 8);

        foreach (var (ext, label) in BuiltInExtensions)
        {
            if (!seen.Add(ext)) continue;
            list.Add(NewChoice(ext, label));
        }

        foreach (var plugin in GetPluginExtensions())
        {
            // a built-in already owns this slot; who actually writes it is decided at save time
            if (!seen.Add(plugin.Ext)) continue;
            list.Add(NewChoice(plugin.Ext, plugin.Ext.TrimStart('.').ToUpperInvariant()));
        }

        return list.ToImmutableList();
    }


    /// <summary>
    /// Builds one picker entry naming the codec that would write it, e.g. <c>JXL – WIC Codec
    /// (*.jxl)</c>. The codec segment is dropped when nothing can write the format.
    /// </summary>
    private static FilePickerFileType NewChoice(string ext, string label)
    {
        var codecName = EncoderNameOf(ext);
        var name = codecName.Length > 0
            ? $"{label} – {codecName} (*{ext})"
            : $"{label} (*{ext})";

        return new FilePickerFileType(name) { Patterns = [$"*{ext}"] };
    }


    /// <summary>
    /// Friendly name of the codec that would write <paramref name="ext"/>, empty when none can.
    /// </summary>
    private static string EncoderNameOf(string ext)
    {
        try
        {
            return Core.CodecRegistry.GetEncodeCodecInfo(ext)?.CodecName ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }


    /// <summary>
    /// Plugin-contributed writable extensions, ordered by encode priority then name.
    /// </summary>
    private static List<(string Ext, int EncodePriority)> GetPluginExtensions()
    {
        try
        {
            return Core.CodecRegistry.GetEncodingExtensions()
                .Where(e => e.IsPlugin)
                .Select(e => (e.Ext, e.EncodePriority))
                .OrderByDescending(e => e.EncodePriority)
                .ThenBy(e => e.Ext, StringComparer.Ordinal)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

}
