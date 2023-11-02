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
#nullable disable
using System.ComponentModel.DataAnnotations;

namespace ImageGlass.Base.Update;

public class UpdateModel
{
    public float ApiVersion { get; set; } = 1;

    public Dictionary<string, ReleaseModel> Releases { get; set; } = new();

}


public class ReleaseModel
{
    public string Version { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    [DataType(DataType.Url)]
    public Uri ChangelogUrl { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime PublishedDate { get; set; }

    public IEnumerable<DownloadModel> Downloads { get; set; }

}

public class DownloadModel
{
    public string Architecture { get; set; } = "";
    public string Extension { get; set; } = "";
    public string HashCode { get; set; } = "";

    [DataType(DataType.Url)]
    public Uri Url { get; set; }
}
