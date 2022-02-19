/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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

using ImageGlass.Base.HybridGraphics;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.WinApi;
using ImageGlass.PhotoBox.ImageAnimator;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using unvell.D2DLib;

namespace ImageGlass.PhotoBox;


/// <summary>
/// Modern photo view box with hardware-accelerated
/// </summary>
public partial class ViewBox : HybridControl
{
    private Bitmap? _imageGdiPlus;
    private D2DBitmap? _imageD2D;
    private CancellationTokenSource? _msgTokenSrc;


    /// <summary>
    /// Gets the area of the image content to draw
    /// </summary>
    private D2DRect _srcRect = new(0, 0, 0, 0);

    /// <summary>
    /// Image viewport
    /// </summary>
    private D2DRect _destRect = new(0, 0, 0, 0);

    private Vector2 _panHostPoint;
    private Vector2 _panSpeedPoint;
    private Vector2 _panHostStartPoint;
    private float _panSpeed = 20f;

    private bool _xOut = false;
    private bool _yOut = false;
    private bool _isMouseDown = false;
    private Vector2 _drawPoint = new();

    // current zoom, minimum zoom, maximum zoom, previous zoom (bigger means zoom in)
    private float _zoomFactor = 1f;
    private float _oldZoomFactor = 1f;
    private bool _isManualZoom = false;
    private ZoomMode _zoomMode = ZoomMode.AutoZoom;
    private float _zoomSpeed = 0f;
    private Base.PhotoBox.InterpolationMode _interpolationMode = Base.PhotoBox.InterpolationMode.NearestNeighbor;

    private CheckerboardMode _checkerboardMode = CheckerboardMode.None;
    private IImageAnimator _imageAnimator;
    private AnimationSource _animationSource = AnimationSource.None;
    private bool _useHardwareAccelerationBackup = true;
    private bool _shouldRecalculateDrawingRegion = true;

    // Navigation buttons
    private const float NAV_PADDING = 20f;
    private bool _isNavLeftHovered = false;
    private bool _isNavLeftPressed = false;
    private bool _isNavRightHovered = false;
    private bool _isNavRightPressed = false;
    private PointF _navLeftPos => new(NavButtonSize.Width / 2 + NAV_PADDING, Height / 2);
    private PointF _navRightPos => new(Width - NavButtonSize.Width / 2 - NAV_PADDING, Height / 2);
    private NavButtonDisplay _navDisplay = NavButtonDisplay.None;
    private bool _isNavVisible = false;
    public float _navBorderRadius = 45f;


    #region Public properties




    // Viewport
    #region Viewport

    /// <summary>
    /// Gets image viewport
    /// </summary>
    [Browsable(false)]
    public RectangleF ImageViewport => new(_destRect.Location, _destRect.Size);


    /// <summary>
    /// Gets the center point of image viewport
    /// </summary>
    [Browsable(false)]
    public PointF ImageViewportCenterPoint => new()
    {
        X = ImageViewport.X + ImageViewport.Width / 2,
        Y = ImageViewport.Y + ImageViewport.Height / 2,
    };

    #endregion


    // Image information
    #region Image information

    /// <summary>
    /// Checks if the bitmap image has alpha pixels.
    /// </summary>
    [Browsable(false)]
    public bool HasAlphaPixels { get; private set; } = false;

    /// <summary>
    /// Checks if the bitmap image can animate.
    /// </summary>
    [Browsable(false)]
    public bool CanImageAnimate { get; private set; } = false;

    /// <summary>
    /// Checks if the image is animating.
    /// </summary>
    [Browsable(false)]
    public bool IsImageAnimating { get; protected set; } = false;

    /// <summary>
    /// Checks if the input image is null.
    /// </summary>
    public ImageSource Source { get; private set; } = ImageSource.Null;

    /// <summary>
    /// Gets the input image's width.
    /// </summary>
    public float ImageWidth { get; private set; } = 0;

    /// <summary>
    /// Gets the input image's height.
    /// </summary>
    public float ImageHeight { get; private set; } = 0;

    #endregion


    // Zooming
    #region Zooming

    /// <summary>
    /// Gets, sets the minimum zoom factor (<c>100% = 1.0f</c>)
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(0.01f)]
    public float MinZoom { get; set; } = 0.01f;

    /// <summary>
    /// Gets, sets the maximum zoom factor (<c>100% = 1.0f</c>)
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(35.0f)]
    public float MaxZoom { get; set; } = 35f;

    /// <summary>
    /// Gets, sets current zoom factor (<c>100% = 1.0f</c>)
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(1.0f)]
    public float ZoomFactor
    {
        get => _zoomFactor;
        set
        {
            if (_zoomFactor != value)
            {
                _zoomFactor = value;
                _isManualZoom = true;

                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets, sets the zoom speed. Value is from -500f to 500f.
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(0f)]
    public float ZoomSpeed
    {
        get => _zoomSpeed;
        set
        {
            _zoomFactor = Math.Min(value, 500f); // max 500f
            _zoomFactor = Math.Max(value, -500f); // min -500f
        }
    }

    /// <summary>
    /// Gets, sets zoom mode
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(ZoomMode.AutoZoom)]
    public ZoomMode ZoomMode
    {
        get => _zoomMode;
        set
        {
            if (_zoomMode != value)
            {
                _zoomMode = value;
                Refresh();
            }
        }
    }

    /// <summary>
    /// Gets, sets interpolation mode
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(Base.PhotoBox.InterpolationMode.NearestNeighbor)]
    public Base.PhotoBox.InterpolationMode InterpolationMode
    {
        get => _interpolationMode;
        set
        {
            if (_interpolationMode != value)
            {
                _interpolationMode = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets, sets the panning speed. Value is from 0 to 100f.
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(20f)]
    public float PanSpeed
    {
        get => _panSpeed;
        set
        {
            _panSpeed = Math.Min(value, 100f); // max 100f
            _panSpeed = Math.Max(value, 0); // min 0
        }
    }


    /// <summary>
    /// Occurs when <see cref="ZoomFactor"/> value changes.
    /// </summary>
    public event ZoomChangedEventHandler? OnZoomChanged = null;
    public delegate void ZoomChangedEventHandler(ZoomEventArgs e);

    #endregion


    // Checkerboard
    #region Checkerboard

    [Category("Checkerboard")]
    [DefaultValue(CheckerboardMode.None)]
    public CheckerboardMode CheckerboardMode
    {
        get => _checkerboardMode;
        set
        {
            if (_checkerboardMode != value)
            {
                _checkerboardMode = value;
                Invalidate();
            }
        }
    }

    [Category("Checkerboard")]
    [DefaultValue(typeof(float), "12")]
    public float CheckerboardCellSize { get; set; } = 12f;

    [Category("Checkerboard")]
    [DefaultValue(typeof(Color), "25, 0, 0, 0")]
    public Color CheckerboardColor1 { get; set; } = Color.FromArgb(25, Color.Black);

    [Category("Checkerboard")]
    [DefaultValue(typeof(Color), "25, 255, 255, 255")]
    public Color CheckerboardColor2 { get; set; } = Color.FromArgb(25, Color.White);

    #endregion


    // Navigation Buttons
    #region Navigation Buttons

    [Category("NavigationButtons")]
    [DefaultValue(NavButtonDisplay.None)]
    public NavButtonDisplay NavDisplay
    {
        get => _navDisplay;
        set
        {
            if (_navDisplay != value)
            {
                _navDisplay = value;
                Invalidate();
            }
        }
    }

    [Category("NavigationButtons")]
    [DefaultValue(90f)]
    public SizeF NavButtonSize { get; set; } = new(90f, 90f);

    [Category("NavigationButtons")]
    [DefaultValue(1f)]
    public float NavBorderRadius
    {
        get => _navBorderRadius;
        set
        {
            _navBorderRadius = Math.Min(Math.Abs(value), NavButtonSize.Width / 2);
        }
    }

    [Category("NavigationButtons")]
    [DefaultValue(typeof(Color), "150, 0, 0, 0")]
    public Color NavHoveredColor { get; set; } = Color.FromArgb(150, Color.Black);

    [Category("NavigationButtons")]
    [DefaultValue(typeof(Color), "200, 0, 0, 0")]
    public Color NavPressedColor { get; set; } = Color.FromArgb(200, Color.Black);

    // Left button
    [Category("NavigationButtons")]
    [DefaultValue(typeof(Bitmap), null)]
    public Bitmap? NavLeftImage { get; set; }

    // Right button
    [Category("NavigationButtons")]
    [DefaultValue(typeof(Bitmap), null)]
    public Bitmap? NavRightImage { get; set; }


    /// <summary>
    /// Occurs when the left navigation button clicked.
    /// </summary>
    [Category("NavigationButtons")]
    public event NavLeftClickedEventHandler? OnNavLeftClicked = null;
    public delegate void NavLeftClickedEventHandler(MouseEventArgs e);


    /// <summary>
    /// Occurs when the right navigation button clicked.
    /// </summary>
    [Category("NavigationButtons")]
    public event NavRightClickedEventHandler? OnNavRightClicked = null;
    public delegate void NavRightClickedEventHandler(MouseEventArgs e);

    #endregion


    // Misc
    #region Misc

    /// <summary>
    /// Gets, sets border radius of message box
    /// </summary>
    [DefaultValue(1f)]
    public float MessageBorderRadius { get; set; } = 1f;

    #endregion


    // Events
    #region Events

    /// <summary>
    /// Occurs when the host is being panned
    /// </summary>
    public event PanningEventHandler? OnPanning;
    public delegate void PanningEventHandler(PanningEventArgs e);


    /// <summary>
    /// Occurs when the image is changed
    /// </summary>
    public event ImageChangedEventHandler? OnImageChanged;
    public delegate void ImageChangedEventHandler(EventArgs e);


    /// <summary>
    /// Occurs when the mouse pointer is moved over the control
    /// </summary>
    public event ImageMouseMoveEventHandler? OnImageMouseMove;
    public delegate void ImageMouseMoveEventHandler(ImageMouseMoveEventArgs e);


    #endregion


    #endregion


    public ViewBox()
    {
        // request for high resolution gif animation
        if (!TimerApi.HasRequestedRateAtLeastAsFastAs(10) && TimerApi.TimeBeginPeriod(10))
        {
            HighResolutionGifAnimator.SetTickTimeInMilliseconds(10);
        }

        _imageAnimator = new HighResolutionGifAnimator();
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        // back up value
        _useHardwareAccelerationBackup = UseHardwareAcceleration;

        // draw the control
        Refresh();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _imageD2D?.Dispose();
        _imageGdiPlus?.Dispose();

        NavLeftImage?.Dispose();
        NavRightImage?.Dispose();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (!IsReady) return;

        var requestRerender = false;

        // Navigation clickable check
        #region Navigation clickable check
        if (e.Button == MouseButtons.Left)
        {
            if (NavDisplay == NavButtonDisplay.Left
                || NavDisplay == NavButtonDisplay.Both)
            {
                // left clickable region
                var leftClickable = new RectangleF(
                _navLeftPos.X - NavButtonSize.Width / 2,
                _navLeftPos.Y - NavButtonSize.Height / 2,
                NavButtonSize.Width,
                NavButtonSize.Height);

                // calculate whether the point inside the rect
                _isNavLeftPressed = leftClickable.Contains(e.Location);
            }


            if (NavDisplay == NavButtonDisplay.Right
                || NavDisplay == NavButtonDisplay.Both)
            {
                // right clickable region
                var rightClickable = new RectangleF(
                _navRightPos.X - NavButtonSize.Width / 2,
                _navRightPos.Y - NavButtonSize.Height / 2,
                NavButtonSize.Width,
                NavButtonSize.Height);

                // calculate whether the point inside the rect
                _isNavRightPressed = rightClickable.Contains(e.Location);
            }

            requestRerender = _isNavLeftPressed || _isNavRightPressed;
        }
        #endregion


        // Image panning check
        #region Image panning check
        if (Source != ImageSource.Null)
        {
            _panHostPoint.X = e.Location.X;
            _panHostPoint.Y = e.Location.Y;
            _panSpeedPoint.X = 0;
            _panSpeedPoint.Y = 0;
            _panHostStartPoint.X = e.Location.X;
            _panHostStartPoint.Y = e.Location.Y;
        }
        #endregion


        _isMouseDown = true;
        if (requestRerender)
        {
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!IsReady) return;


        // Navigation clickable check
        #region Navigation clickable check
        if (e.Button == MouseButtons.Left)
        {
            if (_isNavLeftPressed)
            {
                // left clickable region
                var leftClickable = new RectangleF(
                    _navLeftPos.X - NavButtonSize.Width / 2,
                    _navLeftPos.Y - NavButtonSize.Height / 2,
                    NavButtonSize.Width,
                    NavButtonSize.Height);

                // emit nav button event if the point inside the rect
                if (leftClickable.Contains(e.Location))
                {
                    OnNavLeftClicked?.Invoke(e);
                }
            }
            else if (_isNavRightPressed)
            {
                // right clickable region
                var rightClickable = new RectangleF(
                    _navRightPos.X - NavButtonSize.Width / 2,
                    _navRightPos.Y - NavButtonSize.Height / 2,
                    NavButtonSize.Width,
                    NavButtonSize.Height);

                // emit nav button event if the point inside the rect
                if (rightClickable.Contains(e.Location))
                {
                    OnNavRightClicked?.Invoke(e);
                }
            }
        }

        _isNavLeftPressed = false;
        _isNavRightPressed = false;
        #endregion


        _isMouseDown = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!IsReady) return;

        var requestRerender = false;


        // Navigation hoverable check
        #region Navigation hoverable check
        // no button pressed
        if (e.Button == MouseButtons.None)
        {
            // left hoverable region
            if (NavDisplay == NavButtonDisplay.Left
                || NavDisplay == NavButtonDisplay.Both)
            {
                var leftHoverable = new RectangleF(
                _navLeftPos.X - NavButtonSize.Width / 2 - NAV_PADDING,
                _navLeftPos.Y - NavButtonSize.Height / 2 * 3,
                NavButtonSize.Width + NAV_PADDING,
                NavButtonSize.Height * 3);

                // calculate whether the point inside the rect
                _isNavLeftHovered = leftHoverable.Contains(e.Location);
            }

            // right hoverable region
            if (NavDisplay == NavButtonDisplay.Right
                || NavDisplay == NavButtonDisplay.Both)
            {
                var rightHoverable = new RectangleF(
                _navRightPos.X - NavButtonSize.Width / 2,
                _navRightPos.Y - NavButtonSize.Height / 2 * 3,
                NavButtonSize.Width + NAV_PADDING,
                NavButtonSize.Height * 3);

                // calculate whether the point inside the rect
                _isNavRightHovered = rightHoverable.Contains(e.Location);
            }

            if (!_isNavLeftHovered && !_isNavRightHovered && _isNavVisible)
            {
                requestRerender = true;
                _isNavVisible = false;
            }
            else
            {
                requestRerender = _isNavVisible = _isNavLeftHovered || _isNavRightHovered;
            }
        }
        #endregion


        // Image panning check
        if (_isMouseDown)
        {
            requestRerender = PanTo(
                _panHostPoint.X - e.Location.X,
                _panHostPoint.Y - e.Location.Y,
                false);
        }


        // emit event OnImageMouseMove
        var imgX = (e.X - _destRect.X) / _zoomFactor + _srcRect.X;
        var imgY = (e.Y - _destRect.Y) / _zoomFactor + _srcRect.Y;
        OnImageMouseMove?.Invoke(new(imgX, imgY, e.Button));


        // request re-render control
        if (requestRerender)
        {
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        _isNavLeftHovered = false;
        _isNavRightHovered = false;


        if (_isNavVisible)
        {
            _isNavVisible = false;
            Invalidate();
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (!IsReady || Source == ImageSource.Null || e.Delta == 0) return;

        _ = ZoomToPoint(e.Delta, e.Location);
    }

    protected override void OnResize(EventArgs e)
    {
        _shouldRecalculateDrawingRegion = true;

        // redraw the control on resizing if it's not manual zoom
        if (IsReady && Source != ImageSource.Null && !_isManualZoom)
        {
            Refresh();
        }

        base.OnResize(e);
    }

    protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
    {
        base.OnPreviewKeyDown(e);


        if (e.KeyCode == Keys.Right)
        {
            StartAnimation(AnimationSource.PanRight);
        }
        else if (e.KeyCode == Keys.Left)
        {
            StartAnimation(AnimationSource.PanLeft);
        }
        else if (e.KeyCode == Keys.Up)
        {
            StartAnimation(AnimationSource.PanUp);
        }
        else if (e.KeyCode == Keys.Down)
        {
            StartAnimation(AnimationSource.PanDown);
        }
        else if (e.KeyCode == Keys.Oemplus)
        {
            StartAnimation(AnimationSource.ZoomIn);
        }
        else if (e.KeyCode == Keys.OemMinus)
        {
            StartAnimation(AnimationSource.ZoomOut);
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        // Panning
        if (_animationSource.HasFlag(AnimationSource.PanLeft))
        {
            StopAnimation(AnimationSource.PanLeft);
        }
        if (_animationSource.HasFlag(AnimationSource.PanRight))
        {
            StopAnimation(AnimationSource.PanRight);
        }
        if (_animationSource.HasFlag(AnimationSource.PanUp))
        {
            StopAnimation(AnimationSource.PanUp);
        }
        if (_animationSource.HasFlag(AnimationSource.PanDown))
        {
            StopAnimation(AnimationSource.PanDown);
        }

        // Zooming
        if (_animationSource.HasFlag(AnimationSource.ZoomIn))
        {
            StopAnimation(AnimationSource.ZoomIn);
        }
        if (_animationSource.HasFlag(AnimationSource.ZoomOut))
        {
            StopAnimation(AnimationSource.ZoomOut);
        }
    }

    protected override void OnFrame()
    {
        base.OnFrame();


        // Panning
        if (_animationSource.HasFlag(AnimationSource.PanLeft))
        {
            _ = PanTo(-PanSpeed, 0, requestRerender: false);
        }
        else if (_animationSource.HasFlag(AnimationSource.PanRight))
        {
            _ = PanTo(PanSpeed, 0, requestRerender: false);
        }

        if (_animationSource.HasFlag(AnimationSource.PanUp))
        {
            _ = PanTo(0, -PanSpeed, requestRerender: false);
        }
        else if (_animationSource.HasFlag(AnimationSource.PanDown))
        {
            _ = PanTo(0, PanSpeed, requestRerender: false);
        }

        // Zooming
        if (_animationSource.HasFlag(AnimationSource.ZoomIn))
        {
            _ = ZoomToPoint(20, requestRerender: false);
        }
        else if (_animationSource.HasFlag(AnimationSource.ZoomOut))
        {
            _ = ZoomToPoint(-20, requestRerender: false);
        }
    }

    protected override void OnRender(IHybridGraphics g)
    {
        base.OnRender(g);


        // update drawing regions
        CalculateDrawingRegion();

        // checkerboard background
        DrawCheckerboardLayer(g);


        if (CanImageAnimate)
        {
            DrawGifFrame(g);
        }
        else
        {
            // image layer
            DrawImageLayer(g);
        }

        // text message
        DrawMessageLayer(g);

        // navigation layer
        DrawNavigationLayer(g);
    }


    /// <summary>
    /// Draw GIF frame using GDI+
    /// </summary>
    /// <param name="hg"></param>
    protected virtual void DrawGifFrame(IHybridGraphics hg)
    {
        if (Source == ImageSource.Null) return;

        // use GDI+ to handle GIF animation
        var g = hg as GDIGraphics;
        if (g is null) return;

        g.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
        g.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        try
        {
            if (IsImageAnimating && !DesignMode)
            {
                _imageAnimator.UpdateFrames(_imageGdiPlus);
            }

            g.DrawImage(_imageGdiPlus, _destRect, _srcRect, 1, (int)_interpolationMode);
        }
        catch (ArgumentException)
        {
            // ignore errors that occur due to the image being disposed
        }
        catch (OutOfMemoryException)
        {
            // also ignore errors that occur due to running out of memory
        }
        catch (ExternalException)
        {
            // stop the animation and reset to the first frame.
            IsImageAnimating = false;
            _imageAnimator.StopAnimate(_imageGdiPlus, OnImageFrameChanged);
        }
        catch (InvalidOperationException)
        {
            // issue #373: a race condition caused this exception: deleting the image from underneath us could
            // cause a collision in HighResolutionGif_animator. I've not been able to repro; hopefully this is
            // the correct response.

            // stop the animation and reset to the first frame.
            IsImageAnimating = false;
            _imageAnimator.StopAnimate(_imageGdiPlus, OnImageFrameChanged);
        }

        g.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
    }


    /// <summary>
    /// Calculates the drawing region
    /// </summary>
    protected virtual void CalculateDrawingRegion()
    {
        if (Source == ImageSource.Null || _shouldRecalculateDrawingRegion is false) return;

        var zoomX = _drawPoint.X;
        var zoomY = _drawPoint.Y;

        _xOut = false;
        _yOut = false;

        var clientW = Width;
        var clientH = Height;

        if (clientW > ImageWidth * _zoomFactor)
        {
            _srcRect.X = 0;
            _srcRect.Width = ImageWidth;
            _destRect.X = (clientW - ImageWidth * _zoomFactor) / 2.0f;
            _destRect.Width = ImageWidth * _zoomFactor;
        }
        else
        {
            _srcRect.X += (clientW / _oldZoomFactor - clientW / _zoomFactor) / ((clientW + 0.001f) / zoomX);
            _srcRect.Width = clientW / _zoomFactor;
            _destRect.X = 0;
            _destRect.Width = clientW;
        }


        if (clientH > ImageHeight * _zoomFactor)
        {
            _srcRect.Y = 0;
            _srcRect.Height = ImageHeight;
            _destRect.Y = (clientH - ImageHeight * _zoomFactor) / 2f;
            _destRect.Height = ImageHeight * _zoomFactor;
        }
        else
        {
            _srcRect.Y += (clientH / _oldZoomFactor - clientH / _zoomFactor) / ((clientH + 0.001f) / zoomY);
            _srcRect.Height = clientH / _zoomFactor;
            _destRect.Y = 0;
            _destRect.Height = clientH;
        }

        _oldZoomFactor = _zoomFactor;
        //------------------------

        if (_srcRect.X + _srcRect.Width > ImageWidth)
        {
            _xOut = true;
            _srcRect.X = ImageWidth - _srcRect.Width;
        }

        if (_srcRect.X < 0)
        {
            _xOut = true;
            _srcRect.X = 0;
        }

        if (_srcRect.Y + _srcRect.Height > ImageHeight)
        {
            _yOut = true;
            _srcRect.Y = ImageHeight - _srcRect.Height;
        }

        if (_srcRect.Y < 0)
        {
            _yOut = true;
            _srcRect.Y = 0;
        }

        _shouldRecalculateDrawingRegion = false;
    }


    /// <summary>
    /// Draw the input image.
    /// </summary>
    /// <param name="g">Drawing graphic object.</param>
    protected virtual void DrawImageLayer(IHybridGraphics g)
    {
        if (Source == ImageSource.Null) return;

        if (Source == ImageSource.Direct2D)
        {
            var d2dg = g as Direct2DGraphics;
            d2dg?.DrawImage(_imageD2D, _destRect, _srcRect, 1f, (int)_interpolationMode);
        }
        else
        {
            g.DrawImage(_imageGdiPlus, _destRect, _srcRect, 1f, (int)_interpolationMode);
        }
    }


    /// <summary>
    /// Draw checkerboard background
    /// </summary>
    /// <param name="g"></param>
    protected virtual void DrawCheckerboardLayer(IHybridGraphics g)
    {
        if (CheckerboardMode == CheckerboardMode.None) return;

        // region to draw
        Rectangle region;

        if (CheckerboardMode == CheckerboardMode.Image)
        {
            // no need to draw checkerboard if image does not has alpha pixels
            if (!HasAlphaPixels) return;

            region = (Rectangle)_destRect;
        }
        else
        {
            region = ClientRectangle;
        }


        if (UseHardwareAcceleration)
        {
            // grid size
            int rows = (int)Math.Ceiling(region.Width / CheckerboardCellSize);
            int cols = (int)Math.Ceiling(region.Height / CheckerboardCellSize);

            // draw grid
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Color color;
                    if ((row + col) % 2 == 0)
                    {
                        color = CheckerboardColor1;
                    }
                    else
                    {
                        color = CheckerboardColor2;
                    }

                    var drawnW = row * CheckerboardCellSize;
                    var drawnH = col * CheckerboardCellSize;

                    var x = drawnW + region.X;
                    var y = drawnH + region.Y;

                    var w = Math.Min(region.Width - drawnW, CheckerboardCellSize);
                    var h = Math.Min(region.Height - drawnH, CheckerboardCellSize);

                    g.FillRectangle(new(x, y, w, h), color);
                }
            }
        }
        else
        {
            // use GDI+ Texture
            var gdiG = g as GDIGraphics;

            using var checkerTile = CreateCheckerBoxTile(CheckerboardCellSize, CheckerboardColor1, CheckerboardColor2);
            using var texture = new TextureBrush(checkerTile);

            gdiG?.Graphics.FillRectangle(texture, region);
        }
    }


    /// <summary>
    /// Draws text message
    /// </summary>
    /// <param name="g"></param>
    protected virtual void DrawMessageLayer(IHybridGraphics g)
    {
        if (Text.Trim().Length == 0) return;

        var textMargin = 20;
        var textPaddingX = textMargin * 2;
        var textPaddingY = textMargin;

        var drawableArea = new Rectangle(
            textMargin,
            textMargin,
            Width - textPaddingX,
            Height - textPaddingY);

        // calculate text region
        var fontSize = DpiApi.Transform<float>(Font.Size * (float)DpiApi.DpiScale);
        var textSize = g.MeasureText(Text, Font, fontSize, drawableArea.Size);
        var region = new RectangleF(
            drawableArea.Width / 2 - textSize.Width / 2,
            drawableArea.Height / 2 - textSize.Height / 2,
            textSize.Width + textPaddingX,
            textSize.Height + textPaddingY);

        // draw text background
        var color = Color.FromArgb(170, BackColor);
        g.DrawRoundedRectangle(region, color, color, new(MessageBorderRadius, MessageBorderRadius));


        // draw text
        g.DrawText(Text, Font, fontSize, ForeColor, region);
    }


    /// <summary>
    /// Draws navigation arrow buttons
    /// </summary>
    /// <param name="g"></param>
    protected virtual void DrawNavigationLayer(IHybridGraphics g)
    {
        if (NavDisplay == NavButtonDisplay.None) return;


        // left navigation
        if (NavDisplay == NavButtonDisplay.Left || NavDisplay == NavButtonDisplay.Both)
        {
            var iconOpacity = 1f;
            var iconY = 0;
            var leftColor = Color.Transparent;

            if (_isNavLeftPressed)
            {
                leftColor = NavPressedColor;
                iconOpacity = 0.7f;
                iconY = 1;
            }
            else if (_isNavLeftHovered)
            {
                leftColor = NavHoveredColor;
            }

            // draw background
            if (leftColor != Color.Transparent)
            {
                var leftBg = new RectangleF(
                    _navLeftPos.X - NavButtonSize.Width / 2,
                    _navLeftPos.Y - NavButtonSize.Height / 2,
                    NavButtonSize.Width,
                    NavButtonSize.Height);

                g.DrawRoundedRectangle(leftBg, leftColor, leftColor, new PointF(NavBorderRadius, NavBorderRadius), 1.25f);
            }

            // draw icon
            if (NavLeftImage is not null && (_isNavLeftHovered || _isNavLeftPressed))
            {
                var iconSize = Math.Min(NavButtonSize.Width, NavButtonSize.Height) / 2;

                g.DrawImage(NavLeftImage,
                    new RectangleF(
                        _navLeftPos.X - iconSize / 2,
                        _navLeftPos.Y - iconSize / 2 + iconY,
                        iconSize,
                        iconSize),
                    new RectangleF(0, 0, NavLeftImage.Width, NavLeftImage.Height),
                    iconOpacity);
            }
        }


        // right navigation
        if (NavDisplay == NavButtonDisplay.Right || NavDisplay == NavButtonDisplay.Both)
        {
            var iconOpacity = 1f;
            var iconY = 0;
            var rightColor = Color.Transparent;

            if (_isNavRightPressed)
            {
                rightColor = NavPressedColor;
                iconOpacity = 0.7f;
                iconY = 1;
            }
            else if (_isNavRightHovered)
            {
                rightColor = NavHoveredColor;
            }

            // draw background
            if (rightColor != Color.Transparent)
            {
                var rightBg = new RectangleF(
                    _navRightPos.X - NavButtonSize.Width / 2,
                    _navRightPos.Y - NavButtonSize.Height / 2,
                    NavButtonSize.Width,
                    NavButtonSize.Height);

                g.DrawRoundedRectangle(rightBg, rightColor, rightColor, new PointF(NavBorderRadius, NavBorderRadius), 1.25f);
            }

            // draw icon
            if (NavRightImage is not null && (_isNavRightHovered || _isNavRightPressed))
            {
                var iconSize = Math.Min(NavButtonSize.Width, NavButtonSize.Height) / 2;

                g.DrawImage(NavRightImage,
                    new RectangleF(
                        _navRightPos.X - iconSize / 2,
                        _navRightPos.Y - iconSize / 2 + iconY,
                        iconSize,
                        iconSize),
                    new RectangleF(0, 0, NavRightImage.Width, NavRightImage.Height),
                    iconOpacity);
            }
        }
    }


    /// <summary>
    /// Updates zoom mode.
    /// </summary>
    /// <param name="mode"></param>
    protected virtual void UpdateZoomMode(ZoomMode? mode = null)
    {
        if (!IsReady || Source == ImageSource.Null) return;

        var viewportW = Width;
        var viewportH = Height;

        var horizontalPadding = Padding.Left + Padding.Right;
        var verticalPadding = Padding.Top + Padding.Bottom;
        var widthScale = (viewportW - horizontalPadding) / ImageWidth;
        var heightScale = (viewportH - verticalPadding) / ImageHeight;

        float zoomFactor;
        var zoomMode = mode ?? _zoomMode;

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
            zoomFactor = ZoomFactor;
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

        _zoomFactor = zoomFactor;
        _isManualZoom = false;
        _shouldRecalculateDrawingRegion = true;
    }


    /// <summary>
    /// Creates checker box tile for drawing checkerboard (GDI+)
    /// </summary>
    /// <param name="cellSize"></param>
    /// <param name="cellColor1"></param>
    /// <param name="cellColor2"></param>
    /// <returns></returns>
    private static Bitmap CreateCheckerBoxTile(float cellSize, Color cellColor1, Color cellColor2)
    {
        // draw the tile
        var width = cellSize * 2;
        var height = cellSize * 2;
        var result = new Bitmap((int)width, (int)height);

        using var g = Graphics.FromImage(result);
        using (Brush brush = new SolidBrush(cellColor2))
        {
            g.FillRectangle(brush, new RectangleF(cellSize, 0, cellSize, cellSize));
            g.FillRectangle(brush, new RectangleF(0, cellSize, cellSize, cellSize));
        }

        using (Brush brush = new SolidBrush(cellColor1))
        {
            g.FillRectangle(brush, new RectangleF(0, 0, cellSize, cellSize));
            g.FillRectangle(brush, new RectangleF(cellSize, cellSize, cellSize, cellSize));
        }

        return result;
    }


    /// <summary>
    /// Force the control to update zoom mode and invalidate itself.
    /// </summary>
    public new void Refresh()
    {
        UpdateZoomMode();
        Invalidate();
    }


    /// <summary>
    /// Starts a built-in animation.
    /// </summary>
    /// <param name="sources">Source of animation</param>
    public void StartAnimation(AnimationSource sources)
    {
        _animationSource = sources;
        RequestUpdateFrame = true;
    }


    /// <summary>
    /// Stops a built-in animation.
    /// </summary>
    /// <param name="sources">Source of animation</param>
    public void StopAnimation(AnimationSource sources)
    {
        _animationSource ^= sources;
        RequestUpdateFrame = false;
    }


    /// <summary>
    /// Zooms into the image.
    /// </summary>
    /// <param name="point">
    /// Client's cursor location to zoom into.
    /// <c><see cref="ImageViewportCenterPoint"/></c> is the default value.
    /// </param>
    /// <returns>
    ///   <list type="table">
    ///     <item><c>true</c> if the viewport is changed.</item>
    ///     <item><c>false</c> if the viewport is unchanged.</item>
    ///   </list>
    /// </returns>
    public bool ZoomIn(PointF? point = null, bool requestRerender = true)
    {
        return ZoomToPoint(SystemInformation.MouseWheelScrollDelta, point, requestRerender);
    }


    /// <summary>
    /// Zooms out of the image.
    /// </summary>
    /// <param name="point">
    /// Client's cursor location to zoom out.
    /// <c><see cref="ImageViewportCenterPoint"/></c> is the default value.
    /// </param>
    /// <returns>
    ///   <list type="table">
    ///     <item><c>true</c> if the viewport is changed.</item>
    ///     <item><c>false</c> if the viewport is unchanged.</item>
    ///   </list>
    /// </returns>
    public bool ZoomOut(PointF? point = null, bool requestRerender = true)
    {
        return ZoomToPoint(-SystemInformation.MouseWheelScrollDelta, point, requestRerender);
    }


    /// <summary>
    /// Scales the image using delta value.
    /// </summary>
    /// <param name="delta">Delta value.
    ///   <list type="table">
    ///     <item><c>delta<![CDATA[>]]>0</c>: Zoom in.</item>
    ///     <item><c>delta<![CDATA[<]]>0</c>: Zoom out.</item>
    ///   </list>
    /// </param>
    /// <param name="point">
    /// Client's cursor location to zoom out.
    /// <c><see cref="ImageViewportCenterPoint"/></c> is the default value.
    /// </param>
    /// <returns>
    ///   <list type="table">
    ///     <item><c>true</c> if the viewport is changed.</item>
    ///     <item><c>false</c> if the viewport is unchanged.</item>
    ///   </list>
    /// </returns>
    public bool ZoomToPoint(float delta, PointF? point = null, bool requestRerender = true)
    {
        var speed = delta / (500f - ZoomSpeed);
        var location = new PointF()
        {
            X = point?.X ?? ImageViewportCenterPoint.X,
            Y = point?.Y ?? ImageViewportCenterPoint.Y,
        };

        // zoom in
        if (delta > 0)
        {
            if (_zoomFactor > MaxZoom)
                return false;

            _oldZoomFactor = _zoomFactor;
            _zoomFactor *= 1f + speed;
            _shouldRecalculateDrawingRegion = true;
        }
        // zoom out
        else if (delta < 0)
        {
            if (_zoomFactor < MinZoom)
                return false;

            _oldZoomFactor = _zoomFactor;
            _zoomFactor /= 1f - speed;
            _shouldRecalculateDrawingRegion = true;
        }

        _isManualZoom = true;
        _drawPoint = location.ToVector2();

        if (requestRerender)
        {
            Invalidate();
        }

        // emit OnZoomChanged event
        OnZoomChanged?.Invoke(new(_zoomFactor));

        return true;
    }


    /// <summary>
    /// Pan the current viewport to a distance
    /// </summary>
    /// <param name="hDistance">Horizontal distance</param>
    /// <param name="vDistance">Vertical distance</param>
    /// <param name="requestRerender"><c>true</c> to request the control invalidates.</param>
    /// <returns>
    /// <list type="table">
    /// <item><c>true</c> if the viewport is changed.</item>
    /// <item><c>false</c> if the viewport is unchanged.</item>
    /// </list>
    /// </returns>
    public bool PanTo(float hDistance, float vDistance, bool requestRerender = true)
    {
        if (Source == ImageSource.Null) return false;
        if (hDistance <= 0 && vDistance <= 0) return false;

        var loc = PointToClient(Cursor.Position);


        // horizontal
        if (hDistance != 0)
        {
            _srcRect.X += (hDistance / _zoomFactor) + _panSpeedPoint.X;
        }

        // vertical 
        if (vDistance != 0)
        {
            _srcRect.Y += (vDistance / _zoomFactor) + _panSpeedPoint.Y;
        }

        _drawPoint = new();
        _shouldRecalculateDrawingRegion = true;


        if (_xOut == false)
        {
            _panHostPoint.X = loc.X;
        }

        if (_yOut == false)
        {
            _panHostPoint.Y = loc.Y;
        }

        // emit event
        OnPanning?.Invoke(new(loc, new(_panHostPoint)));

        if (requestRerender)
        {
            Invalidate();
        }

        return true;
    }


    /// <summary>
    /// Shows text message.
    /// </summary>
    /// <param name="text">Message to show</param>
    /// <param name="durationMs">Display duration in millisecond.
    /// Set it <b>greater than 0</b> to disable auto clear.</param>
    /// <param name="delayMs">Duration to delay before displaying the message.</param>
    private async void ShowMessagePrivate(string text, int durationMs = 0, int delayMs = 0)
    {
        var token = _msgTokenSrc?.Token ?? default;

        try
        {
            if (delayMs > 0)
            {
                await Task.Delay(delayMs, token);
            }

            Text = text;

            if (durationMs > 0)
            {
                await Task.Delay(durationMs, token);
            }
        }
        catch { }

        if (durationMs > 0 || token.IsCancellationRequested)
        {
            Text = string.Empty;
        }
    }


    /// <summary>
    /// Shows text message.
    /// </summary>
    /// <param name="text">Message to show</param>
    /// <param name="durationMs">Display duration in millisecond.
    /// Set it <b>greater than 0</b> to disable auto clear.</param>
    /// <param name="delayMs">Duration to delay before displaying the message.</param>
    public void ShowMessage(string text, int durationMs = 0, int delayMs = 0)
    {
        _msgTokenSrc?.Cancel();
        _msgTokenSrc = new();

        ShowMessagePrivate(text, durationMs, delayMs);
    }


    /// <summary>
    /// Immediately clears text message.
    /// </summary>
    public void ClearMessage()
    {
        _msgTokenSrc?.Cancel();
        Text = string.Empty;
    }


    /// <summary>
    /// Checks the input image and updates dependent properties.
    /// </summary>
    /// <param name="bmp"></param>
    private void CheckInputImage(Bitmap? bmp)
    {
        if (bmp is null)
        {
            ImageWidth = 0;
            ImageHeight = 0;
            HasAlphaPixels = false;
            CanImageAnimate = false;
        }
        else
        {
            ImageWidth = bmp.Width;
            ImageHeight = bmp.Height;
            HasAlphaPixels = bmp.PixelFormat.HasFlag(PixelFormat.Alpha);
            CanImageAnimate = _imageAnimator.CanAnimate(bmp);
        }

        var shouldUseDirect2D = !CanImageAnimate && !HasAlphaPixels;

        // backup current UseHardwardAcceleration value
        _useHardwareAccelerationBackup = UseHardwareAcceleration;

        if (!shouldUseDirect2D)
        {
            // force using GDI+
            UseHardwareAcceleration = false;
        }
    }


    /// <summary>
    /// Load image
    /// </summary>
    /// <param name="bmp"></param>
    public void SetImage(Bitmap? bmp)
    {
        // disable animations
        StopAnimatingImage();

        // restore the UseHardwardAcceleration value
        UseHardwareAcceleration = _useHardwareAccelerationBackup;

        Source = ImageSource.Null;
        _imageD2D?.Dispose();
        _imageD2D = null;
        _imageGdiPlus = null;

        if (bmp is null) return;

        // Check and preprocess image info
        CheckInputImage(bmp);

        // use Direct2D
        if (UseHardwareAcceleration && !CanImageAnimate && !HasAlphaPixels)
        {
            _imageD2D = Device.CreateBitmapFromGDIBitmap(bmp);
            Source = ImageSource.Direct2D;
        }
        // use GDI+
        else
        {
            _imageGdiPlus = bmp;
            Source = ImageSource.GDIPlus;
        }

        // emit OnImageChanged event
        OnImageChanged?.Invoke(EventArgs.Empty);

        if (CanImageAnimate && Source != ImageSource.Null)
        {
            UpdateZoomMode();
            StartAnimatingImage();
        }
        else
        {
            Refresh();
        }
    }


    private void OnImageFrameChanged(object? sender, EventArgs eventArgs)
    {
        Invalidate();
    }





    /// <summary>
    /// Start animating the image if it can animate, using GDI+.
    /// </summary>
    public void StartAnimatingImage()
    {
        if (IsImageAnimating || !CanImageAnimate || Source == ImageSource.Null)
            return;

        try
        {
            _imageAnimator.Animate(_imageGdiPlus, OnImageFrameChanged);
            IsImageAnimating = true;
        }
        catch (Exception) { }
    }


    /// <summary>
    /// Stop animating the image
    /// </summary>
    public void StopAnimatingImage()
    {
        if (Source != ImageSource.Null)
        {
            _imageAnimator.StopAnimate(_imageGdiPlus, OnImageFrameChanged);
        }

        IsImageAnimating = false;
    }


}
