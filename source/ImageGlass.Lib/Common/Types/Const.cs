/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using Avalonia.Media;
using System.Globalization;

namespace ImageGlass.Common.Types;


/// <summary>
/// Constants list of the app
/// </summary>
public static class Const
{
    public const int TOOLBAR_ICON_HEIGHT = 24;
    public const string CONFIG_CMD_PREFIX = "-p:";
    public const string DATETIME_FORMAT = "yyyy/MM/dd HH:mm:ss";
    public const string DATE_FORMAT = "yyyy/MM/dd";
    public const string APP_PROTOCOL = "imageglass";

    /// <summary>
    /// Official website host. Campaign params are appended to this host and its subdomains only.
    /// </summary>
    public const string WEBSITE_HOST = "imageglass.org";


    /// <summary>
    /// Marker file that turns on portable mode when present in the startup dir: the config dir
    /// becomes the startup dir. Shipped in the ZIP package only (see <see cref="ConfigMode"/>).
    /// </summary>
    public const string PORTABLE_MARKER_FILE = ".igportable";

    public static readonly string SIGN_POSITIVE = NumberFormatInfo.CurrentInfo.PositiveSign;
    public static readonly string SIGN_NEGATIVE = NumberFormatInfo.CurrentInfo.NegativeSign;
    public static readonly string DECIMAL_SEPARATOR = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
    public static readonly Color COLOR_EMPTY = new Color(0, 0, 0, 0);
    public static readonly int MOUSE_WHEEL_SCROLL_DELTA = 120;


    /// <summary>
    /// A file macro to replace with the current viewing image file path in double quotes.
    /// Example: <c>"C:\my\photo.jpg"</c>
    /// </summary>
    public const string FILE_MACRO = "<file>";
    public const string THEME_SYSTEM_ACCENT = "accent";


    /// <summary>
    /// Quick setup version constant.
    /// If the value read from config file is less than this value,
    /// the Quick setup dialog will be opened.
    /// </summary>
    public const double QUICK_SETUP_VERSION = 10f;

    /// <summary>
    /// The default theme pack (dark mode).
    /// </summary>
    public const string DEFAULT_THEME = "Kobe";

    /// <summary>
    /// The default theme pack for light mode.
    /// </summary>
    public const string DEFAULT_LIGHT_THEME = "Kobe-Light";

    /// <summary>
    /// Gets built-in image formats
    /// </summary>
    public const string IMAGE_FORMATS = ".3fr;.ari;.arw;.avif;.bay;.bmp;.cap;.cr2;.cr3;.crw;.cur;.cut;.dcr;.dcs;.dds;.dib;.dng;.drf;.eip;.emf;.erf;.exif;.exr;.fax;.fff;.fits;.flif;.gif;.gifv;.gpr;.hdr;.heic;.heif;.hif;.ico;.iiq;.jfif;.jp2;.jpe;.jpeg;.jpg;.jxl;.k25;.kdc;.mdc;.mef;.mjpeg;.mos;.mrw;.nef;.nrw;.obm;.orf;.pbm;.pcx;.pef;.pgm;.png;.ppm;.psb;.psd;.ptx;.pxn;.qoi;.r3d;.raf;.raw;.rw2;.rwl;.rwz;.sr2;.srf;.srw;.svg;.svgz;.tga;.tif;.tiff;.viff;.webp;.wmf;.wpg;.x3f;.xbm;.xpm;.xv";


    /// <summary>
    /// Monospace font stack for code / credits / metadata text. The list is
    /// OS-specific because Avalonia's font matcher resolves the first family the
    /// platform recognizes — and on Linux fontconfig substitutes any unknown
    /// family (e.g. "Cascadia Code") with the default <i>proportional</i> font
    /// instead of failing, so a real monospace family must be listed first.
    /// "monospace" is the generic fontconfig alias kept as a final fallback.
    /// </summary>
    public static readonly string FONT_CODE = BHelper.OS switch
    {
        OSType.Mac => "SF Mono, Menlo, Monaco, Courier New, monospace",
        OSType.Windows => "Cascadia Code, Cascadia Mono, Consolas, Courier New, monospace",
        _ => "DejaVu Sans Mono, Liberation Mono, Noto Sans Mono, monospace",
    };


    /// <summary>
    /// On macOS the bundled Inter font renders larger than the native system font,
    /// so the standard sizes are nudged down to match the Windows/Linux builds.
    /// </summary>
    private static readonly double FONT_SIZE_MAC_OFFSET = BHelper.OS != OSType.Mac ? -1 : 0;
    public static readonly double FONT_SIZE_BODY = BHelper.OS == OSType.Mac ? 12.5 : 13;
    public static readonly double FONT_SIZE_TITLE = 22 + FONT_SIZE_MAC_OFFSET;
    public static readonly double FONT_SIZE_SUBTITLE = 18 + FONT_SIZE_MAC_OFFSET;
    public static readonly double FONT_SIZE_SMALL = 13 + FONT_SIZE_MAC_OFFSET;

}

