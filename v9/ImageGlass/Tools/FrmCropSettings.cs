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

using ImageGlass.Settings;
using ImageGlass.UI;

namespace ImageGlass;

public partial class FrmCropSettings : DialogForm
{
    public CropToolConfig Settings { get; init; }


    public FrmCropSettings(CropToolConfig settings) : base()
    {
        InitializeComponent();
        Settings = settings;

        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }


    // Override / Virtual methods
    #region Override / Virtual methods

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        ApplyLanguage();
        LoadSettings();
    }

    protected override void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        // update theme here
        ApplyTheme(e.IsDarkMode);

        base.OnRequestUpdatingColorMode(e);
    }


    protected override int OnUpdateHeight(bool performUpdate = true)
    {
        var baseHeight = base.OnUpdateHeight(false);
        var contentHeight = tableTop.Height + tableTop.Padding.Vertical;

        if (performUpdate)
        {
            Height = contentHeight + baseHeight;
        }

        return contentHeight;
    }


    protected override void OnAcceptButtonClicked()
    {
        base.OnAcceptButtonClicked();
        ApplySettings();
    }


    protected override void OnApplyButtonClicked()
    {
        base.OnApplyButtonClicked();
        ApplySettings();
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
        LblDefaultSelectionType.Text = Config.Language[$"{Name}.{nameof(LblDefaultSelectionType)}"];
        LoadSelectionTypeItems();

        LblDefaultSelectionArea.Text = Config.Language[$"{Name}.{nameof(LblDefaultSelectionArea)}"];
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
                displayName = enumName;
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
        var isCustomSelect = selectionType == DefaultSelectionType.CustomArea;

        LblDefaultSelectionArea.Visible =
            LblLocation.Visible =
            NumX.Visible =
            NumY.Visible =
            ChkAutoCenterSelection.Visible =
            LblSize.Visible =
            NumWidth.Visible =
            NumHeight.Visible = isCustomSelect;

        OnUpdateHeight();
    }


    #endregion // Private methods


}
