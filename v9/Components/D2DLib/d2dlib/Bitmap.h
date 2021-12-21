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
	D2DLIB_API HANDLE CreateBitmapFromHBitmap(HANDLE ctx, HBITMAP hBitmap, BOOL useAlpha = FALSE);
	D2DLIB_API HANDLE CreateBitmapFromBytes(HANDLE ctx, BYTE* buffer, UINT offset, UINT length);
	D2DLIB_API HANDLE CreateBitmapFromFile(HANDLE ctx, LPCWSTR filepath);
	D2DLIB_API HANDLE CreateBitmapFromMemory(HANDLE ctx, UINT width, UINT height, UINT stride, BYTE* buffer, UINT offset, UINT length);

	D2DLIB_API void DrawD2DBitmap(HANDLE ctx, HANDLE d2dbitmap, D2D1_RECT_F* destRect = NULL, D2D1_RECT_F* srcRect = NULL, FLOAT opacity = 1,
		D2D1_BITMAP_INTERPOLATION_MODE interpolationMode = D2D1_BITMAP_INTERPOLATION_MODE_LINEAR);

	D2DLIB_API void DrawGDIBitmap(HANDLE context, HBITMAP bitmap, FLOAT opacity = 1, BOOL alpha = FALSE,
		D2D1_BITMAP_INTERPOLATION_MODE = D2D1_BITMAP_INTERPOLATION_MODE_LINEAR);

	D2DLIB_API void DrawGDIBitmapRect(HANDLE context, HBITMAP bitmap,
		D2D1_RECT_F* destRect = NULL, D2D1_RECT_F* srcRect = NULL, FLOAT opacity = 1, 
		BOOL alpha = FALSE, D2D1_BITMAP_INTERPOLATION_MODE = D2D1_BITMAP_INTERPOLATION_MODE_LINEAR);

	D2DLIB_API D2D1_SIZE_F GetBitmapSize(HANDLE d2dbitmap);

	D2DLIB_API HBITMAP CopyD2DBitmapToHBitmap(HANDLE d2dbitmap);
}