

namespace D2DLibExport;


public unsafe class MemoryBitmap : IDisposable
{
	private uint* buffer;
	public uint* Buffer { get { return this.buffer; } }
	public IntPtr BufferPtr { get { return (IntPtr)this.buffer; } }

	public int BufferSize { get; private set; }
	public int Width { get; private set; }
	public int Height { get; private set; }
	public int PixelCount { get; private set; }

	const int stride = sizeof(uint);

	public MemoryBitmap(int width, int height)
	{
		this.Width = width;
		this.Height = height;
		this.BufferSize = stride * width * height;
		this.PixelCount = this.Width * this.Height;
		this.buffer = (uint*)System.Runtime.InteropServices.Marshal.AllocHGlobal(this.BufferSize);
	}

	~MemoryBitmap()
	{
		this.Dispose();
	}

	public static void CopyBuffer(MemoryBitmap from, MemoryBitmap to)
	{
		CopyBuffer(from.BufferPtr, from.Width, from.Height, to.BufferPtr, to.Width, to.Height, 0, 0,
			Math.Min(from.Width, to.Width), Math.Min(from.Height, to.Height));
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
		this.FillRectangle(x, y, w, h, (uint)c.ToArgb());
	}

	public void FillRectangle(int x, int y, int w, int h, uint c)
	{
		uint* line = this.buffer + (y * this.Width);

		for (int i = 0; i < h; i++)
		{
			uint* p = line + x;

			for (int j = 0; j < w; j++)
			{
				*p = c;
				p++;
			}

			line += this.Width;
		}
	}

	public void SetPixel(Point v, Color c)
	{
		this.SetPixel(v, c.ToArgb());
	}

	public void SetPixel(PointF p, int c)
	{
		this.SetPixel((int)(p.X), (int)(p.Y), (uint)c);
	}

	public void SetPixel(int x, int y, Color c)
	{
		this.SetPixel(x, y, (uint)c.ToArgb());
	}

	public void SetPixel(int x, int y, uint c)
	{
		*(this.buffer + (y * this.Width) + x) = c;
	}

	public uint GetPixel(PointF v)
	{
		return this.GetPixel((int)v.X, (int)v.Y);
	}

	public uint GetPixel(int x, int y)
	{
		return *(this.buffer + (y * this.Width) + x);
	}

	public Color GetColor4i(int x, int y)
	{
		return Color.FromArgb((int)this.GetPixel(x, y));
	}

	public Color GetColor4f(int x, int y)
	{
		return Color.FromArgb((int)this.GetPixel(x, y));
	}

	internal void Clear(Color c)
	{
		this.Clear((uint)c.ToArgb());
	}

	internal void Clear(uint c = 0xffffffff)
	{
		uint* p = this.buffer;

		for (int i = 0; i < this.PixelCount; i++)
		{
			*p = c;
			p++;
		}
	}

	public void Dispose()
	{
		if (this.buffer != null)
		{
			try
			{
				System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)this.buffer);
				this.buffer = null;
			}
			catch { }
		}
	}
}
