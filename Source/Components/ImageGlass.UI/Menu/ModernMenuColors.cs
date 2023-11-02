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
