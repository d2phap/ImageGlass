/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using SkiaSharp;
using Svg.Skia;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;

/// <summary>
/// SVG loading and rasterization via Svg.Skia.
/// </summary>
public static class SvgCodec
{
    private static readonly string[] _svgExtensions = [".svg", ".svgz"];


    /// <summary>
    /// Checks if the file is an SVG file by extension.
    /// </summary>
    public static bool IsSvgFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;

        var ext = Path.GetExtension(filePath);
        foreach (var svgExt in _svgExtensions)
        {
            if (ext.Equals(svgExt, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }


    /// <summary>
    /// Loads an SVG document from a file path.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "SVG parsing requires reflection for element discovery; rendering is AOT-safe.")]
    public static SKSvg LoadSvg(string filePath)
    {
        return SKSvg.CreateFromFile(filePath);
    }


    /// <summary>
    /// Loads an SVG document from a stream.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "SVG parsing requires reflection for element discovery; rendering is AOT-safe.")]
    public static SKSvg LoadSvg(Stream stream)
    {
        return SKSvg.CreateFromStream(stream);
    }


    /// <summary>
    /// Extracts the intrinsic size from an SVG picture's CullRect.
    /// Handles SVGs with non-zero origin.
    /// </summary>
    public static Size GetIntrinsicSize(SKPicture picture)
    {
        var cullRect = picture.CullRect;
        return new Size(
            Math.Max(1, cullRect.Width),
            Math.Max(1, cullRect.Height));
    }


    /// <summary>
    /// Rasterizes an SVG picture to an <see cref="SKImage"/> at a scale
    /// that fits within <paramref name="maxDimension"/>.
    /// Used for gallery thumbnails and rasterized fallback.
    /// </summary>
    public static SKImage? RasterizeThumbnail(SKPicture picture, int maxDimension)
    {
        var cullRect = picture.CullRect;
        var w = cullRect.Width;
        var h = cullRect.Height;
        if (w <= 0 || h <= 0) return null;

        var scale = Math.Min((float)maxDimension / w, (float)maxDimension / h);
        scale = Math.Min(scale, 1f); // don't upscale beyond intrinsic size

        var targetW = (int)Math.Ceiling(w * scale);
        var targetH = (int)Math.Ceiling(h * scale);
        if (targetW <= 0 || targetH <= 0) return null;

        var info = new SKImageInfo(targetW, targetH, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        if (surface is null) return null;

        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // handle non-zero origin CullRect
        if (cullRect.Left != 0 || cullRect.Top != 0)
        {
            canvas.Translate(-cullRect.Left * scale, -cullRect.Top * scale);
        }

        canvas.Scale(scale, scale);
        canvas.DrawPicture(picture);

        return surface.Snapshot();
    }


    /// <summary>
    /// Loads SVG-specific metadata (size, vector flag, animation detection).
    /// </summary>
    public static async Task<PhotoMetadata> LoadMetadataAsync(string filePath, CancellationToken token = default)
    {
        var meta = new PhotoMetadata(filePath)
        {
            IsVector = true,
            FrameCount = 1,
            HasAlpha = true,
        };

        await Task.Run(() =>
        {
            try
            {
                using var svgDoc = LoadSvg(filePath);
                var picture = svgDoc.Picture;
                if (picture is not null)
                {
                    var size = GetIntrinsicSize(picture);
                    meta.OriginalWidth = meta.Width = (uint)size.Width;
                    meta.OriginalHeight = meta.Height = (uint)size.Height;
                }

                // detect SMIL animations
                if (svgDoc.HasAnimations)
                {
                    meta.CanAnimate = true;
                }
            }
            catch { }
        }, token).ConfigureAwait(false);

        return meta;
    }
}
