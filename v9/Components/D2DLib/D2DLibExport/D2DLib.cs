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
using System.Drawing.Drawing2D;

namespace unvell.D2DLib
{
	internal static class D2D
	{

#if DEBUG

#if X86
		const string DLL_NAME = "d2dlib32d.dll";
#elif X64
		const string DLL_NAME = "d2dlib64d.dll";
#endif

#else // Release

#if X86
		const string DLL_NAME = "d2dlib32.dll";
#elif X64
		const string DLL_NAME = "d2dlib64.dll";
#endif

#endif

		#region Device Context

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE GetLastResult();

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE CreateContext([In] HANDLE hwnd);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DestroyContext([In] HANDLE context);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetContextProperties([In] HANDLE context, D2DAntialiasMode antialiasMode = D2DAntialiasMode.PerPrimitive);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE BeginRender([In] HANDLE context);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE BeginRenderWithBackgroundColor([In] HANDLE context, D2DColor backColor);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE BeginRenderWithBackgroundBitmap(HANDLE context, HANDLE bitmap);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void EndRender([In] HANDLE context);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Flush([In] HANDLE context);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Clear([In] HANDLE context, D2DColor color);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE CreateBitmapRenderTarget([In] HANDLE context, D2DSize size);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawBitmapRenderTarget([In] HANDLE context, HANDLE bitmapRenderTarget, ref D2DRect rect,
			FLOAT opacity = 1, D2DBitmapInterpolationMode interpolationMode = D2DBitmapInterpolationMode.Linear);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE GetBitmapRenderTargetBitmap(HANDLE bitmapRenderTarget);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DestroyBitmapRenderTarget([In] HANDLE context);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE ResizeContext([In] HANDLE context);


		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE GetDPI([In] HANDLE context, out FLOAT dpix, out FLOAT dpiy);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE SetDPI([In] HANDLE context, FLOAT dpix, FLOAT dpiy);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void PushClip([In] HANDLE context, [In] ref D2DRect rect,
			D2DAntialiasMode antiAliasMode = D2DAntialiasMode.PerPrimitive);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void PopClip([In] HANDLE context);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern HANDLE CreateLayer(HANDLE ctx);
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern HANDLE PushLayer(HANDLE ctx, HANDLE layerHandle, ref D2DRect contentBounds,
      [In, Optional] HANDLE geometryHandle, [In, Optional] HANDLE opacityBrush, 
      LayerOptions layerOptions = LayerOptions.InitializeForClearType);
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void PopLayer(HANDLE ctx);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void PushTransform([In] HANDLE context);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void PopTransform([In] HANDLE context);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void RotateTransform([In] HANDLE context, [In] FLOAT angle);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void RotateTransform([In] HANDLE context, [In] FLOAT angle, [In] D2DPoint center);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void TranslateTransform([In] HANDLE context, [In] FLOAT x, [In] FLOAT y);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ScaleTransform([In] HANDLE context, [In] FLOAT sx, [In] FLOAT sy, [Optional] D2DPoint center);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SkewTransform([In] HANDLE ctx, [In] FLOAT angleX, [In] FLOAT angleY, [Optional] D2DPoint center);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetTransform([In] HANDLE context, [In] ref D2DMatrix3x2 mat);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void GetTransform([In] HANDLE context, [Out] out D2DMatrix3x2 mat);
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ResetTransform([In] HANDLE context);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ReleaseObject([In] HANDLE objectHandle);

		#endregion // Device Context

		#region Simple Sharp

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawLine(HANDLE context, D2DPoint start, D2DPoint end, D2DColor color,
			FLOAT weight = 1, D2DDashStyle dashStyle = D2DDashStyle.Solid,
			D2DCapStyle startCap = D2DCapStyle.Flat, D2DCapStyle endCap = D2DCapStyle.Flat);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawLines(HANDLE context, D2DPoint[] points, UINT count, D2DColor color,
			FLOAT weight = 1, D2DDashStyle dashStyle = D2DDashStyle.Solid);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawRectangle(HANDLE context, ref D2DRect rect, D2DColor color,
			FLOAT weight = 1, D2DDashStyle dashStyle = D2DDashStyle.Solid);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawRectangleWithPen(HANDLE context, ref D2DRect rect, HANDLE strokePen, FLOAT weight = 1);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FillRectangle(HANDLE context, ref D2DRect rect, D2DColor color);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FillRectangleWithBrush(HANDLE context, ref D2DRect rect, HANDLE brush);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawRoundedRect(HANDLE ctx, ref D2DRoundedRect roundedRect,
			D2DColor strokeColor, D2DColor fillColor,
			FLOAT strokeWidth = 1, D2DDashStyle strokeStyle = D2DDashStyle.Solid);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawRoundedRectWithBrush(HANDLE ctx, ref D2DRoundedRect roundedRect,
			HANDLE strokePen, HANDLE fillBrush, float strokeWidth = 1);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawEllipse(HANDLE context, ref D2DEllipse rect, D2DColor color,
			FLOAT width = 1, D2DDashStyle dashStyle = D2DDashStyle.Solid);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FillEllipse(HANDLE context, ref D2DEllipse rect, D2DColor color);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FillEllipseWithBrush(HANDLE ctx, ref D2DEllipse ellipse, HANDLE brush);

		#endregion // Simple Sharp

		#region Text

		[DllImport(DLL_NAME, EntryPoint = "DrawString", CharSet = CharSet.Unicode,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawText([In] HANDLE context, [In] string text, [In] D2DColor color,
			[In] string fontName, [In] FLOAT fontSize, [In] ref D2DRect rect,
			[In] DWriteTextAlignment halign = DWriteTextAlignment.Leading,
			[In] DWriteParagraphAlignment valign = DWriteParagraphAlignment.Near);

		[DllImport(DLL_NAME, EntryPoint = "MeasureText", CharSet = CharSet.Unicode,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern void MeasureText([In] HANDLE ctx, [In] string text, [In] string fontName,
			[In] FLOAT fontSize, ref D2DSize size);

		#endregion // Text

		#region Geometry

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE CreateRectangleGeometry([In] HANDLE ctx, [In] ref D2DRect rect);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void DestroyGeometry(HANDLE geometryContext);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern HANDLE CreateEllipseGeometry(HANDLE ctx, ref D2DEllipse ellipse);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern HANDLE CreatePathGeometry(HANDLE ctx);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void DestroyPathGeometry(HANDLE ctx);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FillGeometryWithBrush([In] HANDLE ctx, [In] HANDLE geoHandle,
			[In] HANDLE brushHandle, [Optional] HANDLE opacityBrushHandle);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawPolygon(HANDLE ctx, D2DPoint[] points, UINT count,
			D2DColor strokeColor, FLOAT strokeWidth, D2DDashStyle dashStyle, D2DColor fillColor);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawPolygonWithBrush(HANDLE ctx, D2DPoint[] points, UINT count,
			D2DColor strokeColor, FLOAT strokeWidth, D2DDashStyle dashStyle, HANDLE brushHandler);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetPathStartPoint(HANDLE ctx, D2DPoint startPoint);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ClosePath(HANDLE ctx);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void AddPathLines(HANDLE path, D2DPoint[] points, uint count);
		public static void AddPathLines(HANDLE path, D2DPoint[] points) { AddPathLines(path, points, (uint)points.Length); }

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void AddPathBeziers(HANDLE ctx, D2DBezierSegment[] bezierSegments, uint count);

		public static void AddPathBeziers(HANDLE ctx, D2DBezierSegment[] bezierSegments)
		{
			AddPathBeziers(ctx, bezierSegments, (uint)bezierSegments.Length);
		}

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void AddPathEllipse(HANDLE path, ref D2DEllipse ellipse);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void AddPathArc(HANDLE ctx, D2DPoint endPoint, D2DSize size, FLOAT sweepAngle,
			D2DArcSize arcSize = D2DArcSize.Small,
			D2DSweepDirection sweepDirection = D2DSweepDirection.Clockwise);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawBeziers(HANDLE ctx, D2DBezierSegment[] bezierSegments, UINT count,
															D2DColor strokeColor, FLOAT strokeWidth = 1,
															D2DDashStyle dashStyle = D2DDashStyle.Solid);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawPath(HANDLE path, D2DColor strokeColor, FLOAT strokeWidth = 1,
			D2DDashStyle dashStyle = D2DDashStyle.Solid);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawPathWithPen(HANDLE path, HANDLE pen, FLOAT strokeWidth = 1);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FillPathD(HANDLE path, D2DColor fillColor);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FillGeometryWithBrush(HANDLE path, HANDLE brush);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool PathFillContainsPoint(HANDLE pathCtx, D2DPoint point);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool PathStrokeContainsPoint(HANDLE pathCtx, D2DPoint point, FLOAT strokeWidth = 1,
			D2DDashStyle dashStyle = D2DDashStyle.Solid);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void GetGeometryBounds(HANDLE pathCtx, ref D2DRect rect);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void GetGeometryTransformedBounds(HANDLE pathCtx, ref D2DMatrix3x2 mat3x2, ref D2DRect rect);

		#endregion // Geometry

		#region Pen
		[DllImport(DLL_NAME, EntryPoint = "CreatePenStroke", CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE CreatePen(HANDLE ctx, D2DColor strokeColor, D2DDashStyle dashStyle = D2DDashStyle.Solid,
			FLOAT[] dashes = null, UINT dashCount = 0, FLOAT dashOffset = 0.0f);

		[DllImport(DLL_NAME, EntryPoint = "DestroyPenStroke", CallingConvention = CallingConvention.Cdecl)]
		public static extern void DestroyPen(HANDLE pen);
		#endregion Pen

		#region Brush

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE CreateSolidColorBrush(HANDLE ctx, D2DColor color);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetSolidColorBrushColor(HANDLE brush, D2DColor color);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE CreateLinearGradientBrush(HANDLE ctx, D2DPoint startPoint, D2DPoint endPoint,
																											D2DGradientStop[] gradientStops, UINT gradientStopCount);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE CreateRadialGradientBrush(HANDLE ctx, D2DPoint origin, D2DPoint offset,
																													FLOAT radiusX, FLOAT radiusY, D2DGradientStop[] gradientStops,
																													UINT gradientStopCount);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ReleaseBrush(HANDLE brushCtx);

		#endregion // Brush

		#region Bitmap
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE CreateBitmapFromHBitmap(HANDLE context, HANDLE hBitmap, bool useAlphaChannel);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE CreateBitmapFromBytes(HANDLE context, byte[] buffer, UINT offset, UINT length);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern HANDLE CreateBitmapFromMemory(HANDLE ctx, UINT width, UINT height, UINT stride, IntPtr buffer,
			UINT offset, UINT length);

		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		public static extern HANDLE CreateBitmapFromFile(HANDLE context, string filepath);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawD2DBitmap(HANDLE context, HANDLE bitmap);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawD2DBitmap(HANDLE context, HANDLE bitmap, ref D2DRect destRect);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawD2DBitmap(HANDLE context, HANDLE bitmap, ref D2DRect destRect, ref D2DRect srcRect,
			FLOAT opacity = 1, D2DBitmapInterpolationMode interpolationMode = D2DBitmapInterpolationMode.Linear);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawGDIBitmap(HANDLE context, HANDLE bitmap, FLOAT opacity = 1,
			D2DBitmapInterpolationMode interpolationMode = D2DBitmapInterpolationMode.Linear);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DrawGDIBitmapRect(HANDLE context, HANDLE bitmap,
			ref D2DRect destRect, ref D2DRect srcRect, FLOAT opacity = 1, bool alpha = false,
			D2DBitmapInterpolationMode interpolationMode = D2DBitmapInterpolationMode.Linear);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern D2DSize GetBitmapSize(HANDLE d2dbitmap);
		#endregion

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void TestDraw(HANDLE ctx);
	}
}
