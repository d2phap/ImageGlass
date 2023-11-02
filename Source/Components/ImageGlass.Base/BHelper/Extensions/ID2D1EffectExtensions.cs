/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using DirectN;

namespace ImageGlass.Base;

public static class ID2D1EffectExtensions
{

    /// <summary>
    /// Gets <see cref="ID2D1Bitmap1"/> from <see cref="ID2D1Effect"/>.
    /// </summary>
    public static IComObject<ID2D1Bitmap1> GetD2D1Bitmap1(this IComObject<ID2D1Effect> effect, IComObject<ID2D1DeviceContext6>? dc)
    {
        // create D2D1Bitmap from WICBitmapSource
        var bmpProps = new D2D1_BITMAP_PROPERTIES1()
        {
            bitmapOptions = D2D1_BITMAP_OPTIONS.D2D1_BITMAP_OPTIONS_TARGET,
            pixelFormat = new D2D1_PIXEL_FORMAT()
            {
                alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED,
                format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
            },
            dpiX = 96.0f,
            dpiY = 96.0f,
        };

        var effectOutputImage = effect.GetOutput();
        dc.Object.GetImageLocalBounds(effectOutputImage.Object, out var outputRect);
        var newD2dBitmap = dc.CreateBitmap<ID2D1Bitmap1>(outputRect.SizeU, bmpProps);


        // save current Target, replace by ID2D1Bitmap
        dc.Object.GetTarget(out var oldTarget);
        var oldTargetObj = new ComObject<ID2D1Image>(oldTarget);
        dc.SetTarget(newD2dBitmap);


        // draw Image on Target
        dc.BeginDraw();
        dc.DrawImage(effectOutputImage, D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_NEAREST_NEIGHBOR, D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER);
        dc.EndDraw();


        // set previous Target
        dc.SetTarget(oldTargetObj);


        // release resources
        oldTargetObj.Dispose();
        effectOutputImage.Dispose();

        return newD2dBitmap;
    }
}
