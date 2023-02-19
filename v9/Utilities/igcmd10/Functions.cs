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
using ImageGlass.Base;
using System.Security;
using Windows.Storage;
using Windows.System.UserProfile;

namespace igcmd10;

public class Functions
{
    /// <summary>
    /// Sets the Lock Screen background
    /// </summary>
    public static IgExitCode SetLockScreenBackground(string imgPath)
    {
        if (string.IsNullOrEmpty(imgPath))
        {
            return IgExitCode.Error;
        }


        var result = BHelper.RunSync(() => SetLockScreenBackgroundAsync(imgPath));

        return result;
    }


    /// <summary>
    /// Sets the Lock Screen background
    /// </summary>
    private static async Task<IgExitCode> SetLockScreenBackgroundAsync(string imgPath)
    {
        try
        {
            var imgFile = await StorageFile.GetFileFromPathAsync(imgPath);

            using var stream = await imgFile.OpenAsync(FileAccessMode.Read);
            await LockScreen.SetImageStreamAsync(stream);
        }
        catch (Exception ex)
        {
            if (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                return IgExitCode.AdminRequired;
            }

            return IgExitCode.Error;
        }

        return IgExitCode.Done;
    }
}
