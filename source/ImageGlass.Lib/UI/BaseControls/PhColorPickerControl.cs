/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI.Windowing;
using System;

namespace ImageGlass.UI;


/// <summary>
/// A reusable color-picker widget: a swatch button (checkerboard behind the selected color),
/// a hex label, and an optional reset link. Clicking the swatch opens <see cref="PhColorPickerDialog"/>;
/// the reset link restores <see cref="DefaultColor"/>. Raises <see cref="ColorChanged"/> whenever
/// <see cref="SelectedColor"/> changes (including programmatic changes).
/// </summary>
public class PhColorPickerControl : PhControl
{
    private Border _swatchBox = null!;
    private Border _swatch = null!;
    private SelectableTextBlock _hexLabel = null!;
    private PhButton _resetButton = null!;


    /// <summary>
    /// Occurs when <see cref="SelectedColor"/> changes.
    /// </summary>
    public event TEventHandler<PhColorPickerControl, EventArgs>? ColorChanged;


    #region Public Properties

    /// <summary>
    /// Gets, sets the currently selected color.
    /// </summary>
    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<PhColorPickerControl, Color>(nameof(SelectedColor));


    /// <summary>
    /// Gets, sets the color restored by the reset link and offered as the dialog's reset value.
    /// </summary>
    public Color DefaultColor
    {
        get => GetValue(DefaultColorProperty);
        set => SetValue(DefaultColorProperty, value);
    }
    public static readonly StyledProperty<Color> DefaultColorProperty =
        AvaloniaProperty.Register<PhColorPickerControl, Color>(nameof(DefaultColor));


    /// <summary>
    /// Gets, sets the title shown on the color-picker dialog.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<PhColorPickerControl, string>(nameof(Title), string.Empty);


    /// <summary>
    /// Gets, sets the value indicates that the reset link is visible.
    /// </summary>
    public bool ShowResetButton
    {
        get => GetValue(ShowResetButtonProperty);
        set => SetValue(ShowResetButtonProperty, value);
    }
    public static readonly StyledProperty<bool> ShowResetButtonProperty =
        AvaloniaProperty.Register<PhColorPickerControl, bool>(nameof(ShowResetButton), true);


    /// <summary>
    /// Gets, sets the value indicates that the hex code of the color is shown next to the swatch.
    /// </summary>
    public bool ShowHexLabel
    {
        get => GetValue(ShowHexLabelProperty);
        set => SetValue(ShowHexLabelProperty, value);
    }
    public static readonly StyledProperty<bool> ShowHexLabelProperty =
        AvaloniaProperty.Register<PhColorPickerControl, bool>(nameof(ShowHexLabel), true);


    /// <summary>
    /// Gets, sets the width of the color swatch.
    /// </summary>
    public double SwatchWidth
    {
        get => GetValue(SwatchWidthProperty);
        set => SetValue(SwatchWidthProperty, value);
    }
    public static readonly StyledProperty<double> SwatchWidthProperty =
        AvaloniaProperty.Register<PhColorPickerControl, double>(nameof(SwatchWidth), 80);

    #endregion // Public Properties



    public PhColorPickerControl()
    {
        Content = BuildContent();
        UpdateSwatch();
    }



    #region Control Events

    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();
        _resetButton.Text = Core.Lang[LangId._Reset];
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == SelectedColorProperty)
        {
            UpdateSwatch();
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (e.Property == ShowResetButtonProperty)
        {
            _resetButton.IsVisible = ShowResetButton;
        }
        else if (e.Property == ShowHexLabelProperty)
        {
            _hexLabel.IsVisible = ShowHexLabel;
        }
        else if (e.Property == SwatchWidthProperty)
        {
            _swatchBox.Width = SwatchWidth;
        }
    }

    #endregion // Control Events



    #region Methods

    /// <summary>
    /// Builds the swatch button (checkerboard + color), the hex label, and the reset link.
    /// </summary>
    private StackPanel BuildContent()
    {
        // checkerboard (for alpha) underneath the selected color
        var checker = new Border { CornerRadius = new CornerRadius(3) };
        checker[!BackgroundProperty] = new DynamicResourceExtension("ColorControlCheckeredBackgroundBrush");

        _swatch = new Border { CornerRadius = new CornerRadius(3) };

        _swatchBox = new Border
        {
            Width = SwatchWidth,
            Height = 20,
            CornerRadius = new CornerRadius(4),
            Child = new Grid { Children = { checker, _swatch } },
        };

        var swatchButton = new PhButton
        {
            Padding = new Thickness(6),
            Content = _swatchBox,
        };
        swatchButton[!CornerRadiusProperty] = Resx.CreateBinding(ResxId.ControlCornerRadius);
        swatchButton.Click += async (_, _) => await PickColorAsync();

        _hexLabel = new SelectableTextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily(Const.FONT_CODE),
            IsVisible = ShowHexLabel,
        };

        _resetButton = new PhButton
        {
            VerticalAlignment = VerticalAlignment.Center,
            Variant = PhButtonVariant.Link,
            Text = Core.Lang[LangId._Reset],
            IsVisible = ShowResetButton,
        };
        _resetButton.Click += (_, _) => SelectedColor = DefaultColor;

        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Orientation = Orientation.Horizontal,
            Spacing = 10,
        };
        panel.Children.AddRange([swatchButton, _hexLabel, _resetButton]);

        return panel;
    }


    /// <summary>
    /// Opens the color-picker dialog and applies the chosen color.
    /// </summary>
    private async System.Threading.Tasks.Task PickColorAsync()
    {
        var dialog = new PhColorPickerDialog(SelectedColor, DefaultColor) { Title = Title };
        if (await dialog.ShowAsync(TopLevel.GetTopLevel(this) as PhWindow) == DialogExitCode.OK)
        {
            SelectedColor = dialog.SelectedColor;
        }
    }


    /// <summary>
    /// Repaints the swatch and refreshes the hex label from <see cref="SelectedColor"/>.
    /// </summary>
    private void UpdateSwatch()
    {
        _swatch.Background = new SolidColorBrush(SelectedColor);
        _hexLabel.Text = SelectedColor.ToHex();
    }

    #endregion // Methods

}
