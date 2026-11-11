/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
namespace ImageGlass.Tools;


/// <summary>
/// Selection aspect ratio.
/// </summary>
public enum SelectionAspectRatio
{
    FreeRatio = 0,
    Custom = 1,
    Original = 2,
    Ratio1_1 = 3,
    Ratio1_2 = 4,
    Ratio2_1 = 5,
    Ratio2_3 = 6,
    Ratio3_2 = 7,
    Ratio3_4 = 8,
    Ratio4_3 = 9,
    Ratio9_16 = 10,
    Ratio16_9 = 11,
}


/// <summary>
/// Options for Crop tool's default selection
/// </summary>
public enum DefaultSelectionType
{
    UseTheLastSelection,
    CustomArea,
    SelectAll,
    SelectNone,
    Select10Percent,
    Select20Percent,
    Select25Percent,
    Select30Percent,
    SelectOneThird,
    Select40Percent,
    Select50Percent,
    Select60Percent,
    SelectTwoThirds,
    Select70Percent,
    Select75Percent,
    Select80Percent,
    Select90Percent,
}

