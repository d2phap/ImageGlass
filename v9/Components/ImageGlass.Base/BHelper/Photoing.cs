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
using ImageGlass.Base.WinApi;
using ImageMagick;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using WicNet;
using ColorProfile = ImageMagick.ColorProfile;

namespace ImageGlass.Base;


public partial class BHelper
{
    /// <summary>
    /// Converts <see cref="BitmapSource"/> to <see cref="WicBitmapSource"/> object.
    /// </summary>
    public static WicBitmapSource? ToWicBitmapSource(BitmapSource? bmp)
    {
        if (bmp == null)
            return null;

        var prop = bmp.GetType().GetProperty("WicSourceHandle",
            BindingFlags.NonPublic | BindingFlags.Instance);

        var srcHandle = (SafeHandleZeroOrMinusOneIsInvalid?)prop?.GetValue(bmp);
        if (srcHandle == null) return null;


        var obj = Marshal.GetObjectForIUnknown(srcHandle.DangerousGetHandle());

        var wicSrc = new WicBitmapSource(obj);
        try
        {
            wicSrc.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);
        }
        catch (InvalidOperationException)
        {
            // cannot convert format
            return null;
        }

        return wicSrc;
    }


    /// <summary>
    /// Converts <see cref="Bitmap"/> to <see cref="WicBitmapSource"/>.
    /// </summary>
    public static WicBitmapSource? ToWicBitmapSource(Bitmap? bmp)
    {
        if (bmp == null) return null;

        var wicSrc = WicBitmapSource.FromHBitmap(bmp.GetHbitmap());

        try
        {
            wicSrc.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);
        }
        catch (InvalidOperationException)
        {
            // cannot convert format
            return null;
        }

        return wicSrc;
    }


    /// <summary>
    /// Converts <see cref="Image"/> to <see cref="WicBitmapSource"/>.
    /// </summary>
    public static WicBitmapSource? ToWicBitmapSource(Image? img)
    {
        if (img == null) return null;

        var bmp = new Bitmap(img);
        return ToWicBitmapSource(bmp);
    }


    /// <summary>
    /// Converts <see cref="Stream"/> to <see cref="WicBitmapSource"/>.
    /// </summary>
    public static WicBitmapSource? ToWicBitmapSource(Stream? stream)
    {
        if (stream == null) return null;

        var wicSrc = WicBitmapSource.Load(stream);
        try
        {
            wicSrc.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);
        }
        catch (InvalidOperationException)
        {
            // cannot convert format
            return null;
        }

        return wicSrc;
    }


    /// <summary>
    /// Converts base64 string to <see cref="WicBitmapSource"/>.
    /// </summary>
    /// <param name="base64">Base64 string</param>
    public static WicBitmapSource? ToWicBitmapSource(string base64)
    {
        var (MimeType, ByteData) = ConvertBase64ToBytes(base64);
        if (string.IsNullOrEmpty(MimeType)) return null;

        // supported MIME types:
        // https://www.iana.org/assignments/media-types/media-types.xhtml#image
        var settings = new MagickReadSettings();
        settings.Format = MimeTypeToMagickFormat(MimeType);

        if (settings.Format == MagickFormat.Svg)
        {
            settings.BackgroundColor = MagickColors.Transparent;
        }


        WicBitmapSource? src = null;
        switch (settings.Format)
        {
            case MagickFormat.Gif:
            case MagickFormat.Gif87:
            case MagickFormat.Tif:
            case MagickFormat.Tiff64:
            case MagickFormat.Tiff:
            case MagickFormat.Ico:
            case MagickFormat.Icon:
                using (var ms = new MemoryStream(ByteData) { Position = 0 })
                {
                    using var bitm = new Bitmap(ms, true);
                    src = WicBitmapSource.FromHBitmap(bitm.GetHbitmap());
                }
                break;

            default:
                using (var imgM = new MagickImage(ByteData, settings))
                {
                    var bmp = imgM.ToBitmapSource();
                    src = BHelper.ToWicBitmapSource(bmp);
                }
                break;
        }

        if (src == null) return null;

        try
        {
            src.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);
        }
        catch (InvalidOperationException)
        {
            // cannot convert format
            return null;
        }
        return src;
    }


    /// <summary>
    /// Converts <see cref="WicBitmapSource"/> to <see cref="Bitmap"/>.
    /// https://stackoverflow.com/a/2897325/2856887
    /// </summary>
    public static Bitmap? ToGdiPlusBitmap(WicBitmapSource? source)
    {
        if (source == null)
            return null;

        var bmp = new Bitmap(
          source.Width,
          source.Height,
          PixelFormat.Format32bppPArgb);

        var data = bmp.LockBits(
          new Rectangle(new(0, 0), bmp.Size),
          ImageLockMode.WriteOnly,
          PixelFormat.Format32bppPArgb);

        source.CopyPixels(data.Height * data.Stride, data.Scan0, data.Stride);

        bmp.UnlockBits(data);

        return bmp;
    }


    /// <summary>
    /// Converts <see cref="BitmapSource"/> to <see cref="Bitmap"/>.
    /// https://stackoverflow.com/a/2897325/2856887
    /// </summary>
    public static Bitmap? ToGdiPlusBitmap(BitmapSource? source)
    {
        if (source == null)
            return null;

        var bmp = new Bitmap(
          (int)source.Width,
          (int)source.Height,
          PixelFormat.Format32bppPArgb);

        var data = bmp.LockBits(
          new Rectangle(new(0, 0), bmp.Size),
          ImageLockMode.WriteOnly,
          PixelFormat.Format32bppPArgb);

        source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);

        bmp.UnlockBits(data);

        return bmp;
    }


    /// <summary>
    /// Loads <see cref="Bitmap"/> from file.
    /// </summary>
    /// <param name="filePath">Full file path.</param>
    /// <param name="useICM">Use color profile.</param>
    public static Bitmap ToGdiPlusBitmap(string filePath, bool useICM = true)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var ms = new MemoryStream();
        fs.CopyTo(ms);
        ms.Position = 0;

        return new Bitmap(ms, useICM);
    }


    /// <summary>
    /// Converts base64 string to <see cref="Bitmap"/>.
    /// </summary>
    public static Bitmap? ToGdiPlusBitmapFromBase64(string? content)
    {
        var (MimeType, ByteData) = ConvertBase64ToBytes(content);
        if (string.IsNullOrEmpty(MimeType)) return null;

        // supported MIME types:
        // https://www.iana.org/assignments/media-types/media-types.xhtml#image
        var settings = new MagickReadSettings();
        settings.Format = MimeTypeToMagickFormat(MimeType);

        if (settings.Format == MagickFormat.Svg)
        {
            settings.BackgroundColor = MagickColors.Transparent;
        }


        Bitmap? bmp = null;
        switch (settings.Format)
        {
            case MagickFormat.Gif:
            case MagickFormat.Gif87:
            case MagickFormat.Tif:
            case MagickFormat.Tiff64:
            case MagickFormat.Tiff:
            case MagickFormat.Ico:
            case MagickFormat.Icon:
                bmp = new Bitmap(new MemoryStream(ByteData)
                {
                    Position = 0
                }, true);

                break;

            default:
                using (var imgM = new MagickImage(ByteData, settings))
                {
                    bmp = imgM.ToBitmap();
                }
                break;
        }

        return bmp;
    }


    /// <summary>
    /// Loads and process the SVG file, replaces <c>#000</c> or <c>#fff</c>
    /// by the corresponding hex color value of the <paramref name="darkMode"/>.
    /// </summary>
    public static Bitmap? ToGdiPlusBitmapFromSvg(string? svgFilePath, bool darkMode, int? width = null, int? height = null)
    {
        if (string.IsNullOrEmpty(svgFilePath)) return null;

        using var imgM = PhotoCodec.ReadSvgWithMagick(svgFilePath, darkMode, width, height);
        return imgM.ToBitmap(ImageFormat.Png);
    }


    /// <summary>
    /// Returns Exif rotation in degrees. Returns 0 if the metadata
    /// does not exist or could not be read. A negative value means
    /// the image needs to be mirrored about the vertical axis.
    /// </summary>
    /// <param name="orientationFlag">Orientation Flag</param>
    /// <returns></returns>
    public static int GetOrientationDegree(int orientationFlag)
    {
        if (orientationFlag == 1)
            return 0;
        else if (orientationFlag == 2)
            return -360;
        else if (orientationFlag == 3)
            return 180;
        else if (orientationFlag == 4)
            return -180;
        else if (orientationFlag == 5)
            return -90;
        else if (orientationFlag == 6)
            return 90;
        else if (orientationFlag == 7)
            return -270;
        else if (orientationFlag == 8)
            return 270;

        return 0;
    }


    /// <summary>
    /// Converts base64 string to byte array, returns MIME type and raw data in byte array.
    /// </summary>
    /// <param name="content">Base64 string</param>
    /// <returns></returns>
    public static (string MimeType, byte[] ByteData) ConvertBase64ToBytes(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentNullException(nameof(content));
        }

        // data:image/svg-xml;base64,xxxxxxxx
        // type is optional
        var base64DataUri = new Regex(@"(^data\:(?<type>image\/[a-z\+\-]*);base64,)?(?<data>[a-zA-Z0-9\+\/\=]+)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);


        var match = base64DataUri.Match(content);
        if (!match.Success)
        {
            throw new FormatException("The base64 content is invalid.");
        }


        var base64Data = match.Groups["data"].Value;
        var byteData = Convert.FromBase64String(base64Data);
        var mimeType = match.Groups["type"].Value.ToLower();

        if (mimeType.Length == 0)
        {
            // use default PNG MIME type
            mimeType = "image/png";
        }

        return (mimeType, byteData);
    }


    /// <summary>
    /// Get built-in color profiles
    /// </summary>
    /// <returns></returns>
    public static string[] GetBuiltInColorProfiles()
    {
        return new string[]
        {
            "AdobeRGB1998",
            "AppleRGB",
            "CoatedFOGRA39",
            "ColorMatchRGB",
            "sRGB",
            "USWebCoatedSWOP",
        };
    }


    /// <summary>
    /// Get the correct color profile name
    /// </summary>
    /// <param name="name">Name or Full path of color profile</param>
    /// <returns></returns>
    public static string GetCorrectColorProfileName(string name)
    {
        var profileName = "";

        if (name.Equals(Constants.CURRENT_MONITOR_PROFILE, StringComparison.InvariantCultureIgnoreCase))
        {
            return Constants.CURRENT_MONITOR_PROFILE;
        }
        else if (File.Exists(name))
        {
            return name;
        }
        else
        {
            var builtInProfiles = GetBuiltInColorProfiles();
            var result = Array.Find(builtInProfiles, i => string.Equals(i, name, StringComparison.InvariantCultureIgnoreCase));

            if (result != null)
            {
                profileName = result;
            }
            else
            {
                return string.Empty;
            }
        }

        return profileName;
    }


    /// <summary>
    /// Get ColorProfile
    /// </summary>
    /// <param name="name">Name or Full path of color profile</param>
    /// <returns></returns>
    public static ColorProfile? GetColorProfile(string name)
    {
        if (name.Equals(Constants.CURRENT_MONITOR_PROFILE, StringComparison.InvariantCultureIgnoreCase))
        {
            var winHandle = Process.GetCurrentProcess().MainWindowHandle;
            var colorProfilePath = DisplayApi.GetMonitorColorProfileFromWindow(winHandle);

            if (string.IsNullOrEmpty(colorProfilePath))
            {
                return ColorProfile.SRGB;
            }

            return new ColorProfile(colorProfilePath);
        }
        else if (File.Exists(name))
        {
            return new ColorProfile(name);
        }
        else
        {
            // get all profile names in Magick.NET
            var profiles = typeof(ColorProfile).GetProperties();
            var result = Array.Find(profiles, i => string.Equals(i.Name, name, StringComparison.InvariantCultureIgnoreCase));

            if (result != null)
            {
                try
                {
                    return (ColorProfile?)result.GetValue(result);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        return null;
    }


    /// <summary>
    /// Gets image MIME type from the input extension.
    /// Returns <c>image/png</c> if the format is not supported.
    /// </summary>
    /// <param name="ext">The extension, example: .png</param>
    public static string GetMIMEType(string? ext)
    {
        var mimeType = ext?.ToUpperInvariant() switch
        {
            ".GIF" => "image/gif",
            ".BMP" => "image/bmp",
            ".PNG" => "image/png",
            ".WEBP" => "image/webp",
            ".SVG" => "image/svg+xml",
            ".JPG" or ".JPEG" or ".JFIF" or ".JP2" => "image/jpeg",
            ".JXL" => "image/jxl",
            ".TIF" or ".TIFF" or "FAX" => "image/tiff",
            ".ICO" or ".ICON" => "image/x-icon",
            _ => "image/png",
        };

        return mimeType;
    }


    /// <summary>
    /// Gets image MIME type from the input <see cref="ImageFormat"/>.
    /// Returns <paramref name="defaultValue"/> if the format is not supported.
    /// </summary>
    /// <param name="format">Image format</param>
    /// <returns></returns>
    public static string GetMIMEType(ImageFormat? format = null, string defaultValue = "image/png")
    {
        if (format == null)
        {
            return defaultValue;
        }

        if (format.Equals(ImageFormat.Gif))
        {
            return "image/gif";
        }

        if (format.Equals(ImageFormat.Bmp))
        {
            return "image/bmp";
        }

        if (format.Equals(ImageFormat.Jpeg))
        {
            return "image/jpeg";
        }

        if (format.Equals(ImageFormat.Png))
        {
            return "image/png";
        }

        if (format.Equals(ImageFormat.Tiff))
        {
            return "image/tiff";
        }

        if (format.Equals(ImageFormat.Icon))
        {
            return "image/x-icon";
        }

        return defaultValue;
    }


    /// <summary>
    /// Gets WIC container format from the input extension.
    /// Default value is <see cref="WicCodec.GUID_ContainerFormatPng"/>
    /// </summary>
    /// <param name="ext">The extension, example: .png</param>
    public static Guid GetWicContainerFormatFromExtension(string ext)
    {
        if (ext.Equals(".gif", StringComparison.OrdinalIgnoreCase))
        {
            return WicCodec.GUID_ContainerFormatGif;
        }

        if (ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase))
        {
            return WicCodec.GUID_ContainerFormatBmp;
        }

        if (ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".jpe", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".jp2", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".jxl", StringComparison.OrdinalIgnoreCase))
        {
            return WicCodec.GUID_ContainerFormatJpeg;
        }

        if (ext.Equals(".tiff", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".tif", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".fax", StringComparison.OrdinalIgnoreCase))
        {
            return WicCodec.GUID_ContainerFormatTiff;
        }

        if (ext.Equals(".ico", StringComparison.OrdinalIgnoreCase))
        {
            return WicCodec.GUID_ContainerFormatIco;
        }

        if (ext.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        {
            return WicCodec.GUID_ContainerFormatWebp;
        }


        return WicCodec.GUID_ContainerFormatPng;
    }




    /// <summary>
    /// Gets <see cref="MagickFormat"/> from mime type.
    /// </summary>
    private static MagickFormat MimeTypeToMagickFormat(string? mimeType)
    {
        return mimeType switch
        {
            "image/avif" => MagickFormat.Avif,
            "image/bmp" => MagickFormat.Bmp,
            "image/gif" => MagickFormat.Gif,
            "image/tiff" => MagickFormat.Tiff,
            "image/jpeg" => MagickFormat.Jpeg,
            "image/svg+xml" => MagickFormat.Svg,
            "image/x-icon" => MagickFormat.Ico,
            "image/x-portable-anymap" => MagickFormat.Pnm,
            "image/x-portable-bitmap" => MagickFormat.Pbm,
            "image/x-portable-graymap" => MagickFormat.Pgm,
            "image/x-portable-pixmap" => MagickFormat.Ppm,
            "image/x-xbitmap" => MagickFormat.Xbm,
            "image/x-xpixmap" => MagickFormat.Xpm,
            "image/x-cmu-raster" => MagickFormat.Ras,
            _ => MagickFormat.Png
        };
    }


    /// <summary>
    /// Crops the image.
    /// </summary>
    public static WicBitmapSource? CropImage(WicBitmapSource? img, RectangleF srcSelection)
    {
        if (img == null) return null;

        var width = (int)srcSelection.Width;
        var height = (int)srcSelection.Height;

        if (width == 0 || height == 0) return null;

        var x = (int)srcSelection.X;
        var y = (int)srcSelection.Y;

        return WicBitmapSource.FromSourceRect(img, x, y, width, height);
    }

}

