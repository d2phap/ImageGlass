/*
ImageGlass Project - Image viewer for Windows
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

using D2Phap.DXControl;
using DirectN;
using ImageGlass.Base;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using System.ComponentModel;
using WicNet;

namespace ImageGlass.Viewer;

/// <summary>
/// ViewerCanvas partial class for image comparison mode.
/// </summary>
public partial class ViewerCanvas
{
    // Comparison mode private fields
    #region Comparison mode private fields

    private bool _comparisonMode = false;
    private float _comparisonSliderPos = 0.5f;
    private float _comparisonSliderHandleY = 0.5f;
    private bool _isDraggingComparisonSlider = false;
    private ComparisonPaneHover _comparisonPaneHover = ComparisonPaneHover.None;

    // Comparison image resources
    private WicBitmapSource? _wicCompareImage;
    private IComObject<ID2D1Bitmap1>? _d2dCompareImage;
    private string _compareImagePath = string.Empty;
    private float _compareImageWidth = 0;
    private float _compareImageHeight = 0;

    // Slider handle icon resources
    private WicBitmapSource? _wicCompareSliderHandle;
    private IComObject<ID2D1Bitmap1>? _d2dCompareSliderHandle;

    // Slider appearance (base values before DPI scaling)
    private const float SLIDER_LINE_HIT_WIDTH_BASE = 20f;
    private const float SLIDER_LINE_WIDTH_BASE = 2f;
    private const float SLIDER_HANDLE_SIZE_BASE = 20f;
    private const float SLIDER_HANDLE_HIT_SIZE_BASE = 26f;

    // Drop highlight state
    private ComparisonPaneHover _dropHighlightPane = ComparisonPaneHover.None;

    #endregion


    // Comparison mode public properties
    #region Comparison mode public properties

    /// <summary>
    /// Gets or sets whether comparison mode is enabled.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ComparisonMode
    {
        get => _comparisonMode;
        set
        {
            if (_comparisonMode != value)
            {
                _comparisonMode = value;
                Invalidate();
                ComparisonModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets or sets the comparison slider position (0.0 to 1.0).
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float ComparisonSliderPosition
    {
        get => _comparisonSliderPos;
        set
        {
            var clamped = Math.Max(0f, Math.Min(1f, value));
            if (Math.Abs(_comparisonSliderPos - clamped) > 0.001f)
            {
                _comparisonSliderPos = clamped;
                Invalidate();
                ComparisonSliderChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets the file path of the comparison image.
    /// </summary>
    public string CompareImagePath => _compareImagePath;

    /// <summary>
    /// Gets whether a comparison image is loaded.
    /// </summary>
    public bool HasCompareImage => _wicCompareImage != null;

    /// <summary>
    /// Gets which pane the mouse is currently hovering over.
    /// </summary>
    public ComparisonPaneHover ComparisonPaneHover => _comparisonPaneHover;

    /// <summary>
    /// Gets the current slider X position in screen coordinates.
    /// </summary>
    public int ComparisonSliderScreenX => GetSliderScreenX();

    /// <summary>
    /// Gets or sets the vertical position of the slider handle (0.0 = top, 1.0 = bottom).
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float ComparisonSliderHandleY
    {
        get => _comparisonSliderHandleY;
        set
        {
            var clamped = Math.Max(0f, Math.Min(1f, value));
            if (Math.Abs(_comparisonSliderHandleY - clamped) > 0.001f)
            {
                _comparisonSliderHandleY = clamped;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the comparison slider handle icon.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public WicBitmapSource? CompareSliderHandleImage
    {
        set
        {
            _wicCompareSliderHandle = value;
            _d2dCompareSliderHandle?.Dispose();
            _d2dCompareSliderHandle = null;
        }
    }

    /// <summary>
    /// Gets or sets the text shown when dropping to replace the main image.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string DropToReplaceMainImageText { get; set; } = "Drop to replace main image";

    /// <summary>
    /// Gets or sets the text shown when dropping to set the comparison image.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string DropToSetComparisonImageText { get; set; } = "Drop to set comparison image";

    #endregion


    // Comparison mode events
    #region Comparison mode events

    /// <summary>
    /// Occurs when comparison mode is toggled.
    /// </summary>
    public event EventHandler? ComparisonModeChanged;

    /// <summary>
    /// Occurs when the comparison slider position changes.
    /// </summary>
    public event EventHandler? ComparisonSliderChanged;

    /// <summary>
    /// Occurs when a comparison pane is clicked.
    /// </summary>
    public event EventHandler<ComparisonPaneClickedEventArgs>? ComparisonPaneClicked;

    /// <summary>
    /// Occurs when a file is dropped on a comparison pane.
    /// </summary>
    public event EventHandler<ComparisonPaneDropEventArgs>? ComparisonPaneFileDrop;

    #endregion


    // Comparison mode public methods
    #region Comparison mode public methods

    /// <summary>
    /// Sets the comparison image from image data.
    /// </summary>
    public void SetCompareImage(IgImgData? imgData, string filePath = "")
    {
        DisposeCompareImageResources();

        if (imgData == null || imgData.IsImageNull)
        {
            _compareImagePath = string.Empty;
            _compareImageWidth = 0;
            _compareImageHeight = 0;
        }
        else
        {
            _compareImagePath = filePath;
            _wicCompareImage = imgData.Image;
            _d2dCompareImage = DXHelper.ToD2D1Bitmap(Device, _wicCompareImage);

            if (_wicCompareImage != null)
            {
                _compareImageWidth = _wicCompareImage.Width;
                _compareImageHeight = _wicCompareImage.Height;
            }
        }

        Invalidate();
    }

    /// <summary>
    /// Sets the comparison image from a WIC bitmap source.
    /// </summary>
    public void SetCompareImage(WicBitmapSource? image, string filePath = "")
    {
        DisposeCompareImageResources();

        if (image == null)
        {
            _compareImagePath = string.Empty;
            _compareImageWidth = 0;
            _compareImageHeight = 0;
        }
        else
        {
            _compareImagePath = filePath;
            _wicCompareImage = image;
            _d2dCompareImage = DXHelper.ToD2D1Bitmap(Device, _wicCompareImage);
            _compareImageWidth = image.Width;
            _compareImageHeight = image.Height;
        }

        Invalidate();
    }

    /// <summary>
    /// Clears the comparison image.
    /// </summary>
    public void ClearCompareImage()
    {
        DisposeCompareImageResources();
        _compareImagePath = string.Empty;
        _compareImageWidth = 0;
        _compareImageHeight = 0;
        Invalidate();
    }

    /// <summary>
    /// Swaps the main image and comparison image.
    /// </summary>
    public void SwapCompareImages()
    {
        if (!_comparisonMode) return;

        (_wicImage, _wicCompareImage) = (_wicCompareImage, _wicImage);
        (_d2dImage, _d2dCompareImage) = (_d2dCompareImage, _d2dImage);

        var tempWidth = SourceWidth;
        var tempHeight = SourceHeight;
        SourceWidth = _compareImageWidth;
        SourceHeight = _compareImageHeight;
        _compareImageWidth = tempWidth;
        _compareImageHeight = tempHeight;

        Invalidate();
    }

    /// <summary>
    /// Resets the comparison slider to center.
    /// </summary>
    public void ResetComparisonSlider()
    {
        ComparisonSliderPosition = 0.5f;
        ComparisonSliderHandleY = 0.5f;

        if (IsWeb2ComparisonModeActive)
        {
            SetWeb2ComparisonSlider(0.5f);
        }
    }

    /// <summary>
    /// Sets the drop highlight pane to show visual feedback during drag operations.
    /// </summary>
    public void SetComparisonDropHighlight(ComparisonPaneHover pane)
    {
        if (_dropHighlightPane != pane)
        {
            _dropHighlightPane = pane;
            Invalidate();
        }
    }

    /// <summary>
    /// Clears the drop highlight visual feedback.
    /// </summary>
    public void ClearComparisonDropHighlight()
    {
        if (_dropHighlightPane != ComparisonPaneHover.None)
        {
            _dropHighlightPane = ComparisonPaneHover.None;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets the target pane for a drop at the given screen coordinates.
    /// </summary>
    public ComparisonPaneHover GetDropTargetPane(Point screenPoint)
    {
        if (!_comparisonMode) return ComparisonPaneHover.None;

        var clientPoint = PointToClient(screenPoint);
        var sliderX = GetSliderScreenX();

        return clientPoint.X < sliderX
            ? ComparisonPaneHover.LeftPane
            : ComparisonPaneHover.RightPane;
    }

    #endregion


    // Comparison mode private methods
    #region Comparison mode private methods

    /// <summary>
    /// Disposes comparison image resources.
    /// </summary>
    private void DisposeCompareImageResources()
    {
        _d2dCompareImage?.Dispose();
        _d2dCompareImage = null;
        _wicCompareImage = null;
    }

    /// <summary>
    /// Gets which pane a point is in during comparison mode.
    /// </summary>
    public ComparisonPaneHover GetComparisonPaneAt(Point point)
    {
        if (!_comparisonMode) return ComparisonPaneHover.None;

        var sliderX = GetSliderScreenX();
        var handleX = GetSliderScreenXClamped();
        var handleY = GetSliderHandleScreenY();

        // Check handle hit area (circular)
        var handleHitSize = DpiApi.Scale(SLIDER_HANDLE_HIT_SIZE_BASE);
        var dx = point.X - handleX;
        var dy = point.Y - handleY;
        var distSq = dx * dx + dy * dy;
        var handleRadius = handleHitSize / 2f;
        if (distSq <= handleRadius * handleRadius)
        {
            return ComparisonPaneHover.Slider;
        }

        // Check line hit area (rectangular along the full height)
        var lineHitWidth = DpiApi.Scale(SLIDER_LINE_HIT_WIDTH_BASE);
        var lineHitLeft = sliderX - lineHitWidth / 2f;
        var lineHitRight = sliderX + lineHitWidth / 2f;
        if (point.X >= lineHitLeft && point.X <= lineHitRight)
        {
            return ComparisonPaneHover.Slider;
        }

        return point.X < sliderX
            ? ComparisonPaneHover.LeftPane
            : ComparisonPaneHover.RightPane;
    }

    /// <summary>
    /// Gets the slider handle Y position in screen coordinates.
    /// </summary>
    private int GetSliderHandleScreenY()
    {
        var handlePadding = (int)DpiApi.Scale(20f);
        var handleY = (int)(handlePadding + (Height - 2 * handlePadding) * _comparisonSliderHandleY);
        return Math.Clamp(handleY, handlePadding, Height - handlePadding);
    }

    /// <summary>
    /// Converts slider position (0-1) to screen X coordinate.
    /// Rounds to pixel boundary to avoid splitting pixels.
    /// </summary>
    private int GetSliderScreenX()
    {
        var virtualRect = GetVirtualImageRect();
        if (virtualRect.Width > 0)
        {
            // Round to nearest pixel boundary to avoid line cutting through pixels
            return (int)MathF.Round(virtualRect.X + virtualRect.Width * _comparisonSliderPos);
        }

        return Width / 2;
    }

    /// <summary>
    /// Gets the slider screen X clamped to visible bounds.
    /// </summary>
    private int GetSliderScreenXClamped()
    {
        var sliderX = GetSliderScreenX();
        var sliderPadding = (int)DpiApi.Scale(20f);
        return Math.Clamp(sliderX, sliderPadding, Width - sliderPadding);
    }

    /// <summary>
    /// Converts screen X coordinate to slider position (0-1).
    /// </summary>
    private float ScreenXToSliderPosition(float screenX)
    {
        var virtualRect = GetVirtualImageRect();
        if (virtualRect.Width > 0)
        {
            var pos = (screenX - virtualRect.X) / virtualRect.Width;
            return Math.Clamp(pos, 0f, 1f);
        }

        return 0.5f;
    }

    /// <summary>
    /// Gets the full image rectangle in screen coordinates (unclipped by viewport).
    /// </summary>
    private RectangleF GetVirtualImageRect()
    {
        if (SourceWidth == 0 || SourceHeight == 0 || _zoomFactor == 0)
            return _destRect;

        var scaledWidth = SourceWidth * _zoomFactor;
        var scaledHeight = SourceHeight * _zoomFactor;

        var controlW = DrawingArea.Width;
        var controlH = DrawingArea.Height;

        float virtualX, virtualY;

        if (scaledWidth <= controlW)
        {
            virtualX = (controlW - scaledWidth) / 2.0f + DrawingArea.Left;
        }
        else
        {
            virtualX = DrawingArea.Left - (_srcRect.X * _zoomFactor);
        }

        if (scaledHeight <= controlH)
        {
            virtualY = (controlH - scaledHeight) / 2.0f + DrawingArea.Top;
        }
        else
        {
            virtualY = DrawingArea.Top - (_srcRect.Y * _zoomFactor);
        }

        return new RectangleF(virtualX, virtualY, scaledWidth, scaledHeight);
    }

    /// <summary>
    /// Draws the comparison overlay. Skipped when WebView2 handles comparison (SVG).
    /// </summary>
    protected virtual void DrawComparisonLayer(DXGraphics g)
    {
        if (!_comparisonMode || IsWeb2ComparisonModeActive) return;

        var sliderX = GetSliderScreenX();

        if (_d2dImage != null && Source != ImageSource.Null)
        {
            var leftClipRect = new D2D_RECT_F(0, 0, sliderX, Height);
            Device.PushAxisAlignedClip(leftClipRect, D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_ALIASED);
            g.DrawBitmap(_d2dImage, _destRect, _srcRect, (D2Phap.DXControl.InterpolationMode)CurrentInterpolation);
            Device.PopAxisAlignedClip();
        }

        if (_d2dCompareImage != null)
        {
            var rightClipRect = new D2D_RECT_F(sliderX, 0, Width, Height);
            Device.PushAxisAlignedClip(rightClipRect, D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_ALIASED);

            var compareSrcRect = CalculateCompareImageSrcRect();
            var compareDestRect = CalculateCompareImageDestRect(compareSrcRect);

            g.DrawBitmap(_d2dCompareImage, compareDestRect, compareSrcRect, (D2Phap.DXControl.InterpolationMode)CurrentInterpolation);
            Device.PopAxisAlignedClip();
        }
        else
        {
            var rightRect = new RectangleF(sliderX, 0, Width - sliderX, Height);
            DrawComparisonPlaceholder(g, rightRect);
        }

        var sliderXClamped = GetSliderScreenXClamped();
        DrawComparisonSlider(g, sliderX, sliderXClamped);

        DrawComparisonDropHighlight(g, sliderX);
    }

    /// <summary>
    /// Draws the comparison slider handle and line.
    /// </summary>
    private void DrawComparisonSlider(DXGraphics g, int sliderX, int handleX)
    {
        // DPI-scaled values
        var lineWidth = DpiApi.Scale(SLIDER_LINE_WIDTH_BASE);
        var handleSize = DpiApi.Scale(SLIDER_HANDLE_SIZE_BASE);
        var handlePadding = (int)DpiApi.Scale(20f);

        // Draw the separator line
        g.DrawLine(sliderX, 0, sliderX, Height, _accentColor, lineWidth);

        var handleY = (int)(handlePadding + (Height - 2 * handlePadding) * _comparisonSliderHandleY);
        handleY = Math.Clamp(handleY, handlePadding, Height - handlePadding);

        var handleRect = new RectangleF(
            handleX - handleSize / 2,
            handleY - handleSize / 2,
            handleSize,
            handleSize);

        if (_wicCompareSliderHandle != null)
        {
            _d2dCompareSliderHandle ??= DXHelper.ToD2D1Bitmap(Device, _wicCompareSliderHandle);

            if (_d2dCompareSliderHandle != null)
            {
                var srcRect = new RectangleF(0, 0, _wicCompareSliderHandle.Width, _wicCompareSliderHandle.Height);
                g.DrawBitmap(_d2dCompareSliderHandle, handleRect, srcRect, D2Phap.DXControl.InterpolationMode.Linear);
                return;
            }
        }

        // Draw circle handle similar to crop tool resizers
        var borderWidth = DpiApi.Scale(2f);
        g.DrawEllipse(handleRect, Color.White.WithAlpha(50), Color.Black.WithAlpha(200), DpiApi.Scale(8f));
        g.DrawEllipse(handleRect, _accentColor, _accentColor, borderWidth);

        // Draw arrows inside handle
        var arrowColor = Color.White;
        var arrowOffset = DpiApi.Scale(6f);
        var arrowTip = DpiApi.Scale(2f);
        var arrowHeight = DpiApi.Scale(4f);
        var arrowStroke = DpiApi.Scale(2f);

        // Left arrow
        g.DrawLine(handleX - arrowOffset, handleY, handleX - arrowTip, handleY - arrowHeight, arrowColor, arrowStroke);
        g.DrawLine(handleX - arrowOffset, handleY, handleX - arrowTip, handleY + arrowHeight, arrowColor, arrowStroke);

        // Right arrow
        g.DrawLine(handleX + arrowOffset, handleY, handleX + arrowTip, handleY - arrowHeight, arrowColor, arrowStroke);
        g.DrawLine(handleX + arrowOffset, handleY, handleX + arrowTip, handleY + arrowHeight, arrowColor, arrowStroke);
    }

    /// <summary>
    /// Draws a placeholder for empty comparison pane.
    /// </summary>
    private void DrawComparisonPlaceholder(DXGraphics g, RectangleF rect)
    {
        g.DrawRectangle(rect, 0, Color.Transparent, Color.Black.WithAlpha(30));
    }

    /// <summary>
    /// Draws the drop highlight overlay during drag operations.
    /// </summary>
    private void DrawComparisonDropHighlight(DXGraphics g, int sliderX)
    {
        if (_dropHighlightPane == ComparisonPaneHover.None) return;

        RectangleF highlightRect;
        if (_dropHighlightPane == ComparisonPaneHover.LeftPane)
        {
            highlightRect = new RectangleF(0, 0, sliderX, Height);
        }
        else if (_dropHighlightPane == ComparisonPaneHover.RightPane)
        {
            highlightRect = new RectangleF(sliderX, 0, Width - sliderX, Height);
        }
        else
        {
            return;
        }

        g.DrawRectangle(highlightRect, 0, Color.Transparent, _accentColor.WithAlpha(40));

        var borderInset = 4f;
        var borderRect = new RectangleF(
            highlightRect.X + borderInset,
            highlightRect.Y + borderInset,
            highlightRect.Width - borderInset * 2,
            highlightRect.Height - borderInset * 2);
        g.DrawRectangle(borderRect, 8, _accentColor.WithAlpha(180), Color.Transparent);

        var text = _dropHighlightPane == ComparisonPaneHover.LeftPane
            ? DropToReplaceMainImageText
            : DropToSetComparisonImageText;
        var textSize = g.MeasureText(text, Font.Name, Font.Size + 2, textDpi: DeviceDpi);
        var textX = highlightRect.X + (highlightRect.Width - textSize.Width) / 2;
        var textY = highlightRect.Y + (highlightRect.Height - textSize.Height) / 2;

        g.DrawText(text, Font.Name, Font.Size + 2, textX, textY, Color.White, textDpi: DeviceDpi);
    }

    /// <summary>
    /// Calculates source rectangle for comparison image using proportional view.
    /// </summary>
    private RectangleF CalculateCompareImageSrcRect()
    {
        if (_compareImageWidth == 0 || _compareImageHeight == 0 || SourceWidth == 0 || SourceHeight == 0)
            return new RectangleF(0, 0, _compareImageWidth, _compareImageHeight);

        var propX = _srcRect.X / SourceWidth;
        var propY = _srcRect.Y / SourceHeight;
        var propW = _srcRect.Width / SourceWidth;
        var propH = _srcRect.Height / SourceHeight;

        return new RectangleF(
            propX * _compareImageWidth,
            propY * _compareImageHeight,
            propW * _compareImageWidth,
            propH * _compareImageHeight
        );
    }

    /// <summary>
    /// Calculates destination rectangle for comparison image.
    /// </summary>
    private RectangleF CalculateCompareImageDestRect(RectangleF compareSrcRect)
    {
        if (compareSrcRect.Width == 0 || compareSrcRect.Height == 0 || _srcRect.Width == 0)
            return _destRect;

        var scale = _destRect.Width / _srcRect.Width;
        var destWidth = compareSrcRect.Width * scale;
        var destHeight = compareSrcRect.Height * scale;

        var mainCenterX = _destRect.X + _destRect.Width / 2;
        var mainCenterY = _destRect.Y + _destRect.Height / 2;

        return new RectangleF(
            mainCenterX - destWidth / 2,
            mainCenterY - destHeight / 2,
            destWidth,
            destHeight
        );
    }

    /// <summary>
    /// Handles mouse down for comparison mode.
    /// </summary>
    private bool HandleComparisonMouseDown(MouseEventArgs e)
    {
        if (!_comparisonMode) return false;

        var pane = GetComparisonPaneAt(e.Location);

        if (pane == ComparisonPaneHover.Slider && e.Button == MouseButtons.Left)
        {
            _isDraggingComparisonSlider = true;
            Cursor = Cursors.SizeWE;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles mouse move for comparison mode.
    /// </summary>
    private bool HandleComparisonMouseMove(MouseEventArgs e)
    {
        if (!_comparisonMode) return false;

        if (_isDraggingComparisonSlider)
        {
            ComparisonSliderPosition = ScreenXToSliderPosition(e.X);
            ComparisonSliderHandleY = ScreenYToHandlePosition(e.Y);
            return true;
        }

        var pane = GetComparisonPaneAt(e.Location);
        _comparisonPaneHover = pane;

        if (pane == ComparisonPaneHover.Slider)
        {
            Cursor = Cursors.SizeAll;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts screen Y coordinate to handle position (0-1).
    /// </summary>
    private float ScreenYToHandlePosition(float screenY)
    {
        var handlePadding = DpiApi.Scale(20f);
        var usableHeight = Height - 2 * handlePadding;
        if (usableHeight <= 0) return 0.5f;

        var pos = (screenY - handlePadding) / usableHeight;
        return Math.Clamp(pos, 0f, 1f);
    }

    /// <summary>
    /// Handles mouse up for comparison mode.
    /// </summary>
    private bool HandleComparisonMouseUp(MouseEventArgs e)
    {
        if (!_comparisonMode) return false;

        if (_isDraggingComparisonSlider)
        {
            _isDraggingComparisonSlider = false;
            Cursor = Cursors.Default;
            return true;
        }

        const int DRAG_THRESHOLD = 5;
        var wasDrag = _mouseDownPoint.HasValue &&
            (Math.Abs(e.X - _mouseDownPoint.Value.X) > DRAG_THRESHOLD ||
             Math.Abs(e.Y - _mouseDownPoint.Value.Y) > DRAG_THRESHOLD);

        if (wasDrag) return false;

        var pane = GetComparisonPaneAt(e.Location);
        if (pane == ComparisonPaneHover.LeftPane || pane == ComparisonPaneHover.RightPane)
        {
            ComparisonSliderPosition = ScreenXToSliderPosition(e.X);
            ComparisonPaneClicked?.Invoke(this, new ComparisonPaneClickedEventArgs(pane, e));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles drag enter for comparison mode.
    /// </summary>
    private ComparisonPaneHover HandleComparisonDragOver(DragEventArgs e)
    {
        if (!_comparisonMode) return ComparisonPaneHover.None;

        var clientPoint = PointToClient(new Point(e.X, e.Y));
        return GetComparisonPaneAt(clientPoint);
    }

    /// <summary>
    /// Handles drag drop for comparison mode.
    /// </summary>
    private bool HandleComparisonDragDrop(DragEventArgs e)
    {
        if (!_comparisonMode) return false;

        var clientPoint = PointToClient(new Point(e.X, e.Y));
        var pane = GetComparisonPaneAt(clientPoint);

        if (pane == ComparisonPaneHover.LeftPane || pane == ComparisonPaneHover.RightPane)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] paths && paths.Length > 0)
                {
                    ComparisonPaneFileDrop?.Invoke(this, new ComparisonPaneDropEventArgs(pane, paths[0]));
                    return true;
                }
            }
        }

        return false;
    }

    #endregion
}


/// <summary>
/// Specifies which pane is being hovered in comparison mode.
/// </summary>
public enum ComparisonPaneHover
{
    None,
    LeftPane,
    RightPane,
    Slider,
}


/// <summary>
/// Event args for comparison pane click events.
/// </summary>
public class ComparisonPaneClickedEventArgs : EventArgs
{
    public ComparisonPaneHover Pane { get; }
    public MouseEventArgs MouseEventArgs { get; }

    public ComparisonPaneClickedEventArgs(ComparisonPaneHover pane, MouseEventArgs mouseArgs)
    {
        Pane = pane;
        MouseEventArgs = mouseArgs;
    }
}


/// <summary>
/// Event args for comparison pane file drop events.
/// </summary>
public class ComparisonPaneDropEventArgs : EventArgs
{
    public ComparisonPaneHover Pane { get; }
    public string FilePath { get; }

    public ComparisonPaneDropEventArgs(ComparisonPaneHover pane, string filePath)
    {
        Pane = pane;
        FilePath = filePath;
    }
}
