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


public class DarkColors : IColors
{
    // app background color
    public Color AppBackground => Color.FromArgb(20, 25, 28);
    public Color AppText => Color.FromArgb(210, 210, 210); // Normal Text
    public Color AppTextDisabled => Color.FromArgb(140, 140, 140); // Disabled Text




    public Color GreyBackground => Color.FromArgb(60, 63, 65); // Form Background


    public Color HeaderBackground => Color.FromArgb(57, 60, 62); // List Alt Color
    public Color BlueBackground => LighterBackground.Blend(WinColorsApi.GetAccentColor(true), 0.7f); // Color.FromArgb(66, 77, 95);
    public Color DarkBlueBackground => LightBackground.Blend(WinColorsApi.GetAccentColor(true), 0.8f); // Color.FromArgb(52, 57, 66);
    public Color DarkBackground => Color.FromArgb(43, 43, 43); // Control Clicked
    public Color MediumBackground => Color.FromArgb(49, 51, 53);
    public Color LightBackground => Color.FromArgb(69, 73, 74); // Control Color
    public Color LighterBackground => Color.FromArgb(95, 101, 102); // Control Hover
    public Color LightestBackground => Color.FromArgb(178, 178, 178);
    public Color LightBorder => Color.FromArgb(81, 81, 81);
    public Color DarkBorder => Color.FromArgb(51, 51, 51);

    public Color BlueHighlight => WinColorsApi.GetAccentColor(false).WithBrightness(0.2f); // Blue Borders
    public Color BlueSelection => WinColorsApi.GetAccentColor(false).WithBrightness(0); // DropDown Selection
    public Color GreyHighlight => Color.FromArgb(122, 128, 132); // ComboBox Arrow
    public Color GreySelection => Color.FromArgb(92, 92, 92); // Control Border
    public Color DarkGreySelection => Color.FromArgb(82, 82, 82);
    public Color DarkBlueBorder => Color.FromArgb(51, 61, 78);
    public Color LightBlueBorder => Color.FromArgb(86, 97, 114);
    public Color ActiveControl => Color.FromArgb(159, 178, 196);

}


