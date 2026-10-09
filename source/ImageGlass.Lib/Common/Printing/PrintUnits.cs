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
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ImageGlass.Common.Printing;


/// <summary>
/// Converts between points (1/72 in), the unit of every print layout, and millimeters, inches and pixels.
/// </summary>
public static class PrintUnits
{
    public const float POINTS_PER_INCH = 72f;
    public const double MM_PER_INCH = 25.4;


    /// <summary>
    /// Gets whether the user's region measures in metric units.
    /// </summary>
    public static bool IsMetricRegion
    {
        get
        {
            try
            {
                return RegionInfo.CurrentRegion.IsMetric;
            }
            catch
            {
                return true;
            }
        }
    }


    public static float MmToPt(double mm) => (float)(mm / MM_PER_INCH * POINTS_PER_INCH);
    public static float InToPt(double inches) => (float)(inches * POINTS_PER_INCH);
    public static double PtToMm(double pt) => pt / POINTS_PER_INCH * MM_PER_INCH;
    public static double PtToIn(double pt) => pt / POINTS_PER_INCH;
    public static float PtToPx(float pt, float dpi) => pt * dpi / POINTS_PER_INCH;


    /// <summary>
    /// Formats a size in points as "210 × 297 mm" or "8.5 × 11 in".
    /// </summary>
    public static string FormatSize(SKSize sizePt, bool metric)
    {
        return metric
            ? $"{FormatNumber(PtToMm(sizePt.Width), 0)} × {FormatNumber(PtToMm(sizePt.Height), 0)} mm"
            : $"{FormatNumber(PtToIn(sizePt.Width), 2)} × {FormatNumber(PtToIn(sizePt.Height), 2)} in";
    }


    private static string FormatNumber(double value, int decimals)
    {
        return Math.Round(value, decimals).ToString("0.##", CultureInfo.CurrentCulture);
    }
}


/// <summary>
/// The paper sizes of the app's own destinations, such as Save as PDF, and the default among them.
/// </summary>
public static class PaperCatalog
{
    /// <summary>
    /// Gets the built-in paper sizes, ISO and US office sizes, then photo sizes.
    /// </summary>
    public static IReadOnlyList<PaperInfo> Papers { get; } =
    [
        Iso("A3", 297, 420),
        Iso("A4", 210, 297),
        Iso("A5", 148, 210),
        Iso("A6", 105, 148),
        Iso("B5", 176, 250),
        Us("Letter", 8.5, 11),
        Us("Legal", 8.5, 14),
        Us("Tabloid", 11, 17),
        Us("4 × 6 in", 4, 6, "photo-4x6in"),
        Us("5 × 7 in", 5, 7, "photo-5x7in"),
        Us("8 × 10 in", 8, 10, "photo-8x10in"),
        Iso("10 × 15 cm", 100, 150, "photo-10x15cm"),
        Iso("13 × 18 cm", 130, 180, "photo-13x18cm"),
    ];


    /// <summary>
    /// Gets the id of the default paper: Letter where the region is not metric, else A4.
    /// </summary>
    public static string DefaultPaperId => PrintUnits.IsMetricRegion ? "A4" : "Letter";


    /// <summary>
    /// Builds the display name of a paper: its name, then its size in the region's units when the name does not already state it.
    /// </summary>
    public static string GetDisplayName(string name, SKSize sizePt)
    {
        if (name.Contains('×')) return name;

        return $"{name} ({PrintUnits.FormatSize(sizePt, PrintUnits.IsMetricRegion)})";
    }


    private static PaperInfo Iso(string name, double widthMm, double heightMm, string? id = null)
    {
        var size = new SKSize(PrintUnits.MmToPt(widthMm), PrintUnits.MmToPt(heightMm));
        return new PaperInfo(id ?? name, GetDisplayName(name, size), size);
    }


    private static PaperInfo Us(string name, double widthIn, double heightIn, string? id = null)
    {
        var size = new SKSize(PrintUnits.InToPt(widthIn), PrintUnits.InToPt(heightIn));
        return new PaperInfo(id ?? name, GetDisplayName(name, size), size);
    }
}
