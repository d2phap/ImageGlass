﻿/*
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
using ImageGlass.Views;

namespace ImageGlass;

public partial class FrmCrop : ToolForm
{
    public FrmCrop(Form owner, IgTheme theme) : base(theme)
    {
        InitializeComponent();
        Owner = owner;


        ApplyTheme(Theme.Settings.IsDarkMode);
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        var isDarkMode = darkMode;

        if (Theme != null)
        {
            isDarkMode = Theme.Settings.IsDarkMode;

            SuspendLayout();
            TableBottom.BackColor = BackColor.InvertBlackOrWhite(30);

            CmbAspectRatio.DarkMode =
                LblLocation.DarkMode =
                LblSize.DarkMode =
                LblAspectRatio.DarkMode =

                NumX.DarkMode =
                NumY.DarkMode =
                NumWidth.DarkMode =
                NumHeight.DarkMode =

                BtnSave.DarkMode =
                BtnSaveAs.DarkMode =
                BtnCopy.DarkMode =
                BtnReset.DarkMode = isDarkMode;

            ResumeLayout(false);
        }

        base.ApplyTheme(isDarkMode, style);
    }


    protected override void OnLoad(EventArgs e)
    {
        UpdateHeight();
        LoadAspectRatioItems();

        Program.FormMain.PicMain.OnSelectionChanged += PicMain_OnImageSelecting;
        Program.FormMain.PicMain.OnImageChanged += PicMain_OnImageChanged;


        // set default location offset on the parent form
        var padding = DpiApi.Transform(10);
        var x = padding;
        var y = DpiApi.Transform(SystemInformation.CaptionHeight + Constants.TOOLBAR_ICON_HEIGHT * 2) + padding;

        InitLocation = new Point(x, y);

        base.OnLoad(e);
    }


    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        Program.FormMain.PicMain.OnSelectionChanged -= PicMain_OnImageSelecting;
        Program.FormMain.PicMain.OnImageChanged -= PicMain_OnImageChanged;
    }


    private void PicMain_OnImageSelecting(Views.SelectionEventArgs e)
    {
        NumX.Value = (decimal)e.SourceSelection.X;
        NumY.Value = (decimal)e.SourceSelection.Y;
        NumWidth.Value = (decimal)e.SourceSelection.Width;
        NumHeight.Value = (decimal)e.SourceSelection.Height;
    }


    private void PicMain_OnImageChanged(EventArgs e)
    {
        TableTop.Enabled =
            TableBottom.Enabled = Program.FormMain.PicMain.Source != ImageSource.Null;

        UpdateAspectRatioValues();
    }


    private void CmbAspectRatio_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateAspectRatioValues();
    }


    private void NumRatio_ValueChanged(object sender, EventArgs e)
    {

    }


    /// <summary>
    /// Recalculate and update window height.
    /// </summary>
    private void UpdateHeight()
    {
        MinimumSize = new Size(0, 0);

        // calculate form height
        var contentHeight = TableTop.Height + TableTop.Padding.Vertical +
            TableBottom.Height + (TableBottom.Padding.Vertical * 2);

        Height = contentHeight;
        MinimumSize = new Size(Width, contentHeight);
    }


    private void LoadAspectRatioItems()
    {
        CmbAspectRatio.Items.Clear();

        foreach (SelectionAspectRatio arValue in Enum.GetValues(typeof(SelectionAspectRatio)))
        {
            var displayName = "";
            if (Constants.AspectRatioValue.TryGetValue(arValue, out var enumValue))
            {
                displayName = string.Join(":", enumValue);
            }
            else
            {
                var arName = Enum.GetName(typeof(SelectionAspectRatio), arValue);
                displayName = arName;
            }

            CmbAspectRatio.Items.Add(displayName);
        }


        // select item
        CmbAspectRatio.SelectedIndex = 2;
    }


    private void UpdateAspectRatioValues()
    {
        var ratio = (SelectionAspectRatio)CmbAspectRatio.SelectedIndex;
        var ratioFrom = 0;
        var ratioTo = 0;


        if (ratio == SelectionAspectRatio.Original)
        {
            var srcW = (int)Program.FormMain.PicMain.SourceWidth;
            var srcH = (int)Program.FormMain.PicMain.SourceHeight;

            if (srcW > 0 && srcH > 0)
            {
                var results = BHelper.SimplifyFractions(srcW, srcH);
                ratioFrom = results[0];
                ratioTo = results[1];
            }
        }

        // fill selected item to the text boxes
        else if (Constants.AspectRatioValue.TryGetValue((SelectionAspectRatio)CmbAspectRatio.SelectedIndex, out var value))
        {
            ratioFrom = value[0];
            ratioTo = value[1];
        }

        NumRatioFrom.Value = ratioFrom;
        NumRatioTo.Value = ratioTo;

        NumRatioFrom.Visible =
            NumRatioTo.Visible =
            ratio == SelectionAspectRatio.Custom || ratio == SelectionAspectRatio.Original;
        NumRatioFrom.Enabled =
            NumRatioTo.Enabled = ratio == SelectionAspectRatio.Custom;

        UpdateHeight();
    }

    
}