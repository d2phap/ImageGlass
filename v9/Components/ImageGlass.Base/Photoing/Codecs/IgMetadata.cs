/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
namespace ImageGlass.Base.Photoing.Codecs;


public class IgMetadata
{
    // File metadata
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;

    public DateTime DateCreated { get; set; }
    public DateTime DateAccessed { get; set; }
    public DateTime DateModified { get; set; }
    public string DateCreatedFormated => BHelper.FormatDateTime(DateCreated);
    public string DateAccessedFormated => BHelper.FormatDateTime(DateAccessed);
    public string DateModifiedFormated => BHelper.FormatDateTime(DateModified);

    public long FileSize { get; set; } = 0;
    public string FileSizeFormated => BHelper.FormatSize(FileSize);


    // Image data
    public int OriginalWidth { get; set; } = 0;
    public int OriginalHeight { get; set; } = 0;
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;

    public int FramesCount { get; set; } = 0;
    public bool HasAlpha { get; set; } = false;
    public bool CanAnimate { get; set; } = false;


    // EXIF metadata
    public int ExifRatingPercent { get; set; } = 0;
    public DateTime? ExifDateTimeOriginal { get; set; } = null;
    public DateTime? ExifDateTime { get; set; } = null;
    public string? ExifImageDescription { get; set; } = null;
    public string? ExifModel { get;set;} = null;
    public string? ExifArtist { get;set;} = null;
    public string? ExifCopyright { get;set;} = null;
    public string? ExifSoftware { get; set; } = null;
    public float? ExifExposureTime { get;set;} = null;
    public float? ExifFNumber { get;set;} = null;
    public int? ExifISOSpeed { get;set;} = null;
    public float? ExifFocalLength { get;set;} = null;
    
}
