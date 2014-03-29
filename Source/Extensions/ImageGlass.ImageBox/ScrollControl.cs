using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

#if USEWIN32PINVOKELIB
using Cyotek.Win32;

#endif

// Original ScrollControl code by Scott Crawford (http://sukiware.com/)

namespace ImageGlass
{
    /// <summary>
    /// Defines a base class for controls that support scrolling behavior.
    /// </summary>
    [ToolboxItem(false)]
    public partial class ScrollControl : Control
    {
        #region Instance Fields

        private bool _alwaysShowHScroll;

        private bool _alwaysShowVScroll;

        private BorderStyle _borderStyle;

        private Size _pageSize;

        private Size _scrollSize;

        private Size _stepSize;

        #endregion

        #region Public Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="ScrollControl" /> class.
        /// </summary>
        public ScrollControl()
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            this.BorderStyle = BorderStyle.Fixed3D;
            this.ScrollSize = Size.Empty;
            this.PageSize = Size.Empty;
            this.StepSize = new Size(10, 10);
            this.HorizontalScroll = new HScrollProperties(this);
            this.VerticalScroll = new VScrollProperties(this);
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        #endregion

        #region Events

        /// <summary>
        ///   Occurs when the BorderStyle property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler BorderStyleChanged;

        /// <summary>
        /// Occurs when the mouse wheel moves while the control has focus.
        /// </summary>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Mouse")]
        public new event MouseEventHandler MouseWheel;

        /// <summary>
        ///   Occurs when the PageSize property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler PageSizeChanged;

        /// <summary>
        ///   Occurs when the user or code scrolls through the client area.
        /// </summary>
        [Category("Action")]
        public event ScrollEventHandler Scroll;

        /// <summary>
        ///   Occurs when the ScrollSize property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler ScrollSizeChanged;

        /// <summary>
        ///   Occurs when the StepSize property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler StepSizeChanged;

        #endregion

        #region Overridden Properties

        /// <summary>
        ///   Gets the required creation parameters when the control handle is created.
        /// </summary>
        /// <value>The create params.</value>
        /// <returns>
        ///   A <see cref="T:System.Windows.Forms.CreateParams" /> that contains the required creation parameters when the handle to the control is created.
        /// </returns>
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

        #region Overridden Methods

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.EnabledChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   An <see cref="T:System.EventArgs" /> that contains the event data.
        /// </param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            this.UpdateScrollbars();
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
        ///   Raises the <see cref="System.Windows.Forms.Control.MouseWheel" /> event.
        /// </summary>
        /// <param name="e">
        ///   A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            MouseEventHandler handler;

            if (this.WheelScrollsControl)
            {
                int x;
                int y;
                int delta;

                x = this.HorizontalScroll.Value;
                y = this.VerticalScroll.Value;

                // TODO: Find if we are hovering over a horizontal scrollbar and scroll that instead of the default vertical.
                if (this.VerticalScroll.Visible && this.VerticalScroll.Enabled)
                {
                    if (ModifierKeys == Keys.Control)
                    {
                        delta = this.VerticalScroll.LargeChange;
                    }
                    else
                    {
                        delta = SystemInformation.MouseWheelScrollLines * this.VerticalScroll.SmallChange;
                    }

                    y += (e.Delta > 0) ? -delta : delta;
                }
                else if (this.HorizontalScroll.Visible && this.HorizontalScroll.Enabled)
                {
                    if (ModifierKeys == Keys.Control)
                    {
                        delta = this.HorizontalScroll.LargeChange;
                    }
                    else
                    {
                        delta = SystemInformation.MouseWheelScrollLines * this.HorizontalScroll.SmallChange;
                    }

                    x += (e.Delta > 0) ? -delta : delta;
                }

                this.ScrollTo(x, y);
            }

            handler = this.MouseWheel;
            if (handler != null)
            {
                handler(this, e);
            }

            base.OnMouseWheel(e);
        }

        /// <summary>
        ///   Processes Windows messages.
        /// </summary>
        /// <param name="msg">
        ///   The Windows <see cref="T:System.Windows.Forms.Message" /> to process.
        /// </param>
        [DebuggerStepThrough]
        protected override void WndProc(ref Message msg)
        {
            switch (msg.Msg)
            {
                case NativeMethods.WM_HSCROLL:
                case NativeMethods.WM_VSCROLL:
                    this.WmScroll(ref msg);
                    break;
                default:
                    base.WndProc(ref msg);
                    break;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets a value indicating whether the horizontal scrollbar should always be displayed, even when not required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the horizontal scrollbar should always be displayed; otherwise, <c>false</c>.
        /// </value>
        [Category("Layout")]
        [DefaultValue(false)]
        public bool AlwaysShowHScroll
        {
            get { return _alwaysShowHScroll; }
            set
            {
                if (_alwaysShowHScroll != value)
                {
                    _alwaysShowHScroll = value;

                    if (value)
                    {
                        NativeMethods.SCROLLINFO scrollInfo;

                        scrollInfo = new NativeMethods.SCROLLINFO();
                        scrollInfo.fMask = NativeMethods.SIF.SIF_RANGE | NativeMethods.SIF.SIF_DISABLENOSCROLL;
                        scrollInfo.nMin = 0;
                        scrollInfo.nMax = 0;
                        scrollInfo.nPage = 1;
                        this.SetScrollInfo(ScrollOrientation.HorizontalScroll, scrollInfo, false);

                        this.Invalidate();
                    }
                    else
                    {
                        this.UpdateHorizontalScrollbar();
                    }
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the vertical scrollbar should always be displayed, even when not required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the vertical scrollbar should always be displayed; otherwise, <c>false</c>.
        /// </value>
        [Category("Layout")]
        [DefaultValue(false)]
        public bool AlwaysShowVScroll
        {
            get { return _alwaysShowVScroll; }
            set
            {
                bool shown = VScroll;

                _alwaysShowVScroll = value;
                if (_alwaysShowVScroll != shown)
                {
                    if (_alwaysShowVScroll)
                    {
                        NativeMethods.SCROLLINFO scrollInfo;

                        scrollInfo = new NativeMethods.SCROLLINFO();

                        scrollInfo.fMask = NativeMethods.SIF.SIF_RANGE | NativeMethods.SIF.SIF_DISABLENOSCROLL;
                        scrollInfo.nMin = 0;
                        scrollInfo.nMax = 0;
                        scrollInfo.nPage = 1;
                        this.SetScrollInfo(ScrollOrientation.VerticalScroll, scrollInfo, false);

                        this.Invalidate();
                    }
                    else
                    {
                        this.UpdateVerticalScrollbar();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the border style.
        /// </summary>
        /// <value>The border style.</value>
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
        ///   Gets the horizontal scrollbar properties.
        /// </summary>
        /// <value>The horizontal scrollbar properties.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public HScrollProperties HorizontalScroll { get; protected set; }

        /// <summary>
        ///   Gets or sets the size of the scroll pages.
        /// </summary>
        /// <value>The size of the page.</value>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Size PageSize
        {
            get { return _pageSize; }
            set
            {
                if (value.Width < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Width must be a positive integer.");
                }

                if (value.Height < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Height must be a positive integer.");
                }

                if (this.PageSize != value)
                {
                    _pageSize = value;

                    this.OnPageSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the size of the scroll area.
        /// </summary>
        /// <value>The size of the scroll.</value>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Size ScrollSize
        {
            get { return _scrollSize; }
            set
            {
                if (value.Width < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Width must be a positive integer.");
                }

                if (value.Height < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Height must be a positive integer.");
                }

                if (this.ScrollSize != value)
                {
                    _scrollSize = value;

                    this.OnScrollSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the size of scrollbar stepping.
        /// </summary>
        /// <value>The size of the step.</value>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        [Category("Layout")]
        [DefaultValue(typeof(Size), "10, 10")]
        public virtual Size StepSize
        {
            get { return _stepSize; }
            set
            {
                if (value.Width < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Width must be a positive integer.");
                }

                if (value.Height < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Height must be a positive integer.");
                }

                if (this.StepSize != value)
                {
                    _stepSize = value;

                    this.OnStepSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///   Gets the vertical scrollbar properties.
        /// </summary>
        /// <value>The vertical scrollbar properties.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public VScrollProperties VerticalScroll { get; protected set; }

        #endregion

        #region Protected Properties

        /// <summary>
        ///   Gets or sets a value indicating whether the horizontal scrollbar is displayed
        /// </summary>
        /// <value>
        ///   <c>true</c> if the horizontal scrollbar is displayed; otherwise, <c>false</c>.
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        protected bool HScroll
        {
            get { return (NativeMethods.GetWindowLong(this.Handle, NativeMethods.GWL_STYLE) & NativeMethods.WS_HSCROLL) == NativeMethods.WS_HSCROLL; }
            set
            {
                uint longValue = NativeMethods.GetWindowLong(this.Handle, NativeMethods.GWL_STYLE);

                if (value)
                {
                    longValue |= NativeMethods.WS_HSCROLL;
                }
                else
                {
                    unchecked
                    {
                        longValue &= (uint)~NativeMethods.WS_HSCROLL;
                    }
                }
                NativeMethods.SetWindowLong(this.Handle, NativeMethods.GWL_STYLE, longValue);
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the vertical scrollbar is displayed
        /// </summary>
        /// <value>
        ///   <c>true</c> if the vertical scrollbar is displayed; otherwise, <c>false</c>.
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        protected bool VScroll
        {
            get { return (NativeMethods.GetWindowLong(this.Handle, NativeMethods.GWL_STYLE) & NativeMethods.WS_VSCROLL) == NativeMethods.WS_VSCROLL; }
            set
            {
                uint longValue = NativeMethods.GetWindowLong(this.Handle, NativeMethods.GWL_STYLE);

                if (value)
                {
                    longValue |= NativeMethods.WS_VSCROLL;
                }
                else
                {
                    unchecked
                    {
                        longValue &= (uint)~NativeMethods.WS_VSCROLL;
                    }
                }
                NativeMethods.SetWindowLong(this.Handle, NativeMethods.GWL_STYLE, longValue);
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the control is scrolled when the mouse wheel is spun
        /// </summary>
        /// <value>
        ///   <c>true</c> if the mouse wheel scrolls the control; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected bool WheelScrollsControl { get; set; }

        #endregion

        #region Public Members

        /// <summary>
        ///   Scrolls both scrollbars to the given values
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public void ScrollTo(int x, int y)
        {
            this.ScrollTo(ScrollOrientation.HorizontalScroll, x);
            this.ScrollTo(ScrollOrientation.VerticalScroll, y);
        }

        #endregion

        #region Protected Members

        /// <summary>
        ///   Gets the type of scrollbar event.
        /// </summary>
        /// <param name="wParam">The wparam value from a window proc.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        protected ScrollEventType GetEventType(IntPtr wParam)
        {
            switch (wParam.ToInt32() & 0xFFFF)
            {
                case NativeMethods.SB_BOTTOM:
                    return ScrollEventType.Last;
                case NativeMethods.SB_ENDSCROLL:
                    return ScrollEventType.EndScroll;
                case NativeMethods.SB_LINEDOWN:
                    return ScrollEventType.SmallIncrement;
                case NativeMethods.SB_LINEUP:
                    return ScrollEventType.SmallDecrement;
                case NativeMethods.SB_PAGEDOWN:
                    return ScrollEventType.LargeIncrement;
                case NativeMethods.SB_PAGEUP:
                    return ScrollEventType.LargeDecrement;
                case NativeMethods.SB_THUMBPOSITION:
                    return ScrollEventType.ThumbPosition;
                case NativeMethods.SB_THUMBTRACK:
                    return ScrollEventType.ThumbTrack;
                case NativeMethods.SB_TOP:
                    return ScrollEventType.First;
                default:
                    throw new ArgumentException(string.Format("{0} isn't a valid scroll event type.", wParam), "wparam");
            }
        }

        /// <summary>
        ///   Raises the <see cref="BorderStyleChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
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

        /// <summary>
        ///   Raises the <see cref="PageSizeChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnPageSizeChanged(EventArgs e)
        {
            EventHandler handler;

            this.UpdateScrollbars();

            handler = this.PageSizeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="Scroll" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="ScrollEventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnScroll(ScrollEventArgs e)
        {
            ScrollEventHandler handler;

            this.UpdateHorizontalScroll();
            this.UpdateVerticalScroll();

            handler = this.Scroll;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="ScrollSizeChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnScrollSizeChanged(EventArgs e)
        {
            EventHandler handler;

            this.UpdateScrollbars();

            handler = this.ScrollSizeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Raises the <see cref="StepSizeChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        protected virtual void OnStepSizeChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.StepSizeChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///   Set the given scrollbar's tracking position to the specified value
        /// </summary>
        /// <param name="scrollbar">The scrollbar.</param>
        /// <param name="value">The value.</param>
        protected virtual void ScrollTo(ScrollOrientation scrollbar, int value)
        {
            NativeMethods.SCROLLINFO oldInfo;

            oldInfo = this.GetScrollInfo(scrollbar);

            if (value > ((oldInfo.nMax - oldInfo.nMin) + 1) - oldInfo.nPage)
            {
                value = ((oldInfo.nMax - oldInfo.nMin) + 1) - oldInfo.nPage;
            }
            if (value < oldInfo.nMin)
            {
                value = oldInfo.nMin;
            }

            if (oldInfo.nPos != value)
            {
                NativeMethods.SCROLLINFO scrollInfo;

                scrollInfo = new NativeMethods.SCROLLINFO();
                scrollInfo.fMask = NativeMethods.SIF.SIF_POS;
                scrollInfo.nPos = value;
                this.SetScrollInfo(scrollbar, scrollInfo, true);

                this.OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, oldInfo.nPos, value, scrollbar));
            }
        }

        /// <summary>
        ///   Updates the properties of the horizontal scrollbar.
        /// </summary>
        protected virtual void UpdateHorizontalScroll()
        {
            NativeMethods.SCROLLINFO scrollInfo;

            scrollInfo = this.GetScrollInfo(ScrollOrientation.HorizontalScroll);

            this.HorizontalScroll.Enabled = this.Enabled;
            this.HorizontalScroll.LargeChange = scrollInfo.nPage;
            this.HorizontalScroll.Maximum = scrollInfo.nMax;
            this.HorizontalScroll.Minimum = scrollInfo.nMin;
            this.HorizontalScroll.SmallChange = this.StepSize.Width;
            this.HorizontalScroll.Value = scrollInfo.nPos;
            this.HorizontalScroll.Visible = true;
        }

        /// <summary>
        /// Updates the horizontal scrollbar.
        /// </summary>
        protected virtual void UpdateHorizontalScrollbar()
        {
            NativeMethods.SCROLLINFO scrollInfo;
            int scrollWidth;
            int pageWidth;

            scrollWidth = this.ScrollSize.Width - 1;
            pageWidth = this.PageSize.Width;

            if (scrollWidth < 1)
            {
                scrollWidth = 0;
                pageWidth = 1;
            }

            scrollInfo = new NativeMethods.SCROLLINFO();
            scrollInfo.fMask = NativeMethods.SIF.SIF_PAGE | NativeMethods.SIF.SIF_RANGE;
            if (this.AlwaysShowHScroll || !this.Enabled)
            {
                scrollInfo.fMask |= NativeMethods.SIF.SIF_DISABLENOSCROLL;
            }
            scrollInfo.nMin = 0;
            scrollInfo.nMax = scrollWidth;
            scrollInfo.nPage = pageWidth;

            this.SetScrollInfo(ScrollOrientation.HorizontalScroll, scrollInfo, true);
        }

        /// <summary>
        ///   Updates the scrollbars.
        /// </summary>
        protected void UpdateScrollbars()
        {
            this.UpdateHorizontalScrollbar();
            this.UpdateVerticalScrollbar();
        }

        /// <summary>
        ///   Updates the properties of the vertical scrollbar.
        /// </summary>
        protected virtual void UpdateVerticalScroll()
        {
            NativeMethods.SCROLLINFO scrollInfo;

            scrollInfo = this.GetScrollInfo(ScrollOrientation.VerticalScroll);

            this.VerticalScroll.Enabled = this.Enabled;
            this.VerticalScroll.LargeChange = scrollInfo.nPage;
            this.VerticalScroll.Maximum = scrollInfo.nMax;
            this.VerticalScroll.Minimum = scrollInfo.nMin;
            this.VerticalScroll.SmallChange = this.StepSize.Height;
            this.VerticalScroll.Value = scrollInfo.nPos;
            this.VerticalScroll.Visible = true;
        }

        /// <summary>
        /// Updates the vertical scrollbar.
        /// </summary>
        protected virtual void UpdateVerticalScrollbar()
        {
            NativeMethods.SCROLLINFO scrollInfo;
            int scrollHeight;
            int pageHeight;

            scrollHeight = this.ScrollSize.Height - 1;
            pageHeight = this.PageSize.Height;

            if (scrollHeight < 1)
            {
                scrollHeight = 0;
                pageHeight = 1;
            }

            scrollInfo = new NativeMethods.SCROLLINFO();
            scrollInfo.fMask = NativeMethods.SIF.SIF_PAGE | NativeMethods.SIF.SIF_RANGE;
            if (AlwaysShowVScroll)
            {
                scrollInfo.fMask |= NativeMethods.SIF.SIF_DISABLENOSCROLL;
            }
            scrollInfo.nMin = 0;
            scrollInfo.nMax = scrollHeight;
            scrollInfo.nPage = pageHeight;

            this.SetScrollInfo(ScrollOrientation.VerticalScroll, scrollInfo, true);
        }

        /// <summary>
        ///   Processes the WM_HSCROLL and WM_VSCROLL Windows messages.
        /// </summary>
        /// <param name="msg">
        ///   The Windows <see cref="T:System.Windows.Forms.Message" /> to process.
        /// </param>
        protected virtual void WmScroll(ref Message msg)
        {
            ScrollOrientation scrollbar;
            int oldValue;
            int newValue;
            ScrollEventType eventType;

            eventType = this.GetEventType(msg.WParam);
            scrollbar = msg.Msg == NativeMethods.WM_HSCROLL ? ScrollOrientation.HorizontalScroll : ScrollOrientation.VerticalScroll;

            if (eventType != ScrollEventType.EndScroll)
            {
                int step;
                NativeMethods.SCROLLINFO scrollInfo;

                step = scrollbar == ScrollOrientation.HorizontalScroll ? this.StepSize.Width : this.StepSize.Height;

                scrollInfo = this.GetScrollInfo(scrollbar);
                scrollInfo.fMask = NativeMethods.SIF.SIF_POS;
                oldValue = scrollInfo.nPos;

                switch (eventType)
                {
                    case ScrollEventType.ThumbPosition:
                    case ScrollEventType.ThumbTrack:
                        scrollInfo.nPos = scrollInfo.nTrackPos;
                        break;

                    case ScrollEventType.SmallDecrement:
                        scrollInfo.nPos = oldValue - step;
                        break;

                    case ScrollEventType.SmallIncrement:
                        scrollInfo.nPos = oldValue + step;
                        break;

                    case ScrollEventType.LargeDecrement:
                        scrollInfo.nPos = oldValue - scrollInfo.nPage;
                        break;

                    case ScrollEventType.LargeIncrement:
                        scrollInfo.nPos = oldValue + scrollInfo.nPage;
                        break;

                    case ScrollEventType.First:
                        scrollInfo.nPos = scrollInfo.nMin;
                        break;

                    case ScrollEventType.Last:
                        scrollInfo.nPos = scrollInfo.nMax;
                        break;
                    default:
                        Debug.Assert(false, string.Format("Unknown scroll event type {0}", eventType));
                        break;
                }

                if (scrollInfo.nPos > ((scrollInfo.nMax - scrollInfo.nMin) + 1) - scrollInfo.nPage)
                {
                    scrollInfo.nPos = ((scrollInfo.nMax - scrollInfo.nMin) + 1) - scrollInfo.nPage;
                }

                if (scrollInfo.nPos < scrollInfo.nMin)
                {
                    scrollInfo.nPos = scrollInfo.nMin;
                }

                newValue = scrollInfo.nPos;
                this.SetScrollInfo(scrollbar, scrollInfo, true);
            }
            else
            {
                oldValue = 0;
                newValue = 0;
            }

            this.OnScroll(new ScrollEventArgs(eventType, oldValue, newValue, scrollbar));
        }

        #endregion

        #region Private Members

        /// <summary>
        ///   Gets scrollbar properties
        /// </summary>
        /// <param name="scrollbar">The bar.</param>
        /// <returns></returns>
        private NativeMethods.SCROLLINFO GetScrollInfo(ScrollOrientation scrollbar)
        {
            NativeMethods.SCROLLINFO info;

            info = new NativeMethods.SCROLLINFO();
            info.fMask = NativeMethods.SIF.SIF_ALL;

            NativeMethods.GetScrollInfo(this.Handle, (int)scrollbar, info);

            return info;
        }

        /// <summary>
        ///   Sets scrollbar properties.
        /// </summary>
        /// <param name="scrollbar">The scrollbar.</param>
        /// <param name="scrollInfo">The scrollbar properties.</param>
        /// <param name="refresh">
        ///   if set to <c>true</c> the scrollbar will be repainted.
        /// </param>
        /// <returns></returns>
        // ReSharper disable UnusedMethodReturnValue.Local
        private int SetScrollInfo(ScrollOrientation scrollbar, NativeMethods.SCROLLINFO scrollInfo, bool refresh) // ReSharper restore UnusedMethodReturnValue.Local
        {
            return NativeMethods.SetScrollInfo(this.Handle, (int)scrollbar, scrollInfo, refresh);
        }

        #endregion
    }
}
