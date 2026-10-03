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
using Avalonia.Media;
using ImageGlass.Common.Extensions;

namespace ImageGlass.Common.AppThemes;


public static class AppThemeColors
{
    // Theme colors
    public static ISolidColorBrush TextColorBrush { get; private set; } = Brushes.Transparent;
    public static ISolidColorBrush BgBrush { get; private set; } = Brushes.Transparent;
    public static ISolidColorBrush ToolbarBgBrush { get; private set; } = Brushes.Transparent;
    public static ISolidColorBrush GalleryBgBrush { get; private set; } = Brushes.Transparent;
    public static ISolidColorBrush MenuBgBrush { get; private set; } = Brushes.White;


    /// <summary>
    /// Checks if the theme pack colors are fully transparent.
    /// </summary>
    public static bool IsFullTransparent => AppThemeColors.BgBrush.A == 0
        && AppThemeColors.ToolbarBgBrush.A == 0
        && AppThemeColors.GalleryBgBrush.A == 0;


    // Static window background Colors
    public static Color BackgroundActivateDark { get; } = BHelper.ColorFromHex("#151b1f");
    public static Color BackgroundActivateLight { get; } = Colors.White;
    public static Color BackgroundInactivateDark { get; } = BHelper.ColorFromHex("#212121");
    public static Color BackgroundInactivateLight { get; } = BHelper.ColorFromHex("#f3f3f3");


    // Static Situational Colors
    // Light theme
    public static Color BackgroundInfoLight { get; } = BHelper.ColorFromHex("#C2DAECBB");
    public static Color BackgroundSuccessLight { get; } = BHelper.ColorFromHex("#DFF6DDBB");
    public static Color BackgroundWarningLight { get; } = BHelper.ColorFromHex("#FFF4CEBB");
    public static Color BackgroundDangerLight { get; } = BHelper.ColorFromHex("#FDE7E9BB");

    public static Color TextSuccessLight { get; } = BHelper.ColorFromHex("#0F7B0F");
    public static Color TextWarningLight { get; } = BHelper.ColorFromHex("#9D5D00");
    public static Color TextDangerLight { get; } = BHelper.ColorFromHex("#C42B1C");


    // Dark theme
    public static Color BackgroundInfoDark { get; } = BHelper.ColorFromHex("#1A3244BB");
    public static Color BackgroundSuccessDark { get; } = BHelper.ColorFromHex("#393D1BBB");
    public static Color BackgroundWarningDark { get; } = BHelper.ColorFromHex("#433519BB");
    public static Color BackgroundDangerDark { get; } = BHelper.ColorFromHex("#442726BB");

    public static Color TextSuccessDark { get; } = BHelper.ColorFromHex("#6CCB5F");
    public static Color TextWarningDark { get; } = BHelper.ColorFromHex("#FCE100");
    public static Color TextDangerDark { get; } = BHelper.ColorFromHex("#FF99A4");



    /// <summary>
    /// Computes colors from the color strings.
    /// </summary>
    public static void Load(IgThemeColors colors, Color? accent = null)
    {
        // Viewer
        var color = BHelper.ColorFromHex(colors.TextColor, accent);
        if (TextColorBrush.Color != color)
        {
            TextColorBrush = color.ToBrush();
        }
        color = BHelper.ColorFromHex(colors.BgColor, accent);
        if (BgBrush.Color != color)
        {
            BgBrush = color.ToBrush();
        }


        // Toolbar
        color = BHelper.ColorFromHex(colors.ToolbarBgColor, accent);
        if (ToolbarBgBrush.Color != color)
        {
            ToolbarBgBrush = color.ToBrush();
        }


        // Gallery
        color = BHelper.ColorFromHex(colors.GalleryBgColor, accent);
        if (GalleryBgBrush.Color != color)
        {
            GalleryBgBrush = color.ToBrush();
        }


        // Menu
        color = BHelper.ColorFromHex(colors.MenuBgColor, accent);
        if (MenuBgBrush.Color != color)
        {
            MenuBgBrush = color.ToBrush();
        }
    }

}
