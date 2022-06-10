

using ImageGlass.Base;
using ImageGlass.Base.WinApi;

namespace igcmd;

public static class Functions
{
    public static IgExitCode SetDesktopWallpaper(string bmpPath, string styleStr)
    {
        // Get style
        if (Enum.TryParse(styleStr, out WallpaperStyle style))
        {
            var result = DesktopApi.SetWallpaper(bmpPath, style);


            if (result == WallpaperResult.PrivilegesFail)
            {
                return IgExitCode.AdminRequired;
            }
            else if (result == WallpaperResult.Success)
            {
                return IgExitCode.Done;
            }
        }

        return IgExitCode.Error;
    }
}
