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

namespace igcmd10;

partial class FrmShare : Form
{
    public FrmShare()
    {
        InitializeComponent();
    }

    private void FrmShare_Load(object sender, EventArgs e)
    {
        // This app closes whenever the main window is focused
        // set the form to fill the entire screen so that whenever we click outside
        // of it the entire app is closed

        // enable fullscreen
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Normal;
        Bounds = Screen.FromControl(this).Bounds;

        OpenShare();
    }

    protected override void OnActivated(EventArgs e)
    {
        if (!WinShare.IsShareShown)
        {
            Thread.Sleep(300);
        }

        base.OnActivated(e);

        if (CanFocus && WinShare.IsShareShown)
        {
            Thread.Sleep(1000);
            Application.Exit();
        }
    }

    private void OpenShare()
    {
        if (Program.Args.Length > 1)
        {
            var args = Program.Args
                // remove the exe file path
                .Skip(1)
                // remove the other commands
                .Where(i => !i.StartsWith("--"))
                .ToArray();

            WinShare.ShowShare(Handle, args);
        }
        else
        {
            Close();
        }
    }

}

