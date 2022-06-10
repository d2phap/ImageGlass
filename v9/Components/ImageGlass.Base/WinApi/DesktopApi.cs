
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace ImageGlass.Base.WinApi;

public class DesktopApi
{
    private const int SPI_SETDESKWALLPAPER = 20;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDWININICHANGE = 0x02;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    public enum Style : int
    {
        /// <summary>
        /// Current windows wallpaper style
        /// </summary>
        Current = -1,
        Centered = 0,
        Stretched = 1,
        Tiled = 2,
    }

    public enum Result
    {
        Success = 0, // Wallpaper successfully set
        PrivsFail,  // Wallpaper failure due to privileges - can re-attempt with Admin privs.
        Fail        // Wallpaper failure - no point in re-attempting
    }


    /// <summary>
    /// Set desktop wallpaper
    /// </summary>
    /// <param name="uri">Image filename</param>
    /// <param name="style">Style of wallpaper</param>
    public static void Set(Uri uri, Style style)
    {
        using var s = new System.Net.WebClient().OpenRead(uri.ToString());

        using var img = Image.FromStream(s);
        Set(img, style);
    }


    /// <summary>
    /// Set desktop wallpaper
    /// </summary>
    /// <param name="img">Image data</param>
    /// <param name="style">Style of wallpaper</param>
    private static void Set(Image img, Style style)
    {
        try
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (key == null)
            {
                return;
            }

            if (style == Style.Stretched)
            {
                key.SetValue("WallpaperStyle", "2");
                key.SetValue("TileWallpaper", "0");
            }

            if (style == Style.Centered)
            {
                key.SetValue("WallpaperStyle", "1");
                key.SetValue("TileWallpaper", "0");
            }

            if (style == Style.Tiled)
            {
                key.SetValue("WallpaperStyle", "1");
                key.SetValue("TileWallpaper", "1");
            }

            var tempPath = App.ConfigDir(PathType.File, Dir.Temporary, "wallpaper_temp.jpg");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);

            _ = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        catch { }
    }


    private static string SaveTempFile(string path)
    {
        return "":
    }


    /// <summary>
    /// Set the desktop wallpaper.
    /// </summary>
    /// <param name="path">File system path to the image</param>
    /// <param name="style">Style of wallpaper</param>
    /// <returns>Success/failure indication.</returns>
    public static Result Set(string path, Style style)
    {
        // TODO use ImageMagick to load image
        var tempPath = Path.Combine(Path.GetTempPath(), "wallpaper_temp.bmp");

        try
        {
            using var img = Image.FromFile(path);
            // SPI_SETDESKWALLPAPER will *only* work consistently if image is a Bitmap! (Issue #327)
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);
        }
        catch
        {
            // Couldn't open/save image file: Fail, and don't re-try
            return Result.Fail;
        }

        try
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                if (key == null)
                {
                    return Result.Fail;
                }

                var tileVal = "0"; // default not-tiled
                var winStyle = "1"; // default centered
                switch (style)
                {
                    case Style.Tiled:
                        tileVal = "1";
                        break;
                    case Style.Stretched:
                        winStyle = "2";
                        break;
                    case Style.Current:
                        tileVal = (string)key.GetValue("TileWallpaper");
                        winStyle = (string)key.GetValue("WallpaperStyle");
                        break;
                }
                key.SetValue("TileWallpaper", tileVal);
                key.SetValue("WallpaperStyle", winStyle);
                key.SetValue("Wallpaper", tempPath);
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        catch (Exception ex)
        {
            if (ex is System.Security.SecurityException ||
                ex is UnauthorizedAccessException)
            {
                return Result.PrivsFail;
            }

            return Result.Fail;
        }
        return Result.Success;
    }
}
