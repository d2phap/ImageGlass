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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using FLOAT = System.Single;
using UINT = System.UInt32;
using UINT32 = System.UInt32;
using HWND = System.IntPtr;
using HANDLE = System.IntPtr;
using HRESULT = System.Int64;
using BOOL = System.Int32;

namespace unvell.D2DLib
{
	public class D2DPen : D2DObject
	{
		public D2DDevice Device { get; private set; }

		public D2DColor Color { get; private set; }

		public D2DDashStyle DashStyle { get; private set; }

		public float[] CustomDashes { get; private set; }

		public float DashOffset { get; private set; }

		internal D2DPen(D2DDevice Device, HANDLE handle, D2DColor color, D2DDashStyle dashStyle = D2DDashStyle.Solid,
			float[] customDashes = null, float dashOffset = 0f)
			: base(handle)
		{
			this.Device = Device;
			this.Color = color;
			this.DashStyle = dashStyle;
			this.CustomDashes = customDashes;
			this.DashOffset = dashOffset;
		}

		public override void Dispose()
		{
			if (this.Handle != IntPtr.Zero)
			{
				this.Device.DestroyPen(this);
				this.handle = IntPtr.Zero;
			}
		}
	}
}
