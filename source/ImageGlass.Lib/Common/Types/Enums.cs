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
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Types;


/// <summary>
/// Types of path
/// </summary>
public enum PathType
{
    File,
    Dir,
    Unknown,
}


/// <summary>
/// Registry scope used to register the app as the default photo viewer.
/// </summary>
public enum DefaultAppScope
{
    /// <summary>
    /// Per-user registration under <c>HKEY_CURRENT_USER</c> (portable / user-profile install).
    /// </summary>
    CurrentUser,

    /// <summary>
    /// Per-machine registration under <c>HKEY_LOCAL_MACHINE</c> (all-users / Program Files install).
    /// </summary>
    LocalMachine,
}


/// <summary>
/// Window backdrop effect.
/// </summary>
public enum BackdropStyle
{
    Mica,
    MicaAlt,
    Acrylic,

    /// <summary>
    /// No backdrop.
    /// </summary>
    None,
}


/// <summary>
/// Exit codes of ImageGlass ultilities
/// </summary>
public enum IgExitCode
{
    Done = 0,
    AdminRequired = 1,
    Error = 2,
    Error_FileNotFound = 3,
}


/// <summary>
/// Options indicate what source of image is saved.
/// </summary>
public enum ImageSaveSource
{
    Undefined,
    SelectedArea,
    Clipboard,
    CurrentFile,
}


/// <summary>
/// The loading order list.
/// **If we need to rename, we MUST update the language string too.
/// Because the name is also language keyword!
/// </summary>
public enum ImageOrderBy
{
    Name = 0,
    Random,
    FileSize,
    Extension,
    DateCreated,
    DateAccessed,
    DateModified,
    ExifDateTaken,
    ExifRating,
}


/// <summary>
/// The loading order types list
/// **If we need to rename, we MUST update the language string too.
/// Because the name is also language keyword!
/// </summary>
public enum ImageOrderType
{
    Asc = 0,
    Desc = 1,
}


/// <summary>
/// How the app behaves while browsing photos.
/// **If we need to rename, we MUST update the language string too.
/// Because the name is also language keyword!
/// </summary>
public enum BrowsingMode
{
    /// <summary>
    /// Navigation never waits: a photo still loading is replaced by the next one,
    /// showing a preview while the user keeps browsing.
    /// </summary>
    Turbo = 0,

    /// <summary>
    /// Navigation is ignored until the current photo is fully loaded and rendered,
    /// so every photo browsed to is displayed in full.
    /// </summary>
    Sequential = 1,
}


/// <summary>
/// Specifies where and how the image information overlay shows; the names are also language keys.
/// </summary>
public enum ImageInfoOverlayMode
{
    /// <summary>
    /// Never shown.
    /// </summary>
    Hidden = 0,

    /// <summary>
    /// Shown in every window mode.
    /// </summary>
    Always = 1,

    /// <summary>
    /// Shown in every window mode, while the pointer is near it.
    /// </summary>
    AlwaysAutoHide = 2,

    /// <summary>
    /// Shown while the window has no title bar: frameless or full screen.
    /// </summary>
    Borderless = 3,

    /// <summary>
    /// Shown while the window has no title bar, and the pointer is near it.
    /// </summary>
    BorderlessAutoHide = 4,
}


/// <summary>
/// Image resampling methods. Member names and order mirror Magick.NET's <c>FilterType</c>
/// (with <see cref="Auto"/> in place of <c>Undefined</c>), so a value casts directly to its filter.
/// </summary>
public enum ImageResamplingMethod : int
{
    Auto = 0,
    Point,
    Box,
    Triangle,
    Hermite,
    Hann,
    Hamming,
    Blackman,
    Gaussian,
    Quadratic,
    Cubic,
    Catrom,
    Mitchell,
    Jinc,
    Sinc,
    SincFast,
    Kaiser,
    Welch,
    Parzen,
    Bohman,
    Bartlett,
    Lagrange,
    Lanczos,
    LanczosSharp,
    Lanczos2,
    Lanczos2Sharp,
    Robidoux,
    RobidouxSharp,
    Cosine,
    Spline,
    LanczosRadius,
    CubicSpline,
    MagicKernelSharp2013,
    MagicKernelSharp2021,
}


public enum OSType
{
    Unknown,
    Windows,
    Mac,
    Linux,
}


/// <summary>
/// Specifies the available controls for app layout.
/// </summary>
public enum LayoutControl
{
    Toolbar,
    Gallery,
}


/// <summary>
/// Specifies the position for app layout.
/// </summary>
[JsonConverter(typeof(JsonStringEnumSafeConverter<LayoutPosition>))]
public enum LayoutPosition
{
    Top,
    Bottom,
    Left,
    Right,
}
