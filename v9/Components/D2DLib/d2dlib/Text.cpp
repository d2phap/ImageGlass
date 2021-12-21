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

#include "stdafx.h"
#include "Text.h"

D2DLIB_API void DrawString(HANDLE ctx, LPCWSTR text, D2D1_COLOR_F color,
													 LPCWSTR fontName, FLOAT fontSize, D2D1_RECT_F* rect,
													 DWRITE_TEXT_ALIGNMENT halign, DWRITE_PARAGRAPH_ALIGNMENT valign)
{
	RetrieveContext(ctx);

	ID2D1SolidColorBrush* brush = NULL;
	IDWriteTextFormat* textFormat = NULL;

	HRESULT hr = context->writeFactory->CreateTextFormat(fontName,
		NULL,
		DWRITE_FONT_WEIGHT_NORMAL, DWRITE_FONT_STYLE_NORMAL, DWRITE_FONT_STRETCH_NORMAL,
		fontSize,
		L"", //locale
		&textFormat);

	if (SUCCEEDED(hr) && textFormat != NULL)
	{
		textFormat->SetTextAlignment(halign);
		textFormat->SetParagraphAlignment(valign);

		// Create a solid color brush.
		hr = (context->renderTarget)->CreateSolidColorBrush(color, &brush);

		if (SUCCEEDED(hr) && brush != NULL) {
			context->renderTarget->DrawText(text, wcslen(text), textFormat, rect, brush);
		}
	}

	SafeRelease(&brush);
	SafeRelease(&textFormat);
}

D2DLIB_API HANDLE CreateTextLayout(HANDLE ctx, LPCWSTR text, LPCWSTR fontName, FLOAT fontSize, D2D1_SIZE_F* size) {
	RetrieveContext(ctx);

	IDWriteTextFormat* textFormat = NULL;

	HRESULT hr = context->writeFactory->CreateTextFormat(fontName,
		NULL,
		DWRITE_FONT_WEIGHT_NORMAL, DWRITE_FONT_STYLE_NORMAL, DWRITE_FONT_STRETCH_NORMAL,
		fontSize,
		L"", //locale
		&textFormat);

	if (SUCCEEDED(hr) && textFormat != NULL)
	{
		IDWriteTextLayout* textLayout;

		hr = context->writeFactory->CreateTextLayout(
			text,      // The string to be laid out and formatted.
			wcslen(text),  // The length of the string.
			textFormat,  // The text format to apply to the string (contains font information, etc).
			size->width,         // The width of the layout box.
			size->height,        // The height of the layout box.
			&textLayout  // The IDWriteTextLayout interface pointer.
		);

		if (SUCCEEDED(hr) && textLayout != NULL) {
			return (HANDLE)textLayout;
		}
	}

	SafeRelease(&textFormat);

	return NULL;
}

D2DLIB_API void MeasureText(HANDLE ctx, LPCWSTR text, LPCWSTR fontName, FLOAT fontSize, D2D1_SIZE_F* size) {
	RetrieveContext(ctx);

	IDWriteTextLayout* textLayout = (IDWriteTextLayout*)CreateTextLayout(ctx, text, fontName, fontSize, size);

	if (textLayout != NULL) {
		DWRITE_TEXT_METRICS tm;
		textLayout->GetMetrics(&tm);

		size->width = tm.width;
		size->height = tm.height;
	}

	SafeRelease(&textLayout);
}

void DrawGlyphRun(HANDLE ctx, D2D1_POINT_2F baselineOrigin, 
			const DWRITE_GLYPH_RUN *glyphRun, D2D1_COLOR_F color,
			DWRITE_MEASURING_MODE measuringMode)
{
	D2DContext* context = reinterpret_cast<D2DContext*>(ctx);

}

//void DrawTextLayout(HANDLE ctx, D2D1_POINT_2F origin,
//  [in]  IDWriteTextLayout *textLayout,
//  [in]  ID2D1Brush *defaultForegroundBrush,
//  D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS_NONE
//) 
//{
//}