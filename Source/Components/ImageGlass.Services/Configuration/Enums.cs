/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017 DUONG DIEU PHAP
Project homepage: http://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace ImageGlass.Services.Configuration
{
    public enum ImageOrderBy
    {
        Name = 0,
        Length = 1,
        CreationTime = 2,
        Extension = 3,
        LastAccessTime = 4,
        LastWriteTime = 5,
        Random = 6
    }

    public enum ZoomOptimizationValue
    {
        Auto = 0,
        SmoothPixels = 1,
        ClearPixels = 2
    }

    public enum ImageFormatGroup
    {
        Default = 0,
        Optional = 1
    }

    public enum Constants
    {
        MENU_ICON_HEIGHT = 21,
        TOOLBAR_ICON_HEIGHT = 20,
        TOOLBAR_HEIGHT = 40
    }
    public enum MouseWheelActions
    {
        DO_NOTHING = 0,
        ZOOM = 1,
        SCROLL_VERTICAL = 2,
        SCROLL_HORIZONTAL = 3,
        BROWSE_IMAGES = 4
    }
    
}
