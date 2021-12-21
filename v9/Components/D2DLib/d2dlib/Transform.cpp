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
#include "Transform.h"

#include <stack>
using namespace std;

void PushTransform(HANDLE ctx)
{
	RetrieveContext(ctx);

	D2D1_MATRIX_3X2_F matrix;

	context->renderTarget->GetTransform(&matrix);
	context->matrixStack->push(matrix);
}

void PopTransform(HANDLE ctx)
{
	RetrieveContext(ctx);

	D2D1_MATRIX_3X2_F matrix = context->matrixStack->top();
	context->matrixStack->pop();

	context->renderTarget->SetTransform(&matrix);
}


void TranslateTransform(HANDLE ctx, FLOAT x, FLOAT y)
{
	RetrieveContext(ctx);

	D2D1_MATRIX_3X2_F matrix;
	context->renderTarget->GetTransform(&matrix);

	D2D1_MATRIX_3X2_F translateMatrix = D2D1::Matrix3x2F::Translation(x, y);
	context->renderTarget->SetTransform(matrix * translateMatrix);
}

void ScaleTransform(HANDLE ctx, FLOAT scaleX, FLOAT scaleY, D2D1_POINT_2F center)
{
	RetrieveContext(ctx);

	D2D1_MATRIX_3X2_F matrix;
	context->renderTarget->GetTransform(&matrix);

	D2D1_MATRIX_3X2_F scaleMatrix = D2D1::Matrix3x2F::Scale(scaleX, scaleY, center);

	context->renderTarget->SetTransform(scaleMatrix * matrix);
}

void RotateTransform(HANDLE ctx, FLOAT angle, D2D_POINT_2F point)
{
	RetrieveContext(ctx);

	D2D1_MATRIX_3X2_F matrix;
	context->renderTarget->GetTransform(&matrix);

	D2D1_MATRIX_3X2_F rotateMatrix = D2D1::Matrix3x2F::Rotation(angle, point);
	context->renderTarget->SetTransform(rotateMatrix * matrix);
}

void SkewTransform(HANDLE ctx, FLOAT angleX, FLOAT angleY, D2D1_POINT_2F center)
{
	RetrieveContext(ctx);

	D2D1_MATRIX_3X2_F matrix = D2D1::Matrix3x2F::Skew(angleX, angleY, center);
	context->renderTarget->SetTransform(matrix);
}

void SetTransform(HANDLE ctx, D2D1_MATRIX_3X2_F* transform)
{
	D2D1::Matrix3x2F mat;
	RetrieveContext(ctx);
	context->renderTarget->SetTransform(transform);
}

void GetTransform(HANDLE ctx, D2D1_MATRIX_3X2_F* transform)
{
	D2D1::Matrix3x2F mat;
	RetrieveContext(ctx);
	context->renderTarget->GetTransform(transform);
}

void ResetTransform(HANDLE ctx)
{
	RetrieveContext(ctx);
	context->renderTarget->SetTransform(D2D1::Matrix3x2F::Identity());
}
