/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;
using System.Globalization;

namespace ImageGlass.Tools;

internal class CropImageSettingsWindow : DialogWindow
{
    private CheckBox _chkCloseToolAfterSaving = null!;
    private ComboBox _cmbSelectionType = null!;
    private CheckBox _chkAutoCenterSelection = null!;

    private Grid _panelCustomArea = null!;
    private NumericUpDown _numX = null!;
    private NumericUpDown _numY = null!;
    private NumericUpDown _numWidth = null!;
    private NumericUpDown _numHeight = null!;


    /// <summary>
    /// Gets a snapshot of the edited config values.
    /// </summary>
    public CropImageConfig ResultConfig { get; private set; } = new();


    public CropImageSettingsWindow(CropImageConfig config)
    {
        IsButton1Visible = true;
        IsButton2Visible = true;
        IsButton3Visible = false;

        DefaultButton = DialogButton.Button1;
        DefaultFocus = DialogFocus.Button1;

        DialogContent = CreateDialogContentElement();
        LoadConfigValues(config);
    }



    #region Override Methods

    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Title = Core.Lang[LangId.Tool_Crop_Title];
        Button1Text = Core.Lang[LangId._OK];
        Button2Text = Core.Lang[LangId._Cancel];

        _chkCloseToolAfterSaving.Content = Core.Lang[LangId.Tool_Crop_ChkCloseToolAfterSaving];
        _chkAutoCenterSelection.Content = Core.Lang[LangId.Tool_Crop_ChkAutoCenterSelection];

        LoadSelectionTypeItems();
    }


    protected override void OnDialogSubmitted(DialogEventArgs e)
    {
        var selectionType = (DefaultSelectionType)_cmbSelectionType.SelectedIndex;

        ResultConfig = new CropImageConfig
        {
            CloseAfterSaved = _chkCloseToolAfterSaving.IsChecked == true,
            InitSelectionType = selectionType,
            AutoCenterSelection = _chkAutoCenterSelection.IsChecked == true,
            InitSelectedArea = new Rect(
                (double)(_numX.Value ?? 0),
                (double)(_numY.Value ?? 0),
                (double)(_numWidth.Value ?? 0),
                (double)(_numHeight.Value ?? 0)),
        };

        base.OnDialogSubmitted(e);
    }

    #endregion // Override Methods



    #region Private Methods

    /// <summary>
    /// Loads the current config values into the controls.
    /// </summary>
    private void LoadConfigValues(CropImageConfig config)
    {
        _chkCloseToolAfterSaving.IsChecked = config.CloseAfterSaved;
        _chkAutoCenterSelection.IsChecked = config.AutoCenterSelection;

        _numX.Value = (decimal)config.InitSelectedArea.X;
        _numY.Value = (decimal)config.InitSelectedArea.Y;
        _numWidth.Value = (decimal)config.InitSelectedArea.Width;
        _numHeight.Value = (decimal)config.InitSelectedArea.Height;

        // load selection type items and select the current one
        LoadSelectionTypeItems();
        _cmbSelectionType.SelectedIndex = (int)config.InitSelectionType;

        UpdateSelectionTypeUI();
    }


    /// <summary>
    /// Loads localized selection type items into the ComboBox.
    /// </summary>
    private void LoadSelectionTypeItems()
    {
        var selectedIndex = _cmbSelectionType.SelectedIndex;
        _cmbSelectionType.Items.Clear();

        foreach (var enumValue in Enum.GetValues<DefaultSelectionType>())
        {
            var enumName = Enum.GetName(enumValue) ?? string.Empty;
            string displayName;

            // try specific lang key first
            var specificKey = enumValue switch
            {
                DefaultSelectionType.UseTheLastSelection => LangId.Tool_Crop_DefaultSelectionType_UseTheLastSelection,
                DefaultSelectionType.SelectNone => LangId.Tool_Crop_DefaultSelectionType_SelectNone,
                DefaultSelectionType.SelectAll => LangId.Tool_Crop_DefaultSelectionType_SelectAll,
                DefaultSelectionType.CustomArea => LangId.Tool_Crop_DefaultSelectionType_CustomArea,
                _ => (LangId?)null,
            };

            if (specificKey is not null)
            {
                displayName = Core.Lang[specificKey.Value];
            }
            else
            {
                // use the "Select X%" template
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

                displayName = Core.Lang[LangId.Tool_Crop_DefaultSelectionType_SelectX, percentText];
            }

            _cmbSelectionType.Items.Add(new ComboBoxItem { Content = displayName });
        }

        // restore selection
        if (selectedIndex >= 0 && selectedIndex < _cmbSelectionType.Items.Count)
        {
            _cmbSelectionType.SelectedIndex = selectedIndex;
        }
    }


    /// <summary>
    /// Updates the visibility of controls based on the selected <see cref="DefaultSelectionType"/>.
    /// </summary>
    private void UpdateSelectionTypeUI()
    {
        var selectionType = (DefaultSelectionType)_cmbSelectionType.SelectedIndex;

        // show auto-center for most selection types
        _chkAutoCenterSelection.IsVisible = selectionType is not DefaultSelectionType.SelectNone
            and not DefaultSelectionType.SelectAll
            and not DefaultSelectionType.UseTheLastSelection;

        // show custom area inputs only for CustomArea
        _panelCustomArea.IsVisible = selectionType == DefaultSelectionType.CustomArea;

        // disable X/Y when auto-center is checked
        _numX.IsEnabled = _chkAutoCenterSelection.IsChecked != true;
        _numY.IsEnabled = _chkAutoCenterSelection.IsChecked != true;
    }


    /// <summary>
    /// Creates dialog content with all settings controls.
    /// </summary>
    private StackPanel CreateDialogContentElement()
    {
        _chkCloseToolAfterSaving = new CheckBox();

        // default selection section
        var lblDefaultSelection = new PhTextBlock
        {
            LangKey = LangId.Tool_Crop_LblDefaultSelection,
            Opacity = 0.6,
            Margin = new Thickness(0, 8, 0, 0),
        };

        _cmbSelectionType = new ComboBox
        {
            MinWidth = 200,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        _cmbSelectionType.SelectionChanged += (_, _) => UpdateSelectionTypeUI();

        _chkAutoCenterSelection = new CheckBox
        {
            Margin = new Thickness(0, 4, 0, 0),
        };
        _chkAutoCenterSelection.IsCheckedChanged += (_, _) =>
        {
            _numX.IsEnabled = _chkAutoCenterSelection.IsChecked != true;
            _numY.IsEnabled = _chkAutoCenterSelection.IsChecked != true;
        };

        // custom area: location
        var lblLocation = new PhTextBlock
        {
            LangKey = LangId.Tool_Crop_LblLocation,
            Opacity = 0.6,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _numX = new NumericUpDown
        {
            ParsingNumberStyle = NumberStyles.Integer,
            ShowButtonSpinner = false,
            MinWidth = 80,
        };
        _numY = new NumericUpDown
        {
            ParsingNumberStyle = NumberStyles.Integer,
            ShowButtonSpinner = false,
            MinWidth = 80,
        };

        // custom area: size
        var lblSize = new PhTextBlock
        {
            LangKey = LangId.Tool_Crop_LblSize,
            Opacity = 0.6,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _numWidth = new NumericUpDown
        {
            ParsingNumberStyle = NumberStyles.Integer,
            ShowButtonSpinner = false,
            MinWidth = 80,
        };
        _numHeight = new NumericUpDown
        {
            ParsingNumberStyle = NumberStyles.Integer,
            ShowButtonSpinner = false,
            MinWidth = 80,
        };

        _panelCustomArea = new Grid
        {
            ColumnDefinitions = new("Auto, Auto, Auto"),
            RowDefinitions = new("Auto, Auto"),
            ColumnSpacing = 6,
            RowSpacing = 6,
        };
        _panelCustomArea.Children.AddRange([
            lblLocation, _numX, _numY,
            lblSize, _numWidth, _numHeight,
        ]);
        Grid.SetColumn(lblLocation, 0);
        Grid.SetColumn(_numX, 1);
        Grid.SetColumn(_numY, 2);
        Grid.SetColumn(lblSize, 0);
        Grid.SetColumn(_numWidth, 1);
        Grid.SetColumn(_numHeight, 2);
        Grid.SetRow(lblLocation, 0);
        Grid.SetRow(_numX, 0);
        Grid.SetRow(_numY, 0);
        Grid.SetRow(lblSize, 1);
        Grid.SetRow(_numWidth, 1);
        Grid.SetRow(_numHeight, 1);


        // root
        var root = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            MinWidth = 300,
        };

        root.Children.AddRange([
            _chkCloseToolAfterSaving,
            lblDefaultSelection,
            _cmbSelectionType,
            _chkAutoCenterSelection,
            _panelCustomArea,
        ]);

        return root;
    }

    #endregion // Private Methods

}
