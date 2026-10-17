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
using System.Collections.Generic;

namespace ImageGlass.Common.Printing;


/// <summary>
/// The Print window's last choices, kept in memory for this session only; counts such as copies always start at 1.
/// </summary>
public sealed class PrintConfig
{
    private static PrintConfig? _saved;


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
    public PrintFitMode Fit { get; set; } = PrintFitMode.Fit;
    public PrintOrientation Orientation { get; set; } = PrintOrientation.Auto;
    public PrintMarginPreset Margins { get; set; } = PrintMarginPreset.Normal;
    public bool AutoRotate { get; set; } = true;
    public bool ShowCaptions { get; set; }
    public bool FillPage { get; set; }
    public bool Collate { get; set; } = true;
    public PrintColorMode ColorMode { get; set; } = PrintColorMode.Color;
    public PrintDuplex Duplex { get; set; } = PrintDuplex.None;


    /// <summary>
    /// Loads the choices saved earlier in this session, or the defaults.
    /// </summary>
    public static PrintConfig Load() => _saved?.Clone() ?? new PrintConfig();


    /// <summary>
    /// Keeps the choices for the next Print window of this session.
    /// </summary>
    public void Save() => _saved = Clone();


    private PrintConfig Clone()
    {
        var copy = (PrintConfig)MemberwiseClone();
        copy.PaperByPrinter = new(PaperByPrinter);
        copy.DpiByPrinter = new(DpiByPrinter);

        return copy;
    }
}
