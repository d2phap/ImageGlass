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
using Cysharp.Text;
using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using ImageGlass.UI;

namespace ImageGlass;

public partial class FrmToolNotFound : DialogForm
{
    private string _toolId;

    private IgTool? Tool => Config.Tools.SingleOrDefault(i => i.ToolId.Equals(_toolId, StringComparison.Ordinal));


    /// <summary>
    /// Gets the executable path of the tool.
    /// </summary>
    public string ExecutablePath { get; private set; } = string.Empty;


    public FrmToolNotFound(string toolId = "") : base()
    {
        InitializeComponent();
        _toolId = toolId;
    }


    // Override / Virtual methods
    #region Override / Virtual methods

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        ApplyTheme(Config.Theme.Settings.IsDarkMode);
        ApplyLanguage();

        TopMost = Config.EnableWindowTopMost;


        var workingArea = Screen.FromControl(this).WorkingArea;
        if (Bottom > workingArea.Bottom) Top = workingArea.Bottom - Height;
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();

        LblHeading.ForeColor =
            LnkGetTools.LinkColor =
            LnkGetTools.VisitedLinkColor =
            WinColorsApi.GetAccentColor(false)
                .WithBrightness(darkMode ? 0.4f : 0f);
        LnkGetTools.ActiveLinkColor = LnkGetTools.LinkColor.WithBrightness(0.3f);

        TableTop.BackColor = Config.Theme.ColorPalatte.AppBg;


        base.ApplyTheme(darkMode, style);
        ResumeLayout();
    }


    protected override void OnSystemAccentColorChanged(SystemAccentColorChangedEventArgs e)
    {
        base.OnSystemAccentColorChanged(e);

        // update the heading color to match system accent color
        LblHeading.ForeColor =
            LnkGetTools.LinkColor =
            LnkGetTools.VisitedLinkColor =
            SystemAccentColorChangedEventArgs.AccentColor
                .NoAlpha()
                .WithBrightness(DarkMode ? 0.4f : 0f);
        LnkGetTools.ActiveLinkColor = LnkGetTools.LinkColor.WithBrightness(0.3f);
    }


    protected override int OnUpdateHeight(bool performUpdate = true)
    {
        var baseHeight = base.OnUpdateHeight(false);
        var formHeight = TableTop.Height + TableTop.Padding.Bottom + baseHeight;

        if (performUpdate && Height != formHeight)
        {
            Height = formHeight;
        }

        return formHeight;
    }


    protected override void OnAcceptButtonClicked()
    {
        ExecutablePath = TxtExecutable.Text.Trim();

        // invalid executable path
        if (string.IsNullOrEmpty(ExecutablePath))
        {
            TxtExecutable.Focus();
            return;
        }


        base.OnAcceptButtonClicked();
    }


    protected override void OnRequestUpdatingLanguage()
    {
        ApplyLanguage();
    }


    #endregion // Override / Virtual methods



    // Private methods
    #region Private methods

    private void ApplyLanguage()
    {
        if (Tool == null) return;

        Text = Config.Language[$"{Name}._Title"];
        BtnAccept.Text = Config.Language["_._OK"];
        BtnCancel.Text = Config.Language["_._Cancel"];
        BtnApply.Text = Config.Language["_._Apply"];

        var toolName = Tool.ToolName ?? Tool.ToolId;

        LblHeading.Text = ZString.Format(Config.Language[$"{Name}.{nameof(LblHeading)}"], toolName);
        LblDescription.Text = ZString.Format(Config.Language[$"{Name}.{nameof(LblDescription)}"], toolName);
        LblDownloadToolText.Text = Config.Language[$"{Name}.{nameof(LblDownloadToolText)}"];
        BtnSelectExecutable.Text = Config.Language[$"{Name}.{nameof(BtnSelectExecutable)}"];
    }


    private void LnkGetTools_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        _ = BHelper.OpenUrlAsync("https://imageglass.org/tools", "from_get_tools");
    }


    private void BtnSelectExecutable_Click(object sender, EventArgs e)
    {
        using var openDlg = new OpenFileDialog()
        {
            Filter = "Executable file (*.exe)|*.exe",
            SupportMultiDottedExtensions = true,
        };
        if (openDlg.ShowDialog() != DialogResult.OK) return;


        TxtExecutable.Text = openDlg.FileName;
    }


    #endregion // Private methods


}
