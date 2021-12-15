

namespace ImageGlass.UI;

public class ModernMenuColors : ProfessionalColorTable
{
    public override Color MenuItemSelected => Color.Transparent;
    public override Color MenuBorder => Color.Transparent;
    public override Color MenuItemBorder => Color.Transparent;

    public override Color ImageMarginGradientBegin => Color.Transparent;
    public override Color ImageMarginGradientMiddle => Color.Transparent;
    public override Color ImageMarginGradientEnd => Color.Transparent;
    public override Color SeparatorDark => Color.Transparent;
    public override Color SeparatorLight => Color.Transparent;
    public override Color CheckBackground => Color.Transparent;
    public override Color CheckPressedBackground => Color.Transparent;
    public override Color CheckSelectedBackground => Color.Transparent;
    public override Color ButtonSelectedBorder => Color.Transparent;
    public override Color ToolStripDropDownBackground => base.ToolStripDropDownBackground;

    private IgTheme theme { get; set; }

    public ModernMenuColors(IgTheme theme)
    {
        this.theme = theme;
    }
}
