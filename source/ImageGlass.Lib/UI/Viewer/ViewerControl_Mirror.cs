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
using Avalonia.Threading;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Types;
using Svg.Skia;
using System;

namespace ImageGlass.UI.Viewer;

public partial class ViewerControl
{
    // the source image the mirror's own tile cache was built from; the cache holds it alive
    private SKImageRef? _mirrorTileSource;

    // the tile cache of the source, drawn from while both tile the same image; read by PhotoRenderer
    internal MipmapTileCache? _mirrorSharedTileCache;

    // an image the source did not tile yet, which the mirror tiles itself if it still does not on the next draw
    private SKImageRef? _pendingTileSource;

    // the vector document the mirror's viewport was computed for; read by PhotoRenderer
    internal SKSvg? _mirrorSvgDocument;


    /// <summary>
    /// Occurs when what the viewer draws, or where it draws it, may have changed.
    /// </summary>
    internal event TEventHandler<ViewerControl, EventArgs>? RenderStateChanged;



    #region Public Properties

    /// <summary>
    /// Gets, sets the viewer whose photo this viewer shows; while set, this viewer is a mirror with its own viewport.
    /// </summary>
    public ViewerControl? MirrorSource
    {
        get => GetValue(MirrorSourceProperty);
        set => SetValue(MirrorSourceProperty, value);
    }
    public static readonly StyledProperty<ViewerControl?> MirrorSourceProperty =
        AvaloniaProperty.Register<ViewerControl, ViewerControl?>(nameof(MirrorSource));


    /// <summary>
    /// Gets, sets whether a mirror follows the zoom, pan and transforms of <see cref="MirrorSource"/>, else it fits the photo by its own <see cref="ZoomMode"/>.
    /// </summary>
    public bool EnableMirrorSync
    {
        get => GetValue(EnableMirrorSyncProperty);
        set => SetValue(EnableMirrorSyncProperty, value);
    }
    public static readonly StyledProperty<bool> EnableMirrorSyncProperty =
        AvaloniaProperty.Register<ViewerControl, bool>(nameof(EnableMirrorSync), true);

    #endregion // Public Properties



    #region Source Side

    /// <summary>
    /// Raises <see cref="RenderStateChanged"/>.
    /// </summary>
    private void OnRenderStateChanged()
    {
        RenderStateChanged?.Invoke(this, EventArgs.Empty);
    }


    /// <summary>
    /// Gets the image point in the middle of the viewport, in <see cref="BitmapSize"/> coordinates.
    /// </summary>
    internal Point GetViewportCenter()
    {
        var imageCenter = new Point(BitmapSize.Width / 2, BitmapSize.Height / 2);
        var zoomFactor = _zooming.Factor / Dpi;
        if (zoomFactor <= 0 || DestRect.IsEmpty) return imageCenter;

        var viewportCenter = DrawingArea.Center;
        var x = SrcRect.X + (viewportCenter.X - DestRect.X) / zoomFactor;
        var y = SrcRect.Y + (viewportCenter.Y - DestRect.Y) / zoomFactor;

        return new Point(
            Math.Clamp(x, 0, BitmapSize.Width),
            Math.Clamp(y, 0, BitmapSize.Height));
    }

    #endregion // Source Side



    #region Mirror Side

    /// <summary>
    /// Swaps the source the mirror listens to.
    /// </summary>
    private void OnMirrorSourceChanged(ViewerControl? oldSource, ViewerControl? newSource)
    {
        if (IsLoaded)
        {
            DetachMirrorSource(oldSource);
            AttachMirrorSource(newSource);
        }

        InvalidateVisual();
    }


    /// <summary>
    /// Starts redrawing whenever the source changes.
    /// </summary>
    private void AttachMirrorSource(ViewerControl? source)
    {
        if (source is null) return;

        source.RenderStateChanged -= MirrorSource_RenderStateChanged;
        source.RenderStateChanged += MirrorSource_RenderStateChanged;
    }


    /// <summary>
    /// Stops listening to the source and drops what was built from it.
    /// </summary>
    private void DetachMirrorSource(ViewerControl? source)
    {
        source?.RenderStateChanged -= MirrorSource_RenderStateChanged;

        SetSharedTileCache(null);
        DisposeOwnTileCache();
        _pendingTileSource = null;
        _mirrorSvgDocument = null;
    }


    private void MirrorSource_RenderStateChanged(ViewerControl sender, EventArgs e)
    {
        // the viewport is taken from the source on render, so a redraw is the whole sync
        if (Dispatcher.UIThread.CheckAccess())
        {
            InvalidateVisual();
        }
        else
        {
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
        }
    }


    /// <summary>
    /// Takes the viewport from <see cref="MirrorSource"/>; runs right before each draw, so it always matches what is drawn.
    /// </summary>
    private void SyncFromMirrorSource()
    {
        var source = MirrorSource;
        if (source is null) return;

        DrawingArea = Bounds.Deflate(Padding);
        var showTransforms = EnableMirrorSync;


        // 1. snapshot what the source shows; the image is only read while the source cannot free it
        SKSvg? svgDocument;
        Size bitmapSize;
        lock (source._lock)
        {
            svgDocument = source.IsVectorSource() ? source._svgDocument : null;
            var drawnImage = showTransforms
                ? source._imgRender ?? source._imgSource
                : source._imgSource;
            var image = drawnImage?.Image;
            var hasImage = image is not null && !image.IsDisposed();

            if (svgDocument is not null)
            {
                bitmapSize = source.BitmapSize;
            }
            else if (hasImage)
            {
                bitmapSize = new Size(image!.Width, image.Height);
            }
            else
            {
                bitmapSize = new Size();
            }

            // vector and animated photos are never tiled
            var canTile = svgDocument is null && source._animator is null;
            UpdateMirrorTileCache(source, canTile ? drawnImage : null);
        }

        _mirrorSvgDocument = svgDocument;
        BitmapSize = bitmapSize;

        if (bitmapSize.IsEmpty || DrawingArea.IsEmpty)
        {
            CalculateDrawingRegion();
            return;
        }


        // 2. a preview is drawn over the footprint of the full image, same as in the source
        var meta = source.Photo?.Metadata;
        var isPreviewing = source._isPreviewing && meta is not null && meta.Width > 0 && meta.Height > 0;
        var fullSize = isPreviewing ? new Size(meta!.Width, meta.Height) : bitmapSize;
        var previewScale = isPreviewing ? GetPreviewToSourceScale(bitmapSize, meta!) : 1d;


        // 3. not following the source: fit the photo by the mirror's own zoom mode
        var canFollowSource = showTransforms && !source.DrawingArea.IsEmpty && !source.BitmapSize.IsEmpty;
        if (!canFollowSource)
        {
            var ownZoom = CalculateZoomFactor(ZoomMode, fullSize.Width, fullSize.Height);
            var imageCenter = new Point(bitmapSize.Width / 2, bitmapSize.Height / 2);

            SetMirrorViewport(ownZoom * previewScale, imageCenter, false);
            return;
        }


        // 4. following the source: an auto mode zooms on the mirror's own size, a manual zoom keeps its ratio to the fit zoom
        var sourceMode = source.ZoomMode;
        var isSourceZoomFixed = source.IsManualZoom || sourceMode == ZoomMode.LockZoom;
        double fullZoom;

        if (isSourceZoomFixed)
        {
            var sourceFullZoom = source.ZoomFactor / previewScale;
            var sourceFitZoom = source.CalculateZoomFactor(ZoomMode.ScaleToFit, fullSize.Width, fullSize.Height);
            var mirrorFitZoom = CalculateZoomFactor(ZoomMode.ScaleToFit, fullSize.Width, fullSize.Height);

            fullZoom = sourceFitZoom > 0
                ? sourceFullZoom * mirrorFitZoom / sourceFitZoom
                : mirrorFitZoom;
        }
        else
        {
            fullZoom = CalculateZoomFactor(sourceMode, fullSize.Width, fullSize.Height);
        }

        var sourceCenter = source.GetViewportCenter();
        SetMirrorViewport(fullZoom * previewScale, sourceCenter, isSourceZoomFixed);
    }


    /// <summary>
    /// Picks the tiles the mirror draws: those of the source when it tiles the same image, else its own; the caller holds the source lock.
    /// </summary>
    private void UpdateMirrorTileCache(ViewerControl source, SKImageRef? tileSource)
    {
        // 1. nothing to tile: no photo, or one too small to need tiles
        var needsTiles = MipmapTileCache.NeedsTiles(tileSource?.Image);
        if (!needsTiles)
        {
            SetSharedTileCache(null);
            DisposeOwnTileCache();
            _pendingTileSource = null;
            return;
        }


        // 2. one proxy, one set of tiles and of their GPU textures for both viewers
        var sourceCache = source._mipmapCache;
        var isSourceTilingIt = ReferenceEquals(sourceCache?.SourceRef, tileSource);
        if (isSourceTilingIt)
        {
            SetSharedTileCache(sourceCache);
            DisposeOwnTileCache();
            _pendingTileSource = null;
            return;
        }

        SetSharedTileCache(null);

        var hasOwnTiles = ReferenceEquals(_mirrorTileSource, tileSource);
        if (hasOwnTiles) return;


        // 3. the source tiles a new image right after its first draw, so give it a moment rather than tile twice
        var isFirstSeen = !ReferenceEquals(_pendingTileSource, tileSource);
        if (isFirstSeen)
        {
            DisposeOwnTileCache();
            _pendingTileSource = tileSource;
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
            return;
        }


        // 4. its own tiles, e.g. of the decoded image while the source shows a rotated one, or is minimized
        _pendingTileSource = null;
        _mirrorTileSource = tileSource;
        _mipmapCache = MipmapTileCache.Create(tileSource, InvalidateVisual);
    }


    /// <summary>
    /// Starts or stops drawing from the tile cache of the source.
    /// </summary>
    private void SetSharedTileCache(MipmapTileCache? cache)
    {
        var isSameCache = ReferenceEquals(_mirrorSharedTileCache, cache);
        if (isSameCache) return;

        // the source disposes its cache by itself; a disposed one just ignores this
        _mirrorSharedTileCache?.RemoveViewer();
        cache?.AddViewer();
        _mirrorSharedTileCache = cache;
    }


    /// <summary>
    /// Disposes the tile cache the mirror built for itself.
    /// </summary>
    private void DisposeOwnTileCache()
    {
        _mipmapCache?.Dispose();
        _mipmapCache = null;
        _mirrorTileSource = null;
    }


    /// <summary>
    /// Zooms to <paramref name="factor"/> with <paramref name="imageCenter"/> in the middle of the viewport.
    /// </summary>
    private void SetMirrorViewport(double factor, Point imageCenter, bool isManualZoom)
    {
        var isValidFactor = double.IsFinite(factor) && factor > 0;
        if (isValidFactor)
        {
            _zooming.Factor = Math.Clamp(factor, MinZoom, MaxZoom);
        }

        // no anchor: the region is placed by the pan state below
        _zooming.OldFactor = _zooming.Factor;
        _zooming.ZoomedPoint = new();
        _zooming.IsManual = isManualZoom;

        // CalculateDrawingRegion centers an axis that fits, and keeps an overflowing one inside the photo
        var zoomFactor = _zooming.Factor / Dpi;
        var viewport = DrawingArea.Size / zoomFactor;
        _logicalSrcPoint = new Point(
            imageCenter.X - viewport.Width / 2,
            imageCenter.Y - viewport.Height / 2);

        CalculateDrawingRegion();
    }

    #endregion // Mirror Side

}
