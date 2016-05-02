using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ImageGlass
{
    // Cyotek ImageBox
    // Copyright (c) 2010-2015 Cyotek Ltd.
    // http://cyotek.com
    // http://cyotek.com/blog/tag/imagebox

    // Licensed under the MIT License. See license.txt for the full text.

    // If you use this control in your applications, attribution, donations or contributions are welcome.

    /// <summary>
    ///   Component for displaying images with support for scrolling and zooming.
    /// </summary>
    [DefaultProperty("Image")]
    [ToolboxBitmap(typeof(ImageBox), "ImageBox.bmp")]
    [ToolboxItem(true)]
    /* [Designer("ImageGlass.ImageBox.Design.ImageBoxDesigner", ImageGlass.ImageBox.ImageBox.Design.dll, PublicKeyToken=58daa28b0b2de221")] */
    public class ImageBox : Control
    {
        #region Instance Fields

        private BorderStyle _borderStyle;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the BorderStyle property is changed
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler BorderStyleChanged;

        #endregion

        #region Overridden Properties

        /// <summary>
        /// Gets the required creation parameters when the control handle is created.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams;

                createParams = base.CreateParams;

                switch (_borderStyle)
                {
                    case BorderStyle.FixedSingle:
                        createParams.Style |= NativeMethods.WS_BORDER;
                        break;
                    case BorderStyle.Fixed3D:
                        createParams.ExStyle |= NativeMethods.WS_EX_CLIENTEDGE;
                        break;
                }

                return createParams;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates the border style for the control.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(typeof(BorderStyle), "Fixed3D")]
        public virtual BorderStyle BorderStyle
        {
            get { return _borderStyle; }
            set
            {
                if (this.BorderStyle != value)
                {
                    _borderStyle = value;

                    this.OnBorderStyleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets value whether the image can animate or not
        /// </summary>
        public bool CanAnimate
        {
            get { return ImageAnimator.CanAnimate(Image); }
        }

        #endregion

        #region Protected Members

        /// <summary>
        ///   Raises the <see cref="BorderStyleChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   An <see cref="T:System.EventArgs" /> that contains the event data.
        /// </param>
        protected virtual void OnBorderStyleChanged(EventArgs e)
        {
            EventHandler handler;

            base.UpdateStyles();

            handler = this.BorderStyleChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
        #region Constants

        private const int MaxZoom = 3500;

        private const int MinZoom = 1;

        private const int SelectionDeadZone = 5;

        #endregion

        #region Instance Fields

        private bool _allowClickZoom;

        private bool _allowDoubleClick;

        private bool _allowZoom;

        private bool _autoCenter;

        private bool _autoPan;

        private int _dropShadowSize;

        private int _gridCellSize;

        private Color _gridColor;

        private Color _gridColorAlternate;

        private ImageBoxGridDisplayMode _gridDisplayMode;

        private ImageBoxGridScale _gridScale;

        private Bitmap _gridTile;

        private Image _image;

        private Color _imageBorderColor;

        private ImageBoxBorderStyle _imageBorderStyle;

        private InterpolationMode _interpolationMode;

        private bool _invertMouse;

        private bool _isPanning;

        private bool _limitSelectionToImage;

        private Color _pixelGridColor;

        private int _pixelGridThreshold;

        private bool _scaleText;

        private Color _selectionColor;

        private ImageBoxSelectionMode _selectionMode;

        private RectangleF _selectionRegion;

        private bool _shortcutsEnabled;

        private bool _showPixelGrid;

        private ImageBoxSizeMode _sizeMode;

        private Point _startMousePosition;

        private Point _startScrollPosition;

        private ContentAlignment _textAlign;

        private Color _textBackColor;

        private ImageBoxGridDisplayMode _textDisplayMode;

        private Padding _textPadding;

        private Brush _texture;

        private int _updateCount;

        private bool _virtualMode;

        private Size _virtualSize;

        private int _zoom;

        private ImageBoxZoomLevelCollection _zoomLevels;

        #endregion

        #region Public Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="ImageBox" /> class.
        /// </summary>
        public ImageBox()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.StandardDoubleClick, false);

            _vScrollBar = new VScrollBar
            {
                Visible = false
            };
            // ReSharper disable once HeapView.DelegateAllocation
            _vScrollBar.Scroll += this.ScrollBarScrollHandler;

            _hScrollBar = new HScrollBar
            {
                Visible = false
            };
            // ReSharper disable once HeapView.DelegateAllocation
            _hScrollBar.Scroll += this.ScrollBarScrollHandler;

            this.Controls.Add(_vScrollBar);
            this.Controls.Add(_hScrollBar);

            this.HorizontalScroll = new ImageBoxScrollProperties(_hScrollBar);
            this.VerticalScroll = new ImageBoxScrollProperties(_vScrollBar);

            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            this.BorderStyle = BorderStyle.Fixed3D;
            this.AllowZoom = true;
            this.LimitSelectionToImage = true;
            this.DropShadowSize = 3;
            this.ImageBorderStyle = ImageBoxBorderStyle.None;
            this.BackColor = Color.White;
            this.AutoSize = false;
            this.GridScale = ImageBoxGridScale.Small;
            this.GridDisplayMode = ImageBoxGridDisplayMode.Client;
            this.GridColor = Color.Gainsboro;
            this.GridColorAlternate = Color.White;
            this.GridCellSize = 8;
            this.AutoPan = true;
            this.InterpolationMode = InterpolationMode.NearestNeighbor;
            this.AutoCenter = true;
            this.SelectionColor = SystemColors.Highlight;
            this.ActualSize();
            this.ShortcutsEnabled = true;
            this.ZoomLevels = ImageBoxZoomLevelCollection.Default;
            this.ImageBorderColor = SystemColors.ControlDark;
            this.PixelGridColor = Color.DimGray;
            this.PixelGridThreshold = 5;
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.TextBackColor = Color.Transparent;
            this.TextDisplayMode = ImageBoxGridDisplayMode.Client;
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        #endregion

        #region Events

        /// <summary>
        ///   Occurs when the AllowClickZoom property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler AllowClickZoomChanged;

        /// <summary>
        ///   Occurs when the AllowDoubleClick property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler AllowDoubleClickChanged;

        /// <summary>
        ///   Occurs when the AllowZoom property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler AllowZoomChanged;

        /// <summary>
        ///   Occurs when the AutoCenter property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler AutoCenterChanged;

        /// <summary>
        ///   Occurs when the AutoPan property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler AutoPanChanged;

        /// <summary>
        ///   Occurs when the DropShadowSize property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler DropShadowSizeChanged;

        /// <summary>
        ///   Occurs when the GridSizeCell property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler GridCellSizeChanged;

        /// <summary>
        ///   Occurs when the GridColorAlternate property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler GridColorAlternateChanged;

        /// <summary>
        ///   Occurs when the GridColor property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler GridColorChanged;

        /// <summary>
        ///   Occurs when the GridDisplayMode property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler GridDisplayModeChanged;

        /// <summary>
        ///   Occurs when the GridScale property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler GridScaleChanged;

        /// <summary>
        ///   Occurs when the ImageBorderColor property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler ImageBorderColorChanged;

        /// <summary>
        ///   Occurs when the ImageBorderStyle property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler ImageBorderStyleChanged;

        /// <summary>
        ///   Occurs when the Image property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler ImageChanged;

        /// <summary>
        ///   Occurs when the InterpolationMode property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler InterpolationModeChanged;

        /// <summary>
        ///   Occurs when the InvertMouse property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler InvertMouseChanged;

        /// <summary>
        ///   Occurs when the LimitSelectionToImage property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler LimitSelectionToImageChanged;

        /// <summary>
        ///   Occurs when panning the control completes.
        /// </summary>
        [Category("Action")]
        public event EventHandler PanEnd;

        /// <summary>
        ///   Occurs when panning the control starts.
        /// </summary>
        [Category("Action")]
        public event EventHandler PanStart;

        /// <summary>
        ///   Occurs when the PixelGridColor property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler PixelGridColorChanged;

        /// <summary>
        /// Occurs when the PixelGridThreshold property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler PixelGridThresholdChanged;

        /// <summary>
        /// Occurs when the ScaleText property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler ScaleTextChanged;

        /// <summary>
        ///   Occurs when a selection region has been defined
        /// </summary>
        [Category("Action")]
        public event EventHandler<EventArgs> Selected;

        /// <summary>
        ///   Occurs when the user starts to define a selection region.
        /// </summary>
        [Category("Action")]
        public event EventHandler<ImageBoxCancelEventArgs> Selecting;

        /// <summary>
        ///   Occurs when the SelectionColor property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler SelectionColorChanged;

        /// <summary>
        ///   Occurs when the SelectionMode property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler SelectionModeChanged;

        /// <summary>
        ///   Occurs when the SelectionRegion property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler SelectionRegionChanged;

        /// <summary>
        ///   Occurs when the ShortcutsEnabled property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler ShortcutsEnabledChanged;

        /// <summary>
        ///   Occurs when the ShowPixelGrid property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler ShowPixelGridChanged;

        /// <summary>
        /// Occurs when the SizeMode property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler SizeModeChanged;

        /// <summary>
        ///   Occurs when the SizeToFit property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler SizeToFitChanged;

        /// <summary>
        /// Occurs when the TextAlign property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler TextAlignChanged;

        /// <summary>
        /// Occurs when the TextBackColor property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler TextBackColorChanged;

        /// <summary>
        /// Occurs when the TextDisplayMode property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler TextDisplayModeChanged;

        /// <summary>
        /// Occurs when the TextPadding property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler TextPaddingChanged;

        /// <summary>
        ///   Occurs when virtual painting should occur
        /// </summary>
        [Category("Appearance")]
        public event PaintEventHandler VirtualDraw;

        /// <summary>
        ///   Occurs when the VirtualMode property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler VirtualModeChanged;

        /// <summary>
        ///   Occurs when the VirtualSize property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler VirtualSizeChanged;

        /// <summary>
        ///   Occurs when the Zoom property is changed.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler ZoomChanged;

        /// <summary>
        ///   Occurs when the ZoomLevels property is changed
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler ZoomLevelsChanged;

        /// <summary>
        /// Occurs when then a zoom action is performed.
        /// </summary>
        [Category("Action")]
        public event EventHandler<ImageBoxZoomEventArgs> Zoomed;

        #endregion

        #region Public Class Members

        /// <summary>
        ///   Creates a bitmap image containing a 2x2 grid using the specified cell size and colors.
        /// </summary>
        /// <param name="cellSize">Size of the cell.</param>
        /// <param name="cellColor">Cell color.</param>
        /// <param name="alternateCellColor">Alternate cell color.</param>
        /// <returns></returns>
        public static Bitmap CreateCheckerBoxTile(int cellSize, Color cellColor, Color alternateCellColor)
        {
            Bitmap result;
            int width;
            int height;

            // draw the tile
            width = cellSize * 2;
            height = cellSize * 2;
            result = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(result))
            {
                using (Brush brush = new SolidBrush(cellColor))
                {
                    g.FillRectangle(brush, new Rectangle(cellSize, 0, cellSize, cellSize));
                    g.FillRectangle(brush, new Rectangle(0, cellSize, cellSize, cellSize));
                }

                using (Brush brush = new SolidBrush(alternateCellColor))
                {
                    g.FillRectangle(brush, new Rectangle(0, 0, cellSize, cellSize));
                    g.FillRectangle(brush, new Rectangle(cellSize, cellSize, cellSize, cellSize));
                }
            }

            return result;
        }

        /// <summary>
        ///   Creates a checked tile texture using default values.
        /// </summary>
        /// <returns></returns>
        public static Bitmap CreateCheckerBoxTile()
        {
            return ImageBox.CreateCheckerBoxTile(8, Color.Gainsboro, Color.WhiteSmoke);
        }

        #endregion

        #region Overridden Properties

        /// <summary>
        ///   Specifies if the control should auto size to fit the image contents.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>
        /// </returns>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(false)]
        public override bool AutoSize
        {
            get { return base.AutoSize; }
            set
            {
                if (base.AutoSize != value)
                {
                    base.AutoSize = value;

                    this.AdjustLayout();
                }
            }
        }

        /// <summary>
        ///   Gets or sets the background color for the control.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///   A <see cref="T:System.Drawing.Color" /> that represents the background color of the control. The default is the value of the
        ///   <see
        ///     cref="P:System.Windows.Forms.Control.DefaultBackColor" />
        ///   property.
        /// </returns>
        [DefaultValue(typeof(Color), "White")]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        /// <summary>
        ///   Gets or sets the background image displayed in the control.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///   An <see cref="T:System.Drawing.Image" /> that represents the image to display in the background of the control.
        /// </returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Image BackgroundImage
        {
            get { return base.BackgroundImage; }
            set { base.BackgroundImage = value; }
        }

        /// <summary>
        ///   Gets or sets the background image layout as defined in the <see cref="T:System.Windows.Forms.ImageLayout" /> enumeration.
        /// </summary>
        /// <value>The background image layout.</value>
        /// <returns>
        ///   One of the values of <see cref="T:System.Windows.Forms.ImageLayout" /> (
        ///   <see
        ///     cref="F:System.Windows.Forms.ImageLayout.Center" />
        ///   , <see cref="F:System.Windows.Forms.ImageLayout.None" />,
        ///   <see
        ///     cref="F:System.Windows.Forms.ImageLayout.Stretch" />
        ///   , <see cref="F:System.Windows.Forms.ImageLayout.Tile" />, or
        ///   <see
        ///     cref="F:System.Windows.Forms.ImageLayout.Zoom" />
        ///   ). <see cref="F:System.Windows.Forms.ImageLayout.Tile" /> is the default value.
        /// </returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ImageLayout BackgroundImageLayout
        {
            get { return base.BackgroundImageLayout; }
            set { base.BackgroundImageLayout = value; }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        ///   Retrieves the size of a rectangular area into which a control can be fitted.
        /// </summary>
        /// <param name="proposedSize">The custom-sized area for a control.</param>
        /// <returns>
        ///   An ordered pair of type <see cref="T:System.Drawing.Size" /> representing the width and height of a rectangle.
        /// </returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            Size size;

            if (!this.ViewSize.IsEmpty)
            {
                int width;
                int height;

                // get the size of the image
                width = this.ScaledImageWidth;
                height = this.ScaledImageHeight;

                // add an offset based on padding
                width += this.Padding.Horizontal;
                height += this.Padding.Vertical;

                // add an offset based on the border style
                width += this.GetImageBorderOffset();
                height += this.GetImageBorderOffset();

                size = new Size(width, height);
            }
            else
            {
                size = base.GetPreferredSize(proposedSize);
            }

            return size;
        }

        /// <summary>
        ///   Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.IsAnimating)
                {
                    // ReSharper disable once HeapView.DelegateAllocation
                    ImageAnimator.StopAnimate(this.Image, this.OnFrameChangedHandler);
                }

                if (_hScrollBar != null)
                {
                    this.Controls.Remove(_hScrollBar);
                    // ReSharper disable once HeapView.DelegateAllocation
                    _hScrollBar.Scroll -= this.ScrollBarScrollHandler;
                    _hScrollBar.Dispose();
                }

                if (_vScrollBar != null)
                {
                    this.Controls.Remove(_vScrollBar);
                    // ReSharper disable once HeapView.DelegateAllocation
                    _vScrollBar.Scroll -= this.ScrollBarScrollHandler;
                    _vScrollBar.Dispose();
                }

                if (_texture != null)
                {
                    _texture.Dispose();
                    _texture = null;
                }

                if (_gridTile != null)
                {
                    _gridTile.Dispose();
                    _gridTile = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///   Determines whether the specified key is a regular input key or a special key that requires preprocessing.
        /// </summary>
        /// <param name="keyData">
        ///   One of the <see cref="T:System.Windows.Forms.Keys" /> values.
        /// </param>
        /// <returns>
        ///   true if the specified key is a regular input key; otherwise, false.
        /// </returns>
        protected override bool IsInputKey(Keys keyData)
        {
            bool result;

            if ((keyData & Keys.Right) == Keys.Right | (keyData & Keys.Left) == Keys.Left | (keyData & Keys.Up) == Keys.Up | (keyData & Keys.Down) == Keys.Down)
            {
                result = true;
            }
            else
            {
                result = base.IsInputKey(keyData);
            }

            return result;
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.BackColorChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   An <see cref="T:System.EventArgs" /> that contains the event data.
        /// </param>
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            this.Invalidate();
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.DockChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   An <see cref="T:System.EventArgs" /> that contains the event data.
        /// </param>
        protected override void OnDockChanged(EventArgs e)
        {
            base.OnDockChanged(e);

            if (this.Dock != DockStyle.None)
            {
                this.AutoSize = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.FontChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            this.Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.ForeColorChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);

            this.Invalidate();
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.KeyDown" /> event.
        /// </summary>
        /// <param name="e">
        ///   A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            this.ProcessScrollingShortcuts(e);

            if (this.ShortcutsEnabled && this.AllowZoom && this.SizeMode == ImageBoxSizeMode.Normal)
            {
                this.ProcessImageShortcuts(e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.MouseDown" /> event.
        /// </summary>
        /// <param name="e">
        ///   A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!this.Focused)
            {
                this.Focus();
            }
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.MouseMove" /> event.
        /// </summary>
        /// <param name="e">
        ///   A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left)
            {
                this.ProcessPanning(e);
                this.ProcessSelection(e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.MouseUp" /> event.
        /// </summary>
        /// <param name="e">
        ///   A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            bool doNotProcessClick;

            base.OnMouseUp(e);

            doNotProcessClick = this.IsPanning || this.IsSelecting;

            if (this.IsPanning)
            {
                this.IsPanning = false;
            }

            if (this.IsSelecting)
            {
                this.EndDrag();
            }
            this.WasDragCancelled = false;

            if (!doNotProcessClick && this.AllowZoom && this.AllowClickZoom && !this.IsPanning && this.SizeMode == ImageBoxSizeMode.Normal)
            {
                if (e.Button == MouseButtons.Left && ModifierKeys == Keys.None)
                {
                    this.ProcessMouseZoom(true, e.Location);
                }
                else if (e.Button == MouseButtons.Right || (e.Button == MouseButtons.Left && ModifierKeys != Keys.None))
                {
                    this.ProcessMouseZoom(false, e.Location);
                }
            }
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.MouseWheel" /> event.
        /// </summary>
        /// <param name="e">
        ///   A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (this.AllowZoom && this.SizeMode == ImageBoxSizeMode.Normal)
            {
                int spins;

                // The MouseWheel event can contain multiple "spins" of the wheel so we need to adjust accordingly
                spins = Math.Abs(e.Delta / SystemInformation.MouseWheelScrollDelta);

                // TODO: Really should update the source method to handle multiple increments rather than calling it multiple times
                for (int i = 0; i < spins; i++)
                {
                    this.ProcessMouseZoom(e.Delta > 0, e.Location);
                }
            }
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.PaddingChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   An <see cref="T:System.EventArgs" /> that contains the event data.
        /// </param>
        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            this.AdjustLayout();
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.Paint" /> event.
        /// </summary>
        /// <param name="e">
        ///   A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.AllowPainting)
            {
                // draw the background
                this.DrawBackground(e);

                // draw the image
                if (!this.ViewSize.IsEmpty)
                {
                    this.DrawImageBorder(e.Graphics);
                }
                if (this.VirtualMode)
                {
                    this.OnVirtualDraw(e);
                }
                else if (this.Image != null)
                {
                    this.DrawImage(e.Graphics);
                }

                // draw the grid
                if (this.ShowPixelGrid && !this.VirtualMode)
                {
                    this.DrawPixelGrid(e.Graphics);
                }

                // draw the selection
                if (this.SelectionRegion != Rectangle.Empty)
                {
                    this.DrawSelection(e);
                }

                // text
                if (!string.IsNullOrEmpty(this.Text) && this.TextDisplayMode != ImageBoxGridDisplayMode.None)
                {
                    this.DrawText(e);
                }

                // scrollbar corners
                if (this.HorizontalScroll.Visible && this.VerticalScroll.Visible)
                {
                    int x;
                    int y;
                    int w;
                    int h;
                    Size clientSize;

                    clientSize = this.ClientSize;
                    w = _vScrollBar.Width;
                    h = _hScrollBar.Height;
                    x = clientSize.Width - w;
                    y = clientSize.Height - h;

                    e.Graphics.FillRectangle(SystemBrushes.Control, x, y, w, h);
                }

                base.OnPaint(e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.ParentChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   An <see cref="T:System.EventArgs" /> that contains the event data.
        /// </param>
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            this.AdjustLayout();
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.Resize" /> event.
        /// </summary>
        /// <param name="e">
        ///   An <see cref="T:System.EventArgs" /> that contains the event data.
        /// </param>
        protected override void OnResize(EventArgs e)
        {
            this.AdjustLayout();

            base.OnResize(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.TextChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            this.Invalidate();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets a value indicating whether clicking the control with the mouse will automatically zoom in or out.
        /// </summary>
        /// <value>
        ///   <c>true</c> if clicking the control allows zooming; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(false)]
        [Category("Behavior")]
        public virtual bool AllowClickZoom
        {
            get { return _allowClickZoom; }
            set
            {
                if (_allowClickZoom != value)
                {
                    _allowClickZoom = value;
                    this.OnAllowClickZoomChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the DoubleClick event can be raised.
        /// </summary>
        /// <value><c>true</c> if the DoubleClick event can be raised; otherwise, <c>false</c>.</value>
        [Category("Behavior")]
        [DefaultValue(false)]
        public virtual bool AllowDoubleClick
        {
            get { return _allowDoubleClick; }
            set
            {
                if (this.AllowDoubleClick != value)
                {
                    _allowDoubleClick = value;

                    this.OnAllowDoubleClickChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the user can change the zoom level.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the zoom level can be changed; otherwise, <c>false</c>.
        /// </value>
        [Category("Behavior")]
        [DefaultValue(true)]
        public virtual bool AllowZoom
        {
            get { return _allowZoom; }
            set
            {
                if (this.AllowZoom != value)
                {
                    _allowZoom = value;

                    this.OnAllowZoomChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the image is centered where possible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the image should be centered where possible; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(true)]
        [Category("Appearance")]
        public virtual bool AutoCenter
        {
            get { return _autoCenter; }
            set
            {
                if (_autoCenter != value)
                {
                    _autoCenter = value;
                    this.OnAutoCenterChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets if the mouse can be used to pan the control.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the control can be auto panned; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>If this property is set, the SizeToFit property cannot be used.</remarks>
        [DefaultValue(true)]
        [Category("Behavior")]
        public virtual bool AutoPan
        {
            get { return _autoPan; }
            set
            {
                if (_autoPan != value)
                {
                    _autoPan = value;
                    this.OnAutoPanChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the point at the center of the currently displayed image viewport.
        /// </summary>
        /// <value>The point at the center of the current image viewport.</value>
        [Browsable(false)]
        public Point CenterPoint
        {
            get
            {
                Rectangle viewport;

                viewport = this.GetImageViewPort();

                return new Point(viewport.Width / 2, viewport.Height / 2);
            }
        }

        /// <summary>
        ///   Gets or sets the size of the drop shadow.
        /// </summary>
        /// <value>The size of the drop shadow.</value>
        [Category("Appearance")]
        [DefaultValue(3)]
        public virtual int DropShadowSize
        {
            get { return _dropShadowSize; }
            set
            {
                if (this.DropShadowSize != value)
                {
                    _dropShadowSize = value;

                    this.OnDropShadowSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the size of the grid cells.
        /// </summary>
        /// <value>The size of the grid cells.</value>
        [Category("Appearance")]
        [DefaultValue(8)]
        public virtual int GridCellSize
        {
            get { return _gridCellSize; }
            set
            {
                if (_gridCellSize != value)
                {
                    _gridCellSize = value;
                    this.OnGridCellSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the color of primary cells in the grid.
        /// </summary>
        /// <value>The color of primary cells in the grid.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Gainsboro")]
        public virtual Color GridColor
        {
            get { return _gridColor; }
            set
            {
                if (_gridColor != value)
                {
                    _gridColor = value;
                    this.OnGridColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the color of alternate cells in the grid.
        /// </summary>
        /// <value>The color of alternate cells in the grid.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "White")]
        public virtual Color GridColorAlternate
        {
            get { return _gridColorAlternate; }
            set
            {
                if (_gridColorAlternate != value)
                {
                    _gridColorAlternate = value;
                    this.OnGridColorAlternateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the grid display mode.
        /// </summary>
        /// <value>The grid display mode.</value>
        [DefaultValue(ImageBoxGridDisplayMode.Client)]
        [Category("Appearance")]
        public virtual ImageBoxGridDisplayMode GridDisplayMode
        {
            get { return _gridDisplayMode; }
            set
            {
                if (_gridDisplayMode != value)
                {
                    _gridDisplayMode = value;
                    this.OnGridDisplayModeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the grid scale.
        /// </summary>
        /// <value>The grid scale.</value>
        [DefaultValue(typeof(ImageBoxGridScale), "Small")]
        [Category("Appearance")]
        public virtual ImageBoxGridScale GridScale
        {
            get { return _gridScale; }
            set
            {
                if (_gridScale != value)
                {
                    _gridScale = value;
                    this.OnGridScaleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the image.
        /// </summary>
        /// <value>The image.</value>
        [Category("Appearance")]
        [DefaultValue(null)]
        public virtual Image Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    // disable animations
                    if (this.IsAnimating)
                    {
                        ImageAnimator.StopAnimate(this.Image, this.OnFrameChangedHandler);
                    }

                    _image = value;
                    this.OnImageChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the color of the image border.
        /// </summary>
        /// <value>The color of the image border.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "ControlDark")]
        public virtual Color ImageBorderColor
        {
            get { return _imageBorderColor; }
            set
            {
                if (this.ImageBorderColor != value)
                {
                    _imageBorderColor = value;

                    this.OnImageBorderColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the image border style.
        /// </summary>
        /// <value>The image border style.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(ImageBoxBorderStyle), "None")]
        public virtual ImageBoxBorderStyle ImageBorderStyle
        {
            get { return _imageBorderStyle; }
            set
            {
                if (this.ImageBorderStyle != value)
                {
                    _imageBorderStyle = value;

                    this.OnImageBorderStyleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the interpolation mode.
        /// </summary>
        /// <value>The interpolation mode.</value>
        [Category("Appearance")]
        [DefaultValue(InterpolationMode.NearestNeighbor)]
        public virtual InterpolationMode InterpolationMode
        {
            get { return _interpolationMode; }
            set
            {
                if (value == InterpolationMode.Invalid)
                {
                    value = InterpolationMode.Default;
                }

                if (_interpolationMode != value)
                {
                    _interpolationMode = value;
                    this.OnInterpolationModeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the mouse should be inverted when panning the control.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the mouse should be inverted when panning the control; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(false)]
        [Category("Behavior")]
        public virtual bool InvertMouse
        {
            get { return _invertMouse; }
            set
            {
                if (_invertMouse != value)
                {
                    _invertMouse = value;
                    this.OnInvertMouseChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the image is currently being displayed at 100% zoom
        /// </summary>
        /// <value><c>true</c> if the image is currently being displayed at 100% zoom; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public virtual bool IsActualSize
        {
            get { return this.Zoom == 100; }
        }

        /// <summary>
        ///   Gets a value indicating whether this control is panning.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this control is panning; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public virtual bool IsPanning
        {
            get { return _isPanning; }
            protected set
            {
                if (_isPanning != value)
                {
                    CancelEventArgs args;

                    args = new CancelEventArgs();

                    if (value)
                    {
                        this.OnPanStart(args);
                    }
                    else
                    {
                        this.OnPanEnd(EventArgs.Empty);
                    }

                    if (!args.Cancel)
                    {
                        _isPanning = value;

                        if (value)
                        {
                            _startScrollPosition = this.AutoScrollPosition;
                            this.Cursor = Cursors.SizeAll;
                        }
                        else
                        {
                            this.Cursor = Cursors.Default;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether this a selection region is currently being drawn.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a selection region is currently being drawn; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool IsSelecting { get; protected set; }

        /// <summary>
        ///   Gets or sets a value indicating whether selection regions should be limited to the image boundaries.
        /// </summary>
        /// <value>
        ///   <c>true</c> if selection regions should be limited to image boundaries; otherwise, <c>false</c>.
        /// </value>
        [Category("Behavior")]
        [DefaultValue(true)]
        public virtual bool LimitSelectionToImage
        {
            get { return _limitSelectionToImage; }
            set
            {
                if (this.LimitSelectionToImage != value)
                {
                    _limitSelectionToImage = value;

                    this.OnLimitSelectionToImageChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the pixel grid.
        /// </summary>
        /// <value>The color of the pixel grid.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "DimGray")]
        public virtual Color PixelGridColor
        {
            get { return _pixelGridColor; }
            set
            {
                if (this.PixelGridColor != value)
                {
                    _pixelGridColor = value;

                    this.OnPixelGridColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum size of zoomed pixel's before the pixel grid will be drawn
        /// </summary>
        /// <value>The pixel grid threshold.</value>
        [Category("Behavior")]
        [DefaultValue(5)]
        public virtual int PixelGridThreshold
        {
            get { return _pixelGridThreshold; }
            set
            {
                if (this.PixelGridThreshold != value)
                {
                    _pixelGridThreshold = value;

                    this.OnPixelGridThresholdChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the font size of text is scaled according to the current zoom level.
        /// </summary>
        /// <value><c>true</c> if the size of text is scaled according to the current zoom level; otherwise, <c>false</c>.</value>
        [Category("Appearance")]
        [DefaultValue(false)]
        public virtual bool ScaleText
        {
            get { return _scaleText; }
            set
            {
                if (this.ScaleText != value)
                {
                    _scaleText = value;

                    this.OnScaleTextChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the color of selection regions.
        /// </summary>
        /// <value>
        ///   The color of selection regions.
        /// </value>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Highlight")]
        public virtual Color SelectionColor
        {
            get { return _selectionColor; }
            set
            {
                if (this.SelectionColor != value)
                {
                    _selectionColor = value;

                    this.OnSelectionColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the selection mode.
        /// </summary>
        /// <value>
        ///   The selection mode.
        /// </value>
        [Category("Behavior")]
        [DefaultValue(typeof(ImageBoxSelectionMode), "None")]
        public virtual ImageBoxSelectionMode SelectionMode
        {
            get { return _selectionMode; }
            set
            {
                if (this.SelectionMode != value)
                {
                    _selectionMode = value;

                    this.OnSelectionModeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the selection region.
        /// </summary>
        /// <value>
        ///   The selection region.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual RectangleF SelectionRegion
        {
            get { return _selectionRegion; }
            set
            {
                if (this.SelectionRegion != value)
                {
                    _selectionRegion = value;

                    this.OnSelectionRegionChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the defined shortcuts are enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> to enable the shortcuts; otherwise, <c>false</c>.
        /// </value>
        [Category("Behavior")]
        [DefaultValue(true)]
        public virtual bool ShortcutsEnabled
        {
            get { return _shortcutsEnabled; }
            set
            {
                if (this.ShortcutsEnabled != value)
                {
                    _shortcutsEnabled = value;

                    this.OnShortcutsEnabledChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a pixel grid is displayed when the control is zoomed.
        /// </summary>
        /// <value><c>true</c> if a pixel grid is displayed when the control is zoomed; otherwise, <c>false</c>.</value>
        [Category("Appearance")]
        [DefaultValue(false)]
        public virtual bool ShowPixelGrid
        {
            get { return _showPixelGrid; }
            set
            {
                if (this.ShowPixelGrid != value)
                {
                    _showPixelGrid = value;

                    this.OnShowPixelGridChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the size mode of images hosted in the control.
        /// </summary>
        /// <value>The size mode.</value>
        [Category("Behavior")]
        [DefaultValue(typeof(ImageBoxSizeMode), "Normal")]
        public virtual ImageBoxSizeMode SizeMode
        {
            get { return _sizeMode; }
            set
            {
                if (this.SizeMode != value)
                {
                    _sizeMode = value;

                    this.OnSizeModeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the control should automatically size to fit the image contents.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the control should size to fit the image contents; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(false)]
        [Category("Appearance")]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("This property is deprecated and will be removed in a future version of the component. Implementors should use the SizeMode property instead.")]
        public virtual bool SizeToFit
        {
            get { return this.SizeMode == ImageBoxSizeMode.Fit; }
            set
            {
                if ((this.SizeMode == ImageBoxSizeMode.Fit) != value)
                {
                    this.SizeMode = value ? ImageBoxSizeMode.Fit : ImageBoxSizeMode.Normal;
                    this.OnSizeToFitChanged(EventArgs.Empty);

                    if (value)
                    {
                        this.AutoPan = false;
                    }
                    else
                    {
                        this.ActualSize();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the text on the control.
        /// </summary>
        /// <value>One of the <see cref="ContentAlignment"/> values. The default is <c>MiddleCenter</c>.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(ContentAlignment), "MiddleCenter")]
        public virtual ContentAlignment TextAlign
        {
            get { return _textAlign; }
            set
            {
                if (this.TextAlign != value)
                {
                    _textAlign = value;

                    this.OnTextAlignChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the text background.
        /// </summary>
        /// <value>The color of the text background.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Transparent")]
        public virtual Color TextBackColor
        {
            get { return _textBackColor; }
            set
            {
                if (this.TextBackColor != value)
                {
                    _textBackColor = value;

                    this.OnTextBackColorChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the text display mode.
        /// </summary>
        /// <value>The text display mode.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(ImageBoxGridDisplayMode), "Client")]
        public virtual ImageBoxGridDisplayMode TextDisplayMode
        {
            get { return _textDisplayMode; }
            set
            {
                if (this.TextDisplayMode != value)
                {
                    _textDisplayMode = value;

                    this.OnTextDisplayModeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets padding of text within the control.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(typeof(Padding), "0, 0, 0, 0")]
        public virtual Padding TextPadding
        {
            get { return _textPadding; }
            set
            {
                if (this.TextPadding != value)
                {
                    _textPadding = value;

                    this.OnTextPaddingChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the control acts as a virtual image box.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the control acts as a virtual image box; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        ///   When this property is set to <c>true</c>, the Image property is ignored in favor of the VirtualSize property. In addition, the VirtualDraw event is raised to allow custom painting of the image area.
        /// </remarks>
        [Category("Behavior")]
        [DefaultValue(false)]
        public virtual bool VirtualMode
        {
            get { return _virtualMode; }
            set
            {
                if (this.VirtualMode != value)
                {
                    _virtualMode = value;

                    this.OnVirtualModeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the size of the virtual image.
        /// </summary>
        /// <value>The size of the virtual image.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(Size), "0, 0")]
        public virtual Size VirtualSize
        {
            get { return _virtualSize; }
            set
            {
                if (this.VirtualSize != value)
                {
                    _virtualSize = value;

                    this.OnVirtualSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the zoom.
        /// </summary>
        /// <value>The zoom.</value>
        [DefaultValue(100)]
        [Category("Appearance")]
        public virtual int Zoom
        {
            get { return _zoom; }
            set { this.SetZoom(value, value > this.Zoom ? ImageBoxZoomActions.ZoomIn : ImageBoxZoomActions.ZoomOut, ImageBoxActionSources.Unknown); }
        }

        /// <summary>
        ///   Gets the zoom factor.
        /// </summary>
        /// <value>The zoom factor.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual double ZoomFactor
        {
            get { return (double)this.Zoom / 100; }
        }

        /// <summary>
        ///   Gets or sets the zoom levels.
        /// </summary>
        /// <value>The zoom levels.</value>
        [Browsable(false) /*Category("Behavior"), DefaultValue(typeof(ZoomLevelCollection), "7, 10, 15, 20, 25, 30, 50, 70, 100, 150, 200, 300, 400, 500, 600, 700, 800, 1200, 1600, 2000, 2500, 3000, 3500")*/]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ImageBoxZoomLevelCollection ZoomLevels
        {
            get { return _zoomLevels; }
            set
            {
                if (this.ZoomLevels != value)
                {
                    _zoomLevels = value;

                    this.OnZoomLevelsChanged(EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        ///   Gets a value indicating whether painting of the control is allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if painting of the control is allowed; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool AllowPainting
        {
            get { return _updateCount == 0; }
        }

        /// <summary>
        /// [PHAP] Gets or sets a value indicating whether the current image is animated.
        /// </summary>
        /// <value><c>true</c> if the current image is animated; otherwise, <c>false</c>.</value>
        public bool IsAnimating { get; protected set; }

        /// <summary>
        ///   Gets the height of the scaled image.
        /// </summary>
        /// <value>The height of the scaled image.</value>
        protected virtual int ScaledImageHeight
        {
            get { return this.Scale(this.ViewSize.Height); }
        }

        /// <summary>
        ///   Gets the width of the scaled image.
        /// </summary>
        /// <value>The width of the scaled image.</value>
        protected virtual int ScaledImageWidth
        {
            get { return this.Scale(this.ViewSize.Width); }
        }

        /// <summary>
        /// Scales the specified integer according to the current zoom factor.
        /// </summary>
        /// <param name="value">The value to scale.</param>
        /// <returns>The specified value scaled by the current zoom factor.</returns>
        protected int Scale(int value)
        {
            return Convert.ToInt32(value * this.ZoomFactor);
        }

        /// <summary>
        /// Gets the size of the view.
        /// </summary>
        /// <value>The size of the view.</value>
        protected virtual Size ViewSize
        {
            get { return _viewSize; }
        }

        private Size _viewSize;

        /// <summary>
        /// Gets or sets a value indicating whether a drag operation was cancelled.
        /// </summary>
        /// <value><c>true</c> if the drag operation was cancelled; otherwise, <c>false</c>.</value>
        protected bool WasDragCancelled { get; set; }

        #endregion

        #region Public Members

        /// <summary>
        ///   Resets the zoom to 100%.
        /// </summary>
        public virtual void ActualSize()
        {
            this.PerformActualSize(ImageBoxActionSources.Unknown);
        }

        /// <summary>
        ///   Disables any redrawing of the image box
        /// </summary>
        public virtual void BeginUpdate()
        {
            _updateCount++;
        }

        /// <summary>
        /// Start animating 
        /// </summary>
        public void StopAnimating()
        {
            if (!IsAnimating)
                return;
            ImageAnimator.StopAnimate(Image, OnFrameChangedHandler);
            IsAnimating = false;
        }

        /// <summary>
        /// Stop animating
        /// </summary>
        public void StartAnimating()
        {
            if (IsAnimating || !CanAnimate)
                return;

            try
            {
                ImageAnimator.Animate(Image, OnFrameChangedHandler);
                IsAnimating = true;
            }
            catch (Exception) { }
        }

        /// <summary>
        ///   Centers the given point in the image in the center of the control
        /// </summary>
        /// <param name="imageLocation">The point of the image to attempt to center.</param>
        public virtual void CenterAt(Point imageLocation)
        {
            this.ScrollTo(imageLocation, this.RelativeCenterPoint);
        }

        /// <summary>
        /// Returns the point in the center of the control based on the current zoom level
        /// </summary>
        private Point RelativeCenterPoint
        { get { return new Point((this.ScaledImageWidth - this.ClientSize.Width) / 2, (this.ScaledImageHeight - this.ClientSize.Height) / 2); } }

        /// <summary>
        ///   Centers the given point in the image in the center of the control
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to center.</param>
        /// <param name="y">The Y co-ordinate of the point to center.</param>
        public void CenterAt(int x, int y)
        {
            this.CenterAt(new Point(x, y));
        }

        /// <summary>
        ///   Centers the given point in the image in the center of the control
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to center.</param>
        /// <param name="y">The Y co-ordinate of the point to center.</param>
        public void CenterAt(float x, float y)
        {
            this.CenterAt(new Point((int)x, (int)y));
        }

        /// <summary>
        /// Resets the viewport to show the center of the image.
        /// </summary>
        public virtual void CenterToImage()
        {
            this.AutoScrollPosition = this.RelativeCenterPoint;
        }

        /// <summary>
        ///   Enables the redrawing of the image box
        /// </summary>
        public virtual void EndUpdate()
        {
            if (_updateCount > 0)
            {
                _updateCount--;
            }

            if (this.AllowPainting)
            {
                this.Invalidate();
            }
        }

        /// <summary>
        ///   Fits a given <see cref="T:System.Drawing.Rectangle" /> to match image boundaries
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <returns>
        ///   A <see cref="T:System.Drawing.Rectangle" /> structure remapped to fit the image boundaries
        /// </returns>
        public Rectangle FitRectangle(Rectangle rectangle)
        {
            int x;
            int y;
            int w;
            int h;

            x = rectangle.X;
            y = rectangle.Y;
            w = rectangle.Width;
            h = rectangle.Height;

            if (x < 0)
            {
                x = 0;
            }

            if (y < 0)
            {
                y = 0;
            }

            if (x + w > this.ViewSize.Width)
            {
                w = this.ViewSize.Width - x;
            }

            if (y + h > this.ViewSize.Height)
            {
                h = this.ViewSize.Height - y;
            }

            return new Rectangle(x, y, w, h);
        }

        /// <summary>
        ///   Fits a given <see cref="T:System.Drawing.RectangleF" /> to match image boundaries
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <returns>
        ///   A <see cref="T:System.Drawing.RectangleF" /> structure remapped to fit the image boundaries
        /// </returns>
        public RectangleF FitRectangle(RectangleF rectangle)
        {
            float x;
            float y;
            float w;
            float h;

            x = rectangle.X;
            y = rectangle.Y;
            w = rectangle.Width;
            h = rectangle.Height;

            if (x < 0)
            {
                w -= -x;
                x = 0;
            }

            if (y < 0)
            {
                h -= -y;
                y = 0;
            }

            if (x + w > this.ViewSize.Width)
            {
                w = this.ViewSize.Width - x;
            }

            if (y + h > this.ViewSize.Height)
            {
                h = this.ViewSize.Height - y;
            }

            return new RectangleF(x, y, w, h);
        }

        /// <summary>
        ///   Gets the image view port.
        /// </summary>
        /// <returns></returns>
        public virtual Rectangle GetImageViewPort()
        {
            Rectangle viewPort;

            if (!this.ViewSize.IsEmpty)
            {
                Rectangle innerRectangle;
                Point offset;
                int width;
                int height;
                bool hScroll;
                bool vScroll;

                innerRectangle = this.GetInsideViewPort(true);

                hScroll = this.HScroll;
                vScroll = this.VScroll;

                if (!hScroll && !vScroll) // if no scrolling is present, tinker the view port so that the image and any applicable borders all fit inside
                {
                    innerRectangle.Inflate(-this.GetImageBorderOffset(), -this.GetImageBorderOffset());
                }

                if (this.SizeMode != ImageBoxSizeMode.Stretch)
                {
                    if (this.AutoCenter)
                    {
                        int x;
                        int y;

                        x = !hScroll ? (innerRectangle.Width - (this.ScaledImageWidth + this.Padding.Horizontal)) / 2 : 0;
                        y = !vScroll ? (innerRectangle.Height - (this.ScaledImageHeight + this.Padding.Vertical)) / 2 : 0;

                        offset = new Point(x, y);
                    }
                    else
                    {
                        offset = Point.Empty;
                    }

                    width = Math.Min(this.ScaledImageWidth - Math.Abs(this.AutoScrollPosition.X), innerRectangle.Width);
                    height = Math.Min(this.ScaledImageHeight - Math.Abs(this.AutoScrollPosition.Y), innerRectangle.Height);
                }
                else
                {
                    offset = Point.Empty;
                    width = innerRectangle.Width;
                    height = innerRectangle.Height;
                }

                viewPort = new Rectangle(offset.X + innerRectangle.Left, offset.Y + innerRectangle.Top, width, height);
            }
            else
            {
                viewPort = Rectangle.Empty;
            }

            return viewPort;
        }

        /// <summary>
        ///   Gets the inside view port, excluding any padding.
        /// </summary>
        /// <returns></returns>
        public Rectangle GetInsideViewPort()
        {
            return this.GetInsideViewPort(false);
        }

        /// <summary>
        ///   Gets the inside view port.
        /// </summary>
        /// <param name="includePadding">
        ///   if set to <c>true</c> [include padding].
        /// </param>
        /// <returns></returns>
        public virtual Rectangle GetInsideViewPort(bool includePadding)
        {
            int left;
            int top;
            int width;
            int height;
            Size clientSize;

            clientSize = this.ClientSize;
            left = 0;
            top = 0;
            width = clientSize.Width;
            height = clientSize.Height;

            if (this.VerticalScroll.Visible)
            {
                width -= _vScrollBar.Width;
            }

            if (this.HorizontalScroll.Visible)
            {
                height -= _hScrollBar.Height;
            }

            if (includePadding)
            {
                Padding padding;

                padding = this.Padding;
                left += padding.Left;
                top += padding.Top;
                width -= padding.Horizontal;
                height -= padding.Vertical;
            }

            return new Rectangle(left, top, width, height);
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Point" /> repositioned to include the current image offset and scaled by the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="Point"/> to offset.</param>
        /// <returns>A <see cref="Point"/> which has been repositioned to match the current zoom level and image offset</returns>
        public virtual Point GetOffsetPoint(Point source)
        {
            PointF offset;

            offset = this.GetOffsetPoint(new PointF(source.X, source.Y));

            return new Point((int)offset.X, (int)offset.Y);
        }

        /// <summary>
        ///   Returns the source co-ordinates repositioned to include the current image offset and scaled by the current zoom level
        /// </summary>
        /// <param name="x">The source X co-ordinate.</param>
        /// <param name="y">The source Y co-ordinate.</param>
        /// <returns>A <see cref="Point"/> which has been repositioned to match the current zoom level and image offset</returns>
        public Point GetOffsetPoint(int x, int y)
        {
            return this.GetOffsetPoint(new Point(x, y));
        }

        /// <summary>
        ///   Returns the source co-ordinates repositioned to include the current image offset and scaled by the current zoom level
        /// </summary>
        /// <param name="x">The source X co-ordinate.</param>
        /// <param name="y">The source Y co-ordinate.</param>
        /// <returns>A <see cref="Point"/> which has been repositioned to match the current zoom level and image offset</returns>
        public PointF GetOffsetPoint(float x, float y)
        {
            return this.GetOffsetPoint(new PointF(x, y));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.PointF" /> repositioned to include the current image offset and scaled by the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="PointF"/> to offset.</param>
        /// <returns>A <see cref="PointF"/> which has been repositioned to match the current zoom level and image offset</returns>
        public virtual PointF GetOffsetPoint(PointF source)
        {
            Rectangle viewport;
            PointF scaled;
            int offsetX;
            int offsetY;

            viewport = this.GetImageViewPort();
            scaled = this.GetScaledPoint(source);
            offsetX = viewport.Left + this.Padding.Left + this.AutoScrollPosition.X;
            offsetY = viewport.Top + this.Padding.Top + this.AutoScrollPosition.Y;

            return new PointF(scaled.X + offsetX, scaled.Y + offsetY);
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.RectangleF" /> scaled according to the current zoom level and repositioned to include the current image offset
        /// </summary>
        /// <param name="source">The source <see cref="RectangleF"/> to offset.</param>
        /// <returns>A <see cref="RectangleF"/> which has been resized and repositioned to match the current zoom level and image offset</returns>
        public virtual RectangleF GetOffsetRectangle(RectangleF source)
        {
            RectangleF viewport;
            RectangleF scaled;
            float offsetX;
            float offsetY;

            viewport = this.GetImageViewPort();
            scaled = this.GetScaledRectangle(source);
            offsetX = viewport.Left + this.Padding.Left + this.AutoScrollPosition.X;
            offsetY = viewport.Top + this.Padding.Top + this.AutoScrollPosition.Y;

            return new RectangleF(new PointF(scaled.Left + offsetX, scaled.Top + offsetY), scaled.Size);
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level and repositioned to include the current image offset
        /// </summary>
        /// <param name="x">The X co-ordinate of the source rectangle.</param>
        /// <param name="y">The Y co-ordinate of the source rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>A <see cref="Rectangle"/> which has been resized and repositioned to match the current zoom level and image offset</returns>
        public Rectangle GetOffsetRectangle(int x, int y, int width, int height)
        {
            return this.GetOffsetRectangle(new Rectangle(x, y, width, height));
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level and repositioned to include the current image offset
        /// </summary>
        /// <param name="x">The X co-ordinate of the source rectangle.</param>
        /// <param name="y">The Y co-ordinate of the source rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>A <see cref="RectangleF"/> which has been resized and repositioned to match the current zoom level and image offset</returns>
        public RectangleF GetOffsetRectangle(float x, float y, float width, float height)
        {
            return this.GetOffsetRectangle(new RectangleF(x, y, width, height));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Rectangle" /> scaled according to the current zoom level and repositioned to include the current image offset
        /// </summary>
        /// <param name="source">The source <see cref="Rectangle"/> to offset.</param>
        /// <returns>A <see cref="Rectangle"/> which has been resized and repositioned to match the current zoom level and image offset</returns>
        public virtual Rectangle GetOffsetRectangle(Rectangle source)
        {
            Rectangle viewport;
            Rectangle scaled;
            int offsetX;
            int offsetY;

            viewport = this.GetImageViewPort();
            scaled = this.GetScaledRectangle(source);
            offsetX = viewport.Left + this.Padding.Left + this.AutoScrollPosition.X;
            offsetY = viewport.Top + this.Padding.Top + this.AutoScrollPosition.Y;

            return new Rectangle(new Point(scaled.Left + offsetX, scaled.Top + offsetY), scaled.Size);
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Point" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to scale.</param>
        /// <param name="y">The Y co-ordinate of the point to scale.</param>
        /// <returns>A <see cref="Point"/> which has been scaled to match the current zoom level</returns>
        public Point GetScaledPoint(int x, int y)
        {
            return this.GetScaledPoint(new Point(x, y));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Point" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to scale.</param>
        /// <param name="y">The Y co-ordinate of the point to scale.</param>
        /// <returns>A <see cref="Point"/> which has been scaled to match the current zoom level</returns>
        public PointF GetScaledPoint(float x, float y)
        {
            return this.GetScaledPoint(new PointF(x, y));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Point" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="Point"/> to scale.</param>
        /// <returns>A <see cref="Point"/> which has been scaled to match the current zoom level</returns>
        public virtual Point GetScaledPoint(Point source)
        {
            return new Point((int)(source.X * this.ZoomFactor), (int)(source.Y * this.ZoomFactor));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.PointF" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="PointF"/> to scale.</param>
        /// <returns>A <see cref="PointF"/> which has been scaled to match the current zoom level</returns>
        public virtual PointF GetScaledPoint(PointF source)
        {
            return new PointF((float)(source.X * this.ZoomFactor), (float)(source.Y * this.ZoomFactor));
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level
        /// </summary>
        /// <param name="x">The X co-ordinate of the source rectangle.</param>
        /// <param name="y">The Y co-ordinate of the source rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>A <see cref="Rectangle"/> which has been scaled to match the current zoom level</returns>
        public Rectangle GetScaledRectangle(int x, int y, int width, int height)
        {
            return this.GetScaledRectangle(new Rectangle(x, y, width, height));
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level
        /// </summary>
        /// <param name="x">The X co-ordinate of the source rectangle.</param>
        /// <param name="y">The Y co-ordinate of the source rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>A <see cref="RectangleF"/> which has been scaled to match the current zoom level</returns>
        public RectangleF GetScaledRectangle(float x, float y, float width, float height)
        {
            return this.GetScaledRectangle(new RectangleF(x, y, width, height));
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level
        /// </summary>
        /// <param name="location">The location of the source rectangle.</param>
        /// <param name="size">The size of the source rectangle.</param>
        /// <returns>A <see cref="Rectangle"/> which has been scaled to match the current zoom level</returns>
        public Rectangle GetScaledRectangle(Point location, Size size)
        {
            return this.GetScaledRectangle(new Rectangle(location, size));
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level
        /// </summary>
        /// <param name="location">The location of the source rectangle.</param>
        /// <param name="size">The size of the source rectangle.</param>
        /// <returns>A <see cref="Rectangle"/> which has been scaled to match the current zoom level</returns>
        public RectangleF GetScaledRectangle(PointF location, SizeF size)
        {
            return this.GetScaledRectangle(new RectangleF(location, size));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Rectangle" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="Rectangle"/> to scale.</param>
        /// <returns>A <see cref="Rectangle"/> which has been scaled to match the current zoom level</returns>
        public virtual Rectangle GetScaledRectangle(Rectangle source)
        {
            return new Rectangle((int)(source.Left * this.ZoomFactor), (int)(source.Top * this.ZoomFactor), (int)(source.Width * this.ZoomFactor), (int)(source.Height * this.ZoomFactor));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.RectangleF" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="RectangleF"/> to scale.</param>
        /// <returns>A <see cref="RectangleF"/> which has been scaled to match the current zoom level</returns>
        public virtual RectangleF GetScaledRectangle(RectangleF source)
        {
            return new RectangleF((float)(source.Left * this.ZoomFactor), (float)(source.Top * this.ZoomFactor), (float)(source.Width * this.ZoomFactor), (float)(source.Height * this.ZoomFactor));
        }

        /// <summary>
        ///   Returns the source size scaled according to the current zoom level
        /// </summary>
        /// <param name="width">The width of the size to scale.</param>
        /// <param name="height">The height of the size to scale.</param>
        /// <returns>A <see cref="SizeF"/> which has been resized to match the current zoom level</returns>
        public SizeF GetScaledSize(float width, float height)
        {
            return this.GetScaledSize(new SizeF(width, height));
        }

        /// <summary>
        ///   Returns the source size scaled according to the current zoom level
        /// </summary>
        /// <param name="width">The width of the size to scale.</param>
        /// <param name="height">The height of the size to scale.</param>
        /// <returns>A <see cref="Size"/> which has been resized to match the current zoom level</returns>
        public Size GetScaledSize(int width, int height)
        {
            return this.GetScaledSize(new Size(width, height));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.SizeF" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="SizeF"/> to scale.</param>
        /// <returns>A <see cref="SizeF"/> which has been resized to match the current zoom level</returns>
        public virtual SizeF GetScaledSize(SizeF source)
        {
            return new SizeF((float)(source.Width * this.ZoomFactor), (float)(source.Height * this.ZoomFactor));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Size" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="Size"/> to scale.</param>
        /// <returns>A <see cref="Size"/> which has been resized to match the current zoom level</returns>
        public virtual Size GetScaledSize(Size source)
        {
            return new Size((int)(source.Width * this.ZoomFactor), (int)(source.Height * this.ZoomFactor));
        }

        /// <summary>
        ///   Creates an image based on the current selection region
        /// </summary>
        /// <returns>An image containing the selection contents if a selection if present, otherwise null</returns>
        /// <remarks>The caller is responsible for disposing of the returned image</remarks>
        public Image GetSelectedImage()
        {
            Image result;

            result = null;

            if (!this.SelectionRegion.IsEmpty)
            {
                Rectangle rect;

                rect = this.FitRectangle(new Rectangle((int)this.SelectionRegion.X, (int)this.SelectionRegion.Y, (int)this.SelectionRegion.Width, (int)this.SelectionRegion.Height));

                if (rect.Width > 0 && rect.Height > 0)
                {
                    result = new Bitmap(rect.Width, rect.Height);

                    using (Graphics g = Graphics.FromImage(result))
                    {
                        g.DrawImage(this.Image, new Rectangle(Point.Empty, rect.Size), rect, GraphicsUnit.Pixel);
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///   Gets the source image region.
        /// </summary>
        /// <returns></returns>
        public virtual RectangleF GetSourceImageRegion()
        {
            RectangleF region;

            if (!this.ViewSize.IsEmpty)
            {
                if (this.SizeMode != ImageBoxSizeMode.Stretch)
                {
                    float sourceLeft;
                    float sourceTop;
                    float sourceWidth;
                    float sourceHeight;
                    Rectangle viewPort;

                    viewPort = this.GetImageViewPort();
                    sourceLeft = (float)(-this.AutoScrollPosition.X / this.ZoomFactor);
                    sourceTop = (float)(-this.AutoScrollPosition.Y / this.ZoomFactor);
                    sourceWidth = (float)(viewPort.Width / this.ZoomFactor);
                    sourceHeight = (float)(viewPort.Height / this.ZoomFactor);

                    region = new RectangleF(sourceLeft, sourceTop, sourceWidth, sourceHeight);
                }
                else
                {
                    region = new RectangleF(PointF.Empty, this.ViewSize);
                }
            }
            else
            {
                region = RectangleF.Empty;
            }

            return region;
        }

        /// <summary>
        ///   Determines whether the specified point is located within the image view port
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        ///   <c>true</c> if the specified point is located within the image view port; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsPointInImage(Point point)
        {
            return this.GetImageViewPort().Contains(point);
        }

        /// <summary>
        ///   Determines whether the specified point is located within the image view port
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to check.</param>
        /// <param name="y">The Y co-ordinate of the point to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified point is located within the image view port; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPointInImage(int x, int y)
        {
            return this.IsPointInImage(new Point(x, y));
        }

        /// <summary>
        ///   Determines whether the specified point is located within the image view port
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to check.</param>
        /// <param name="y">The Y co-ordinate of the point to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified point is located within the image view port; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPointInImage(float x, float y)
        {
            return this.IsPointInImage(new Point((int)x, (int)y));
        }

        /// <summary>
        ///   Converts the given client size point to represent a coordinate on the source image.
        /// </summary>
        /// <param name="point">The source point.</param>
        /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
        public Point PointToImage(Point point)
        {
            return this.PointToImage(point, false);
        }

        /// <summary>
        ///   Converts the given client size point to represent a coordinate on the source image.
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to convert.</param>
        /// <param name="y">The Y co-ordinate of the point to convert.</param>
        /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
        public Point PointToImage(float x, float y)
        {
            return this.PointToImage(x, y, false);
        }

        /// <summary>
        ///   Converts the given client size point to represent a coordinate on the source image.
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to convert.</param>
        /// <param name="y">The Y co-ordinate of the point to convert.</param>
        /// <param name="fitToBounds">
        ///   if set to <c>true</c> and the point is outside the bounds of the source image, it will be mapped to the nearest edge.
        /// </param>
        /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
        public Point PointToImage(float x, float y, bool fitToBounds)
        {
            return this.PointToImage(new Point((int)x, (int)y), fitToBounds);
        }

        /// <summary>
        ///   Converts the given client size point to represent a coordinate on the source image.
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to convert.</param>
        /// <param name="y">The Y co-ordinate of the point to convert.</param>
        /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
        public Point PointToImage(int x, int y)
        {
            return this.PointToImage(x, y, false);
        }

        /// <summary>
        ///   Converts the given client size point to represent a coordinate on the source image.
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to convert.</param>
        /// <param name="y">The Y co-ordinate of the point to convert.</param>
        /// <param name="fitToBounds">
        ///   if set to <c>true</c> and the point is outside the bounds of the source image, it will be mapped to the nearest edge.
        /// </param>
        /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
        public Point PointToImage(int x, int y, bool fitToBounds)
        {
            return this.PointToImage(new Point(x, y), fitToBounds);
        }

        /// <summary>
        ///   Converts the given client size point to represent a coordinate on the source image.
        /// </summary>
        /// <param name="point">The source point.</param>
        /// <param name="fitToBounds">
        ///   if set to <c>true</c> and the point is outside the bounds of the source image, it will be mapped to the nearest edge.
        /// </param>
        /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
        public virtual Point PointToImage(Point point, bool fitToBounds)
        {
            Rectangle viewport;
            int x;
            int y;

            viewport = this.GetImageViewPort();

            if (!fitToBounds || viewport.Contains(point))
            {
                if (this.AutoScrollPosition != Point.Empty)
                {
                    point = new Point(point.X - this.AutoScrollPosition.X, point.Y - this.AutoScrollPosition.Y);
                }

                x = (int)((point.X - viewport.X) / this.ZoomFactor);
                y = (int)((point.Y - viewport.Y) / this.ZoomFactor);

                if (fitToBounds)
                {
                    Size viewSize;

                    viewSize = this.ViewSize;

                    if (x < 0)
                    {
                        x = 0;
                    }
                    else if (x > viewSize.Width)
                    {
                        x = viewSize.Width;
                    }

                    if (y < 0)
                    {
                        y = 0;
                    }
                    else if (y > viewSize.Height)
                    {
                        y = viewSize.Height;
                    }
                }
            }
            else
            {
                x = 0; // Return Point.Empty if we couldn't match
                y = 0;
            }

            return new Point(x, y);
        }

        /// <summary>
        ///   Scrolls the control to the given point in the image, offset at the specified display point
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to scroll to.</param>
        /// <param name="y">The Y co-ordinate of the point to scroll to.</param>
        /// <param name="relativeX">The X co-ordinate relative to the <c>x</c> parameter.</param>
        /// <param name="relativeY">The Y co-ordinate relative to the <c>y</c> parameter.</param>
        public void ScrollTo(int x, int y, int relativeX, int relativeY)
        {
            this.ScrollTo(new Point(x, y), new Point(relativeX, relativeY));
        }

        /// <summary>
        ///   Scrolls the control to the given point in the image, offset at the specified display point
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to scroll to.</param>
        /// <param name="y">The Y co-ordinate of the point to scroll to.</param>
        /// <param name="relativeX">The X co-ordinate relative to the <c>x</c> parameter.</param>
        /// <param name="relativeY">The Y co-ordinate relative to the <c>y</c> parameter.</param>
        public void ScrollTo(float x, float y, float relativeX, float relativeY)
        {
            this.ScrollTo(new Point((int)x, (int)y), new Point((int)relativeX, (int)relativeY));
        }

        /// <summary>
        ///   Scrolls the control to the given point in the image, offset at the specified display point
        /// </summary>
        /// <param name="imageLocation">The point of the image to attempt to scroll to.</param>
        /// <param name="relativeDisplayPoint">The relative display point to offset scrolling by.</param>
        public virtual void ScrollTo(Point imageLocation, Point relativeDisplayPoint)
        {
            int x;
            int y;

            x = this.Scale(imageLocation.X) - relativeDisplayPoint.X;
            y = this.Scale(imageLocation.Y) - relativeDisplayPoint.Y;

            this.AutoScrollPosition = new Point(x, y);
        }

        /// <summary>
        ///   Creates a selection region which encompasses the entire image
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if no image is currently set</exception>
        public virtual void SelectAll()
        {
            this.SelectionRegion = new RectangleF(PointF.Empty, this.ViewSize);
        }

        /// <summary>
        ///   Clears any existing selection region
        /// </summary>
        public virtual void SelectNone()
        {
            this.SelectionRegion = RectangleF.Empty;
        }

        /// <summary>
        ///   Zooms into the image
        /// </summary>
        public virtual void ZoomIn()
        {
            this.ZoomIn(true);
        }

        /// <summary>
        ///   Zooms into the image
        /// </summary>
        /// <param name="preservePosition"><c>true</c> if the current scrolling position should be preserved relative to the new zoom level, <c>false</c> to reset.</param>
        public virtual void ZoomIn(bool preservePosition)
        {
            this.PerformZoomIn(ImageBoxActionSources.Unknown, preservePosition);
        }

        /// <summary>
        ///   Zooms out of the image
        /// </summary>
        public virtual void ZoomOut()
        {
            this.ZoomOut(true);
        }

        /// <summary>
        ///   Zooms out of the image
        /// </summary>
        /// <param name="preservePosition"><c>true</c> if the current scrolling position should be preserved relative to the new zoom level, <c>false</c> to reset.</param>
        public virtual void ZoomOut(bool preservePosition)
        {
            this.PerformZoomOut(ImageBoxActionSources.Unknown, preservePosition);
        }
        /// <summary>
        ///   Zooms to the maximum size for displaying the entire image within the bounds of the control.
        /// </summary>
        public virtual void ZoomToFit()
        {
            if (!this.ViewSize.IsEmpty)
            {
                Rectangle innerRectangle;
                double zoom;
                double aspectRatio;

                innerRectangle = this.GetInsideViewPort(true);

                if (this.ViewSize.Width > this.ViewSize.Height)
                {
                    aspectRatio = (double)innerRectangle.Width / this.ViewSize.Width;
                    zoom = aspectRatio * 100.0;

                    if (innerRectangle.Height < ((this.ViewSize.Height * zoom) / 100.0))
                    {
                        aspectRatio = (double)innerRectangle.Height / this.ViewSize.Height;
                        zoom = aspectRatio * 100.0;
                    }
                }
                else
                {
                    aspectRatio = (double)innerRectangle.Height / this.ViewSize.Height;
                    zoom = aspectRatio * 100.0;

                    if (innerRectangle.Width < ((this.ViewSize.Width * zoom) / 100.0))
                    {
                        aspectRatio = (double)innerRectangle.Width / this.ViewSize.Width;
                        zoom = aspectRatio * 100.0;
                    }
                }

                this.Zoom = (int)Math.Round(Math.Floor(zoom));
            }
        }

        /// <summary>
        /// [PHAP] Zooms to the maximum size for displaying the entire image within the bounds of the control. If image size is smaller than viewer size, keep its original size.
        /// </summary>
        public virtual void ZoomAuto()
        {
            if (!this.ViewSize.IsEmpty)
            {
                Rectangle innerRectangle;
                double zoom;
                double aspectRatio;

                innerRectangle = this.GetInsideViewPort(true);
                
                if (this.ViewSize.Width <= innerRectangle.Width && this.ViewSize.Height <= innerRectangle.Height)
                {
                    zoom = 100.0;
                }
                else
                {
                    if (this.ViewSize.Width > this.ViewSize.Height)
                    {
                        aspectRatio = (double)innerRectangle.Width / this.ViewSize.Width;
                        zoom = aspectRatio * 100.0;

                        if (innerRectangle.Height < ((this.ViewSize.Height * zoom) / 100.0))
                        {
                            aspectRatio = (double)innerRectangle.Height / this.ViewSize.Height;
                            zoom = aspectRatio * 100.0;
                        }
                    }
                    else
                    {
                        aspectRatio = (double)innerRectangle.Height / this.ViewSize.Height;
                        zoom = aspectRatio * 100.0;

                        if (innerRectangle.Width < ((this.ViewSize.Width * zoom) / 100.0))
                        {
                            aspectRatio = (double)innerRectangle.Width / this.ViewSize.Width;
                            zoom = aspectRatio * 100.0;
                        }
                    }
                }

                this.Zoom = (int)Math.Round(Math.Floor(zoom));
            }
        }



        /// <summary>
        ///   Adjusts the view port to fit the given region
        /// </summary>
        /// <param name="x">The X co-ordinate of the selection region.</param>
        /// <param name="y">The Y co-ordinate of the selection region.</param>
        /// <param name="width">The width of the selection region.</param>
        /// <param name="height">The height of the selection region.</param>
        public void ZoomToRegion(float x, float y, float width, float height)
        {
            this.ZoomToRegion(new RectangleF(x, y, width, height));
        }

        /// <summary>
        ///   Adjusts the view port to fit the given region
        /// </summary>
        /// <param name="x">The X co-ordinate of the selection region.</param>
        /// <param name="y">The Y co-ordinate of the selection region.</param>
        /// <param name="width">The width of the selection region.</param>
        /// <param name="height">The height of the selection region.</param>
        public void ZoomToRegion(int x, int y, int width, int height)
        {
            this.ZoomToRegion(new RectangleF(x, y, width, height));
        }

        /// <summary>
        ///   Adjusts the view port to fit the given region
        /// </summary>
        /// <param name="rectangle">The rectangle to fit the view port to.</param>
        public virtual void ZoomToRegion(RectangleF rectangle)
        {
            double ratioX;
            double ratioY;
            double zoomFactor;
            int cx;
            int cy;

            ratioX = this.ClientSize.Width / rectangle.Width;
            ratioY = this.ClientSize.Height / rectangle.Height;
            zoomFactor = Math.Min(ratioX, ratioY);
            cx = (int)(rectangle.X + (rectangle.Width / 2));
            cy = (int)(rectangle.Y + (rectangle.Height / 2));

            this.Zoom = (int)(zoomFactor * 100);
            this.CenterAt(new Point(cx, cy));
        }

        /// <summary>
        /// Clamps the specified value within the given range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        /// <returns></returns>
        private int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        #endregion

        #region Protected Members

        /// <summary>
        ///   Adjusts the layout.
        /// </summary>
        protected virtual void AdjustLayout()
        {
            if (this.AllowPainting)
            {
                if (this.AutoSize)
                {
                    this.AdjustSize();
                }
                else if (this.SizeMode != ImageBoxSizeMode.Normal)
                {
                    this.ZoomToFit();
                }

                this.AdjustViewPort();
                this.Invalidate();
            }
        }

        private HScrollBar _hScrollBar;
        private VScrollBar _vScrollBar;

        /// <summary>
        ///   Adjusts the scroll.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        protected virtual void AdjustScroll(int x, int y)
        {
            Point scrollPosition;

            scrollPosition = new Point(this.HorizontalScroll.Value + x, this.VerticalScroll.Value + y);

            this.UpdateScrollPosition(scrollPosition);
        }

        /// <summary>
        ///   Adjusts the size.
        /// </summary>
        protected virtual void AdjustSize()
        {
            if (this.AutoSize && this.Dock == DockStyle.None)
            {
                base.Size = base.PreferredSize;
            }
        }

        /// <summary>
        ///   Adjusts the view port.
        /// </summary>
        protected virtual void AdjustViewPort()
        {
            Size viewSize;
            Size clientSize;
            int cw;
            int ch;
            bool vScroll;
            bool hScroll;

            viewSize = this.ViewSize;
            clientSize = this.ClientSize;

            if (!viewSize.IsEmpty)
            {
                Size size;

                size = this.GetInsideViewPort(true).Size;

                hScroll = this.ScaledImageWidth > size.Width;
                vScroll = this.ScaledImageHeight > size.Height;
            }
            else
            {
                hScroll = false;
                vScroll = false;
            }

            this.HScroll = hScroll;
            this.VScroll = vScroll;

            this.UpdateScrollbars();

            cw = vScroll ? clientSize.Width - _vScrollBar.Width : clientSize.Width;
            ch = hScroll ? clientSize.Height - _hScrollBar.Height : clientSize.Height;

            _hScrollBar.Width = cw;
            _hScrollBar.Top = clientSize.Height - _hScrollBar.Height;

            _vScrollBar.Height = ch;
            _vScrollBar.Left = clientSize.Width - _vScrollBar.Width;
        }

        /// <summary>
        ///   Creates the grid tile image.
        /// </summary>
        /// <param name="cellSize">Size of the cell.</param>
        /// <param name="firstColor">The first color.</param>
        /// <param name="secondColor">Color of the second.</param>
        /// <returns></returns>
        protected virtual Bitmap CreateGridTileImage(int cellSize, Color firstColor, Color secondColor)
        {
            float scale;

            // rescale the cell size
            switch (this.GridScale)
            {
                case ImageBoxGridScale.Medium:
                    scale = 1.5F;
                    break;

                case ImageBoxGridScale.Large:
                    scale = 2;
                    break;

                case ImageBoxGridScale.Tiny:
                    scale = 0.5F;
                    break;

                default:
                    scale = 1;
                    break;
            }

            cellSize = (int)(cellSize * scale);

            return CreateCheckerBoxTile(cellSize, firstColor, secondColor);
        }

        /// <summary>
        /// Draws the background of the control.
        /// </summary>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        protected virtual void DrawBackground(PaintEventArgs e)
        {
            Rectangle innerRectangle;

            innerRectangle = this.GetInsideViewPort();

            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(brush, innerRectangle);
            }

            if (_texture != null && this.GridDisplayMode != ImageBoxGridDisplayMode.None)
            {
                switch (this.GridDisplayMode)
                {
                    case ImageBoxGridDisplayMode.Image:
                        Rectangle fillRectangle;

                        fillRectangle = this.GetImageViewPort();
                        e.Graphics.FillRectangle(_texture, fillRectangle);
                        break;

                    case ImageBoxGridDisplayMode.Client:
                        e.Graphics.FillRectangle(_texture, innerRectangle);
                        break;
                }
            }
        }

        /// <summary>
        ///   Draws a drop shadow.
        /// </summary>
        /// <param name="g">The graphics. </param>
        /// <param name="viewPort"> The view port. </param>
        protected virtual void DrawDropShadow(Graphics g, Rectangle viewPort)
        {
            Rectangle rightEdge;
            Rectangle bottomEdge;

            rightEdge = new Rectangle(viewPort.Right + 1, viewPort.Top + this.DropShadowSize, this.DropShadowSize, viewPort.Height);
            bottomEdge = new Rectangle(viewPort.Left + this.DropShadowSize, viewPort.Bottom + 1, viewPort.Width + 1, this.DropShadowSize);

            using (Brush brush = new SolidBrush(this.ImageBorderColor))
            {
                g.FillRectangles(brush, new[]
                                {
                                  rightEdge, bottomEdge
                                });
            }
        }

        /// <summary>
        ///   Draws a glow shadow.
        /// </summary>
        /// <param name="g">The graphics.</param>
        /// <param name="viewPort">The view port.</param>
        protected virtual void DrawGlowShadow(Graphics g, Rectangle viewPort)
        {
            // Glow code adapted from http://www.codeproject.com/Articles/372743/gGlowBox-Create-a-glow-effect-around-a-focused-con

            g.SetClip(viewPort, CombineMode.Exclude); // make sure the inside glow doesn't appear

            using (GraphicsPath path = new GraphicsPath())
            {
                int glowSize;
                int feather;

                path.AddRectangle(viewPort);
                glowSize = this.DropShadowSize * 3;
                feather = 50;

                for (int i = 1; i <= glowSize; i += 2)
                {
                    int alpha;

                    alpha = feather - ((feather / glowSize) * i);

                    using (Pen pen = new Pen(Color.FromArgb(alpha, this.ImageBorderColor), i)
                                     {
                                         LineJoin = LineJoin.Round
                                     })
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
        }

        /// <summary>
        ///   Draws the image.
        /// </summary>
        /// <param name="g">The g.</param>
        protected virtual void DrawImage(Graphics g)
        {
            InterpolationMode currentInterpolationMode;
            PixelOffsetMode currentPixelOffsetMode;

            currentInterpolationMode = g.InterpolationMode;
            currentPixelOffsetMode = g.PixelOffsetMode;

            g.InterpolationMode = this.GetInterpolationMode();

            // disable pixel offsets. Thanks to Rotem for the info.
            // http://stackoverflow.com/questions/14070311/why-is-graphics-drawimage-cropping-part-of-my-image/14070372#14070372
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            try
            {
                // Animation. Thanks to teamalpha5441 for the contribution
                if (this.IsAnimating && !this.DesignMode)
                {
                    ImageAnimator.UpdateFrames(this.Image);
                }

                g.DrawImage(this.Image, this.GetImageViewPort(), this.GetSourceImageRegion(), GraphicsUnit.Pixel);
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
                IsAnimating = false;
                ImageAnimator.StopAnimate(this.Image, OnFrameChangedHandler);
            }

            g.PixelOffsetMode = currentPixelOffsetMode;
            g.InterpolationMode = currentInterpolationMode;
        }

        /// <summary>
        ///   Draws a border around the image.
        /// </summary>
        /// <param name="graphics"> The graphics. </param>
        protected virtual void DrawImageBorder(Graphics graphics)
        {
            if (this.ImageBorderStyle != ImageBoxBorderStyle.None)
            {
                Rectangle viewPort;

                graphics.SetClip(this.GetInsideViewPort()); // make sure the image border doesn't overwrite the control border

                viewPort = this.GetImageViewPort();
                viewPort = new Rectangle(viewPort.Left - 1, viewPort.Top - 1, viewPort.Width + 1, viewPort.Height + 1);

                using (Pen borderPen = new Pen(this.ImageBorderColor))
                {
                    graphics.DrawRectangle(borderPen, viewPort);
                }

                switch (this.ImageBorderStyle)
                {
                    case ImageBoxBorderStyle.FixedSingleDropShadow:
                        this.DrawDropShadow(graphics, viewPort);
                        break;
                    case ImageBoxBorderStyle.FixedSingleGlowShadow:
                        this.DrawGlowShadow(graphics, viewPort);
                        break;
                }

                graphics.ResetClip();
            }
        }

        /// <summary>
        /// Draws the specified text within the specified bounds using the specified device context.
        /// </summary>
        /// <param name="graphics">The device context in which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="bounds">The <see cref="Rectangle"/> that represents the bounds of the text.</param>
        protected void DrawLabel(Graphics graphics, string text, Rectangle bounds)
        {
            this.DrawLabel(graphics, text, this.Font, this.ForeColor, this.TextBackColor, this.TextAlign, bounds);
        }

        /// <summary>
        /// Draws the specified text within the specified bounds using the specified device context and font.
        /// </summary>
        /// <param name="graphics">The device context in which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The <see cref="Font"/> to apply to the drawn text.</param>
        /// <param name="bounds">The <see cref="Rectangle"/> that represents the bounds of the text.</param>
        protected void DrawLabel(Graphics graphics, string text, Font font, Rectangle bounds)
        {
            this.DrawLabel(graphics, text, font, this.ForeColor, this.TextBackColor, this.TextAlign, bounds);
        }

        /// <summary>
        /// Draws the specified text within the specified bounds using the specified device context, font, and color.
        /// </summary>
        /// <param name="graphics">The device context in which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The <see cref="Font"/> to apply to the drawn text.</param>
        /// <param name="foreColor">The <see cref="Color"/> to apply to the text.</param>
        /// <param name="bounds">The <see cref="Rectangle"/> that represents the bounds of the text.</param>
        protected void DrawLabel(Graphics graphics, string text, Font font, Color foreColor, Rectangle bounds)
        {
            this.DrawLabel(graphics, text, font, foreColor, this.TextBackColor, this.TextAlign, bounds);
        }

        /// <summary>
        /// Draws the specified text within the specified bounds using the specified device context, font, color, and back color.
        /// </summary>
        /// <param name="graphics">The device context in which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The <see cref="Font"/> to apply to the drawn text.</param>
        /// <param name="foreColor">The <see cref="Color"/> to apply to the text.</param>
        /// <param name="backColor">The <see cref="Color"/> to apply to the area represented by <c>bounds</c>.</param>
        /// <param name="bounds">The <see cref="Rectangle"/> that represents the bounds of the text.</param>
        protected void DrawLabel(Graphics graphics, string text, Font font, Color foreColor, Color backColor, Rectangle bounds)
        {
            this.DrawLabel(graphics, text, font, foreColor, backColor, this.TextAlign, bounds);
        }

        /// <summary>
        /// Draws the specified text within the specified bounds using the specified device context, font, color, back color, and formatting instructions.
        /// </summary>
        /// <param name="graphics">The device context in which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The <see cref="Font"/> to apply to the drawn text.</param>
        /// <param name="foreColor">The <see cref="Color"/> to apply to the text.</param>
        /// <param name="backColor">The <see cref="Color"/> to apply to the area represented by <c>bounds</c>.</param>
        /// <param name="textAlign">The <see cref="ContentAlignment"/> to apply to the text.</param>
        /// <param name="bounds">The <see cref="Rectangle"/> that represents the bounds of the text.</param>
        protected void DrawLabel(Graphics graphics, string text, Font font, Color foreColor, Color backColor, ContentAlignment textAlign, Rectangle bounds)
        {
            this.DrawLabel(graphics, text, font, foreColor, backColor, textAlign, bounds, this.ScaleText);
        }

        /// <summary>
        /// Draws the specified text within the specified bounds using the specified device context, font, color, back color, and formatting instructions.
        /// </summary>
        /// <param name="graphics">The device context in which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The <see cref="Font"/> to apply to the drawn text.</param>
        /// <param name="foreColor">The <see cref="Color"/> to apply to the text.</param>
        /// <param name="backColor">The <see cref="Color"/> to apply to the area represented by <c>bounds</c>.</param>
        /// <param name="textAlign">The <see cref="ContentAlignment"/> to apply to the text.</param>
        /// <param name="bounds">The <see cref="Rectangle"/> that represents the bounds of the text.</param>
        /// <param name="scaleText">If set to <c>true</c> the font size is scaled according to the current zoom level.</param>
        protected virtual void DrawLabel(Graphics graphics, string text, Font font, Color foreColor, Color backColor, ContentAlignment textAlign, Rectangle bounds, bool scaleText)
        {
            this.DrawLabel(graphics, text, font, foreColor, backColor, textAlign, bounds, scaleText, Padding.Empty);
        }

        /// <summary>
        /// Draws the specified text within the specified bounds using the specified device context, font, color, back color, and formatting instructions.
        /// </summary>
        /// <param name="graphics">The device context in which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The <see cref="Font"/> to apply to the drawn text.</param>
        /// <param name="foreColor">The <see cref="Color"/> to apply to the text.</param>
        /// <param name="backColor">The <see cref="Color"/> to apply to the area represented by <c>bounds</c>.</param>
        /// <param name="textAlign">The <see cref="ContentAlignment"/> to apply to the text.</param>
        /// <param name="bounds">The <see cref="Rectangle"/> that represents the bounds of the text.</param>
        /// <param name="scaleText">If set to <c>true</c> the font size is scaled according to the current zoom level.</param>
        /// <param name="padding">Padding to apply around the text</param>
        protected virtual void DrawLabel(Graphics graphics, string text, Font font, Color foreColor, Color backColor, ContentAlignment textAlign, Rectangle bounds, bool scaleText, Padding padding)
        {
            TextFormatFlags flags;

            if (scaleText)
            {
                font = new Font(font.FontFamily, (float)(font.Size * this.ZoomFactor), font.Style);
            }

            flags = TextFormatFlags.NoPrefix | TextFormatFlags.WordEllipsis | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;

            switch (textAlign)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    flags |= TextFormatFlags.Left;
                    break;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    flags |= TextFormatFlags.Right;
                    break;
                default:
                    flags |= TextFormatFlags.HorizontalCenter;
                    break;
            }

            switch (textAlign)
            {
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopRight:
                    flags |= TextFormatFlags.Top;
                    break;
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomRight:
                    flags |= TextFormatFlags.Bottom;
                    break;
                default:
                    flags |= TextFormatFlags.VerticalCenter;
                    break;
            }

            if (padding.Horizontal != 0 || padding.Vertical != 0)
            {
                Size size;
                int x;
                int y;
                int width;
                int height;

                size = TextRenderer.MeasureText(graphics, text, font, bounds.Size, flags);
                width = size.Width;
                height = size.Height;

                switch (textAlign)
                {
                    case ContentAlignment.TopLeft:
                        x = bounds.Left + padding.Left;
                        y = bounds.Top + padding.Top;
                        break;
                    case ContentAlignment.TopCenter:
                        x = bounds.Left + padding.Left + (((bounds.Width - width) / 2) - padding.Right);
                        y = bounds.Top + padding.Top;
                        break;
                    case ContentAlignment.TopRight:
                        x = bounds.Right - (padding.Right + width);
                        y = bounds.Top + padding.Top;
                        break;
                    case ContentAlignment.MiddleLeft:
                        x = bounds.Left + padding.Left;
                        y = bounds.Top + padding.Top + ((bounds.Height - height) / 2);
                        break;
                    case ContentAlignment.MiddleCenter:
                        x = bounds.Left + padding.Left + (((bounds.Width - width) / 2) - padding.Right);
                        y = bounds.Top + padding.Top + ((bounds.Height - height) / 2);
                        break;
                    case ContentAlignment.MiddleRight:
                        x = bounds.Right - (padding.Right + width);
                        y = bounds.Top + padding.Top + ((bounds.Height - height) / 2);
                        break;
                    case ContentAlignment.BottomLeft:
                        x = bounds.Left + padding.Left;
                        y = bounds.Bottom - (padding.Bottom + height);
                        break;
                    case ContentAlignment.BottomCenter:
                        x = bounds.Left + padding.Left + (((bounds.Width - width) / 2) - padding.Right);
                        y = bounds.Bottom - (padding.Bottom + height);
                        break;
                    case ContentAlignment.BottomRight:
                        x = bounds.Right - (padding.Right + width);
                        y = bounds.Bottom - (padding.Bottom + height);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("textAlign");
                }

                if (backColor != Color.Empty && backColor.A > 0)
                {
                    using (Brush brush = new SolidBrush(backColor))
                    {
                        graphics.FillRectangle(brush, x - padding.Left, y - padding.Top, width + padding.Horizontal, height + padding.Vertical);
                    }
                }

                bounds = new Rectangle(x, y, width, height);

                //bounds = new Rectangle(bounds.Left + padding.Left, bounds.Top + padding.Top, bounds.Width - padding.Horizontal, bounds.Height - padding.Vertical);
            }

            TextRenderer.DrawText(graphics, text, font, bounds, foreColor, backColor, flags);

            if (scaleText)
            {
                font.Dispose();
            }
        }

        /// <summary>
        ///   Draws a pixel grid.
        /// </summary>
        /// <param name="g">The graphics to draw the grid to.</param>
        protected virtual void DrawPixelGrid(Graphics g)
        {
            float pixelSize;

            pixelSize = (float)this.ZoomFactor;

            if (pixelSize > this.PixelGridThreshold)
            {
                Rectangle viewport;
                float offsetX;
                float offsetY;

                viewport = this.GetImageViewPort();
                offsetX = Math.Abs(this.AutoScrollPosition.X) % pixelSize;
                offsetY = Math.Abs(this.AutoScrollPosition.Y) % pixelSize;

                using (Pen pen = new Pen(this.PixelGridColor)
                                 {
                                     DashStyle = DashStyle.Dot
                                 })
                {
                    for (float x = viewport.Left + pixelSize - offsetX; x < viewport.Right; x += pixelSize)
                    {
                        g.DrawLine(pen, x, viewport.Top, x, viewport.Bottom);
                    }

                    for (float y = viewport.Top + pixelSize - offsetY; y < viewport.Bottom; y += pixelSize)
                    {
                        g.DrawLine(pen, viewport.Left, y, viewport.Right, y);
                    }

                    g.DrawRectangle(pen, viewport);
                }
            }
        }

        /// <summary>
        ///   Draws the selection region.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.Windows.Forms.PaintEventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void DrawSelection(PaintEventArgs e)
        {
            RectangleF rect;

            e.Graphics.SetClip(this.GetInsideViewPort(true));

            rect = this.GetOffsetRectangle(this.SelectionRegion);

            using (Brush brush = new SolidBrush(Color.FromArgb(128, this.SelectionColor)))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            using (Pen pen = new Pen(this.SelectionColor))
            {
                e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }

            e.Graphics.ResetClip();
        }

        /// <summary>
        /// Draws the text.
        /// </summary>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        protected virtual void DrawText(PaintEventArgs e)
        {
            Rectangle bounds;

            bounds = this.TextDisplayMode == ImageBoxGridDisplayMode.Client ? this.GetInsideViewPort() : this.GetImageViewPort();

            this.DrawLabel(e.Graphics, this.Text, this.Font, this.ForeColor, this.TextBackColor, this.TextAlign, bounds, this.ScaleText, this.TextPadding);
        }

        /// <summary>
        /// Completes an ongoing selection or drag operation.
        /// </summary>
        protected virtual void EndDrag()
        {
            this.IsSelecting = false;
            this.OnSelected(EventArgs.Empty);
        }

        /// <summary>
        ///   Gets an offset based on the current image border style.
        /// </summary>
        /// <returns></returns>
        protected virtual int GetImageBorderOffset()
        {
            int offset;

            switch (this.ImageBorderStyle)
            {
                case ImageBoxBorderStyle.FixedSingle:
                    offset = 1;
                    break;

                case ImageBoxBorderStyle.FixedSingleDropShadow:
                    offset = (this.DropShadowSize + 1);
                    break;
                default:
                    offset = 0;
                    break;
            }

            return offset;
        }

        /// <summary>
        /// Determines a suitable interpolation mode based in the value of the <see cref="InterpolationMode"/> and <see cref="Zoom"/> properties.
        /// </summary>
        /// <returns>A <see cref="InterpolationMode"/> for rendering the image.</returns>
        protected virtual InterpolationMode GetInterpolationMode()
        {
            InterpolationMode mode;

            mode = this.InterpolationMode;

            if (mode == InterpolationMode.Default)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (this.Zoom < 100)
                {
                    // TODO: Check to see if we should cherry pick other modes depending on how much the image is actually zoomed
                    mode = InterpolationMode.HighQualityBicubic;
                }
                else
                {
                    mode = InterpolationMode.NearestNeighbor;
                }
            }

            return mode;
        }

        /// <summary>
        ///   Raises the <see cref="AllowClickZoomChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnAllowClickZoomChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.AllowClickZoomChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="AllowDoubleClickChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnAllowDoubleClickChanged(EventArgs e)
        {
            EventHandler handler;

            this.SetStyle(ControlStyles.StandardDoubleClick, this.AllowDoubleClick);

            handler = this.AllowDoubleClickChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="AllowZoomChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnAllowZoomChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.AllowZoomChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="AutoCenterChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnAutoCenterChanged(EventArgs e)
        {
            EventHandler handler;

            this.Invalidate();

            handler = this.AutoCenterChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="AutoPanChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnAutoPanChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.AutoPanChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="DropShadowSizeChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnDropShadowSizeChanged(EventArgs e)
        {
            this.Invalidate();

            EventHandler handler;

            handler = this.DropShadowSizeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="GridCellSizeChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnGridCellSizeChanged(EventArgs e)
        {
            EventHandler handler;

            this.InitializeGridTile();

            handler = this.GridCellSizeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="GridColorAlternateChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnGridColorAlternateChanged(EventArgs e)
        {
            EventHandler handler;

            this.InitializeGridTile();

            handler = this.GridColorAlternateChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="GridColorChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnGridColorChanged(EventArgs e)
        {
            EventHandler handler;

            this.InitializeGridTile();

            handler = this.GridColorChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="GridDisplayModeChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnGridDisplayModeChanged(EventArgs e)
        {
            EventHandler handler;

            this.InitializeGridTile();
            this.Invalidate();

            handler = this.GridDisplayModeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="GridScaleChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnGridScaleChanged(EventArgs e)
        {
            EventHandler handler;

            this.InitializeGridTile();

            handler = this.GridScaleChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="ImageBorderColorChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnImageBorderColorChanged(EventArgs e)
        {
            EventHandler handler;

            this.Invalidate();

            handler = this.ImageBorderColorChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="ImageBorderStyleChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnImageBorderStyleChanged(EventArgs e)
        {
            EventHandler handler;

            this.Invalidate();

            handler = this.ImageBorderStyleChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }
        

        /// <summary>
        ///   Raises the <see cref="ImageChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnImageChanged(EventArgs e)
        {
            EventHandler handler;

            this.DefineViewSize();
            this.IsAnimating = false;

            if (this.Image != null)
            {
                //try
                //{
                //    this.IsAnimating = ImageAnimator.CanAnimate(this.Image);
                //    if (this.IsAnimating)
                //    {
                //        ImageAnimator.Animate(this.Image, this.OnFrameChangedHandler);
                //    }
                //}
                //catch (ArgumentException)
                //{
                //    // probably a disposed image, ignore
                //}

                StartAnimating();
            }

            this.AdjustLayout();

            handler = this.ImageChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Defines the view sized based on the current image and if the control is operating in virtual mode or not
        /// </summary>
        private void DefineViewSize()
        {
            _viewSize = this.VirtualMode ? this.VirtualSize : this.GetImageSize();

            this.UpdateScrollbars();
        }

        /// <summary>
        /// Returns if horizontal scrolling is enabled
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HScroll { get; protected set; }

        /// <summary>
        /// Returns if the vertical scrolling is enabled
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool VScroll { get; protected set; }

        /// <summary>
        /// Updates all properties of the embedded scroll bar controls.
        /// </summary>
        private void UpdateScrollbars()
        {
            Size viewSize;

            viewSize = this.GetInsideViewPort(true).Size;

            if (viewSize.Width > 0 && viewSize.Height > 0)
            {
                ImageBoxScrollProperties horizontal;
                ImageBoxScrollProperties vertical;
                Point autoScrollPosition;
                bool hScroll;
                bool vScroll;
                bool enabled;

                autoScrollPosition = this.AutoScrollPosition;
                hScroll = this.HScroll;
                vScroll = this.VScroll;
                enabled = this.Enabled;

                horizontal = this.HorizontalScroll;
                horizontal.Maximum = this.ScaledImageWidth;
                horizontal.LargeChange = viewSize.Width;
                horizontal.SmallChange = 10;
                horizontal.Value = -autoScrollPosition.X;
                horizontal.Visible = this.ShouldShowScrollbar(this.HorizontalScrollBarStyle, hScroll);
                horizontal.Enabled = enabled && hScroll;

                vertical = this.VerticalScroll;
                vertical.Maximum = this.ScaledImageHeight;
                vertical.LargeChange = viewSize.Height;
                vertical.SmallChange = 10;
                vertical.Value = -autoScrollPosition.Y;
                vertical.Visible = this.ShouldShowScrollbar(this.VerticalScrollBarStyle, vScroll);
                vertical.Enabled = enabled && vScroll;
            }
        }

        /// <summary>
        /// Determines if a scroll bar should be displayed.
        /// </summary>
        /// <param name="style">The user defined style of the scroll bar.</param>
        /// <param name="visible">The visibility state for automatic styles.</param>
        /// <returns><c>true</c> if the scroll bar should be displayed, otherwise <c>false</c>.</returns>
        private bool ShouldShowScrollbar(ImageBoxScrollBarStyle style, bool visible)
        {
            bool result;

            switch (style)
            {
                case ImageBoxScrollBarStyle.Auto:
                    result = visible;
                    break;
                case ImageBoxScrollBarStyle.Show:
                    result = true;
                    break;
                case ImageBoxScrollBarStyle.Hide:
                    result = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("style", style, null);
            }

            return result;
        }

        /// <summary>
        ///   Raises the <see cref="InterpolationModeChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnInterpolationModeChanged(EventArgs e)
        {
            EventHandler handler;

            this.Invalidate();

            handler = this.InterpolationModeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="InvertMouseChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnInvertMouseChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.InvertMouseChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="LimitSelectionToImageChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnLimitSelectionToImageChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.LimitSelectionToImageChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="PanEnd" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnPanEnd(EventArgs e)
        {
            EventHandler handler;

            handler = this.PanEnd;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="PanStart" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.ComponentModel.CancelEventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnPanStart(CancelEventArgs e)
        {
            EventHandler handler;

            handler = this.PanStart;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="PixelGridColorChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnPixelGridColorChanged(EventArgs e)
        {
            EventHandler handler;

            this.Invalidate();

            handler = this.PixelGridColorChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="PixelGridThresholdChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnPixelGridThresholdChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.PixelGridThresholdChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ScaleTextChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnScaleTextChanged(EventArgs e)
        {
            EventHandler handler;

            this.Invalidate();

            handler = this.ScaleTextChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="Selected" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnSelected(EventArgs e)
        {
            EventHandler<EventArgs> handler;

            switch (this.SelectionMode)
            {
                case ImageBoxSelectionMode.Zoom:
                    if (this.SelectionRegion.Width > SelectionDeadZone && this.SelectionRegion.Height > SelectionDeadZone)
                    {
                        this.ZoomToRegion(this.SelectionRegion);
                        this.SelectNone();
                    }
                    break;
            }

            handler = this.Selected;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="Selecting" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnSelecting(ImageBoxCancelEventArgs e)
        {
            EventHandler<ImageBoxCancelEventArgs> handler;

            handler = this.Selecting;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="SelectionColorChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnSelectionColorChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.SelectionColorChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="SelectionModeChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnSelectionModeChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.SelectionModeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="SelectionRegionChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnSelectionRegionChanged(EventArgs e)
        {
            EventHandler handler;

            this.Invalidate();

            handler = this.SelectionRegionChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="ShortcutsEnabledChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnShortcutsEnabledChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.ShortcutsEnabledChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="ShowPixelGridChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnShowPixelGridChanged(EventArgs e)
        {
            EventHandler handler;

            this.Invalidate();

            handler = this.ShowPixelGridChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="SizeModeChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnSizeModeChanged(EventArgs e)
        {
            EventHandler handler;

            this.AdjustLayout();

            handler = this.SizeModeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="SizeToFitChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnSizeToFitChanged(EventArgs e)
        {
            EventHandler handler;

            this.AdjustLayout();

            handler = this.SizeToFitChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="TextAlignChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnTextAlignChanged(EventArgs e)
        {
            EventHandler handler;

            this.Invalidate();

            handler = this.TextAlignChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="TextBackColorChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnTextBackColorChanged(EventArgs e)
        {
            EventHandler handler;

            this.Invalidate();

            handler = this.TextBackColorChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="TextDisplayModeChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnTextDisplayModeChanged(EventArgs e)
        {
            EventHandler handler;

            this.Invalidate();

            handler = this.TextDisplayModeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="TextPaddingChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnTextPaddingChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.TextPaddingChanged;

            this.Invalidate();

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="VirtualDraw" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="PaintEventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnVirtualDraw(PaintEventArgs e)
        {
            PaintEventHandler handler;

            handler = this.VirtualDraw;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="VirtualModeChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnVirtualModeChanged(EventArgs e)
        {
            EventHandler handler;

            this.DefineViewSize();
            this.AdjustLayout();

            handler = this.VirtualModeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="VirtualSizeChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnVirtualSizeChanged(EventArgs e)
        {
            EventHandler handler;

            this.AdjustLayout();

            handler = this.VirtualSizeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="ZoomChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnZoomChanged(EventArgs e)
        {
            EventHandler handler;

            this.AdjustLayout();

            handler = this.ZoomChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="ZoomLevelsChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnZoomLevelsChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.ZoomLevelsChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="Zoomed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnZoomed(ImageBoxZoomEventArgs e)
        {
            EventHandler<ImageBoxZoomEventArgs> handler;

            handler = this.Zoomed;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Processes shortcut keys for zooming and selection
        /// </summary>
        /// <param name="e">
        ///   The <see cref="KeyEventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void ProcessImageShortcuts(KeyEventArgs e)
        {
            Point currentPixel;
            int currentZoom;
            Point relativePoint;

            relativePoint = this.CenterPoint;
            currentPixel = this.PointToImage(relativePoint);
            currentZoom = this.Zoom;

            switch (e.KeyCode)
            {
                //case Keys.Home:
                //    if (this.AllowZoom)
                //    {
                //        this.PerformActualSize(ImageBoxActionSources.User);
                //    }
                //    break;

                //case Keys.PageDown:
                case Keys.Oemplus:
                    if (this.AllowZoom)
                    {
                        this.PerformZoomIn(ImageBoxActionSources.User, true);
                    }
                    break;

                //case Keys.PageUp:
                case Keys.OemMinus:
                    if (this.AllowZoom)
                    {
                        this.PerformZoomOut(ImageBoxActionSources.User, true);
                    }
                    break;
            }

            if (this.Zoom != currentZoom)
            {
                this.ScrollTo(currentPixel, relativePoint);
            }
        }

        /// <summary>
        ///   Processes zooming with the mouse. Attempts to keep the pre-zoom image pixel under the mouse after the zoom has completed.
        /// </summary>
        /// <param name="isZoomIn">
        ///   if set to <c>true</c> zoom in, otherwise zoom out.
        /// </param>
        /// <param name="cursorPosition">The cursor position.</param>
        protected virtual void ProcessMouseZoom(bool isZoomIn, Point cursorPosition)
        {
            this.PerformZoom(isZoomIn ? ImageBoxZoomActions.ZoomIn : ImageBoxZoomActions.ZoomOut, ImageBoxActionSources.User, true, cursorPosition);
        }

        /// <summary>
        ///   Performs mouse based panning
        /// </summary>
        /// <param name="e">
        ///   The <see cref="MouseEventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void ProcessPanning(MouseEventArgs e)
        {
            if (this.AutoPan && !this.ViewSize.IsEmpty && this.SelectionMode == ImageBoxSelectionMode.None)
            {
                Size clientSize;

                clientSize = this.GetInsideViewPort(true).Size;

                if (!this.IsPanning && (this.ScaledImageWidth > clientSize.Width || this.ScaledImageHeight > clientSize.Height))
                {
                    _startMousePosition = e.Location;
                    this.IsPanning = true;
                }

                if (this.IsPanning)
                {
                    int x;
                    int y;
                    Point position;

                    if (!this.InvertMouse)
                    {
                        x = -_startScrollPosition.X + (_startMousePosition.X - e.Location.X);
                        y = -_startScrollPosition.Y + (_startMousePosition.Y - e.Location.Y);
                    }
                    else
                    {
                        x = -(_startScrollPosition.X + (_startMousePosition.X - e.Location.X));
                        y = -(_startScrollPosition.Y + (_startMousePosition.Y - e.Location.Y));
                    }

                    position = new Point(x, y);

                    this.UpdateScrollPosition(position);
                }
            }
        }

        /// <summary>
        ///   Processes shortcut keys for scrolling
        /// </summary>
        /// <param name="e">
        ///   The <see cref="KeyEventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void ProcessScrollingShortcuts(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    this.AdjustScroll(-(e.Modifiers == Keys.None ? this.HorizontalScroll.SmallChange : this.HorizontalScroll.LargeChange), 0);
                    break;

                case Keys.Right:
                    this.AdjustScroll(e.Modifiers == Keys.None ? this.HorizontalScroll.SmallChange : this.HorizontalScroll.LargeChange, 0);
                    break;

                case Keys.Up:
                    this.AdjustScroll(0, -(e.Modifiers == Keys.None ? this.VerticalScroll.SmallChange : this.VerticalScroll.LargeChange));
                    break;

                case Keys.Down:
                    this.AdjustScroll(0, e.Modifiers == Keys.None ? this.VerticalScroll.SmallChange : this.VerticalScroll.LargeChange);
                    break;
            }
        }

        /// <summary>
        /// Gets the characteristics associated with the horizontal scroll bar.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ImageBoxScrollProperties HorizontalScroll { get; private set; }

        /// <summary>
        /// Gets the characteristics associated with the vertical scroll bar.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ImageBoxScrollProperties VerticalScroll { get; private set; }

        /// <summary>
        ///   Performs mouse based region selection
        /// </summary>
        /// <param name="e">
        ///   The <see cref="MouseEventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void ProcessSelection(MouseEventArgs e)
        {
            if (this.SelectionMode != ImageBoxSelectionMode.None && e.Button == MouseButtons.Left && !this.WasDragCancelled)
            {
                if (!this.IsSelecting)
                {
                    this.StartDrag(e);
                }

                if (this.IsSelecting)
                {
                    float x;
                    float y;
                    float w;
                    float h;
                    Point imageOffset;
                    RectangleF selection;

                    imageOffset = this.GetImageViewPort().Location;

                    if (e.X < _startMousePosition.X)
                    {
                        x = e.X;
                        w = _startMousePosition.X - e.X;
                    }
                    else
                    {
                        x = _startMousePosition.X;
                        w = e.X - _startMousePosition.X;
                    }

                    if (e.Y < _startMousePosition.Y)
                    {
                        y = e.Y;
                        h = _startMousePosition.Y - e.Y;
                    }
                    else
                    {
                        y = _startMousePosition.Y;
                        h = e.Y - _startMousePosition.Y;
                    }

                    x = x - imageOffset.X - this.AutoScrollPosition.X;
                    y = y - imageOffset.Y - this.AutoScrollPosition.Y;

                    x = x / (float)this.ZoomFactor;
                    y = y / (float)this.ZoomFactor;
                    w = w / (float)this.ZoomFactor;
                    h = h / (float)this.ZoomFactor;

                    selection = new RectangleF(x, y, w, h);
                    if (this.LimitSelectionToImage)
                    {
                        selection = this.FitRectangle(selection);
                    }

                    this.SelectionRegion = selection;
                }
            }
        }

        /// <summary>
        /// Resets the <see cref="SizeMode"/> property whilsts retaining the original <see cref="Zoom"/>.
        /// </summary>
        protected void RestoreSizeMode()
        {
            if (this.SizeMode != ImageBoxSizeMode.Normal)
            {
                int previousZoom;

                previousZoom = this.Zoom;
                this.SizeMode = ImageBoxSizeMode.Normal;
                this.Zoom = previousZoom; // Stop the zoom getting reset to 100% before calculating the new zoom
            }
        }

        /// <summary>
        /// Initializes a selection or drag operation.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        protected virtual void StartDrag(MouseEventArgs e)
        {
            ImageBoxCancelEventArgs args;

            args = new ImageBoxCancelEventArgs(e.Location);

            this.OnSelecting(args);

            this.WasDragCancelled = args.Cancel;
            this.IsSelecting = !args.Cancel;
            if (this.IsSelecting)
            {
                this.SelectNone();

                _startMousePosition = e.Location;
            }
        }

        /// <summary>
        ///   Updates the scroll position.
        /// </summary>
        /// <param name="position">The position.</param>
        protected virtual void UpdateScrollPosition(Point position)
        {
            this.AutoScrollPosition = position;

            this.Invalidate();

            this.OnScroll(new ScrollEventArgs(ScrollEventType.EndScroll, 0));
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Gets the size of the image.
        /// </summary>
        /// <remarks>If an error occurs, for example due to the image being disposed, an empty size is returned</remarks>
        /// <returns>Size.</returns>
        private Size GetImageSize()
        {
            Size result;

            // HACK: This whole thing stinks. Hey MS, how about an IsDisposed property for images?

            if (this.Image != null)
            {
                try
                {
                    result = this.Image.Size;
                }
                catch
                {
                    result = Size.Empty;
                }
            }
            else
            {
                result = Size.Empty;
            }

            return result;
        }

        /// <summary>
        ///   Initializes the grid tile.
        /// </summary>
        private void InitializeGridTile()
        {
            if (_texture != null)
            {
                _texture.Dispose();
            }

            if (_gridTile != null)
            {
                _gridTile.Dispose();
            }

            if (this.GridDisplayMode != ImageBoxGridDisplayMode.None && this.GridCellSize != 0)
            {
                if (this.GridScale != ImageBoxGridScale.None)
                {
                    _gridTile = this.CreateGridTileImage(this.GridCellSize, this.GridColor, this.GridColorAlternate);
                    _texture = new TextureBrush(_gridTile);
                }
                else
                {
                    _texture = new SolidBrush(this.GridColor);
                }
            }

            this.Invalidate();
        }

        /// <summary>
        /// Called when the animation frame changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnFrameChangedHandler(object sender, EventArgs eventArgs)
        {
            this.Invalidate();
        }

        /// <summary>
        /// Resets the zoom to 100%.
        /// </summary>
        /// <param name="source">The source that initiated the action.</param>
        private void PerformActualSize(ImageBoxActionSources source)
        {
            this.SizeMode = ImageBoxSizeMode.Normal;
            this.SetZoom(100, ImageBoxZoomActions.ActualSize | (this.Zoom < 100 ? ImageBoxZoomActions.ZoomIn : ImageBoxZoomActions.ZoomOut), source);
        }

        /// <summary>
        /// Zooms into the image
        /// </summary>
        /// <param name="source">The source that initiated the action.</param>
        /// <param name="preservePosition"><c>true</c> if the current scrolling position should be preserved relative to the new zoom level, <c>false</c> to reset.</param>
        private void PerformZoomIn(ImageBoxActionSources source, bool preservePosition)
        {
            this.PerformZoom(ImageBoxZoomActions.ZoomIn, source, preservePosition);
        }

        /// <summary>
        /// Performs a zoom action.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="source">The source that initiated the action.</param>
        /// <param name="preservePosition"><c>true</c> if the current scrolling position should be preserved relative to the new zoom level, <c>false</c> to reset.</param>
        private void PerformZoom(ImageBoxZoomActions action, ImageBoxActionSources source, bool preservePosition)
        {
            this.PerformZoom(action, source, preservePosition, this.CenterPoint);
        }

        /// <summary>
        /// Performs a zoom action.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="source">The source that initiated the action.</param>
        /// <param name="preservePosition"><c>true</c> if the current scrolling position should be preserved relative to the new zoom level, <c>false</c> to reset.</param>
        /// <param name="relativePoint">A <see cref="Point"/> describing the current center of the control.</param>
        private void PerformZoom(ImageBoxZoomActions action, ImageBoxActionSources source, bool preservePosition, Point relativePoint)
        {
            Point currentPixel;
            int currentZoom;
            int newZoom;

            currentPixel = this.PointToImage(relativePoint);
            currentZoom = this.Zoom;
            newZoom = this.GetZoomLevel(action);

            this.RestoreSizeMode();
            this.SetZoom(newZoom, action, source);

            if (preservePosition && this.Zoom != currentZoom)
            {
                this.ScrollTo(currentPixel, relativePoint);
            }
        }

        /// <summary>
        /// Returns an appropriate zoom level based on the specified action, relative to the current zoom level.
        /// </summary>
        /// <param name="action">The action to determine the zoom level.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if an unsupported action is specified.</exception>
        private int GetZoomLevel(ImageBoxZoomActions action)
        {
            int result;

            switch (action)
            {
                case ImageBoxZoomActions.None:
                    result = this.Zoom;
                    break;
                case ImageBoxZoomActions.ZoomIn:
                    result = this.ZoomLevels.NextZoom(this.Zoom);
                    break;
                case ImageBoxZoomActions.ZoomOut:
                    result = this.ZoomLevels.PreviousZoom(this.Zoom);
                    break;
                case ImageBoxZoomActions.ActualSize:
                    result = 100;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("action");
            }

            return result;
        }

        /// <summary>
        /// Zooms out of the image
        /// </summary>
        /// <param name="source">The source that initiated the action.</param>
        /// <param name="preservePosition"><c>true</c> if the current scrolling position should be preserved relative to the new zoom level, <c>false</c> to reset.</param>
        private void PerformZoomOut(ImageBoxActionSources source, bool preservePosition)
        {
            this.PerformZoom(ImageBoxZoomActions.ZoomOut, source, preservePosition);
        }

        /// <summary>
        /// Updates the current zoom.
        /// </summary>
        /// <param name="value">The new zoom value.</param>
        /// <param name="actions">The zoom actions that caused the value to be updated.</param>
        /// <param name="source">The source of the zoom operation.</param>
        private void SetZoom(int value, ImageBoxZoomActions actions, ImageBoxActionSources source)
        {
            int previousZoom;

            previousZoom = this.Zoom;

            if (value < MinZoom)
            {
                value = MinZoom;
            }
            else if (value > MaxZoom)
            {
                value = MaxZoom;
            }

            if (_zoom != value)
            {
                _zoom = value;

                this.OnZoomChanged(EventArgs.Empty);

                this.OnZoomed(new ImageBoxZoomEventArgs(actions, source, previousZoom, this.Zoom));
            }
        }

        #endregion

        private bool _allowUnfocusedMouseWheel;

        /// <summary>
        /// Gets or sets a value indicating whether the control can respond to mouse wheel events regardless of if the control has focus or not.
        /// </summary>
        [Category("Behavior"), DefaultValue(false)]
        public virtual bool AllowUnfocusedMouseWheel
        {
            get { return _allowUnfocusedMouseWheel; }
            set
            {
                if (this.AllowUnfocusedMouseWheel != value)
                {
                    _allowUnfocusedMouseWheel = value;

                    this.OnAllowUnfocusedMouseWheelChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the AllowUnfocusedMouseWheel property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler AllowUnfocusedMouseWheelChanged;

        /// <summary>
        /// Raises the <see cref="AllowUnfocusedMouseWheelChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnAllowUnfocusedMouseWheelChanged(EventArgs e)
        {
            EventHandler handler;

            if (this.AllowUnfocusedMouseWheel)
            {
                // TODO: Not doing any reference counting so there's
                // currently no way of disabling the message filter
                // after the first time it has been enabled
                ImageBoxMouseWheelMessageFilter.Active = true;
            }

            handler = this.AllowUnfocusedMouseWheelChanged;

            if (handler != null)
                handler(this, e);
        }

        private Point _autoScrollPosition;

        private bool _updatingPosition;

        /// <summary>
        /// Gets or sets the location of the auto-scroll position.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Category("Layout")]
        public Point AutoScrollPosition
        {
            get { return _autoScrollPosition; }
            set
            {
                if (!_updatingPosition)
                {
                    try
                    {
                        int maxH;
                        int maxW;

                        _updatingPosition = true;

                        maxW = this.HScroll ? this.ScaledImageWidth - this.HorizontalScroll.LargeChange : 0;
                        maxH = this.VScroll ? this.ScaledImageHeight - this.VerticalScroll.LargeChange : 0;

                        value = new Point(-this.Clamp(value.X, 0, maxW), -this.Clamp(value.Y, 0, maxH));

                        if (_autoScrollPosition != value)
                        {
                            Debug.WriteLine(value);

                            _autoScrollPosition = value;

                            this.UpdateScrollbars();

                            this.Invalidate();
                        }
                    }
                    finally
                    {
                        _updatingPosition = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Scroll event of the embedded horizontal and vertical scroll bar controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="ScrollEventArgs"/> that contains the event data.</param>
        private void ScrollBarScrollHandler(object sender, ScrollEventArgs e)
        {
            this.UpdateScrollPosition(new Point(_hScrollBar.Value, _vScrollBar.Value));
        }


        /// <summary>
        ///   Occurs when the user or code scrolls through the client area.
        /// </summary>
        [Category("Action")]
        public event ScrollEventHandler Scroll;

        /// <summary>
        ///   Raises the <see cref="Scroll" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="ScrollEventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnScroll(ScrollEventArgs e)
        {
            ScrollEventHandler handler;

            handler = this.Scroll;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private ImageBoxScrollBarStyle _horizontalScrollBarStyle;

        /// <summary>
        /// Gets or sets the style of the horizontal scroll bar.
        /// </summary>
        [Category("Behavior"), DefaultValue(typeof(ImageBoxScrollBarStyle), "Auto")]
        public virtual ImageBoxScrollBarStyle HorizontalScrollBarStyle
        {
            get { return _horizontalScrollBarStyle; }
            set
            {
                if (this.HorizontalScrollBarStyle != value)
                {
                    _horizontalScrollBarStyle = value;

                    this.OnHorizontalScrollBarStyleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the HorizontalScrollBarStyle property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler HorizontalScrollBarStyleChanged;

        /// <summary>
        /// Raises the <see cref="HorizontalScrollBarStyleChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnHorizontalScrollBarStyleChanged(EventArgs e)
        {
            EventHandler handler;

            this.AdjustViewPort();
            this.Invalidate();

            handler = this.HorizontalScrollBarStyleChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private ImageBoxScrollBarStyle _verticalScrollBarStyle;

        /// <summary>
        /// Gets or sets the style of the vertical scroll bar.
        /// </summary>
        [Category("Behavior"), DefaultValue(typeof(ImageBoxScrollBarStyle), "Auto")]
        public virtual ImageBoxScrollBarStyle VerticalScrollBarStyle
        {
            get { return _verticalScrollBarStyle; }
            set
            {
                if (this.VerticalScrollBarStyle != value)
                {
                    _verticalScrollBarStyle = value;

                    this.OnVerticalScrollBarStyleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the VerticalScrollBarStyle property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler VerticalScrollBarStyleChanged;

        /// <summary>
        /// Raises the <see cref="VerticalScrollBarStyleChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnVerticalScrollBarStyleChanged(EventArgs e)
        {
            EventHandler handler;

            this.AdjustViewPort();
            this.Invalidate();

            handler = this.VerticalScrollBarStyleChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }


    }
}