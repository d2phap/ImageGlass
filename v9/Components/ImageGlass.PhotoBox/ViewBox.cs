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

using ImageGlass.Base;
using ImageGlass.Base.PhotoBox;
using System.ComponentModel;
using System.Numerics;
using unvell.D2DLib;

namespace ImageGlass.PhotoBox;


/// <summary>
/// Modern photo view box with hardware-accelerated
/// </summary>
public partial class ViewBox : D2DControl
{
    private D2DBitmap? _image;

    /// <summary>
    /// Gets the area of the image content to draw
    /// </summary>
    private D2DRect _srcRect = new(0, 0, 0, 0);

    /// <summary>
    /// Gets the boundary of the displaying image content
    /// </summary>
    private D2DRect _destRect = new(0, 0, 0, 0);

    private Vector2 _panHostPoint;
    private Vector2 _panSpeed;
    private Vector2 _panHostStartPoint;

    private bool _xOut = false;
    private bool _yOut = false;
    private bool _isMouseDown = false;
    private Vector2 _drawPoint = new();

    // current zoom, minimum zoom, maximum zoom, previous zoom (bigger means zoom in)
    private float _zoomFactor = 1f;
    private float _oldZoomFactor = 1f;
    private bool _isManualZoom = false;
    private ZoomMode _zoomMode = ZoomMode.AutoZoom;
    private InterpolationMode _interpolationMode = InterpolationMode.NearestNeighbor;

    private CheckerboardMode _checkerboardMode = CheckerboardMode.Image;


    #region Public properties

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
    [DefaultValue(40.0f)]
    public float MaxZoom { get; set; } = 40f;

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
    [Category("Drawing")]
    [DefaultValue(InterpolationMode.NearestNeighbor)]
    public InterpolationMode InterpolationMode
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
    /// Shows or hides checkerboard
    /// </summary>
    [Category("Appearence")]
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

    [Category("Appearence")]
    [DefaultValue(typeof(SizeF), "12, 12")]
    public SizeF CheckerboardCellSize { get; set; } = new(12, 12);

    [Category("Appearence")]
    [DefaultValue(typeof(Color), "25, 0, 0, 0")]
    public Color CheckerboardColor1 { get; set; } = Color.FromArgb(25, Color.Black);

    [Category("Appearence")]
    [DefaultValue(typeof(Color), "25, 255, 255, 255")]
    public Color CheckerboardColor2 { get; set; } = Color.FromArgb(25, Color.White);


    /// <summary>
    /// Occurs when the host is being panned
    /// </summary>
    public event PanningEventHandler? OnPanning;
    public delegate void PanningEventHandler(PanningEventArgs e);


    /// <summary>
    /// Occurs when the mouse pointer is moved over the control
    /// </summary>
    public event MouseMoveEventHandler? OnMouseMoveEx;
    public delegate void MouseMoveEventHandler(MouseMoveEventArgs e);


    /// <summary>
    /// Occurs when <see cref="ZoomFactor"/> value changes
    /// </summary>
    public event ZoomChangedEventHandler? OnZoomChanged = null;
    public delegate void ZoomChangedEventHandler(ZoomEventArgs e);

    #endregion


    public ViewBox()
    {

    }


    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (!IsReady || _image is null) return;

        _panHostPoint.X = e.Location.X;
        _panHostPoint.Y = e.Location.Y;
        _panSpeed.X = 0;
        _panSpeed.Y = 0;
        _panHostStartPoint.X = e.Location.X;
        _panHostStartPoint.Y = e.Location.Y;
        _isMouseDown = true;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (!IsReady) return;

        _isMouseDown = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!IsReady || _image is null || _isMouseDown is false) return;

        _srcRect.X += ((_panHostPoint.X - e.Location.X) / _zoomFactor)
            + _panSpeed.X;

        _srcRect.Y += ((_panHostPoint.Y - e.Location.Y) / _zoomFactor)
            + _panSpeed.Y;


        _drawPoint = new();
        Invalidate();


        if (_xOut == false)
        {
            _panHostPoint.X = e.Location.X;
        }

        if (_yOut == false)
        {
            _panHostPoint.Y = e.Location.Y;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (!IsReady || _image is null || e.Delta == 0) return;

        var speed = e.Delta / 500f;

        // zoom in
        if (e.Delta > 0)
        {
            if (_zoomFactor > MaxZoom)
                return;

            _oldZoomFactor = _zoomFactor;
            _zoomFactor *= 1f + speed;
        }
        // zoom out
        else if (e.Delta < 0)
        {
            if (_zoomFactor < MinZoom)
                return;

            _oldZoomFactor = _zoomFactor;
            _zoomFactor /= 1f - speed;
        }

        _isManualZoom = true;
        _drawPoint = new(e.Location.X, e.Location.Y);
        Invalidate();

        OnZoomChanged?.Invoke(new(_zoomFactor));
    }

    protected override void OnResize(EventArgs e)
    {
        // redraw the control on resizing if it's not manual zoom
        if (IsReady && _image is not null && !_isManualZoom)
        {
            Refresh();
        }

        base.OnResize(e);
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        // draw the control
        Refresh();
    }

    protected override void OnRender(D2DGraphics g)
    {
        base.OnRender(g);

        if (!IsReady) return;

        // draw image
        DrawBitmap(g);
    }


    private void DrawBitmap(D2DGraphics g)
    {
        if (_image is null) return;

        var zoomX = _drawPoint.X;
        var zoomY = _drawPoint.Y;

        _xOut = false;
        _yOut = false;

        var clientW = Width;
        var clientH = Height;

        if (clientW > _image.Width * _zoomFactor)
        {
            _srcRect.X = 0;
            _srcRect.Width = _image.Width;
            _destRect.X = (clientW - _image.Width * _zoomFactor) / 2.0f;
            _destRect.Width = _image.Width * _zoomFactor;
        }
        else
        {
            _srcRect.X += (clientW / _oldZoomFactor - clientW / _zoomFactor) / ((clientW + 0.001f) / zoomX);
            _srcRect.Width = clientW / _zoomFactor;
            _destRect.X = 0;
            _destRect.Width = clientW;
        }


        if (clientH > _image.Height * _zoomFactor)
        {
            _srcRect.Y = 0;
            _srcRect.Height = _image.Height;
            _destRect.Y = (clientH - _image.Height * _zoomFactor) / 2f;
            _destRect.Height = _image.Height * _zoomFactor;
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

        if (_srcRect.X + _srcRect.Width > _image.Width)
        {
            _xOut = true;
            _srcRect.X = _image.Width - _srcRect.Width;
        }

        if (_srcRect.X < 0)
        {
            _xOut = true;
            _srcRect.X = 0;
        }

        if (_srcRect.Y + _srcRect.Height > _image.Height)
        {
            _yOut = true;
            _srcRect.Y = _image.Height - _srcRect.Height;
        }

        if (_srcRect.Y < 0)
        {
            _yOut = true;
            _srcRect.Y = 0;
        }

        // draw checkerboard background
        DrawCheckerboard(g);

        // draw bitmap
        g.DrawBitmap(_image, _destRect, _srcRect, 1f, (D2DBitmapInterpolationMode)InterpolationMode);
    }


    private void DrawCheckerboard(D2DGraphics g)
    {
        if (CheckerboardMode == CheckerboardMode.None) return;

        // region to draw
        var region = ClientRectangle;

        if (CheckerboardMode == CheckerboardMode.Image)
        {
            region = (Rectangle)_destRect;
        }

        // grid size
        int rows = (int)Math.Ceiling(region.Width / CheckerboardCellSize.Width);
        int cols = (int)Math.Ceiling(region.Height / CheckerboardCellSize.Height);


        // draw grid
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                D2DColor color;
                if ((row + col) % 2 == 0)
                {
                    color = D2DColor.FromGDIColor(CheckerboardColor1);
                }
                else
                {
                    color = D2DColor.FromGDIColor(CheckerboardColor2);
                }

                var drawnW = row * CheckerboardCellSize.Width;
                var drawnH = col * CheckerboardCellSize.Height;

                var x = drawnW + region.X;
                var y = drawnH + region.Y;

                var w = Math.Min(region.Width - drawnW, CheckerboardCellSize.Width);
                var h = Math.Min(region.Height - drawnH, CheckerboardCellSize.Height);

                g.FillRectangle(new(x, y, w, h), color);
            }
        }
    }


    private void UpdateZoomMode(ZoomMode? mode = null)
    {
        if (!IsReady || _image is null) return;

        var viewportW = Width;
        var viewportH = Height;
        var imgFullW = _image.Width;
        var imgFullH = _image.Height;

        var horizontalPadding = Padding.Left + Padding.Right;
        var verticalPadding = Padding.Top + Padding.Bottom;
        var widthScale = (viewportW - horizontalPadding) / imgFullW;
        var heightScale = (viewportH - verticalPadding) / imgFullH;

        float zoomFactor;
        var zoomMode = mode ?? _zoomMode;

        if (zoomMode  == ZoomMode.ScaleToWidth)
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
    }


    /// <summary>
    /// Force the control to update zoom mode and invalidate itself
    /// </summary>
    public new void Refresh()
    {
        UpdateZoomMode();
        Invalidate();
    }


    /// <summary>
    /// Load image from file path
    /// </summary>
    /// <param name="filename">Full path of file</param>
    public void LoadImage(string filename)
    {
        if (string.IsNullOrEmpty(filename)) return;

        _image?.Dispose();
        _image = Device.LoadBitmap(filename);

        if (IsReady)
        {
            Refresh();
        }
    }

}
