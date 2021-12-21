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
	D2DLIB_API void DrawString(HANDLE handle, LPCWSTR text, D2D1_COLOR_F color,
														 LPCWSTR fontName, FLOAT fontSize, D2D1_RECT_F* rect,
														 DWRITE_TEXT_ALIGNMENT halign = DWRITE_TEXT_ALIGNMENT::DWRITE_TEXT_ALIGNMENT_LEADING,
														 DWRITE_PARAGRAPH_ALIGNMENT valign = DWRITE_PARAGRAPH_ALIGNMENT::DWRITE_PARAGRAPH_ALIGNMENT_NEAR);

	D2DLIB_API HANDLE CreateTextLayout(HANDLE ctx, LPCWSTR text, LPCWSTR fontName, FLOAT fontSize, D2D1_SIZE_F* size);

	D2DLIB_API void MeasureText(HANDLE ctx, LPCWSTR text, LPCWSTR fontName, FLOAT fontSize, D2D1_SIZE_F* size);

	D2DLIB_API void DrawGlyphRun(HANDLE ctx, D2D1_POINT_2F baselineOrigin, 
			const DWRITE_GLYPH_RUN *glyphRun, D2D1_COLOR_F color,
			DWRITE_MEASURING_MODE measuringMode = DWRITE_MEASURING_MODE_NATURAL);
}
