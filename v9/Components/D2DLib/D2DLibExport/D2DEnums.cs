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
using System.Text;

namespace unvell.D2DLib
{
	enum D2DDebugLevel
	{
		None = 0,
		Error = 1,
		Warning = 2,
		Information = 3,
	}

	enum D2DFactoryType 
	{
		//
		// The resulting factory and derived resources may only be invoked serially.
		// Reference counts on resources are interlocked, however, resource and render
		// target state is not protected from multi-threaded access.
		//
		SingleThreaded = 0,

		//
		// The resulting factory may be invoked from multiple threads. Returned resources
		// use interlocked reference counting and their state is protected.
		//
		MultiThreaded = 1,
	}

	enum D2DRenderTargetType
	{
		//
		// D2D is free to choose the render target type for the caller.
		//
		Default = 0,

		//
		// The render target will render using the CPU.
		//
		Software = 1,

		//
		// The render target will render using the GPU.
		//
		Hardware = 2,
	}

	enum D2DRenderTargetUsage
	{
		None = 0x00000000,

		//
		// Rendering will occur locally, if a terminal-services session is established, the
		// bitmap updates will be sent to the terminal services client.
		//
		ForceBitmapRemoting = 0x00000001,

		//
		// The render target will allow a call to GetDC on the ID2D1GdiInteropRenderTarget
		// interface. Rendering will also occur locally.
		//
		GDICompatible = 0x00000002,
	}

	enum D2DFeatureLevel
	{
		//
		// The caller does not require a particular underlying D3D device level.
		//
		Default = 0,

		//
		// The D3D device level is DX9 compatible.
		//
		Level9 = D3DFeatureLevel.Level9_1,

		//
		// The D3D device level is DX10 compatible.
		//
		Level10 = D3DFeatureLevel.Level10_0,
	}

	enum D3DFeatureLevel
	{
		Level9_1 = 0x9100,
		Level9_2 = 0x9200,
		Level9_3 = 0x9300,
		Level10_0 = 0xa000,
		Level10_1 = 0xa100,
		Level11_0 = 0xb000,
		Level11_1 = 0xb100
	}

	enum DXGIFormat
	{
		Unknown = 0,
		R32G32B32A32_TYPELESS = 1,
		R32G32B32A32_FLOAT = 2,
		R32G32B32A32_UINT = 3,
		R32G32B32A32_SINT = 4,
		R32G32B32_TYPELESS = 5,
		R32G32B32_FLOAT = 6,
		R32G32B32_UINT = 7,
		R32G32B32_SINT = 8,
		R16G16B16A16_TYPELESS = 9,
		R16G16B16A16_FLOAT = 10,
		R16G16B16A16_UNORM = 11,
		R16G16B16A16_UINT = 12,
		R16G16B16A16_SNORM = 13,
		R16G16B16A16_SINT = 14,
		R32G32_TYPELESS = 15,
		R32G32_FLOAT = 16,
		R32G32_UINT = 17,
		R32G32_SINT = 18,
		R32G8X24_TYPELESS = 19,
		D32_FLOAT_S8X24_UINT = 20,
		R32_FLOAT_X8X24_TYPELESS = 21,
		X32_TYPELESS_G8X24_UINT = 22,
		R10G10B10A2_TYPELESS = 23,
		R10G10B10A2_UNORM = 24,
		R10G10B10A2_UINT = 25,
		R11G11B10_FLOAT = 26,
		R8G8B8A8_TYPELESS = 27,
		R8G8B8A8_UNORM = 28,
		R8G8B8A8_UNORM_SRGB = 29,
		R8G8B8A8_UINT = 30,
		R8G8B8A8_SNORM = 31,
		R8G8B8A8_SINT = 32,
		R16G16_TYPELESS = 33,
		R16G16_FLOAT = 34,
		R16G16_UNORM = 35,
		R16G16_UINT = 36,
		R16G16_SNORM = 37,
		R16G16_SINT = 38,
		R32_TYPELESS = 39,
		D32_FLOAT = 40,
		R32_FLOAT = 41,
		R32_UINT = 42,
		R32_SINT = 43,
		R24G8_TYPELESS = 44,
		D24_UNORM_S8_UINT = 45,
		R24_UNORM_X8_TYPELESS = 46,
		X24_TYPELESS_G8_UINT = 47,
		R8G8_TYPELESS = 48,
		R8G8_UNORM = 49,
		R8G8_UINT = 50,
		R8G8_SNORM = 51,
		R8G8_SINT = 52,
		R16_TYPELESS = 53,
		R16_FLOAT = 54,
		D16_UNORM = 55,
		R16_UNORM = 56,
		R16_UINT = 57,
		R16_SNORM = 58,
		R16_SINT = 59,
		R8_TYPELESS = 60,
		R8_UNORM = 61,
		R8_UINT = 62,
		R8_SNORM = 63,
		R8_SINT = 64,
		A8_UNORM = 65,
		R1_UNORM = 66,
		R9G9B9E5_SHAREDEXP = 67,
		R8G8_B8G8_UNORM = 68,
		G8R8_G8B8_UNORM = 69,
		BC1_TYPELESS = 70,
		BC1_UNORM = 71,
		BC1_UNORM_SRGB = 72,
		BC2_TYPELESS = 73,
		BC2_UNORM = 74,
		BC2_UNORM_SRGB = 75,
		BC3_TYPELESS = 76,
		BC3_UNORM = 77,
		BC3_UNORM_SRGB = 78,
		BC4_TYPELESS = 79,
		BC4_UNORM = 80,
		BC4_SNORM = 81,
		BC5_TYPELESS = 82,
		BC5_UNORM = 83,
		BC5_SNORM = 84,
		B5G6R5_UNORM = 85,
		B5G5R5A1_UNORM = 86,
		B8G8R8A8_UNORM = 87,
		B8G8R8X8_UNORM = 88,
		R10G10B10_XR_BIAS_A2_UNORM = 89,
		B8G8R8A8_TYPELESS = 90,
		B8G8R8A8_UNORM_SRGB = 91,
		B8G8R8X8_TYPELESS = 92,
		B8G8R8X8_UNORM_SRGB = 93,
		BC6H_TYPELESS = 94,
		BC6H_UF16 = 95,
		BC6H_SF16 = 96,
		BC7_TYPELESS = 97,
		BC7_UNORM = 98,
		BC7_UNORM_SRGB = 99,
		AYUV = 100,
		Y410 = 101,
		Y416 = 102,
		NV12 = 103,
		P010 = 104,
		P016 = 105,
		OPAQUE_420 = 106,
		YUY2 = 107,
		Y210 = 108,
		Y216 = 109,
		NV11 = 110,
		AI44 = 111,
		IA44 = 112,
		P8 = 113,
		A8P8 = 114,
		B4G4R4A4_UNORM = 115,
	}

	enum D2D1AlphaMode
	{
		//
		// Alpha mode should be determined implicitly. Some target surfaces do not supply
		// or imply this information in which case alpha must be specified.
		//
		Unknown = 0,

		//
		// Treat the alpha as premultipled.
		//
		Premultiplied = 1,

		//
		// Opacity is in the 'A' component only.
		//
		Straight = 2,

		//
		// Ignore any alpha channel information.
		//
		Ignore = 3,
	}

	enum D2D1PresentOptions
	{
		None = 0x00000000,

		//
		// Keep the target contents intact through present.
		//
		RetainContents = 0x00000001,

		//
		// Do not wait for display refresh to commit changes to display.
		//
		Immediately = 0x00000002,
	}

	public enum D2DDashStyle
	{
		Solid = 0,
		Dash = 1,
		Dot = 2,
		DashDot = 3,
		DashDotDot = 4,
		Custom = 5,
	}

	/// <summary>
	/// From <c>D2D1_CAP_STYLE</c>.
	/// </summary>
	public enum D2DCapStyle
	{
		/// <summary>
		/// Flat line cap.
		/// </summary>
		Flat = 0,
		
		/// <summary>
		/// Square line cap.
		/// </summary>
		Square = 1,
		
		/// <summary>
		/// Round line cap.
		/// </summary>
		Round = 2,
		
		/// <summary>
		/// Triangle line cap.
		/// </summary>
		Triangle = 3
	}

	public enum D2DArcSize
	{
		Small = 0,
		Large = 1,
	}

	public enum D2DSweepDirection
	{
		CounterClockwise = 0,
		Clockwise = 1,
	}

	public enum D2DAntialiasMode
	{
		//
		// The edges of each primitive are antialiased sequentially.
		//
		PerPrimitive = 0,

		//
		// Each pixel is rendered if its pixel center is contained by the geometry.
		//
		Aliased = 1,
	}

	public enum D2DBitmapInterpolationMode
	{
		//
		// Nearest Neighbor filtering. Also known as nearest pixel or nearest point
		// sampling.
		//
		NearestNeighbor = 0,

		//
		// Linear filtering.
		//
		Linear = 1,
	}

	enum DWriteMeasuringMode
	{
		/// <summary>
		/// Text is measured using glyph ideal metrics whose values are independent to the current display resolution.
		/// </summary>
		Natural = 0,

		/// <summary>
		/// Text is measured using glyph display compatible metrics whose values tuned for the current display resolution.
		/// </summary>
		GDIClassic = 1,

		/// <summary>
		/// Text is measured using the same glyph display metrics as text measured by GDI using a font
		/// created with CLEARTYPE_NATURAL_QUALITY.
		/// </summary>
		GDINatural = 2
	}

	/// <summary>
	/// Alignment of paragraph text along the reading direction axis relative to 
	/// the leading and trailing edge of the layout box.
	/// </summary>
	public enum DWriteTextAlignment
	{
		/// <summary>
		/// The leading edge of the paragraph text is aligned to the layout box's leading edge.
		/// </summary>
		Leading,

		/// <summary>
		/// The trailing edge of the paragraph text is aligned to the layout box's trailing edge.
		/// </summary>
		Trailing,

		/// <summary>
		/// The center of the paragraph text is aligned to the center of the layout box.
		/// </summary>
		Center
	};

	/// <summary>
	/// Alignment of paragraph text along the flow direction axis relative to the
	/// flow's beginning and ending edge of the layout box.
	/// </summary>
	public enum DWriteParagraphAlignment
	{
		/// <summary>
		/// The first line of paragraph is aligned to the flow's beginning edge of the layout box.
		/// </summary>
		Near,

		/// <summary>
		/// The last line of paragraph is aligned to the flow's ending edge of the layout box.
		/// </summary>
		Far,

		/// <summary>
		/// The center of the paragraph is aligned to the center of the flow of the layout box.
		/// </summary>
		Center
	};

	public enum LayerOptions
	{
		None = 0x00000000,

		/// <summary>
		/// The layer will render correctly for ClearType text. If the render target was set
		/// to ClearType previously, the layer will continue to render ClearType. If the
		/// render target was set to ClearType and this option is not specified, the render
		/// target will be set to render gray-scale until the layer is popped. The caller
		/// can override this default by calling SetTextAntialiasMode while within the
		/// layer. This flag is slightly slower than the default.
		/// </summary>
		InitializeForClearType = 0x00000001,
	}
}

