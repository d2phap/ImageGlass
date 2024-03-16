/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
/// Language information
/// </summary>
public record IgLangMetadata
{
    public string Code { get; set; } = "en-US";
    public string EnglishName { get; set; } = "English";
    public string LocalName { get; set; } = "English";
    public string Author { get; set; } = "Duong Dieu Phap";
    public string MinVersion { get; set; } = "9.1";
}


/// <summary>
/// JSON model for IgLang file
/// </summary>
/// <param name="_Metadata"></param>
/// <param name="Items"></param>
public record IgLangJsonModel(IgLangMetadata _Metadata, IDictionary<string, string> Items) { }
