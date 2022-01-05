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

---------------------
ImageGlass.Gallery is based on ImageListView v13.8.2:
Url: https://github.com/oozcitak/imagelistview
License: Apache License Version 2.0, http://www.apache.org/licenses/
---------------------
*/

namespace ImageGlass.Gallery;

/// <summary>
/// Contains image metadata.
/// </summary>
public class Metadata
{
    /// <summary>
    /// Error.
    /// </summary>
    public Exception? Error { get; internal set; } = null;

    /// <summary>
    /// Image width.
    /// </summary>
    public int Width { get; internal set; } = 0;

    /// <summary>
    /// Image height.
    /// </summary>
    public int Height { get; internal set; } = 0;

    /// <summary>
    /// Horizontal DPI.
    /// </summary>
    public double DPIX { get; internal set; } = 0.0;

    /// <summary>
    /// Vertical DPI.
    /// </summary>
    public double DPIY { get; internal set; } = 0.0;

    /// <summary>
    /// Date taken (null = not available).
    /// </summary>
    public DateTime? DateTaken { get; internal set; } = null;

    /// <summary>
    /// Image description (null = not available).
    /// </summary>
    public string? ImageDescription { get; internal set; } = null;

    /// <summary>
    /// Camera manufacturer (null = not available).
    /// </summary>
    public string? EquipmentManufacturer { get; internal set; } = null;

    /// <summary>
    /// Camera model (null = not available).
    /// </summary>
    public string? EquipmentModel { get; internal set; } = null;

    /// <summary>
    /// Image creator (null = not available).
    /// </summary>
    public string? Artist { get; internal set; } = null;

    /// <summary>
    /// Iso speed rating.
    /// </summary>
    public int ISOSpeed { get; internal set; } = 0;

    /// <summary>
    /// Exposure time.
    /// </summary>
    public double ExposureTime { get; internal set; } = 0.0;

    /// <summary>
    /// F number.
    /// </summary>
    public double FNumber { get; internal set; } = 0.0;

    /// <summary>
    /// Copyright information (null = not available).
    /// </summary>
    public string? Copyright { get; internal set; } = null;

    /// <summary>
    /// Rating value between 0-99.
    /// </summary>
    public int Rating { get; internal set; } = 0;

    /// <summary>
    /// User comment (null = not available).
    /// </summary>
    public string? Comment { get; internal set; } = null;

    /// <summary>
    /// Software used (null = not available).
    /// </summary>
    public string? Software { get; internal set; } = null;

    /// <summary>
    /// Focal length.
    /// </summary>
    public double FocalLength { get; internal set; } = 0.0;

}
