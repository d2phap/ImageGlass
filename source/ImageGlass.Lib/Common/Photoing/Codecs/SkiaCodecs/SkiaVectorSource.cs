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
using ImageGlass.Common.Types;
using SkiaSharp;
using Svg.Skia;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// Holds the SVG document and vector picture for scalable rendering.
/// The viewer takes ownership of <see cref="SvgDocument"/> by setting it
/// to <c>null</c> after capturing the reference.
/// </summary>
public sealed class SkiaVectorSource : PhDisposable
{
    /// <summary>
    /// Gets, sets the SVG document. Owns the <see cref="VectorPicture"/>.
    /// Set to <c>null</c> when transferring ownership to the viewer.
    /// </summary>
    public SKSvg? SvgDocument { get; set; }

    /// <summary>
    /// Gets the vector picture for scalable rendering.
    /// Owned by <see cref="SvgDocument"/> - do NOT dispose separately.
    /// </summary>
    public SKPicture? VectorPicture => SvgDocument?.Picture;

    /// <summary>
    /// Gets, sets the pre-rasterized fallback for pixel operations (copy, export).
    /// Set to <c>null</c> when transferring ownership to the viewer.
    /// </summary>
    public SKImage? RasterizedFallback { get; set; }

    /// <summary>
    /// Gets the intrinsic size of the SVG (from CullRect).
    /// </summary>
    public Size IntrinsicSize { get; }

    /// <summary>
    /// Gets whether the SVG contains SMIL animations.
    /// </summary>
    public bool HasAnimations => SvgDocument?.HasAnimations ?? false;


    public SkiaVectorSource(SKSvg svgDocument, Size intrinsicSize, SKImage? rasterizedFallback)
    {
        SvgDocument = svgDocument;
        IntrinsicSize = intrinsicSize;
        RasterizedFallback = rasterizedFallback;
    }


    protected override void OnDisposing()
    {
        RasterizedFallback?.Dispose();
        RasterizedFallback = null;

        // VectorPicture is derived from SvgDocument, no separate disposal needed
        SvgDocument?.Dispose();
        SvgDocument = null;
    }
}
