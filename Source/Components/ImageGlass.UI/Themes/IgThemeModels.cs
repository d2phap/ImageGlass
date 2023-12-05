/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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

using ImageGlass.Base;
using WicNet;

namespace ImageGlass.UI;


/// <summary>
/// Metadata for theme config file
/// </summary>
public record IgThemeMetadata
{
    /// <summary>
    /// Config version of this theme to work with (required)
    /// </summary>
    public string Version { get; set; } = "9.0";

    public string Description { get; set; } = "ImageGlass theme configuration file";

    public string Docs { get; set; } = "Visit https://github.com/ImageGlass/theme to learn about ImageGlass theme pack specification";
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
}


/// <summary>
/// Theme colors
/// </summary>
public record IgThemeColors
{
    public Color TextColor { get; set; } = BHelper.ColorFromHex("#d3d3d3");
    public Color BgColor { get; set; } = BHelper.ColorFromHex("#151b1f00");


    // Toolbar
    public Color ToolbarBgColor { get; set; } = BHelper.ColorFromHex("#1E242900");
    public Color ToolbarTextColor { get; set; } = BHelper.ColorFromHex("#dedede");
    public Color ToolbarItemHoverColor { get; set; } = BHelper.ColorFromHex("#ffffff33");
    public Color ToolbarItemActiveColor { get; set; } = BHelper.ColorFromHex("#ffffff22");
    public Color ToolbarItemSelectedColor { get; set; } = BHelper.ColorFromHex("#ffffff44");


    // Gallery
    public Color GalleryBgColor { get; set; } = BHelper.ColorFromHex("#1E242900");
    public Color GalleryTextColor { get; set; } = BHelper.ColorFromHex("#dedede");
    public Color GalleryItemHoverColor { get; set; } = BHelper.ColorFromHex("#ffffff33");
    public Color GalleryItemActiveColor { get; set; } = BHelper.ColorFromHex("#ffffff22");
    public Color GalleryItemSelectedColor { get; set; } = BHelper.ColorFromHex("#ffffff44");


    // Menu
    public Color MenuBgColor { get; set; } = BHelper.ColorFromHex("#1E2429");
    public Color MenuBgHoverColor { get; set; } = BHelper.ColorFromHex("#ffffff15");
    public Color MenuTextColor { get; set; } = BHelper.ColorFromHex("#dedede");
    public Color MenuTextHoverColor { get; set; } = BHelper.ColorFromHex("#dedede");


    // Viewer
    public Color NavigationButtonColor { get; set; } = BHelper.ColorFromHex("ff000015");
}


/// <summary>
/// Theme other settings
/// </summary>
public record IgThemeSettings : IDisposable
{
    #region IDisposable Disposing

    private bool _isDisposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            // Free any other managed objects here.

            NavButtonLeft?.Dispose();
            NavButtonLeft = null;

            NavButtonRight?.Dispose();
            NavButtonRight = null;

            AppLogo?.Dispose();
            AppLogo = null;
        }

        // Free any unmanaged objects here.
        _isDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~IgThemeSettings()
    {
        Dispose(false);
    }

    #endregion


    /// <summary>
    /// Default value is <c>true</c>.
    /// </summary>
    public bool IsDarkMode { get; set; } = true;

    /// <summary>
    /// Gets, sets the navigation left arrow
    /// </summary>
    public WicBitmapSource? NavButtonLeft { get; set; }

    /// <summary>
    /// Gets, sets the navigation right arrow
    /// </summary>
    public WicBitmapSource? NavButtonRight { get; set; }

    /// <summary>
    /// Sets, sets app logo
    /// </summary>
    public Bitmap? AppLogo { get; set; } = Properties.Resources.DefaultLogo;

    /// <summary>
    /// The preview image of the theme
    /// </summary>
    public string PreviewImage { get; set; } = string.Empty;
}


/// <summary>
/// Theme toolbar icons
/// </summary>
public class IgThemeToolbarIcons : IDisposable
{
    public Bitmap? ActualSize { get; set; }
    public Bitmap? AutoZoom { get; set; }
    public Bitmap? Checkerboard { get; set; }
    public Bitmap? ColorPicker { get; set; }
    public Bitmap? Crop { get; set; }
    public Bitmap? Delete { get; set; }
    public Bitmap? Edit { get; set; }
    public Bitmap? Exit { get; set; }
    public Bitmap? Export { get; set; }
    public Bitmap? FlipHorz { get; set; }
    public Bitmap? FlipVert { get; set; }
    public Bitmap? FullScreen { get; set; }
    public Bitmap? Gallery { get; set; }
    public Bitmap? LockZoom { get; set; }
    public Bitmap? MainMenu { get; set; }
    public Bitmap? OpenFile { get; set; }
    public Bitmap? Pause { get; set; }
    public Bitmap? Play { get; set; }
    public Bitmap? Print { get; set; }
    public Bitmap? Refresh { get; set; }
    public Bitmap? RotateLeft { get; set; }
    public Bitmap? RotateRight { get; set; }
    public Bitmap? Save { get; set; }
    public Bitmap? ScaleToFill { get; set; }
    public Bitmap? ScaleToFit { get; set; }
    public Bitmap? ScaleToHeight { get; set; }
    public Bitmap? ScaleToWidth { get; set; }
    public Bitmap? Slideshow { get; set; }
    public Bitmap? ViewFirstImage { get; set; }
    public Bitmap? ViewLastImage { get; set; }
    public Bitmap? ViewNextImage { get; set; }
    public Bitmap? ViewPreviousImage { get; set; }
    public Bitmap? WindowFit { get; set; }
    public Bitmap? ZoomIn { get; set; }
    public Bitmap? ZoomOut { get; set; }


    #region IDisposable Disposing

    private bool _isDisposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            // Free any other managed objects here.

            ActualSize?.Dispose();
            ActualSize = null;

            AutoZoom?.Dispose();
            AutoZoom = null;

            Checkerboard?.Dispose();
            Checkerboard = null;

            ColorPicker?.Dispose();
            ColorPicker = null;

            Crop?.Dispose();
            Crop = null;

            Delete?.Dispose();
            Delete = null;

            Edit?.Dispose();
            Edit = null;

            Exit?.Dispose();
            Exit = null;

            FlipHorz?.Dispose();
            FlipHorz = null;

            FlipVert?.Dispose();
            FlipVert = null;

            FullScreen?.Dispose();
            FullScreen = null;

            LockZoom?.Dispose();
            LockZoom = null;

            MainMenu?.Dispose();
            MainMenu = null;

            OpenFile?.Dispose();
            OpenFile = null;

            Print?.Dispose();
            Print = null;

            Refresh?.Dispose();
            Refresh = null;

            RotateLeft?.Dispose();
            RotateLeft = null;

            RotateRight?.Dispose();
            RotateRight = null;

            Save?.Dispose();
            Save = null;

            ScaleToFill?.Dispose();
            ScaleToFill = null;

            ScaleToFit?.Dispose();
            ScaleToFit = null;

            ScaleToHeight?.Dispose();
            ScaleToHeight = null;

            ScaleToWidth?.Dispose();
            ScaleToWidth = null;

            Slideshow?.Dispose();
            Slideshow = null;

            Gallery?.Dispose();
            Gallery = null;

            ViewFirstImage?.Dispose();
            ViewFirstImage = null;

            ViewLastImage?.Dispose();
            ViewLastImage = null;

            ViewNextImage?.Dispose();
            ViewNextImage = null;

            ViewPreviousImage?.Dispose();
            ViewPreviousImage = null;

            WindowFit?.Dispose();
            WindowFit = null;

            ZoomIn?.Dispose();
            ZoomIn = null;

            ZoomOut?.Dispose();
            ZoomOut = null;

            Play?.Dispose();
            Play = null;

            Pause?.Dispose();
            Pause = null;

            Export?.Dispose();
            Export = null;
        }

        // Free any unmanaged objects here.
        _isDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~IgThemeToolbarIcons()
    {
        Dispose(false);
    }

    #endregion

}


/// <summary>
/// JSON model for theme file
/// </summary>
/// <param name="_Metadata"></param>
/// <param name="Info"></param>
/// <param name="Settings"></param>
/// <param name="Colors"></param>
/// <param name="ToolbarIcons"></param>
public record IgThemeJsonModel(
    IgThemeMetadata _Metadata,
    IgThemeInfo Info,
    Dictionary<string, object> Settings,
    Dictionary<string, object> Colors,
    Dictionary<string, string> ToolbarIcons)
{ }
