using System.Drawing;
using System.Windows.Forms;

namespace ImageGlass.Theme
{
    public class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        public ModernMenuRenderer() : base(new ModernColors()) { }
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
