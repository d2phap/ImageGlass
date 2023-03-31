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

public interface IColors
{
    Color AppBackground { get; }
    Color AppText { get; }
    Color AppTextDisabled { get; }




    Color GreyBackground { get; }
    Color HeaderBackground { get; }
    Color BlueBackground { get; }
    Color DarkBlueBackground { get; }
    Color DarkBackground { get; }
    Color MediumBackground { get; }
    Color LightBackground { get; }
    Color LighterBackground { get; }
    Color LightestBackground { get; }
    Color LightBorder { get; }
    Color DarkBorder { get; }

    Color BlueHighlight { get; }
    Color BlueSelection { get; }
    Color GreyHighlight { get; }
    Color GreySelection { get; }
    Color DarkGreySelection { get; }
    Color DarkBlueBorder { get; }
    Color LightBlueBorder { get; }
    Color ActiveControl { get; }
}
