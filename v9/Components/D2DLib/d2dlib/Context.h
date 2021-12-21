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

#include <Windows.h>
#include <d2d1.h>
#include <dwrite.h>
#include "Wincodec.h"

#include <stack>

using namespace std;

#ifndef __D2DLIB_H__
#define __D2DLIB_H__

typedef struct D2DContext
{
	ID2D1Factory* factory;
	IDWriteFactory* writeFactory;
	IWICImagingFactory* imageFactory;
	
	union
	{
		struct // HwndRenderTarget
		{
			ID2D1HwndRenderTarget* renderTarget;
			HWND hwnd;
		};

		struct // BitmapRenderTarget
		{
			ID2D1BitmapRenderTarget *bitmapRenderTarget;
			ID2D1Bitmap* bitmap;
		};
	};

	std::stack<D2D1_MATRIX_3X2_F>* matrixStack;
	//std::stack<ID2D1Layer*> layerStack;

	HRESULT lastErrorCode;
	
} D2DContext;

enum GeometryType {
	GeoType_RectangleGeometry,
	GeoType_PathGeometry,
};

typedef struct D2DGeometryContext {
	D2DContext* d2context;
	ID2D1Geometry* geometry;
} D2DGeometryContext;

typedef struct D2DPathContext : D2DGeometryContext
{
	ID2D1PathGeometry* path;
	ID2D1GeometrySink* sink;
	bool isOpen;
	bool isClosed;
} D2DPathContext;

//typedef struct D2DBitmapRenderTargetContext
//{
//	ID2D1Factory *factory;
//	IDWriteFactory *writeFactory;
//	IWICImagingFactory* imageFactory;
//	HRESULT lastResult;
//
//} D2DBitmapRenderTargetContext;

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the D2DLIB_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// D2DLIB_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef D2DLIB_EXPORTS
#define D2DLIB_API __declspec(dllexport)
#else
#define D2DLIB_API __declspec(dllimport)
#endif

template<class Interface>
inline void SafeRelease(Interface **ppInterfaceToRelease)
{
  if (*ppInterfaceToRelease != NULL)
  {
    (*ppInterfaceToRelease)->Release();
    (*ppInterfaceToRelease) = NULL;
  }
}

#define RetrieveContext(ctx) D2DContext* context = reinterpret_cast<D2DContext*>(ctx)
#define RetrieveD2DBitmap(ctx) ID2D1Bitmap* d2dbitmap = reinterpret_cast<ID2D1Bitmap*>(d2dbitmapHandle)
//#define RetrieveContext(ctx) D2DContext* context = (D2DContext*)(ctx)

//extern D2DLIB_API HRESULT LastResultCode;

extern "C" 
{
	D2DLIB_API HANDLE CreateContext(HWND hwnd);
	D2DLIB_API void DestroyContext(HANDLE context);
	D2DLIB_API void ResizeContext(HANDLE context);
	
	D2DLIB_API void SetContextProperties(HANDLE ctx,
		D2D1_ANTIALIAS_MODE antialiasMode = D2D1_ANTIALIAS_MODE::D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
	
	D2DLIB_API void BeginRender(HANDLE context);
	D2DLIB_API void BeginRenderWithBackgroundColor(HANDLE ctx, D2D1_COLOR_F color);
	D2DLIB_API void BeginRenderWithBackgroundBitmap(HANDLE ctx, HANDLE bitmap);
	D2DLIB_API void EndRender(HANDLE context);
	D2DLIB_API void Flush(HANDLE context);
	D2DLIB_API void Clear(HANDLE context, D2D1_COLOR_F color);

	D2DLIB_API HANDLE CreateBitmapRenderTarget(HANDLE context, D2D1_SIZE_F size = D2D1::SizeF());
	D2DLIB_API void DrawBitmapRenderTarget(HANDLE context, HANDLE bitmapRenderTargetHandle, D2D1_RECT_F* rect = NULL,
		FLOAT opacity = 1, D2D1_BITMAP_INTERPOLATION_MODE interpolationMode = D2D1_BITMAP_INTERPOLATION_MODE_LINEAR);
	D2DLIB_API HANDLE GetBitmapRenderTargetBitmap(HANDLE bitmapRenderTargetHandle);
	D2DLIB_API void DestroyBitmapRenderTarget(HANDLE context);

	D2DLIB_API void GetDPI(HANDLE ctx, FLOAT* dpiX, FLOAT* dpiY);
	D2DLIB_API void SetDPI(HANDLE ctx, FLOAT dpiX, FLOAT dpiY);

	D2DLIB_API void PushClip(HANDLE context, D2D1_RECT_F* rect, 
												   D2D1_ANTIALIAS_MODE antiAliasMode = D2D1_ANTIALIAS_MODE::D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
	D2DLIB_API void PopClip(HANDLE context);

	D2DLIB_API HANDLE CreateLayer(HANDLE context);
	D2DLIB_API void PushLayer(HANDLE ctx, HANDLE layerHandle, D2D1_RECT_F& contentBounds = D2D1::InfiniteRect(),
		__in_opt HANDLE geometryHandle = NULL, __in_opt ID2D1Brush *opacityBrush = NULL, 
		D2D1_LAYER_OPTIONS layerOptions = D2D1_LAYER_OPTIONS_NONE);
	D2DLIB_API void PopLayer(HANDLE ctx);

	D2DLIB_API HRESULT GetLastErrorCode(HANDLE ctx);

	D2DLIB_API void ReleaseObject(HANDLE handle);

	D2DLIB_API void TestDraw(HANDLE context);
}

#endif /* __D2DLIB_H__ */