
namespace ImageGlass.PhotoBox;

public class ViewBox
{
    #region Private properties

    // Host control , host control graphics , and source bitmap
    private Control _host = new();
    private Graphics? _graphics;
    private Bitmap? _srcBitmap;

    // Source bitmap Handle
    private IntPtr _srcBitmapHandle;

    // Source DC handle , Destination DC handle
    private IntPtr _srcDcHandle;
    private IntPtr _desDcHandle;

    // Viewport and display boundary
    private RectangleF _imageViewPort = new();
    private RectangleF _displayBounds = new();

    private PointF _panHostPoint;
    private PointF _panSpeed;
    private PointF _panHostStartPoint;

    // current zoom, minimum zoom, maximum zoom, previous zoom (bigger means zoom in)
    private float _zoomFactor = 1f;
    private float _oldZoomFactor = 1f;

    private bool _xOut = false;
    private bool _yOut = false;
    private bool _isHostLoadComplete = false;
    private bool _isMouseDown = false;

    #endregion


    #region Public properties
    /// <summary>
    /// Gets, sets the minimum zoom factor (100% = 1.0f)
    /// </summary>
    public float MinZoom { get; set; } = 0.01f;

    /// <summary>
    /// Gets, sets the maximum zoom factor (100% = 1.0f)
    /// </summary>
    public float MaxZoom { get; set; } = 35f;

    /// <summary>
    /// Gets, sets current zoom factor (100% = 1.0f)
    /// </summary>
    public float CurrentZoom
    {
        get => _zoomFactor;
        set
        {
            _zoomFactor = value;
            DrawPic(_host.Width / _zoomFactor, _host.Height / _zoomFactor);
        }
    }

    /// <summary>
    /// Gets sets the bitmap image
    /// </summary>
    public Bitmap? Image
    {
        get
        {
            return _srcBitmap;
        }
        set
        {
            Dispose();
            _srcBitmap = value;

            DrawPic(0, 0);
        }
    }

    /// <summary>
    /// Gets the source device context (DC) handle
    /// </summary>
    public IntPtr SrcDcHandle => _srcDcHandle;

    /// <summary>
    /// Gets the source bitmap handle
    /// </summary>
    public IntPtr SrcBitmapHandle => _srcBitmapHandle;

    /// <summary>
    /// Gets the area of the image content to draw
    /// </summary>
    public RectangleF ImageViewPort => _imageViewPort;

    /// <summary>
    /// Gets the boundary of the displaying image content
    /// </summary>
    public RectangleF DisplayBounds => _displayBounds;


    /// <summary>
    /// Occurs when the host is being panned
    /// </summary>
    public event PanningEventHandler? OnPanning = null;
    public delegate void PanningEventHandler(PanningEventArgs e);


    /// <summary>
    /// Occurs when the mouse pointer is moved over the control
    /// </summary>
    public event MouseMoveEventHandler? OnMouseMove = null;
    public delegate void MouseMoveEventHandler(MouseMoveEventArgs e);


    /// <summary>
    /// Occurs when <see cref="CurrentZoom"/> value changes
    /// </summary>
    public event ZoomChangedEventHandler? OnZoomChanged = null;
    public delegate void ZoomChangedEventHandler(ZoomEventArgs e);

    #endregion


    /// <summary>
    /// Initializes ViewBox instance
    /// </summary>
    /// <param name="host">Control to host</param>
    /// <param name="bmp">Bitmap image to display</param>
    public ViewBox(Control host, Bitmap? bmp = null)
    {
        _host = host;
        _srcBitmap = bmp;

        _host.MouseDown += Host_MouseDown;
        _host.MouseUp += Host_MouseUp;
        _host.MouseMove += Host_MouseMove;
        _host.MouseWheel += Host_MouseWheel;

        _host.Paint += Host_Paint;
        _host.Resize += Host_Resize;

        _srcDcHandle = default;
        _desDcHandle = default;
        _isHostLoadComplete = true;
    }


    public void Dispose()
    {
        if (_srcBitmap is not null)
        {
            _ = WinApi.DeleteObject(_srcBitmapHandle);
            _srcBitmap = null;
            _srcBitmapHandle = default;
        }

        if (!_srcDcHandle.Equals(IntPtr.Zero))
        {
            WinApi.DeleteDC(_srcDcHandle);
            _srcDcHandle = default;
        }

        if (_graphics is not null)
        {
            _graphics.Dispose();
            _graphics = null;
        }

        GC.Collect();
    }


    private void Host_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (_srcBitmap is null) return;

        var speed = e.Delta / 500f;

        // zoom in
        if (e.Delta > 0)
        {
            if (_zoomFactor > MaxZoom)
                return;
            _oldZoomFactor = _zoomFactor;
            _zoomFactor *= 1f + speed;
            DrawPic(e.X, e.Y);
        }
        // zoom out
        else if (e.Delta < 0)
        {
            if (_zoomFactor < MinZoom)
                return;
            _oldZoomFactor = _zoomFactor;
            _zoomFactor /= 1f - speed;
            DrawPic(e.X, e.Y);
        }


        OnZoomChanged?.Invoke(new(_zoomFactor));
    }

    private void Host_Paint(object? sender, PaintEventArgs e)
    {
        DrawPic(0, 0);
    }

    private void Host_Resize(object? sender, EventArgs e)
    {
        if (_isHostLoadComplete == true)
        {
            DrawPic(0, 0);
        } 
    }

    private void Host_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_srcBitmap is null)
            return;

        _isMouseDown = false;
        var speed = e.Delta / 200f;

        if (_panHostStartPoint.X == e.X & _panHostStartPoint.Y == e.Y)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_zoomFactor > MaxZoom)
                    return;
                _oldZoomFactor = _zoomFactor;
                _zoomFactor *= 1f + speed;
                DrawPic(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (_zoomFactor < MinZoom)
                    return;
                _oldZoomFactor = _zoomFactor;
                _zoomFactor /= 1f - speed;
                DrawPic(e.X, e.Y);
            }

            OnZoomChanged?.Invoke(new(_zoomFactor));
        }
    }

    private void Host_MouseDown(object? sender, MouseEventArgs e)
    {
        if (_srcBitmap is null)
            return;

        _panHostPoint.X = e.X;
        _panHostPoint.Y = e.Y;
        _panSpeed.X = 0;
        _panSpeed.Y = 0;
        _panHostStartPoint.X = e.X;
        _panHostStartPoint.Y = e.Y;
        _isMouseDown = true;
    }



    private void Host_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_srcBitmap is null)
            return;

        if (_isMouseDown == true)
        {
            // accelerated scrolling when right click drag ----------------
            if (e.Button == MouseButtons.Right)
            {
                _panSpeed.X = (_panHostPoint.X - e.X) * (_srcBitmap.Width / 2000f);
                _panSpeed.Y = (_panHostPoint.Y - e.Y) * (_srcBitmap.Height / 2000f);
            }

            _imageViewPort.X += ((_panHostPoint.X - e.X) / _zoomFactor)
                + _panSpeed.X;

            _imageViewPort.Y += ((_panHostPoint.Y - e.Y) / _zoomFactor)
                + _panSpeed.Y;

            DrawPic(0, 0);

            if (_xOut == false)
            {
                _panHostPoint.X = e.X;
            }
                
            if (_yOut == false)
            {
                _panHostPoint.Y = e.Y;
            }

            OnPanning?.Invoke(new(_panHostPoint, _panHostStartPoint));
        }

        var imgX = (e.X - _displayBounds.X) / _zoomFactor + _imageViewPort.X;
        var imgY = (e.Y - _displayBounds.Y) / _zoomFactor + _imageViewPort.Y;

        OnMouseMove?.Invoke(new(imgX, imgY, e.Button));
    }

    private void DrawPic(float zoomX, float zoomY)
    {
        if (_srcBitmap is null)
            return;

        if (_srcDcHandle.Equals(IntPtr.Zero))
        {
            _srcDcHandle = WinApi.CreateCompatibleDC(IntPtr.Zero);
            _srcBitmapHandle = _srcBitmap.GetHbitmap(Color.FromArgb(0));
            _ = WinApi.SelectObject(_srcDcHandle, _srcBitmapHandle);
        }
        

        if (_desDcHandle.Equals(IntPtr.Zero))
        {
            if (_graphics is null)
            {
                _graphics = _host.CreateGraphics();
            }  

            _desDcHandle = _graphics.GetHdc();
            _ = WinApi.SetStretchBltMode(_desDcHandle, StretchBltMode.STRETCH_DELETESCANS);
        }


        _xOut = false;
        _yOut = false;

        if (_host.Width > _srcBitmap.Width * _zoomFactor)
        {
            _imageViewPort.X = 0;
            _imageViewPort.Width = _srcBitmap.Width;
            _displayBounds.X = (_host.Width - _srcBitmap.Width * _zoomFactor) / 2.0f;
            _displayBounds.Width = _srcBitmap.Width * _zoomFactor;

            // clear left side
            _ = WinApi.BitBlt(_desDcHandle, 0, 0, (int)_displayBounds.X, _host.Height, _srcDcHandle, 0, 0, TernaryRasterOperations.BLACKNESS);

            // clear right side
            _ = WinApi.BitBlt(_desDcHandle, (int)_displayBounds.Right, 0, (int)_displayBounds.X, _host.Height, _srcDcHandle, 0, 0, TernaryRasterOperations.BLACKNESS);
        }
        else
        {
            _imageViewPort.X += (_host.Width / _oldZoomFactor - _host.Width / _zoomFactor) / ((_host.Width + 0.001f) / zoomX);
            _imageViewPort.Width = _host.Width / _zoomFactor;
            _displayBounds.X = 0;
            _displayBounds.Width = _host.Width;
        }

        if (_host.Height > _srcBitmap.Height * _zoomFactor)
        {
            _imageViewPort.Y = 0;
            _imageViewPort.Height = _srcBitmap.Height;
            _displayBounds.Y = (_host.Height - _srcBitmap.Height * _zoomFactor) / 2f;
            _displayBounds.Height = _srcBitmap.Height * _zoomFactor;

            // clear top
            WinApi.BitBlt(_desDcHandle, 0, 0, _host.Width, (int)_displayBounds.Y, _srcDcHandle, 0, 0, TernaryRasterOperations.BLACKNESS);

            // clear bottom
            WinApi.BitBlt(_desDcHandle, 0, (int)_displayBounds.Bottom, _host.Width, (int)_displayBounds.Y, _srcDcHandle, 0, 0, TernaryRasterOperations.BLACKNESS);
        }
        else
        {
            _imageViewPort.Y += (_host.Height / _oldZoomFactor - _host.Height / _zoomFactor) / ((_host.Height + 0.001f) / zoomY);
            _imageViewPort.Height = _host.Height / _zoomFactor;
            _displayBounds.Y = 0;
            _displayBounds.Height = _host.Height;
        }

        //// clear center
        //WinApi.BitBlt(_desDcHandle, (int)_displayBounds.X, (int)_displayBounds.Y, (int)_displayBounds.Width, (int)_displayBounds.Height, _srcDcHandle, 0, 0, TernaryRasterOperations.BLACKNESS);

        _oldZoomFactor = _zoomFactor;
        // -----------------------------------

        if (_imageViewPort.Right > _srcBitmap.Width)
        {
            _xOut = true;
            _imageViewPort.X = _srcBitmap.Width - _imageViewPort.Width;
        }

        if (_imageViewPort.X < 0)
        {
            _xOut = true;
            _imageViewPort.X = 0;
        }

        if (_imageViewPort.Bottom > _srcBitmap.Height)
        {
            _yOut = true;
            _imageViewPort.Y = _srcBitmap.Height - _imageViewPort.Height;
        }

        if (_imageViewPort.Y < 0)
        {
            _yOut = true;
            _imageViewPort.Y = 0;
        }

        // start drawing
        //_ = WinApi.StretchBlt(_desDcHandle, (int)_displayBounds.X, (int)_displayBounds.Y, (int)_displayBounds.Width, (int)_displayBounds.Height, _srcDcHandle, (int)_imageViewPort.X, (int)_imageViewPort.Y, (int)_imageViewPort.Width, (int)_imageViewPort.Height, TernaryRasterOperations.SRCCOPY);

        _ = WinApi.AlphaBlend(_desDcHandle, (int)_displayBounds.X, (int)_displayBounds.Y, (int)_displayBounds.Width, (int)_displayBounds.Height, _srcDcHandle, (int)_imageViewPort.X, (int)_imageViewPort.Y, (int)_imageViewPort.Width, (int)_imageViewPort.Height, new(255));


        _graphics?.ReleaseHdc(_desDcHandle);
        

        _desDcHandle = default;
    }

    
}

