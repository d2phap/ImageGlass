/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
using D2Phap;
using DirectN;
using ImageGlass.Base.PhotoBox;
using System.Runtime.InteropServices;
using WicNet;

namespace ImageGlass.Views;


public static class DXCanvasExtensions
{
    /// <summary>
    /// Converts <see cref="WicBitmapSource"/> to <see cref="ID2D1Bitmap1"/> COM object.
    /// </summary>
    public static ComObject<ID2D1Bitmap>? FromWicBitmapSource(this DXCanvas c, WicBitmapSource? wicSrc)
    {
        if (c.Device == null || wicSrc == null)
        {
            return null;
        }

        wicSrc.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);

        // create D2DBitmap from WICBitmapSource
        var bitmapProps = DXHelper.CreateDefaultBitmapProps();
        var bitmapPropsPtr = bitmapProps.StructureToPtr();

        _ = c.Device.CreateBitmapFromWicBitmap(wicSrc.ComObject.Object,
            bitmapPropsPtr, out ID2D1Bitmap? bmp)
            .ThrowOnError();

        var comBmp = new ComObject<ID2D1Bitmap>(bmp);

        return comBmp;
    }


    /// <summary>
    /// Checks if the input point is inside the navigation buttons.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="onlyCheckLeftNav">
    /// If the value is
    /// <list type="bullet">
    ///   <item><c>null</c>, checks both navs</item>
    ///   <item><c>true</c>, checks only left nav</item>
    ///   <item><c>false</c>, checks only right nav</item>
    /// </list>
    /// </param>
    public static MouseAndNavLocation CheckWhichNav(this DXCanvas c, Point point,
        bool? onlyCheckLeftNav = null)
    {
        var isLocationInNavLeft = false;
        var isLocationInNavRight = false;


        if (c.NavDisplay == NavButtonDisplay.Left || c.NavDisplay == NavButtonDisplay.Both)
        {
            if (onlyCheckLeftNav == null || onlyCheckLeftNav == false)
            {
                // right clickable region
                var rightClickable = new RectangleF(
                    c.NavRightPos.X - c.NavButtonSize.Width / 2,
                    c.NavRightPos.Y - c.NavButtonSize.Height / 2,
                    c.NavButtonSize.Width,
                    c.NavButtonSize.Height);

                // emit nav button event if the point inside the rect
                if (rightClickable.Contains(point))
                {
                    // nav right clicked
                    isLocationInNavRight = true;
                }
            }
        }


        if (c.NavDisplay == NavButtonDisplay.Right || c.NavDisplay == NavButtonDisplay.Both)
        {
            if (onlyCheckLeftNav == null || onlyCheckLeftNav == true)
            {
                // left clickable region
                var leftClickable = new RectangleF(
                    c.NavLeftPos.X - c.NavButtonSize.Width / 2,
                    c.NavLeftPos.Y - c.NavButtonSize.Height / 2,
                    c.NavButtonSize.Width,
                    c.NavButtonSize.Height);

                // emit nav button event if the point inside the rect
                if (leftClickable.Contains(point))
                {
                    // nav left clicked
                    isLocationInNavLeft = true;
                }
            }
        }


        if (isLocationInNavLeft && isLocationInNavRight)
        {
            return MouseAndNavLocation.BothNavs;
        }

        if (isLocationInNavLeft)
        {
            return MouseAndNavLocation.LeftNav;
        }

        if (isLocationInNavRight)
        {
            return MouseAndNavLocation.RightNav;
        }

        return MouseAndNavLocation.Outside;
    }

}