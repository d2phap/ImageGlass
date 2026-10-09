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
using Avalonia;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using SkiaSharp;
using System;
using System.Threading;

namespace ImageGlass.UI.Viewer;


/// <summary>
/// The part of the shown image to copy or print.
/// </summary>
public enum ViewerImageRegion
{
    WholeImage,
    Selection,
    VisibleArea,
}


/// <summary>
/// What the viewer showed at one moment: its edits plus the shown image, kept alive until this is disposed.
/// </summary>
public sealed class ViewerImageState : IDisposable
{
    private SKImageRef.ImageLease? _lease;
    private SKImage? _ownedImage;


    /// <summary>
    /// Gets the photo shown.
    /// </summary>
    public required Photo Photo { get; init; }

    /// <summary>
    /// Gets whether the photo is a vector image; <see cref="Image"/> is then only its raster fallback.
    /// </summary>
    public bool IsVector { get; init; }

    /// <summary>
    /// Gets whether the photo is an animation, whose frames take no edits.
    /// </summary>
    public bool IsAnimated { get; init; }

    /// <summary>
    /// Gets the index of the frame or page shown.
    /// </summary>
    public int FrameIndex { get; init; }

    /// <summary>
    /// Gets the orientation the photo is shown in.
    /// </summary>
    public ImageOrientation Orientation { get; init; } = ImageOrientation.Identity;

    /// <summary>
    /// Gets whether the colors are shown inverted.
    /// </summary>
    public bool IsColorInverted { get; init; }

    /// <summary>
    /// Gets the color channels shown.
    /// </summary>
    public ColorChannels Channels { get; init; } = ColorChannels.RGBA;

    /// <summary>
    /// Gets the selection in pixels of <see cref="Image"/>; empty when there is none.
    /// </summary>
    public Rect SelectionRect { get; init; }

    /// <summary>
    /// Gets the part of <see cref="Image"/> visible in the viewer, in its pixels.
    /// </summary>
    public Rect VisibleRect { get; init; }

    /// <summary>
    /// Gets whether <see cref="Image"/> is still the low-resolution preview of the photo.
    /// </summary>
    public bool IsPreview { get; init; }

    /// <summary>
    /// Gets the tone mapping the viewer applied to an HDR photo, or <c>null</c> when it applied none.
    /// </summary>
    public HdrToneMappingOptions? HdrToneMapping { get; init; }

    /// <summary>
    /// Gets whether <see cref="Image"/> is tagged with the color space its pixels are in, so drawing it elsewhere converts exactly.
    /// </summary>
    public bool HasExactColorTag { get; init; } = true;

    /// <summary>
    /// Gets the shown image, edits applied, valid until this is disposed.
    /// </summary>
    public SKImage? Image => _ownedImage ?? _lease?.Image;


    internal ViewerImageState(SKImageRef.ImageLease? lease, SKImage? ownedImage)
    {
        _lease = lease;
        _ownedImage = ownedImage;
    }


    /// <summary>
    /// Gets the pixel rectangle of a region within <see cref="Image"/>; empty when the region is.
    /// </summary>
    public SKRectI GetRegionRect(ViewerImageRegion region)
    {
        var img = Image;
        if (img.IsDisposed()) return SKRectI.Empty;

        var bounds = new Rect(0, 0, img.Width, img.Height);
        var rect = region switch
        {
            ViewerImageRegion.Selection => SelectionRect.Normalize(),
            ViewerImageRegion.VisibleArea => VisibleRect.Normalize(),
            _ => bounds,
        };

        // a selection can reach past the image edges
        var clipped = rect.GetIntersection(bounds);
        return clipped.IsEmpty ? SKRectI.Empty : clipped.ToSKRectI();
    }


    /// <summary>
    /// Copies the pixels of a region into a new bitmap the caller owns; safe on any thread.
    /// </summary>
    public SKBitmap? CopyPixels(ViewerImageRegion region = ViewerImageRegion.WholeImage)
    {
        var img = Image;
        if (img.IsDisposed()) return null;

        var rect = GetRegionRect(region);
        if (rect.IsEmpty) return null;

        var info = new SKImageInfo(rect.Width, rect.Height, img.ColorType, img.AlphaType, img.ColorSpace);
        var bmp = new SKBitmap(info);

        if (!img.ReadPixels(info, bmp.GetPixels(), bmp.RowBytes, rect.Left, rect.Top))
        {
            bmp.Dispose();
            return null;
        }

        return bmp;
    }


    /// <summary>
    /// Releases the shown image.
    /// </summary>
    public void Dispose()
    {
        Interlocked.Exchange(ref _lease, null)?.Dispose();
        Interlocked.Exchange(ref _ownedImage, null)?.Dispose();
    }
}
