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

using D2DLibExport;
using System.ComponentModel;

namespace unvell.D2DLib;


/// <summary>
/// Direct2D control for WinForms
/// </summary>
public class D2DControl : Control
{
    private const int WM_SIZE = 0x0005;
    private const int WM_ERASEBKGND = 0x0014;

    private bool _isControlLoaded = false;
    private D2DDevice? _device;
    private D2DBitmap? _backgroundImage;

    private D2DGraphics graphics;
    private Direct2DGraphics? d2dGraphics;
    private GDIGraphics gdiGraphics;
    private bool hardwardAcceleration = true;

    private bool _firstPaintBackground = true;
    private bool _enableAnimation;
    private int _currentFps = 0;
    private int _lastFps = 0;
    private DateTime _lastFpsUpdate = DateTime.UtcNow;
    private System.Windows.Forms.Timer _animationTimer = new();


    /// <summary>
    /// Request to update frame by <see cref="OnFrame"/> event.
    /// </summary>
    protected bool IsSceneChanged { get; set; } = false;
    public int AnimationInterval
    {
        get => _animationTimer.Interval;
        set => _animationTimer.Interval = value;
    }

    

    public bool HardwardAcceleration
    {
        get { return this.hardwardAcceleration; }
        set
        {
            this.hardwardAcceleration = value;
            this.DoubleBuffered = !this.hardwardAcceleration;
        }
    }



    #region Public properties

    /// <summary>
    /// Gets the value indicates if control is fully loaded
    /// </summary>
    [Browsable(false)]
    public bool IsReady => !DesignMode && _isControlLoaded;

    /// <summary>
    /// Gets Direct2D device
    /// </summary>
    [Browsable(false)]
    public D2DDevice Device
    {
        get
        {
            var hwnd = Handle;

            if (_device == null)
            {
                // Note: do not pass 'Handle' directly here
                _device = D2DDevice.FromHwnd(hwnd);
            }

            return _device;
        }
    }

    /// <summary>
    /// Gets, sets background image
    /// </summary>
    [Browsable(false)]
    public new D2DBitmap? BackgroundImage
    {
        get => _backgroundImage;
        set
        {
            if (_backgroundImage != value)
            {
                if (_backgroundImage != null)
                {
                    _backgroundImage.Dispose();
                }

                _backgroundImage = value;
                Invalidate();
            }
        }
    }

    ///// <summary>
    ///// <b>Do not</b> use this property.
    ///// Sets <c>DoubleBuffered = false</c> by default.
    ///// </summary>
    //[Browsable(false)]
    //[Obsolete("This property does not work.")]
    //protected override bool DoubleBuffered
    //{
    //    get => base.DoubleBuffered;
    //    set => base.DoubleBuffered = false;
    //}

    /// <summary>
    /// Shows or hides Frame per second info
    /// </summary>
    [Category("Animation")]
    [DefaultValue(false)]
    public bool ShowFPS { get; set; } = false;

    /// <summary>
    /// Enables animation support for the control.
    /// </summary>
    [Category("Animation")]
    [DefaultValue(false)]
    public bool EnableAnimation
    {
        get => _enableAnimation;
        set
        {
            _enableAnimation = value;

            if (!_enableAnimation)
            {
                if (_animationTimer.Enabled) _animationTimer.Stop();
            }
            else
            {
                if (!_animationTimer.Enabled) _animationTimer.Start();
            }
        }
    }

    #endregion


    public D2DControl()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    }


    #region Override functions

    protected override void CreateHandle()
    {
        base.CreateHandle();
        if (DesignMode) return;

        this.DoubleBuffered = false;

        if (this._device == null)
        {
            this._device = D2DDevice.FromHwnd(this.Handle);
        }

        this.graphics = new D2DGraphics(this._device);

        // only support the base DPI
        graphics.SetDPI(96, 96);


        _animationTimer.Interval = AnimationInterval;
        _animationTimer.Tick += (ss, ee) =>
        {
            if (EnableAnimation || IsSceneChanged)
            {
                OnFrame();
                Invalidate();
                IsSceneChanged = false;
            }
        };
    }

    protected override void DestroyHandle()
    {
        base.DestroyHandle();
        _device?.Dispose();
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_ERASEBKGND:
                if (!this.hardwardAcceleration)
                {
                    base.WndProc(ref m);
                }
                else
                {
                    // to fix background is delayed to paint on launch
                    if (_firstPaintBackground)
                    {
                        d2dGraphics?.g?.BeginRender(D2DColor.FromGDIColor(BackColor));
                        d2dGraphics?.g?.EndRender();
                        _firstPaintBackground = false;
                    }
                }
                break;

            case WM_SIZE:
                base.WndProc(ref m);
                if (_device != null) _device.Resize();
                break;

            case 0x0002 /* WM_DESTROY */:
                if (_device != null) _device.Dispose();
                base.WndProc(ref m);
                break;

            default:
                base.WndProc(ref m);
                break;
        }
    }


    protected override void OnSizeChanged(EventArgs e)
    {
        // detect if control is loaded
        if (!DesignMode && Created)
        {
            // control is loaded
            if (!_isControlLoaded)
            {
                _isControlLoaded = true;

                OnLoaded();
            }

            // update the control once size/windows state changed
            ResizeRedraw = true;

            base.OnSizeChanged(e);
        }
    }

    /// <summary>
    /// <b>Do use</b> <see cref="OnRender(D2DGraphics)"/> if you want to draw on the control.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (!this.hardwardAcceleration)
        {
            base.OnPaintBackground(e);
        }
    }


    /// <summary>
    /// <b>Do use</b> <see cref="OnRender(D2DGraphics)"/> if you want to draw on the control.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint(PaintEventArgs e)
    {
        if (DesignMode)
        {
            e.Graphics.Clear(BackColor);

            using var brush = new SolidBrush(ForeColor);
            e.Graphics.DrawString("This control does not support rendering in design mode.", Font, brush, 10, 10);

            return;
        }


        if (HardwardAcceleration)
        {
            this.DoubleBuffered = false;

            if (this.d2dGraphics == null)
            {
                this.d2dGraphics = new Direct2DGraphics(this.graphics);
            }
            else
            {
                this.d2dGraphics.g = this.graphics;
            }

            this.graphics.BeginRender(D2DColor.FromGDIColor(this.BackColor));

            this.OnRender(this.d2dGraphics);

            this.graphics.EndRender();
        }
        else
        {
            this.DoubleBuffered = true;

            if (this.gdiGraphics == null)
            {
                this.gdiGraphics = new GDIGraphics(e.Graphics);
            }
            else
            {
                this.gdiGraphics.g = e.Graphics;
            }

            this.OnRender(this.gdiGraphics);
        }

        if (ShowFPS)
        {
            if (_lastFpsUpdate.Second != DateTime.UtcNow.Second)
            {
                _lastFps = _currentFps;
                _currentFps = 0;
                _lastFpsUpdate = DateTime.UtcNow;
            }
            else
            {
                _currentFps++;
            }

            var info = string.Format("{0} FPS", _lastFps);
            var size = e.Graphics.MeasureString(info, Font, Width);

            e.Graphics.DrawString(info, Font, Brushes.Black, ClientRectangle.Right - size.Width - 10, 5);
        }

        // Start animation
        if (_enableAnimation && !_animationTimer.Enabled)
        {
            _animationTimer.Start();
        }
    }

    #endregion



    #region New / Virtual functions

    /// <summary>
    /// User drawing method. Override this method to draw anything on your form.
    /// </summary>
    /// <param name="g">Graphics context supports both GDI+ and Direct2D rendering.</param>
    protected virtual void OnRender(IHybridGraphics g)
    {
        if (!IsReady) return;
    }


    /// <summary>
    /// Process animation logic when frame changes
    /// </summary>
    protected virtual void OnFrame()
    {
        if (!IsReady) return;
    }


    /// <summary>
    /// Happens when control is ready.
    /// </summary>
    protected virtual void OnLoaded() { }


    /// <summary>
    /// Invalidates client retangle of the control and causes a paint message to the control.
    /// This does not apply to child controls.
    /// </summary>
    public new void Invalidate()
    {
        Invalidate(false);
    }

    #endregion

}

