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
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using SkiaSharp;
using System;
using System.Threading;

namespace ImageGlass.UI.Viewer;

public partial class PhotoRenderer : ICustomDrawOperation
{

    #region IDisposable Disposing

    protected InterlockedBool _isDisposed = new(false);


    /// <summary>
    /// Gets a value indicating whether the object has been disposed.
    /// </summary>
    public bool IsDisposed => _isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed) return;

        if (disposing)
        {
            // Free any other managed objects here.
            OnDisposing();
        }

        // Free any unmanaged objects here.
        _isDisposed.SetTrue();
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~PhotoRenderer()
    {
        Dispose(false);
    }

    #endregion



    private readonly Rect _bounds;
    private readonly Action<SKImage?> _onDrawFirstTime;
    private readonly Lock _lock;
    private bool _isFirstDraw;

    private readonly SKImageRef? _imgSource;
    private readonly SKImageRef? _imgRender;
    private readonly SKRect _srcRect;
    private readonly SKRect _destRect;
    private readonly SKSamplingOptions _samplingOptions;
    private readonly MipmapTileCache? _tileCache;
    private readonly double _zoomFactor;
    private readonly ViewerControl _viewer;


    #region Public Properties

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Rect Bounds => _bounds;


    #endregion // Public Properties


    public PhotoRenderer(ViewerControl viewer, Action<SKImage?> processFirstDrawFn)
    {
        _lock = viewer._lock;

        lock (_lock)
        {
            _bounds = viewer.Bounds;
            _onDrawFirstTime = processFirstDrawFn;

            _imgSource = viewer._imgSource;
            _imgRender = viewer._imgRender;

            // keep images alive until this renderer is disposed
            _imgSource?.KeepAlive();
            _imgRender?.KeepAlive();

            _srcRect = viewer.SrcRect.ToSKRect();
            _destRect = viewer.DestRect.ToSKRect();
            _samplingOptions = SkiaCodec.ToSamplingOptions(viewer.CurrentInterpolation);
            _tileCache = viewer._mipmapCache;
            _zoomFactor = viewer.ZoomFactor;
            _viewer = viewer;

            // Atomically claim first-draw ownership so that concurrent
            // InvalidateVisual() calls cannot trigger duplicate
            // OnDrawnImageFirstTime processing.
            _isFirstDraw = viewer._isFirstDraw;
            if (_isFirstDraw)
            {
                viewer._isFirstDraw.SetFalse();
            }
        }
    }


    #region Interface Methods


    protected virtual void OnDisposing()
    {
        // the frame was dropped before it rendered, so the next paint must do the first draw
        ReturnFirstDrawClaim();

        _imgSource?.RequestDispose();
        _imgRender?.RequestDispose();
    }


    /// <summary>
    /// Gives an unconsumed first-draw claim back, else it dies here and nothing caches the image.
    /// </summary>
    private void ReturnFirstDrawClaim()
    {
        lock (_lock)
        {
            if (!_isFirstDraw) return;
            _isFirstDraw = false;

            // a newer source owns the claim now
            if (ReferenceEquals(_viewer._imgSource, _imgSource))
            {
                _viewer._isFirstDraw.SetTrue();
            }
        }
    }


    public bool Equals(ICustomDrawOperation? other) => false;
    public bool HitTest(Point p) => true;



    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Render(ImmediateDrawingContext c)
    {
        var leaseFeature = c.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature is null) return;

        using var lease = leaseFeature.Lease();
        if (lease is null) return;


        lock (_lock)
        {
            SKImageRef.ImageLease? imageLease = null;
            SKImageRef.ImageLease? srcLease = null;

            try
            {
                SKImage? imageRender;

                // Vector (SVG) rendering: read the picture live from the
                // viewer so we always use the latest picture reference
                // produced by SvgAnimator (avoids using a stale/disposed snapshot).
                var svgPicture = _viewer._svgPicture;
                if (svgPicture is not null && !svgPicture.IsDisposed())
                {
                    RenderVector(lease.SkCanvas, svgPicture);

                    if (_isFirstDraw)
                    {
                        // clear old cache
                        lease.GrContext?.PurgeResources();

                        _isFirstDraw = false;

                        // no raster image to process for vector; pass null
                        Dispatcher.UIThread.Post(() => _onDrawFirstTime(null), DispatcherPriority.Send);
                    }
                }
                else if (_isFirstDraw)
                {
                    srcLease = _imgSource?.Acquire();
                    var srcImage = srcLease?.Image;

                    // source went away since the snapshot: let a later paint do the first draw
                    if (srcImage.IsDisposed())
                    {
                        ReturnFirstDrawClaim();
                        return;
                    }


                    // set the image to draw
                    imageRender = srcImage;


                    // draw the full image for first frame
                    var canvas = lease.SkCanvas;
                    canvas.Save();
                    canvas.DrawImage(imageRender, _srcRect, _destRect, _samplingOptions);
                    canvas.Restore();


                    // clear old cache
                    lease.GrContext?.PurgeResources();

                    // process after first time drawing
                    _isFirstDraw = false;
                    Dispatcher.UIThread.Post(() => _onDrawFirstTime(imageRender), DispatcherPriority.Send);
                }
                else if (_tileCache?.AcquireProxy() is { } proxyLease)
                {
                    using (proxyLease)
                    {
                        RenderTiled(lease.SkCanvas, proxyLease.Image);
                    }
                }
                else
                {
                    // direct rendering for small / animated images, and until the proxy is ready
                    imageLease = _imgRender?.Acquire() ?? _imgSource?.Acquire();
                    imageRender = imageLease?.Image;

                    if (imageRender is null || imageRender.IsDisposed()) return;

                    var canvas = lease.SkCanvas;
                    canvas.Save();
                    canvas.DrawImage(imageRender, _srcRect, _destRect, _samplingOptions);
                    canvas.Restore();
                }
            }
            finally
            {
                imageLease?.Dispose();
                srcLease?.Dispose();
            }
        }
    }


    #endregion // Interface Methods



    #region Private Methods


    /// <summary>
    /// Renders the proxy image followed by available detail tiles.
    /// </summary>
    private void RenderTiled(SKCanvas canvas, SKImage proxy)
    {
        var tileCache = _tileCache!;
        var mipLevel = MipmapTileCache.GetMipLevel(_zoomFactor);
        var sourceTileSize = MipmapTileCache.GetSourceTileSize(mipLevel);

        // calculate visible tile range from SrcRect
        var tileStartX = Math.Max(0, (int)(_srcRect.Left / sourceTileSize));
        var tileStartY = Math.Max(0, (int)(_srcRect.Top / sourceTileSize));
        var tileEndX = (int)Math.Ceiling(_srcRect.Right / sourceTileSize);
        var tileEndY = (int)Math.Ceiling(_srcRect.Bottom / sourceTileSize);

        // scale factors from source to destination coordinates
        var scaleX = _destRect.Width / _srcRect.Width;
        var scaleY = _destRect.Height / _srcRect.Height;

        // image coordinate scale: source pixels -> tile image pixels
        var imageScale = (float)MipmapTileCache.TILE_SIZE / sourceTileSize;

        canvas.Save();

        var proxyScaleX = (float)proxy.Width / tileCache.SourceWidth;
        var proxyScaleY = (float)proxy.Height / tileCache.SourceHeight;
        var proxySrc = new SKRect(
            _srcRect.Left * proxyScaleX,
            _srcRect.Top * proxyScaleY,
            _srcRect.Right * proxyScaleX,
            _srcRect.Bottom * proxyScaleY);
        canvas.DrawImage(proxy, proxySrc, _destRect, _samplingOptions);

        for (var ty = tileStartY; ty < tileEndY; ty++)
        {
            for (var tx = tileStartX; tx < tileEndX; tx++)
            {
                // tile bounds in original image coordinates
                float tileSrcLeft = tx * sourceTileSize;
                float tileSrcTop = ty * sourceTileSize;
                float tileSrcW = Math.Min(sourceTileSize, tileCache.SourceWidth - tileSrcLeft);
                float tileSrcH = Math.Min(sourceTileSize, tileCache.SourceHeight - tileSrcTop);

                // clip to visible source rect
                var clippedLeft = Math.Max(tileSrcLeft, _srcRect.Left);
                var clippedTop = Math.Max(tileSrcTop, _srcRect.Top);
                var clippedRight = Math.Min(tileSrcLeft + tileSrcW, _srcRect.Right);
                var clippedBottom = Math.Min(tileSrcTop + tileSrcH, _srcRect.Bottom);

                if (clippedLeft >= clippedRight || clippedTop >= clippedBottom) continue;

                using var tileLease = tileCache.GetOrQueueTile(tx, ty, mipLevel);
                var imgTile = tileLease?.Image;
                if (imgTile.IsDisposed()) continue;

                // map clipped region to tile bitmap coordinates
                var tileBitmapSrc = new SKRect(
                    (clippedLeft - tileSrcLeft) * imageScale,
                    (clippedTop - tileSrcTop) * imageScale,
                    Math.Min((clippedRight - tileSrcLeft) * imageScale, imgTile.Width),
                    Math.Min((clippedBottom - tileSrcTop) * imageScale, imgTile.Height));

                // map clipped region to destination screen coordinates
                var tileDest = new SKRect(
                    _destRect.Left + (clippedLeft - _srcRect.Left) * scaleX,
                    _destRect.Top + (clippedTop - _srcRect.Top) * scaleY,
                    _destRect.Left + (clippedRight - _srcRect.Left) * scaleX,
                    _destRect.Top + (clippedBottom - _srcRect.Top) * scaleY);

                canvas.DrawImage(imgTile, tileBitmapSrc, tileDest, _samplingOptions);
            }
        }

        canvas.Restore();
    }


    /// <summary>
    /// Renders the SVG picture scaled to the destination rect.
    /// </summary>
    private void RenderVector(SKCanvas canvas, SKPicture picture)
    {
        // safety: the picture may have been disposed between the null-check
        // and arriving here if the animation advanced on another thread.
        if (picture.IsDisposed()) return;

        // compute transform: map SVG CullRect to destRect, accounting for srcRect (pan/zoom)
        var transform = ViewerControl.ComputeVectorTransform(_srcRect, _destRect);

        canvas.Save();
        canvas.ClipRect(_destRect);
        canvas.SetMatrix(canvas.TotalMatrix.PreConcat(transform));
        canvas.DrawPicture(picture);
        canvas.Restore();
    }


    #endregion // Private Methods


}
