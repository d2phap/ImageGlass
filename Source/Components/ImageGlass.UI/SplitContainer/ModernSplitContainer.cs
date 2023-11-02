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

namespace ImageGlass.UI;

public class ModernSplitContainer : SplitContainer
{
    private Color _splitterColor = Color.Transparent;

    public Color SplitterBackColor
    {
        get => _splitterColor;
        set
        {
            _splitterColor = value;
            Invalidate();
        }
    }

    public ModernSplitContainer()
    {
        // set default splitter width
        SplitterWidth = 10;
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);

        // change default cursors
        Cursor = Orientation == Orientation.Vertical ? Cursors.SizeWE : Cursors.SizeNS;
    }


    protected override void OnGotFocus(EventArgs e)
    {
        //base.OnGotFocus(e);
    }



    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // draw splitter background
        e.Graphics.FillRectangle(new SolidBrush(SplitterBackColor), SplitterRectangle);
    }
}