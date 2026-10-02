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
using ImageGlass.Common.Types;

namespace ImageGlass.UI;


/// <summary>
/// Semantic color variants for <see cref="PhChip"/>.
/// </summary>
public enum PhChipVariant
{
    Neutral,
    Success,
    Warning,
    Danger,
    Info,
    Accent,
}


/// <summary>
/// A small rounded badge ("chip") whose fill and text color follow the app theme via the
/// situational <see cref="Resx"/> resources selected by <see cref="Variant"/>. Colors are bound
/// with dynamic resources, so the chip re-colors automatically when the theme changes.
/// </summary>
public class PhChip : PhControl
{
    // the shape of a chip, for controls that draw chips themselves
    public const double CHIP_CORNER_RADIUS = 4;
    public static readonly Thickness CHIP_PADDING = new(7, 1);

    private readonly Border _border;
    private readonly TextBlock _label;


    /// <summary>
    /// Gets, sets the chip text.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<PhChip, string?>(nameof(Text));


    /// <summary>
    /// Gets, sets the semantic color variant.
    /// </summary>
    public PhChipVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
    public static readonly StyledProperty<PhChipVariant> VariantProperty =
        AvaloniaProperty.Register<PhChip, PhChipVariant>(nameof(Variant), PhChipVariant.Neutral);


    public PhChip()
    {
        _label = new TextBlock
        {
            FontSize = Const.FONT_SIZE_SMALL,
            IsTabStop = false,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _label[!TextBlock.TextProperty] = this[!TextProperty];

        _border = new Border
        {
            CornerRadius = new CornerRadius(CHIP_CORNER_RADIUS),
            Padding = CHIP_PADDING,
            Child = _label,
        };

        // hug the content instead of stretching to fill the parent
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        Content = _border;

        ApplyVariant(Variant);
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == VariantProperty)
        {
            ApplyVariant(Variant);
        }
    }


    /// <summary>
    /// Binds the fill and text color to the themed resources for the current variant. Neutral and
    /// Info have no dedicated text brush, so their text inherits the default (slightly muted) color.
    /// </summary>
    private void ApplyVariant(PhChipVariant variant)
    {
        ResxId bgId;
        ResxId? fgId;

        switch (variant)
        {
            case PhChipVariant.Success:
                bgId = ResxId.IG_BackgroundSuccessBrush;
                fgId = ResxId.IG_TextSuccessBrush;
                break;
            case PhChipVariant.Warning:
                bgId = ResxId.IG_BackgroundWarningBrush;
                fgId = ResxId.IG_TextWarningBrush;
                break;
            case PhChipVariant.Danger:
                bgId = ResxId.IG_BackgroundDangerBrush;
                fgId = ResxId.IG_TextDangerBrush;
                break;
            case PhChipVariant.Info:
                bgId = ResxId.IG_BackgroundInfoBrush;
                fgId = null;
                break;
            case PhChipVariant.Accent:
                bgId = ResxId.IG_AccentBrush;
                fgId = ResxId.AccentButtonForeground;
                break;
            default: // Neutral
                bgId = ResxId.IG_BackgroundNeutralBrush;
                fgId = null;
                break;
        }

        _border[!Border.BackgroundProperty] = Resx.CreateBinding(bgId);

        if (fgId is not null)
        {
            _label[!TextBlock.ForegroundProperty] = Resx.CreateBinding(fgId.Value);
            _label.Opacity = 1;
        }
        else
        {
            _label.ClearValue(TextBlock.ForegroundProperty);
            _label.Opacity = 0.75;
        }
    }
}
