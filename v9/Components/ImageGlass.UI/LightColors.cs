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

---------------------
DarkColors is based on DarkUI
Url: https://github.com/RobinPerris/DarkUI
License: MIT, https://github.com/RobinPerris/DarkUI/blob/master/LICENSE
---------------------
*/

using ImageGlass.Base;
using ImageGlass.Base.WinApi;

namespace ImageGlass.UI;


public class LightColors : IColors
{
    public Color AppBackground => Color.White;
    public Color AppText => Color.FromArgb(30, 30, 30); // Normal Text
    public Color AppTextDisabled => Color.FromArgb(113, 113, 113); // Disabled Text



    public Color GreyBackground => Color.FromArgb(231, 231, 232);
    public Color HeaderBackground => Color.FromArgb(177, 180, 182);
    public Color BlueBackground => LighterBackground.Blend(WinColorsApi.GetAccentColor(true), 0.88f); // Color.FromArgb(255, 255, 255);
    public Color DarkBlueBackground => LightBackground.Blend(WinColorsApi.GetAccentColor(true), 0.92f); // Color.FromArgb(237, 242, 250);
    public Color DarkBackground => Color.FromArgb(220, 220, 220);
    public Color MediumBackground => Color.FromArgb(230, 231, 234);
    public Color LightBackground => Color.FromArgb(245, 245, 250); // Control Colour
    public Color LighterBackground => Color.FromArgb(235, 235, 240); // Control Hover
    public Color LightestBackground => Color.FromArgb(255, 255, 255);
    public Color LightBorder => Color.FromArgb(201, 201, 201);
    public Color DarkBorder => Color.FromArgb(220, 220, 220);

    public Color BlueHighlight => WinColorsApi.GetAccentColor(false).WithBrightness(0.2f); // Selected Control Borders
    public Color BlueSelection => WinColorsApi.GetAccentColor(false).WithBrightness(0); // DropDown Selection
    public Color GreyHighlight => Color.FromArgb(113, 113, 113); // ComboBox Arrow
    public Color GreySelection => Color.FromArgb(204, 206, 219); // Control Border
    public Color DarkGreySelection => Color.FromArgb(202, 202, 202);
    public Color DarkBlueBorder => Color.FromArgb(171, 181, 198);
    public Color LightBlueBorder => Color.FromArgb(206, 217, 114);
    public Color ActiveControl => Color.FromArgb(159, 178, 196);

}


