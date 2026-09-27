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
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Loggers;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using ImageGlass.UI.Viewer.Checkerboard;
using SkiaSharp;
using System;
using System.Threading;

namespace ImageGlass.UI.Viewer;


public partial class ViewerControl
{
    // FPS
    private double _fps;
    private long _lastFpsTime = Environment.TickCount64;
    private TimeSpan _lastAnimationFrameTime = TimeSpan.Zero;


    // drawing image
    internal SKImageRef? _imgSource;
    internal SKImageRef? _imgRender;
    private AnimatorImpl? _animator;
    internal MipmapTileCache? _mipmapCache;

    // retained pre-tone-map HDR frame for live in-memory re-tone-mapping (only while the HDR tool is active)
    private SKImageRef? _imgHdrSource;
    private InterlockedBool _liveHdrToneMapping = new(false);

    // coalescing state for the background HDR re-tone-map pump (latest-request-wins)
    private volatile bool _hdrDirty;
    private int _hdrActive;

    private RenderTargetBitmap? _bmpCheckerboard;
    private readonly CheckerboardInfo _checkerboard = new();

    internal readonly Lock _lock = new();



    #region Public Properties

    /// <summary>
    /// Gets, sets the debug mode.
    /// </summary>
    public bool EnableDebug
    {
        get => GetValue(EnableDebugProperty);
        set => SetValue(EnableDebugProperty, value);
    }
    public static readonly StyledProperty<bool> EnableDebugProperty =
        AvaloniaProperty.Register<ViewerControl, bool>(nameof(EnableDebug));


    /// <summary>
    /// Gets, sets value indicates whether the previewing is enabled or not.
    /// </summary>
    public bool EnableImagePreview { get; set; } = true;


    /// <summary>
    /// Gets the rectangle of the source image region being drawn.
    /// </summary>
    public Rect SrcRect { get; internal set; } = new();


    /// <summary>
    /// Gets rectangle of the viewport.
    /// </summary>
    public Rect DestRect { get; internal set; } = new();


    /// <summary>
    /// Gets or sets the mode of the checkerboard.
    /// </summary>
    public CheckerboardType CheckerboardMode
    {
        get => GetValue(CheckerboardModeProperty);
        set => SetValue(CheckerboardModeProperty, value);
    }
    public static readonly StyledProperty<CheckerboardType> CheckerboardModeProperty =
        AvaloniaProperty.Register<ViewerControl, CheckerboardType>(nameof(CheckerboardMode), CheckerboardType.None,
            coerce: (sender, value) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var control = (ViewerControl)sender;
                    control.DisposeCheckerboard();
                    control._checkerboard.Mode = value;
                    control.InvalidateVisual();
                });

                return value;
            });


    /// <summary>
    /// Gets, sets interpolation mode used when the
    /// <see cref="ZoomFactor"/> is less than or equal <c>1.0f</c>.
    /// </summary>
    public ImageInterpolation InterpolationScaleDown
    {
        get => GetValue(InterpolationScaleDownProperty);
        set => SetValue(InterpolationScaleDownProperty, value);
    }
    public static readonly StyledProperty<ImageInterpolation> InterpolationScaleDownProperty =
        AvaloniaProperty.Register<ViewerControl, ImageInterpolation>(nameof(InterpolationScaleDown), ImageInterpolation.CubicMitchell, coerce: (sender, value) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                var control = (ViewerControl)sender;
                control.InvalidateVisual();
            });

            return value;
        });


    /// <summary>
    /// Gets, sets interpolation mode used when the
    /// <see cref="ZoomFactor"/> is greater than <c>1.0f</c>.
    /// </summary>
    public ImageInterpolation InterpolationScaleUp
    {
        get => GetValue(InterpolationScaleUpProperty);
        set => SetValue(InterpolationScaleUpProperty, value);
    }
    public static readonly StyledProperty<ImageInterpolation> InterpolationScaleUpProperty =
        AvaloniaProperty.Register<ViewerControl, ImageInterpolation>(nameof(InterpolationScaleUp), ImageInterpolation.CubicMitchell, coerce: (sender, value) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                var control = (ViewerControl)sender;
                control.InvalidateVisual();
            });

            return value;
        });


    /// <summary>
    /// Gets the current <see cref="ImageInterpolation"/> mode.
    /// </summary>
    public ImageInterpolation CurrentInterpolation
    {
        get
        {
            if (ZoomFactor < 1f) return InterpolationScaleDown;
            if (ZoomFactor > 1f) return InterpolationScaleUp;

            return ImageInterpolation.Nearest;
        }
    }

    #endregion // Public Properties



    #region Override / Virtual Methods

    protected void DisposeCheckerboard()
    {
        _bmpCheckerboard?.Dispose();
        _bmpCheckerboard = null;
    }


    public override void Render(DrawingContext c)
    {
        base.Render(c);

        using (c.PushClip(DrawingArea))
        {
            OnDrawCheckerboard(c);      // draw checkerboard
            OnDrawImage(c);             // draw image
            OnDrawSelection(c);         // draw selection
        }

        OnDrawDebugInfo(c);         // draw debug info
    }


    /// <summary>
    /// Draw debug information.
    /// </summary>
    protected virtual void OnDrawDebugInfo(DrawingContext c)
    {
        if (!EnableDebug) return;

        var cacheIndexes = Core.Photos.CachedIndexSnapshot;
        var cacheIndexStr = cacheIndexes.Length > 0
            ? string.Join(", ", cacheIndexes)
            : "(none)";

        var text = $"""
            DPI = {Dpi}, FPS = {_fps}
            Control Bounds = {Bounds}
            DrawingArea = {DrawingArea}
            Image size = {BitmapSize}
            Use MipMap = {_mipmapCache != null}
            _isPreviewing = {_isPreviewing}
            _srcRect = {SrcRect}
            _destRect = {DestRect}
            _zoomFactor = {_zooming.Factor}
            _zoomedPoint = {_zooming.ZoomedPoint}

            SourceRect = {_selection.SourceRect}
            ClientSelection = {ClientSelection}

            Cache: {Core.Photos.CachedCount} photos, {Core.Photos.CachedMemoryMb} MB
            Cached indexes = [{cacheIndexStr}]
            """;

        var textLayout = new TextLayout(text, new Typeface("Consolas"), FontSize, Brushes.HotPink);
        var textOrigin = new Point(DrawingArea.X + 20, DrawingArea.Y + 20);
        for (int i = 0; i < textLayout.TextLines.Count; i++)
        {
            var line = textLayout.TextLines[i];
            var bounds = line.GetTextBounds(line.FirstTextSourceIndex, line.Length);
            if (bounds.Count == 0) continue;

            var lineRect = bounds[0].Rectangle;
            lineRect = lineRect.WithX(lineRect.X + textOrigin.X).WithY(lineRect.Y + line.Height * i + textOrigin.Y);

            c.FillRectangle(Colors.Black.A(180).ToBrush(), lineRect);
        }

        // draw text
        textLayout.Draw(c, textOrigin);

        // draw image bounds
        c.DrawRectangle(new Pen(Brushes.LightGreen, 2, DashStyle.Dash, PenLineCap.Round, PenLineJoin.Round), DestRect);

        // draw zoomed point
        c.DrawEllipse(Brushes.Red, new Pen(Brushes.White, 2), _zooming.ZoomedPoint, 5, 5);
    }


    /// <summary>
    /// Draws checkerboard.
    /// </summary>
    protected virtual void OnDrawCheckerboard(DrawingContext c)
    {
        if (CheckerboardMode == CheckerboardType.None) return;

        // region to draw
        Rect region;
        if (CheckerboardMode == CheckerboardType.Image)
        {
            region = DestRect;
        }
        else
        {
            region = DrawingArea;
        }


        // create empty render bitmap: [X, O]
        //                             [O, X]
        var cellW = (int)(_checkerboard.Size.Width * Dpi);
        var cellH = (int)(_checkerboard.Size.Height * Dpi);
        var pxSize = new PixelSize(cellW * 2, cellH * 2);
        _bmpCheckerboard ??= new RenderTargetBitmap(pxSize);

        // fill the bitmap with a tile
        using (var ctx = _bmpCheckerboard.CreateDrawingContext())
        {
            var brushX = new ImmutableSolidColorBrush(Colors.Black);
            var brushO = new ImmutableSolidColorBrush(Colors.White);

            // draw cells: [X, ]
            //             [ ,X]
            ctx.FillRectangle(brushX, new Rect(0, 0, cellW, cellH));
            ctx.FillRectangle(brushX, new Rect(cellW, cellH, cellW, cellH));

            // draw cells: [ ,O]
            //             [O, ]
            ctx.FillRectangle(brushO, new Rect(cellW, 0, cellW, cellH));
            ctx.FillRectangle(brushO, new Rect(0, cellH, cellW, cellH));
        }


        // create brush for checkerboard
        var imgBrush = new ImageBrush
        {
            Source = _bmpCheckerboard,
            Stretch = Stretch.Fill,
            TileMode = TileMode.Tile,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
            DestinationRect = new(0, 0, cellW, cellH, RelativeUnit.Absolute),
            Opacity = 0.1,
        };


        // draw the checkerboard
        using (c.PushRenderOptions(new() { BitmapInterpolationMode = BitmapInterpolationMode.None }))
        {
            c.FillRectangle(imgBrush, region);
        }
    }


    /// <summary>
    /// Draws image.
    /// </summary>
    protected virtual void OnDrawImage(DrawingContext c)
    {
        // draw image
        c.Custom(new PhotoRenderer(this, OnDrawnImageFirstTime));
    }


    /// <summary>
    /// Occurs when the source image drawn for the first time.
    /// </summary>
    protected void OnDrawnImageFirstTime(SKImage? img)
    {
        lock (_lock)
        {
            // img may be disposed after the render posted this callback; ?. only guards null
            var imgSize = img.IsDisposed() ? "0x0" : $"{img.Width}x{img.Height}";
            PhotoTrace.Mark("render:first-draw", Photo?.FilePath,
                $"{imgSize}, vector={IsVectorSource()}");

            // the photo is now on screen; release anyone waiting on the first paint
            _firstDrawTcs?.TrySetResult();

            // vector images don't need raster caching
            if (IsVectorSource())
            {
                InvalidateVisual();
                return;
            }

            // cache the proccessed image for next draw
            SKImageRef.Set(ref _imgRender, img, _imgSource);

            // apply color channel filter
            if (_loadingOptions.Channels != ColorChannels.RGBA)
            {
                _ = FilterColorChannels(_loadingOptions.Channels, false);
            }

            // build mipmap tile cache for non-animated photos
            CreateMipmapTileCache();

            // draw again
            InvalidateVisual();
        }
    }


    /// <summary>
    /// Draws animation source.
    /// </summary>
    protected virtual void OnDrawAnimationSource(double eslapseDelta)
    {
        var panDelta = eslapseDelta * 30;
        var zoomDelta = eslapseDelta * 1000;

        // Panning
        if (AnimationSource.HasFlag(AnimationSources.PanLeft))
        {
            PanLeft(panDelta, requestRerender: false);
        }
        else if (AnimationSource.HasFlag(AnimationSources.PanRight))
        {
            PanRight(panDelta, requestRerender: false);
        }

        if (AnimationSource.HasFlag(AnimationSources.PanUp))
        {
            PanUp(panDelta, requestRerender: false);
        }
        else if (AnimationSource.HasFlag(AnimationSources.PanDown))
        {
            PanDown(panDelta, requestRerender: false);
        }


        // Zooming
        if (AnimationSource.HasFlag(AnimationSources.ZoomIn))
        {
            if (_zooming.ZoomedPoint.X == 0 && _zooming.ZoomedPoint.Y == 0)
            {
                _zooming.ZoomedPoint = DestRect.Center;
            }
            _ = ZoomByDeltaToPoint(zoomDelta, _zooming.ZoomedPoint, requestRerender: false);
        }
        else if (AnimationSource.HasFlag(AnimationSources.ZoomOut))
        {
            if (_zooming.ZoomedPoint.X == 0 && _zooming.ZoomedPoint.Y == 0)
            {
                _zooming.ZoomedPoint = DestRect.Center;
            }
            _ = ZoomByDeltaToPoint(-zoomDelta, _zooming.ZoomedPoint, requestRerender: false);
        }

    }


    #endregion // Override / Virtual Methods



    #region Control Methods


    /// <summary>
    /// Calculates FPS in debug mode.
    /// </summary>
    private void CalculateFps()
    {
        if (!EnableDebug) return;

        var now = Environment.TickCount64;
        var delta = now - _lastFpsTime;

        if (delta > 0)
        {
            _fps = _fps * 0.9 + (1000.0 / delta) * 0.1;
            _fps = Math.Round(_fps, 2);
        }

        _lastFpsTime = now;
    }


    /// <summary>
    /// Creates the mipmap tile cache from the current render image.
    /// Must be called inside <see cref="_lock"/>.
    /// Skips animated photos (they update frames too frequently for tiling).
    /// </summary>
    private void CreateMipmapTileCache()
    {
        // dispose previous mipmap tile cache
        _mipmapCache?.Dispose();
        _mipmapCache = null;

        // skip for animated photos
        if (_animator is not null) return;

        // use the processed (color-managed) image if available, otherwise the source
        _mipmapCache = MipmapTileCache.Create(_imgRender ?? _imgSource, InvalidateVisual);

        PhotoTrace.Mark("render:mipmap-cache", Photo?.FilePath,
            _mipmapCache is null ? "not created (image too small)" : "created");
    }


    #endregion // Control Methods



}
