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

namespace ImageGlass.Base.HybridGraphics;


public unsafe class MemoryBitmap : IDisposable
{
    private uint* buffer;
    public uint* Buffer { get { return buffer; } }
    public IntPtr BufferPtr { get { return (IntPtr)buffer; } }

    public int BufferSize { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int PixelCount { get; private set; }

    const int stride = sizeof(uint);

    public MemoryBitmap(int width, int height)
    {
        Width = width;
        Height = height;
        BufferSize = stride * width * height;
        PixelCount = Width * Height;
        buffer = (uint*)System.Runtime.InteropServices.Marshal.AllocHGlobal(BufferSize);
    }


    public static void CopyBuffer(MemoryBitmap from, MemoryBitmap to)
    {
        CopyBuffer(from.BufferPtr, from.Width, from.Height, to.BufferPtr, to.Width, to.Height, 0, 0, Math.Min(from.Width, to.Width), Math.Min(from.Height, to.Height));
    }

    public static void CopyBuffer(IntPtr source, int sourceWidth, int sourceHeight,
        IntPtr destination, int destWidth, int destHeight, int copyLeft, int copyTop, int copyWidth, int copyHeight)
    {
        uint* fromBuffer = (uint*)source;
        uint* toBuffer = (uint*)destination;

        int right = copyLeft + copyWidth, bottom = copyTop + copyHeight;

        uint* fromLine = fromBuffer + copyTop * sourceWidth;
        uint* toLine = toBuffer + copyTop * destWidth;

        for (int y = copyTop; y < bottom; y++)
        {
            uint* from = fromLine;
            uint* to = toLine;

            for (int x = copyLeft; x < right; x++)
            {
                *to = *from;
                to++;
                from++;
            }

            fromLine += sourceWidth;
            toLine += destWidth;
        }
    }

    public void FillRectangle(int x, int y, int w, int h, Color c)
    {
        FillRectangle(x, y, w, h, (uint)c.ToArgb());
    }

    public void FillRectangle(int x, int y, int w, int h, uint c)
    {
        uint* line = buffer + (y * Width);

        for (int i = 0; i < h; i++)
        {
            uint* p = line + x;

            for (int j = 0; j < w; j++)
            {
                *p = c;
                p++;
            }

            line += Width;
        }
    }

    public void SetPixel(Point v, Color c)
    {
        SetPixel(v, c.ToArgb());
    }

    public void SetPixel(PointF p, int c)
    {
        SetPixel((int)(p.X), (int)(p.Y), (uint)c);
    }

    public void SetPixel(int x, int y, Color c)
    {
        SetPixel(x, y, (uint)c.ToArgb());
    }

    public void SetPixel(int x, int y, uint c)
    {
        *(buffer + (y * Width) + x) = c;
    }

    public uint GetPixel(PointF v)
    {
        return GetPixel((int)v.X, (int)v.Y);
    }

    public uint GetPixel(int x, int y)
    {
        return *(buffer + (y * Width) + x);
    }

    public Color GetColor4i(int x, int y)
    {
        return Color.FromArgb((int)GetPixel(x, y));
    }

    public Color GetColor4f(int x, int y)
    {
        return Color.FromArgb((int)GetPixel(x, y));
    }

    internal void Clear(Color c)
    {
        Clear((uint)c.ToArgb());
    }

    internal void Clear(uint c = 0xffffffff)
    {
        uint* p = buffer;

        for (int i = 0; i < PixelCount; i++)
        {
            *p = c;
            p++;
        }
    }


    #region Dispose

    ~MemoryBitmap()
    {
        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (buffer != null)
            {
                try
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)buffer);
                    buffer = null;
                }
                catch { }
            }
        }
    }

    #endregion

}
