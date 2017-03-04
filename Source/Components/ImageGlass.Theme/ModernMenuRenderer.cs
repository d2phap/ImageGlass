using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ImageGlass.Theme
{
    public class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        public ModernMenuRenderer() : base(new ModernColors()) { }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            Pen pen = new Pen(Color.Black, 1);

            e.Graphics.DrawLine(pen,
                e.Item.Width - (5*e.Item.Height / 8),
                3 * e.Item.Height / 8,
                e.Item.Width - (4 * e.Item.Height / 8),
                e.Item.Height / 2);

            e.Graphics.DrawLine(pen,
                e.Item.Width - (4 * e.Item.Height / 8),
                e.Item.Height / 2,
                e.Item.Width - (5*e.Item.Height / 8),
                5 * e.Item.Height / 8);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Pen pen = new Pen(Color.Black, 2);

            e.Graphics.DrawLine(pen,
                2* e.Item.Height / 10 + 1,
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

    public class ModernColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected
        {
            get
            {
                return Color.FromArgb(229, 229, 229);
            }
        }
        public override Color MenuBorder
        {
            get
            {
                return Color.FromArgb(160, 160, 160);
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
                return Color.FromArgb(215,215,215);
            }
        }
        public override Color SeparatorLight
        {
            get
            {
                return Color.FromArgb(215, 215, 215);
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

    }
}
