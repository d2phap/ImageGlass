
namespace ImageGlass.PhotoBox;

public class ViewBox
{
    // Host control , host control graphics , and source bitmap
    private Control Host = new();
    private Graphics Gr;
    private Bitmap? srcBitmap;

    // Source bitmap Handle
    private IntPtr HBitmapSrc;

    // Source DC handle , Destination DC handle
    private IntPtr srcHDC;
    private IntPtr desHDC;

    private PointF P;
    private PointF CP;
    private PointF CS;

    // Main rectangle , Boundary rectangle
    private RectangleF Mrec;
    private RectangleF Brec;

    // current zoom, minimum zoom, maximum zoom, previous zoom (bigger means zoom in)
    private float Zfactor = 1f;
    private float MinZ = 0.05f;
    private float MaxZ = 20f;
    private float oldZfactor = 1f;

    private bool Xout = false;
    private bool Yout = false;
    private bool HostLoadComplete = false;
    private bool DownPress = false;

    public ViewBox(Control hostControl, Bitmap? srcImage)
    {
        Host = hostControl;
        srcBitmap = srcImage;

        Host.MouseDown += Host_MouseDown;
        Host.MouseUp += Host_MouseUp;
        Host.MouseMove += Host_MouseMove;
        Host.Paint += Host_Paint;
        Host.Resize += Host_Resize;

        srcHDC = default;
        desHDC = default;
        HostLoadComplete = true;
    }


    public void Dispose()
    {
        if (srcBitmap is not null)
        {
            srcBitmap.Dispose();
            srcBitmap = null;
        }

        if (!srcHDC.Equals(IntPtr.Zero))
        {
            WinApi.DeleteDC(srcHDC);
            srcHDC = default;
        }

        if (Gr is not null)
        {
            Gr.Dispose();
            Gr = null;
        }

        GC.Collect();
    }

    private void Host_Paint(object? sender, PaintEventArgs e)
    {
        DrawPic(0, 0);
    }

    private void Host_Resize(object? sender, EventArgs e)
    {
        if (HostLoadComplete == true)
            DrawPic(0, 0);
    }

    private void Host_MouseUp(object? sender, MouseEventArgs e)
    {
        if (srcBitmap is null)
            return;

        DownPress = false;
        Host.Cursor = Cursors.Arrow;

        if (CS.X == e.X & CS.Y == e.Y)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (Zfactor > MaxZ)
                    return;
                oldZfactor = Zfactor;
                Zfactor *= 1.3f;
                DrawPic(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (Zfactor < MinZ)
                    return;
                oldZfactor = Zfactor;
                Zfactor = Zfactor / 1.3f;
                DrawPic(e.X, e.Y);
            }
            ZoomChanged?.Invoke(Zfactor);
        }
    }

    private void Host_MouseDown(object? sender, MouseEventArgs e)
    {
        if (srcBitmap is null)
            return;

        P.X = e.X;
        P.Y = e.Y;
        CP.X = 0;
        CP.Y = 0;
        CS.X = e.X;
        CS.Y = e.Y;
        DownPress = true;
    }

    private void Host_MouseMove(object? sender, MouseEventArgs e)
    {
        if (srcBitmap is null)
            return;

        if (DownPress == true)
        {
            Host.Cursor = Cursors.SizeAll;

            // accelerated scrolling when right click drag ----------------
            if (e.Button == MouseButtons.Right)
            {
                CP.X = (P.X - e.X) * (srcBitmap.Width / 2000f);
                CP.Y = (P.Y - e.Y) * (srcBitmap.Height / 2000f);
            }


            Mrec.X = ((P.X - e.X) / Zfactor) + Mrec.X + CP.X;
            Mrec.Y = ((P.Y - e.Y) / Zfactor) + Mrec.Y + CP.Y;
            DrawPic(0, 0);
            if (Xout == false)
                P.X = e.X;
            if (Yout == false)
                P.Y = e.Y;
        }

        MoveOver?.Invoke((e.X - Brec.X) / Zfactor + Mrec.X, (e.Y - Brec.Y) / Zfactor + Mrec.Y);
    }

    private void DrawPic(float ZoomX, float ZoomY)
    {
        if (srcBitmap is null)
            return;

        if (srcHDC.Equals(IntPtr.Zero))
        {
            srcHDC = WinApi.CreateCompatibleDC(IntPtr.Zero);
            HBitmapSrc = srcBitmap.GetHbitmap();
            _ = WinApi.SelectObject(srcHDC, HBitmapSrc);
        }

        if (desHDC.Equals(IntPtr.Zero))
        {
            if (Gr is null)
                Gr = Host.CreateGraphics();
            desHDC = Gr.GetHdc();
            _ = WinApi.SetStretchBltMode(desHDC, StretchBltMode.STRETCH_DELETESCANS);
        }



        Xout = false;
        Yout = false;

        if (Host.Width > srcBitmap.Width * Zfactor)
        {
            Mrec.X = 0;
            Mrec.Width = srcBitmap.Width;
            Brec.X = (Host.Width - srcBitmap.Width * Zfactor) / 2.0f;
            Brec.Width = srcBitmap.Width * Zfactor;

            _ = WinApi.BitBlt(desHDC, 0, 0, (int)Brec.X, Host.Height, srcHDC, 0, 0, TernaryRasterOperations.BLACKNESS);
            _ = WinApi.BitBlt(desHDC, (int)Brec.Right, 0, (int)Brec.X, Host.Height, srcHDC, 0, 0, TernaryRasterOperations.BLACKNESS);
        }
        else
        {
            Mrec.X += (Host.Width / oldZfactor - Host.Width / Zfactor) / ((Host.Width + 0.001f) / ZoomX);
            Mrec.Width = Host.Width / Zfactor;
            Brec.X = 0;
            Brec.Width = Host.Width;
        }

        if (Host.Height > srcBitmap.Height * Zfactor)
        {
            Mrec.Y = 0;
            Mrec.Height = srcBitmap.Height;
            Brec.Y = (Host.Height - srcBitmap.Height * Zfactor) / 2f;
            Brec.Height = srcBitmap.Height * Zfactor;

            WinApi.BitBlt(desHDC, 0, 0, Host.Width, (int)Brec.Y, srcHDC, 0, 0, TernaryRasterOperations.BLACKNESS);
            WinApi.BitBlt(desHDC, 0, (int)Brec.Bottom, Host.Width, (int)Brec.Y, srcHDC, 0, 0, TernaryRasterOperations.BLACKNESS);
        }
        else
        {
            Mrec.Y += (Host.Height / oldZfactor - Host.Height / Zfactor) / ((Host.Height + 0.001f) / ZoomY);
            Mrec.Height = Host.Height / Zfactor;
            Brec.Y = 0;
            Brec.Height = Host.Height;
        }

        oldZfactor = Zfactor;
        // -----------------------------------

        if (Mrec.Right > srcBitmap.Width)
        {
            Xout = true;
            Mrec.X = (srcBitmap.Width - Mrec.Width);
        }

        if (Mrec.X < 0)
        {
            Xout = true;
            Mrec.X = 0;
        }

        if (Mrec.Bottom > srcBitmap.Height)
        {
            Yout = true;
            Mrec.Y = srcBitmap.Height - Mrec.Height;
        }

        if (Mrec.Y < 0)
        {
            Yout = true;
            Mrec.Y = 0;
        }

        _ = WinApi.StretchBlt(desHDC, (int)Brec.X, (int)Brec.Y, (int)Brec.Width, (int)Brec.Height, srcHDC, (int)Mrec.X, (int)Mrec.Y, (int)Mrec.Width, (int)Mrec.Height, TernaryRasterOperations.SRCCOPY);

        Gr.ReleaseHdc(desHDC);
        desHDC = default;
    }

    public Bitmap? Image
    {
        get
        {
            return srcBitmap;
        }
        set
        {
            Dispose();
            srcBitmap = value;
            Zfactor = 1;
            DrawPic(0, 0);
        }
    }

    public IntPtr SourceHDC
    {
        get
        {
            return srcHDC;
        }
    }

    public IntPtr srcBitmapHande
    {
        get
        {
            return HBitmapSrc;
        }
    }

    public float MinZoom
    {
        get
        {
            return MinZ;
        }
        set
        {
            MinZ = MinZoom;
        }
    }


    public float MaxZoom
    {
        get
        {
            return MaxZ;
        }
        set
        {
            MaxZ = MaxZoom;
        }
    }

    public float CurZoom
    {
        get
        {
            return Zfactor;
        }
        set
        {
            Zfactor = CurZoom;
            DrawPic(Host.Width / 2f, Host.Height / 2f);
        }
    }



    public event MoveOverEventHandler MoveOver;

    public delegate void MoveOverEventHandler(float Px, float Py);

    public event ZoomChangedEventHandler ZoomChanged;

    public delegate void ZoomChangedEventHandler(float CurZoom);
}

