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

#pragma once

#include "Context.h"

extern "C"
{
	D2DLIB_API void DestroyGeometry(HANDLE geometryHandle);

	D2DLIB_API HANDLE CreateEllipseGeometry(HANDLE ctx, const D2D1_ELLIPSE& ellipse);

	D2DLIB_API HANDLE CreateRectangleGeometry(HANDLE ctx, D2D1_RECT_F& rect);

	D2DLIB_API HANDLE CreatePathGeometry(HANDLE ctx);
	D2DLIB_API void DestroyPathGeometry(HANDLE handle);

	D2DLIB_API void SetPathStartPoint(HANDLE handle, D2D1_POINT_2F startPoint);
	D2DLIB_API void ClosePath(HANDLE handle);

	D2DLIB_API void AddPathLines(HANDLE ctx, D2D1_POINT_2F* points, UINT count);

	D2DLIB_API void AddPathBeziers(HANDLE ctx, D2D1_BEZIER_SEGMENT* bezierSegments, UINT count);

	D2DLIB_API void AddPathEllipse(HANDLE ctx, const D2D1_ELLIPSE* ellipse);

	D2DLIB_API void AddPathArc(HANDLE ctx, D2D1_POINT_2F endPoint, D2D1_SIZE_F size, FLOAT sweepAngle,
		D2D1_ARC_SIZE arcSize = D2D1_ARC_SIZE::D2D1_ARC_SIZE_SMALL,
		D2D1_SWEEP_DIRECTION sweepDirection = D2D1_SWEEP_DIRECTION::D2D1_SWEEP_DIRECTION_CLOCKWISE);

	D2DLIB_API void DrawBeziers(HANDLE ctx, D2D1_BEZIER_SEGMENT* bezierSegments, UINT count, 
															D2D1_COLOR_F strokeColor, FLOAT strokeWidth = 1, 
															D2D1_DASH_STYLE dashStyle = D2D1_DASH_STYLE::D2D1_DASH_STYLE_SOLID);

	D2DLIB_API void DrawPath(HANDLE pathCtx, D2D1_COLOR_F strokeColor, FLOAT strokeWidth, D2D1_DASH_STYLE dashStyle);
	D2DLIB_API void DrawPathWithPen(HANDLE pathCtx, HANDLE strokePen, FLOAT strokeWidth);
	D2DLIB_API void FillPathD(HANDLE pathCtx, D2D1_COLOR_F fillColor);

	D2DLIB_API void FillGeometryWithBrush(HANDLE ctx, HANDLE geoHandle, 
		__in HANDLE brushHandle, __in_opt HANDLE opacityBrushHandle = NULL);

	D2DLIB_API bool PathFillContainsPoint(HANDLE pathCtx, D2D1_POINT_2F point);
	D2DLIB_API bool PathStrokeContainsPoint(HANDLE pathCtx, D2D1_POINT_2F point, FLOAT strokeWidth = 1,
		D2D1_DASH_STYLE dashStyle = D2D1_DASH_STYLE::D2D1_DASH_STYLE_SOLID);

	D2DLIB_API void GetGeometryBounds(HANDLE pathCtx, __out D2D1_RECT_F* rect);
	D2DLIB_API void GetGeometryTransformedBounds(HANDLE pathCtx, __in D2D1_MATRIX_3X2_F* mat3x2, __out D2D1_RECT_F* rect);

	D2DLIB_API void DrawPolygon(HANDLE ctx, D2D1_POINT_2F* points, UINT count,
		D2D1_COLOR_F strokeColor, FLOAT strokeWidth, D2D1_DASH_STYLE dashStyle, D2D1_COLOR_F fillColor);
	D2DLIB_API void DrawPolygonWithBrush(HANDLE ctx, D2D1_POINT_2F* points, UINT count,
		D2D1_COLOR_F strokeColor, FLOAT strokeWidth, D2D1_DASH_STYLE dashStyle, HANDLE brushHandle);
}