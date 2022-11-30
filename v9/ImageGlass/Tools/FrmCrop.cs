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
using ImageGlass.UI;

namespace ImageGlass;

public partial class FrmCrop : ToolForm
{
    public FrmCrop(Form owner, IgTheme theme) : base(theme)
    {
        InitializeComponent();
        Owner = owner;

        var padding = DpiApi.Transform(10);
        var top = SystemInformation.CaptionHeight + Constants.TOOLBAR_ICON_HEIGHT * 2;

        // set default location offset on the parent form
        InitLocation = new Point(padding, DpiApi.Transform(top) + padding);


        ApplyTheme(Theme.Settings.IsDarkMode);
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        var isDarkMode = darkMode;

        if (Theme != null)
        {
            isDarkMode = Theme.Settings.IsDarkMode;

            SuspendLayout();
            tableBottom.BackColor = BackColor.InvertBlackOrWhite(30);

            cmbAspectRatio.DarkMode =
                lblX.DarkMode =
                lblY.DarkMode =
                lblWidth.DarkMode =
                lblHeight.DarkMode =
                lblAspectRatio.DarkMode =

                numX.DarkMode =
                numY.DarkMode =
                numWidth.DarkMode =
                numHeight.DarkMode =

                btnSave.DarkMode =
                btnSaveAs.DarkMode =
                btnCopy.DarkMode =
                btnReset.DarkMode = isDarkMode;

            ResumeLayout(false);
        }

        base.ApplyTheme(isDarkMode, style);
    }


    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        UpdateHeight();
    }


    /// <summary>
    /// Recalculate and update window height.
    /// </summary>
    public void UpdateHeight()
    {
        // calculate form height
        var contentHeight = tableTop.Height + tableTop.Padding.Vertical +
            tableBottom.Height + (tableBottom.Padding.Vertical * 2);

        Height = contentHeight;
        MinimumSize = new Size(Width, contentHeight);
    }


}
