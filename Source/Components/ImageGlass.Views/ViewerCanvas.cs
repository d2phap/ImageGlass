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
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.Photoing.Animators;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Numerics;
using WicNet;
using Cursor = System.Windows.Forms.Cursor;
using InterpolationMode = D2Phap.DXControl.InterpolationMode;

namespace ImageGlass.Viewer;

public partial class ViewerCanvas : DXCanvas
{

    // Private properties
    #region Private properties

    private CancellationTokenSource? _msgTokenSrc;

    // original WIC image resources
    private WicBitmapSource? _wicImage;
    private WicBitmapSource? _wicNavLeftImage;
    private WicBitmapSource? _wicNavRightImage;

    // Direct2D image resources
    private IComObject<ID2D1Bitmap1>? _d2dImage;
    private IComObject<ID2D1Bitmap1>? _d2dNavLeftImage;
    private IComObject<ID2D1Bitmap1>? _d2dNavRightImage;
    private ComObject<ID2D1BitmapBrush1>? _checkerboardBrushD2D;


    // to distinguish between clicks
    // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/input-mouse/how-to-distinguish-between-clicks-and-double-clicks
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

    private Color _accentColor = Color.Blue;
    private bool _isPreviewing = false;
    private bool _debugMode = false;
    private ImageDrawingState _imageDrawingState = ImageDrawingState.NotStarted;
    private bool _isColorInverted = false;

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
    private float _zoomSpeed = 0f;
    private float _minZoom = 0.01f; // 1%
    private float _maxZoom = 100f; // 10_000%
    private float[] _zoomLevels = [];
    private ZoomMode _zoomMode = ZoomMode.AutoZoom;
    private ImageInterpolation _interpolationScaleDown = ImageInterpolation.MultiSampleLinear;
    private ImageInterpolation _interpolationScaleUp = ImageInterpolation.NearestNeighbor;

    // checkerboard
    private CheckerboardMode _checkerboardMode = CheckerboardMode.None;
    private Color _checkerboardColor1 = Color.Black.WithAlpha(25);
    private Color _checkerboardColor2 = Color.White.WithAlpha(25);

    // Image source
    private ImageSource _imageSource = ImageSource.Null;
    private AnimatorSource _animatorSource = AnimatorSource.None;
    private ImgAnimator? _imgAnimator = null;
    private AnimationSource _animationSource = AnimationSource.None;

    // Navigation buttons
    private DXButtonStates _navLeftState = DXButtonStates.Normal;
    private DXButtonStates _navRightState = DXButtonStates.Normal;
    private bool _isNavVisible = false;
    private NavButtonDisplay _navDisplay = NavButtonDisplay.None;
    private float NavBorderRadius => NavButtonSize.Width / 2;
    private Color _navButtonColor = Color.Blue;
    internal static float NAV_PADDING => 20f;
    internal PointF NavLeftPos => new(
        DrawingArea.Left + NavButtonSize.Width / 2 + NAV_PADDING,
        DrawingArea.Top + DrawingArea.Height / 2);
    internal PointF NavRightPos => new(
        DrawingArea.Right - NavButtonSize.Width / 2 - NAV_PADDING,
        DrawingArea.Top + DrawingArea.Height / 2);

    // Motion button
    private bool _showMotionButton = false;
    private DXButtonStates _motionBtnState = DXButtonStates.Normal;
    private RectangleF MotionButtonRect
    {
        get
        {
            var margin = this.ScaleToDpi(20f);
            var size = this.ScaleToDpi(40f);

            return new RectangleF()
            {
                X = PaddingLeft + margin,
                Y = PaddingTop + margin,
                Width = size,
                Height = size,
            };
        }
    }


    // selection
    private bool _enableSelection = false;
    private Rectangle _sourceSelection = default;
    private RectangleF _srcSelectionBeforeMoved = default;
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
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int PaddingLeft
    {
        get => Padding.Left;
        set
        {
            Padding = new Padding(value, Padding.Top, Padding.Right, Padding.Bottom);

            // update drawing regions
            CalculateDrawingRegion();
            Refresh(!_isManualZoom);
        }
    }

    /// <summary>
    /// Gets, sets the top padding.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int PaddingTop
    {
        get => Padding.Top;
        set
        {
            Padding = new Padding(Padding.Left, value, Padding.Right, Padding.Bottom);

            // update drawing regions
            CalculateDrawingRegion();
            Refresh(!_isManualZoom);
        }
    }

    /// <summary>
    /// Gets, sets the right padding.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int PaddingRight
    {
        get => Padding.Right;
        set
        {
            Padding = new Padding(Padding.Left, Padding.Top, value, Padding.Bottom);

            // update drawing regions
            CalculateDrawingRegion();
            Refresh(!_isManualZoom);
        }
    }

    /// <summary>
    /// Gets, sets the bottom padding.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int PaddingBottom
    {
        get => Padding.Bottom;
        set
        {
            Padding = new Padding(Padding.Left, Padding.Top, Padding.Right, value);

            // update drawing regions
            CalculateDrawingRegion();
            Refresh(!_isManualZoom);
        }
    }

    /// <summary>
    /// Gets the drawing area after deducting <see cref="Padding"/>.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RectangleF DrawingArea => new RectangleF(
        Padding.Left,
        Padding.Top,
        Width - Padding.Horizontal,
        Height - Padding.Vertical);

    /// <summary>
    /// Gets rectangle of the viewport.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RectangleF ImageDestBounds => _destRect;


    /// <summary>
    /// Gets the rectangle of the source image region being drawn.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RectangleF ImageSourceBounds => _srcRect;


    /// <summary>
    /// Gets the center point of image viewport.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PointF ImageViewportCenterPoint => new()
    {
        X = ImageDestBounds.X + ImageDestBounds.Width / 2,
        Y = ImageDestBounds.Y + ImageDestBounds.Height / 2,
    };


    /// <summary>
    /// Checks if the viewing image size if smaller than the viewport size.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ImageDrawingState ImageDrawingState => _imageDrawingState;


    /// <summary>
    /// Gets, sets accent color.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentColor
    {
        get => _accentColor;
        set
        {
            _accentColor = value;

            if (Web2 != null) Web2.AccentColor = _accentColor;
        }
    }


    /// <summary>
    /// Gets the client selection area.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Rectangle ClientSelection => this.RectSourceToClient(_sourceSelection).ToRectangle();


    /// <summary>
    /// Gets, sets the image source selection. This will emit the event <see cref="SelectionChanged"/>.
    /// Use <see cref="SetSourceSelection"/> to control the selection event.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Rectangle SourceSelection
    {
        get => _sourceSelection;
        set => SetSourceSelection(value, true);
    }


    /// <summary>
    /// Gets the resizers of the selection rectangle
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<SelectionResizer> SelectionResizers
    {
        get
        {
            if (SourceSelection.Size.IsEmpty) return [];


            var resizerSize = DpiApi.Scale(Font.Size * 1.3f);
            var resizerMargin = DpiApi.Scale(2);
            var hitSize = DpiApi.Scale(resizerSize * 1.3f);

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
                topLeftHit.Right,
                ClientSelection.Y - hitSize / 2 + resizerSize / 2,
                topRightHit.X - topLeftHit.Right, hitSize);

            // right
            var right = new RectangleF(
                    ClientSelection.Right - resizerSize - resizerMargin,
                    ClientSelection.Y + ClientSelection.Height / 2 - resizerSize / 2,
                    resizerSize, resizerSize);
            var rightHit = new RectangleF(
                    ClientSelection.Right - hitSize / 2 - resizerSize / 2,
                    topRightHit.Bottom,
                    hitSize, bottomRightHit.Y - topRightHit.Bottom);

            // bottom
            var bottom = new RectangleF(
                    ClientSelection.X + ClientSelection.Width / 2 - resizerSize / 2,
                    ClientSelection.Bottom - resizerSize - resizerMargin,
                    resizerSize, resizerSize);
            var bottomHit = new RectangleF(
                    bottomLeftHit.Right,
                    ClientSelection.Bottom - hitSize / 2 - resizerSize / 2,
                    bottomRightHit.X - bottomLeftHit.Right, hitSize);

            // left
            var left = new RectangleF(
                    ClientSelection.X + resizerMargin,
                    ClientSelection.Y + ClientSelection.Height / 2 - resizerSize / 2,
                    resizerSize, resizerSize);
            var leftHit = new RectangleF(
                    ClientSelection.X - hitSize / 2 + resizerSize / 2,
                    topLeftHit.Bottom,
                    hitSize, bottomLeftHit.Y - topLeftHit.Bottom);

            // 8 resizers
            return [
                // bottom-right is in higher layer
                new(SelectionResizerType.BottomRight, bottomRight, bottomRightHit),
                new(SelectionResizerType.TopLeft, topLeft, topLeftHit),
                new(SelectionResizerType.BottomLeft, bottomLeft, bottomLeftHit),
                new(SelectionResizerType.TopRight, topRight, topRightHit),

                new(SelectionResizerType.Right, right, rightHit),
                new(SelectionResizerType.Bottom, bottom, bottomHit),
                new(SelectionResizerType.Left, left, leftHit),
                new(SelectionResizerType.Top, top, topHit),
            ];
        }
    }


    /// <summary>
    /// Gets, sets selection aspect ratio.
    /// If Width or Height is <c>less than or equals 0</c>, we will use free aspect ratio.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public SizeF SelectionAspectRatio { get; set; } = new();


    /// <summary>
    /// Enables or disables the selection.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public SelectionAction CurrentSelectionAction { get; private set; } = SelectionAction.None;

    #endregion // Viewport


    // Image information
    #region Image information

    /// <summary>
    /// Checks if the bitmap image has alpha pixels.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool HasAlphaPixels { get; private set; } = false;

    /// <summary>
    /// Checks if the bitmap image can animate.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool CanImageAnimate { get; private set; } = false;

    /// <summary>
    /// Checks if the image is animating.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsImageAnimating { get; protected set; } = false;

    /// <summary>
    /// Checks if the input image is null.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ImageSource Source
    {
        get => _imageSource;
        private set
        {
            _imageSource = value;

            if (_web2 != null)
            {
                // WebView2 visible for SVG source or SVG comparison mode
                var shouldBeVisible = value == ImageSource.Webview2 || IsWeb2ComparisonModeActive;
                _ = _web2.SetWeb2VisibilityAsync(shouldBeVisible);

                if (IsWeb2ComparisonModeActive && value != ImageSource.Webview2)
                {
                    _web2.BringToFront();
                }
            }
        }
    }

    /// <summary>
    /// Gets the input image's width.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float SourceWidth { get; private set; } = 0;

    /// <summary>
    /// Gets the input image's height.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float SourceHeight { get; private set; } = 0;

    #endregion // Image information


    // Zooming
    #region Zooming

    /// <summary>
    /// Gets, sets zoom levels (ordered by ascending).
    /// </summary>
    [Category("Zooming")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float[] ZoomLevels
    {
        get => _zoomLevels;
        set => _zoomLevels = value.OrderBy(x => x)
            .Where(i => i > 0)
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Gets, sets the minimum zoom factor (<c>1.0f = 100%</c>).
    /// Returns the first value of <see cref="ZoomLevels"/> if it is not empty.
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(0.01f)]
    public float MinZoom
    {
        get
        {
            if (ZoomLevels.Length > 0) return ZoomLevels[0];
            return _minZoom;
        }
        set => _minZoom = Math.Min(Math.Max(0.001f, value), 1000);
    }

    /// <summary>
    /// Gets, sets the maximum zoom factor (<c>1.0f = 100%</c>).
    /// Returns the last value of <see cref="ZoomLevels"/> if it is not empty.
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(100.0f)]
    public float MaxZoom
    {
        get
        {
            if (ZoomLevels.Length > 0) return ZoomLevels[ZoomLevels.Length - 1];
            return _maxZoom;
        }
        set => _maxZoom = Math.Min(Math.Max(0.001f, value), 1000);
    }

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
            _zoomMode = value;
            Refresh();
        }
    }

    /// <summary>
    /// Gets, sets interpolation mode used when the
    /// <see cref="ZoomFactor"/> is less than or equal <c>1.0f</c>.
    /// </summary>
    [Category("Zooming")]
    [DefaultValue(ImageInterpolation.Linear)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        get => _interpolationScaleUp;
        set
        {
            if (_interpolationScaleUp != value)
            {
                _interpolationScaleUp = value;
                Invalidate();
            }
        }
    }


    /// <summary>
    /// Gets the current <see cref="ImageInterpolation"/> mode.
    /// </summary>
    [Browsable(false)]
    public ImageInterpolation CurrentInterpolation
    {
        get
        {
            if (ZoomFactor < 1f) return _interpolationScaleDown;
            if (ZoomFactor > 1f) return _interpolationScaleUp;

            return ImageInterpolation.NearestNeighbor;
        }
    }

    #endregion // Zooming


    // Checkerboard
    #region Checkerboard

    [Category("Checkerboard")]
    [DefaultValue(CheckerboardMode.None)]

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

    #endregion // Checkerboard


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

    #endregion // Panning


    // Navigation
    #region Navigation

    /// <summary>
    /// Gets, sets the Motion button visibility.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowMotionButton
    {
        get => _showMotionButton;
        set
        {
            if (_showMotionButton != value)
            {
                _showMotionButton = value;
                Invalidate();
            }
        }
    }


    /// <summary>
    /// Gets, sets the navigation buttons display style.
    /// </summary>
    [Category("Navigation")]
    [DefaultValue(NavButtonDisplay.None)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

            SetWeb2NavButtonStyles();
        }
    }


    /// <summary>
    /// Gets, sets color for navigation buttons.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color NavButtonColor
    {
        get => _navButtonColor;
        set
        {
            _navButtonColor = value;

            if (Web2 != null) SetWeb2NavButtonStyles();
        }
    }

    /// <summary>
    /// Gets, sets the navigation button size.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public SizeF NavButtonSize => this.ScaleToDpi(new SizeF(50f, 50f));


    /// <summary>
    /// Gets, sets the left navigation button icon image.
    /// </summary>
    [Category("Navigation")]
    [DefaultValue(typeof(Bitmap), null)]
    public WicBitmapSource? NavLeftImage
    {
        set
        {
            // _wicNavLeftImage is a ref, do not dispose here
            _wicNavLeftImage = value;
            DXHelper.DisposeD2D1Bitmap(ref _d2dNavLeftImage);
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
            // _wicNavRightImage is a ref, do not dispose here
            _wicNavRightImage = value;
            DXHelper.DisposeD2D1Bitmap(ref _d2dNavRightImage);
        }
    }

    #endregion // Navigation


    // Misc
    #region Misc

    /// <summary>
    /// Enable transparent background.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool EnableTransparent { get; set; } = true;


    /// <summary>
    /// Gets, sets the message heading text
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TextHeading { get; set; } = string.Empty;


    /// <summary>
    /// Gets, sets border radius of message box
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float MessageBorderRadius => this.ScaleToDpi(8f);


    /// <summary>
    /// Gets the current animating source
    /// </summary>
    [Browsable(false)]
    public AnimationSource AnimationSource => _animationSource;


    /// <summary>
    /// Gets the value indicates whether the color is inverted by <see cref="InvertColor(bool)"/>.
    /// </summary>
    [Browsable(false)]
    public bool IsColorInverted => _isColorInverted;


    /// <summary>
    /// Enables, disables debug mode.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool EnableDebug
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
    /// Occurs when when image is drawn and before control's controls are drawn.
    /// </summary>
    public event EventHandler<DrawingEventArgs>? Drawing;

    /// <summary>
    /// Occurs when <see cref="ZoomFactor"/> value changes.
    /// </summary>
    public event EventHandler<ZoomEventArgs>? OnZoomChanged;

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
    /// Occurs when the motion button clicked.
    /// </summary>
    public event EventHandler<MouseEventArgs>? OnMotionBtnClicked;


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



    public ViewerCanvas()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor
            | ControlStyles.UserPaint
            | ControlStyles.Selectable
            | ControlStyles.UserMouse, true);
        _clickTimer.Tick += ClickTimer_Tick;

        // touch gesture support
        InitializeTouchGesture();
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
        DisposeCompareImageResources();

        DXHelper.DisposeD2D1Bitmap(ref _d2dNavLeftImage);
        DXHelper.DisposeD2D1Bitmap(ref _d2dNavRightImage);
        _d2dCompareSliderHandle?.Dispose();
        _d2dCompareSliderHandle = null;

        DisposeCheckerboardBrushes();

        _clickTimer.Dispose();
        _msgTokenSrc?.Dispose();
        _msgTokenSrc = null;

        DisposeWeb2Control();
    }

    protected override void OnDeviceCreated(DeviceCreatedReason reason)
    {
        base.OnDeviceCreated(reason);

        // dispose the Direct2D resource of the old device
        DXHelper.DisposeD2D1Bitmap(ref _d2dNavLeftImage);
        DXHelper.DisposeD2D1Bitmap(ref _d2dNavRightImage);

        if (reason != DeviceCreatedReason.FirstTime)
        {
            // dispose checkerboard
            DisposeCheckerboardBrushes();

            // dispose the Direct2D image
            DXHelper.DisposeD2D1Bitmap(ref _d2dImage);

            // dispose and recreate comparison D2D image if we have a WIC source
            DXHelper.DisposeD2D1Bitmap(ref _d2dCompareImage);
            if (_wicCompareImage != null)
            {
                _d2dCompareImage = DXHelper.ToD2D1Bitmap(Device, _wicCompareImage);
            }
        }
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

        // Handle comparison mode interactions first
        if (_comparisonMode && HandleComparisonMouseDown(e))
        {
            return;
        }

        _mouseDownButton = e.Button;
        _isMouseDragged = false;
        _mouseDownPoint = e.Location;

        var canSelect = EnableSelection && _mouseDownButton == MouseButtons.Left;
        var requestRerender = false;


        // Check the clickable buttons
        #region Check the clickable buttons
        if (e.Button == MouseButtons.Left)
        {
            if (ShowMotionButton && MotionButtonRect.Contains(e.Location))
            {
                _motionBtnState |= DXButtonStates.Pressed;
                requestRerender = true;
            }
            else
            {
                // calculate whether the point inside the left nav
                if (this.CheckWhichNav(e.Location, NavCheck.LeftOnly) == MouseAndNavLocation.LeftNav)
                {
                    _navLeftState |= DXButtonStates.Pressed;
                    requestRerender = true;
                }

                // calculate whether the point inside the right nav
                if (this.CheckWhichNav(e.Location, NavCheck.RightOnly) == MouseAndNavLocation.RightNav)
                {
                    _navRightState |= DXButtonStates.Pressed;
                    requestRerender = true;
                }
            }
        }
        #endregion


        // Image panning & Selecting check
        #region Image panning & selecting check
        if (Source != ImageSource.Null)
        {
            requestRerender = requestRerender || (canSelect && !SourceSelection.Size.IsEmpty);
            _selectedResizer = SelectionResizers.Find(i => i.HitRegion.Contains(e.Location));
            _canDrawSelection = canSelect && !_isSelectionHovered && _hoveredResizer == null;

            if (canSelect)
            {
                _srcSelectionBeforeMoved = new RectangleF(_sourceSelection.Location, _sourceSelection.Size);

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


        if (requestRerender) Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!IsReady) return;

        // Handle comparison mode interactions first
        if (_comparisonMode && HandleComparisonMouseUp(e))
        {
            // Reset mouse state to prevent stuck panning
            _mouseDownButton = MouseButtons.None;
            _mouseDownPoint = null;
            return;
        }


        // Distinguish between clicks
        #region Distinguish between clicks
        if (_mouseDownButton != MouseButtons.None)
        {
            // always treat Middle & XButtons as single click
            if (_mouseDownButton == MouseButtons.Middle
                || _mouseDownButton == MouseButtons.XButton1
                || _mouseDownButton == MouseButtons.XButton2)
            {
                // enable Middle & XButtons click only if not panning in selection mode
                if (!_isMouseDragged) base.OnMouseClick(e);
            }
            else
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
            }


            // emit event ImageMouseClick
            var imgX = (e.X - _destRect.X) / _zoomFactor + _srcRect.X;
            var imgY = (e.Y - _destRect.Y) / _zoomFactor + _srcRect.Y;
            ImageMouseClick?.Invoke(this, new(imgX, imgY, e.Button));
        }
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


        // Check the clickable buttons
        #region Check the clickable buttons

        // trigger nav click only if selection is empty
        if (e.Button == MouseButtons.Left && SourceSelection.Size.IsEmpty)
        {
            // emit motion button event
            if (ShowMotionButton
                && MotionButtonRect.Contains(e.Location)
                && _motionBtnState.HasFlag(DXButtonStates.Pressed))
            {
                OnMotionBtnClicked?.Invoke(this, e);
            }
            else if (_navRightState.HasFlag(DXButtonStates.Pressed))
            {
                // emit nav button event if the point inside the right nav
                if (this.CheckWhichNav(e.Location, NavCheck.RightOnly) == MouseAndNavLocation.RightNav)
                {
                    OnNavRightClicked?.Invoke(this, e);
                }
            }
            else if (_navLeftState.HasFlag(DXButtonStates.Pressed))
            {
                // emit nav button event if the point inside the left nav
                if (this.CheckWhichNav(e.Location, NavCheck.LeftOnly) == MouseAndNavLocation.LeftNav)
                {
                    OnNavLeftClicked?.Invoke(this, e);
                }
            }
        }

        _motionBtnState &= ~DXButtonStates.Pressed;
        _navLeftState &= ~DXButtonStates.Pressed;
        _navRightState &= ~DXButtonStates.Pressed;
        #endregion

    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!IsReady) return;

        // Handle comparison mode interactions first
        if (_comparisonMode && HandleComparisonMouseMove(e))
        {
            return;
        }

        var canSelect = EnableSelection && _mouseDownButton == MouseButtons.Left;
        var requestRerender = false;
        _mouseMovePoint = e.Location;
        CurrentSelectionAction = SelectionAction.None;


        // Navigation hoverable check
        #region Navigation hoverable check

        // hide nav button when hovering on the selection area
        if (ClientSelection.Contains(e.Location))
        {
            _motionBtnState &= ~DXButtonStates.Hover;
            _navLeftState &= ~DXButtonStates.Hover;
            _navRightState &= ~DXButtonStates.Hover;
        }

        // if no button pressed, check if nav is hovered
        else if (e.Button == MouseButtons.None)
        {
            // check if Motion btn is being hovered
            var isMotionBtnHovered = ShowMotionButton && MotionButtonRect.Contains(e.Location);
            var isNavLeftHovered = false;
            var isNavRightHovered = false;

            if (isMotionBtnHovered)
            {
                _motionBtnState |= DXButtonStates.Hover;
                _navLeftState &= ~DXButtonStates.Hover;
                _navRightState &= ~DXButtonStates.Hover;
            }
            // check if Nav buttons are being hovered
            else
            {
                _motionBtnState &= ~DXButtonStates.Hover;

                // calculate whether the point inside the left nav
                isNavLeftHovered = this.CheckWhichNav(e.Location, NavCheck.LeftOnly) == MouseAndNavLocation.LeftNav;
                if (isNavLeftHovered) _navLeftState |= DXButtonStates.Hover;
                else _navLeftState &= ~DXButtonStates.Hover;

                // calculate whether the point inside the right nav
                isNavRightHovered = this.CheckWhichNav(e.Location, NavCheck.RightOnly) == MouseAndNavLocation.RightNav;
                if (isNavRightHovered) _navRightState |= DXButtonStates.Hover;
                else _navRightState &= ~DXButtonStates.Hover;
            }

            if (!isMotionBtnHovered && !isNavLeftHovered && !isNavRightHovered && _isNavVisible)
            {
                requestRerender = true;
                _isNavVisible = false;
            }
            else
            {
                requestRerender = _isNavVisible = isMotionBtnHovered || isNavLeftHovered || isNavRightHovered;
            }
        }
        #endregion



        var canPanWhileSelecting = _mouseDownButton == MouseButtons.Middle
            && EnableSelection
            && !SourceSelection.Size.IsEmpty;

        // Image panning check
        if (_mouseDownButton == MouseButtons.Left || canPanWhileSelecting)
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
                requestRerender = true;
            }
            // move selection
            else if (canSelect)
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
        if (!EnableSelection)
        {
            Cursor = Cursors.Default;
        }
        else
        {
            // set resizer cursor
            var hoveredResizer = SelectionResizers.Find(i => i.HitRegion.Contains(e.Location));

            if (hoveredResizer != null)
            {
                Cursor = hoveredResizer.Cursor;
            }
            else if (ClientSelection.Contains(e.Location))
            {
                Cursor = Cursors.SizeAll;
            }
            else
            {
                Cursor = Cursors.Cross;
            }


            // redraw the canvas
            var isSelectionHovered = ClientSelection.Contains(e.Location);
            requestRerender = requestRerender
                || _isSelectionHovered != isSelectionHovered
                || _hoveredResizer != hoveredResizer;


            _isSelectionHovered = isSelectionHovered;
            _hoveredResizer = hoveredResizer;
        }


        // request re-render control
        if (requestRerender) Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        _motionBtnState &= ~DXButtonStates.Hover;
        _navLeftState &= ~DXButtonStates.Hover;
        _navRightState &= ~DXButtonStates.Hover;
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
        base.OnResize(e);
        if (DrawingArea.IsEmpty) return;

        // update drawing regions
        CalculateDrawingRegion();

        // redraw the control on resizing if it's not manual zoom
        if (IsReady && Source != ImageSource.Null && !_isManualZoom)
        {
            Refresh(true, false, true);
        }
    }

    protected override void OnFrame(FrameEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(OnFrame, e);
            return;
        }

        base.OnFrame(e);
        if (UseWebview2) return;


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
    }


    protected override void OnRender(DXGraphics g)
    {
        // check if the image is already drawn
        var isImageDrawn = _imageDrawingState == ImageDrawingState.Drawing && !_isPreviewing;

        // correct the background if no transparency
        if (!EnableTransparent)
        {
            g.ClearBackground(TopLevelControl.BackColor);
            g.DrawRectangle(ClientRectangle, 0, Color.Transparent, BackColor);
        }

        // checkerboard background
        DrawCheckerboardLayer(g);


        // draw image layer (or comparison layer if in comparison mode)
        if (_comparisonMode)
        {
            DrawComparisonLayer(g);
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

        // emit drawing event
        Drawing?.Invoke(this, new DrawingEventArgs
        {
            Graphics = g,
            SrcRect = _srcRect,
            DestRect = _destRect,
            ZoomFactor = _zoomFactor,
        });


        // Draw selection layer
        if (EnableSelection)
        {
            DrawSelectionLayer(g);
        }


        // text message
        DrawMessageLayer(g);


        // draw motion button
        DrawMotionButton(g);


        // navigation layer
        DrawNavigationLayer(g);

        if (EnableDebug)
        {
            var text = "";
            var monitor = DirectN.Monitor.FromWindow(TopLevelControl.Handle);

            if (monitor != null)
            {
                text = $"Monitor={monitor?.Bounds.Size}; {(int)monitor.ScaleFactor}%; ";
            }

            text = $"Dpi={DeviceDpi}; Renderer={Source}; Acceleration={UseHardwareAcceleration}; ";
            if (UseWebview2)
            {
                text += $"v{Web2.Webview2Version}; ";
            }
            else
            {
                text += $"FPS={FPS}; ";
            }

            var textSize = g.MeasureText(text, Font.Name, Font.Size, textDpi: DeviceDpi);
            g.DrawRectangle(0, 0, textSize.Width, textSize.Height, 0, Color.Red, Color.Black.WithAlpha(200));
            g.DrawText(text, Font.Name, Font.Size, 0f, 0f, Color.Yellow, textDpi: DeviceDpi);
        }

        base.OnRender(g);


        // emits event ImageLoaded
        if (isImageDrawn)
        {
            _imageDrawingState = ImageDrawingState.Done;
            ImageLoaded?.Invoke(this, EventArgs.Empty);
        }
    }


    /// <summary>
    /// Calculates the drawing region
    /// </summary>
    protected virtual void CalculateDrawingRegion()
    {
        if (Source == ImageSource.Null || DrawingArea.IsEmpty) return;

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
    }


    /// <summary>
    /// Draw the input image.
    /// </summary>
    protected virtual void DrawImageLayer(DXGraphics g)
    {
        if (UseWebview2) return;
        if (Source == ImageSource.Null) return;

        g.DrawBitmap(_d2dImage, _destRect, _srcRect, (InterpolationMode)CurrentInterpolation);
    }


    /// <summary>
    /// Draw checkerboard background
    /// </summary>
    protected virtual void DrawCheckerboardLayer(DXGraphics g)
    {
        if (CheckerboardMode == CheckerboardMode.None) return;

        // region to draw
        RectangleF region;

        if (CheckerboardMode == CheckerboardMode.Image)
        {
            if (UseWebview2)
            {
                region = _web2DestRect;
            }
            else
            {
                // no need to draw checkerboard if image does not has alpha pixels
                if (!HasAlphaPixels) return;

                region = _destRect;
            }
        }
        else
        {
            region = DrawingArea;
        }


        // create bitmap brush
        _checkerboardBrushD2D ??= VHelper.CreateCheckerBoxTileD2D(Device, this.ScaleToDpi(Const.VIEWER_GRID_SIZE), CheckerboardColor1, CheckerboardColor2);

        // draw checkerboard
        Device.FillRectangle(DXHelper.ToD2DRectF(region), _checkerboardBrushD2D);
    }


    /// <summary>
    /// Draw selection layer
    /// </summary>
    protected virtual void DrawSelectionLayer(DXGraphics g)
    {
        if (UseWebview2 || Source == ImageSource.Null || SourceSelection.Size.IsEmpty) return;

        // draw the clip selection region
        using var selectionGeo = g.GetCombinedRectanglesGeometry(ClientSelection, _destRect, 0, 0, D2D1_COMBINE_MODE.D2D1_COMBINE_MODE_XOR);
        g.DrawGeometry(selectionGeo, Color.Transparent, Color.Black.WithAlpha(_mouseDownButton == MouseButtons.Left ? 100 : 180));


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
            var text = $"{_sourceSelection.Width} x {_sourceSelection.Height}";
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


            // draw resizers with layer order
            var resizers = SelectionResizers;
            resizers.Reverse();

            foreach (var rItem in resizers)
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
                if (EnableDebug)
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
    /// Draw Motion button
    /// </summary>
    protected virtual void DrawMotionButton(DXGraphics g)
    {
        if (!ShowMotionButton || EnableSelection) return;

        VHelper.DrawDXButton(g,
            MotionButtonRect,
            MessageBorderRadius,
            ForeColor.InvertBlackOrWhite(150),
            NavButtonColor,
            this.ScaleToDpi(1f),
            null,
            _motionBtnState);

        var iconAlpha = _motionBtnState.HasFlag(DXButtonStates.Pressed)
            ? 153 // 0.6f
            : 200;
        var iconY = _motionBtnState.HasFlag(DXButtonStates.Pressed)
            ? this.ScaleToDpi(1f)
            : 0f;


        // draw solid circle
        var iconSize = MotionButtonRect.Height * 0.25f;
        var iconRect = new RectangleF(
            MotionButtonRect.X + MotionButtonRect.Width / 2 - iconSize / 2,
            MotionButtonRect.Y + MotionButtonRect.Height / 2 - iconSize / 2 + iconY,
            iconSize,
            iconSize);
        g.DrawEllipse(iconRect, ForeColor.WithAlpha(iconAlpha), null, this.ScaleToDpi(1f));


        // draw dashed circle
        iconSize = MotionButtonRect.Height * 0.5f;
        iconRect = new RectangleF(
            MotionButtonRect.X + MotionButtonRect.Width / 2 - iconSize / 2,
            MotionButtonRect.Y + MotionButtonRect.Height / 2 - iconSize / 2 + iconY,
            iconSize,
            iconSize);

        var ellipse = new D2D1_ELLIPSE(iconRect.X + iconRect.Width / 2, iconRect.Y + iconRect.Height / 2, iconRect.Width / 2, iconRect.Height / 2);


        // draw ellipse border ------------------------------------
        // create solid brush for border
        var bdColor = DXHelper.FromColor(ForeColor.WithAlpha(iconAlpha));
        using var bdBrush = g.DeviceContext.CreateSolidColorBrush(bdColor);

        using var strokeStyle = g.D2DFactory.CreateStrokeStyle(new D2D1_STROKE_STYLE_PROPERTIES()
        {
            dashCap = D2D1_CAP_STYLE.D2D1_CAP_STYLE_ROUND,
            dashStyle = D2D1_DASH_STYLE.D2D1_DASH_STYLE_CUSTOM,
        }, [this.ScaleToDpi(1.5f), this.ScaleToDpi(1.5f)]);

        // draw border
        g.DeviceContext.DrawEllipse(ellipse, bdBrush, this.ScaleToDpi(1f), strokeStyle);
    }


    /// <summary>
    /// Draws text message.
    /// </summary>
    protected virtual void DrawMessageLayer(DXGraphics g)
    {
        if (UseWebview2) return;

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
        var bgColor = ForeColor.InvertBlackOrWhite(200);
        g.DrawRectangle(bgRegion, MessageBorderRadius, bgColor, bgColor);


        // debug
        if (EnableDebug)
        {
            g.DrawRectangle(drawableArea, MessageBorderRadius, Color.Red);
            g.DrawRectangle(hRegion, MessageBorderRadius, Color.Orange);
            g.DrawRectangle(tRegion, MessageBorderRadius, Color.Green);
        }


        // draw text heading
        if (hasHeading)
        {
            var headingColor = AccentColor;//.Blend(Color.White, 0.7f);
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
    protected virtual void DrawNavigationLayer(DXGraphics g)
    {
        if (NavDisplay == NavButtonDisplay.None || EnableSelection) return;


        // left navigation
        if (NavDisplay == NavButtonDisplay.Left || NavDisplay == NavButtonDisplay.Both)
        {
            if (_navLeftState != DXButtonStates.Normal)
            {
                _d2dNavLeftImage ??= DXHelper.ToD2D1Bitmap(Device, _wicNavLeftImage);

                // draw background
                VHelper.DrawDXButton(g,
                    new RectangleF()
                    {
                        X = NavLeftPos.X - NavButtonSize.Width / 2,
                        Y = NavLeftPos.Y - NavButtonSize.Height / 2,
                        Width = NavButtonSize.Width,
                        Height = NavButtonSize.Height,
                    },
                    NavBorderRadius,
                    ForeColor.InvertBlackOrWhite(),
                    NavButtonColor,
                    this.ScaleToDpi(1f),
                    _d2dNavLeftImage,
                    _navLeftState);
            }
        }


        // right navigation
        if (NavDisplay == NavButtonDisplay.Right || NavDisplay == NavButtonDisplay.Both)
        {
            if (_navRightState != DXButtonStates.Normal)
            {
                _d2dNavRightImage ??= DXHelper.ToD2D1Bitmap(Device, _wicNavRightImage);


                // draw background
                VHelper.DrawDXButton(g,
                    new RectangleF()
                    {
                        X = NavRightPos.X - NavButtonSize.Width / 2,
                        Y = NavRightPos.Y - NavButtonSize.Height / 2,
                        Width = NavButtonSize.Width,
                        Height = NavButtonSize.Height,
                    },
                    NavBorderRadius,
                    ForeColor.InvertBlackOrWhite(),
                    NavButtonColor,
                    this.ScaleToDpi(1f),
                    _d2dNavRightImage,
                    _navRightState);
            }
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

        if (UseWebview2)
        {
            SetZoomFactorWeb2(zoomValue, isManualZoom);
            return;
        }

        // update drawing regions
        CalculateDrawingRegion();

        Invalidate();

        OnZoomChanged?.Invoke(this, new ZoomEventArgs()
        {
            ZoomFactor = _zoomFactor,
            IsManualZoom = _isManualZoom,
            IsZoomModeChange = false,
            IsPreviewingImage = _isPreviewing,
            ChangeSource = ZoomChangeSource.Unknown,
        });
    }


    /// <summary>
    /// Calculates zoom factor by the input zoom mode, and source size.
    /// </summary>
    public float CalculateZoomFactor(ZoomMode zoomMode, float srcWidth, float srcHeight)
    {
        return CalculateZoomFactor(zoomMode, srcWidth, srcHeight, (int)DrawingArea.Width, (int)DrawingArea.Height);
    }


    /// <summary>
    /// Calculates zoom factor by the input zoom mode, and source size.
    /// </summary>
    public float CalculateZoomFactor(ZoomMode zoomMode, float srcWidth, float srcHeight, int viewportW, int viewportH)
    {
        if (srcWidth == 0 || srcHeight == 0
            || viewportW == 0 || viewportH == 0) return _zoomFactor;


        var widthScale = viewportW / srcWidth;
        var heightScale = viewportH / srcHeight;
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
    /// Updates zoom mode logic. This <u><c>does not</c></u> redraw the viewing image.
    /// </summary>
    public void SetZoomMode(ZoomMode? mode = null, bool isManualZoom = false, bool zoomedByResizing = false)
    {
        if (DrawingArea.IsEmpty) return;


        // get zoom factor after applying the zoom mode
        _zoomMode = mode ?? _zoomMode;
        _zoomFactor = CalculateZoomFactor(_zoomMode, SourceWidth, SourceHeight);
        _isManualZoom = isManualZoom;

        // update drawing regions
        CalculateDrawingRegion();

        // use webview
        if (UseWebview2)
        {
            var obj = new ExpandoObject();
            _ = obj.TryAdd("ZoomMode", _zoomMode.ToString());
            _ = obj.TryAdd("IsManualZoom", isManualZoom);

            Web2.PostWeb2Message(Web2BackendMsgNames.SET_ZOOM_MODE, BHelper.ToJson(obj));
            return;
        }


        if (!IsReady || Source == ImageSource.Null) return;

        OnZoomChanged?.Invoke(this, new ZoomEventArgs()
        {
            ZoomFactor = ZoomFactor,
            IsManualZoom = _isManualZoom,
            IsZoomModeChange = mode != _zoomMode,
            IsPreviewingImage = _isPreviewing,
            ChangeSource = zoomedByResizing ? ZoomChangeSource.SizeChanged : ZoomChangeSource.ZoomMode,
        });
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
    public void Refresh(bool resetZoom = true, bool isManualZoom = false, bool zoomedByResizing = false)
    {
        if (resetZoom)
        {
            SetZoomMode(null, isManualZoom, zoomedByResizing);
        }

        Invalidate();
    }


    /// <summary>
    /// Starts a built-in animation.
    /// </summary>
    /// <param name="sources">Source of animation</param>
    public void StartAnimation(AnimationSource sources)
    {
        if (UseWebview2)
        {
            StartWeb2Animation(sources);
        }

        _animationSource = sources;
        RequestUpdateFrame = true;
    }


    /// <summary>
    /// Stops a built-in animation.
    /// </summary>
    /// <param name="sources">Source of animation</param>
    public void StopAnimation(AnimationSource sources)
    {
        if (UseWebview2)
        {
            StopWeb2Animations();
        }

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
        if (factor >= MaxZoom || factor <= MinZoom) return false;

        var newZoomFactor = factor;
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
            X = (location.X - gapX) * newZoomFactor / ZoomFactor,
            Y = (location.Y - gapY) * newZoomFactor / ZoomFactor,
        };

        // the distance of 2 points after zoomed
        var zoomedDistance = new SizeF()
        {
            Width = zoomedLocation.X - location.X,
            Height = zoomedLocation.Y - location.Y,
        };

        // perform zoom if the new zoom factor is different
        if (_zoomFactor != newZoomFactor)
        {
            _zoomFactor = Math.Min(MaxZoom, Math.Max(newZoomFactor, MinZoom));
            _isManualZoom = true;

            // update drawing regions
            CalculateDrawingRegion();

            // if using Webview2
            if (UseWebview2)
            {
                SetZoomFactorWeb2(_zoomFactor, _isManualZoom);
                return true;
            }

            PanTo(zoomedDistance.Width, zoomedDistance.Height, requestRerender);

            // emit OnZoomChanged event
            OnZoomChanged?.Invoke(this, new ZoomEventArgs()
            {
                ZoomFactor = _zoomFactor,
                IsManualZoom = _isManualZoom,
                IsZoomModeChange = false,
                IsPreviewingImage = _isPreviewing,
                ChangeSource = ZoomChangeSource.Unknown,
            });

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
    public bool ZoomByDeltaToPoint(float delta, PointF? point = null,
        bool requestRerender = true)
    {
        var newZoomFactor = _zoomFactor;
        var isZoomingByMouseWheel = Math.Abs(delta) == SystemInformation.MouseWheelScrollDelta;

        // use zoom levels
        if (ZoomLevels.Length > 0 && isZoomingByMouseWheel)
        {
            var minZoomLevel = ZoomLevels[0];
            var maxZoomLevel = ZoomLevels[ZoomLevels.Length - 1];

            // zoom in
            if (delta > 0)
            {
                newZoomFactor = ZoomLevels.FirstOrDefault(i => i > _zoomFactor);
            }
            // zoom out
            else if (delta < 0)
            {
                newZoomFactor = ZoomLevels.LastOrDefault(i => i < _zoomFactor);
            }
            if (newZoomFactor == 0) return false;

            // limit zoom factor
            newZoomFactor = Math.Min(Math.Max(minZoomLevel, newZoomFactor), maxZoomLevel);

        }
        // use smooth zooming
        else
        {
            var speed = delta / (501f - ZoomSpeed);

            // zoom in
            if (delta > 0)
            {
                newZoomFactor = _zoomFactor * (1f + speed);
            }
            // zoom out
            else if (delta < 0)
            {
                newZoomFactor = _zoomFactor / (1f - speed);
            }

            // limit zoom factor
            newZoomFactor = Math.Min(Math.Max(MinZoom, newZoomFactor), MaxZoom);
        }


        if (newZoomFactor == _zoomFactor) return false;

        var location = point ?? new PointF(-1, -1);
        // use the center point if the point is outside
        if (!Bounds.Contains((int)location.X, (int)location.Y))
        {
            location = ImageViewportCenterPoint;
        }


        _oldZoomFactor = _zoomFactor;
        _zoomFactor = newZoomFactor;
        _isManualZoom = true;
        _zoommedPoint = location.ToVector2();

        if (UseWebview2)
        {
            var newDelta = _zoomFactor / _oldZoomFactor;
            SetZoomFactorWeb2(_zoomFactor, _isManualZoom, newDelta);

            return false;
        }

        // update drawing regions
        CalculateDrawingRegion();

        if (requestRerender)
        {
            Invalidate();
        }

        // emit OnZoomChanged event
        OnZoomChanged?.Invoke(this, new ZoomEventArgs()
        {
            ZoomFactor = _zoomFactor,
            IsManualZoom = _isManualZoom,
            IsZoomModeChange = false,
            IsPreviewingImage = _isPreviewing,
            ChangeSource = ZoomChangeSource.Unknown,
        });

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


        // update drawing regions
        CalculateDrawingRegion();

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
    public void ShowMessage(string text, string? heading = null, int durationMs = -1, int delayMs = 0, bool forceUpdate = true)
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

        ShowMessagePrivate(text, null, durationMs, delayMs, forceUpdate);
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
    /// Select image source area.
    /// </summary>
    public void SetSourceSelection(Rectangle srcRect, bool triggerEvent = true)
    {
        srcRect.Intersect(new Rectangle(0, 0, (int)SourceWidth, (int)SourceHeight));
        _sourceSelection = srcRect;

        if (triggerEvent)
        {
            SelectionChanged?.Invoke(this, new SelectionEventArgs(ClientSelection, SourceSelection));
        }
    }


    /// <summary>
    /// Updates <see cref="ClientSelection"/> using <see cref="BHelper.GetSelection"/>.
    /// </summary>
    public void UpdateSelectionByMousePosition()
    {
        if (_mouseDownPoint == null || _mouseMovePoint == null) return;

        var cliRect = BHelper.GetSelection(_mouseDownPoint, _mouseMovePoint, SelectionAspectRatio, SourceWidth, SourceHeight, _destRect);

        // limit the selected area to the image
        cliRect.Intersect(_destRect);

        var srcRect = this.RectClientToSource(cliRect).ToRectangle();
        SetSourceSelection(srcRect, true);
    }


    /// <summary>
    /// Moves the current selection to the given location
    /// </summary>
    public void MoveSelection(PointF clientPoint)
    {
        if (!EnableSelection || _mouseDownPoint == null) return;

        var srcPoint = this.PointClientToSource(clientPoint);
        var srcMouseDownPoint = this.PointClientToSource(_mouseDownPoint.Value);


        // get the distance the source rect moved
        var dX = srcMouseDownPoint.X - _srcSelectionBeforeMoved.X;
        var dY = srcMouseDownPoint.Y - _srcSelectionBeforeMoved.Y;


        // get the new selection start point
        var newSrcPoint = new PointF(srcPoint.X - dX, srcPoint.Y - dY);


        // limit the new selection to the image source
        if (newSrcPoint.X < 0) newSrcPoint.X = 0; // left edge
        if (newSrcPoint.Y < 0) newSrcPoint.Y = 0; // right edge

        // right edge
        if (newSrcPoint.X + _srcSelectionBeforeMoved.Width > SourceWidth)
        {
            newSrcPoint.X = SourceWidth - _srcSelectionBeforeMoved.Width;
        }
        // bottom edge
        if (newSrcPoint.Y + _srcSelectionBeforeMoved.Height > SourceHeight)
        {
            newSrcPoint.Y = SourceHeight - _srcSelectionBeforeMoved.Height;
        }


        // set the final source selection after moved
        var srcRect = new Rectangle(newSrcPoint.ToPoint(), _srcSelectionBeforeMoved.Size.ToSize());
        SetSourceSelection(srcRect, true);
    }


    /// <summary>
    /// Resizes the current selection.
    /// </summary>
    public void ResizeSelection(PointF clientCursorPoint, SelectionResizerType direction)
    {
        if (!EnableSelection || _mouseDownPoint == null) return;

        var srcPoint = this.PointClientToSource(clientCursorPoint);
        var srcMouseDownPoint = this.PointClientToSource(_mouseDownPoint.Value);
        var newSrcRect = this.SourceSelection.ToRectangleF();
        var finalSrcRect = new Rectangle();


        #region 1. Get correct size and location of new selection

        var isTopDirections = direction == SelectionResizerType.Top
            || direction == SelectionResizerType.TopLeft
            || direction == SelectionResizerType.TopRight;

        var isBottomDirections = direction == SelectionResizerType.Bottom
            || direction == SelectionResizerType.BottomLeft
            || direction == SelectionResizerType.BottomRight;

        var isLeftDirections = direction == SelectionResizerType.Left
            || direction == SelectionResizerType.TopLeft
            || direction == SelectionResizerType.BottomLeft;

        var isRightDirections = direction == SelectionResizerType.Right
            || direction == SelectionResizerType.TopRight
            || direction == SelectionResizerType.BottomRight;


        // top resizers
        if (isTopDirections)
        {
            var gapY = _srcSelectionBeforeMoved.Y - srcMouseDownPoint.Y;
            var dH = srcPoint.Y - _srcSelectionBeforeMoved.Y + gapY;

            newSrcRect.Y = _srcSelectionBeforeMoved.Y + dH;
            newSrcRect.Height = _srcSelectionBeforeMoved.Height - dH;
        }

        // right resizers
        if (isRightDirections)
        {
            var gapX = _srcSelectionBeforeMoved.Right - srcMouseDownPoint.X;
            var dW = srcPoint.X - _srcSelectionBeforeMoved.Right + gapX;

            newSrcRect.Width = _srcSelectionBeforeMoved.Width + dW;
        }

        // bottom resizers
        if (isBottomDirections)
        {
            var gapY = _srcSelectionBeforeMoved.Bottom - srcMouseDownPoint.Y;
            var dH = srcPoint.Y - _srcSelectionBeforeMoved.Bottom + gapY;

            newSrcRect.Height = _srcSelectionBeforeMoved.Height + dH;
        }

        // left resizers
        if (isLeftDirections)
        {
            var gapX = _srcSelectionBeforeMoved.X - srcMouseDownPoint.X;
            var dW = srcPoint.X - _srcSelectionBeforeMoved.X + gapX;

            newSrcRect.X = _srcSelectionBeforeMoved.X + dW;
            newSrcRect.Width = _srcSelectionBeforeMoved.Width - dW;
        }


        if (newSrcRect.Width < 0) newSrcRect.Width = 0;
        if (newSrcRect.Height < 0) newSrcRect.Height = 0;


        // limit the selected client rect to the image source
        newSrcRect.Intersect(new(0, 0, SourceWidth, SourceHeight));

        #endregion // 1. Get correct size and location of new selection


        #region 2. Handle Aspect ratio

        // update selection size according to the ratio
        if (SelectionAspectRatio.Width > 0 && SelectionAspectRatio.Height > 0)
        {
            var wRatio = SelectionAspectRatio.Width / SelectionAspectRatio.Height;
            var hRatio = SelectionAspectRatio.Height / SelectionAspectRatio.Width;

            if (wRatio > hRatio)
            {
                if (direction == SelectionResizerType.Top
                    || direction == SelectionResizerType.TopRight
                    || direction == SelectionResizerType.TopLeft
                    || direction == SelectionResizerType.Bottom
                    || direction == SelectionResizerType.BottomLeft
                    || direction == SelectionResizerType.BottomRight)
                {
                    newSrcRect.Width = newSrcRect.Height / hRatio;

                    if (newSrcRect.Right >= SourceWidth)
                    {
                        var maxWidth = SourceWidth - newSrcRect.X; ;
                        newSrcRect.Width = maxWidth;
                        newSrcRect.Height = maxWidth * hRatio;
                    }
                }
                else
                {
                    newSrcRect.Height = newSrcRect.Width / wRatio;
                }


                if (newSrcRect.Bottom >= SourceHeight)
                {
                    var maxHeight = SourceHeight - newSrcRect.Y;
                    newSrcRect.Width = maxHeight * wRatio;
                    newSrcRect.Height = maxHeight;
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
                    newSrcRect.Height = newSrcRect.Width / wRatio;

                    if (newSrcRect.Bottom >= SourceHeight)
                    {
                        var maxHeight = SourceHeight - newSrcRect.Y;
                        newSrcRect.Width = maxHeight * wRatio;
                        newSrcRect.Height = maxHeight;
                    }
                }
                else
                {
                    newSrcRect.Width = newSrcRect.Height / hRatio;
                }


                if (newSrcRect.Right >= SourceWidth)
                {
                    var maxWidth = SourceWidth - newSrcRect.X;
                    newSrcRect.Width = maxWidth;
                    newSrcRect.Height = maxWidth * hRatio;
                }
            }
        }

        #endregion // 2. Handle Aspect ratio


        #region 3. Convert float values to int

        // round the values of location & size
        if (isTopDirections || isLeftDirections)
        {
            finalSrcRect = new Rectangle(
                (int)newSrcRect.X, (int)newSrcRect.Y,
                (int)Math.Ceiling(newSrcRect.Width),
                (int)Math.Ceiling(newSrcRect.Height));
        }
        else
        {
            finalSrcRect = new Rectangle(
                (int)newSrcRect.X, (int)newSrcRect.Y,
                (int)Math.Round(newSrcRect.Width),
                (int)Math.Round(newSrcRect.Height));
        }

        #endregion // 3. Convert float values to int


        #region 4. Handle small size (<= 1px)

        // limit the size to 1 pixel
        if (finalSrcRect.Width <= 1) finalSrcRect.Width = 1;
        if (finalSrcRect.Height <= 1) finalSrcRect.Height = 1;


        // make sure selection rect is not moved when size <= 1

        // top, top-left, left
        if (direction == SelectionResizerType.Top
            || direction == SelectionResizerType.TopLeft
            || direction == SelectionResizerType.Left)
        {
            if (finalSrcRect.Width <= 1)
            {
                finalSrcRect.X = (int)_srcSelectionBeforeMoved.Right - 1;
            }
            if (finalSrcRect.Height <= 1)
            {
                finalSrcRect.Y = (int)_srcSelectionBeforeMoved.Bottom - 1;
            }
        }

        // right, bottom-right, bottom
        else if (direction == SelectionResizerType.Right
            || direction == SelectionResizerType.BottomRight
            || direction == SelectionResizerType.Bottom)
        {
            if (finalSrcRect.Width <= 1)
            {
                finalSrcRect.X = (int)_srcSelectionBeforeMoved.X;
            }
            if (finalSrcRect.Height <= 1)
            {
                finalSrcRect.Y = (int)_srcSelectionBeforeMoved.Y;
            }
        }

        // top-right
        else if (direction == SelectionResizerType.TopRight)
        {
            if ((finalSrcRect.Width <= 1 && finalSrcRect.Height <= 1)
                || (finalSrcRect.Width > 1 && finalSrcRect.Height <= 1))
            {
                finalSrcRect.X = (int)_srcSelectionBeforeMoved.Left;
                finalSrcRect.Y = (int)_srcSelectionBeforeMoved.Bottom - 1;
            }
            else if (finalSrcRect.Width <= 1 && finalSrcRect.Height > 1)
            {
                finalSrcRect.X = (int)_srcSelectionBeforeMoved.Left;
            }
        }
        // bottom-left
        else
        {
            if (finalSrcRect.Width <= 1)
            {
                finalSrcRect.X = (int)_srcSelectionBeforeMoved.Right - 1;
            }
        }

        #endregion // 4. Handle small size (<= 1px)


        SetSourceSelection(finalSrcRect, true);
    }


    /// <summary>
    /// Load image.
    /// </summary>
    public void SetImage(IgImgData? imgData,
        uint frameIndex = 0,
        bool resetZoom = true,
        float initOpacity = 0.5f,
        float opacityStep = 0.05f,
        bool isForPreview = false,
        bool autoAnimate = true,
        ImgTransform? transforms = null,
        ColorChannels channels = ColorChannels.RGBA)
    {
        // reset variables
        _imageDrawingState = ImageDrawingState.NotStarted;
        _animationSource = AnimationSource.None;
        _animatorSource = AnimatorSource.None;
        _isPreviewing = isForPreview;
        _sourceSelection = default;
        _isColorInverted = false;


        // disable animations
        StopCurrentAnimator();
        DisposeImageResources();


        // emit OnImageChanging event
        ImageLoading?.Invoke(this, EventArgs.Empty);

        // Check and preprocess image info
        LoadImageData(imgData, frameIndex, autoAnimate);

        if (imgData == null || imgData.IsImageNull)
        {
            Refresh(resetZoom);
        }
        else
        {
            // initialize animator if the image source is AnimatedImage
            CreateAnimatorFromSource(imgData);


            // viewing single frame of animated image
            if (imgData.Source is AnimatedImg animatedImg && !autoAnimate)
            {
                var frame = animatedImg.GetFrame((int)frameIndex);
                _wicImage = BHelper.ToWicBitmapSource(frame.Bitmap as Bitmap);
            }
            // viewing single frame of animated GIF
            else if (imgData.Source is Bitmap bmp && !autoAnimate)
            {
                bmp.SetActiveTimeFrame((int)frameIndex);
                _wicImage = BHelper.ToWicBitmapSource(bmp);
            }
            // viewing non-animated multiple frames
            else if (imgData?.Source is WicBitmapDecoder decoder)
            {
                _wicImage = decoder.GetFrame((int)frameIndex);
            }
            // viewing single frame
            else
            {
                _wicImage = imgData.Image;
            }

            // apply color channels filter
            if (channels != ColorChannels.RGBA)
            {
                // use original image to create color channel filter
                FilterColorChannels(channels, false);
            }
            else
            {
                _d2dImage = DXHelper.ToD2D1Bitmap(Device, _wicImage);
            }


            // apply transformations
            if (transforms != null)
            {
                _ = RotateImage(transforms.Rotation, false);
                _ = FlipImage(transforms.Flips, false);
            }

            Source = ImageSource.Direct2D;


            // start drawing
            _imageDrawingState = ImageDrawingState.Drawing;
            if (CanImageAnimate && Source != ImageSource.Null && autoAnimate)
            {
                SetZoomMode();
                StartAnimator();
            }
            else
            {
                Refresh(resetZoom);
            }
        }
    }


    /// <summary>
    /// Start animating the image if it can animate, using GDI+.
    /// </summary>
    public void StartAnimator()
    {
        if (IsImageAnimating || !CanImageAnimate || Source == ImageSource.Null)
            return;

        try
        {
            _imgAnimator?.Animate();
            IsImageAnimating = true;
        }
        catch (Exception) { }
    }


    /// <summary>
    /// Stop animating the image
    /// </summary>
    public void StopCurrentAnimator()
    {
        _imgAnimator?.StopAnimate();
        IsImageAnimating = false;
    }


    /// <summary>
    /// Rotates the image.
    /// </summary>
    public bool RotateImage(float degree, bool requestRerender = true)
    {
        if (_d2dImage == null || degree == 0 || degree == 360 || IsImageAnimating) return false;

        // create effect
        using var effect = Device.CreateEffect(Direct2DEffects.CLSID_D2D12DAffineTransform);
        effect.SetInput(_d2dImage, 0);


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
        DXHelper.DisposeD2D1Bitmap(ref _d2dImage);
        _d2dImage = effect.GetD2D1Bitmap1(Device);

        // update new source size
        var newSize = _d2dImage.GetSize();
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
        if (_d2dImage == null || IsImageAnimating) return false;

        // create effect
        using var effect = Device.CreateEffect(Direct2DEffects.CLSID_D2D12DAffineTransform);
        effect.SetInput(_d2dImage, 0);


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
        DXHelper.DisposeD2D1Bitmap(ref _d2dImage);
        _d2dImage = effect.GetD2D1Bitmap1(Device);

        // render the transformation
        if (requestRerender)
        {
            Refresh(resetZoom: false);
        }

        return true;
    }


    /// <summary>
    /// Inverts image colors.
    /// </summary>
    public bool InvertColor(bool requestRerender = true)
    {
        if (_d2dImage == null || IsImageAnimating) return false;

        // create effect
        using var effect = Device.CreateEffect(Direct2DEffects.CLSID_D2D1Invert);
        effect.SetInput(_d2dImage, 0);

        // apply the transformation
        DXHelper.DisposeD2D1Bitmap(ref _d2dImage);
        _d2dImage = effect.GetD2D1Bitmap1(Device, false);


        // render the transformation
        if (requestRerender)
        {
            Refresh(resetZoom: false);
        }

        _isColorInverted = !_isColorInverted;

        return true;
    }


    /// <summary>
    /// Filters image color channels.
    /// </summary>
    public bool FilterColorChannels(ColorChannels colors, bool requestRerender = true)
    {
        if (_wicImage == null || IsImageAnimating) return false;

        // create Direct2D bitmap from the original WIC bitmap
        using var oriD2dImage = DXHelper.ToD2D1Bitmap(Device, _wicImage);

        // create effect
        using var effect = Device.CreateEffect(Direct2DEffects.CLSID_D2D1ColorMatrix);
        effect.SetInput(oriD2dImage, 0);


        var redOnly = colors.HasFlag(ColorChannels.R)
            && !colors.HasFlag(ColorChannels.G)
            && !colors.HasFlag(ColorChannels.B);
        var greenOnly = colors.HasFlag(ColorChannels.G)
            && !colors.HasFlag(ColorChannels.R)
            && !colors.HasFlag(ColorChannels.B);
        var blueOnly = colors.HasFlag(ColorChannels.B)
            && !colors.HasFlag(ColorChannels.G)
            && !colors.HasFlag(ColorChannels.R);
        var alphaOnly = colors == ColorChannels.A;
        var ignoreAlpha = alphaOnly || !colors.HasFlag(ColorChannels.A);


        var red = !alphaOnly && colors.HasFlag(ColorChannels.R) ? 1f : 0f;
        var green = !alphaOnly && colors.HasFlag(ColorChannels.G) ? 1f : 0f;
        var blue = !alphaOnly && colors.HasFlag(ColorChannels.B) ? 1f : 0f;

        var mRed = redOnly ? 1f : 0f;
        var mGreen = greenOnly ? 1f : 0f;
        var mBlue = blueOnly ? 1f : 0f;
        var mAlpha = alphaOnly ? 1f : 0f;


        var matrix = new D2D_MATRIX_5X4_F()
        {
            #pragma warning disable format
            _11 = red,      _12 = mRed,     _13 = mRed,     _14 = 0,
            _21 = mGreen,   _22 = green,    _23 = mGreen,   _24 = 0,
            _31 = mBlue,    _32 = mBlue,    _33 = blue,     _34 = 0,
            _41 = 0,        _42 = 0,        _43 = 0,        _44 = 1f,
            _51 = mAlpha,   _52 = mAlpha,   _53 = mAlpha,   _54 = 0,
            #pragma warning restore format
        };
        effect.SetValue((int)D2D1_COLORMATRIX_PROP.D2D1_COLORMATRIX_PROP_COLOR_MATRIX, matrix);


        // apply the transformation
        DXHelper.DisposeD2D1Bitmap(ref _d2dImage);
        _d2dImage = effect.GetD2D1Bitmap1(Device, ignoreAlpha);


        // update new source size
        var newSize = _d2dImage.GetSize();
        var isSizeChanged = newSize.width != SourceWidth || newSize.height != SourceHeight;
        SourceWidth = newSize.width;
        SourceHeight = newSize.height;

        // render the transformation
        if (requestRerender)
        {
            Refresh(resetZoom: isSizeChanged);
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
            return _d2dImage.GetPixelColor(Device, x, y);
        }

        return Color.Transparent;
    }


    /// <summary>
    /// Gets the <see cref="WicBitmapSource"/> being rendered.
    /// </summary>
    public WicBitmapSource? GetRenderedBitmap()
    {
        var wicBmp = _d2dImage.ToWicBitmapSource(Device);

        return wicBmp;
    }


    /// <summary>
    /// Disposes and set all image resources to <c>null</c>.
    /// </summary>
    public void DisposeImageResources()
    {
        Source = ImageSource.Null;

        // only dispose Direct2D resources, don't dispose _wicImage here
        DXHelper.DisposeD2D1Bitmap(ref _d2dImage);

        DisposeAnimator();
    }

    #endregion // Public methods


    // Private methods
    #region Private methods

    /// <summary>
    /// Loads image data.
    /// </summary>
    private void LoadImageData(IgImgData? imgData, uint frameIndex, bool autoAnimate)
    {
        SourceWidth = 0;
        SourceHeight = 0;
        CanImageAnimate = imgData?.CanAnimate ?? false;
        HasAlphaPixels = imgData?.HasAlpha ?? false;


        if (imgData?.Source is AnimatedImg animatedImg)
        {
            var frame = animatedImg.GetFrame((int)frameIndex);
            if (frame?.Bitmap is IDisposable src)
            {
                SourceWidth = (src as dynamic).Width;
                SourceHeight = (src as dynamic).Height;

                _animatorSource = AnimatorSource.ImageAnimator;
            }
        }
        else if (imgData?.Source is Bitmap bmp)
        {
            bmp.SetActiveTimeFrame((int)frameIndex);
            SourceWidth = bmp.Width;
            SourceHeight = bmp.Height;

            if (CanImageAnimate)
            {
                _animatorSource = AnimatorSource.GifAnimator;
            }
        }
        else if (imgData?.Source is WicBitmapDecoder decoder)
        {
            var size = decoder.GetFrame((int)frameIndex).Size;

            SourceWidth = size.Width;
            SourceHeight = size.Height;
        }
        else
        {
            SourceWidth = imgData?.Image?.Width ?? 0;
            SourceHeight = imgData?.Image?.Height ?? 0;
        }


        var exceedMaxDimention = SourceWidth > Const.MAX_IMAGE_DIMENSION
            || SourceHeight > Const.MAX_IMAGE_DIMENSION;

        UseHardwareAcceleration = !exceedMaxDimention;
    }


    /// <summary>
    /// Resets the current image animator.
    /// </summary>
    private void CreateAnimatorFromSource(IgImgData? imgData)
    {
        if (_animatorSource == AnimatorSource.None) return;
        DisposeAnimator();


        if (imgData?.Source is AnimatedImg animatedImg)
        {
            _imgAnimator = new AnimatedImgAnimator(animatedImg);
        }
        else if (imgData?.Source is Bitmap bmp && CanImageAnimate)
        {
            _imgAnimator = new GifAnimator(bmp);
        }

        _imgAnimator.FrameChanged += ImgAnimator_FrameChanged;
    }


    /// <summary>
    /// Disposes the current image animator.
    /// </summary>
    private void DisposeAnimator()
    {
        if (_imgAnimator != null)
        {
            _imgAnimator.FrameChanged -= ImgAnimator_FrameChanged;
            _imgAnimator.Dispose();
            _imgAnimator = null;
        }
    }

    private void ImgAnimator_FrameChanged(object? sender, EventArgs e)
    {
#if DEBUG
        Trace.WriteLine(">>>>>>>>>>>> ImgAnimator_FrameChanged");
#endif

        if (InvokeRequired)
        {
            Invoke(delegate { ImgAnimator_FrameChanged(sender, e); });
            return;
        }

        if (!IsImageAnimating || _animatorSource == AnimatorSource.None) return;
        if (sender is IDisposable src)
        {
            DXHelper.DisposeD2D1Bitmap(ref _d2dImage);

            if (src is Bitmap bmp)
            {
                using var wicSrc = BHelper.ToWicBitmapSource(bmp);
                _d2dImage = DXHelper.ToD2D1Bitmap(Device, wicSrc);
            }
            else if (src is WicBitmapSource wicSrc)
            {
                _d2dImage = DXHelper.ToD2D1Bitmap(Device, wicSrc);
            }

            Source = ImageSource.Direct2D;
            UseHardwareAcceleration = true;

            Invalidate();
        }
    }


    /// <summary>
    /// Disposes and set all checkerboard brushes to <c>null</c>.
    /// </summary>
    private void DisposeCheckerboardBrushes()
    {
        _checkerboardBrushD2D?.Dispose();
        _checkerboardBrushD2D = null;
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
    private async void ShowMessagePrivate(string text, string? heading = null, int durationMs = -1, int delayMs = 0, bool forceUpdate = true)
    {
        if (durationMs == 0) return;

        var token = _msgTokenSrc?.Token ?? default;

        try
        {
            if (delayMs > 0)
            {
                await Task.Delay(delayMs, token);
            }

            TextHeading = heading ?? string.Empty;
            Text = text;

            if (UseWebview2) ShowWeb2Message(Text, TextHeading);
            if (forceUpdate) Invalidate();

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

            if (UseWebview2) ShowWeb2Message(Text, TextHeading);
            if (forceUpdate) Invalidate();
        }
    }


    #endregion // Private methods


}
