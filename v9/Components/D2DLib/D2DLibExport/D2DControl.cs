/*
* MIT License
*
* Copyright (c) 2009-2021 Jingwood, unvell.com. All right reserved.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
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

    private D2DDevice? _device;
    private D2DBitmap? _backgroundImage;
    private D2DGraphics? _graphics;

    private int currentFps = 0;
    private int lastFps = 0;
    private DateTime lastFpsUpdate = DateTime.UtcNow;


    #region Public properties

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


    #endregion


    #region Protected/Override functions

    protected override void CreateHandle()
    {
        base.CreateHandle();

        DoubleBuffered = false;

        if (_device == null)
        {
            _device = D2DDevice.FromHwnd(Handle);
        }

        _graphics = new D2DGraphics(_device);
    }


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

    protected override void DestroyHandle()
    {
        base.DestroyHandle();
        _device?.Dispose();
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }


    /// <summary>
    /// Main method to draw graphics on the control.
    /// <b>Do not</b> use <see cref="OnPaint(PaintEventArgs)"/>
    /// </summary>
    /// <param name="g"></param>
    protected virtual void OnRender(D2DGraphics g) { }

    protected override void WndProc(ref Message m)
    {
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

    public new void Invalidate()
    {
        Invalidate(false);
    }

    #endregion

}


