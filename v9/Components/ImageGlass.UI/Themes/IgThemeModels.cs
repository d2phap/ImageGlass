/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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

namespace ImageGlass.UI;


/// <summary>
/// Metadata for theme config file
/// </summary>
public record IgThemeMetadata
{
    public string Description { get; set; } = "ImageGlass theme configuration file";

    /// <summary>
    /// Config version of this theme to work with (required)
    /// </summary>
    public string Version { get; set; } = "9.0";
}


/// <summary>
/// Theme information
/// </summary>
public record IgThemeInfo
{
    /// <summary>
    /// Theme name (required)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDark { get; set; } = true;
}


/// <summary>
/// Theme colors and others
/// </summary>
public record IgThemeSettings
{
    public Color AccentColor { get; set; } = ThemeUtils.ColorFromHex("#0078D7");
    public Color AccentHoverColor { get; set; } = ThemeUtils.ColorFromHex("#0d92ff");
    public Color AccentSelectedColor { get; set; } = ThemeUtils.ColorFromHex("#0060ae");
    public Color TextColor { get; set; } = ThemeUtils.ColorFromHex("#d3d3d3");
    public Color BgColor { get; set; } = ThemeUtils.ColorFromHex("#151b1f");


    // Toolbar
    public Color ToolbarBgColor { get; set; } = ThemeUtils.ColorFromHex("#242b31");
    public Color ToolbarTextColor { get; set; } = ThemeUtils.ColorFromHex("#dedede");
    public Color ToolbarItemHoverColor { get; set; } = ThemeUtils.ColorFromHex("#ffffff33");
    public Color ToolbarItemActiveColor { get; set; } = ThemeUtils.ColorFromHex("#ffffff22");
    public Color ToolbarItemSelectedColor { get; set; } = ThemeUtils.ColorFromHex("#ffffff44");


    // Gallery
    public Color ThumbnailBarBgColor { get; set; } = ThemeUtils.ColorFromHex("#242b31");
    public Color ThumbnailBarTextColor { get; set; } = ThemeUtils.ColorFromHex("#dedede");
    public Color ThumbnailItemHoverColor { get; set; } = ThemeUtils.ColorFromHex("#ffffff33");
    public Color ThumbnailItemActiveColor { get; set; } = ThemeUtils.ColorFromHex("#ffffff22");
    public Color ThumbnailItemSelectedColor { get; set; } = ThemeUtils.ColorFromHex("#ffffff44");


    // Menu
    public Color MenuBgColor { get; set; } = ThemeUtils.ColorFromHex("#242b31");
    public Color MenuBgHoverColor { get; set; } = ThemeUtils.ColorFromHex("#ffffff33");
    public Color MenuTextColor { get; set; } = ThemeUtils.ColorFromHex("#dedede");
    public Color MenuTextHoverColor { get; set; } = ThemeUtils.ColorFromHex("#dedede");


    /// <summary>
    /// Gets, sets the navigation left arrow
    /// </summary>
    public Bitmap? NavButtonLeft { get; set; }

    /// <summary>
    /// Gets, sets the navigation right arrow
    /// </summary>
    public Bitmap? NavButtonRight { get; set; }

    /// <summary>
    /// Sets, sets app logo
    /// </summary>
    public Bitmap AppLogo { get; set; } = Properties.Resources.DefaultLogo;

    /// <summary>
    /// Show or hide logo on title bar of window
    /// </summary>
    public bool IsShowTitlebarLogo { get; set; } = true;

    /// <summary>
    /// The preview image of the theme
    /// </summary>
    public Bitmap? PreviewImage { get; set; }
}


/// <summary>
/// Theme toolbar icons
/// </summary>
public class IgThemeToolbarIcons
{
    public Bitmap? ActualSize { get; set; }
    public Bitmap? AutoZoom { get; set; }
    public Bitmap? Checkerboard { get; set; }
    public Bitmap? ColorPicker { get; set; }
    public Bitmap? Crop { get; set; }
    public Bitmap? Delete { get; set; }
    public Bitmap? Edit { get; set; }
    public Bitmap? Exit { get; set; }
    public Bitmap? FlipHorz { get; set; }
    public Bitmap? FlipVert { get; set; }
    public Bitmap? FullScreen { get; set; }
    public Bitmap? GoToImage { get; set; }
    public Bitmap? LockZoom { get; set; }
    public Bitmap? MainMenu { get; set; }
    public Bitmap? OpenFile { get; set; }
    public Bitmap? Print { get; set; }
    public Bitmap? Refresh { get; set; }
    public Bitmap? RotateLeft { get; set; }
    public Bitmap? RotateRight { get; set; }
    public Bitmap? SaveAs { get; set; }
    public Bitmap? ScaleToFill { get; set; }
    public Bitmap? ScaleToFit { get; set; }
    public Bitmap? ScaleToHeight { get; set; }
    public Bitmap? ScaleToWidth { get; set; }
    public Bitmap? Slideshow { get; set; }
    public Bitmap? ThumbnailBar { get; set; }
    public Bitmap? ViewFirstImage { get; set; }
    public Bitmap? ViewLastImage { get; set; }
    public Bitmap? ViewNextImage { get; set; }
    public Bitmap? ViewPreviousImage { get; set; }
    public Bitmap? WindowFit { get; set; }
    public Bitmap? ZoomIn { get; set; }
    public Bitmap? ZoomOut { get; set; }
}


/// <summary>
/// JSON model for theme file
/// </summary>
/// <param name="_Metadata"></param>
/// <param name="Info"></param>
/// <param name="Settings"></param>
/// <param name="ToolbarIcons"></param>
public record IgThemeJsonModel(
    IgThemeMetadata _Metadata,
    IgThemeInfo Info,
    Dictionary<string, object> Settings,
    Dictionary<string, string> ToolbarIcons)
{ }
