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
using ImageGlass.Settings;

namespace ImageGlass;

public partial class FrmCropSettings : DialogForm
{
    public CropToolConfig Settings { get; init; }


    public FrmCropSettings(CropToolConfig settings) : base()
    {
        InitializeComponent();
        Settings = settings;
    }


    // Override / Virtual methods
    #region Override / Virtual methods

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        ApplyTheme(Config.Theme.Settings.IsDarkMode);
        ApplyLanguage();
        LoadSettings();


        var workingArea = Screen.FromControl(this).WorkingArea;
        if (Bottom > workingArea.Bottom) Top = workingArea.Bottom - Height;
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();


        TableTop.BackColor = Config.Theme.ColorPalatte.AppBg;


        base.ApplyTheme(darkMode, style);
        ResumeLayout();
    }


    protected override int OnUpdateHeight(bool performUpdate = true)
    {
        var baseHeight = base.OnUpdateHeight(false);
        var formHeight = TableTop.Height + baseHeight;

        if (performUpdate && Height != formHeight)
        {
            Height = formHeight;
        }

        return formHeight;
    }


    protected override void OnAcceptButtonClicked()
    {
        base.OnAcceptButtonClicked();
        ApplySettings();
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
        Text = Config.Language[$"{Name}._Title"];
        BtnAccept.Text = Config.Language["_._OK"];
        BtnCancel.Text = Config.Language["_._Cancel"];
        BtnApply.Text = Config.Language["_._Apply"];

        ChkCloseToolAfterSaving.Text = Config.Language[$"{Name}.{nameof(ChkCloseToolAfterSaving)}"];

        LblDefaultSelection.Text = Config.Language[$"{Name}.{nameof(LblDefaultSelection)}"];
        LoadSelectionTypeItems();

        ChkAutoCenterSelection.Text = Config.Language[$"{Name}.{nameof(ChkAutoCenterSelection)}"];
        LblLocation.Text = Config.Language[$"{nameof(FrmCrop)}.{nameof(LblLocation)}"];
        LblSize.Text = Config.Language[$"{nameof(FrmCrop)}.{nameof(LblSize)}"];

    }


    private void LoadSettings()
    {
        ChkCloseToolAfterSaving.Checked = Settings.CloseToolAfterSaving;
        LoadSelectionTypeItems();

        NumX.Value = Settings.InitSelectedArea.X;
        NumY.Value = Settings.InitSelectedArea.Y;
        NumWidth.Value = Settings.InitSelectedArea.Width;
        NumHeight.Value = Settings.InitSelectedArea.Height;

        ChkAutoCenterSelection.Checked = Settings.AutoCenterSelection;
    }


    private void ApplySettings()
    {
        Settings.CloseToolAfterSaving = ChkCloseToolAfterSaving.Checked;
        Settings.InitSelectionType = (DefaultSelectionType)CmbSelectionType.SelectedIndex;

        Settings.InitSelectedArea = new Rectangle(
            (int)NumX.Value,
            (int)NumY.Value,
            (int)NumWidth.Value,
            (int)NumHeight.Value);
        Settings.AutoCenterSelection = ChkAutoCenterSelection.Checked;
    }


    private void LoadSelectionTypeItems()
    {
        CmbSelectionType.Items.Clear();
        var langPath = $"{Name}.{nameof(DefaultSelectionType)}";
        var i = 0;
        var cmbIndex = 0;

        foreach (DefaultSelectionType enumValue in Enum.GetValues(typeof(DefaultSelectionType)))
        {
            if (enumValue == Settings.InitSelectionType)
            {
                cmbIndex = i;
            }


            var enumName = Enum.GetName(typeof(DefaultSelectionType), enumValue);
            if (!Config.Language.TryGetValue($"{langPath}._{enumName}", out var displayName))
            {
                Config.Language.TryGetValue($"{langPath}._SelectX", out displayName);
                var percentText = enumValue switch
                {
                    DefaultSelectionType.Select10Percent => "10%",
                    DefaultSelectionType.Select20Percent => "20%",
                    DefaultSelectionType.Select25Percent => "25%",
                    DefaultSelectionType.Select30Percent => "30%",
                    DefaultSelectionType.SelectOneThird => "1/3",
                    DefaultSelectionType.Select40Percent => "40%",
                    DefaultSelectionType.Select50Percent => "50%",
                    DefaultSelectionType.Select60Percent => "60%",
                    DefaultSelectionType.SelectTwoThirds => "2/3",
                    DefaultSelectionType.Select70Percent => "70%",
                    DefaultSelectionType.Select75Percent => "75%",
                    DefaultSelectionType.Select80Percent => "80%",
                    DefaultSelectionType.Select90Percent => "90%",
                    _ => enumName,
                };

                displayName = ZString.Format(displayName ?? enumName, percentText);
            }

            CmbSelectionType.Items.Add(displayName ?? enumName);
            i++;
        }

        // select item
        CmbSelectionType.SelectedIndex = cmbIndex;
    }


    private void CmbSelectionType_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectionType = (DefaultSelectionType)CmbSelectionType.SelectedIndex;

        ChkAutoCenterSelection.Visible = selectionType != DefaultSelectionType.SelectNone
            && selectionType != DefaultSelectionType.SelectAll
            && selectionType != DefaultSelectionType.UseTheLastSelection;


        LblLocation.Visible =
            NumX.Visible =
            NumY.Visible =
            LblSize.Visible =
            NumWidth.Visible =
            NumHeight.Visible = selectionType == DefaultSelectionType.CustomArea;

        OnUpdateHeight();
    }


    private void ChkAutoCenterSelection_CheckedChanged(object sender, EventArgs e)
    {
        NumX.Enabled = NumY.Enabled = !ChkAutoCenterSelection.Checked;
    }


    #endregion // Private methods


}
