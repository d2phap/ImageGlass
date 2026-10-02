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
using ImageGlass.UI.Viewer.ZoomAndPan;
using System;
using System.Linq;

namespace ImageGlass.UI.Viewer;

public partial class ViewerControl
{
    private readonly ZoomInfo _zooming = new();
    private bool _enablePanningVelocity = true;


    /// <summary>
    /// Logical source position (can be negative when over-panning).
    /// This value is NOT clipped to image bounds, preserving the over-pan state across frames.
    /// </summary>
    private Point _logicalSrcPoint = new();


    // Public Events
    #region Public Events

    /// <summary>
    /// Occurs when <see cref="ZoomFactor"/> value changes.
    /// </summary>
    public event TEventHandler<ViewerControl, ViewerZoomEventArgs>? ZoomChanged;


    /// <summary>
    /// Occurs when the image is being panned.
    /// </summary>
    public event TEventHandler<ViewerControl, ViewerPanEventArgs>? Panning;

    #endregion // Public Events



    // Public Properies
    #region Public Properies

    /// <summary>
    /// Gets, sets zoom mode.
    /// </summary>
    public ZoomMode ZoomMode
    {
        get => GetValue(ZoomModeProperty);
        set => SetValue(ZoomModeProperty, value);
    }
    public static readonly StyledProperty<ZoomMode> ZoomModeProperty =
        AvaloniaProperty.Register<ViewerControl, ZoomMode>(nameof(ZoomMode), ZoomMode.AutoZoom,
            coerce: (sender, value) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var control = (ViewerControl)sender;
                    control._zooming.Mode = value;
                    control.Refresh();
                });

                return value;
            });


    /// <summary>
    /// Gets, sets current zoom factor (<c>1.0f = 100%</c>).
    /// This is a manual zooming.
    /// 
    /// <para>
    /// Use <see cref="SetZoomFactor(double, bool)"/> for more options.
    /// </para>
    /// </summary>
    public double ZoomFactor
    {
        get => _zooming.Factor;
        set => SetZoomFactor(value, true);
    }


    /// <summary>
    /// Gets, sets the minimum zoom factor (<c>1.0f = 100%</c>).
    /// Returns the first value of <see cref="ZoomLevels"/> if it is not empty.
    /// </summary>
    public double MinZoom
    {
        get
        {
            if (ZoomLevels.Length > 0) return ZoomLevels[0];
            return _zooming.Min;
        }
        set => _zooming.Min = Math.Min(Math.Max(0.001f, value), 1000);
    }


    /// <summary>
    /// Gets, sets the maximum zoom factor (<c>1.0f = 100%</c>).
    /// Returns the last value of <see cref="ZoomLevels"/> if it is not empty.
    /// </summary>
    public double MaxZoom
    {
        get
        {
            if (ZoomLevels.Length > 0) return ZoomLevels[^1];
            return _zooming.Max;
        }
        set => _zooming.Max = Math.Min(Math.Max(0.001f, value), 1000);
    }


    /// <summary>
    /// Gets or sets an array of zoom levels (ordered by ascending).
    /// </summary>
    public double[] ZoomLevels
    {
        get => GetValue(ZoomLevelsProperty);
        set => SetValue(ZoomLevelsProperty, value);
    }
    public static readonly StyledProperty<double[]> ZoomLevelsProperty =
        AvaloniaProperty.Register<ViewerControl, double[]>(nameof(ZoomLevels), [],
            coerce: (_, value) => (value ?? [])
                .OrderBy(x => x)
                .Where(i => i > 0)
                .Distinct()
                .ToArray());


    /// <summary>
    /// Gets, sets the zoom speed. Value is from <c>-500f</c> to <c>500f</c>.
    /// </summary>
    public double ZoomSpeed
    {
        get => GetValue(ZoomSpeedProperty);
        set => SetValue(ZoomSpeedProperty, value);
    }
    public static readonly StyledProperty<double> ZoomSpeedProperty =
        AvaloniaProperty.Register<ViewerControl, double>(nameof(ZoomSpeed), 0d,
            coerce: (_, value) => Math.Clamp(value, -ZoomInfo.MAX_ZOOM_SPEED, ZoomInfo.MAX_ZOOM_SPEED));


    /// <summary>
    /// Gets a value indicating whether zooming is currently controlled manually by the user.
    /// </summary>
    public bool IsManualZoom => _zooming.IsManual;


    /// <summary>
    /// Gets, sets the speed for manual panning. Min value is <c>0</c>.
    /// </summary>
    public double PanSpeed
    {
        get => GetValue(PanSpeedProperty);
        set => SetValue(PanSpeedProperty, value);
    }
    public static readonly StyledProperty<double> PanSpeedProperty =
        AvaloniaProperty.Register<ViewerControl, double>(nameof(PanSpeed), 20d,
            coerce: (_, value) => Math.Max(value, 0)); // min 0


    /// <summary>
    /// Gets, sets if the window is on Window Fit mode.
    /// </summary>
    public bool IsWindowFitMode
    {
        get => GetValue(IsWindowFitModeProperty);
        set => SetValue(IsWindowFitModeProperty, value);
    }
    public static readonly StyledProperty<bool> IsWindowFitModeProperty =
        AvaloniaProperty.Register<ViewerControl, bool>(nameof(IsWindowFitMode));


    /// <summary>
    /// Gets, sets a value indicating whether panning is allowed
    /// when the rendered image fits within the <see cref="DrawingArea"/>.
    /// </summary>
    public bool EnableFreePan
    {
        get => GetValue(EnableFreePanProperty);
        set => SetValue(EnableFreePanProperty, value);
    }
    public static readonly StyledProperty<bool> EnableFreePanProperty =
        AvaloniaProperty.Register<ViewerControl, bool>(nameof(EnableFreePan));


    /// <summary>
    /// Gets a value indicating whether free panning is currently available.
    /// </summary>
    public bool CanUseFreePan => EnableFreePan && !IsWindowFitMode;


    /// <summary>
    /// Gets, sets the maximum panning margin in screen pixels beyond the image edge.
    /// </summary>
    public double PanMargin
    {
        get => GetValue(PanMarginProperty);
        set => SetValue(PanMarginProperty, value);
    }
    public static readonly StyledProperty<double> PanMarginProperty =
        AvaloniaProperty.Register<ViewerControl, double>(nameof(PanMargin), 30);

    #endregion // Public Properies



    // Public Methods
    #region Public Methods


    /// <summary>
    /// Calculates the drawing region for an image based on zoom level and control dimensions.
    /// Adjusts source and destination rectangles accordingly.
    /// 
    /// <para>
    /// The method determines two rectangles:
    /// <list type="bullet">
    ///   <item><see cref="SrcRect"/>: the region of the source image to sample from.</item>
    ///   <item><see cref="DestRect"/>: the region on the control where the sampled image is drawn.</item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// High-level flow:
    /// <list type="number">
    ///   <item>Convert zoom factors to DPI-aware values and compute scaled image size.</item>
    ///   <item>For each axis (X then Y), determine source offset and dest position based on
    ///         whether the scaled image fits within or overflows the viewport.</item>
    ///   <item>Clamp the source position to enforce panning margins (with FreePan ratcheting).</item>
    ///   <item>Preserve the logical (unclipped) position for the next frame.</item>
    ///   <item>Align the source position to the pixel grid while rendering 1:1.</item>
    ///   <item>Clip source rect to valid image bounds, adjusting dest rect proportionally
    ///         to show a gap at the edge when over-panned.</item>
    ///   <item>Snap the dest origin to a whole device pixel.</item>
    /// </list>
    /// </para>
    /// </summary>
    public virtual void CalculateDrawingRegion()
    {
        if (DrawingArea.IsEmpty || BitmapSize.IsEmpty)
        {
            SrcRect = new();
            DestRect = new();
            _logicalSrcPoint = new();
            UpdateScrollbars();
            OnRenderStateChanged();
            return;
        }


        // ═══════════════════════════════════════════════════════════════════════
        // 1. Prepare DPI-scaled values and shared state
        // ═══════════════════════════════════════════════════════════════════════

        var dpi = Dpi;

        // zoom factors in device pixels (divide by DPI to go from logical to physical)
        var currentZoomFactor = _zooming.Factor / dpi;
        var oldZoomFactor = _zooming.OldFactor / dpi;

        // cursor position relative to the DrawingArea origin (excluding padding)
        var zoomX = _zooming.ZoomedPoint.X - Padding.Left;
        var zoomY = _zooming.ZoomedPoint.Y - Padding.Top;

        // viewport dimensions
        var controlW = DrawingArea.Width;
        var controlH = DrawingArea.Height;

        // image dimensions scaled to screen pixels at current zoom
        var scaledImgWidth = BitmapSize.Width * currentZoomFactor;
        var scaledImgHeight = BitmapSize.Height * currentZoomFactor;

        // true when this call is a zoom-to-cursor operation (factor changed with a valid cursor point)
        var isZoomingToPoint = currentZoomFactor != oldZoomFactor && _zooming.ZoomedPoint != default;

        // initialize from the previous frame:
        //   srcX/srcY  = logical (unclipped) source position from last frame
        //   srcWidth/srcHeight, destX/destY/destWidth/destHeight = previous output rects
        var srcX = _logicalSrcPoint.X;
        var srcY = _logicalSrcPoint.Y;
        var srcWidth = SrcRect.Width;
        var srcHeight = SrcRect.Height;

        var destX = DestRect.X;
        var destY = DestRect.Y;
        var destWidth = DestRect.Width;
        var destHeight = DestRect.Height;


        // ═══════════════════════════════════════════════════════════════════════
        // 2. X-axis: determine srcX, srcWidth, destX, destWidth
        // ═══════════════════════════════════════════════════════════════════════
        if (scaledImgWidth <= controlW)
        {
            // --- Fits within viewport: show entire image width, center horizontally ---
            srcX = 0;
            srcWidth = BitmapSize.Width;
            destWidth = scaledImgWidth;

            if (CanUseFreePan)
            {
                if (isZoomingToPoint)
                {
                    // Zoom anchor: map cursor to an image coordinate, then position
                    // the image so that coordinate stays under the cursor.
                    // Works for both normal fits-within and overflow -> fits-within transitions.
                    var screenZoomX = zoomX + DrawingArea.Left;
                    var imgX = SrcRect.X + (screenZoomX - DestRect.X) / oldZoomFactor;
                    imgX = Math.Clamp(imgX, 0, BitmapSize.Width);
                    destX = screenZoomX - imgX * currentZoomFactor;
                }
                else
                {
                    // Panning or static: center image, then offset by the pan amount.
                    // The pan offset is clamped so the image can't move beyond its centered position.
                    var maxPanScreenX = (controlW - scaledImgWidth) / 2.0;
                    var panOffsetX = Math.Clamp(_logicalSrcPoint.X * currentZoomFactor, -maxPanScreenX, maxPanScreenX);
                    destX = (controlW - scaledImgWidth) / 2.0 + DrawingArea.Left - panOffsetX;
                }
            }
            else
            {
                // No free-pan: always center the image
                destX = (controlW - scaledImgWidth) / 2.0 + DrawingArea.Left;
            }
        }
        else
        {
            // --- Overflows viewport: show a viewport-width slice of the image ---
            srcWidth = controlW / currentZoomFactor;

            if (isZoomingToPoint)
            {
                // Zoom anchor: map cursor to an image coordinate, then compute the
                // source offset so that coordinate stays under the cursor.
                var screenZoomX = zoomX + DrawingArea.Left;
                var rawImgX = SrcRect.X + (screenZoomX - DestRect.X) / oldZoomFactor;
                var imgX = Math.Clamp(rawImgX, 0, BitmapSize.Width);

                if (!CanUseFreePan && rawImgX != imgX)
                {
                    // Cursor is in the PanMargin gap (outside the rendered image).
                    // Anchor to the image edge's current screen position instead of
                    // the cursor, so the margin gap is preserved during zoom.
                    var edgeScreenX = rawImgX < 0
                        ? DestRect.X
                        : DestRect.X + DestRect.Width;
                    srcX = imgX - (edgeScreenX - DrawingArea.Left) / currentZoomFactor;
                }
                else
                {
                    srcX = imgX - zoomX / currentZoomFactor;
                }
            }
            // else: panning — srcX retains _logicalSrcPoint.X (initialized above)

            destX = DrawingArea.Left;
            destWidth = controlW;
        }


        // ═══════════════════════════════════════════════════════════════════════
        // 3. Y-axis: determine srcY, srcHeight, destY, destHeight
        //    (mirrors the X-axis logic above)
        // ═══════════════════════════════════════════════════════════════════════
        if (scaledImgHeight <= controlH)
        {
            // --- Fits within viewport ---
            srcY = 0;
            srcHeight = BitmapSize.Height;
            destHeight = scaledImgHeight;

            if (CanUseFreePan)
            {
                if (isZoomingToPoint)
                {
                    var screenZoomY = zoomY + DrawingArea.Top;
                    var imgY = SrcRect.Y + (screenZoomY - DestRect.Y) / oldZoomFactor;
                    imgY = Math.Clamp(imgY, 0, BitmapSize.Height);
                    destY = screenZoomY - imgY * currentZoomFactor;
                }
                else
                {
                    var maxPanScreenY = (controlH - scaledImgHeight) / 2.0;
                    var panOffsetY = Math.Clamp(_logicalSrcPoint.Y * currentZoomFactor, -maxPanScreenY, maxPanScreenY);
                    destY = (controlH - scaledImgHeight) / 2.0 + DrawingArea.Top - panOffsetY;
                }
            }
            else
            {
                destY = (controlH - scaledImgHeight) / 2.0 + DrawingArea.Top;
            }
        }
        else
        {
            // --- Overflows viewport ---
            srcHeight = controlH / currentZoomFactor;

            if (isZoomingToPoint)
            {
                var screenZoomY = zoomY + DrawingArea.Top;
                var rawImgY = SrcRect.Y + (screenZoomY - DestRect.Y) / oldZoomFactor;
                var imgY = Math.Clamp(rawImgY, 0, BitmapSize.Height);

                if (!CanUseFreePan && rawImgY != imgY)
                {
                    var edgeScreenY = rawImgY < 0
                        ? DestRect.Y
                        : DestRect.Y + DestRect.Height;
                    srcY = imgY - (edgeScreenY - DrawingArea.Top) / currentZoomFactor;
                }
                else
                {
                    srcY = imgY - zoomY / currentZoomFactor;
                }
            }
            // else: panning — srcY retains _logicalSrcPoint.Y (initialized above)

            destY = DrawingArea.Top;
            destHeight = controlH;
        }


        // ═══════════════════════════════════════════════════════════════════════
        // 4. Clamp source position to enforce panning margins
        // ═══════════════════════════════════════════════════════════════════════
        //
        // For overflow axes, limit how far the user can pan beyond the image edge.
        // panMarginSrc is PanMargin (screen px) converted to source coordinates.
        //
        // Clamping is SKIPPED during zoom-to-cursor only when CanUseFreePan is on, so the
        // overflow <-> fits-within transition stays smooth. Without FreePan the image must
        // always stay inside the viewport, so the clamp keeps running while zooming.
        var panMargin = IsWindowFitMode ? 0 : PanMargin;
        var panMarginSrc = DpiScale(panMargin) / currentZoomFactor;
        var skipClampWhileZooming = isZoomingToPoint && CanUseFreePan;

        // An axis that just grew past the viewport has no over-pan gap to preserve, so it
        // lands flush against the edge instead of inheriting the previous centering gap.
        var wasWidthFitting = BitmapSize.Width * oldZoomFactor <= controlW;
        var wasHeightFitting = BitmapSize.Height * oldZoomFactor <= controlH;

        // --- X-axis margin clamping ---
        if (scaledImgWidth > controlW && !skipClampWhileZooming)
        {
            var baseMarginX = isZoomingToPoint && wasWidthFitting ? 0 : panMarginSrc;

            // Compute per-side effective margins.
            // When CanUseFreePan is on, use the PREVIOUS frame's edge gap (from DestRect,
            // which hasn't been overwritten yet) as a floor. This "ratchet" preserves the
            // over-pan established by zoom-to-cursor: the user can pan back but not further out.
            var effectiveLeftMarginX = baseMarginX;
            var effectiveRightMarginX = baseMarginX;
            if (CanUseFreePan)
            {
                var prevLeftGap = Math.Max(0, DestRect.X - DrawingArea.Left) / currentZoomFactor;
                var prevRightGap = Math.Max(0, DrawingArea.Left + controlW - (DestRect.X + DestRect.Width)) / currentZoomFactor;
                effectiveLeftMarginX = Math.Max(baseMarginX, prevLeftGap);
                effectiveRightMarginX = Math.Max(baseMarginX, prevRightGap);
            }

            if (srcX < -effectiveLeftMarginX)
            {
                srcX = -effectiveLeftMarginX;
            }
            else if (srcX + srcWidth > BitmapSize.Width + effectiveRightMarginX)
            {
                srcX = BitmapSize.Width - srcWidth + effectiveRightMarginX;
            }
        }

        // --- Y-axis margin clamping ---
        if (scaledImgHeight > controlH && !skipClampWhileZooming)
        {
            var baseMarginY = isZoomingToPoint && wasHeightFitting ? 0 : panMarginSrc;

            var effectiveTopMarginY = baseMarginY;
            var effectiveBottomMarginY = baseMarginY;
            if (CanUseFreePan)
            {
                var prevTopGap = Math.Max(0, DestRect.Y - DrawingArea.Top) / currentZoomFactor;
                var prevBottomGap = Math.Max(0, DrawingArea.Top + controlH - (DestRect.Y + DestRect.Height)) / currentZoomFactor;
                effectiveTopMarginY = Math.Max(baseMarginY, prevTopGap);
                effectiveBottomMarginY = Math.Max(baseMarginY, prevBottomGap);
            }

            if (srcY + srcHeight > BitmapSize.Height + effectiveBottomMarginY)
            {
                srcY = BitmapSize.Height - srcHeight + effectiveBottomMarginY;
            }

            if (srcY < -effectiveTopMarginY)
            {
                srcY = -effectiveTopMarginY;
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        // 4.1. Preserve the logical (unclipped) source position for the next frame
        // ═══════════════════════════════════════════════════════════════════════
        //
        // For overflow axes: store srcX/srcY directly (already margin-clamped above).
        // For fits-within axes:
        //   - If FreePan is off: no panning state, always 0.
        //   - If FreePan is on: back-compute the pan offset from destX/destY and clamp
        //     it so the image can't drift beyond its centered position.
        //     (Zoom-to-cursor sets destX/destY directly, so this clamp only
        //     constrains subsequent panning frames.)

        double logicalX;
        if (scaledImgWidth > controlW)
        {
            logicalX = srcX;
        }
        else if (CanUseFreePan)
        {
            var halfGapX = (controlW - scaledImgWidth) / 2.0;
            logicalX = Math.Clamp(
                (halfGapX + DrawingArea.Left - destX) / currentZoomFactor,
                -halfGapX / currentZoomFactor,
                halfGapX / currentZoomFactor);
        }
        else
        {
            logicalX = 0;
        }

        double logicalY;
        if (scaledImgHeight > controlH)
        {
            logicalY = srcY;
        }
        else if (CanUseFreePan)
        {
            var halfGapY = (controlH - scaledImgHeight) / 2.0;
            logicalY = Math.Clamp(
                (halfGapY + DrawingArea.Top - destY) / currentZoomFactor,
                -halfGapY / currentZoomFactor,
                halfGapY / currentZoomFactor);
        }
        else
        {
            logicalY = 0;
        }

        _logicalSrcPoint = new(logicalX, logicalY);


        // ═══════════════════════════════════════════════════════════════════════
        // 4.2. Keep 1:1 rendering aligned to the pixel grid
        // ═══════════════════════════════════════════════════════════════════════
        //
        // At 100% one source pixel maps to one device pixel, so a fractional source offset
        // puts the sampler on texel boundaries. Applied after step 4.1 so the pan state keeps
        // its sub-pixel precision; only the drawn position is quantized.
        if (_zooming.Factor == 1)
        {
            if (scaledImgWidth > controlW) srcX = Math.Round(srcX, MidpointRounding.AwayFromZero);
            if (scaledImgHeight > controlH) srcY = Math.Round(srcY, MidpointRounding.AwayFromZero);
        }


        // ═══════════════════════════════════════════════════════════════════════
        // 4.3. Clip source rect to valid image bounds
        // ═══════════════════════════════════════════════════════════════════════
        //
        // When the source position extends beyond [0, BitmapSize], clip it back
        // and proportionally shrink/offset the dest rect. This creates a visible
        // gap at the edge when the user has over-panned.

        // left edge: srcX < 0 -> shift dest right, narrow both rects
        if (srcX < 0)
        {
            var overPan = -srcX * currentZoomFactor;
            destX += overPan;
            destWidth -= overPan;
            srcWidth += srcX; // reduce by |srcX|
            srcX = 0;
        }

        // right edge: source extends past image width -> narrow dest from the right
        if (srcX + srcWidth > BitmapSize.Width)
        {
            var excess = srcX + srcWidth - BitmapSize.Width;
            var excessScreen = excess * currentZoomFactor;
            destWidth -= excessScreen;
            srcWidth = BitmapSize.Width - srcX;
        }

        // top edge
        if (srcY < 0)
        {
            var overPan = -srcY * currentZoomFactor;
            destY += overPan;
            destHeight -= overPan;
            srcHeight += srcY; // reduce by |srcY|
            srcY = 0;
        }

        // bottom edge
        if (srcY + srcHeight > BitmapSize.Height)
        {
            var excess = srcY + srcHeight - BitmapSize.Height;
            var excessScreen = excess * currentZoomFactor;
            destHeight -= excessScreen;
            srcHeight = BitmapSize.Height - srcY;
        }


        // ═══════════════════════════════════════════════════════════════════════
        // 5. Commit the final rectangles
        // ═══════════════════════════════════════════════════════════════════════
        //
        // A half-pixel dest origin makes the sampler land on source texel boundaries, so a 1px
        // row/column is duplicated or dropped at 100% zoom. Snapped here rather than earlier
        // because step 4.1 back-computes the pan state from the unsnapped position.
        var snappedDestX = SnapToDevicePixel(destX, dpi);
        var snappedDestY = SnapToDevicePixel(destY, dpi);

        SrcRect = new(srcX, srcY, srcWidth, srcHeight);
        DestRect = new(snappedDestX, snappedDestY, destWidth, destHeight);

        _zooming.OldFactor = _zooming.Factor;

        // 6. Sync the scrollbars with the new viewport
        UpdateScrollbars();

        // 7. Let mirrors follow the new viewport
        OnRenderStateChanged();
    }


    /// <summary>
    /// Rounds a logical coordinate so that it lands on a whole device pixel.
    /// </summary>
    private static double SnapToDevicePixel(double logicalValue, double dpi)
    {
        if (dpi <= 0) return logicalValue;

        var devicePixel = Math.Round(logicalValue * dpi, MidpointRounding.AwayFromZero);
        return devicePixel / dpi;
    }


    /// <summary>
    /// Sets the zoom factor for a view while ensuring it stays within defined limits.
    /// </summary>
    public void SetZoomFactor(double zoomValue, bool isManualZoom)
    {
        // reset viewport
        if (!isManualZoom)
        {
            SrcRect = new();
            _logicalSrcPoint = new();
            _zooming.ZoomedPoint = new();
        }

        _zooming.Factor = Math.Min(MaxZoom, Math.Max(zoomValue, MinZoom));
        _zooming.IsManual = isManualZoom;


        // update drawing regions
        CalculateDrawingRegion();
        InvalidateVisual();

        ZoomChanged?.Invoke(this, new ViewerZoomEventArgs()
        {
            ZoomFactor = _zooming.Factor,
            IsManualZoom = _zooming.IsManual,
            IsZoomModeChange = false,
            IsPreviewingImage = _isPreviewing,
            ChangeSource = ZoomChangeSource.Unknown,
        });
    }


    /// <summary>
    /// Sets the zoom mode and updates the zoom factor based on the provided parameters.
    /// This <u><c>does not</c></u> redraw the viewing image.
    /// </summary>
    public void SetZoomMode(ZoomMode? mode = null, bool isManualZoom = false, bool zoomedByResizing = false)
    {
        var effectiveMode = mode ?? _zooming.Mode;

        // preserve pan position for LockZoom to maintain viewport across photo changes
        if (effectiveMode != ZoomMode.LockZoom)
        {
            _logicalSrcPoint = default;
        }
        _zooming.ZoomedPoint = new();

        _zooming.Mode = effectiveMode;
        _zooming.Factor = CalculateZoomFactor(_zooming.Mode, BitmapSize.Width, BitmapSize.Height);
        _zooming.IsManual = isManualZoom;


        // update drawing regions
        CalculateDrawingRegion();


        ZoomChanged?.Invoke(this, new ViewerZoomEventArgs()
        {
            ZoomFactor = _zooming.Factor,
            IsManualZoom = _zooming.IsManual,
            IsZoomModeChange = mode != _zooming.Mode,
            IsPreviewingImage = _isPreviewing,
            ChangeSource = zoomedByResizing ? ZoomChangeSource.SizeChanged : ZoomChangeSource.ZoomMode,
        });
    }


    /// <summary>
    /// Re-fits the viewport to an animation frame whose size differs from the
    /// previous one. UI thread, outside <c>_lock</c> (same as <see cref="SetZoomMode"/>).
    /// </summary>
    internal void RefitForFrameSize(Size newSize)
    {
        if (newSize.IsEmpty || newSize == BitmapSize) return;

        BitmapSize = newSize;

        // the previous frame's rects belong to a differently-sized image
        _logicalSrcPoint = default;
        _zooming.ZoomedPoint = new();

        // a manual zoom factor survives; otherwise refit via the active mode
        if (!_zooming.IsManual)
        {
            _zooming.Factor = CalculateZoomFactor(_zooming.Mode, newSize.Width, newSize.Height);
        }
        _zooming.OldFactor = _zooming.Factor;

        CalculateDrawingRegion();

        if (!SourceSelection.IsEmpty) SetSourceSelection(SourceSelection);

        ZoomChanged?.Invoke(this, new ViewerZoomEventArgs()
        {
            ZoomFactor = _zooming.Factor,
            IsManualZoom = _zooming.IsManual,
            IsZoomModeChange = false,
            IsPreviewingImage = _isPreviewing,
            ChangeSource = ZoomChangeSource.FrameChanged,
        });
    }


    /// <summary>
    /// Calculates zoom factor by the input zoom mode, and source size.
    /// </summary>
    public double CalculateZoomFactor(ZoomMode zoomMode, double srcWidth, double srcHeight)
    {
        return CalculateZoomFactor(zoomMode, srcWidth, srcHeight, DrawingArea.Width, DrawingArea.Height);
    }


    /// <summary>
    /// Calculates zoom factor by the input zoom mode, and source size.
    /// </summary>
    public double CalculateZoomFactor(ZoomMode zoomMode, double srcWidth, double srcHeight, double viewportW, double viewportH)
    {
        if (srcWidth == 0 || srcHeight == 0 || viewportW == 0 || viewportH == 0) return _zooming.Factor;

        var widthScale = viewportW / srcWidth * Dpi;
        var heightScale = viewportH / srcHeight * Dpi;
        double zoomFactor;

        if (zoomMode == ZoomMode.ScaleToWidth)
        {
            zoomFactor = widthScale;
        }
        else if (zoomMode == ZoomMode.ScaleToHeight)
        {
            zoomFactor = heightScale;
        }
        else if (zoomMode == ZoomMode.ScaleToFit)
        {
            zoomFactor = Math.Min(widthScale, heightScale);
        }
        else if (zoomMode == ZoomMode.ScaleToFill)
        {
            zoomFactor = Math.Max(widthScale, heightScale);
        }
        else if (zoomMode == ZoomMode.LockZoom)
        {
            zoomFactor = _zooming.Factor;
        }
        // AutoZoom
        else
        {
            // viewbox size >= image size
            if (widthScale >= 1 && heightScale >= 1)
            {
                zoomFactor = 1; // show original size
            }
            else
            {
                zoomFactor = Math.Min(widthScale, heightScale);
            }
        }

        return zoomFactor;
    }




    /// <summary>
    /// Zooms into the image.
    /// </summary>
    /// <param name="point">
    /// Client's cursor location to zoom into.
    /// </param>
    /// <returns>
    ///   <list type="table">
    ///     <item><c>true</c> if the viewport is changed.</item>
    ///     <item><c>false</c> if the viewport is unchanged.</item>
    ///   </list>
    /// </returns>
    public bool ZoomIn(Point? point = null, bool requestRerender = true)
    {
        return ZoomByDeltaToPoint(Const.MOUSE_WHEEL_SCROLL_DELTA, point, requestRerender);
    }


    /// <summary>
    /// Zooms out of the image.
    /// </summary>
    /// <param name="point">
    /// Client's cursor location to zoom out.
    /// </param>
    /// <returns>
    ///   <list type="table">
    ///     <item><c>true</c> if the viewport is changed.</item>
    ///     <item><c>false</c> if the viewport is unchanged.</item>
    ///   </list>
    /// </returns>
    public bool ZoomOut(Point? point = null, bool requestRerender = true)
    {
        return ZoomByDeltaToPoint(-Const.MOUSE_WHEEL_SCROLL_DELTA, point, requestRerender);
    }


    /// <summary>
    /// Scales the image using factor value.
    /// </summary>
    /// <param name="factor">Zoom factor (<c>1.0f = 100%</c>).</param>
    /// <param name="point">
    /// Client's cursor location to zoom out.
    /// If its value is <c>null</c> or outside of the <see cref="VirtualViewerControl"/> control,
    /// use DrawingArea center point.
    /// </param>
    /// <returns>
    ///   <list type="table">
    ///     <item><c>true</c> if the viewport is changed.</item>
    ///     <item><c>false</c> if the viewport is unchanged.</item>
    ///   </list>
    /// </returns>
    public bool ZoomToPoint(float factor, Point? point = null, bool requestRerender = true)
    {
        if (factor >= _zooming.Max || factor <= _zooming.Min) return false;

        var newZoomFactor = (double)factor;
        if (newZoomFactor == _zooming.Factor) return false;

        // Use the provided point, or fall back to the last tracked cursor position.
        // If neither is valid, use the center of the drawing area.
        var location = point ?? _zooming.ZoomedPoint;
        if (location == default || !Bounds.Contains(location))
        {
            location = DrawingArea.Center;
        }

        _zooming.OldFactor = _zooming.Factor;
        _zooming.Factor = Math.Min(MaxZoom, Math.Max(newZoomFactor, MinZoom));
        _zooming.IsManual = true;
        _zooming.ZoomedPoint = location;

        // update drawing regions (handles zoom-to-cursor anchoring)
        CalculateDrawingRegion();

        if (requestRerender) InvalidateVisual();

        // emit ZoomChanged event
        ZoomChanged?.Invoke(this, new ViewerZoomEventArgs()
        {
            ZoomFactor = _zooming.Factor,
            IsManualZoom = _zooming.IsManual,
            IsZoomModeChange = false,
            IsPreviewingImage = _isPreviewing,
            ChangeSource = ZoomChangeSource.Unknown,
        });

        return true;
    }


    /// <summary>
    /// Scales the image using delta value.
    /// </summary>
    /// <param name="delta">Delta value.
    ///   <list type="table">
    ///     <item><c>delta <![CDATA[>]]> 0</c>: Zoom in.</item>
    ///     <item><c>delta <![CDATA[<]]> 0</c>: Zoom out.</item>
    ///   </list>
    /// </param>
    /// <param name="point">
    /// Client's cursor location to zoom out.
    /// </param>
    /// <returns>
    ///   <list type="table">
    ///     <item><c>true</c> if the viewport is changed.</item>
    ///     <item><c>false</c> if the viewport is unchanged.</item>
    ///   </list>
    /// </returns>
    public bool ZoomByDeltaToPoint(double delta, Point? point = null, bool requestRerender = true)
    {
        var newZoomFactor = _zooming.Factor;
        var isZoomingByMouseWheel = Math.Abs(delta) == Const.MOUSE_WHEEL_SCROLL_DELTA;

        // use zoom levels
        if (ZoomLevels.Length > 0 && isZoomingByMouseWheel)
        {
            var minZoomLevel = ZoomLevels[0];
            var maxZoomLevel = ZoomLevels[^1];

            // zoom in
            if (delta > 0)
            {
                newZoomFactor = ZoomLevels.FirstOrDefault(i => i > _zooming.Factor);
            }
            // zoom out
            else if (delta < 0)
            {
                newZoomFactor = ZoomLevels.LastOrDefault(i => i < _zooming.Factor);
            }
            if (newZoomFactor == 0) return false;

            // limit zoom factor
            newZoomFactor = Math.Min(Math.Max(minZoomLevel, newZoomFactor), maxZoomLevel);

        }
        // use smooth zooming
        else
        {
            var speed = delta / (ZoomInfo.MAX_ZOOM_SPEED - ZoomSpeed + 1);

            // zoom in
            if (delta > 0)
            {
                newZoomFactor = _zooming.Factor * (1f + speed);
            }
            // zoom out
            else if (delta < 0)
            {
                newZoomFactor = _zooming.Factor / (1f - speed);
            }

            // limit zoom factor
            newZoomFactor = Math.Min(Math.Max(MinZoom, newZoomFactor), MaxZoom);
        }


        if (newZoomFactor == _zooming.Factor) return false;

        var location = point ?? new Point(-1, -1);

        // use the center point if the point is outside
        if (!Bounds.Contains(location))
        {
            location = DrawingArea.Center;
        }


        _zooming.OldFactor = _zooming.Factor;
        _zooming.Factor = newZoomFactor;
        _zooming.IsManual = true;
        _zooming.ZoomedPoint = new(location.X, location.Y);


        // update drawing regions
        CalculateDrawingRegion();


        if (requestRerender) InvalidateVisual();


        // emit ZoomChanged event
        ZoomChanged?.Invoke(this, new ViewerZoomEventArgs()
        {
            ZoomFactor = _zooming.Factor,
            IsManualZoom = _zooming.IsManual,
            IsZoomModeChange = false,
            IsPreviewingImage = _isPreviewing,
            ChangeSource = ZoomChangeSource.Unknown,
        });

        return true;
    }


    /// <summary>
    /// Pan the current viewport to a distance.
    /// </summary>
    /// <param name="hDistance">Horizontal distance.</param>
    /// <param name="vDistance">Vertical distance.</param>
    /// <param name="requestRerender"><c>true</c> to request the control invalidates.</param>
    /// 
    /// <returns>
    /// <list type="table">
    ///   <item><c>true</c> if the viewport is changed.</item>
    ///   <item><c>false</c> if the viewport is unchanged.</item>
    /// </list>
    /// </returns>
    public bool PanTo(double hDistance, double vDistance, Point? pointerPosition, bool requestRerender = true)
    {
        if (hDistance == 0 && vDistance == 0) return false;

        hDistance *= Dpi;
        vDistance *= Dpi;
        var oldSrcRect = SrcRect;


        // horizontal
        if (hDistance != 0)
        {
            var newX = _logicalSrcPoint.X + hDistance / _zooming.Factor;
            _logicalSrcPoint = _logicalSrcPoint.WithX(newX);
        }

        // vertical 
        if (vDistance != 0)
        {
            var newY = _logicalSrcPoint.Y + vDistance / _zooming.Factor;
            _logicalSrcPoint = _logicalSrcPoint.WithY(newY);
        }

        if (pointerPosition is not null)
        {
            _zooming.ZoomedPoint = pointerPosition.Value;
        }


        // emit panning event
        Panning?.Invoke(this, new ViewerPanEventArgs(oldSrcRect, SrcRect));

        // update drawing regions
        CalculateDrawingRegion();


        if (requestRerender) InvalidateVisual();

        return true;
    }


    /// <summary>
    /// Moves the view to the left by a specified distance.
    /// </summary>
    /// <param name="distance">Distance to move</param>
    /// <param name="requestRerender"><c>true</c> to request the control invalidates.</param>
    public void PanLeft(double distance = 0, bool requestRerender = true)
    {
        distance *= PanSpeed;
        distance = Math.Max(distance, 0); // min 0

        _ = PanTo(-distance, 0, null, requestRerender);
    }


    /// <summary>
    /// Moves the view to the right by a specified distance.
    /// </summary>
    /// <param name="distance">Distance to move</param>
    /// <param name="requestRerender"><c>true</c> to request the control invalidates.</param>
    public void PanRight(double distance = 0, bool requestRerender = true)
    {
        distance *= PanSpeed;
        distance = Math.Max(distance, 0); // min 0

        _ = PanTo(distance, 0, null, requestRerender);
    }


    /// <summary>
    /// Moves the view to the top by a specified distance.
    /// </summary>
    /// <param name="distance">Distance to move</param>
    /// <param name="requestRerender"><c>true</c> to request the control invalidates.</param>
    public void PanUp(double distance = 0, bool requestRerender = true)
    {
        distance *= PanSpeed;
        distance = Math.Max(distance, 0); // min 0

        _ = PanTo(0, -distance, null, requestRerender);
    }


    /// <summary>
    /// Moves the view to the bottom by a specified distance.
    /// </summary>
    /// <param name="distance">Distance to move</param>
    /// <param name="requestRerender"><c>true</c> to request the control invalidates.</param>
    public void PanDown(double distance = 0, bool requestRerender = true)
    {
        distance *= PanSpeed;
        distance = Math.Max(distance, 0); // min 0

        _ = PanTo(0, distance, null, requestRerender);
    }




    #endregion // Public Methods

}
