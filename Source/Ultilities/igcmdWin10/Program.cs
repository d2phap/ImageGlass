/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;

/// <summary>
/// Application to perform Windows 8/10 specific operations.
/// These require using Windows 8/10 DLLs which aren't available
/// on Windows 7. [... and would cause igcmd to crash on Win7 if 
/// the references were added to that project]
/// </summary>
namespace igcmdWin10
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            string topcmd = args[0].ToLower().Trim();
            if (topcmd == "setlockimage")
            {
                return SetLockScreenImage(args);
            }

            return 0;
        }

        internal static int SetLockScreenImage(string[] args)
        {
            string imgPath = args[1];
            var task = LockScreenImage.SetAsync(imgPath);
            task.Wait();
            var result = task.Result;
            return result;
        }

    }
}
