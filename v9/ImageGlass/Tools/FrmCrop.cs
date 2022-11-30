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
using ImageGlass.Settings;
using ImageGlass.UI;
using ImageGlass.Views;
using System;

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

        // add control events
        Program.FormMain.PicMain.OnSelectionChanged += PicMain_OnImageSelecting;
        Program.FormMain.PicMain.OnImageChanged += PicMain_OnImageChanged;
        NumX.LostFocus += NumSelections_LostFocus;
        NumY.LostFocus += NumSelections_LostFocus;
        NumWidth.LostFocus += NumSelections_LostFocus;
        NumHeight.LostFocus += NumSelections_LostFocus;


        // set default location offset on the parent form
        var padding = DpiApi.Transform(10);
        var x = padding;
        var y = DpiApi.Transform(SystemInformation.CaptionHeight + Constants.TOOLBAR_ICON_HEIGHT * 2) + padding;
        InitLocation = new Point(x, y);

        base.OnLoad(e);

        ApplyLanguage();
    }


    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        Program.FormMain.PicMain.OnSelectionChanged -= PicMain_OnImageSelecting;
        Program.FormMain.PicMain.OnImageChanged -= PicMain_OnImageChanged;
        NumX.LostFocus -= NumSelections_LostFocus;
        NumY.LostFocus -= NumSelections_LostFocus;
        NumWidth.LostFocus -= NumSelections_LostFocus;
        NumHeight.LostFocus -= NumSelections_LostFocus;
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // get hotkey list
        bool CheckHotkey(string action)
        {
            var menuHotkeyList = Config.GetHotkey(FrmMain.CurrentMenuHotkeys, action);
            var menuHotkey = menuHotkeyList.SingleOrDefault(k => k.KeyData == e.KeyData);

            return menuHotkey != null;
        }
        
        if (CheckHotkey(nameof(Program.FormMain.MnuSave)))
        {
            BtnSave.PerformClick();
            return;
        }

        if (CheckHotkey(nameof(Program.FormMain.MnuSaveAs)))
        {
            BtnSaveAs.PerformClick();
            return;
        }

        if (CheckHotkey(nameof(Program.FormMain.MnuCopyImageData)))
        {
            BtnCopy.PerformClick();
            return;
        }
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
        Program.FormMain.PicMain.SelectionAspectRatio = new SizeF((float)NumRatioFrom.Value, (float)NumRatioTo.Value);
    }


    private void NumSelections_LostFocus(object? sender, EventArgs e)
    {
        var newRect = new RectangleF(
                (float)NumX.Value,
                (float)NumY.Value,
                (float)NumWidth.Value,
                (float)NumHeight.Value);

        if (newRect != Program.FormMain.PicMain.SourceSelection)
        {
            Program.FormMain.PicMain.SourceSelection = newRect;
        }

        Program.FormMain.PicMain.Invalidate();
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


    private void ApplyLanguage()
    {
        // get hotkey string
        var saveHotkey = Config.GetHotkeyString(FrmMain.CurrentMenuHotkeys, nameof(Program.FormMain.MnuSave));
        var saveAsHotkey = Config.GetHotkeyString(FrmMain.CurrentMenuHotkeys, nameof(Program.FormMain.MnuSaveAs));
        var copyHotkey = Config.GetHotkeyString(FrmMain.CurrentMenuHotkeys, nameof(Program.FormMain.MnuCopyImageData));

        // set hotkey
        TooltipMain.SetToolTip(BtnSave, saveHotkey);
        TooltipMain.SetToolTip(BtnSaveAs, saveAsHotkey);
        TooltipMain.SetToolTip(BtnCopy, copyHotkey);
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
        CmbAspectRatio.SelectedIndex = 0;
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

        // load custom aspect ratio
        NumRatioFrom.Value = ratioFrom;
        NumRatioTo.Value = ratioTo;

        NumRatioFrom.Visible = NumRatioTo.Visible =
            ratio == SelectionAspectRatio.Original || ratio == SelectionAspectRatio.Custom;
        NumRatioFrom.Enabled = NumRatioTo.Enabled = ratio == SelectionAspectRatio.Custom;

        // adjust form size
        UpdateHeight();


        // load selection area data
        NumX.Value = (decimal)Program.FormMain.PicMain.SourceSelection.X;
        NumY.Value = (decimal)Program.FormMain.PicMain.SourceSelection.Y;
        NumWidth.Value = (decimal)Program.FormMain.PicMain.SourceSelection.Width;
        NumHeight.Value = (decimal)Program.FormMain.PicMain.SourceSelection.Height;

    }


    private void BtnSave_Click(object sender, EventArgs e)
    {
        //SaveSelectionAsync();
    }

    private async Task SaveSelectionAsync()
    {
        var img = await Program.FormMain.GetSelectedImageAreaAsync();
    }


    private void BtnSaveAs_Click(object sender, EventArgs e)
    {
        //
    }


    private void BtnCopy_Click(object sender, EventArgs e)
    {
        Program.FormMain.MnuCopyImageData.PerformClick();
    }


    private void BtnReset_Click(object sender, EventArgs e)
    {
        Program.FormMain.PicMain.ClientSelection = new();
        Program.FormMain.PicMain.Invalidate();
    }

    
}
