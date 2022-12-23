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

namespace ImageGlass.Base;


/// <summary>
/// The list of commands for slideshow
/// </summary>
public static class SlideshowPipeCommands
{
    /// <summary>
    /// Sends the list of file paths as JSON to the slideshow client.
    /// </summary>
    public static string SET_IMAGE_LIST => "set-image-list";


    /// <summary>
    /// Sends the language to the slideshow client.
    /// </summary>
    public static string SET_LANGUAGE => "set-language";


    /// <summary>
    /// Sends the theme name to the slideshow client.
    /// </summary>
    public static string SET_THEME => "set-theme";


}
