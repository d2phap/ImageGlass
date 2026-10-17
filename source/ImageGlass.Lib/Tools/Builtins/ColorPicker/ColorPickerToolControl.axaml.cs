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
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Viewer;
using ImageGlass.UI.Windowing;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageGlass.Tools;

public partial class ColorPickerToolControl : PhControl, IToolControl
{
    public static string TOOL_ID => "Tool_ColorPicker";
    public string ToolId => TOOL_ID;
    public string ToolName => Core.Lang[LangId.Menu_MnuColorPicker];
    public ResxIconId? ToolIcon => ResxIconId.IconToolColorPicker;
    public bool HasSettingsUI => true;
    public object? Settings { get; private set; } = new ColorPickerConfig();
    public ColorPickerConfig Options => (ColorPickerConfig)Settings!;
    public ViewerControl Viewer { get; set; } = null!;



    #region Public Properties

    /// <summary>
    /// Gets the tooltip text displayed for the copy button.
    /// </summary>
    public string CopyButtonTooltipText
    {
        get => GetValue(CopyButtonTooltipTextProperty);
        private set => SetValue(CopyButtonTooltipTextProperty, value);
    }
    public static readonly StyledProperty<string> CopyButtonTooltipTextProperty =
        AvaloniaProperty.Register<ColorPickerToolControl, string>(nameof(CopyButtonTooltipText));


    /// <summary>
    /// Gets, sets the current point.
    /// </summary>
    public Point? CurrentPoint
    {
        get => GetValue(CurrentPointProperty);
        set => SetValue(CurrentPointProperty, value);
    }
    public static readonly StyledProperty<Point?> CurrentPointProperty =
        AvaloniaProperty.Register<ColorPickerToolControl, Point?>(nameof(CurrentPoint));


    /// <summary>
    /// Gets, sets the selected point.
    /// </summary>
    public Point? SelectedPoint
    {
        get => GetValue(SelectedPointProperty);
        set => SetValue(SelectedPointProperty, value);
    }
    public static readonly StyledProperty<Point?> SelectedPointProperty =
        AvaloniaProperty.Register<ColorPickerToolControl, Point?>(nameof(SelectedPoint));


    /// <summary>
    /// Gets, sets the current text color.
    /// </summary>
    public Color CurrentTextColor => GetPositionTextColor(CurrentColor);
    public static readonly DirectProperty<ColorPickerToolControl, Color> CurrentTextColorProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerToolControl, Color>(nameof(CurrentTextColor), i => i.CurrentTextColor);


    /// <summary>
    /// Gets, sets the selected text color.
    /// </summary>
    public Color SelectedTextColor => GetPositionTextColor(SelectedColor);
    public static readonly DirectProperty<ColorPickerToolControl, Color> SelectedTextColorProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerToolControl, Color>(nameof(SelectedTextColor), i => i.SelectedTextColor);


    /// <summary>
    /// Gets, sets the current color.
    /// </summary>
    public Color CurrentColor
    {
        get => GetValue(CurrentColorProperty);
        set => SetValue(CurrentColorProperty, value);
    }
    public static readonly StyledProperty<Color> CurrentColorProperty =
        AvaloniaProperty.Register<ColorPickerToolControl, Color>(nameof(CurrentColor), Const.COLOR_EMPTY);


    /// <summary>
    /// Gets, sets the selected color.
    /// </summary>
    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<ColorPickerToolControl, Color>(nameof(SelectedColor));





    /// <summary>
    /// Gets the selected color in RGB format.
    /// </summary>
    public string? ColorRGB => FormatColor(ColorFormat.RGB, Options.ShowRgbWithAlpha);
    public static readonly DirectProperty<ColorPickerToolControl, string?> ColorRGBProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerToolControl, string?>(nameof(ColorRGB), i => i.ColorRGB);


    /// <summary>
    /// Gets the selected color in HEX format.
    /// </summary>
    public string? ColorHEX => FormatColor(ColorFormat.HEX, Options.ShowHexWithAlpha);
    public static readonly DirectProperty<ColorPickerToolControl, string?> ColorHEXProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerToolControl, string?>(nameof(ColorHEX), i => i.ColorHEX);


    /// <summary>
    /// Gets the selected color in HSL format.
    /// </summary>
    public string? ColorHSL => FormatColor(ColorFormat.HSL, Options.ShowHslWithAlpha);
    public static readonly DirectProperty<ColorPickerToolControl, string?> ColorHSLProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerToolControl, string?>(nameof(ColorHSL), i => i.ColorHSL);


    /// <summary>
    /// Gets the selected color in HSV format.
    /// </summary>
    public string? ColorHSV => FormatColor(ColorFormat.HSV, Options.ShowHsvWithAlpha);
    public static readonly DirectProperty<ColorPickerToolControl, string?> ColorHSVProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerToolControl, string?>(nameof(ColorHSV), i => i.ColorHSV);


    /// <summary>
    /// Gets the selected color in CMYK format.
    /// </summary>
    public string? ColorCMYK => FormatColor(ColorFormat.CMYK, Options.ShowCmykWithAlpha);
    public static readonly DirectProperty<ColorPickerToolControl, string?> ColorCMYKProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerToolControl, string?>(nameof(ColorCMYK), i => i.ColorCMYK);


    /// <summary>
    /// Gets the selected color in CIELAB format.
    /// </summary>
    public string? ColorCIELAB => FormatColor(ColorFormat.CIELAB, Options.ShowCIELabWithAlpha);
    public static readonly DirectProperty<ColorPickerToolControl, string?> ColorCIELABProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerToolControl, string?>(nameof(ColorCIELAB), i => i.ColorCIELAB);


    #endregion // Public Properties



    public ColorPickerToolControl()
    {
        InitializeComponent();
    }



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        RaiseColorFormatPropertiesChanged();

        Viewer.ViewerPointerMoved += Viewer_ViewerPointerMoved;
        Viewer.ViewerPointerPressed += Viewer_ViewerPointerPressed;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Viewer.ViewerPointerMoved -= Viewer_ViewerPointerMoved;
        Viewer.ViewerPointerPressed -= Viewer_ViewerPointerPressed;
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == CurrentColorProperty)
        {
            RaisePropertyChanged(CurrentTextColorProperty, default, CurrentTextColor);
        }
        else if (e.Property == SelectedColorProperty)
        {
            RaisePropertyChanged(SelectedTextColorProperty, default, SelectedTextColor);
            RaiseColorFormatPropertiesChanged();
        }
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        CopyButtonTooltipText = Core.Lang[LangId._Copy];
    }


    private void ButtonCopy_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not PhToolButton btn) return;
        if (btn.Content is not string colorCode) return;

        var top = TopLevel.GetTopLevel(this)?.Clipboard;
        _ = top?.SetTextAsync(colorCode);
    }


    private void Viewer_ViewerPointerMoved(ViewerControl sender, ViewerPointerEventArgs e)
    {
        var isValidSrcPoint = new Rect(sender.BitmapSize).Contains(e.SourcePoint.ToPoint(1));
        if (!isValidSrcPoint)
        {
            CurrentPoint = null;
            CurrentColor = Const.COLOR_EMPTY;
            return;
        }

        CurrentPoint = e.SourcePoint.ToPoint(1);
        CurrentColor = sender.GetColorAt(e.SourcePoint.X, e.SourcePoint.Y);
    }


    private void Viewer_ViewerPointerPressed(ViewerControl sender, ViewerPointerEventArgs e)
    {
        var isValidSrcPoint = new Rect(sender.BitmapSize).Contains(e.SourcePoint.ToPoint(1));
        if (!isValidSrcPoint) return;

        SelectedPoint = e.SourcePoint.ToPoint(1);
        SelectedColor = sender.GetColorAt(e.SourcePoint.X, e.SourcePoint.Y);
    }


    #endregion // Control Events



    #region Control Methods

    /// <summary>
    /// Determines the appropriate text color to use based on the specified background color.
    /// </summary>
    private Color GetPositionTextColor(Color c)
    {
        if (c.IsEmpty) return Core.Theme.InvertedBaseColor;
        if (c.A < 130) return Core.Theme.BaseColor.InvertBlackOrWhite();
        return CurrentColor.InvertBlackOrWhite();
    }


    /// <summary>
    /// Formats the currently selected color as a string in the specified color format.
    /// </summary>
    private string FormatColor(ColorFormat format, bool includeAlpha = true)
    {
        var skipAlpha = !includeAlpha;
        var code = format switch
        {
            ColorFormat.RGB => SelectedColor.ToRgbaString(skipAlpha),
            ColorFormat.HEX => SelectedColor.ToHex(skipAlpha),
            ColorFormat.HSL => SelectedColor.ToHslString(skipAlpha),
            ColorFormat.HSV => SelectedColor.ToHsvString(skipAlpha),
            ColorFormat.CMYK => SelectedColor.ToCmykString(skipAlpha),
            ColorFormat.CIELAB => SelectedColor.ToCIELABString(skipAlpha),
            _ => string.Empty,
        };

        return code;
    }


    /// <summary>
    /// Raises property changed notifications for all color format properties.
    /// </summary>
    private void RaiseColorFormatPropertiesChanged()
    {
        RaisePropertyChanged(ColorRGBProperty, default, ColorRGB);
        RaisePropertyChanged(ColorHEXProperty, default, ColorHEX);
        RaisePropertyChanged(ColorHSLProperty, default, ColorHSL);
        RaisePropertyChanged(ColorHSVProperty, default, ColorHSV);
        RaisePropertyChanged(ColorCMYKProperty, default, ColorCMYK);
        RaisePropertyChanged(ColorCIELABProperty, default, ColorCIELAB);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void LoadSettings(JsonElement? jsonEl)
    {
        var settings = jsonEl?.Deserialize(ColorPickerConfigJsonContext.Default.ColorPickerConfig);
        if (settings is not null)
        {
            Settings = settings;
        }
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public JsonElement? SaveSettings()
    {
        var jsonEl = JsonSerializer.SerializeToElement(Options, ColorPickerConfigJsonContext.Default.ColorPickerConfig);

        return jsonEl;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task ShowSettingsWindowAsync()
    {
        var window = new ColorPickerSettingsWindow(Options);
        var owner = TopLevel.GetTopLevel(this) as PhWindow;
        var result = await window.ShowAsync(owner);

        if (result == DialogExitCode.OK)
        {
            Settings = window.ResultConfig;
            RaiseColorFormatPropertiesChanged();
        }
    }

    #endregion // Control Methods


}



public enum ColorFormat
{
    RGB,
    HEX,
    HSL,
    HSV,
    CMYK,
    CIELAB,
}

