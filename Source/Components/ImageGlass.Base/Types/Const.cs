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

using System.Globalization;

namespace ImageGlass.Base;


/// <summary>
/// Constants list of the app
/// </summary>
public static class Const
{
    public const string UPDATE_CHANNEL = "stable"; // stable or beta
    public const int MENU_ICON_HEIGHT = 24;
    public const float VIEWER_GRID_SIZE = 9f;
    public const int TOOLBAR_ICON_HEIGHT = 24;
    public const int THUMBNAIL_HEIGHT = 70;
    public const string CONFIG_CMD_PREFIX = "/";
    public const string DATETIME_FORMAT = "yyyy/MM/dd HH:mm:ss";
    public const string DATE_FORMAT = "yyyy/MM/dd";
    public const string APP_PROTOCOL = "imageglass";
    public const string APP_CODE = "kobe";
    public const string MS_APPSTORE_ID = "9N33VZK3C7TH";
    public const int MAX_IMAGE_DIMENSION = 16_384;

    /// <summary>
    /// A file macro to replace with the current viewing image file path in double quotes.
    /// Example: <c>"C:\my\photo.jpg"</c>
    /// </summary>
    public const string FILE_MACRO = "<file>";

    public const string THEME_SYSTEM_ACCENT_COLOR = "accent";

    // predefined built-in tool names
    public const string IGTOOL_EXIFTOOL = "Tool_ExifGlass";
    public const string IGTOOL_SLIDESHOW = "Tool_Slideshow";

    public const string FRAME_NAV_TOOLBAR_FRAME_INFO = "Lbl_FrameNav_FrameInfo";
    public const string FRAME_NAV_TOOLBAR_TOGGLE_ANIMATION = "Btn_FrameNav_ToggleFrameAnimation";


    /// <summary>
    /// Gets the aspect ratio value.
    /// </summary>
    public static Dictionary<SelectionAspectRatio, int[]> AspectRatioValue => new(9)
    {
        { SelectionAspectRatio.Ratio1_1,    [1, 1] },
        { SelectionAspectRatio.Ratio1_2,    [1, 2] },
        { SelectionAspectRatio.Ratio2_1,    [2, 1] },
        { SelectionAspectRatio.Ratio2_3,    [2, 3] },
        { SelectionAspectRatio.Ratio3_2,    [3, 2] },
        { SelectionAspectRatio.Ratio3_4,    [3, 4] },
        { SelectionAspectRatio.Ratio4_3,    [4, 3] },
        { SelectionAspectRatio.Ratio9_16,   [9, 16] },
        { SelectionAspectRatio.Ratio16_9,   [16, 9] },
    };

    /// <summary>
    /// Quick setup version constant.
    /// If the value read from config file is less than this value,
    /// the Quick setup dialog will be opened.
    /// </summary>
    public const int QUICK_SETUP_VERSION = 9;

    /// <summary>
    /// The default theme pack
    /// </summary>
    public const string DEFAULT_THEME = "Kobe";

    /// <summary>
    /// Gets built-in image formats
    /// </summary>
    public const string IMAGE_FORMATS = ".3fr;.apng;.ari;.arw;.avif;.b64;.bay;.bmp;.cap;.cr2;.cr3;.crw;.cur;.cut;.dcr;.dcs;.dds;.dib;.dng;.drf;.eip;.emf;.erf;.exif;.exr;.fff;.fits;.flif;.gif;.gifv;.gpr;.hdr;.heic;.heif;.ico;.iiq;.jfif;.jp2;.jpe;.jpeg;.jpg;.jxl;.k25;.kdc;.mdc;.mef;.mjpeg;.mos;.mrw;.nef;.nrw;.obm;.orf;.pbm;.pcx;.pef;.pgm;.png;.ppm;.psb;.psd;.ptx;.pxn;.qoi;.r3d;.raf;.raw;.rw2;.rwl;.rwz;.sr2;.srf;.srw;.svg;.tga;.tif;.tiff;.viff;.webp;.wmf;.wpg;.x3f;.xbm;.xpm;.xv";

    /// <summary>
    /// Number format to use for save/restore ImageGlass settings
    /// </summary>
    public static NumberFormatInfo NumberFormat => new()
    {
        NegativeSign = "-",
    };

}