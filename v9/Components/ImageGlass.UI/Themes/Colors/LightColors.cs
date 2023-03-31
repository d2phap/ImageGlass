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
    public Color AppBg => Color.White;
    public Color AppText => Color.FromArgb(30, 30, 30); // Normal Text
    public Color AppTextDisabled => Color.FromArgb(113, 113, 113); // Disabled Text
    public Color Accent => WinColorsApi.GetAccentColor(false);


    public Color ControlBg => Color.FromArgb(245, 245, 250); // Control Color
    public Color ControlBgHover => Color.FromArgb(235, 235, 240); // Control Hover
    public Color ControlBgPressed => Color.FromArgb(220, 220, 220); // Control Clicked
    public Color ControlBgPressed2 => Color.FromArgb(230, 231, 234); // Control Clicked 2
    public Color ControlBgDisabled => Color.FromArgb(202, 202, 202);
    public Color ControlBgAccent => ControlBg.Blend(WinColorsApi.GetAccentColor(true), 0.92f);
    public Color ControlBgAccentHover => ControlBgHover.Blend(WinColorsApi.GetAccentColor(true), 0.88f);


    public Color ControlBorder => Color.FromArgb(204, 206, 219); // Control Border
    public Color ControlBorderAccent => WinColorsApi.GetAccentColor(false).WithBrightness(0.2f); // Accent Border

}
