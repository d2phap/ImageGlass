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
#include "Context.h"
#include "Simple.h"

void Test(HANDLE handle)
{
	D2DContext* context = reinterpret_cast<D2DContext*>(handle);

	if (context->renderTarget == NULL)
	{
		RECT rc;
		GetClientRect(context->hwnd, &rc);

		D2D1_SIZE_U size = D2D1::SizeU(rc.right - rc.left, rc.bottom - rc.top);

		context->factory->CreateHwndRenderTarget(
			D2D1::RenderTargetProperties(),
			D2D1::HwndRenderTargetProperties(context->hwnd, size),
			&context->renderTarget);

		//context->renderTarget->SetAntialiasMode(D2D1_ANTIALIAS_MODE::D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
	}

	IDWriteTextFormat* textFormat = NULL;
	context->writeFactory->CreateTextFormat(
		TEXT("NSimSun"), 
		NULL,
		DWRITE_FONT_WEIGHT_NORMAL,
		DWRITE_FONT_STYLE_NORMAL,
		DWRITE_FONT_STRETCH_NORMAL,
		14,
		L"", //locale
		&textFormat);

	ID2D1RenderTarget* rt = context->renderTarget;

	D2D1_COLOR_F color = D2D1::ColorF(D2D1::ColorF::Blue);

	D2D1_RECT_F rect = { 20, 100, 200, 140 };

	// Create a black brush.
	ID2D1SolidColorBrush* brush = NULL;
	rt->CreateSolidColorBrush(color, &brush);

	const TCHAR* str = TEXT("汉字测试");

	rt->BeginDraw();
	rt->Clear(D2D1::ColorF(D2D1::ColorF::White));

	rt->DrawText(str, wcslen(str), textFormat, rect, brush);

	rt->EndDraw();

	SafeRelease(&brush);
	SafeRelease(&textFormat);
}

void TestDraw1(HANDLE ctx)
{
	RetrieveContext(ctx);

	ID2D1RenderTarget* render = context->renderTarget;
	
	ID2D1SolidColorBrush* brush;
	render->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Silver), &brush);

	RECT rect;
	GetClientRect(context->hwnd, &rect);

	D2D1_ELLIPSE ellipse;
	ellipse.radiusX = 10;
	ellipse.radiusY = 10;

	for (float x = 0; x < 1920; x += 20)
	{
		for (float y = 0; y < 1080; y += 20)
		{
			ellipse.point.x = x;
			ellipse.point.y = y;

			render->DrawEllipse(&ellipse, brush);
		}
	}

	SafeRelease(&brush);
}

void TestDraw(HANDLE ctx)
{
	RetrieveContext(ctx);

	ID2D1BitmapRenderTarget* renderTarget = NULL;
	context->renderTarget->CreateCompatibleRenderTarget(&renderTarget);
	ID2D1Bitmap* bitmap = NULL;
	renderTarget->GetBitmap(&bitmap);

	D2D1_ELLIPSE ellipse = D2D1::Ellipse(D2D1::Point2F(100, 100), 50, 50);

	ID2D1SolidColorBrush* brush = NULL;
	context->renderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Green), &brush);

	renderTarget->BeginDraw();
	renderTarget->Clear(D2D1::ColorF(D2D1::ColorF::Yellow));
	renderTarget->DrawEllipse(&ellipse, brush);
	renderTarget->EndDraw();

	context->renderTarget->DrawBitmap(bitmap);

	D2D1_RECT_F rect = D2D1::RectF(10, 10, 20, 20);
	context->renderTarget->DrawRectangle(&rect, brush);


	D2D1_RECT_F rect2 = D2D1::RectF(100, 100, 150, 150);
	context->renderTarget->DrawBitmap(bitmap, &rect2);

	SafeRelease(&bitmap);
	SafeRelease(&renderTarget);
	SafeRelease(&brush);
}