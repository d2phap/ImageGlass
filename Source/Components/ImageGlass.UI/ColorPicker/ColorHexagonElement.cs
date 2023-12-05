/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using System.Drawing.Drawing2D;

namespace ImageGlass.UI;

internal class ColorHexagonElement
{
    private readonly PointF[] _hexagonPoints = new PointF[6];


    // Public Properties
    #region Public Properties

    /// <summary>
    /// Gets the bounding rectangle
    /// </summary>
    public Rectangle BoundingRectangle { get; private set; } = Rectangle.Empty;

    /// <summary>
    /// Gets, sets the current color
    /// </summary>
    public Color CurrentColor { get; set; } = Color.Empty;

    /// <summary>
    /// Gets, sets the hover state
    /// </summary>
    public bool IsHovered { get; set; } = false;

    /// <summary>
    /// Gets, sets the selection state
    /// </summary>
    public bool IsSelected { get; set; } = false;

    #endregion // Public Properties


    // Public Methods
    #region Public Methods

    /// <summary>
    /// Paints hexagon element.
    /// </summary>
    public void Paint(Graphics g)
    {
        using var path = new GraphicsPath();
        path.AddPolygon(_hexagonPoints);
        path.CloseAllFigures();

        using var brush = new SolidBrush(CurrentColor);
        g.FillPath(brush, path);


        if (IsHovered || IsSelected)
        {
            var opacity = IsHovered ? 200 : 255;
            var smoothingMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var pen = new Pen(Color.FromArgb(opacity, Color.White), 5f)
            {
                LineJoin = LineJoin.Round,
                DashCap = DashCap.Round,
                Alignment = PenAlignment.Inset,
            };

            g.DrawPath(pen, path);

            pen.Color = Color.FromArgb(opacity, Color.Black);
            pen.Width = 3f;
            g.DrawPath(pen, path);

            g.SmoothingMode = smoothingMode;
        }
    }


    /// <summary>
    /// Sets hexagon points.
    /// </summary>
    public void SetHexagonPoints(float xCoordinate, float yCoordinate, int hexagonWidth)
    {
        var num = hexagonWidth * 0.5773503f;

        _hexagonPoints[0] = new PointF(
            (float)Math.Floor((double)(xCoordinate - hexagonWidth / 2f)),
            (float)Math.Floor((double)(yCoordinate - num / 2f)) - 1);

        _hexagonPoints[1] = new PointF(
            (float)Math.Floor((double)xCoordinate),
            (float)Math.Floor((double)(yCoordinate - hexagonWidth / 2)) - 1);

        _hexagonPoints[2] = new PointF(
            (float)Math.Floor((double)(xCoordinate + hexagonWidth / 2)),
            (float)Math.Floor((double)(yCoordinate - num / 2f)) - 1);

        _hexagonPoints[3] = new PointF(
            (float)Math.Floor((double)(xCoordinate + hexagonWidth / 2)),
            (float)Math.Floor((double)(yCoordinate + num / 2f)) + 1);

        _hexagonPoints[4] = new PointF(
            (float)Math.Floor((double)xCoordinate),
            (float)Math.Floor((double)(yCoordinate + hexagonWidth / 2)) + 1);

        _hexagonPoints[5] = new PointF(
            (float)Math.Floor((double)(xCoordinate - hexagonWidth / 2)),
            (float)Math.Floor((double)(yCoordinate + num / 2f)) + 1);


        using var path = new GraphicsPath();
        path.AddPolygon(_hexagonPoints);
        BoundingRectangle = Rectangle.Round(path.GetBounds());
        BoundingRectangle.Inflate(2, 2);
    }

    #endregion // Public Methods

}
