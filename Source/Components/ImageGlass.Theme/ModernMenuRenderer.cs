/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
Project homepage: http://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Drawing;
using System.Windows.Forms;

namespace ImageGlass.Theme
{
    public class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        public Color ThemeBackgroundColor { get; set; } = Color.White;
        public Color ThemeTextColor { get; set; } = Color.White;

        public ModernMenuRenderer(Color backgroundColor, Color textColor) : base(new ModernColors())
        {
            this.ThemeBackgroundColor = backgroundColor;
            this.ThemeTextColor = textColor;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (e.Item.Enabled)
            {
                e.TextColor = this.ThemeTextColor;
                base.OnRenderItemText(e);
            }
            else
            {
                // KBR 20190615 this step appears to be unnecessary [and prevents the menu from auto-collapsing]
                //e.Item.Enabled = true;

                if (this.ThemeBackgroundColor.GetBrightness() > 0.5) //light background color
                {
                    e.TextColor = Theme.DarkenColor(this.ThemeBackgroundColor, 0.5f);
                }
                else //dark background color
                {
                    e.TextColor = Theme.LightenColor(this.ThemeBackgroundColor, 0.5f);
                }


                base.OnRenderItemText(e);

                // KBR 20190615 this step appears to be unnecessary [and prevents the menu from auto-collapsing]
                //e.Item.Enabled = false;
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            if (e.Vertical || (e.Item as ToolStripSeparator) == null)
            {
                base.OnRenderSeparator(e);
            }
            else
            {
                var tsBounds = new Rectangle(Point.Empty, e.Item.Size);

                var lineY = tsBounds.Bottom - (tsBounds.Height / 2);
                var lineLeft = tsBounds.Left;
                var lineRight = tsBounds.Right;
                using (var pen = new Pen(Color.Black)) // KBR 20181231 fix handle leak
                {
                    if (this.ThemeBackgroundColor.GetBrightness() > 0.5) //light background color
                    {
                        pen.Color = Color.FromArgb(35, 0, 0, 0);
                    }
                    else //dark background color
                    {
                        pen.Color = Color.FromArgb(35, 255, 255, 255);
                    }

                    e.Graphics.DrawLine(pen, lineLeft, lineY, lineRight, lineY);
                }
                base.OnRenderSeparator(e);
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is ToolStripDropDown)
            {
                // draw background
                using (var brush = new SolidBrush(this.ThemeBackgroundColor)) // KBR 20181231 fix handle leak
                    e.Graphics.FillRectangle(brush, e.AffectedBounds);


                // draw border
                using (var pen = new Pen(Color.Black)) // KBR 20181231 fix handle leak
                {
                    if (this.ThemeBackgroundColor.GetBrightness() > 0.5) //light background color
                    {
                        pen.Color = Color.FromArgb(35, 0, 0, 0);
                    }
                    else //dark background color
                    {
                        pen.Color = Color.FromArgb(35, 255, 255, 255);
                    }

                    e.Graphics.DrawRectangle(pen, 0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1);
                }
            }

            base.OnRenderToolStripBackground(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            using (Pen pen = new Pen(this.ThemeTextColor, 1)) // KBR 20181231 fix handle leak
            {
                e.Graphics.DrawLine(pen,
                    e.Item.Width - (5 * e.Item.Height / 8),
                    3 * e.Item.Height / 8,
                    e.Item.Width - (4 * e.Item.Height / 8),
                    e.Item.Height / 2);

                e.Graphics.DrawLine(pen,
                    e.Item.Width - (4 * e.Item.Height / 8),
                    e.Item.Height / 2,
                    e.Item.Width - (5 * e.Item.Height / 8),
                    5 * e.Item.Height / 8);
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            using (Pen pen = new Pen(this.ThemeTextColor, 2)) // KBR 20181231 fix handle leak
            {
                e.Graphics.DrawLine(pen,
                    2 * e.Item.Height / 10 + 1,
                    e.Item.Height / 2,
                    4 * e.Item.Height / 10 + 1,
                    7 * e.Item.Height / 10);

                e.Graphics.DrawLine(pen,
                    4 * e.Item.Height / 10,
                    7 * e.Item.Height / 10,
                    8 * e.Item.Height / 10,
                    3 * e.Item.Height / 10);
            }
        }
        
    }



    public class ModernColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected
        {
            get
            {
                return Color.FromArgb(35, 0, 0, 0);
            }
        }
        public override Color MenuBorder
        {
            get
            {
                return Color.Transparent;
            }
        }
        public override Color MenuItemBorder
        {
            get
            {
                return Color.Transparent;
            }
        }
        
        public override Color ImageMarginGradientBegin
        {
            get
            {
                return Color.Transparent;
            }
        }
        public override Color ImageMarginGradientMiddle
        {
            get
            {
                return Color.Transparent;
            }
        }
        public override Color ImageMarginGradientEnd
        {
            get
            {
                return Color.Transparent;
            }
        }
        public override Color SeparatorDark
        {
            get
            {
                return Color.Transparent;
            }
        }
        public override Color SeparatorLight
        {
            get
            {
                return Color.Transparent;
            }
        }
        public override Color CheckBackground
        {
            get
            {
                return Color.Transparent;
            }
        }
        public override Color CheckPressedBackground
        {
            get
            {
                return Color.Transparent;
            }
        }
        public override Color CheckSelectedBackground
        {
            get
            {
                return Color.Transparent;
            }
        }
        public override Color ButtonSelectedBorder
        {
            get
            {
                return Color.Transparent;
            }
        }

        public override Color ToolStripDropDownBackground => base.ToolStripDropDownBackground;
    }
}
