/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using D2Phap;
using DirectN;
using System.Runtime.InteropServices;
using WicNet;

namespace ImageGlass.Views;


/// <summary>
/// Provides helper functions for <see cref="DXCanvas"/>.
/// </summary>
public static class VHelper
{
    /// <summary>
    /// Creates checkerboard tile brush (GDI+)
    /// </summary>
    public static TextureBrush CreateCheckerBoxTileGdip(float cellSize, Color cellColor1, Color cellColor2)
    {
        // draw the tile
        var width = cellSize * 2;
        var height = cellSize * 2;
        var tileImg = new Bitmap((int)width, (int)height);

        using var g = Graphics.FromImage(tileImg);
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

        return new TextureBrush(tileImg);
    }


    /// <summary>
    /// Creates checkerboard tile brush (Direct2D)
    /// </summary>
    public static ComObject<ID2D1BitmapBrush1> CreateCheckerBoxTileD2D(IComObject<ID2D1DeviceContext6> dc, float cellSize, Color cellColor1, Color cellColor2)
    {
        // create tile: [X,O]
        //              [O,X]
        var width = (int)cellSize * 2;
        var height = (int)cellSize * 2;

        var tileImg = new WicBitmapSource(width, height, WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);

        using var tileImgDc = tileImg.CreateRenderTarget();
        tileImgDc.Object.SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_ALIASED);
        tileImgDc.BeginDraw();


        // draw X cells -------------------------------
        var color1 = DXHelper.FromColor(cellColor1);
        tileImgDc.Object.CreateSolidColorBrush(color1, IntPtr.Zero, out var brush1);

        // draw cell: [X, ]
        //            [ ,X]
        tileImgDc.Object.FillRectangle(DXHelper.ToD2DRectF(0, 0, cellSize, cellSize), brush1);
        tileImgDc.Object.FillRectangle(DXHelper.ToD2DRectF(cellSize, cellSize, cellSize, cellSize), brush1);


        // draw O cells -------------------------------
        var color2 = DXHelper.FromColor(cellColor2);
        tileImgDc.Object.CreateSolidColorBrush(color2, IntPtr.Zero, out var brush2);

        // draw cell: [X,O]
        //            [O,X]
        tileImgDc.Object.FillRectangle(DXHelper.ToD2DRectF(cellSize, 0, cellSize, cellSize), brush2);
        tileImgDc.Object.FillRectangle(DXHelper.ToD2DRectF(0, cellSize, cellSize, cellSize), brush2);


        tileImgDc.EndDraw();


        // create D2DBitmap from WICBitmapSource
        using var bmp = DXHelper.ToD2D1Bitmap(dc, tileImg);
        var bmpPropsPtr = new D2D1_BITMAP_BRUSH_PROPERTIES1()
        {
            extendModeX = D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_WRAP,
            extendModeY = D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_WRAP,
        }.StructureToPtr();

        // create bitmap brush
        dc.Object.CreateBitmapBrush(bmp.Object, bmpPropsPtr, IntPtr.Zero, out ID2D1BitmapBrush1 bmpBrush).ThrowOnError();


        Marshal.FreeHGlobal(bmpPropsPtr);

        return new ComObject<ID2D1BitmapBrush1>(bmpBrush);
    }

}

