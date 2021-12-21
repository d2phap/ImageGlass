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
#include "Bitmap.h"

HANDLE CreateBitmapFromFile(HANDLE ctx,
  PCWSTR uri, UINT destinationWidth, UINT destinationHeight, ID2D1Bitmap **ppBitmap)
{
	D2DContext* context = reinterpret_cast<D2DContext*>(ctx);
	
	IWICBitmapDecoder *pDecoder = NULL;
	IWICBitmapFrameDecode *pSource = NULL;
	IWICStream *pStream = NULL;
	IWICFormatConverter *pConverter = NULL;
	//IWICBitmapScaler *pScaler = NULL;

	HRESULT hr = context->imageFactory->CreateDecoderFromFilename(
		uri, NULL, GENERIC_READ, WICDecodeMetadataCacheOnLoad, &pDecoder);
	
	if (SUCCEEDED(hr))
  {
    hr = pDecoder->GetFrame(0, &pSource);
  }

	if (SUCCEEDED(hr))
  {
    // Convert the image format to 32bppPBGRA
    // (DXGI_FORMAT_B8G8R8A8_UNORM + D2D1_ALPHA_MODE_PREMULTIPLIED).
    hr = context->imageFactory->CreateFormatConverter(&pConverter);
	}

	if (SUCCEEDED(hr))
  {
		hr = pConverter->Initialize(pSource, GUID_WICPixelFormat32bppPBGRA, WICBitmapDitherTypeNone, 
			NULL, 0.f, WICBitmapPaletteTypeMedianCut);
	}

	if (SUCCEEDED(hr))
  {
		hr = context->renderTarget->CreateBitmapFromWicBitmap(pConverter, NULL, ppBitmap);
	}

	SafeRelease(&pDecoder);
  SafeRelease(&pSource);
  SafeRelease(&pStream);
  SafeRelease(&pConverter);
  //SafeRelease(&pScaler);

	return (HANDLE)0;
}

HANDLE CreateBitmapFromHBitmap(HANDLE ctx, HBITMAP hBitmap, BOOL alpha)
{
	RetrieveContext(ctx);
	
	IWICBitmap* wicBitmap = NULL;
	HRESULT hr = context->imageFactory->CreateBitmapFromHBITMAP(hBitmap, NULL, 
		alpha ? WICBitmapAlphaChannelOption::WICBitmapUsePremultipliedAlpha
		: WICBitmapAlphaChannelOption::WICBitmapIgnoreAlpha, &wicBitmap);

	if (!SUCCEEDED(hr)) {
		context->lastErrorCode = hr;
		return NULL;
	}

	IWICFormatConverter *wicConverter = NULL;
	ID2D1Bitmap* d2dBitmap = NULL;
	IWICBitmapSource* wicBitmapSource = NULL;

	if (alpha) {
		hr = context->imageFactory->CreateFormatConverter(&wicConverter);

		if (!SUCCEEDED(hr)) {
			context->lastErrorCode = hr;
			return NULL;
		}

		hr = wicConverter->Initialize(wicBitmap, GUID_WICPixelFormat32bppPRGBA, WICBitmapDitherTypeNone,
			NULL, 0.f, WICBitmapPaletteTypeMedianCut);

		if (!SUCCEEDED(hr)) {
			SafeRelease(&wicBitmap);
			context->lastErrorCode = hr;
			return NULL;
		}

		wicBitmapSource = wicConverter;
	}
	else {
		wicBitmapSource = wicBitmap;
	}

	hr = context->renderTarget->CreateBitmapFromWicBitmap(wicBitmapSource, NULL, &d2dBitmap);

	SafeRelease(&wicBitmap);
	SafeRelease(&wicConverter);

	if (!SUCCEEDED(hr)) {
		context->lastErrorCode = hr;
		return NULL;
	}

	return (HANDLE)d2dBitmap;
}

HANDLE CreateBitmapFromMemory(HANDLE ctx, UINT width, UINT height, UINT stride, BYTE* buffer, UINT offset, UINT length)
{
	RetrieveContext(ctx);

	IWICBitmap* wicBitmap = NULL;

	HRESULT hr = context->imageFactory->CreateBitmapFromMemory(width, height, 
		GUID_WICPixelFormat32bppPBGRA, stride, length, buffer, &wicBitmap);

	ID2D1Bitmap* d2dBitmap = NULL;
	hr = context->renderTarget->CreateBitmapFromWicBitmap(wicBitmap, NULL, &d2dBitmap);

	SafeRelease(&wicBitmap);

	return (HANDLE)d2dBitmap;
}

HANDLE CreateBitmapFromBytes(HANDLE ctx, BYTE* buffer, UINT offset, UINT length)
{
	RetrieveContext(ctx);

	IWICImagingFactory* imageFactory = context->imageFactory;

	IWICBitmapDecoder *decoder = NULL;
	IWICBitmapFrameDecode *source = NULL;
	IWICStream *stream = NULL;
	IWICFormatConverter *converter = NULL;
	IWICBitmapScaler *scaler = NULL;

	ID2D1Bitmap* bitmap = NULL;

	HRESULT hr = S_OK;

	if (SUCCEEDED(hr))
	{
		// Create a WIC stream to map onto the memory.
		hr = imageFactory->CreateStream(&stream);
	}

	if (SUCCEEDED(hr))
	{
		// Initialize the stream with the memory pointer and size.
		hr = stream->InitializeFromMemory(reinterpret_cast<BYTE*>(buffer + offset), length);
	}

	if (SUCCEEDED(hr))
	{
		// Create a decoder for the stream.
		hr = imageFactory->CreateDecoderFromStream(stream, NULL, WICDecodeMetadataCacheOnLoad, &decoder);
	}

	if (SUCCEEDED(hr))
	{
		// Create the initial frame.
		hr = decoder->GetFrame(0, &source);
	}
	
	if (SUCCEEDED(hr))
	{
		// Convert the image format to 32bppPBGRA
		// (DXGI_FORMAT_B8G8R8A8_UNORM + D2D1_ALPHA_MODE_PREMULTIPLIED).
		hr = imageFactory->CreateFormatConverter(&converter);
	}

	if (SUCCEEDED(hr))
  {
		hr = converter->Initialize(source, GUID_WICPixelFormat32bppPBGRA, 
			WICBitmapDitherTypeNone, NULL, 0.f, WICBitmapPaletteTypeMedianCut);
	}

	if (SUCCEEDED(hr))
	{
		//create a Direct2D bitmap from the WIC bitmap.
		hr = context->renderTarget->CreateBitmapFromWicBitmap(converter, NULL, &bitmap);
	}

	SafeRelease(&decoder);
	SafeRelease(&source);
	SafeRelease(&stream);
	SafeRelease(&converter);
	SafeRelease(&scaler);

	return (HANDLE)bitmap;
}

HANDLE CreateBitmapFromFile(HANDLE ctx, LPCWSTR filepath) 
{
	RetrieveContext(ctx);

	IWICImagingFactory* imageFactory = context->imageFactory;

	IWICBitmapDecoder *decoder = NULL;
	IWICBitmapFrameDecode *source = NULL;
	IWICStream *stream = NULL;
	IWICFormatConverter *converter = NULL;
	IWICBitmapScaler *scaler = NULL;

	ID2D1Bitmap* bitmap = NULL;

	HRESULT hr = S_OK;

	if (SUCCEEDED(hr))
	{
		hr = imageFactory->CreateStream(&stream);
	}

	if (SUCCEEDED(hr))
	{
		hr = stream->InitializeFromFilename(filepath, GENERIC_READ);
	}

	if (SUCCEEDED(hr))
	{
		hr = imageFactory->CreateDecoderFromStream(stream, NULL, WICDecodeMetadataCacheOnLoad, &decoder);
	}

	if (SUCCEEDED(hr))
	{
		hr = decoder->GetFrame(0, &source);
	}

	if (SUCCEEDED(hr))
	{
		hr = imageFactory->CreateFormatConverter(&converter);
	}

	if (SUCCEEDED(hr))
  {
		hr = converter->Initialize(source, GUID_WICPixelFormat32bppPBGRA, 
			WICBitmapDitherTypeNone, NULL, 0.f, WICBitmapPaletteTypeMedianCut);
	}

	if (SUCCEEDED(hr))
	{
		hr = context->renderTarget->CreateBitmapFromWicBitmap(converter, NULL, &bitmap);
	}

	SafeRelease(&decoder);
	SafeRelease(&source);
	SafeRelease(&stream);
	SafeRelease(&converter);
	SafeRelease(&scaler);

	return (HANDLE)bitmap;
}

void DrawGDIBitmap(HANDLE hContext, HBITMAP hBitmap, FLOAT opacity, BOOL alpha,
									 D2D1_BITMAP_INTERPOLATION_MODE interpolationMode)
{
	DrawGDIBitmapRect(hContext, hBitmap, NULL, NULL, opacity, alpha, interpolationMode);
}

void DrawGDIBitmapRect(HANDLE hContext, HBITMAP hBitmap, D2D1_RECT_F* rect, 
											 D2D1_RECT_F* sourceRectangle, FLOAT opacity, BOOL alpha,
											 D2D1_BITMAP_INTERPOLATION_MODE interpolationMode)
{
	D2DContext* context = reinterpret_cast<D2DContext*>(hContext);

	IWICBitmap* wicBmp = NULL;

	context->imageFactory->CreateBitmapFromHBITMAP(hBitmap, 0, 
		alpha ? WICBitmapAlphaChannelOption::WICBitmapUseAlpha
		: WICBitmapAlphaChannelOption::WICBitmapIgnoreAlpha, &wicBmp);

	ID2D1Bitmap* d2dBitmap = NULL;

	HRESULT hr = context->renderTarget->CreateBitmapFromWicBitmap(wicBmp, NULL, &d2dBitmap);
	
	if (!SUCCEEDED(hr) || d2dBitmap == NULL && alpha)
	{
		// if convert is failed, try create d2d bitmap from 32bppPBGRA format
		IWICFormatConverter* converter = 0;
		context->imageFactory->CreateFormatConverter(&converter);

		if (converter != NULL)
		{
			converter->Initialize(wicBmp, GUID_WICPixelFormat32bppPBGRA, WICBitmapDitherTypeNone, NULL, 0.f, WICBitmapPaletteTypeMedianCut);
			hr = context->renderTarget->CreateBitmapFromWicBitmap(converter, 0, &d2dBitmap);
			converter->Release();
		}
	}

	if (SUCCEEDED(hr) && d2dBitmap != NULL) {
		context->renderTarget->DrawBitmap(d2dBitmap, rect, opacity, interpolationMode, sourceRectangle);
	}

	SafeRelease(&d2dBitmap);
	SafeRelease(&wicBmp);
}

void DrawD2DBitmap(HANDLE ctx, HANDLE d2dbitmap, D2D1_RECT_F* destRect, D2D1_RECT_F* srcRect,
	FLOAT opacity, D2D1_BITMAP_INTERPOLATION_MODE interpolationMode)
{
	RetrieveContext(ctx);
	ID2D1Bitmap* bitmap = reinterpret_cast<ID2D1Bitmap*>(d2dbitmap);

	context->renderTarget->DrawBitmap(bitmap, destRect, opacity, interpolationMode, srcRect);
}

D2D1_SIZE_F GetBitmapSize(HANDLE d2dbitmap)
{
	ID2D1Bitmap* bitmap = reinterpret_cast<ID2D1Bitmap*>(d2dbitmap);
	return bitmap->GetSize();
}

#ifdef __unused__
void _unused_get_wic_info() {

	TCHAR msg[256];

	UINT w, h;
	wicBitmap->GetSize(&w, &h);
	_swprintf_c(msg, 256, L"width: %d, height: %d", w, h);
	MessageBox(context->hwnd, msg, NULL, MB_OK);

	WICPixelFormatGUID pf;
	wicBitmap->GetPixelFormat(&pf);
	_swprintf_c(msg, 256, L"%X-%X-%X-%X-%X-%X-%X-%X-%X-%X-%X", pf.Data1, pf.Data2, pf.Data3,
		pf.Data4[0], pf.Data4[1], pf.Data4[2], pf.Data4[3], pf.Data4[4], pf.Data4[5], pf.Data4[6], pf.Data4[7]);
	MessageBox(context->hwnd, msg, NULL, MB_OK);
}
#endif /* __unused */