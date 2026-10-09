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
using System;

namespace ImageGlass.UI.Viewer;


public enum PhotoSource
{
    None,
    Native,
    VectorRenderer,
}


/// <summary>
/// Specifies the display styles for the background texture grid
/// </summary>
public enum CheckerboardType
{
    /// <summary>
    /// No background.
    /// </summary>
    None = 0,

    /// <summary>
    /// Background is displayed in the control's client area.
    /// </summary>
    Client = 1,

    /// <summary>
    /// Background is displayed only in the image region.
    /// </summary>
    Image = 2,
}


/// <summary>
/// Specifies how the viewer shows its scrollbars when the image does not fit in it.
/// </summary>
public enum ScrollbarMode
{
    /// <summary>
    /// No scrollbars.
    /// </summary>
    Never = 0,

    /// <summary>
    /// Thin scrollbars that expand while the pointer is over them.
    /// </summary>
    AutoHide = 1,

    /// <summary>
    /// Full scrollbars at all times.
    /// </summary>
    Always = 2,
}


public enum ZoomMode
{
    AutoZoom,
    LockZoom,
    ScaleToWidth,
    ScaleToHeight,
    ScaleToFit,
    ScaleToFill,
}


public enum ZoomChangeSource
{
    Unknown,
    ZoomMode,
    SizeChanged,

    /// <summary>
    /// Re-fitted for an animation frame of a different size.
    /// </summary>
    FrameChanged,
}


[Flags]
public enum AnimationSources
{
    None = 0,

    PanLeft = 1 << 1,
    PanRight = 1 << 2,
    PanUp = 1 << 3,
    PanDown = 1 << 4,

    /// <summary>
    /// Zoom in animation. It does nothing if <see cref="ViewerCanvas.ZoomLevels"/> is set.
    /// </summary>
    ZoomIn = 1 << 5,
    /// <summary>
    /// Zoom out animation. It does nothing if <see cref="ViewerCanvas.ZoomLevels"/> is set.
    /// </summary>
    ZoomOut = 1 << 6,
}


/// <summary>
/// Interpolation modes.
/// These values are based on <see cref="Avalonia.Media.Imaging.BitmapInterpolationMode"/>.
/// </summary>
public enum ImageInterpolation : int
{
    /// <summary>
    /// Nearest-neighbor. No blending. 
    /// Best for: Pixel art, 'retro' aesthetics, or viewing individual pixels when zoomed in > 1000%.
    /// Performance: Fastest.
    /// </summary>
    Nearest,

    /// <summary>
    /// Nearest-neighbor with nearest mipmap level.
    /// Best for: Decreasing aliasing in high-speed thumbnail scrolling where quality is secondary to speed.
    /// </summary>
    NearestMipmapNearest,

    /// <summary>
    /// Nearest-neighbor with linear interpolation between mipmap levels.
    /// Best for: Specific GPU-heavy texture effects; rarely used for standard photo viewing.
    /// </summary>
    NearestMipmapLinear,

    /// <summary>
    /// Bilinear filtering. 
    /// Best for: General purpose scaling and fast UI transformations (panning/zooming).
    /// Performance: High.
    /// </summary>
    Linear,

    /// <summary>
    /// Bilinear filtering with nearest mipmap level.
    /// Best for: Moderate quality downscaling with lower memory bandwidth than Trilinear.
    /// </summary>
    LinearMipmapNearest,

    /// <summary>
    /// Trilinear filtering (Bilinear + Mipmap interpolation).
    /// Best for: High-quality downscaling during smooth zoom animations. Prevents 'shimmering'.
    /// </summary>
    LinearMipmapLinear,

    /// <summary>
    /// Bicubic (Mitchell-Netravali).
    /// Best for: High-quality downscaling of photographic content. Provides a smooth, natural look.
    /// Performance: Moderate (higher CPU/GPU cost than Linear).
    /// </summary>
    CubicMitchell,

    /// <summary>
    /// Bicubic (Catmull-Rom).
    /// Best for: Upscaling or when a sharper result is desired. 
    /// Note: May produce slight 'ringing' artifacts on high-contrast edges.
    /// </summary>
    CubicCatmullRom,

}


public class AnimationSourceArgs(AnimationSources source, Action? callbackFn)
{
    public AnimationSources Source => source;
    public Action? CallbackFn => callbackFn;
}

