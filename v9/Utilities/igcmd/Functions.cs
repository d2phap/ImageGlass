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
