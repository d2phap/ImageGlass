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

using ImageGlass.Base.Photoing.Codecs;

namespace ImageGlass.Gallery;


/// <summary>
/// Extracts thumbnails from images.
/// </summary>
internal static class Extractor
{
    private static IExtractor _extractor = new GDIExtractor();

    public static IExtractor Current
    {
        get
        {
            if (_extractor == null)
            {
                throw new ArgumentNullException(nameof(_extractor));
            }

            return _extractor;
        }
    }
}

