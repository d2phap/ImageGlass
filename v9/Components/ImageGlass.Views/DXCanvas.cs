/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using D2Phap;
using DirectN;
using ImageGlass.Base;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using ImageGlass.Views.ImageAnimator;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Runtime.InteropServices;
using WicNet;
using InterpolationMode = D2Phap.InterpolationMode;

namespace ImageGlass.Views;

public class DXCanvas : DXControl
{

    // Private properties
    #region Private properties

    private IComObject<ID2D1Bitmap1>? _imageD2D = null;
    private Bitmap? _imageGdiPlus = null;
    private CancellationTokenSource? _msgTokenSrc;


    // to distinguish between clicks
    // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/input-mouse/how-to-distinguish-between-clicks-and-double-clicks?view=netdesktop-6.0
    private DateTime _lastClick = DateTime.UtcNow;
    private MouseEventArgs _lastClickArgs = new(MouseButtons.Left, 0, 0, 0, 0);
    private bool _isMouseDragged = false;
    private bool _isDoubleClick = false;
    private Rectangle _doubleClickArea = default;
    private readonly TimeSpan _doubleClickMaxTime = TimeSpan.FromMilliseconds(SystemInformation.DoubleClickTime);
    private readonly System.Windows.Forms.Timer _clickTimer = new()
    {
        Interval = SystemInformation.DoubleClickTime / 2,
    };

    private float _imageOpacity = 1f;
    private float _opacityStep = 0.05f;
    private ImageDrawingState _imageDrawingState = ImageDrawingState.NotStarted;
    private bool _isPreviewing = false;
    private bool _debugMode = false;


    private RectangleF _srcRect = default; // image source rectangle
    private RectangleF _destRect = default; // image destination rectangle

    private Vector2 _panHostFromPoint;
    private Vector2 _panHostToPoint;
    private float _panDistance = 20f;

    private bool _xOut = false;
    private bool _yOut = false;
    private MouseButtons _mouseDownButton = MouseButtons.None;
    private Point? _mouseDownPoint = null;
    private Point? _mouseMovePoint = null;
    private Vector2 _zoommedPoint = default;

    // current zoom, minimum zoom, maximum zoom, previous zoom (bigger means zoom in)
    private float _zoomFactor = 1f;
    private float _oldZoomFactor = 1f;
    private bool _isManualZoom = false;
    private ZoomMode _zoomMode = ZoomMode.AutoZoom;
    private float _zoomSpeed = 0f;
    private ImageInterpolation _interpolationScaleDown = ImageInterpolation.SampleLinear;
    private ImageInterpolation _interpolationScaledUp = ImageInterpolation.NearestNeighbor;

    // checkerboard
    private CheckerboardMode _checkerboardMode = CheckerboardMode.None;
    private float _checkerboardCellSize = 10f;
    private Color _checkerboardColor1 = Color.Black.WithAlpha(25);
    private Color _checkerboardColor2 = Color.White.WithAlpha(25);
    private TextureBrush? _checkerboardBrushGdip;
    private ComObject<ID2D1BitmapBrush1>? _checkerboardBrushD2D;

    private IImageAnimator _imageAnimator;
    private AnimationSource _animationSource = AnimationSource.None;
    private bool _shouldRecalculateDrawingRegion = true;

    // Navigation buttons
    internal float NAV_PADDING => 20f;
    private bool _isNavLeftHovered = false;
    private bool _isNavLeftPressed = false;
    private bool _isNavRightHovered = false;
    private bool _isNavRightPressed = false;
    internal PointF NavLeftPos => new(
        DrawingArea.Left + NavButtonSize.Width / 2 + NAV_PADDING,
        DrawingArea.Top + DrawingArea.Height / 2);
    internal PointF NavRightPos => new(
        DrawingArea.Right - NavButtonSize.Width / 2 - NAV_PADDING,
        DrawingArea.Top + DrawingArea.Height / 2);
    private NavButtonDisplay _navDisplay = NavButtonDisplay.None;
    private bool _isNavVisible = false;
    private float NavBorderRadius => NavButtonSize.Width / 2;
    private IComObject<ID2D1Bitmap1>? _navLeftImage = null;
    private IComObject<ID2D1Bitmap1>? _navRightImage = null;
    private Bitmap? _navLeftImageGdip = null;
    private Bitmap? _navRightImageGdip = null;

    // selection
    private bool _enableSelection = false;
    private RectangleF _clientSelection = default;
    private RectangleF _selectionBeforeMove = default;
    private bool _canDrawSelection = false;
    private bool _isSelectionHovered = false;
    private SelectionResizer? _hoveredResizer = null;
    private SelectionResizer? _selectedResizer = null;

    #endregion // Private properties


    // Public properties
    #region Public properties


    // Viewport
    #region Viewport

    /// <summary>
    /// Gets, sets the left padding.
    /// </summary>
    [Browsable(false)]
    public int PaddingLeft
    {
        get => Padding.Left;
        set
        {
            Padding = new Padding(value, Padding.Top, Padding.Right, Padding.Bottom);
            _shouldRecalculateDrawingRegion = true;
            Refresh(!_isManualZoom);
        }
    }

    /// <summary>
    /// Gets, sets the top padding.
    /// </summary>
    [Browsable(false)]
    public int PaddingTop
    {
        get => Padding.Top;
        set
        {
            Padding = new Padding(Padding.Left, value, Padding.Right, Padding.Bottom);
            _shouldRecalculateDrawingRegion = true;
            Refresh(!_isManualZoom);
        }
    }

    /// <summary>
    /// Gets, sets the right padding.
    /// </summary>
    [Browsable(false)]
    public int PaddingRight
    {
        get => Padding.Right;
        set
        {
            Padding = new Padding(Padding.Left, Padding.Top, value, Padding.Bottom);
            _shouldRecalculateDrawingRegion = true;
            Refresh(!_isManualZoom);
        }
    }

    /// <summary>
    /// Gets, sets the bottom padding.
    /// </summary>
    [Browsable(false)]
    public int PaddingBottom
    {
        get => Padding.Bottom;
        set
        {
            Padding = new Padding(Padding.Left, Padding.Top, Padding.Right, value);
            _shouldRecalculateDrawingRegion = true;
            Refresh(!_isManualZoom);
        }
    }

    /// <summary>
    /// Gets the drawing area after deducting <see cref="Padding"/>.
    /// </summary>
    [Browsable(false)]
    public RectangleF DrawingArea => new RectangleF(
        Padding.Left,
        Padding.Top,
        Width - Padding.Horizontal,
        Height - Padding.Vertical);

    /// <summary>
    /// Gets rectangle of the viewport.
    /// </summary>
    [Browsable(false)]
    public RectangleF ImageDestBounds => _destRect;


    /// <summary>
    /// Gets the rectangle of the source image region being drawn.
    /// </summary>
    [Browsable(false)]
    public RectangleF ImageSourceBounds => _srcRect;


    /// <summary>
    /// Gets the center point of image viewport.
    /// </summary>
    [Browsable(false)]
    public PointF ImageViewportCenterPoint => new()
    {
        X = ImageDestBounds.X + ImageDestBounds.Width / 2,
        Y = ImageDestBounds.Y + ImageDestBounds.Height / 2,
    };


    /// <summary>
    /// Checks if the viewing image size if smaller than the viewport size.
    /// </summary>
    [Browsable(false)]
    public bool IsViewingSizeSmallerViewportSize
    {
        get
        {
            if (_destRect.X > DrawingArea.Left && _destRect.Y > DrawingArea.Top) return true;


            if (SourceWidth > SourceHeight)
            {
                return SourceWidth * ZoomFactor <= Width;
            }
            else
            {
                return SourceHeight * ZoomFactor <= Height;
            }
        }
    }


    /// <summary>
    /// Gets the drawing state of the image source
    /// </summary>
    [Browsable(false)]
    public ImageDrawingState ImageDrawingState => _imageDrawingState;


    /// <summary>
    /// Gets, sets accent color.
    /// </summary>
    public Color AccentColor { get; set; } = Color.Blue;


    /// <summary>
    /// Gets, sets the client selection area. This will emit the event <see cref="SelectionChanged"/>.
    /// </summary>
    [Browsable(false)]
    public RectangleF ClientSelection
    {
        get
        {
            // limit the selected area to the image
            _clientSelection.Intersect(_destRect);

            return _clientSelection;
        }
        set
        {
            value.Intersect(_destRect);
            _clientSelection = value;

            SelectionChanged?.Invoke(this, new SelectionEventArgs(_clientSelection, SourceSelection));
        }
    }


    /// <summary>
    /// Gets, sets the source selection area.
    /// </summary>
    [Browsable(false)]
    public RectangleF SourceSelection
    {
        get
        {
            var p1 = this.PointClientToSource(ClientSelection.Location);
            var p2 = this.PointClientToSource(new PointF(ClientSelection.Right, ClientSelection.Bottom));


            // get the min int value
            var floorP1 = new PointF(
                (float)Math.Floor(Math.Round(p1.X, 1)),
                (float)Math.Floor(Math.Round(p1.Y, 1)));
            if (floorP1.X < 0) floorP1.X = 0;
            if (floorP1.Y < 0) floorP1.Y = 0;
            if (floorP1.X > SourceWidth) floorP1.X = SourceWidth;
            if (floorP1.Y > SourceHeight) floorP1.Y = SourceHeight;

            if (p1 == p2)
            {
                return new RectangleF(floorP1, new SizeF(0, 0));
            }


            // get the max int value
            var ceilP2 = new PointF(
                (float)Math.Ceiling(Math.Round(p2.X, 1)),
                (float)Math.Ceiling(Math.Round(p2.Y, 1)));
            if (ceilP2.X < 0) ceilP2.X = 0;
            if (ceilP2.Y < 0) ceilP2.Y = 0;
            if (ceilP2.X > SourceWidth) ceilP2.X = SourceWidth;
            if (ceilP2.Y > SourceHeight) ceilP2.Y = SourceHeight;


            // the selection area is where the p1 and p2 intersected.
            return new RectangleF(
                floorP1,
                new SizeF(ceilP2.X - floorP1.X, ceilP2.Y - floorP1.Y));
        }
        set
        {
            var loc = this.PointSourceToClient(value.Location);
            var size = new SizeF(value.Width * ZoomFactor, value.Height * ZoomFactor);

            var newClientSelection = new RectangleF(loc, size);
            newClientSelection.Intersect(_destRect);
            _clientSelection = newClientSelection;
        }
    }


    /// <summary>
    /// Gets the resizers of the selection rectangle
    /// </summary>
    [Browsable(false)]
    public List<SelectionResizer> SelectionResizers
    {
        get
        {
            if (ClientSelection.IsEmpty) return new List<SelectionResizer>();


            var resizerSize = DpiApi.Transform(Font.Size * 1.3f);
            var resizerMargin = DpiApi.Transform(2);
            var hitSize = DpiApi.Transform(resizerSize * 1.3f);

            // top left
            var topLeft = new RectangleF(
                ClientSelection.X + resizerMargin,
                ClientSelection.Y + resizerMargin,
                resizerSize, resizerSize);
            var topLeftHit = new RectangleF(
                ClientSelection.X - hitSize / 2 + resizerSize / 2,
                ClientSelection.Y - hitSize / 2 + resizerSize / 2,
                hitSize, hitSize);

            // top right
            var topRight = new RectangleF(
                ClientSelection.Right - resizerSize - resizerMargin,
                ClientSelection.Y + resizerMargin,
                resizerSize, resizerSize);
            var topRightHit = new RectangleF(
                ClientSelection.Right - hitSize / 2 - resizerSize / 2,
                ClientSelection.Y - hitSize / 2 + resizerSize / 2,
                hitSize, hitSize);

            // bottom left
            var bottomLeft = new RectangleF(
                ClientSelection.X + resizerMargin,
                ClientSelection.Bottom - resizerSize - resizerMargin,
                resizerSize, resizerSize);
            var bottomLeftHit = new RectangleF(
                ClientSelection.X - hitSize / 2 + resizerSize / 2,
                ClientSelection.Bottom - hitSize / 2 - resizerSize / 2,
                hitSize, hitSize);

            // bottom right
            var bottomRight = new RectangleF(
                ClientSelection.Right - resizerSize - resizerMargin,
                ClientSelection.Bottom - resizerSize - resizerMargin,
                resizerSize, resizerSize);
            var bottomRightHit = new RectangleF(
                ClientSelection.Right - hitSize / 2 - resizerSize / 2,
                ClientSelection.Bottom - hitSize / 2 - resizerSize / 2,
                hitSize, hitSize);

            // top
            var top = new RectangleF(
                ClientSelection.X + ClientSelection.Width / 2 - resizerSize / 2,
                ClientSelection.Y + resizerMargin,
                resizerSize, resizerSize);
            var topHit = new RectangleF(
                ClientSelection.X + ClientSelection.Width / 2 - hitSize / 2,
                ClientSelection.Y - hitSize / 2 + resizerSize / 2,
                hitSize, hitSize);

            // right
            var right = new RectangleF(
                    ClientSelection.Right - resizerSize - resizerMargin,
                    ClientSelection.Y + ClientSelection.Height / 2 - resizerSize / 2,
                    resizerSize, resizerSize);
            var rightHit = new RectangleF(
                    ClientSelection.Right - hitSize / 2 - resizerSize / 2,
                    ClientSelection.Y + ClientSelection.Height / 2 - hitSize / 2,
                    hitSize, hitSize);

            // bottom
            var bottom = new RectangleF(
                    ClientSelection.X + ClientSelection.Width / 2 - resizerSize / 2,
                    ClientSelection.Bottom - resizerSize - resizerMargin,
                    resizerSize, resizerSize);
            var bottomHit = new RectangleF(
                    ClientSelection.X + ClientSelection.Width / 2 - hitSize / 2,
                    ClientSelection.Bottom - hitSize / 2 - resizerSize / 2,
                    hitSize, hitSize);

            // left
            var left = new RectangleF(
                    ClientSelection.X + resizerMargin,
                    ClientSelection.Y + ClientSelection.Height / 2 - resizerSize / 2,
                    resizerSize, resizerSize);
            var leftHit = new RectangleF(
                    ClientSelection.X - hitSize / 2 + resizerSize / 2,
                    ClientSelection.Y + ClientSelection.Height / 2 - hitSize / 2,
                    hitSize, hitSize);

            // 8 resizers
            return new List<SelectionResizer>(8)
            {
                new(SelectionResizerType.TopLeft, topLeft, topLeftHit),
                new(SelectionResizerType.BottomLeft, bottomLeft, bottomLeftHit),
                new(SelectionResizerType.TopRight, topRight, topRightHit),
                new(SelectionResizerType.BottomRight, bottomRight, bottomRightHit),

                new(SelectionResizerType.Left, left, leftHit),
                new(SelectionResizerType.Top, top, topHit),
                new(SelectionResizerType.Right, right, rightHit),
                new(SelectionResizerType.Bottom, bottom, bottomHit),
            };
        }
    }


    /// <summary>
    /// Gets, sets selection aspect ratio.
    /// If Width or Height is <c>less than or equals 0</c>, we will use free aspect ratio.
    /// </summary>
    public SizeF SelectionAspectRatio { get; set; } = new();


    /// <summary>
    /// Enables or disables the selection.
    /// </summary>
    public bool EnableSelection
    {
        get => _enableSelection;
        set
        {
            _enableSelection = value;
            if (!_enableSelection && Parent != null)
            {
                Cursor = Parent.Cursor;
            }
        }
    }


    /// <summary>
    /// Gets the current action of selection.
    /// </summary>
    public SelectionAction CurrentSelectionAction { get; private set; } = SelectionAction.None;


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
    [Browsable(false)]
    public ImageSource Source { get; private set; } = ImageSource.Null;

    /// <summary>
    /// Gets the input image's width.
    /// </summary>
    [Browsable(false)]
    public float SourceWidth { get; private set; } = 0;

    /// <summary>
    /// Gets the input image's height.
    /// </summary>
    [Browsable(false)]
    public float SourceHeight { get; private set; } = 0;


    #endregion


    // Zooming
    #region Zooming

    /// <summary>
    /// Gets, sets the minimum zoom factor (<c>1.0f = 100%</c>).
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(0.01f)]
    public float MinZoom { get; set; } = 0.01f;

    /// <summary>
    /// Gets, sets the maximum zoom factor (<c>1.0f = 100%</c>).
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(35.0f)]
    public float MaxZoom { get; set; } = 35f;

    /// <summary>
    /// Gets, sets current zoom factor (<c>1.0f = 100%</c>).
    /// This also sets <see cref="_isManualZoom"/> to <c>true</c>.
    /// 
    /// <para>
    /// Use <see cref="SetZoomFactor(float, bool)"/> for more options.
    /// </para>
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(1.0f)]
    public float ZoomFactor
    {
        get => _zoomFactor;
        set => SetZoomFactor(value, true);
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
            _zoomSpeed = Math.Min(value, 500f); // max 500f
            _zoomSpeed = Math.Max(value, -500f); // min -500f
        }
    }

    /// <summary>
    /// Gets, sets zoom mode.
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
    /// Gets, sets interpolation mode used when the
    /// <see cref="ZoomFactor"/> is less than or equal <c>1.0f</c>.
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(ImageInterpolation.Linear)]
    public ImageInterpolation InterpolationScaleDown
    {
        get => _interpolationScaleDown;
        set
        {
            if (_interpolationScaleDown != value)
            {
                _interpolationScaleDown = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets, sets interpolation mode used when the
    /// <see cref="ZoomFactor"/> is greater than <c>1.0f</c>.
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(ImageInterpolation.NearestNeighbor)]
    public ImageInterpolation InterpolationScaleUp
    {
        get => _interpolationScaledUp;
        set
        {
            if (_interpolationScaledUp != value)
            {
                _interpolationScaledUp = value;
                Invalidate();
            }
        }
    }


    /// <summary>
    /// Gets the current <see cref="ImageInterpolation"/> mode.
    /// </summary>
    [Browsable(false)]
    public ImageInterpolation CurrentInterpolation => ZoomFactor > 1f ? _interpolationScaledUp : _interpolationScaleDown;


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

                // reset checkerboard brush
                DisposeCheckerboardBrushes();

                Invalidate();
            }
        }
    }


    [Category("Checkerboard")]
    [DefaultValue(typeof(float), "10")]
    public float CheckerboardCellSize
    {
        get => _checkerboardCellSize;
        set
        {
            if (_checkerboardCellSize != value)
            {
                _checkerboardCellSize = value;

                // reset checkerboard brush
                DisposeCheckerboardBrushes();

                Invalidate();
            }
        }
    }


    [Category("Checkerboard")]
    [DefaultValue(typeof(Color), "25, 0, 0, 0")]
    public Color CheckerboardColor1
    {
        get => _checkerboardColor1;
        set
        {
            if (_checkerboardColor1 != value)
            {
                _checkerboardColor1 = value;

                // reset checkerboard brush
                DisposeCheckerboardBrushes();

                Invalidate();
            }
        }
    }


    [Category("Checkerboard")]
    [DefaultValue(typeof(Color), "25, 255, 255, 255")]
    public Color CheckerboardColor2
    {
        get => _checkerboardColor2;
        set
        {
            if (_checkerboardColor2 != value)
            {
                _checkerboardColor2 = value;

                // reset checkerboard brush
                DisposeCheckerboardBrushes();

                Invalidate();
            }
        }
    }

    #endregion


    // Panning
    #region Panning

    /// <summary>
    /// Gets, sets the panning distance. Min value is <c>0</c>.
    /// </summary>
    [Category("Panning")]
    [DefaultValue(20f)]
    public float PanDistance
    {
        get => _panDistance;
        set
        {
            _panDistance = Math.Max(value, 0); // min 0
        }
    }

    /// <summary>
    /// Checks if the current viewing image supports horizontal panning.
    /// </summary>
    [Browsable(false)]
    public bool CanPanHorizontal => Width < SourceWidth * ZoomFactor;

    /// <summary>
    /// Checks if the current viewing image supports vertical panning.
    /// </summary>
    [Browsable(false)]
    public bool CanPanVertical => Height < SourceHeight * ZoomFactor;
    #endregion


    // Navigation
    #region Navigation

    /// <summary>
    /// Gets, sets the navigation buttons display style.
    /// </summary>
    [Category("Navigation")]
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

    /// <summary>
    /// Gets, sets the navigation button size.
    /// </summary>
    [Browsable(false)]
    public SizeF NavButtonSize { get; set; } = new(60f, 60f);

    /// <summary>
    /// Gets, sets the navigation button color when hovered.
    /// </summary>
    [Category("Navigation")]
    [DefaultValue(typeof(Color), "150, 0, 0, 0")]
    public Color NavHoveredColor { get; set; } = Color.FromArgb(150, Color.Black);

    /// <summary>
    /// Gets, sets the navigation button color when pressed.
    /// </summary>
    [Category("Navigation")]
    [DefaultValue(typeof(Color), "200, 0, 0, 0")]
    public Color NavPressedColor { get; set; } = Color.FromArgb(200, Color.Black);

    /// <summary>
    /// Gets, sets the left navigation button icon image.
    /// </summary>
    [Category("Navigation")]
    [DefaultValue(typeof(Bitmap), null)]
    public WicBitmapSource? NavLeftImage
    {
        set
        {
            DXHelper.DisposeD2D1Bitmap(ref _navLeftImage);
            _navLeftImageGdip?.Dispose();
            _navLeftImageGdip = null;

            _navLeftImage = DXHelper.ToD2D1Bitmap(Device, value);
            _navLeftImageGdip = BHelper.ToGdiPlusBitmap(value);
        }
    }


    /// <summary>
    /// Gets, sets the right navigation button icon image.
    /// </summary>
    [Category("Navigation")]
    [DefaultValue(typeof(Bitmap), null)]
    public WicBitmapSource? NavRightImage
    {
        set
        {
            DXHelper.DisposeD2D1Bitmap(ref _navRightImage);
            _navRightImageGdip?.Dispose();
            _navRightImageGdip = null;

            _navRightImage = DXHelper.ToD2D1Bitmap(Device, value);
            _navRightImageGdip = BHelper.ToGdiPlusBitmap(value);
        }
    }

    #endregion


    // Misc
    #region Misc

    /// <summary>
    /// Gets, sets the message heading text
    /// </summary>
    [Category("Misc")]
    [DefaultValue("")]
    public string TextHeading { get; set; } = string.Empty;


    /// <summary>
    /// Gets, sets border radius of message box
    /// </summary>
    [Category("Misc")]
    [DefaultValue(1f)]
    public float MessageBorderRadius { get; set; } = 6f;


    /// <summary>
    /// Gets the current animating source
    /// </summary>
    [Browsable(false)]
    public AnimationSource AnimationSource => _animationSource;


    /// <summary>
    /// Gets, sets debug mode.
    /// </summary>
    [Browsable(false)]
    public bool DebugMode
    {
        get => _debugMode;
        set
        {
            _debugMode = value;
            CheckFPS = value;
        }
    }

    #endregion


    // Events
    #region Events

    /// <summary>
    /// Occurs when <see cref="ZoomFactor"/> value changes.
    /// </summary>
    public event EventHandler<ZoomEventArgs>? OnZoomChanged = null;

    /// <summary>
    /// Occurs when the host is being panned.
    /// </summary>
    public event EventHandler<PanningEventArgs>? Panning;


    /// <summary>
    /// Occurs when the image is being loaded.
    /// </summary>
    public event EventHandler? ImageLoading;


    /// <summary>
    /// Occurs when the image is drawn adn its animation, preview finished.
    /// </summary>
    public event EventHandler? ImageLoaded;


    /// <summary>
    /// Occurs when the image is drawn to the canvas.
    /// </summary>
    public event EventHandler? ImageDrawn;


    /// <summary>
    /// Occurs when the mouse pointer is moved over the control.
    /// </summary>
    public event EventHandler<ImageMouseEventArgs>? ImageMouseMove;


    /// <summary>
    /// Occurs when the control is clicked by the mouse.
    /// </summary>
    public event EventHandler<ImageMouseEventArgs>? ImageMouseClick;


    /// <summary>
    /// Occurs when the <see cref="ClientSelection"/> is changed.
    /// </summary>
    public event EventHandler<SelectionEventArgs>? SelectionChanged;


    /// <summary>
    /// Occurs when the left navigation button clicked.
    /// </summary>
    public event EventHandler<MouseEventArgs>? OnNavLeftClicked;


    /// <summary>
    /// Occurs when the right navigation button clicked.
    /// </summary>
    public event EventHandler<MouseEventArgs>? OnNavRightClicked;


    #endregion


    #endregion // Public properties



    public DXCanvas()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);

        // request for high resolution gif animation
        if (!TimerApi.HasRequestedRateAtLeastAsFastAs(10) && TimerApi.TimeBeginPeriod(10))
        {
            HighResolutionGifAnimator.SetTickTimeInMilliseconds(10);
        }
        _imageAnimator = new HighResolutionGifAnimator();

        _clickTimer.Tick += ClickTimer_Tick;
    }

    private void ClickTimer_Tick(object? sender, EventArgs e)
    {
        // Clear double click watcher and timer
        _isDoubleClick = false;
        _clickTimer.Stop();

        if (this.CheckWhichNav(_lastClickArgs.Location) == MouseAndNavLocation.Outside
            && !_isMouseDragged)
        {
            base.OnMouseClick(_lastClickArgs);
        }

        _isMouseDragged = false;
    }


    // Override / Virtual methods
    #region Protected / Virtual methods

    protected override void OnLoaded()
    {
        base.OnLoaded();

        // draw the control
        Refresh();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        DisposeImageResources();

        DXHelper.DisposeD2D1Bitmap(ref _navLeftImage);
        _navLeftImageGdip?.Dispose();
        _navLeftImageGdip = null;

        DXHelper.DisposeD2D1Bitmap(ref _navRightImage);
        _navRightImageGdip?.Dispose();
        _navRightImageGdip = null;

        DisposeCheckerboardBrushes();

        _clickTimer.Dispose();
        _msgTokenSrc?.Dispose();
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        // disable the default OnMouseClick
        //base.OnMouseClick(e);
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        // disable the default OnMouseDoubleClick
        //base.OnMouseDoubleClick(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (!IsReady) return;

        _mouseDownButton = e.Button;
        _isMouseDragged = false;
        _mouseDownPoint = e.Location;

        var canSelect = EnableSelection && _mouseDownButton == MouseButtons.Left;
        var requestRerender = false;


        // Navigation clickable check
        #region Navigation clickable check
        if (e.Button == MouseButtons.Left)
        {
            // calculate whether the point inside the left nav
            if (this.CheckWhichNav(e.Location, NavCheck.LeftOnly) == MouseAndNavLocation.LeftNav)
            {
                _isNavLeftPressed = true;
            }

            // calculate whether the point inside the right nav
            if (this.CheckWhichNav(e.Location, NavCheck.RightOnly) == MouseAndNavLocation.RightNav)
            {
                _isNavRightPressed = true;
            }

            requestRerender = _isNavLeftPressed || _isNavRightPressed;
        }
        #endregion


        // Image panning & Selecting check
        #region Image panning & selecting check
        if (Source != ImageSource.Null)
        {
            requestRerender = requestRerender || (canSelect && !ClientSelection.IsEmpty);
            _selectedResizer = SelectionResizers.Find(i => i.HitRegion.Contains(e.Location));
            _canDrawSelection = canSelect && !_isSelectionHovered && _hoveredResizer == null;

            if (canSelect)
            {
                _selectionBeforeMove = new RectangleF(_clientSelection.Location, _clientSelection.Size);

                // resize selection
                if (canSelect && _hoveredResizer != null)
                {
                    CurrentSelectionAction = SelectionAction.Resizing;
                }

                // draw selection
                else if (canSelect && _isSelectionHovered)
                {
                    CurrentSelectionAction = SelectionAction.Drawing;
                }
            }


            // update selection
            if (_canDrawSelection)
            {
                _clientSelection = new(_mouseDownPoint.Value, new SizeF());
            }

            // panning
            else
            {

                _panHostToPoint.X = e.Location.X;
                _panHostToPoint.Y = e.Location.Y;
                _panHostFromPoint.X = e.Location.X;
                _panHostFromPoint.Y = e.Location.Y;
            }
        }
        #endregion


        if (requestRerender)
        {
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!IsReady) return;


        // Distinguish between clicks
        #region Distinguish between clicks
        if (_mouseDownButton != MouseButtons.None)
        {
            if (_isDoubleClick)
            {
                _isDoubleClick = false;

                var length = DateTime.UtcNow - _lastClick;

                // If double click is valid, respond
                if (_doubleClickArea.Contains(e.Location) && length < _doubleClickMaxTime)
                {
                    _clickTimer.Stop();
                    if (this.CheckWhichNav(e.Location) == MouseAndNavLocation.Outside)
                    {
                        base.OnMouseDoubleClick(e);
                    }
                }
            }
            else
            {
                // Double click was invalid, restart 
                _clickTimer.Stop();
                _clickTimer.Start();
                _lastClick = DateTime.UtcNow;
                _isDoubleClick = true;
                _doubleClickArea = new(e.Location - (SystemInformation.DoubleClickSize / 2), SystemInformation.DoubleClickSize);
            }


            // emit event ImageMouseClick
            var imgX = (e.X - _destRect.X) / _zoomFactor + _srcRect.X;
            var imgY = (e.Y - _destRect.Y) / _zoomFactor + _srcRect.Y;
            ImageMouseClick?.Invoke(this, new(imgX, imgY, e.Button));
        }
        #endregion


        // Navigation clickable check
        #region Navigation clickable check
        if (e.Button == MouseButtons.Left)
        {
            if (_isNavRightPressed)
            {
                // emit nav button event if the point inside the right nav
                if (this.CheckWhichNav(e.Location, NavCheck.RightOnly) == MouseAndNavLocation.RightNav)
                {
                    OnNavRightClicked?.Invoke(this, e);
                }
            }
            else if (_isNavLeftPressed)
            {
                // emit nav button event if the point inside the left nav
                if (this.CheckWhichNav(e.Location, NavCheck.LeftOnly) == MouseAndNavLocation.LeftNav)
                {
                    OnNavLeftClicked?.Invoke(this, e);
                }
            }
        }

        _isNavLeftPressed = false;
        _isNavRightPressed = false;
        #endregion


        var mouseDownButton = _mouseDownButton;
        _mouseDownButton = MouseButtons.None;
        _mouseDownPoint = null;
        _selectedResizer = null;
        _lastClickArgs = e;

        var canSelect = EnableSelection && mouseDownButton == MouseButtons.Left;
        if (canSelect)
        {
            SelectionChanged?.Invoke(this, new SelectionEventArgs(ClientSelection, SourceSelection));
            Invalidate();
        }
    }


    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!IsReady) return;

        var canSelect = EnableSelection && _mouseDownButton == MouseButtons.Left;
        var requestRerender = false;
        _mouseMovePoint = e.Location;
        CurrentSelectionAction = SelectionAction.None;


        // Navigation hoverable check
        #region Navigation hoverable check
        // no button pressed
        if (e.Button == MouseButtons.None)
        {
            // calculate whether the point inside the left nav
            _isNavLeftHovered = this.CheckWhichNav(e.Location, NavCheck.LeftOnly) == MouseAndNavLocation.LeftNav;

            // calculate whether the point inside the right nav
            _isNavRightHovered = this.CheckWhichNav(e.Location, NavCheck.RightOnly) == MouseAndNavLocation.RightNav;

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
        if (_mouseDownButton == MouseButtons.Left)
        {
            _isMouseDragged = true;


            // resize the selection
            if (_selectedResizer != null)
            {
                CurrentSelectionAction = SelectionAction.Resizing;
                ResizeSelection(e.Location, _selectedResizer.Type);
                requestRerender = true;
            }
            // draw new selection
            else if (_canDrawSelection)
            {
                CurrentSelectionAction = SelectionAction.Drawing;
                UpdateSelectionByMousePosition();

                SelectionChanged?.Invoke(this, new SelectionEventArgs(ClientSelection, SourceSelection));
                requestRerender = true;
            }
            // move selection
            else if (canSelect && IsViewingSizeSmallerViewportSize)
            {
                CurrentSelectionAction = SelectionAction.Moving;
                MoveSelection(e.Location);
                requestRerender = true;
            }
            // pan image
            else
            {
                requestRerender = PanTo(
                    _panHostToPoint.X - e.Location.X,
                    _panHostToPoint.Y - e.Location.Y,
                    false);
            }
        }


        // emit event ImageMouseMove
        var imgX = (e.X - _destRect.X) / _zoomFactor + _srcRect.X;
        var imgY = (e.Y - _destRect.Y) / _zoomFactor + _srcRect.Y;
        ImageMouseMove?.Invoke(this, new(imgX, imgY, e.Button));

        // change cursor
        if (EnableSelection)
        {
            // set resizer cursor
            var hoveredResizer = SelectionResizers.Find(i => i.HitRegion.Contains(e.Location));
            Cursor = hoveredResizer?.Cursor ?? Parent.Cursor;


            // redraw the canvas
            var isSelectionHovered = ClientSelection.Contains(e.Location);
            requestRerender = requestRerender
                || _isSelectionHovered != isSelectionHovered
                || _hoveredResizer != hoveredResizer;


            _isSelectionHovered = isSelectionHovered;
            _hoveredResizer = hoveredResizer;
        }

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
        _isSelectionHovered = false;
        _mouseMovePoint = null;


        if (_isNavVisible)
        {
            _isNavVisible = false;
            Invalidate();
        }
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

    protected override void OnFrame(FrameEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(OnFrame, e);
            return;
        }

        base.OnFrame(e);


        // Panning
        if (_animationSource.HasFlag(AnimationSource.PanLeft))
        {
            PanLeft(requestRerender: false);
        }
        else if (_animationSource.HasFlag(AnimationSource.PanRight))
        {
            PanRight(requestRerender: false);
        }

        if (_animationSource.HasFlag(AnimationSource.PanUp))
        {
            PanUp(requestRerender: false);
        }
        else if (_animationSource.HasFlag(AnimationSource.PanDown))
        {
            PanDown(requestRerender: false);
        }

        // Zooming
        if (_animationSource.HasFlag(AnimationSource.ZoomIn))
        {
            var point = PointToClient(Cursor.Position);
            _ = ZoomByDeltaToPoint(20, point, requestRerender: false);
        }
        else if (_animationSource.HasFlag(AnimationSource.ZoomOut))
        {
            var point = PointToClient(Cursor.Position);
            _ = ZoomByDeltaToPoint(-20, point, requestRerender: false);
        }


        if (_animationSource.HasFlag(AnimationSource.ImageFadeIn))
        {
            _imageOpacity += _opacityStep;

            if (_imageOpacity > 1)
            {
                StopAnimation(AnimationSource.ImageFadeIn);
                _imageOpacity = 1;
            }
        }
    }


    protected override void OnRender(IGraphics g)
    {
        // check if the image is already drawn
        var isImageDrawn = _imageDrawingState == ImageDrawingState.Drawing && !_isPreviewing;

        // check if this is the final draw since the image is set
        var isImageFinalDrawn = _imageOpacity == 1 && isImageDrawn;


        // update drawing regions
        CalculateDrawingRegion();

        // checkerboard background
        DrawCheckerboardLayer(g);


        // draw image layer
        if (CanImageAnimate)
        {
            DrawGifFrame(g);
        }
        else
        {
            DrawImageLayer(g);
        }

        // emits event ImageDrawn
        if (isImageDrawn)
        {
            ImageDrawn?.Invoke(this, EventArgs.Empty);
        }


        // Draw selection layer
        if (EnableSelection)
        {
            DrawSelectionLayer(g);
        }


        // text message
        DrawMessageLayer(g);

        // navigation layer
        DrawNavigationLayer(g);


        if (DebugMode)
        {
            var text = $"FPS: {FPS}";
            var textSize = g.MeasureText(text, Font.Name, Font.Size, textDpi: DeviceDpi);
            g.DrawRectangle(0, 0, textSize.Width, textSize.Height, 0, Color.Red, Color.Black.WithAlpha(200));
            g.DrawText(text, Font.Name, Font.Size, 0f, 0f, Color.Yellow, textDpi: DeviceDpi);
        }

        base.OnRender(g);


        // emits event ImageLoaded
        if (isImageFinalDrawn)
        {
            _imageDrawingState = ImageDrawingState.Done;
            ImageLoaded?.Invoke(this, EventArgs.Empty);
        }
    }


    /// <summary>
    /// Draw GIF frame using GDI+
    /// </summary>
    protected virtual void DrawGifFrame(IGraphics g)
    {
        if (Source == ImageSource.Null) return;

        // use GDI+ to handle GIF animation
        if (g is not GdipGraphics gdip) return;

        gdip.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
        gdip.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        try
        {
            if (IsImageAnimating && !DesignMode)
            {
                _imageAnimator.UpdateFrames(_imageGdiPlus);
            }

            g.DrawBitmap(_imageGdiPlus, _destRect, _srcRect, (InterpolationMode)CurrentInterpolation);
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
            // issue #373: a race condition caused this exception: deleting
            // the image from underneath us could cause a collision in
            // HighResolutionGif_animator. I've not been able to repro;
            // hopefully this is the correct response.

            // stop the animation and reset to the first frame.
            IsImageAnimating = false;
            _imageAnimator.StopAnimate(_imageGdiPlus, OnImageFrameChanged);
        }

        gdip.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
    }


    /// <summary>
    /// Calculates the drawing region
    /// </summary>
    protected virtual void CalculateDrawingRegion()
    {
        if (Source == ImageSource.Null || _shouldRecalculateDrawingRegion is false) return;

        var zoomX = _zoommedPoint.X;
        var zoomY = _zoommedPoint.Y;

        _xOut = false;
        _yOut = false;

        var controlW = DrawingArea.Width;
        var controlH = DrawingArea.Height;
        var scaledImgWidth = SourceWidth * _zoomFactor;
        var scaledImgHeight = SourceHeight * _zoomFactor;


        // image width < control width
        if (scaledImgWidth < controlW)
        {
            _srcRect.X = 0;
            _srcRect.Width = SourceWidth;

            _destRect.X = (controlW - scaledImgWidth) / 2.0f + DrawingArea.Left;
            _destRect.Width = scaledImgWidth;
        }
        else
        {
            _srcRect.X += (controlW / _oldZoomFactor - controlW / _zoomFactor) / ((controlW + float.Epsilon) / zoomX);
            _srcRect.Width = controlW / _zoomFactor;

            _destRect.X = DrawingArea.Left;
            _destRect.Width = controlW;
        }


        // image height < control height
        if (scaledImgHeight < controlH)
        {
            _srcRect.Y = 0;
            _srcRect.Height = SourceHeight;

            _destRect.Y = (controlH - scaledImgHeight) / 2f + DrawingArea.Top;
            _destRect.Height = scaledImgHeight;
        }
        else
        {
            _srcRect.Y += (controlH / _oldZoomFactor - controlH / _zoomFactor) / ((controlH + float.Epsilon) / zoomY);
            _srcRect.Height = controlH / _zoomFactor;

            _destRect.Y = DrawingArea.Top;
            _destRect.Height = controlH;
        }


        _oldZoomFactor = _zoomFactor;
        //------------------------

        if (_srcRect.X + _srcRect.Width > SourceWidth)
        {
            _xOut = true;
            _srcRect.X = SourceWidth - _srcRect.Width;
        }

        if (_srcRect.X < 0)
        {
            _xOut = true;
            _srcRect.X = 0;
        }

        if (_srcRect.Y + _srcRect.Height > SourceHeight)
        {
            _yOut = true;
            _srcRect.Y = SourceHeight - _srcRect.Height;
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
    protected virtual void DrawImageLayer(IGraphics g)
    {
        if (Source == ImageSource.Null) return;

        if (UseHardwareAcceleration)
        {
            g.DrawBitmap(_imageD2D, _destRect, _srcRect, (InterpolationMode)CurrentInterpolation, _imageOpacity);

        }
        else
        {
            g.DrawBitmap(_imageGdiPlus, _destRect, _srcRect, (InterpolationMode)CurrentInterpolation, _imageOpacity);
        }
    }


    /// <summary>
    /// Draw checkerboard background
    /// </summary>
    protected virtual void DrawCheckerboardLayer(IGraphics g)
    {
        if (CheckerboardMode == CheckerboardMode.None) return;

        // region to draw
        RectangleF region;

        if (CheckerboardMode == CheckerboardMode.Image)
        {
            // no need to draw checkerboard if image does not has alpha pixels
            if (!HasAlphaPixels) return;

            region = _destRect;
        }
        else
        {
            region = DrawingArea;
        }


        if (UseHardwareAcceleration)
        {
            // create bitmap brush
            _checkerboardBrushD2D ??= VHelper.CreateCheckerBoxTileD2D(Device, CheckerboardCellSize, CheckerboardColor1, CheckerboardColor2);

            // draw checkerboard
            Device.FillRectangle(DXHelper.ToD2DRectF(region), _checkerboardBrushD2D);
        }
        else
        {
            var gdiG = g as GdipGraphics;

            // create bitmap brush
            _checkerboardBrushGdip ??= VHelper.CreateCheckerBoxTileGdip(CheckerboardCellSize, CheckerboardColor1, CheckerboardColor2);

            // draw checkerboard
            gdiG?.Graphics.FillRectangle(_checkerboardBrushGdip, region);
        }
    }


    /// <summary>
    /// Draw selection layer
    /// </summary>
    protected virtual void DrawSelectionLayer(IGraphics g)
    {
        if (Source == ImageSource.Null || (_mouseDownButton != MouseButtons.Left && ClientSelection.IsEmpty))
            return;


        if (g is D2DGraphics dg)
        {
            // draw the clip selection region
            using var selectionGeo = dg.GetCombinedRectanglesGeometry(ClientSelection, _destRect, 0, 0, D2D1_COMBINE_MODE.D2D1_COMBINE_MODE_XOR);

            dg.DrawGeometry(selectionGeo, Color.Transparent, Color.Black.WithAlpha(_mouseDownButton == MouseButtons.Left ? 100 : 180));
        }


        // draw selection grid, resizers
        if (_mouseDownButton == MouseButtons.Left || _isSelectionHovered || _hoveredResizer != null)
        {
            var width3 = ClientSelection.Width / 3;
            var height3 = ClientSelection.Height / 3;


            // draw grid, ignore alpha value
            for (int i = 1; i < 3; i++)
            {
                g.DrawLine(
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y,
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y + ClientSelection.Height, Color.Black.WithAlpha(200),
                    0.4f);
                g.DrawLine(
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y,
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y + ClientSelection.Height, Color.White.WithAlpha(200),
                    0.4f);
                g.DrawLine(
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y,
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y + ClientSelection.Height, AccentColor.WithAlpha(200),
                    0.4f);


                g.DrawLine(
                    ClientSelection.X,
                    ClientSelection.Y + (i * height3),
                    ClientSelection.X + ClientSelection.Width,
                    ClientSelection.Y + (i * height3), Color.Black.WithAlpha(200),
                    0.4f);
                g.DrawLine(
                    ClientSelection.X,
                    ClientSelection.Y + (i * height3),
                    ClientSelection.X + ClientSelection.Width,
                    ClientSelection.Y + (i * height3), Color.White.WithAlpha(200),
                    0.4f);
                g.DrawLine(
                    ClientSelection.X,
                    ClientSelection.Y + (i * height3),
                    ClientSelection.X + ClientSelection.Width,
                    ClientSelection.Y + (i * height3), AccentColor.WithAlpha(200),
                    0.4f);
            }


            // draw selection size
            var text = $"{SourceSelection.Width} x {SourceSelection.Height}";
            var textSize = g.MeasureText(text, Font.Name, Font.Size, textDpi: DeviceDpi);
            var textPadding = new Padding(10, 5, 10, 5);
            var textX = ClientSelection.X + (ClientSelection.Width / 2 - textSize.Width / 2);
            var textY = ClientSelection.Y + (ClientSelection.Height / 2 - textSize.Height / 2);
            var textBgRect = new RectangleF(
                textX - textPadding.Left,
                textY - textPadding.Top,
                textSize.Width + textPadding.Horizontal,
                textSize.Height + textPadding.Vertical);

            if (textBgRect.Width + 10 < ClientSelection.Width
                && textBgRect.Height + 10 < ClientSelection.Height)
            {
                g.DrawRectangle(textBgRect, textSize.Height / 5, Color.White.WithAlpha(100), Color.Black.WithAlpha(100));
                g.DrawRectangle(textBgRect, textSize.Height / 5, AccentColor.WithAlpha(100), AccentColor.WithAlpha(150));
                g.DrawText(text, Font.Name, Font.Size, textX, textY, Color.White, textDpi: DeviceDpi);
            }


            // draw resizers
            foreach (var rItem in SelectionResizers)
            {
                var hideTopBottomResizers = ClientSelection.Width < rItem.IndicatorRegion.Width * 5;
                if (hideTopBottomResizers
                    && (rItem.Type == SelectionResizerType.Top
                    || rItem.Type == SelectionResizerType.Bottom)) continue;

                var hideLeftRightResizers = ClientSelection.Height < rItem.IndicatorRegion.Height * 5;
                if (hideLeftRightResizers
                    && (rItem.Type == SelectionResizerType.Left
                    || rItem.Type == SelectionResizerType.Right)) continue;

                // hover style
                var resizerRect = rItem.IndicatorRegion;
                var fillColor = Color.White.WithAlpha(200);
                if (rItem.Type == _hoveredResizer?.Type)
                {
                    resizerRect.Inflate(this.ScaleToDpi(new SizeF(1.45f, 1.45f)));
                    fillColor = AccentColor.WithAlpha(200);
                }

                g.DrawEllipse(resizerRect, Color.White.WithAlpha(50), Color.Black.WithAlpha(200), 8f);
                g.DrawEllipse(resizerRect, AccentColor.WithAlpha(255), fillColor, 2f);


                // draw debug Hit region
                if (DebugMode)
                {
                    g.DrawRectangle(rItem.HitRegion, 0, Color.Red);
                }
            }
        }


        // draw the selection border
        var borderWidth = (_isSelectionHovered && _mouseDownButton != MouseButtons.Left) ? 0.6f : 0.3f;
        g.DrawRectangle(ClientSelection, 0, Color.White, null, borderWidth);
        g.DrawRectangle(ClientSelection, 0, AccentColor, null, borderWidth);

    }


    /// <summary>
    /// Draws text message.
    /// </summary>
    protected virtual void DrawMessageLayer(IGraphics g)
    {
        var hasHeading = !string.IsNullOrEmpty(TextHeading);
        var hasText = !string.IsNullOrEmpty(Text);

        if (!hasHeading && !hasText) return;

        var textMargin = 20;
        var textPaddingX = Padding.Horizontal;
        var textPaddingY = Padding.Vertical;
        var gap = hasHeading && hasText
            ? textMargin
            : 0;

        var drawableArea = new RectangleF(
            textPaddingX / 2,
            textPaddingY / 2,
            Math.Max(0, Width - textPaddingX),
            Math.Max(0, Height - textPaddingY));

        var hTextSize = new SizeF();
        var tTextSize = new SizeF();

        var headingFontSize = Font.Size * 1.5f;
        var textFontSize = Font.Size * 1.1f;

        // heading size
        if (hasHeading)
        {
            hTextSize = g.MeasureText(TextHeading, Font.Name, headingFontSize, drawableArea.Width, drawableArea.Height, DeviceDpi);
        }

        // text size
        if (hasText)
        {
            tTextSize = g.MeasureText(Text, Font.Name, textFontSize, drawableArea.Width, drawableArea.Height, DeviceDpi);
        }

        var centerX = drawableArea.X + drawableArea.Width / 2;
        var centerY = drawableArea.Y + drawableArea.Height / 2;

        var hRegion = new RectangleF()
        {
            X = centerX - hTextSize.Width / 2,
            Y = centerY - ((hTextSize.Height + tTextSize.Height) / 2) - gap / 2,
            Width = hTextSize.Width + textPaddingX - drawableArea.X * 2 + 1,
            Height = hTextSize.Height + textPaddingY / 2 - drawableArea.Y,
        };

        var tRegion = new RectangleF()
        {
            X = centerX - tTextSize.Width / 2,
            Y = centerY - ((hTextSize.Height + tTextSize.Height) / 2) + hTextSize.Height + gap / 2,
            Width = tTextSize.Width + textPaddingX - drawableArea.X * 2 + 1,
            Height = tTextSize.Height + textPaddingY / 2 - drawableArea.Y,
        };

        var bgRegion = new RectangleF()
        {
            X = Math.Min(tRegion.X, hRegion.X) - textMargin / 2,
            Y = Math.Min(tRegion.Y, hRegion.Y) - textMargin / 2,
            Width = Math.Max(tRegion.Width, hRegion.Width) + textMargin,
            Height = tRegion.Height + hRegion.Height + textMargin + gap,
        };


        // draw background
        var bgColor = BackColor.WithAlpha(200);
        g.DrawRectangle(bgRegion, MessageBorderRadius, bgColor, bgColor);


        // debug
        if (DebugMode)
        {
            g.DrawRectangle(drawableArea, MessageBorderRadius, Color.Red);
            g.DrawRectangle(hRegion, MessageBorderRadius, Color.Orange);
            g.DrawRectangle(tRegion, MessageBorderRadius, Color.Green);
        }


        // draw text heading
        if (hasHeading)
        {
            var headingColor = AccentColor.Blend(Color.White, 0.7f);
            g.DrawText(TextHeading, Font.Name, headingFontSize, hRegion, headingColor, DeviceDpi, StringAlignment.Center);
        }

        // draw text
        if (hasText)
        {
            g.DrawText(Text, Font.Name, textFontSize, tRegion, ForeColor.WithAlpha(230), DeviceDpi, StringAlignment.Center);
        }
    }


    /// <summary>
    /// Draws navigation arrow buttons
    /// </summary>
    protected virtual void DrawNavigationLayer(IGraphics g)
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
                var leftBgRect = new RectangleF()
                {
                    X = NavLeftPos.X - NavButtonSize.Width / 2,
                    Y = NavLeftPos.Y - NavButtonSize.Height / 2,
                    Width = NavButtonSize.Width,
                    Height = NavButtonSize.Height,
                };

                g.DrawRectangle(leftBgRect, NavBorderRadius, leftColor.Blend(AccentColor, 0.5f, leftColor.A), leftColor.Blend(AccentColor, 0.7f, leftColor.A), 1.25f);
            }

            // draw icon
            if (_isNavLeftHovered || _isNavLeftPressed)
            {
                var iconSize = Math.Min(NavButtonSize.Width, NavButtonSize.Height) / 2;

                object? bmpObj;
                SizeF srcIconSize;
                if (UseHardwareAcceleration && _navLeftImage != null)
                {
                    bmpObj = _navLeftImage;
                    _navLeftImage.Object.GetSize(out var size);

                    srcIconSize = DXHelper.ToSize(size);
                }
                else if (_navLeftImageGdip != null)
                {
                    bmpObj = _navLeftImageGdip;
                    srcIconSize = _navLeftImageGdip.Size;
                }
                else
                {
                    return;
                }

                g.DrawBitmap(bmpObj, new RectangleF()
                {
                    X = NavLeftPos.X - iconSize / 2,
                    Y = NavLeftPos.Y - iconSize / 2 + iconY,
                    Width = iconSize,
                    Height = iconSize,
                }, new RectangleF(0, 0, srcIconSize.Width, srcIconSize.Height), InterpolationMode.Linear, iconOpacity);
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
                var rightBgRect = new RectangleF()
                {
                    X = NavRightPos.X - NavButtonSize.Width / 2,
                    Y = NavRightPos.Y - NavButtonSize.Height / 2,
                    Width = NavButtonSize.Width,
                    Height = NavButtonSize.Height,
                };

                g.DrawRectangle(rightBgRect, NavBorderRadius, rightColor.Blend(AccentColor, 0.5f, rightColor.A), rightColor.Blend(AccentColor, 0.7f, rightColor.A), 1.25f);
            }

            // draw icon
            if (_isNavRightHovered || _isNavRightPressed)
            {
                var iconSize = Math.Min(NavButtonSize.Width, NavButtonSize.Height) / 2;

                object? bmpObj;
                SizeF srcIconSize;
                if (UseHardwareAcceleration && _navRightImage != null)
                {
                    bmpObj = _navRightImage;
                    _navRightImage.Object.GetSize(out var size);

                    srcIconSize = DXHelper.ToSize(size);
                }
                else if (_navRightImageGdip != null)
                {
                    bmpObj = _navRightImageGdip;
                    srcIconSize = _navRightImageGdip.Size;
                }
                else
                {
                    return;
                }

                g.DrawBitmap(bmpObj, new RectangleF()
                {
                    X = NavRightPos.X - iconSize / 2,
                    Y = NavRightPos.Y - iconSize / 2 + iconY,
                    Width = iconSize,
                    Height = iconSize,
                }, new RectangleF(0, 0, srcIconSize.Width, srcIconSize.Height),
                    InterpolationMode.Linear, iconOpacity);
            }
        }
    }


    /// <summary>
    /// Updates zoom mode.
    /// </summary>
    protected virtual void UpdateZoomMode(ZoomMode? mode = null)
    {
        if (!IsReady || Source == ImageSource.Null) return;

        // get zoom factor after applying the zoom mode
        var zoomMode = mode ?? _zoomMode;
        _zoomFactor = CalculateZoomFactor(zoomMode, SourceWidth, SourceHeight, Width, Height);
        _isManualZoom = false;
        _shouldRecalculateDrawingRegion = true;

        OnZoomChanged?.Invoke(this, new(ZoomFactor, _isManualZoom, mode != _zoomMode));

        // emit selecting event
        if (EnableSelection && !ClientSelection.IsEmpty)
        {
            SelectionChanged?.Invoke(this, new SelectionEventArgs(ClientSelection, SourceSelection));
        }
    }

    #endregion // Override / Virtual methods


    // Public methods
    #region Public methods

    /// <summary>
    /// Sets zoom factor value.
    /// </summary>
    /// <param name="zoomValue">Zoom factor value</param>
    /// <param name="isManualZoom">Value for <see cref="_isManualZoom"/></param>
    public void SetZoomFactor(float zoomValue, bool isManualZoom)
    {
        if (_zoomFactor == zoomValue) return;

        _zoomFactor = Math.Min(MaxZoom, Math.Max(zoomValue, MinZoom));
        _isManualZoom = isManualZoom;
        _shouldRecalculateDrawingRegion = true;

        Invalidate();

        OnZoomChanged?.Invoke(this, new(_zoomFactor, _isManualZoom, false));

        // emit selecting event
        if (EnableSelection && !ClientSelection.IsEmpty)
        {
            SelectionChanged?.Invoke(this, new SelectionEventArgs(ClientSelection, SourceSelection));
        }
    }


    /// <summary>
    /// Calculates zoom factor by the input zoom mode, and source size.
    /// </summary>
    public float CalculateZoomFactor(ZoomMode zoomMode, float srcWidth, float srcHeight, int viewportW, int viewportH)
    {
        var horizontalPadding = Padding.Left + Padding.Right;
        var verticalPadding = Padding.Top + Padding.Bottom;
        var widthScale = (viewportW - horizontalPadding) / srcWidth;
        var heightScale = (viewportH - verticalPadding) / srcHeight;

        float zoomFactor;

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

        return zoomFactor;
    }


    /// <summary>
    /// Forces the control to reset zoom mode and invalidate itself.
    /// </summary>
    public new void Refresh()
    {
        Refresh(true);
    }


    /// <summary>
    /// Forces the control to invalidate itself.
    /// </summary>
    public void Refresh(bool resetZoom = true)
    {
        if (resetZoom)
        {
            UpdateZoomMode();
        }

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
        return ZoomByDeltaToPoint(SystemInformation.MouseWheelScrollDelta, point, requestRerender);
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
        return ZoomByDeltaToPoint(-SystemInformation.MouseWheelScrollDelta, point, requestRerender);
    }


    /// <summary>
    /// Scales the image using factor value.
    /// </summary>
    /// <param name="factor">Zoom factor (<c>1.0f = 100%</c>).</param>
    /// <param name="point">
    /// Client's cursor location to zoom out.
    /// If its value is <c>null</c> or outside of the <see cref="ViewBox"/> control,
    /// <c><see cref="ImageViewportCenterPoint"/></c> is used.
    /// </param>
    /// <returns>
    ///   <list type="table">
    ///     <item><c>true</c> if the viewport is changed.</item>
    ///     <item><c>false</c> if the viewport is unchanged.</item>
    ///   </list>
    /// </returns>
    public bool ZoomToPoint(float factor, PointF? point = null, bool requestRerender = true)
    {
        var location = point ?? new PointF(-1, -1);

        // use the center point if the point is outside
        if (!Bounds.Contains((int)location.X, (int)location.Y))
        {
            location = ImageViewportCenterPoint;
        }

        // get the gap when the viewport is smaller than the control size
        var gapX = Math.Max(ImageDestBounds.X, 0);
        var gapY = Math.Max(ImageDestBounds.Y, 0);

        // the location after zoomed
        var zoomedLocation = new PointF()
        {
            X = (location.X - gapX) * factor / ZoomFactor,
            Y = (location.Y - gapY) * factor / ZoomFactor,
        };

        // the distance of 2 points after zoomed
        var zoomedDistance = new SizeF()
        {
            Width = zoomedLocation.X - location.X,
            Height = zoomedLocation.Y - location.Y,
        };

        // perform zoom if the factor is different
        if (_zoomFactor != factor)
        {
            _zoomFactor = Math.Min(MaxZoom, Math.Max(factor, MinZoom));
            _shouldRecalculateDrawingRegion = true;
            _isManualZoom = true;

            PanTo(zoomedDistance.Width, zoomedDistance.Height, requestRerender);

            // emit OnZoomChanged event
            OnZoomChanged?.Invoke(this, new(_zoomFactor, _isManualZoom, false));


            // emit selecting event
            if (EnableSelection && !ClientSelection.IsEmpty)
            {
                SelectionChanged?.Invoke(this, new SelectionEventArgs(ClientSelection, SourceSelection));
            }

            return true;
        }

        return false;
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
    public bool ZoomByDeltaToPoint(float delta, PointF? point = null, bool requestRerender = true)
    {
        var speed = delta / (501f - ZoomSpeed);
        var location = point ?? new PointF(-1, -1);

        // use the center point if the point is outside
        if (!Bounds.Contains((int)location.X, (int)location.Y))
        {
            location = ImageViewportCenterPoint;
        }

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
        _zoommedPoint = location.ToVector2();

        if (requestRerender)
        {
            Invalidate();
        }

        // emit OnZoomChanged event
        OnZoomChanged?.Invoke(this, new(_zoomFactor, _isManualZoom, false));


        // emit selecting event
        if (EnableSelection && !ClientSelection.IsEmpty)
        {
            SelectionChanged?.Invoke(this, new SelectionEventArgs(ClientSelection, SourceSelection));
        }

        return true;
    }


    /// <summary>
    /// Pan the viewport left
    /// </summary>
    /// <param name="distance">Distance to pan</param>
    /// <param name="requestRerender"><c>true</c> to request the control invalidates.</param>
    public void PanLeft(float? distance = null, bool requestRerender = true)
    {
        distance ??= PanDistance;
        distance = Math.Max(distance.Value, 0); // min 0

        _ = PanTo(-distance.Value, 0, requestRerender);
    }


    /// <summary>
    /// Pan the viewport right
    /// </summary>
    /// <param name="distance">Distance to pan</param>
    /// <param name="requestRerender"><c>true</c> to request the control invalidates.</param>
    public void PanRight(float? distance = null, bool requestRerender = true)
    {
        distance ??= PanDistance;
        distance = Math.Max(distance.Value, 0); // min 0

        _ = PanTo(distance.Value, 0, requestRerender);
    }


    /// <summary>
    /// Pan the viewport up
    /// </summary>
    /// <param name="distance">Distance to pan</param>
    /// <param name="requestRerender"><c>true</c> to request the control invalidates.</param>
    public void PanUp(float? distance = null, bool requestRerender = true)
    {
        distance ??= PanDistance;
        distance = Math.Max(distance.Value, 0); // min 0

        _ = PanTo(0, -distance.Value, requestRerender);
    }


    /// <summary>
    /// Pan the viewport down
    /// </summary>
    /// <param name="distance">Distance to pan</param>
    /// <param name="requestRerender"><c>true</c> to request the control invalidates.</param>
    public void PanDown(float? distance = null, bool requestRerender = true)
    {
        distance ??= PanDistance;
        distance = Math.Max(distance.Value, 0); // min 0

        _ = PanTo(0, distance.Value, requestRerender);
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
        if (InvokeRequired)
        {
            return (bool)Invoke(PanTo, hDistance, vDistance, requestRerender);
        }

        if (Source == ImageSource.Null) return false;
        if (hDistance == 0 && vDistance == 0) return false;

        var loc = PointToClient(Cursor.Position);


        // horizontal
        if (hDistance != 0)
        {
            _srcRect.X += (hDistance / _zoomFactor);
        }

        // vertical 
        if (vDistance != 0)
        {
            _srcRect.Y += (vDistance / _zoomFactor);
        }

        _zoommedPoint = new();
        _shouldRecalculateDrawingRegion = true;


        if (_xOut == false)
        {
            _panHostToPoint.X = loc.X;
        }

        if (_yOut == false)
        {
            _panHostToPoint.Y = loc.Y;
        }

        _panHostToPoint.X = loc.X;
        _panHostToPoint.Y = loc.Y;


        // emit panning event
        Panning?.Invoke(this, new PanningEventArgs(loc, new PointF(_panHostFromPoint)));

        // emit selecting event
        if (EnableSelection && !ClientSelection.IsEmpty)
        {
            SelectionChanged?.Invoke(this, new SelectionEventArgs(ClientSelection, SourceSelection));
        }

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
    /// <param name="heading">Heading text</param>
    /// <param name="durationMs">Display duration in millisecond.
    /// Set it <b>0</b> to disable,
    /// or <b>-1</b> to display permanently.</param>
    /// <param name="delayMs">Duration to delay before displaying the message.</param>
    public void ShowMessage(string text, string heading = "", int durationMs = -1, int delayMs = 0, bool forceUpdate = true)
    {
        if (InvokeRequired)
        {
            Invoke(delegate
            {
                ShowMessage(text, heading, durationMs, delayMs, forceUpdate);
            });
            return;
        }

        _msgTokenSrc?.Cancel();
        _msgTokenSrc = new();

        ShowMessagePrivate(text, heading, durationMs, delayMs, forceUpdate);
    }


    /// <summary>
    /// Shows text message.
    /// </summary>
    /// <param name="text">Message to show</param>
    /// <param name="durationMs">Display duration in millisecond.
    /// Set it <b>0</b> to disable,
    /// or <b>-1</b> to display permanently.</param>
    /// <param name="delayMs">Duration to delay before displaying the message.</param>
    public void ShowMessage(string text, int durationMs = -1, int delayMs = 0, bool forceUpdate = true)
    {
        if (InvokeRequired)
        {
            Invoke(delegate
            {
                ShowMessage(text, durationMs, delayMs, forceUpdate);
            });
            return;
        }

        _msgTokenSrc?.Cancel();
        _msgTokenSrc = new();

        ShowMessagePrivate(text, string.Empty, durationMs, delayMs, forceUpdate);
    }


    /// <summary>
    /// Immediately clears text message.
    /// </summary>
    public void ClearMessage(bool forceUpdate = true)
    {
        if (InvokeRequired)
        {
            Invoke(ClearMessage, forceUpdate);
            return;
        }

        _msgTokenSrc?.Cancel();
        Text = string.Empty;
        TextHeading = string.Empty;

        if (forceUpdate)
        {
            Invalidate();
        }
    }


    /// <summary>
    /// Updates <see cref="ClientSelection"/> using <see cref="BHelper.GetSelection"/>.
    /// </summary>
    public void UpdateSelectionByMousePosition()
    {
        if (_mouseDownPoint == null || _mouseMovePoint == null) return;

        _clientSelection = BHelper.GetSelection(_mouseDownPoint, _mouseMovePoint, SelectionAspectRatio, SourceWidth, SourceHeight, _destRect);
    }


    /// <summary>
    /// Moves the current selection to the given location
    /// </summary>
    public void MoveSelection(PointF loc)
    {
        if (!EnableSelection) return;

        // translate mousedown point to selection start point
        var tX = (_mouseDownPoint?.X ?? 0) - _selectionBeforeMove.X;
        var tY = (_mouseDownPoint?.Y ?? 0) - _selectionBeforeMove.Y;

        // get the new selection start point
        var newX = loc.X - tX;
        var newY = loc.Y - tY;

        if (newX < _destRect.X) newX = _destRect.X;
        if (newY < _destRect.Y) newY = _destRect.Y;
        if (newX + _selectionBeforeMove.Width > _destRect.Right)
        {
            newX = _destRect.Right - _selectionBeforeMove.Width;
        }

        if (newY + _selectionBeforeMove.Height > _destRect.Bottom)
        {
            newY = _destRect.Bottom - _selectionBeforeMove.Height;
        }


        _clientSelection.X = newX;
        _clientSelection.Y = newY;
        _clientSelection.Width = _selectionBeforeMove.Width;
        _clientSelection.Height = _selectionBeforeMove.Height;


        SelectionChanged?.Invoke(this, new SelectionEventArgs(ClientSelection, SourceSelection));
    }


    /// <summary>
    /// Resizes the current selection
    /// </summary>
    public void ResizeSelection(PointF loc, SelectionResizerType direction)
    {
        if (!EnableSelection) return;

        if (direction == SelectionResizerType.Top
            || direction == SelectionResizerType.TopLeft
            || direction == SelectionResizerType.TopRight)
        {
            var gapY = _selectionBeforeMove.Y - (_mouseDownPoint?.Y ?? 0);
            var dH = loc.Y - _selectionBeforeMove.Y + gapY;

            _clientSelection.Y = _selectionBeforeMove.Y + dH;
            _clientSelection.Height = _selectionBeforeMove.Height - dH;
        }

        if (direction == SelectionResizerType.Right
            || direction == SelectionResizerType.TopRight
            || direction == SelectionResizerType.BottomRight)
        {
            var gapX = _selectionBeforeMove.Right - (_mouseDownPoint?.X ?? 0);
            var dW = loc.X - _selectionBeforeMove.Right + gapX;

            _clientSelection.Width = _selectionBeforeMove.Width + dW;
        }

        if (direction == SelectionResizerType.Bottom
            || direction == SelectionResizerType.BottomLeft
            || direction == SelectionResizerType.BottomRight)
        {
            var gapY = _selectionBeforeMove.Bottom - (_mouseDownPoint?.Y ?? 0);
            var dH = loc.Y - _selectionBeforeMove.Bottom + gapY;

            _clientSelection.Height = _selectionBeforeMove.Height + dH;
        }

        if (direction == SelectionResizerType.Left
            || direction == SelectionResizerType.TopLeft
            || direction == SelectionResizerType.BottomLeft)
        {
            var gapX = _selectionBeforeMove.X - (_mouseDownPoint?.X ?? 0);
            var dW = loc.X - _selectionBeforeMove.X + gapX;

            _clientSelection.X = _selectionBeforeMove.X + dW;
            _clientSelection.Width = _selectionBeforeMove.Width - dW;
        }

        // limit the selected area to the image
        _clientSelection.Intersect(_destRect);

        // not the free aspect ratio
        if (SelectionAspectRatio.Width > 0 && SelectionAspectRatio.Height > 0)
        {
            var wRatio = SelectionAspectRatio.Width / SelectionAspectRatio.Height;
            var hRatio = SelectionAspectRatio.Height / SelectionAspectRatio.Width;

            // update selection size according to the ratio
            if (wRatio > hRatio)
            {
                if (direction == SelectionResizerType.Top
                    || direction == SelectionResizerType.TopRight
                    || direction == SelectionResizerType.TopLeft
                    || direction == SelectionResizerType.Bottom
                    || direction == SelectionResizerType.BottomLeft
                    || direction == SelectionResizerType.BottomRight)
                {
                    _clientSelection.Width = _clientSelection.Height / hRatio;

                    if (_clientSelection.Right >= _destRect.Right)
                    {
                        var maxWidth = _destRect.Right - _clientSelection.X; ;
                        _clientSelection.Width = maxWidth;
                        _clientSelection.Height = maxWidth * hRatio;
                    }
                }
                else
                {
                    _clientSelection.Height = _clientSelection.Width / wRatio;
                }


                if (_clientSelection.Bottom >= _destRect.Bottom)
                {
                    var maxHeight = _destRect.Bottom - _clientSelection.Y;
                    _clientSelection.Width = maxHeight * wRatio;
                    _clientSelection.Height = maxHeight;
                }
            }
            else
            {
                if (direction == SelectionResizerType.Left
                    || direction == SelectionResizerType.TopLeft
                    || direction == SelectionResizerType.BottomLeft
                    || direction == SelectionResizerType.Right
                    || direction == SelectionResizerType.TopRight
                    || direction == SelectionResizerType.BottomRight)
                {
                    _clientSelection.Height = _clientSelection.Width / wRatio;

                    if (_clientSelection.Bottom >= _destRect.Bottom)
                    {
                        var maxHeight = _destRect.Bottom - _clientSelection.Y;
                        _clientSelection.Width = maxHeight * wRatio;
                        _clientSelection.Height = maxHeight;
                    }
                }
                else
                {
                    _clientSelection.Width = _clientSelection.Height / hRatio;
                }


                if (_clientSelection.Right >= _destRect.Right)
                {
                    var maxWidth = _destRect.Right - _clientSelection.X;
                    _clientSelection.Width = maxWidth;
                    _clientSelection.Height = maxWidth * hRatio;
                }
            }
        }

        SelectionChanged?.Invoke(this, new SelectionEventArgs(ClientSelection, SourceSelection));
    }


    /// <summary>
    /// Load image.
    /// </summary>
    public void SetImage(IgImgData? imgData,
        bool resetZoom = true,
        bool enableFading = true,
        float initOpacity = 0.5f,
        float opacityStep = 0.05f,
        bool isForPreview = false,
        ImgTransform? transforms = null)
    {
        // reset variables
        _imageDrawingState = ImageDrawingState.NotStarted;
        _isPreviewing = isForPreview;
        _clientSelection = default;


        // disable animations
        StopAnimation(AnimationSource.ImageFadeIn);
        StopAnimatingImage();
        DisposeImageResources();
        GC.Collect();


        // emit OnImageChanging event
        ImageLoading?.Invoke(this, EventArgs.Empty);


        // Check and preprocess image info
        LoadImageData(imgData);

        if (imgData == null || imgData.IsImageNull)
        {
            Refresh(resetZoom);
        }
        else
        {
            if (UseHardwareAcceleration)
            {
                _imageD2D = DXHelper.ToD2D1Bitmap(Device, imgData.Image);

                // apply transformations
                if (transforms != null)
                {
                    _ = RotateImage(transforms.Rotation, false);

                    _ = FlipImage(transforms.Flips, false);
                }

                Source = ImageSource.Direct2D;
            }
            else
            {
                _imageGdiPlus = imgData.Bitmap;
                Source = ImageSource.GDIPlus;
            }


            // start drawing
            _imageDrawingState = ImageDrawingState.Drawing;
            if (CanImageAnimate && Source != ImageSource.Null)
            {
                UpdateZoomMode();
                StartAnimatingImage();
            }
            else if (enableFading)
            {

                _imageOpacity = initOpacity;
                _opacityStep = opacityStep;

                if (resetZoom)
                {
                    UpdateZoomMode();
                }

                StartAnimation(AnimationSource.ImageFadeIn);
            }
            else
            {
                _imageOpacity = 1;
                Refresh(resetZoom);
            }
        }
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


    /// <summary>
    /// Rotates the image.
    /// </summary>
    public bool RotateImage(float degree, bool requestRerender = true)
    {
        if (_imageD2D == null || degree == 0 || degree == 360) return false;

        // create effect
        using var effect = Device.CreateEffect(Direct2DEffects.CLSID_D2D12DAffineTransform);
        effect.SetInput(0, _imageD2D);


        // rotate the image
        var rotationMx = D2D_MATRIX_3X2_F.Rotation(degree);
        var transformMx = D2D_MATRIX_3X2_F.Identity();

        // translate the image after rotation
        if (degree == 90 || degree == -270)
        {
            transformMx = rotationMx * D2D_MATRIX_3X2_F.Translation(SourceHeight, 0);
        }
        else if (degree == 180 || degree == -180)
        {
            transformMx = rotationMx * D2D_MATRIX_3X2_F.Translation(SourceWidth, SourceHeight);
        }
        else if (degree == 270 || degree == -90)
        {
            transformMx = rotationMx * D2D_MATRIX_3X2_F.Translation(0, SourceWidth);
        }

        effect.SetValue((int)D2D1_2DAFFINETRANSFORM_PROP.D2D1_2DAFFINETRANSFORM_PROP_TRANSFORM_MATRIX, transformMx);


        // apply the transformation
        DXHelper.DisposeD2D1Bitmap(ref _imageD2D);
        _imageD2D = effect.GetD2D1Bitmap1(Device);

        // update new source size
        var newSize = _imageD2D.GetSize();
        SourceWidth = newSize.width;
        SourceHeight = newSize.height;

        // render the transformation
        if (requestRerender)
        {
            Refresh();
        }

        return true;
    }


    /// <summary>
    /// Flips the image.
    /// </summary>
    public bool FlipImage(FlipOptions flips, bool requestRerender = true)
    {
        if (_imageD2D == null) return false;

        // create effect
        using var effect = Device.CreateEffect(Direct2DEffects.CLSID_D2D12DAffineTransform);
        effect.SetInput(0, _imageD2D);


        // flip transformation
        if (flips.HasFlag(FlipOptions.Horizontal))
        {
            var flipHorizMx = new D2D_MATRIX_3X2_F(-1f, 0, 0, 1f, 0, 0);
            effect.SetValue((int)D2D1_2DAFFINETRANSFORM_PROP.D2D1_2DAFFINETRANSFORM_PROP_TRANSFORM_MATRIX, flipHorizMx * D2D_MATRIX_3X2_F.Translation(SourceWidth, 0));
        }

        if (flips.HasFlag(FlipOptions.Vertical))
        {
            var flipVertMx = new D2D_MATRIX_3X2_F(1f, 0, 0, -1f, 0, 0);
            effect.SetValue((int)D2D1_2DAFFINETRANSFORM_PROP.D2D1_2DAFFINETRANSFORM_PROP_TRANSFORM_MATRIX, flipVertMx * D2D_MATRIX_3X2_F.Translation(0, SourceHeight));
        }


        // apply the transformation
        DXHelper.DisposeD2D1Bitmap(ref _imageD2D);
        _imageD2D = effect.GetD2D1Bitmap1(Device);

        // render the transformation
        if (requestRerender)
        {
            Refresh();
        }

        return true;
    }


    /// <summary>
    /// Gets pixel color.
    /// </summary>
    /// <returns>
    /// <see cref="Color.Transparent"/> if the <see cref="Source"/> is <see cref="ImageSource.Null"/>.
    /// </returns>
    public Color GetColorAt(int x, int y)
    {
        if (Source == ImageSource.Direct2D)
        {
            return _imageD2D.GetPixelColor(Device, x, y);
        }

        if (Source == ImageSource.GDIPlus)
        {
            return _imageGdiPlus.GetPixelColor(x, y);
        }


        return Color.Transparent;
    }

    #endregion // Public methods


    // Private methods
    #region Private methods


    /// <summary>
    /// Loads image data.
    /// </summary>
    private void LoadImageData(IgImgData? imgData)
    {
        CanImageAnimate = imgData?.CanAnimate ?? false;
        HasAlphaPixels = imgData?.HasAlpha ?? false;
        var hasGdiPlusSource = imgData?.Bitmap != null;

        if (CanImageAnimate || hasGdiPlusSource)
        {
            SourceWidth = imgData?.Bitmap?.Width ?? 0;
            SourceHeight = imgData?.Bitmap?.Height ?? 0;
        }
        else
        {
            SourceWidth = imgData?.Image?.Width ?? 0;
            SourceHeight = imgData?.Image?.Height ?? 0;
        }

        var exceedMaxDimention = SourceWidth > Constants.MAX_IMAGE_DIMENSION
            || SourceHeight > Constants.MAX_IMAGE_DIMENSION;

        UseHardwareAcceleration = !hasGdiPlusSource && !CanImageAnimate && !exceedMaxDimention;
    }

    private void OnImageFrameChanged(object? sender, EventArgs eventArgs)
    {
        Invalidate();
    }


    /// <summary>
    /// Disposes and set all checkerboard brushes to <c>null</c>.
    /// </summary>
    private void DisposeCheckerboardBrushes()
    {
        _checkerboardBrushGdip?.Dispose();
        _checkerboardBrushGdip = null;

        _checkerboardBrushD2D?.Dispose();
        _checkerboardBrushD2D = null;
    }


    /// <summary>
    /// Disposes and set all image resources to <c>null</c>.
    /// </summary>
    private void DisposeImageResources()
    {
        Source = ImageSource.Null;

        DXHelper.DisposeD2D1Bitmap(ref _imageD2D);

        // *** Do not dispose the GDI Bitmap because it's a ref to the Local.Images

        //_imageGdiPlus?.Dispose();
        //_imageGdiPlus = null;
    }


    /// <summary>
    /// Shows text message.
    /// </summary>
    /// <param name="text">Message to show</param>
    /// <param name="heading">Heading text</param>
    /// <param name="durationMs">Display duration in millisecond.
    /// Set it <b>0</b> to disable,
    /// or <b>-1</b> to display permanently.</param>
    /// <param name="delayMs">Duration to delay before displaying the message.</param>
    private async void ShowMessagePrivate(string text, string heading = "", int durationMs = -1, int delayMs = 0, bool forceUpdate = true)
    {
        if (durationMs == 0) return;

        var token = _msgTokenSrc?.Token ?? default;

        try
        {
            if (delayMs > 0)
            {
                await Task.Delay(delayMs, token);
            }

            TextHeading = heading;
            Text = text;

            if (forceUpdate)
            {
                Invalidate();
            }

            if (durationMs > 0)
            {
                await Task.Delay(durationMs, token);
            }
        }
        catch { }

        var textNotChanged = Text.Equals(text, StringComparison.InvariantCultureIgnoreCase);
        if (textNotChanged && (durationMs > 0 || token.IsCancellationRequested))
        {
            TextHeading = Text = string.Empty;

            if (forceUpdate)
            {
                Invalidate();
            }
        }
    }


    #endregion // Private methods


}
