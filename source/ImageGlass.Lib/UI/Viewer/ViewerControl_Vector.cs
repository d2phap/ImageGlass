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
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using SkiaSharp;
using Svg.Skia;

namespace ImageGlass.UI.Viewer;


public partial class ViewerControl
{
    /// <summary>
    /// The current SVG picture for vector rendering.
    /// Owned by <see cref="_svgDocument"/> - do NOT dispose separately.
    /// </summary>
    internal SKPicture? _svgPicture;

    /// <summary>
    /// The SVG document that owns <see cref="_svgPicture"/>.
    /// </summary>
    internal SKSvg? _svgDocument;


    /// <summary>
    /// Disposes vector-specific resources.
    /// Must be called inside <see cref="_lock"/>.
    /// </summary>
    private void DisposeVectorResources()
    {
        _svgPicture = null;
        _svgDocument?.Dispose();
        _svgDocument = null;
    }


    /// <summary>
    /// Handles loading a vector photo from the decoded SVG vector source.
    /// Must be called inside <see cref="_lock"/>.
    /// </summary>
    /// <returns><c>true</c> if the vector photo was handled successfully.</returns>
    private bool HandleVectorPhotoLoaded(SkiaVectorSource vectorSource)
    {
        if (vectorSource.VectorPicture is null) return false;

        // dispose old vector resources before taking new ones
        DisposeVectorResources();

        // take ownership of the SVG document and picture
        _svgDocument = vectorSource.SvgDocument;
        _svgPicture = vectorSource.VectorPicture;

        // prevent Photo.UnloadBitmap() from disposing our SVG
        vectorSource.SvgDocument = null;

        SourceKind = PhotoSource.VectorRenderer;

        // use intrinsic size for zoom/pan calculations
        BitmapSize = vectorSource.IntrinsicSize;

        // set pre-rasterized fallback as _imgSource (for pixel operations: copy, export)
        SKImageRef.Set(ref _imgSource, vectorSource.RasterizedFallback);
        vectorSource.RasterizedFallback = null; // transfer ownership

        // mark for first draw
        _isFirstDraw.SetTrue();

        // set up SMIL animation if the SVG has animations
        if (_svgDocument!.HasAnimations)
        {
            _animator?.FrameChanged -= Animator_FrameChanged;
            _animator?.Dispose();

            _animator = new SvgAnimator(
                _svgDocument,
                _lock,
                picture => _svgPicture = picture,
                InvalidateVectorFrame);
            _animator.FrameChanged += Animator_FrameChanged;
            StartAnimator();
        }

        return true;
    }


    /// <summary>
    /// Redraws a new SMIL frame here and in the mirrors.
    /// </summary>
    private void InvalidateVectorFrame()
    {
        InvalidateVisual();
        OnRenderStateChanged();
    }


    /// <summary>
    /// Checks if the current source is a vector renderer.
    /// </summary>
    private bool IsVectorSource() => SourceKind == PhotoSource.VectorRenderer;


    /// <summary>
    /// Computes the transform matrix that maps SVG coordinates to screen coordinates.
    /// </summary>
    internal static SKMatrix ComputeVectorTransform(SKRect srcRect, SKRect destRect)
    {
        var scaleX = destRect.Width / srcRect.Width;
        var scaleY = destRect.Height / srcRect.Height;
        var tx = destRect.Left - srcRect.Left * scaleX;
        var ty = destRect.Top - srcRect.Top * scaleY;

        return SKMatrix.CreateScaleTranslation(scaleX, scaleY, tx, ty);
    }
}
