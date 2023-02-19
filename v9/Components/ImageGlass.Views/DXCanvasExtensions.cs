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
using ImageGlass.Base.PhotoBox;

namespace ImageGlass.Views;


public static class DXCanvasExtensions
{

    /// <summary>
    /// Checks if the input point is inside the navigation buttons.
    /// </summary>
    public static MouseAndNavLocation CheckWhichNav(this DXCanvas c, Point point,
        NavCheck navCheck = NavCheck.Both)
    {
        var isLocationInNavLeft = false;
        var isLocationInNavRight = false;


        if (c.NavDisplay == NavButtonDisplay.Left || c.NavDisplay == NavButtonDisplay.Both)
        {
            if (navCheck == NavCheck.Both || navCheck == NavCheck.RightOnly)
            {
                // right clickable region
                var rightClickable = new RectangleF(
                    c.NavRightPos.X - c.NavButtonSize.Width / 2 + c.NAV_PADDING,
                    c.DrawingArea.Top,
                    c.NavButtonSize.Width,
                    c.DrawingArea.Height);

                // check if the point inside the rect;
                isLocationInNavRight = rightClickable.Contains(point);
            }
        }


        if (c.NavDisplay == NavButtonDisplay.Right || c.NavDisplay == NavButtonDisplay.Both)
        {
            if (navCheck == NavCheck.Both || navCheck == NavCheck.LeftOnly)
            {
                // left clickable region
                var leftClickable = new RectangleF(
                    c.NavLeftPos.X - c.NavButtonSize.Width / 2 - c.NAV_PADDING,
                    c.DrawingArea.Top,
                    c.NavButtonSize.Width,
                    c.DrawingArea.Height);

                // check if the point inside the rect
                isLocationInNavLeft = leftClickable.Contains(point);
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


    /// <summary>
    /// Computes the location of the client point into source coords.
    /// </summary>
    public static Point PointClientToSource(this DXCanvas c, Point clientPoint)
    {
        return PointClientToSource(c, clientPoint);
    }


    /// <summary>
    /// Computes the location of the client point into source coords.
    /// </summary>
    public static PointF PointClientToSource(this DXCanvas c, PointF clientPoint)
    {
        var x = (clientPoint.X - c.ImageDestBounds.X) / c.ZoomFactor + c.ImageSourceBounds.X;
        var y = (clientPoint.Y - c.ImageDestBounds.Y) / c.ZoomFactor + c.ImageSourceBounds.Y;

        return new PointF(x, y);
    }


    /// <summary>
    /// Computes the location of the source point into client coords.
    /// </summary>
    public static Point PointSourceToClient(this DXCanvas c, Point srcPoint)
    {
        return PointSourceToClient(c, srcPoint);
    }


    /// <summary>
    /// Computes the location of the source point into client coords.
    /// </summary>
    public static PointF PointSourceToClient(this DXCanvas c, PointF srcPoint)
    {
        var x = (srcPoint.X - c.ImageSourceBounds.X) * c.ZoomFactor + c.ImageDestBounds.X;
        var y = (srcPoint.Y - c.ImageSourceBounds.Y) * c.ZoomFactor + c.ImageDestBounds.Y;

        return new PointF(x, y);
    }

}