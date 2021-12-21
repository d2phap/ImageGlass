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
	D2DLIB_API void PushTransform(HANDLE context);
	D2DLIB_API void PopTransform(HANDLE context);
	D2DLIB_API void TranslateTransform(HANDLE context, FLOAT x, FLOAT y);
	D2DLIB_API void ScaleTransform(HANDLE context, FLOAT scaleX, FLOAT scaleY, D2D1_POINT_2F center = D2D1::Point2F());
	D2DLIB_API void RotateTransform(HANDLE context, FLOAT angle, D2D_POINT_2F point = D2D1::Point2F());
	D2DLIB_API void SkewTransform(HANDLE ctx, FLOAT angleX, FLOAT angleY, D2D1_POINT_2F center = D2D1::Point2F());
	D2DLIB_API void SetTransform(HANDLE context, D2D1_MATRIX_3X2_F* transform);
	D2DLIB_API void GetTransform(HANDLE context, D2D1_MATRIX_3X2_F* transform);
	D2DLIB_API void ResetTransform(HANDLE context);
}
