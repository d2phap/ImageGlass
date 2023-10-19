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

using ImageGlass.Base.WinApi;

namespace ImageGlass.Base;


public class DarkColors : IColors
{
    private bool _designMode = false;


    public Color AppBg => Color.FromArgb(20, 25, 28);
    public Color AppText => Color.FromArgb(210, 210, 210); // Normal Text
    public Color AppTextDisabled => Color.FromArgb(140, 140, 140); // Disabled Text
    public Color Accent => _designMode ? Color.FromArgb(0, 120, 212) : WinColorsApi.GetAccentColor(true);


    public Color ControlBg => Color.FromArgb(69, 73, 74); // Control Color
    public Color ControlBgHover => Color.FromArgb(95, 101, 102); // Control Hover
    public Color ControlBgPressed => Color.FromArgb(43, 43, 43); // Control Clicked
    public Color ControlBgPressed2 => Color.FromArgb(49, 51, 53); // Control Clicked 2
    public Color ControlBgDisabled => Color.FromArgb(82, 82, 82);
    public Color ControlBgAccent => ControlBg.Blend(Accent, 0.8f);
    public Color ControlBgAccentHover => ControlBgHover.Blend(Accent, 0.7f);


    public Color ControlBorder => Color.FromArgb(92, 92, 92); // Control Border
    public Color ControlBorderAccent => Accent.WithBrightness(0.2f); // Accent Border


    // status background color
    public Color BgNeutral => Color.FromArgb(32, 38, 43);
    public Color BgInfo => Color.FromArgb(20, 44, 59);
    public Color BgSuccess => Color.FromArgb(34, 59, 42);
    public Color BgWarning => Color.FromArgb(59, 40, 10);
    public Color BgDanger => Color.FromArgb(59, 20, 19);


    public DarkColors(bool designMode = false)
    {
        _designMode = designMode;
    }
}
