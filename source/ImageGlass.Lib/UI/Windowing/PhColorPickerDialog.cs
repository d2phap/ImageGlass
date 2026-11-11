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
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using System;
using System.Globalization;

namespace ImageGlass.UI.Windowing;

public partial class PhColorPickerDialog : DialogWindow
{
    private const double SPECTRUM_SIZE = 280;
    private const double SLIDER_WIDTH = 20;
    private const double INPUT_WIDTH = 100;

    private ColorSpectrum _colorSpectrum = null!;
    private ColorSlider _colorSlider = null!;
    private ColorSlider _alphaSlider = null!;
    private TextBox _txtHex = null!;
    private NumericUpDown _numR = null!;
    private NumericUpDown _numG = null!;
    private NumericUpDown _numB = null!;
    private NumericUpDown _numA = null!;
    private Grid _currentColorPreview = null!;
    private Grid _previousColorPreview = null!;
    private SolidColorBrush _currentColorPreviewBrush = null!;
    private SolidColorBrush _previousColorPreviewBrush = null!;
    private PhButton _btnReset = null!;
    private Color _previousColor = Const.COLOR_EMPTY;
    private HsvColor _selectedHsvColor = Const.COLOR_EMPTY.ToHsv();
    private bool _isUpdatingColor;


    #region Public Properties

    /// <summary>
    /// Gets, sets the selected color.
    /// </summary>
    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<PhColorPickerDialog, Color>(nameof(SelectedColor), Const.COLOR_EMPTY);


    /// <summary>
    /// Gets, sets the default color.
    /// </summary>
    public Color DefaultColor
    {
        get => GetValue(DefaultColorProperty);
        set => SetValue(DefaultColorProperty, value);
    }
    public static readonly StyledProperty<Color> DefaultColorProperty =
        AvaloniaProperty.Register<PhColorPickerDialog, Color>(nameof(DefaultColor), Const.COLOR_EMPTY);

    #endregion // Public Properties



    public PhColorPickerDialog(Color? initColor = null, Color? defaultColor = null)
    {
        // Configure the modal buttons and default focus behavior.
        Title = string.Empty;
        IsButton1Visible = true;
        IsButton2Visible = true;
        IsButton3Visible = false;
        DefaultButton = DialogButton.Button1;
        DefaultFocus = DialogFocus.Default;

        // Capture the initial color as both the selected and previous color.
        if (initColor != null)
        {
            SelectedColor = initColor.Value;
            _previousColor = initColor.Value;
            _selectedHsvColor = initColor.Value.ToHsv();
        }
        if (defaultColor != null) DefaultColor = defaultColor.Value;

        // Build the visual tree, then push the initial color into every picker control.
        DialogContent = CreateDialogContentElement();
        DialogFooterLeftContent = CreateDialogFooterLeftContentElement();
        UpdatePickerControlsFromSelectedColor();
    }



    #region Override methods

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // Keep the composed controls in sync when SelectedColor changes externally.
        if (e.Property == SelectedColorProperty && !_isUpdatingColor)
        {
            UpdatePickerControlsFromSelectedColor();
        }
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        // Refresh all localized dialog captions.
        Button1Text = Core.Lang[LangId._OK];
        Button2Text = Core.Lang[LangId._Cancel];
        _btnReset.Text = Core.Lang[LangId._Reset];
    }


    protected override void OnDialogSubmitted(DialogEventArgs e)
    {
        // Commit any pending HEX edit before the dialog result is consumed.
        CommitHexTextInput();

        base.OnDialogSubmitted(e);
    }


    #endregion // Override methods



    #region Private methods

    private Grid CreateDialogContentElement()
    {
        // Build the current/previous color preview strip.
        _currentColorPreview = CreateColorPreview(
            SelectedColor,
            out _currentColorPreviewBrush);
        _previousColorPreview = CreateColorPreview(
            _previousColor,
            out _previousColorPreviewBrush);

        var previewGrid = new Grid
        {
            ColumnDefinitions = new("*,*"),
        };
        var previewBorder = new Border
        {
            Child = previewGrid,
            ClipToBounds = true,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(1),
            Margin = new Thickness(0, 0, 0, 8),
            Height = 40,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,

            [!Border.BorderBrushProperty] = Resx.CreateBinding(ResxId.IG_BorderControlBrush),
        };
        previewGrid.Children.Add(_currentColorPreview);
        Grid.SetColumn(_previousColorPreview, 1);
        previewGrid.Children.Add(_previousColorPreview);


        // Build the spectrum and the two vertical HSV component sliders.
        _colorSpectrum = new ColorSpectrum
        {
            Width = SPECTRUM_SIZE,
            Height = SPECTRUM_SIZE,
            Components = ColorSpectrumComponents.HueSaturation,
            Shape = ColorSpectrumShape.Box,
            HsvColor = SelectedColor.ToHsv(),
            ClipToBounds = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        _colorSpectrum.ColorChanged += ColorControl_ColorChanged;

        _colorSlider = CreateVerticalColorSlider(ColorComponent.Component3, false);
        _colorSlider.ColorChanged += ColorControl_ColorChanged;
        _alphaSlider = CreateVerticalColorSlider(ColorComponent.Alpha, true);
        _alphaSlider.ColorChanged += ColorControl_ColorChanged;

        // Stack the preview strip over the text/numeric inputs on the right.
        var inputsGrid = CreateInputsGrid();
        var rightPanel = new StackPanel
        {
            Spacing = 12,
        };
        rightPanel.Children.AddRange([previewBorder, inputsGrid]);


        // Lay out the picker surface: spectrum, value slider, alpha slider, and input panel.
        var root = new Grid
        {
            ColumnDefinitions = new($"{SPECTRUM_SIZE},{SLIDER_WIDTH},{SLIDER_WIDTH},Auto"),
            ColumnSpacing = 12,
        };

        var colorSpectrumHost = new Border
        {
            Width = SPECTRUM_SIZE,
            Height = SPECTRUM_SIZE,
            ClipToBounds = true,
            Child = _colorSpectrum,
        };
        root.Children.Add(colorSpectrumHost);

        Grid.SetColumn(_colorSlider, 1);
        root.Children.Add(_colorSlider);

        Grid.SetColumn(_alphaSlider, 2);
        root.Children.Add(_alphaSlider);

        Grid.SetColumn(rightPanel, 3);
        root.Children.Add(rightPanel);

        return root;
    }


    private StackPanel CreateDialogFooterLeftContentElement()
    {
        // Create the reset command in the left footer slot.
        _btnReset = new PhButton
        {
            MinWidth = 80,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        _btnReset.Click += (_, _) =>
        {
            // Restore the dialog selection to the caller-provided default color.
            SelectedColor = DefaultColor;
        };

        // Wrap the reset button so the footer can host more left-side controls later.
        var root = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
        };
        root.Children.Add(_btnReset);

        return root;
    }


    private static Grid CreateColorPreview(Color color, out SolidColorBrush colorBrush)
    {
        // Keep a mutable color brush so updates do not rebuild the preview visual tree.
        colorBrush = new SolidColorBrush(color);

        // Layer the checkerboard underneath the selected color for alpha visibility.
        return new Grid
        {
            Children =
                {
                    new Border
                    {
                        [!Border.BackgroundProperty] = new DynamicResourceExtension("ColorControlCheckeredBackgroundBrush"),
                    },
                    new Border
                    {
                        Background = colorBrush,
                    },
                },
        };
    }


    private static ColorSlider CreateVerticalColorSlider(ColorComponent component, bool isAlphaVisible)
    {
        // Create a vertical ColorPicker slider for a single HSV component.
        return new ColorSlider
        {
            Width = SLIDER_WIDTH,
            Height = SPECTRUM_SIZE,
            BorderThickness = new Thickness(1),
            Orientation = Orientation.Vertical,
            ColorModel = ColorModel.Hsva,
            ColorComponent = component,
            IsAlphaVisible = isAlphaVisible,
            IsPerceptive = true,
            HsvColor = Colors.White.ToHsv(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,

            [!TemplatedControl.BorderBrushProperty] = Resx.CreateBinding(ResxId.IG_BorderControlBrush),
        };
    }


    private Grid CreateInputsGrid()
    {
        // Create the HEX input. It commits on blur rather than on each keystroke.
        _txtHex = new TextBox
        {
            MinWidth = INPUT_WIDTH,
            MaxLength = 9,
            Text = SelectedColor.ToHex(),
            TextAlignment = TextAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        _txtHex.LostFocus += HexTextBox_LostFocus;

        // Create the byte inputs for RGBA channels.
        _numR = CreateByteInput();
        _numG = CreateByteInput();
        _numB = CreateByteInput();
        _numA = CreateByteInput();

        // Build the two-column form: right-aligned labels and fixed-width inputs.
        var root = new Grid
        {
            ColumnDefinitions = new($"Auto,{INPUT_WIDTH}"),
            RowDefinitions = new("Auto,Auto,Auto,Auto,Auto"),
            ColumnSpacing = 8,
            RowSpacing = 8,
            VerticalAlignment = VerticalAlignment.Top,
        };

        AddInputRow(root, 0, "HEX", _txtHex);
        AddInputRow(root, 1, "R", _numR);
        AddInputRow(root, 2, "G", _numG);
        AddInputRow(root, 3, "B", _numB);
        AddInputRow(root, 4, "A", _numA);

        return root;
    }


    private NumericUpDown CreateByteInput()
    {
        // Clamp all component inputs to byte range values.
        var input = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 255,
            Increment = 1,
            ClipValueToMinMax = true,
            FormatString = "N0",
            ParsingNumberStyle = NumberStyles.Integer,
            ShowButtonSpinner = false,
            MinWidth = INPUT_WIDTH,
            TextAlignment = TextAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
        };

        // React to user edits and keep the rest of the picker synchronized.
        input.ValueChanged += ComponentInput_ValueChanged;

        return input;
    }


    private static void AddInputRow(Grid root, int row, string label, Control input)
    {
        // Create a compact, right-aligned label for the input row.
        var labelBlock = new TextBlock
        {
            Text = label,
            Opacity = 0.65,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };

        // Add the label in column 0.
        Grid.SetRow(labelBlock, row);
        root.Children.Add(labelBlock);

        // Add the input in column 1.
        Grid.SetColumn(input, 1);
        Grid.SetRow(input, row);
        root.Children.Add(input);
    }


    private void ColorControl_ColorChanged(object? sender, ColorChangedEventArgs e)
    {
        // Ignore feedback loops caused by programmatic synchronization.
        if (_isUpdatingColor) return;

        // Preserve HSV precision by reading the source control's HSV value directly.
        var hsvColor = sender switch
        {
            ColorSpectrum colorSpectrum => colorSpectrum.HsvColor,
            ColorSlider colorSlider => colorSlider.HsvColor,
            _ => e.NewColor.ToHsv(),
        };

        // Push the new HSV value through the whole composed picker.
        SetSelectedHsvColorFromInput(hsvColor);
    }


    private void ComponentInput_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        // Ignore feedback loops caused by programmatic synchronization.
        if (_isUpdatingColor) return;

        // Build a new RGB color from the four numeric inputs.
        var currentColor = SelectedColor;
        var color = Color.FromArgb(
            GetByteInputValue(_numA, currentColor.A),
            GetByteInputValue(_numR, currentColor.R),
            GetByteInputValue(_numG, currentColor.G),
            GetByteInputValue(_numB, currentColor.B));

        // RGB inputs intentionally reset HSV from the resulting color.
        SetSelectedColorFromInput(color);
    }


    private void HexTextBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        // Treat focus loss as the commit point for freeform HEX text.
        CommitHexTextInput();
    }


    private void CommitHexTextInput()
    {
        // Avoid re-entering while another control is updating the HEX text.
        if (_isUpdatingColor) return;

        // Apply a valid HEX value through the RGB pathway.
        if (TryParseHexColor(_txtHex.Text, out var color))
        {
            SetSelectedColorFromInput(color);
            return;
        }

        // Revert invalid or incomplete HEX text back to the current selected color.
        UpdatePickerControlsFromSelectedColor();
    }


    private void SetSelectedColorFromInput(Color color)
    {
        // Mark synchronization so property-change callbacks do not bounce back.
        _isUpdatingColor = true;
        try
        {
            // Convert RGB input to HSV, then update the public selected color.
            _selectedHsvColor = color.ToHsv();
            SelectedColor = color;

            // Refresh every visual/input control from the canonical HSV value.
            UpdatePickerControls(_selectedHsvColor);
        }
        finally
        {
            // Always release the synchronization guard.
            _isUpdatingColor = false;
        }
    }


    private void SetSelectedHsvColorFromInput(HsvColor hsvColor)
    {
        // Mark synchronization so property-change callbacks do not bounce back.
        _isUpdatingColor = true;
        try
        {
            // Store HSV directly to avoid marker drift while adjusting value/alpha sliders.
            _selectedHsvColor = hsvColor;
            SelectedColor = hsvColor.ToRgb();

            // Refresh every visual/input control from the canonical HSV value.
            UpdatePickerControls(hsvColor);
        }
        finally
        {
            // Always release the synchronization guard.
            _isUpdatingColor = false;
        }
    }


    private void UpdatePickerControlsFromSelectedColor()
    {
        // Mark synchronization so property-change callbacks do not bounce back.
        _isUpdatingColor = true;
        try
        {
            // Convert the public RGB color into the internal HSV source of truth.
            _selectedHsvColor = SelectedColor.ToHsv();

            // Refresh every visual/input control from the canonical HSV value.
            UpdatePickerControls(_selectedHsvColor);
        }
        finally
        {
            // Always release the synchronization guard.
            _isUpdatingColor = false;
        }
    }


    private void UpdatePickerControls(HsvColor hsvColor)
    {
        // Skip synchronization until all controls have been created.
        if (_colorSpectrum is null
            || _colorSlider is null
            || _alphaSlider is null
            || _txtHex is null
            || _numR is null
            || _numG is null
            || _numB is null
            || _numA is null
            || _currentColorPreviewBrush is null
            || _previousColorPreviewBrush is null)
        {
            return;
        }

        // Convert HSV once for all RGB-oriented text, numeric, and preview controls.
        var color = hsvColor.ToRgb();

        // Synchronize the interactive picker controls.
        _colorSpectrum.HsvColor = hsvColor;
        _colorSlider.HsvColor = hsvColor;
        _alphaSlider.HsvColor = hsvColor;

        // Synchronize the text and numeric inputs.
        _txtHex.Text = color.ToHex();
        _numR.Value = color.R;
        _numG.Value = color.G;
        _numB.Value = color.B;
        _numA.Value = color.A;

        // Synchronize the current/previous color previews.
        _currentColorPreviewBrush.Color = color;
        _previousColorPreviewBrush.Color = _previousColor;
    }


    private static byte GetByteInputValue(NumericUpDown input, byte fallback)
    {
        // Preserve the previous channel value while the numeric input is empty.
        if (input.Value is not { } value) return fallback;

        // Round and clamp decimal input into a valid byte channel.
        var roundedValue = (int)Math.Round(value, MidpointRounding.AwayFromZero);
        return (byte)Math.Clamp(roundedValue, 0, 255);
    }


    private static bool TryParseHexColor(string? text, out Color color)
    {
        // Start with an explicit empty output for invalid input.
        color = Const.COLOR_EMPTY;
        if (string.IsNullOrWhiteSpace(text)) return false;

        // Allow either #RRGGBB/#RRGGBBAA or RRGGBB/RRGGBBAA input.
        var colorText = text.Trim();
        var hex = colorText.StartsWith('#') ? colorText[1..] : colorText;

        // Reject incomplete values before delegating to BHelper.ColorFromHex.
        if (hex.Length is not (3 or 4 or 6 or 8)) return false;

        // Reject invalid hex characters so BHelper.ColorFromHex is only used for valid hex text.
        foreach (var c in hex.AsSpan())
        {
            if (!Uri.IsHexDigit(c)) return false;
        }

        // Use the shared ImageGlass parser to keep HEX semantics consistent with the app.
        color = BHelper.ColorFromHex(colorText, skipAlpha: false);
        return true;
    }

    #endregion // Private methods

}
