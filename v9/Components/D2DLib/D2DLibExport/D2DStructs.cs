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

using System;
using System.Runtime.InteropServices;

using FLOAT = System.Single;

namespace unvell.D2DLib
{
	#region Color
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct D2DColor
	{
		public FLOAT r;
		public FLOAT g;
		public FLOAT b;
		public FLOAT a;

		public D2DColor(FLOAT r, FLOAT g, FLOAT b)
			: this(1, r, g, b)
		{
		}

		public D2DColor(FLOAT a, FLOAT r, FLOAT g, FLOAT b)
		{
			this.a = a;
			this.r = r;
			this.g = g;
			this.b = b;
		}

		public D2DColor(FLOAT alpha, D2DColor color)
		{
			this.a = alpha;
			this.r = color.r;
			this.g = color.g;
			this.b = color.b;
		}

		public static D2DColor operator *(D2DColor c, float s) {
			return new D2DColor(c.a, c.r * s, c.g * s, c.b * s);
		}

		public static bool operator ==(D2DColor c1, D2DColor c2)
		{
			return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b && c1.a == c2.a;
		}

		public static bool operator !=(D2DColor c1, D2DColor c2)
		{
			return c1.r != c2.r || c1.g != c2.g || c1.b != c2.b || c1.a != c2.a;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is D2DColor)) return false;
			var c2 = (D2DColor)obj;

			return this.r == c2.r && this.g == c2.g && this.b == c2.b && this.a == c2.a;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static D2DColor FromGDIColor(System.Drawing.Color gdiColor)
		{
			return new D2DColor(gdiColor.A / 255f, gdiColor.R / 255f,
				gdiColor.G / 255f, gdiColor.B / 255f);
		}

		public static System.Drawing.Color ToGDIColor(D2DColor d2color)
		{
			var c = MathFunctions.Clamp(d2color * 255);
			return System.Drawing.Color.FromArgb((int)c.a, (int)c.r, (int)c.g, (int)c.b);
		}

		private static readonly Random rand = new Random();

		/// <summary>
		/// Create color by randomly color components.
		/// </summary>
		/// <returns></returns>
		public static D2DColor Randomly() {
			return new D2DColor(1, (float)rand.NextDouble(), (float)rand.NextDouble(),
				(float)rand.NextDouble());
		}

		public static readonly D2DColor Transparent = new D2DColor(0, 0, 0, 0);

		public static readonly D2DColor Black = new D2DColor(0, 0, 0);
		public static readonly D2DColor DimGray = D2DColor.FromGDIColor(System.Drawing.Color.DimGray);
		public static readonly D2DColor Gray = D2DColor.FromGDIColor(System.Drawing.Color.Gray);
		public static readonly D2DColor DarkGray = D2DColor.FromGDIColor(System.Drawing.Color.DarkGray);
		public static readonly D2DColor Silver = D2DColor.FromGDIColor(System.Drawing.Color.Silver);
		public static readonly D2DColor GhostWhite = D2DColor.FromGDIColor(System.Drawing.Color.GhostWhite);
		public static readonly D2DColor LightGray = D2DColor.FromGDIColor(System.Drawing.Color.LightGray);
		public static readonly D2DColor White = D2DColor.FromGDIColor(System.Drawing.Color.White);
		public static readonly D2DColor SlateGray = D2DColor.FromGDIColor(System.Drawing.Color.SlateGray);
		public static readonly D2DColor DarkSlateGray = D2DColor.FromGDIColor(System.Drawing.Color.DarkSlateGray);
    public static readonly D2DColor WhiteSmoke = D2DColor.FromGDIColor(System.Drawing.Color.WhiteSmoke);

    public static readonly D2DColor Red = D2DColor.FromGDIColor(System.Drawing.Color.Red);
		public static readonly D2DColor DarkRed = D2DColor.FromGDIColor(System.Drawing.Color.DarkRed);
		public static readonly D2DColor PaleVioletRed = D2DColor.FromGDIColor(System.Drawing.Color.PaleVioletRed);
		public static readonly D2DColor OrangeRed = D2DColor.FromGDIColor(System.Drawing.Color.OrangeRed);
		public static readonly D2DColor IndianRed = D2DColor.FromGDIColor(System.Drawing.Color.IndianRed);
		public static readonly D2DColor MediumVioletRed = D2DColor.FromGDIColor(System.Drawing.Color.MediumVioletRed);
		public static readonly D2DColor Coral = D2DColor.FromGDIColor(System.Drawing.Color.Coral);
		public static readonly D2DColor LightCoral = D2DColor.FromGDIColor(System.Drawing.Color.LightCoral);

		public static readonly D2DColor Beige = D2DColor.FromGDIColor(System.Drawing.Color.Beige);
		public static readonly D2DColor Bisque = D2DColor.FromGDIColor(System.Drawing.Color.Bisque);
		public static readonly D2DColor LightYellow = D2DColor.FromGDIColor(System.Drawing.Color.LightYellow);
		public static readonly D2DColor Yellow = D2DColor.FromGDIColor(System.Drawing.Color.Yellow);
		public static readonly D2DColor Gold = D2DColor.FromGDIColor(System.Drawing.Color.Gold);
		public static readonly D2DColor Goldenrod = D2DColor.FromGDIColor(System.Drawing.Color.Goldenrod);
    public static readonly D2DColor LightGoldenrodYellow = D2DColor.FromGDIColor(System.Drawing.Color.LightGoldenrodYellow);
		public static readonly D2DColor Orange = D2DColor.FromGDIColor(System.Drawing.Color.Orange);
		public static readonly D2DColor DarkOrange = D2DColor.FromGDIColor(System.Drawing.Color.DarkOrange);
		public static readonly D2DColor BurlyWood = D2DColor.FromGDIColor(System.Drawing.Color.BurlyWood);
		public static readonly D2DColor Chocolate = D2DColor.FromGDIColor(System.Drawing.Color.Chocolate);

		public static readonly D2DColor LawnGreen = D2DColor.FromGDIColor(System.Drawing.Color.LawnGreen);
		public static readonly D2DColor LightGreen = D2DColor.FromGDIColor(System.Drawing.Color.LightGreen);
		public static readonly D2DColor LightSeaGreen = D2DColor.FromGDIColor(System.Drawing.Color.LightSeaGreen);
		public static readonly D2DColor MediumSeaGreen = D2DColor.FromGDIColor(System.Drawing.Color.MediumSeaGreen);
		public static readonly D2DColor DarkSeaGreen = D2DColor.FromGDIColor(System.Drawing.Color.DarkSeaGreen);
		public static readonly D2DColor Green = D2DColor.FromGDIColor(System.Drawing.Color.Green);
		public static readonly D2DColor DarkGreen = D2DColor.FromGDIColor(System.Drawing.Color.DarkGreen);
		public static readonly D2DColor DarkOliveGreen = D2DColor.FromGDIColor(System.Drawing.Color.DarkOliveGreen);
		public static readonly D2DColor ForestGreen = D2DColor.FromGDIColor(System.Drawing.Color.ForestGreen);
		public static readonly D2DColor GreenYellow = D2DColor.FromGDIColor(System.Drawing.Color.GreenYellow);

		public static readonly D2DColor AliceBlue = D2DColor.FromGDIColor(System.Drawing.Color.AliceBlue);
		public static readonly D2DColor LightBlue = D2DColor.FromGDIColor(System.Drawing.Color.LightBlue);
		public static readonly D2DColor Blue = D2DColor.FromGDIColor(System.Drawing.Color.Blue);
		public static readonly D2DColor DarkBlue = D2DColor.FromGDIColor(System.Drawing.Color.DarkBlue);
		public static readonly D2DColor SkyBlue = D2DColor.FromGDIColor(System.Drawing.Color.SkyBlue);
		public static readonly D2DColor SteelBlue = D2DColor.FromGDIColor(System.Drawing.Color.SteelBlue);
		public static readonly D2DColor BlueViolet = D2DColor.FromGDIColor(System.Drawing.Color.BlueViolet);
		public static readonly D2DColor CadetBlue = D2DColor.FromGDIColor(System.Drawing.Color.CadetBlue);
		public static readonly D2DColor BlanchedAlmond = D2DColor.FromGDIColor(System.Drawing.Color.BlanchedAlmond);
		public static readonly D2DColor PowderBlue = D2DColor.FromGDIColor(System.Drawing.Color.PowderBlue);
		public static readonly D2DColor CornflowerBlue = D2DColor.FromGDIColor(System.Drawing.Color.CornflowerBlue);

    public static readonly D2DColor Cyan = D2DColor.FromGDIColor(System.Drawing.Color.Cyan);
    public static readonly D2DColor DarkCyan = D2DColor.FromGDIColor(System.Drawing.Color.DarkCyan);
    public static readonly D2DColor LightCyan = D2DColor.FromGDIColor(System.Drawing.Color.LightCyan);

    public static readonly D2DColor Cornsilk = D2DColor.FromGDIColor(System.Drawing.Color.Cornsilk);
    public static readonly D2DColor Thistle = D2DColor.FromGDIColor(System.Drawing.Color.Thistle);
    public static readonly D2DColor Tomato = D2DColor.FromGDIColor(System.Drawing.Color.Tomato);

    public static readonly D2DColor Pink = D2DColor.FromGDIColor(System.Drawing.Color.Pink);
    public static readonly D2DColor DeepPink = D2DColor.FromGDIColor(System.Drawing.Color.DeepPink);
    public static readonly D2DColor HotPink = D2DColor.FromGDIColor(System.Drawing.Color.HotPink);
    public static readonly D2DColor LightPink = D2DColor.FromGDIColor(System.Drawing.Color.LightPink);
	}
	#endregion

	#region Rect
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct D2DRect
	{
		public FLOAT left;
		public FLOAT top;
		public FLOAT right;
		public FLOAT bottom;

		public D2DRect(float left, float top, float width, float height)
		{
			this.left = left;
			this.top = top;
			this.right = left + width;
			this.bottom = top + height;
		}

		public D2DRect(D2DPoint origin, D2DSize size)
			: this(origin.x - size.width * 0.5f, origin.y - size.height * 0.5f, size.width, size.height)
		{ }

		public D2DPoint Location
		{
			get { return new D2DPoint(left, top); }
			set
			{
				FLOAT width = this.right - this.left;
				FLOAT height = this.bottom - this.top;
				this.left = value.x;
				this.right = value.x + width;
				this.top = value.y;
				this.bottom = value.y + height;
			}
		}

		public FLOAT Width
		{
			get { return this.right - this.left; }
			set { this.right = this.left + value; }
		}

		public FLOAT Height
		{
			get { return this.bottom - this.top; }
			set { this.bottom = this.top + value; }
		}

		public void Offset(FLOAT x, FLOAT y)
		{
			this.left += x;
			this.right += x;
			this.top += y;
			this.bottom += y;
		}

		public FLOAT X
		{
			get { return this.left; }
			set
			{
				FLOAT width = this.right - this.left;
				this.left = value;
				this.right = value + width;
			}
		}

		public FLOAT Y
		{
			get { return this.top; }
			set
			{
				FLOAT height = this.bottom - this.top;
				this.top = value;
				this.bottom = value + height;
			}
		}

		public D2DSize Size
		{
			get
			{
				return new D2DSize(this.Width, this.Height);
			}
			set
			{
				this.Width = value.width;
				this.Height = value.height;
			}
		}

		public static implicit operator D2DRect(System.Drawing.Rectangle rect)
		{
			return new D2DRect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static implicit operator D2DRect(System.Drawing.RectangleF rect)
		{
			return new D2DRect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static implicit operator System.Drawing.RectangleF(D2DRect rect)
		{
			return new System.Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static explicit operator System.Drawing.Rectangle(D2DRect rect)
		{
			return System.Drawing.Rectangle.Round(rect);
		}
	}
	#endregion Rect

	#region Rounded Rect

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct D2DRoundedRect
	{
		public D2DRect rect;
		public FLOAT radiusX;
		public FLOAT radiusY;
	}
	#endregion Rounded Rect

	#region Point
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct D2DPoint
	{
		public FLOAT x;
		public FLOAT y;

		public D2DPoint(FLOAT x, FLOAT y)
		{
			this.x = x;
			this.y = y;
		}

		public void Offset(FLOAT offx, FLOAT offy)
		{
			this.x += offx;
			this.y += offy;
		}

		public static readonly D2DPoint Zero = new D2DPoint(0, 0);
		public static readonly D2DPoint One = new D2DPoint(1, 1);

		public override bool Equals(object obj)
		{
			if (!(obj is D2DPoint)) return false;

			var p2 = (D2DPoint)obj;

			return x == p2.x && y == p2.y;
		}

		public static bool operator==(D2DPoint p1, D2DPoint p2)
		{
			return p1.x == p2.x && p1.y == p2.y;
		}

		public static bool operator !=(D2DPoint p1, D2DPoint p2)
		{
			return p1.x != p2.x || p1.y != p2.y;
		}

		public static implicit operator D2DPoint(System.Drawing.Point p)
		{
			return new D2DPoint(p.X, p.Y);
		}

		public static implicit operator D2DPoint(System.Drawing.PointF p)
		{
			return new D2DPoint(p.X, p.Y);
		}

		public static implicit operator System.Drawing.PointF(D2DPoint p)
		{
			return new System.Drawing.PointF(p.x, p.y);
		}

		public static explicit operator System.Drawing.Point(D2DPoint p)
		{
			return System.Drawing.Point.Round(p);
		}

		public override int GetHashCode()
		{
			return (int)((this.x * 0xff) + this.y);
		}
	}
	#endregion

	#region Size
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct D2DSize
	{
		public FLOAT width;
		public FLOAT height;

		public D2DSize(FLOAT width, FLOAT height)
		{
			this.width = width;
			this.height = height;
		}

		public static readonly D2DSize Empty = new D2DSize(0, 0);

		public static implicit operator D2DSize(System.Drawing.Size wsize)
		{
			return new D2DSize(wsize.Width, wsize.Height);
		}

		public static implicit operator D2DSize(System.Drawing.SizeF wsize)
		{
			return new D2DSize(wsize.Width, wsize.Height);
		}

		public static implicit operator System.Drawing.SizeF(D2DSize s)
		{
			return new System.Drawing.SizeF(s.width, s.height);
		}

		public static explicit operator System.Drawing.Size(D2DSize s)
		{
			return System.Drawing.Size.Round(s);
		}

		public override string ToString()
		{
			return string.Format("D2DSize({0}, {1})", this.width, this.height);
		}
	}
	#endregion Size

	#region Ellipse
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct D2DEllipse
	{
		public D2DPoint origin;
		public FLOAT radiusX;
		public FLOAT radiusY;

		public D2DEllipse(D2DPoint center, FLOAT radiusX, FLOAT radiusY)
		{
			this.origin = center;
			this.radiusX = radiusX;
			this.radiusY = radiusY;
		}


		public D2DEllipse(D2DPoint center, D2DSize radius)
			: this(center, radius.width, radius.height)
		{
		}

		public D2DEllipse(FLOAT x, FLOAT y, FLOAT rx, FLOAT ry)
			: this(new D2DPoint(x, y), rx, ry)
		{
		}

		public FLOAT X { get { return origin.x; } set { origin.x = value; } }
		public FLOAT Y { get { return origin.y; } set { origin.y = value; } }
	}
	#endregion

	#region BezierSegment
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct D2DBezierSegment
	{
		public D2DPoint point1;
		public D2DPoint point2;
		public D2DPoint point3;

		public D2DBezierSegment(D2DPoint point1, D2DPoint point2, D2DPoint point3)
		{
			this.point1 = point1;
			this.point2 = point2;
			this.point3 = point3;
		}

		public D2DBezierSegment(FLOAT x1, FLOAT y1, FLOAT x2, FLOAT y2, FLOAT x3, FLOAT y3)
		{
			this.point1 = new D2DPoint(x1, y1);
			this.point2 = new D2DPoint(x2, y2);
			this.point3 = new D2DPoint(x3, y3);
		}
	}
	#endregion

	#region Matrix
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct D2DMatrix3x2
	{
		public FLOAT a1, b1;
		public FLOAT a2, b2;
		public FLOAT a3, b3;

		public D2DMatrix3x2(float a1, float b1, float a2, float b2, float a3, float b3) {
			this.a1 = a1; this.b1 = b1;
			this.a2 = a2; this.b2 = b2;
			this.a3 = a3; this.b3 = b3;
		}
	}

	#endregion // Matrix
}
