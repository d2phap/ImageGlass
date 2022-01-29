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

using System.ComponentModel;
using unvell.D2DLib;

namespace ImageGlass.Base.HybridGraphics;


/// <summary>
/// Direct2D control for WinForms
/// </summary>
public class HybridControl : Control
{
    private const int WM_SIZE = 0x0005;
    private const int WM_ERASEBKGND = 0x0014;
    private const int WM_DESTROY = 0x0002;

    private bool _isControlLoaded = false;
    private D2DDevice? _device;
    private D2DGraphics? _graphics;
    private D2DBitmap? _backgroundImage;

    private bool _useHardwardAcceleration = true;
    private Direct2DGraphics? _graphicsD2D;
    private GDIGraphics? _graphicsGdi;

    private bool _firstPaintBackground = true;
    private bool _enableAnimation = true;
    private int _currentFps = 0;
    private int _lastFps = 0;
    private DateTime _lastFpsUpdate = DateTime.UtcNow;
    private System.Windows.Forms.Timer _animationTimer = new() { Interval = 10 };


    /// <summary>
    /// Request to update frame by <see cref="OnFrame"/> event.
    /// </summary>
    protected bool RequestUpdateFrame { get; set; } = false;
    


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
    /// Gets, sets a value indicating whether this control should draw its surface
    /// using Direct2D or GDI+.
    /// </summary>
    [Category("Graphics")]
    [DefaultValue(true)]
    public virtual bool UseHardwareAcceleration
    {
        get => _useHardwardAcceleration;
        set
        {
            _useHardwardAcceleration = value;
            DoubleBuffered = !_useHardwardAcceleration;
        }
    }

    /// <summary>
    /// Shows or hides Frame per second info
    /// </summary>
    [Category("Graphics")]
    [DefaultValue(false)]
    public bool ShowDebugInfo { get; set; } = false;

    /// <summary>
    /// Enables animation support for the control.
    /// </summary>
    [Category("Animation")]
    [DefaultValue(true)]
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

    /// <summary>
    /// Gets, sets animation timer interval.
    /// </summary>
    [Category("Animation")]
    [DefaultValue(10)]
    public int AnimationInterval
    {
        get => _animationTimer.Interval;
        set => _animationTimer.Interval = value;
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


    #endregion


    public HybridControl()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    }


    #region Override functions

    protected override void CreateHandle()
    {
        base.CreateHandle();
        if (DesignMode) return;

        DoubleBuffered = false;

        if (_device == null)
        {
            _device = D2DDevice.FromHwnd(Handle);
        }

        _graphics = new D2DGraphics(_device);

        // only support the base DPI
        _graphics.SetDPI(96, 96);

        // animation initiation
        _animationTimer.Interval = AnimationInterval;
        _animationTimer.Tick += (ss, ee) =>
        {
            if (EnableAnimation && RequestUpdateFrame)
            {
                OnFrame();
                Invalidate();
            }
        };
    }

    protected override void DestroyHandle()
    {
        base.DestroyHandle();
        _device?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _backgroundImage?.Dispose();
        _animationTimer?.Dispose();
        _graphicsGdi?.Dispose();
        _graphicsD2D?.Dispose();

        // '_device' must be disposed in DestroyHandle()
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_ERASEBKGND:

                // to fix background is delayed to paint on launch
                if (_firstPaintBackground)
                {
                    _firstPaintBackground = false;
                    if (!_useHardwardAcceleration)
                    {
                        base.WndProc(ref m);
                    }
                    else
                    {
                        _graphics?.BeginRender(D2DColor.FromGDIColor(BackColor));
                        _graphics?.EndRender();
                    }
                }
                break;

            case WM_SIZE:
                base.WndProc(ref m);
                if (_device != null) _device.Resize();
                break;

            case WM_DESTROY:
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


    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (!_useHardwardAcceleration)
        {
            base.OnPaintBackground(e);
        }
        else
        {
            // handled in OnPaint event
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
            e.Graphics.DrawString("This control does not support rendering in design mode.",
                Font, brush, 10, 10);

            return;
        }


        // use hardware acceleration
        if (UseHardwareAcceleration && _graphics is not null)
        {
            DoubleBuffered = false;

            if (_graphicsD2D == null)
            {
                _graphicsD2D = new Direct2DGraphics(_graphics);
            }
            else
            {
                _graphicsD2D.Graphics = _graphics;
            }

            _graphics.BeginRender(D2DColor.FromGDIColor(BackColor));
            OnRender(_graphicsD2D);
            _graphics.EndRender();
        }
        // use GDI+ graphics
        else
        {
            DoubleBuffered = true;

            if (_graphicsGdi == null)
            {
                _graphicsGdi = new GDIGraphics(e.Graphics);
            }
            else
            {
                _graphicsGdi.Graphics = e.Graphics;
            }

            OnRender(_graphicsGdi);
        }


        // Show debug info
        if (ShowDebugInfo)
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

            var info = $"{_lastFps} FPS, {(UseHardwareAcceleration ? "GPU" : "GDI+")}";
            var size = e.Graphics.MeasureString(info, Font, Width);
            var infoRect = new RectangleF(
                ClientRectangle.Right - size.Width - 10,
                5, size.Width, size.Height);

            using var infoBgBrush = new SolidBrush(Color.FromArgb(180, BackColor));
            e.Graphics.FillRectangle(infoBgBrush, infoRect);

            using var infoTextBrush = new SolidBrush(ForeColor);
            e.Graphics.DrawString(info, Font, infoTextBrush, infoRect);
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

