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
    private D2DGraphics? _graphics;

    private int currentFps = 0;
    private int lastFps = 0;
    private DateTime lastFpsUpdate = DateTime.UtcNow;


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

    /// <summary>
    /// Shows or hides Frame per second info
    /// </summary>
    [DefaultValue(false)]
    public bool ShowFPS { get; set; } = false;

    /// <summary>
    /// <b>Do not</b> use this property.
    /// Sets <c>DoubleBuffered = false</c> by default.
    /// </summary>
    [Browsable(false)]
    [Obsolete("This property does not work.")]
    protected override bool DoubleBuffered
    {
        get => base.DoubleBuffered;
        set => base.DoubleBuffered = false;
    }

    #endregion


    #region Override functions

    protected override void CreateHandle()
    {
        base.CreateHandle();
        base.DoubleBuffered = false;

        if (DesignMode) return;

        if (_device == null)
        {
            _device = D2DDevice.FromHwnd(Handle);
        }

        _graphics = new D2DGraphics(_device);
    }

    protected override void DestroyHandle()
    {
        base.DestroyHandle();
        _device?.Dispose();
    }

    protected override void WndProc(ref Message m)
    {
        if (DesignMode) return;

        switch (m.Msg)
        {
            case WM_ERASEBKGND:
                break;

            case WM_SIZE:
                base.WndProc(ref m);
                if (_device != null) _device.Resize();
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
    protected override void OnPaintBackground(PaintEventArgs e) { }


    /// <summary>
    /// <b>Do use</b> <see cref="OnRender(D2DGraphics)"/> if you want to draw on the control.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint(PaintEventArgs e)
    {
        if (_graphics is null) return;

        if (_backgroundImage != null)
        {
            _graphics.BeginRender(_backgroundImage);
        }
        else
        {
            _graphics.BeginRender(D2DColor.FromGDIColor(BackColor));
        }

        // only support the base DPI
        _graphics.SetDPI(96, 96);

        OnRender(_graphics);

        if (ShowFPS)
        {
            if (lastFpsUpdate.Second != DateTime.UtcNow.Second)
            {
                lastFps = currentFps;
                currentFps = 0;
                lastFpsUpdate = DateTime.UtcNow;
            }
            else
            {
                currentFps++;
            }

            var info = string.Format("{0} FPS", lastFps);
            var size = e.Graphics.MeasureString(info, Font, Width);

            e.Graphics.DrawString(info, Font, Brushes.Black, ClientRectangle.Right - size.Width - 10, 5);
        }

        _graphics.EndRender();
    }

    #endregion


    #region New / Virtual functions

    /// <summary>
    /// Main method to draw graphics on the control.
    /// <b>Do not</b> use <see cref="OnPaint(PaintEventArgs)"/>
    /// </summary>
    /// <param name="g"></param>
    protected virtual void OnRender(D2DGraphics g) {
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


