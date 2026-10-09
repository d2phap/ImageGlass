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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ImageGlass.Common.Printing;


public enum PrintLayoutKind
{
    FullPage,
    Grid,
    FixedSize,
    Wallet,
    Passport,
    ContactSheet,
}


/// <summary>
/// A way to place prints on a page.
/// </summary>
public sealed record PrintLayout
{
    /// <summary>
    /// Gets the id the layout is saved by.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the kind of layout.
    /// </summary>
    public required PrintLayoutKind Kind { get; init; }

    /// <summary>
    /// Gets the number of prints per page of a grid.
    /// </summary>
    public int CellCount { get; init; } = 1;

    /// <summary>
    /// Gets the size of one print of a fixed-size layout, in points, portrait.
    /// </summary>
    public SKSize PrintSizePt { get; init; }

    /// <summary>
    /// Gets the columns and rows of a contact sheet on a portrait page.
    /// </summary>
    public (int Columns, int Rows) Grid { get; init; } = (1, 1);

    /// <summary>
    /// Gets a name that needs no translation, such as "10 × 15 cm"; <c>null</c> when the UI names the layout.
    /// </summary>
    public string? FixedName { get; init; }
}


/// <summary>
/// The layouts the Print window offers, sized for the user's region.
/// </summary>
public static class PrintLayouts
{
    public const string FULL_PAGE_ID = "full";

    /// <summary>
    /// Gets the full-page layout, the default.
    /// </summary>
    public static PrintLayout FullPage { get; } = new() { Id = FULL_PAGE_ID, Kind = PrintLayoutKind.FullPage };


    /// <summary>
    /// Gets the layouts for a metric or an imperial region: full page, grids, photo sizes, wallet, passport, contact sheet.
    /// </summary>
    public static IReadOnlyList<PrintLayout> GetAll(bool metric)
    {
        var list = new List<PrintLayout> { FullPage, GridOf(2), GridOf(4), GridOf(6), GridOf(9) };

        if (metric)
        {
            list.Add(FixedCm(9, 13));
            list.Add(FixedCm(10, 15));
            list.Add(FixedCm(13, 18));
            list.Add(FixedCm(20, 25));
        }
        else
        {
            list.Add(FixedIn(3.5, 5));
            list.Add(FixedIn(4, 6));
            list.Add(FixedIn(5, 7));
            list.Add(FixedIn(8, 10));
        }

        // wallet prints are 2.5 x 3.5 in everywhere; passport photos differ by region
        list.Add(new() { Id = "wallet", Kind = PrintLayoutKind.Wallet, PrintSizePt = new(PrintUnits.InToPt(2.5), PrintUnits.InToPt(3.5)) });
        list.Add(metric
            ? new() { Id = "passport-35x45mm", Kind = PrintLayoutKind.Passport, PrintSizePt = new(PrintUnits.MmToPt(35), PrintUnits.MmToPt(45)), FixedName = "35 × 45 mm" }
            : new() { Id = "passport-2x2in", Kind = PrintLayoutKind.Passport, PrintSizePt = new(PrintUnits.InToPt(2), PrintUnits.InToPt(2)), FixedName = "2 × 2 in" });

        list.Add(new() { Id = "contact", Kind = PrintLayoutKind.ContactSheet, Grid = (5, 7) });
        return list;
    }


    /// <summary>
    /// Finds a layout by id among both regions' layouts, so a saved choice survives a region change; else the full page.
    /// </summary>
    public static PrintLayout Find(string? id)
    {
        if (string.IsNullOrEmpty(id)) return FullPage;

        return GetAll(true).Concat(GetAll(false)).FirstOrDefault(i => i.Id == id) ?? FullPage;
    }


    private static PrintLayout GridOf(int count) => new() { Id = $"grid-{count}", Kind = PrintLayoutKind.Grid, CellCount = count };


    private static PrintLayout FixedCm(double w, double h) => new()
    {
        Id = string.Create(CultureInfo.InvariantCulture, $"fixed-{w}x{h}cm"),
        Kind = PrintLayoutKind.FixedSize,
        PrintSizePt = new(PrintUnits.MmToPt(w * 10), PrintUnits.MmToPt(h * 10)),
        FixedName = $"{w} × {h} cm",
    };


    private static PrintLayout FixedIn(double w, double h) => new()
    {
        Id = string.Create(CultureInfo.InvariantCulture, $"fixed-{w}x{h}in"),
        Kind = PrintLayoutKind.FixedSize,
        PrintSizePt = new(PrintUnits.InToPt(w), PrintUnits.InToPt(h)),
        FixedName = $"{w} × {h} in",
    };
}
