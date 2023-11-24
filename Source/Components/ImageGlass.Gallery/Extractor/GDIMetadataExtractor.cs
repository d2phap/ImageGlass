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
ImageGlass.Gallery is based on ImageListView v13.8.2:
Url: https://github.com/oozcitak/imagelistview
License: Apache License Version 2.0, http://www.apache.org/licenses/
---------------------
*/
using ImageGlass.Base.Photoing.Codecs;
using System.Text;

namespace ImageGlass.Gallery;


/// <summary>
/// Read metadata.
/// </summary>
public partial class GDIExtractor : IExtractor
{
    #region Properties
    /// <summary>
    /// Gets the name of this extractor.
    /// </summary>
    public virtual string Name => "GDI Thumbnail Extractor";

    #endregion


    #region Exif Tag IDs
    private const int TagImageDescription = 0x010E;
    private const int TagEquipmentModel = 0x0110;
    private const int TagDateTimeOriginal = 0x9003;
    private const int TagArtist = 0x013B;
    private const int TagCopyright = 0x8298;
    private const int TagExposureTime = 0x829A;
    private const int TagFNumber = 0x829D;
    private const int TagISOSpeed = 0x8827;
    private const int TagUserComment = 0x9286;
    private const int TagRating = 0x4746;
    private const int TagRatingPercent = 0x4749;
    private const int TagEquipmentManufacturer = 0x010F;
    private const int TagFocalLength = 0x920A;
    private const int TagSoftware = 0x0131;

    #endregion


    #region Exif Format Conversion
    /// <summary>
    /// Converts the given Exif data to an ASCII encoded string.
    /// </summary>
    /// <param name="value">Exif data as a byte array.</param>
    private static string ExifAscii(byte[] value)
    {
        if (value == null || value.Length == 0)
            return string.Empty;

        var str = Encoding.ASCII.GetString(value).Trim(new char[] { '\0' });

        return str;
    }

    /// <summary>
    /// Converts the given Exif data to DateTime.
    /// </summary>
    /// <param name="value">Exif data as a byte array.</param>
    private static DateTime ExifDateTime(byte[] value)
    {
        return ExifDateTime(ExifAscii(value));
    }

    /// <summary>
    /// Converts the given Exif data to DateTime.
    /// Value must be formatted as yyyy:MM:dd HH:mm:ss.
    /// </summary>
    /// <param name="value">Exif data as a string.</param>
    private static DateTime ExifDateTime(string value)
    {
        var parts = value.Split(new char[] { ':', ' ' });
        try
        {
            if (parts.Length == 6)
            {
                // yyyy:MM:dd HH:mm:ss
                // This is the expected format though some cameras
                // can use single digits. See Issue 21.
                return new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]));
            }
            else if (parts.Length == 3)
            {
                // yyyy:MM:dd
                return new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
            }
            else
            {
                return DateTime.MinValue;
            }
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// Converts the given Exif data to an 16-bit unsigned integer.
    /// The value must have 2 bytes.
    /// </summary>
    /// <param name="value">Exif data as a byte array.</param>
    private static ushort ExifUShort(byte[] value)
    {
        return BitConverter.ToUInt16(value, 0);
    }

    /// <summary>
    /// Converts the given Exif data to an 32-bit unsigned integer.
    /// The value must have 4 bytes.
    /// </summary>
    /// <param name="value">Exif data as a byte array.</param>
    private static uint ExifUInt(byte[] value)
    {
        return BitConverter.ToUInt32(value, 0);
    }

    /// <summary>
    /// Converts the given Exif data to an 32-bit signed integer.
    /// The value must have 4 bytes.
    /// </summary>
    /// <param name="value">Exif data as a byte array.</param>
    private static int ExifInt(byte[] value)
    {
        return BitConverter.ToInt32(value, 0);
    }

    /// <summary>
    /// Converts the given Exif data to an unsigned rational value
    /// represented as a string.
    /// The value must have 8 bytes.
    /// </summary>
    /// <param name="value">Exif data as a byte array.</param>
    private static string ExifURational(byte[] value)
    {
        return BitConverter.ToUInt32(value, 0).ToString() + "/" +
                BitConverter.ToUInt32(value, 4).ToString();
    }

    /// <summary>
    /// Converts the given Exif data to a signed rational value
    /// represented as a string.
    /// The value must have 8 bytes.
    /// </summary>
    /// <param name="value">Exif data as a byte array.</param>
    private static string ExifRational(byte[] value)
    {
        return BitConverter.ToInt32(value, 0).ToString() + "/" +
                BitConverter.ToInt32(value, 4).ToString();
    }

    /// <summary>
    /// Converts the given Exif data to a double number.
    /// The value must have 8 bytes.
    /// </summary>
    /// <param name="value">Exif data as a byte array.</param>
    private static double ExifDouble(byte[] value)
    {
        var num = BitConverter.ToUInt32(value, 0);
        var den = BitConverter.ToUInt32(value, 4);

        if (den == 0)
            return 0.0;
        else
            return num / (double)den;
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Returns image metadata.
    /// </summary>
    /// <param name="path">Filepath of image</param>
    public virtual IgMetadata GetMetadata(string path)
    {
        return PhotoCodec.LoadMetadata(path);
    }

    #endregion
}

