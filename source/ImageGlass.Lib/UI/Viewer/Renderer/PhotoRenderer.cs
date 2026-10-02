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
using ImageGlass.UI.Viewer.Transitions;
using SkiaSharp;
using Svg.Skia;
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
    private readonly Action<SKImage?>? _onDrawFirstTime;
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
    private readonly float _dpi;
    private readonly SKRect _drawingArea;

    // a mirror draws the photo of _viewer through the viewport of another viewer
    private readonly bool _isMirror;
    private readonly SKSvg? _mirrorSvgDocument;


    #region Public Properties

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Rect Bounds => _bounds;


    #endregion // Public Properties


    /// <param name="processFirstDrawFn">Called after the first draw of a new source; <c>null</c> never claims the first draw.</param>
    public PhotoRenderer(ViewerControl viewer, Action<SKImage?>? processFirstDrawFn)
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
            _dpi = (float)viewer.Dpi;
            _drawingArea = viewer.DrawingArea.ToSKRect();

            // claim first-draw ownership atomically so concurrent paints cannot process it twice
            _isFirstDraw = processFirstDrawFn is not null && viewer._isFirstDraw;
            if (_isFirstDraw)
            {
                viewer._isFirstDraw.SetFalse();
            }
        }
    }


    /// <summary>
    /// Draws the photo of <paramref name="source"/>, transformed if <paramref name="showTransforms"/>, through the viewport of <paramref name="mirror"/>.
    /// </summary>
    internal PhotoRenderer(ViewerControl mirror, ViewerControl source, bool showTransforms)
    {
        // drawn under the source lock: the source frees its images under it
        _lock = source._lock;
        _viewer = source;
        _isMirror = true;
        _onDrawFirstTime = null;

        // UI-thread state of the mirror, computed for what the source shows right now
        _bounds = mirror.Bounds;
        _srcRect = mirror.SrcRect.ToSKRect();
        _destRect = mirror.DestRect.ToSKRect();
        _samplingOptions = SkiaCodec.ToSamplingOptions(mirror.CurrentInterpolation);
        _tileCache = mirror._mirrorSharedTileCache ?? mirror._mipmapCache;
        _zoomFactor = mirror.ZoomFactor;
        _dpi = (float)mirror.Dpi;
        _drawingArea = mirror.DrawingArea.ToSKRect();
        _mirrorSvgDocument = mirror._mirrorSvgDocument;

        lock (_lock)
        {
            // a vector photo draws its live picture, never the rasterized fallback
            if (_mirrorSvgDocument is not null) return;

            _imgSource = source._imgSource;
            _imgRender = showTransforms ? source._imgRender : null;

            // keep images alive until this renderer is disposed
            _imgSource?.KeepAlive();
            _imgRender?.KeepAlive();
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
            // read live under the same lock; a mirror skips it, as the old frame is in the source's viewport
            var transition = _isMirror ? null : _viewer._transition;
            if (transition is null || transition.IsDisposed)
            {
                DrawContent(lease.SkCanvas, lease.GrContext);
            }
            else
            {
                RenderTransition(lease.SkCanvas, lease.GrContext, transition);
            }
        }
    }


    /// <summary>
    /// Records the photo as drawn now into a picture in viewer coordinates; the checkerboard stays out of it.
    /// </summary>
    internal SKPicture RecordFrame()
    {
        lock (_lock)
        {
            using var recorder = new SKPictureRecorder();
            var canvas = recorder.BeginRecording(_drawingArea);

            DrawContent(canvas, null);

            return recorder.EndRecording();
        }
    }


    #endregion // Interface Methods



    #region Private Methods


    /// <summary>
    /// Draws the photo in viewer coordinates; the caller holds <see cref="_lock"/>.
    /// </summary>
    private void DrawContent(SKCanvas canvas, GRContext? grContext)
    {
        SKImageRef.ImageLease? imageLease = null;
        SKImageRef.ImageLease? srcLease = null;

        try
        {
            SKImage? imageRender;

            // read the SVG picture live: SvgAnimator replaces it between frames
            var svgPicture = GetVectorPicture();
            if (svgPicture is not null && !svgPicture.IsDisposed())
            {
                RenderVector(canvas, svgPicture);

                if (_isFirstDraw)
                {
                    // clear old cache
                    grContext?.PurgeResources();

                    _isFirstDraw = false;

                    // no raster image to process for vector; pass null
                    Dispatcher.UIThread.Post(() => _onDrawFirstTime!(null), DispatcherPriority.Send);
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
                canvas.Save();
                canvas.DrawImage(imageRender, _srcRect, _destRect, _samplingOptions);
                canvas.Restore();


                // clear old cache
                grContext?.PurgeResources();

                // process after first time drawing
                _isFirstDraw = false;
                Dispatcher.UIThread.Post(() => _onDrawFirstTime!(imageRender), DispatcherPriority.Send);
            }
            else if (_tileCache?.AcquireProxy() is { } proxyLease)
            {
                using (proxyLease)
                {
                    RenderTiled(canvas, proxyLease.Image);
                }
            }
            else
            {
                // direct rendering for small / animated images, and until the proxy is ready
                imageLease = _imgRender?.Acquire() ?? _imgSource?.Acquire();
                imageRender = imageLease?.Image;

                if (imageRender is null || imageRender.IsDisposed()) return;

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



    /// <summary>
    /// Gets the SVG picture to draw; the caller holds <see cref="_lock"/>.
    /// </summary>
    private SKPicture? GetVectorPicture()
    {
        if (!_isMirror) return _viewer._svgPicture;
        if (_mirrorSvgDocument is null) return null;

        // a mirror viewport is computed for one document, so a newer one waits for the next draw
        var isSameDocument = ReferenceEquals(_viewer._svgDocument, _mirrorSvgDocument);
        return isSameDocument ? _viewer._svgPicture : null;
    }


    /// <summary>
    /// Draws the transition: the old frame until the new photo can be drawn, then the effect blending both.
    /// </summary>
    private void RenderTransition(SKCanvas canvas, GRContext? grContext, ViewerTransition transition)
    {
        if (transition.StartTime is null)
        {
            canvas.DrawPicture(transition.FromFrame);
            return;
        }

        var area = _drawingArea;
        var width = (int)Math.Ceiling(area.Width * _dpi);
        var height = (int)Math.Ceiling(area.Height * _dpi);
        var effect = transition.Request.Effect.GetEffect();

        var info = new SKImageInfo(Math.Max(1, width), Math.Max(1, height), SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        using var fromSurface = effect is null ? null : CreateSurface(grContext, info);
        using var toSurface = fromSurface is null ? null : CreateSurface(grContext, info);

        // no effect to play: draw the new photo as usual
        if (effect is null || fromSurface is null || toSurface is null)
        {
            DrawContent(canvas, grContext);
            return;
        }


        // 1. render both sides at device resolution, so the effect samples them 1:1
        PrepareTransitionCanvas(fromSurface.Canvas, area).DrawPicture(transition.FromFrame);

        var toCanvas = PrepareTransitionCanvas(toSurface.Canvas, area);
        DrawContent(toCanvas, grContext);

        using var fromImage = fromSurface.Snapshot();
        using var toImage = toSurface.Snapshot();


        // 2. feed both sides to the effect; outside its side, an image reads as transparent
        var toLogical = SKMatrix.CreateScale(1 / _dpi, 1 / _dpi);
        var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
        using var fromShader = fromImage.ToShader(SKShaderTileMode.Decal, SKShaderTileMode.Decal, sampling, toLogical);
        using var toShader = toImage.ToShader(SKShaderTileMode.Decal, SKShaderTileMode.Decal, sampling, toLogical);

        using var uniforms = new SKRuntimeEffectUniforms(effect);
        if (uniforms.Contains("progress")) uniforms["progress"] = transition.Progress;
        if (uniforms.Contains("resolution")) uniforms["resolution"] = new[] { area.Width, area.Height };
        if (uniforms.Contains("navigationDirection")) uniforms["navigationDirection"] = (float)transition.Request.Direction;
        if (uniforms.Contains("ratio")) uniforms["ratio"] = area.Width / area.Height;

        using var children = new SKRuntimeEffectChildren(effect);
        if (children.Contains("fromImage")) children["fromImage"] = fromShader;
        if (children.Contains("toImage")) children["toImage"] = toShader;


        // 3. draw the effect over the drawing area, which the shader sees as (0, 0)-resolution
        using var shader = effect.ToShader(uniforms, children);
        using var paint = new SKPaint { Shader = shader };

        canvas.Save();
        canvas.Translate(area.Left, area.Top);
        canvas.DrawRect(0, 0, area.Width, area.Height, paint);
        canvas.Restore();
    }


    /// <summary>
    /// Creates an offscreen surface on the GPU when there is one, else in memory.
    /// </summary>
    private static SKSurface? CreateSurface(GRContext? grContext, SKImageInfo info)
    {
        return grContext is null
            ? SKSurface.Create(info)
            : SKSurface.Create(grContext, false, info);
    }


    /// <summary>
    /// Clears a transition side and maps viewer coordinates onto its device pixels.
    /// </summary>
    private SKCanvas PrepareTransitionCanvas(SKCanvas canvas, SKRect area)
    {
        canvas.Clear(SKColors.Transparent);
        canvas.Scale(_dpi);
        canvas.Translate(-area.Left, -area.Top);

        return canvas;
    }


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
        // the animation may have disposed the picture since the caller's null-check
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
