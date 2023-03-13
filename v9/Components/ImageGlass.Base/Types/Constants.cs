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

using System.Globalization;

namespace ImageGlass.Base;


/// <summary>
/// Constants list of the app
/// </summary>
public static class Constants
{
    public const string UPDATE_CHANNEL = "moon";
    public const int MENU_ICON_HEIGHT = 24;
    public const float VIEWER_GRID_SIZE = 9f;
    public const int TOOLBAR_ICON_HEIGHT = 24;
    public const int THUMBNAIL_HEIGHT = 70;
    public const string FILE_MACRO = "<file>";
    public const string CONFIG_CMD_PREFIX = "-";
    public const string DATETIME_FORMAT = "yyyy/MM/dd HH:mm:ss";
    public const string DATE_FORMAT = "yyyy/MM/dd";
    public const string APP_PROTOCOL = "igm";
    public const string APP_CODE = "moon";
    public const string MS_APPSTORE_ID = "9N33VZK3C7TH";
    public const string CURRENT_MONITOR_PROFILE = "CurrentMonitorProfile";
    public const int MAX_IMAGE_DIMENSION = 16_384;

    public const string SLIDESHOW_PIPE_PREFIX = "ImageGlass_Slideshow_Pipe_";
    public const string THEME_SYSTEM_ACCENT_COLOR = "accent";

    // predefined built-in tool names
    public const string IGTOOL_EXIFTOOL = "Tool_ExifGlass";


    /// <summary>
    /// Gets the aspect ratio value.
    /// </summary>
    public static Dictionary<SelectionAspectRatio, int[]> AspectRatioValue => new(9)
    {
        { SelectionAspectRatio.Ratio1_1,    new int[] {1, 1} },
        { SelectionAspectRatio.Ratio1_2,    new int[] {1, 2} },
        { SelectionAspectRatio.Ratio2_1,    new int[] {2, 1} },
        { SelectionAspectRatio.Ratio2_3,    new int[] {2, 3} },
        { SelectionAspectRatio.Ratio3_2,    new int[] {3, 2} },
        { SelectionAspectRatio.Ratio3_4,    new int[] {3, 4} },
        { SelectionAspectRatio.Ratio4_3,    new int[] {4, 3} },
        { SelectionAspectRatio.Ratio9_16,   new int[] {9, 16} },
        { SelectionAspectRatio.Ratio16_9,   new int[] {16, 9} },
    };

    /// <summary>
    /// First launch version constant.
    /// If the value read from config file is less than this value,
    /// the First-Launch Configs screen will be launched.
    /// </summary>
    public const int FIRST_LAUNCH_VERSION = 9;

    /// <summary>
    /// The default theme pack
    /// </summary>
    public const string DEFAULT_THEME = "Kobe";

    /// <summary>
    /// Gets built-in image formats
    /// </summary>
    public const string IMAGE_FORMATS = "*.3fr;*.ari;*.arw;*.avif;*.b64;*.bay;*.bmp;*.cap;*.cr2;*.cr3;*.crw;*.cur;*.cut;*.dcr;*.dcs;*.dds;*.dib;*.dng;*.drf;*.eip;*.emf;*.erf;*.exif;*.exr;*.fff;*.fits;*.flif;*.gif;*.gifv;*.gpr;*.hdr;*.heic;*.heif;*.ico;*.iiq;*.jp2;*.jpe;*.jpeg;*.jpg;*.jxl;*.k25;*.kdc;*.mdc;*.mef;*.mjpeg;*.mos;*.mrw;*.nef;*.nrw;*.obm;*.orf;*.pbm;*.pcx;*.pef;*.pgm;*.png;*.ppm;*.psb;*.psd;*.ptx;*.pxn;*.qoi;*.r3d;*.raf;*.raw;*.rw2;*.rwl;*.rwz;*.sr2;*.srf;*.srw;*.svg;*.tga;*.tif;*.tiff;*.viff;*.webp;*.wmf;*.wpg;*.x3f;*.xbm;*.xpm;*.xv";

    /// <summary>
    /// Number format to use for save/restore ImageGlass settings
    /// </summary>
    public static NumberFormatInfo NumberFormat => new()
    {
        NegativeSign = "-",
    };

}