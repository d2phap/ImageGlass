
using D2Phap;
using DirectN;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WicNet;

namespace ImageGlass.Views;


public static class VHelper
{
    /// <summary>
    /// Creates checker box tile for drawing checkerboard (GDI+)
    /// </summary>
    public static Bitmap CreateCheckerBoxTileGdip(float cellSize, Color cellColor1, Color cellColor2)
    {
        // draw the tile
        var width = cellSize * 2;
        var height = cellSize * 2;
        var result = new Bitmap((int)width, (int)height);

        using var g = Graphics.FromImage(result);
        using (Brush brush = new SolidBrush(cellColor1))
        {
            g.FillRectangle(brush, new RectangleF(0, 0, cellSize, cellSize));
            g.FillRectangle(brush, new RectangleF(cellSize, cellSize, cellSize, cellSize));
        }

        using (Brush brush = new SolidBrush(cellColor2))
        {
            g.FillRectangle(brush, new RectangleF(cellSize, 0, cellSize, cellSize));
            g.FillRectangle(brush, new RectangleF(0, cellSize, cellSize, cellSize));
        }

        return result;
    }


    /// <summary>
    /// Creates checker box tile for drawing checkerboard (Direct2D)
    /// </summary>
    public static WicBitmapSource CreateCheckerBoxTileD2D(float cellSize, Color cellColor1, Color cellColor2)
    {
        // create tile: [X,O]
        //              [O,X]
        var width = (int)cellSize * 2;
        var height = (int)cellSize * 2;

        var wicSrc = new WicBitmapSource(width, height, WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);

        var dc = wicSrc.CreateRenderTarget().Object;
        dc.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_ALIASED);
        dc.BeginDraw();


        // draw X cells -------------------------------
        var color1 = DXHelper.FromColor(cellColor1);
        dc.CreateSolidColorBrush(color1, IntPtr.Zero, out var brush1);

        // draw cell: [X, ]
        //            [ ,X]
        dc.FillRectangle(DXHelper.ToD2DRectF(0, 0, cellSize, cellSize), brush1);
        dc.FillRectangle(DXHelper.ToD2DRectF(cellSize, cellSize, cellSize, cellSize), brush1);


        // draw O cells -------------------------------
        var color2 = DXHelper.FromColor(cellColor2);
        dc.CreateSolidColorBrush(color2, IntPtr.Zero, out var brush2);

        // draw cell: [X,O]
        //            [O,X]
        dc.FillRectangle(DXHelper.ToD2DRectF(cellSize, 0, cellSize, cellSize), brush2);
        dc.FillRectangle(DXHelper.ToD2DRectF(0, cellSize, cellSize, cellSize), brush2);


        dc.EndDraw();

        return wicSrc;
    }

}

