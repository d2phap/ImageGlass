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
using ImageGlass.Common.Types.JsonTypeConverters;
using ImageGlass.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Printing;


[JsonSerializable(typeof(PrintConfig))]
public partial class PrintConfigJsonContext : JsonSerializerContext { }


/// <summary>
/// The Print window's last choices, saved in the tool settings of the config.
/// </summary>
public sealed class PrintConfig
{
    /// <summary>
    /// The key the settings are saved under; it only follows the tool settings naming.
    /// </summary>
    public const string SETTINGS_KEY = "Tool_Print";


    public string? LastPrinterId { get; set; }

    /// <summary>
    /// Gets, sets the last paper of each printer, by printer id.
    /// </summary>
    public Dictionary<string, string> PaperByPrinter { get; set; } = [];

    /// <summary>
    /// Gets, sets the last resolution of each printer, by printer id.
    /// </summary>
    public Dictionary<string, int> DpiByPrinter { get; set; } = [];

    public string LayoutId { get; set; } = PrintLayouts.FULL_PAGE_ID;

    [JsonConverter(typeof(JsonStringEnumSafeConverter<PrintFitMode>))]
    public PrintFitMode Fit { get; set; } = PrintFitMode.Fit;

    [JsonConverter(typeof(JsonStringEnumSafeConverter<PrintOrientation>))]
    public PrintOrientation Orientation { get; set; } = PrintOrientation.Auto;

    [JsonConverter(typeof(JsonStringEnumSafeConverter<PrintMarginPreset>))]
    public PrintMarginPreset Margins { get; set; } = PrintMarginPreset.Normal;

    public bool AutoRotate { get; set; } = true;
    public bool ShowCaptions { get; set; }
    public int PrintsPerItem { get; set; } = 1;
    public bool FillPage { get; set; }
    public bool Collate { get; set; } = true;

    [JsonConverter(typeof(JsonStringEnumSafeConverter<PrintColorMode>))]
    public PrintColorMode ColorMode { get; set; } = PrintColorMode.Color;

    [JsonConverter(typeof(JsonStringEnumSafeConverter<PrintDuplex>))]
    public PrintDuplex Duplex { get; set; } = PrintDuplex.None;


    /// <summary>
    /// Loads the saved settings, or the defaults when there are none or they cannot be read.
    /// </summary>
    public static PrintConfig Load()
    {
        try
        {
            if (Core.Config.ToolSettings.TryGetValue(SETTINGS_KEY, out var el)
                && el.Deserialize(PrintConfigJsonContext.Default.PrintConfig) is { } config)
            {
                return config;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(PrintConfig)}.{nameof(Load)}: {ex.Message}");
        }

        return new PrintConfig();
    }


    /// <summary>
    /// Writes the settings into the config, which saves them with the rest of it.
    /// </summary>
    public void Save()
    {
        ToolRegistry.SetToolSettings(SETTINGS_KEY, JsonSerializer.SerializeToElement(this, PrintConfigJsonContext.Default.PrintConfig));
    }
}
