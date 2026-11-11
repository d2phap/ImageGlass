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
using Avalonia.Controls;
using Avalonia.Layout;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.UI.Windowing;

namespace ImageGlass.Tools;

internal class ColorPickerSettingsWindow : DialogWindow
{
    private CheckBox _chkShowRgbA = null!;
    private CheckBox _chkShowHexA = null!;
    private CheckBox _chkShowHslA = null!;
    private CheckBox _chkShowHsvA = null!;
    private CheckBox _chkShowCmykA = null!;
    private CheckBox _chkShowCIELabA = null!;


    /// <summary>
    /// Gets a snapshot of the edited config values.
    /// </summary>
    public ColorPickerConfig ResultConfig { get; private set; } = new();


    public ColorPickerSettingsWindow(ColorPickerConfig config)
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

        Title = Core.Lang[LangId.Tool_ColorPicker_Title];
        Button1Text = Core.Lang[LangId._OK];
        Button2Text = Core.Lang[LangId._Cancel];

        _chkShowRgbA.Content = Core.Lang[LangId.Tool_ColorPicker_ChkShowRgbA];
        _chkShowHexA.Content = Core.Lang[LangId.Tool_ColorPicker_ChkShowHexA];
        _chkShowHslA.Content = Core.Lang[LangId.Tool_ColorPicker_ChkShowHslA];
        _chkShowHsvA.Content = Core.Lang[LangId.Tool_ColorPicker_ChkShowHsvA];
        _chkShowCmykA.Content = Core.Lang[LangId.Tool_ColorPicker_ChkShowCmykA];
        _chkShowCIELabA.Content = Core.Lang[LangId.Tool_ColorPicker_ChkShowCIELabA];
    }


    protected override void OnDialogSubmitted(DialogEventArgs e)
    {
        ResultConfig = new ColorPickerConfig
        {
            ShowRgbWithAlpha = _chkShowRgbA.IsChecked == true,
            ShowHexWithAlpha = _chkShowHexA.IsChecked == true,
            ShowHslWithAlpha = _chkShowHslA.IsChecked == true,
            ShowHsvWithAlpha = _chkShowHsvA.IsChecked == true,
            ShowCmykWithAlpha = _chkShowCmykA.IsChecked == true,
            ShowCIELabWithAlpha = _chkShowCIELabA.IsChecked == true,
        };

        base.OnDialogSubmitted(e);
    }

    #endregion // Override Methods



    #region Private Methods

    /// <summary>
    /// Loads the current config values into the checkboxes.
    /// </summary>
    private void LoadConfigValues(ColorPickerConfig config)
    {
        _chkShowRgbA.IsChecked = config.ShowRgbWithAlpha;
        _chkShowHexA.IsChecked = config.ShowHexWithAlpha;
        _chkShowHslA.IsChecked = config.ShowHslWithAlpha;
        _chkShowHsvA.IsChecked = config.ShowHsvWithAlpha;
        _chkShowCmykA.IsChecked = config.ShowCmykWithAlpha;
        _chkShowCIELabA.IsChecked = config.ShowCIELabWithAlpha;
    }


    /// <summary>
    /// Creates dialog content with checkboxes for all color format settings.
    /// </summary>
    private StackPanel CreateDialogContentElement()
    {
        _chkShowRgbA = new CheckBox();
        _chkShowHexA = new CheckBox();
        _chkShowHslA = new CheckBox();
        _chkShowHsvA = new CheckBox();
        _chkShowCmykA = new CheckBox();
        _chkShowCIELabA = new CheckBox();

        var root = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            MinWidth = 300,
        };

        root.Children.AddRange([
            _chkShowRgbA,
            _chkShowHexA,
            _chkShowHslA,
            _chkShowHsvA,
            _chkShowCmykA,
            _chkShowCIELabA,
        ]);

        return root;
    }

    #endregion // Private Methods


}
