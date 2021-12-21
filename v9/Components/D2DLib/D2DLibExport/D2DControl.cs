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

using unvell.D2DLib;

namespace D2DLibExport;

/// <summary>
/// Direct2D control for WinForms
/// </summary>
public class D2DControl : Control
{
    private const int WM_ERASEBKGND = 0x0014;
    private const int WM_SIZE = 0x0005;

    private D2DDevice? _device;
    private D2DGraphics? _graphics;
    private D2DBitmap? _backgroundImage = null;

    private int currentFps = 0;
    private int lastFps = 0;
    private DateTime lastFpsUpdate = DateTime.UtcNow;


    #region Public properties

    public D2DDevice Device
    {
        get
        {
            if (_device == null)
            {
                _device = D2DDevice.FromHwnd(Handle);
            }

            return _device;
        }
    }

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

    public bool DrawFps { get; set; } = false;


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

        OnRender(_graphics);

        if (DrawFps)
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

            var info = string.Format("{0} fps", lastFps);
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