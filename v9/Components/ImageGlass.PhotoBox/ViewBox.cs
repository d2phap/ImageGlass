

using System.Numerics;
using unvell.D2DLib;

namespace ImageGlass.PhotoBox;

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

    // current zoom, minimum zoom, maximum zoom, previous zoom (bigger means zoom in)
    private float _zoomFactor = 1f;
    private float _oldZoomFactor = 1f;

    private bool _xOut = false;
    private bool _yOut = false;
    private bool _isMouseDown = false;
    private Vector2 _drawPoint = new();


    #region Public properties

    /// <summary>
    /// Gets, sets the minimum zoom factor (<c>100% = 1.0f</c>)
    /// </summary>
    public float MinZoom { get; set; } = 0.01f;

    /// <summary>
    /// Gets, sets the maximum zoom factor (<c>100% = 1.0f</c>)
    /// </summary>
    public float MaxZoom { get; set; } = 40f;

    /// <summary>
    /// Gets, sets current zoom factor (<c>100% = 1.0f</c>)
    /// </summary>
    public float CurrentZoom
    {
        get => _zoomFactor;
        set
        {
            _zoomFactor = value;
            Invalidate();
        }
    }

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
    /// Occurs when <see cref="CurrentZoom"/> value changes
    /// </summary>
    public event ZoomChangedEventHandler? OnZoomChanged = null;
    public delegate void ZoomChangedEventHandler(ZoomEventArgs e);

    #endregion


    public ViewBox()
    {

    }



    public void LoadImage(string filename)
    {
        if (string.IsNullOrEmpty(filename)) return;

        _image?.Dispose();
        _image = Device.LoadBitmap(filename);

        Invalidate();
    }


    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (_image is null) return;

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

        _isMouseDown = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_image is null || _isMouseDown is false) return;

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

        if (_image is null || e.Delta == 0) return;

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

        _drawPoint = new(e.Location.X, e.Location.Y);
        Invalidate();

        OnZoomChanged?.Invoke(new(_zoomFactor));
    }


    protected override void OnInvalidated(InvalidateEventArgs e)
    {
        if (Parent != null && !DesignMode)
        {
            // fix the incorrect scale
            Width = Parent.Width;
            Height = Parent.Height;
        }

        base.OnInvalidated(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        // update the control once size/windows state changed
        ResizeRedraw = true;
    }

    protected override void OnRender(D2DGraphics g)
    {
        base.OnRender(g);

        if (DesignMode || _image is null) return;

        
        g.SetDPI(96, 96);

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

        g.DrawBitmap(_image,
            _destRect,
            _srcRect,
            1f,
            D2DBitmapInterpolationMode.NearestNeighbor);
    }
}
