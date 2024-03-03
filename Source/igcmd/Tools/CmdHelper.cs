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
using igcmd.Tools;
using ImageGlass.Base;
using ImageGlass.Settings;
using System.Diagnostics;

namespace igcmd;


public static class CmdHelper
{
    /// <summary>
    /// Kills ImageGlass processes.
    /// Returns <c>true</c> if all processes are terminated.
    /// </summary>
    public static bool KillImageGlassProcessesAsync(Form? formOwner = null, bool showConfirm = true)
    {
        // get all IG processes
        var igProcesses = Process.GetProcesses()
        .Where(p =>
            p.Id != Environment.ProcessId &&
            p.ProcessName.Contains("ImageGlass")
        ).ToList();


        // show user confirm
        if (igProcesses.Count > 0)
        {
            var canProceed = true;
            if (showConfirm)
            {
                var result = Config.ShowInfo(formOwner,
                    title: formOwner?.Text ?? string.Empty,
                    heading: Config.Language[$"{nameof(FrmQuickSetup)}._ConfirmCloseProcess"],
                    icon: ImageGlass.Base.WinApi.ShellStockIcon.SIID_HELP,
                    buttons: PopupButton.Yes_No);

                canProceed = result.ExitResult == PopupExitResult.OK;
            }

            if (!canProceed) return false;

            // Kill all processes
            igProcesses.ForEach(p => p.Kill());
        }

        return true;
    }



    /// <summary>
    /// Launch ImageGlass app
    /// </summary>
    public static void LaunchImageGlass()
    {
        using var p = new Process();
        p.StartInfo.FileName = App.IGExePath;
        p.Start();
    }

}
