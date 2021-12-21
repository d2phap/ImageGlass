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

enum BrushType {
	BrushType_SolidBrush,
	BrushType_LinearGradientBrush,
	BrushType_RadialGradientBrush,
};

struct BrushContext {
	D2DContext* context;
	ID2D1Brush* brush;
	BrushType type;
	union {
		ID2D1GradientStopCollection* gradientStops = NULL;
	};
};

extern "C"
{
	D2DLIB_API HANDLE CreateSolidColorBrush(HANDLE ctx, D2D1_COLOR_F color);
	D2DLIB_API void SetSolidColorBrushColor(HANDLE brush, D2D1_COLOR_F color);

	D2DLIB_API HANDLE CreateLinearGradientBrush(HANDLE ctx, D2D1_POINT_2F startPoint, D2D1_POINT_2F endPoint,
		D2D1_GRADIENT_STOP* gradientStops, UINT gradientStopCount);

	D2DLIB_API HANDLE CreateRadialGradientBrush(HANDLE ctx, D2D1_POINT_2F origin, D2D1_POINT_2F offset,
																						  FLOAT radiusX, FLOAT radiusY, D2D1_GRADIENT_STOP* gradientStops, 
																							UINT gradientStopCount);

	D2DLIB_API void ReleaseBrush(HANDLE brushHandle);
}