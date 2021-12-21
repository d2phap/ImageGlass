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
	D2DLIB_API void DrawLine(HANDLE ctx, D2D1_POINT_2F start, D2D1_POINT_2F end, D2D1_COLOR_F color,
		FLOAT width = 1, D2D1_DASH_STYLE dashStyle = D2D1_DASH_STYLE::D2D1_DASH_STYLE_SOLID,
		D2D1_CAP_STYLE startCap = D2D1_CAP_STYLE::D2D1_CAP_STYLE_FLAT,
		D2D1_CAP_STYLE endCap = D2D1_CAP_STYLE::D2D1_CAP_STYLE_FLAT);

	D2DLIB_API void DrawLineWithPen(HANDLE ctx, D2D1_POINT_2F start, D2D1_POINT_2F end, HANDLE pen, FLOAT width = 1);

	D2DLIB_API void DrawLines(HANDLE ctx, D2D1_POINT_2F* points, UINT count, D2D1_COLOR_F color,
		FLOAT width = 1, D2D1_DASH_STYLE dashStyle = D2D1_DASH_STYLE::D2D1_DASH_STYLE_SOLID);

	D2DLIB_API void DrawRectangle(HANDLE ctx, D2D1_RECT_F* rect, D2D1_COLOR_F color,
		FLOAT width = 1, D2D1_DASH_STYLE dashStyle = D2D1_DASH_STYLE::D2D1_DASH_STYLE_SOLID);

	D2DLIB_API void DrawRectangleWithPen(HANDLE ctx, D2D1_RECT_F* rect, HANDLE strokePen, FLOAT width = 1);

	D2DLIB_API void FillRectangle(HANDLE ctx, D2D1_RECT_F* rect, D2D1_COLOR_F color);

	D2DLIB_API void FillRectangleWithBrush(HANDLE ctx, D2D1_RECT_F* rect, HANDLE brushHandle);

	D2DLIB_API void DrawRoundedRect(HANDLE ctx, D2D1_ROUNDED_RECT* roundedRect, D2D1_COLOR_F strokeColor, D2D1_COLOR_F fillColor,
		FLOAT strokeWidth = 1, D2D1_DASH_STYLE strokeStyle = D2D1_DASH_STYLE::D2D1_DASH_STYLE_SOLID);

	D2DLIB_API void DrawRoundedRectWithBrush(HANDLE ctx, D2D1_ROUNDED_RECT* roundedRect,
		HANDLE strokePen, HANDLE fillBrush, float strokeWidth = 1);

	D2DLIB_API void DrawEllipse(HANDLE ctx, D2D1_ELLIPSE* ellipse, D2D1_COLOR_F color,
		FLOAT width = 1, D2D1_DASH_STYLE dashStyle = D2D1_DASH_STYLE::D2D1_DASH_STYLE_SOLID);

	D2DLIB_API void FillEllipse(HANDLE ctx, D2D1_ELLIPSE* ellipse, D2D1_COLOR_F color);

	D2DLIB_API void FillEllipseWithBrush(HANDLE ctx, D2D1_ELLIPSE* ellipse, HANDLE brush);

}