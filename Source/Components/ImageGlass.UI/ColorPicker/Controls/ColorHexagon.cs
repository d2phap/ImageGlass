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
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ImageGlass.UI;


[DefaultEvent(nameof(ColorChanged))]
public partial class ColorHexagon : Control
{
    private const float COEFFCIENT = 0.824f;
    private const int MAX_SECTORS = 7;

    private readonly ColorHexagonElement[] _hexagonElements = new ColorHexagonElement[0x93]; // 147
    private readonly float[] _matrix1 = new float[] { -0.5f, -1f, -0.5f, 0.5f, 1f, 0.5f };
    private readonly float[] _matrix2 = new float[] { COEFFCIENT, 0f, -COEFFCIENT, -COEFFCIENT, 0f, COEFFCIENT };
    private int _oldSelectedHexagonIndex = -1;
    private int _selectedHexagonIndex = -1;


    /// <summary>
    /// Occurs when color is changed.
    /// </summary>
    public event EventHandler<ColorChangedEventArgs>? ColorChanged;


    /// <summary>
    /// Gets the selected color.
    /// </summary>
    public Color SelectedColor
    {
        get
        {
            if (_selectedHexagonIndex < 0) return Color.Empty;

            return _hexagonElements[_selectedHexagonIndex].CurrentColor;
        }
    }


    public ColorHexagon()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint, true);

        for (int i = 0; i < _hexagonElements.Length; i++)
        {
            _hexagonElements[i] = new ColorHexagonElement();
        }
    }


    // Override Methods
    #region Override Methods

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (_selectedHexagonIndex >= 0)
            {
                _hexagonElements[_selectedHexagonIndex].IsSelected = false;

                var rect = _hexagonElements[_selectedHexagonIndex].BoundingRectangle;
                rect.Inflate(5, 5);
                Invalidate(rect);
            }

            _selectedHexagonIndex = -1;
            if (_oldSelectedHexagonIndex >= 0)
            {
                _selectedHexagonIndex = _oldSelectedHexagonIndex;
                _hexagonElements[_selectedHexagonIndex].IsSelected = true;

                ColorChanged?.Invoke(this, new ColorChangedEventArgs(SelectedColor));

                var rect = _hexagonElements[_selectedHexagonIndex].BoundingRectangle;
                rect.Inflate(5, 5);
                Invalidate(rect);
            }
        }

        base.OnMouseDown(e);
    }


    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        DrawHexagonHighlighter(-1);
    }


    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        DrawHexagonHighlighter(GetHexagonIndexFromCoordinates(e.X, e.Y));
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        if (BackColor == Color.Transparent)
        {
            base.OnPaintBackground(e);
        }

        Graphics g = e.Graphics;
        using var brush = new SolidBrush(BackColor);
        g.FillRectangle(brush, ClientRectangle);


        g.SmoothingMode = SmoothingMode.AntiAlias;
        foreach (var element in _hexagonElements)
        {
            element.Paint(g);
        }


        if (_oldSelectedHexagonIndex >= 0)
        {
            _hexagonElements[_oldSelectedHexagonIndex].Paint(g);
        }

        if (_selectedHexagonIndex >= 0)
        {
            _hexagonElements[_selectedHexagonIndex].Paint(g);
        }

        base.OnPaint(e);
    }


    protected override void OnResize(EventArgs e)
    {
        InitializeHexagons();
        base.OnResize(e);
    }

    #endregion // Override Methods


    // Private methods
    #region Private methods

    private void DrawHexagonHighlighter(int selectedHexagonIndex)
    {
        if (selectedHexagonIndex == _oldSelectedHexagonIndex) return;

        if (_oldSelectedHexagonIndex >= 0)
        {
            _hexagonElements[_oldSelectedHexagonIndex].IsHovered = false;

            var rect = _hexagonElements[_oldSelectedHexagonIndex].BoundingRectangle;
            rect.Inflate(5, 5);
            Invalidate(rect);
        }

        _oldSelectedHexagonIndex = selectedHexagonIndex;
        if (_oldSelectedHexagonIndex >= 0)
        {
            _hexagonElements[_oldSelectedHexagonIndex].IsHovered = true;

            var rect = _hexagonElements[_oldSelectedHexagonIndex].BoundingRectangle;
            rect.Inflate(5, 5);
            Invalidate(rect);
        }
    }


    private int GetHexagonIndexFromCoordinates(int xCoordinate, int yCoordinate)
    {
        for (int i = 0; i < _hexagonElements.Length; i++)
        {
            if (_hexagonElements[i].BoundingRectangle.Contains(xCoordinate, yCoordinate))
            {
                return i;
            }
        }

        return -1;
    }


    private void InitializeGrayscaleHexagons(ref Rectangle clientRectangle, int hexagonWidth,
                                             ref int centerOfMiddleHexagonX, ref int centerOfMiddleHexagonY,
                                             ref int index)
    {
        int red = 0xff;
        int num4 = 0x11;
        int num3 = 0x10;
        int num5 = (clientRectangle.Width - (MAX_SECTORS * hexagonWidth)) / 2
            + clientRectangle.X
            - (hexagonWidth / 2);

        centerOfMiddleHexagonX = num5;
        centerOfMiddleHexagonY = clientRectangle.Bottom;

        for (int i = 0; i < num3; i++)
        {
            _hexagonElements[index].CurrentColor = Color.FromArgb(red, red, red);
            _hexagonElements[index].SetHexagonPoints(centerOfMiddleHexagonX, centerOfMiddleHexagonY, hexagonWidth);
            centerOfMiddleHexagonX += hexagonWidth;
            index++;

            if (i == 7)
            {
                centerOfMiddleHexagonX = num5 + (hexagonWidth / 2);
                centerOfMiddleHexagonY += (int)(hexagonWidth * 0.824f);
            }

            red -= num4;
        }
    }


    private void InitializeHexagons()
    {
        var clientRectangle = ClientRectangle;
        clientRectangle.Offset(0, -8);

        if (clientRectangle.Height < clientRectangle.Width)
        {
            clientRectangle.Inflate(-(clientRectangle.Width - clientRectangle.Height) / 2, 0);
        }
        else
        {
            clientRectangle.Inflate(0, -(clientRectangle.Height - clientRectangle.Width) / 2);
        }

        var hexagonWidth = GetHexagonWidth(Math.Min(clientRectangle.Height, clientRectangle.Width));
        var centerOfMiddleHexagonX = (clientRectangle.Left + clientRectangle.Right) / 2;
        var centerOfMiddleHexagonY = (clientRectangle.Top + clientRectangle.Bottom) / 2;

        centerOfMiddleHexagonY -= hexagonWidth;
        _hexagonElements[0].CurrentColor = Color.White;
        _hexagonElements[0].SetHexagonPoints(centerOfMiddleHexagonX, centerOfMiddleHexagonY, hexagonWidth);

        var index = 1;
        for (int i = 1; i < MAX_SECTORS; i++)
        {
            var yCoordinate = centerOfMiddleHexagonY;
            var xCoordinate = centerOfMiddleHexagonX + (hexagonWidth * i);

            for (int j = 0; j < MAX_SECTORS - 1; j++)
            {
                var num9 = (int)(hexagonWidth * _matrix2[j]);
                var num10 = (int)(hexagonWidth * _matrix1[j]);

                for (int k = 0; k < i; k++)
                {
                    var num12 = (0.936 * (MAX_SECTORS - i) / MAX_SECTORS) + 0.12;
                    var colorQuotient = GetColorQuotient(xCoordinate - centerOfMiddleHexagonX, yCoordinate - centerOfMiddleHexagonY);

                    _hexagonElements[index].SetHexagonPoints(xCoordinate, yCoordinate, hexagonWidth);
                    _hexagonElements[index].CurrentColor = ColorFromRGBRatios((double)colorQuotient, num12, 1.0);

                    yCoordinate += num9;
                    xCoordinate += num10;
                    index++;
                }
            }
        }

        clientRectangle.Y -= hexagonWidth + (hexagonWidth / 2);

        InitializeGrayscaleHexagons(ref clientRectangle, hexagonWidth, ref centerOfMiddleHexagonX, ref centerOfMiddleHexagonY, ref index);
    }

    #endregion // Private methods


    // Private Static functions
    #region Private Static functions

    private static int GetHexagonWidth(int availableHeight)
    {
        int num = availableHeight / (2 * MAX_SECTORS);

        if ((((int)Math.Floor((double)(num / 2.0))) * 2) < num)
        {
            num--;
        }

        return num;
    }


    private static float GetColorQuotient(float value1, float value2)
    {
        return (float)(Math.Atan2((double)value2, (double)value1) * 180.0 / Math.PI);
    }


    private static int GetColorChannelValue(float value1, float value2, float value3)
    {
        if (value3 > 360f)
        {
            value3 -= 360f;
        }
        else if (value3 < 0f)
        {
            value3 += 360f;
        }

        if (value3 < 60f)
        {
            value1 += (value2 - value1) * value3 / 60f;
        }
        else if (value3 < 180f)
        {
            value1 = value2;
        }
        else if (value3 < 240f)
        {
            value1 += (value2 - value1) * (240f - value3) / 60f;
        }

        return (int)(value1 * 255f);
    }


    private static Color ColorFromRGBRatios(double value1, double value2, double value3)
    {
        int num;
        int num2;
        int num3;

        if (value3 == 0.0)
        {
            num = num2 = num3 = (int)(value2 * 255.0);
        }
        else
        {
            float num4;
            if (value2 <= 0.5)
            {
                num4 = (float)(value2 + (value2 * value3));
            }
            else
            {
                num4 = (float)(value2 + value3 - (value2 * value3));
            }

            var num5 = (float)(2.0 * value2) - num4;

            num = GetColorChannelValue(num5, num4, (float)(value1 + 120.0));
            num2 = GetColorChannelValue(num5, num4, (float)value1);
            num3 = GetColorChannelValue(num5, num4, (float)(value1 - 120.0));
        }

        return Color.FromArgb(num, num2, num3);
    }

    #endregion // Private Static functions

}

