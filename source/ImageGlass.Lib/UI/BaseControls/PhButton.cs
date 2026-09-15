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
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Types;
using System;
using System.ComponentModel;

namespace ImageGlass.UI;


public class PhButton : Button
{
    protected override Type StyleKeyOverride => typeof(Button);

    // gap between the icon and the text of the default content
    private static readonly Thickness ICON_GAP = new(0, 0, 4, 0);

    // elements of the default content, kept to toggle the link underline and repaint the icon
    private TextBlock? _textEl;
    private Path? _iconEl;


    // events
    public event TEventHandler<ContextMenu, CancelEventArgs>? DropdownOpening;
    public event TEventHandler<ContextMenu, RoutedEventArgs>? DropdownOpened;
    public event TEventHandler<ContextMenu, CancelEventArgs>? DropdownClosing;
    public event TEventHandler<ContextMenu, RoutedEventArgs>? DropdownClosed;


    #region Public Properties

    /// <summary>
    /// Gets the DPI scale value.
    /// </summary>
    public double Dpi => TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;


    /// <summary>
    /// Gets, sets the icon geometry data.
    /// </summary>
    public Geometry? IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    public static readonly StyledProperty<Geometry?> IconDataProperty =
        AvaloniaProperty.Register<PhButton, Geometry?>(nameof(IconData));


    /// <summary>
    /// Gets, sets the stroke width for an outline icon; <c>0</c> (default) fills the icon instead.
    /// </summary>
    public double IconStrokeThickness
    {
        get => GetValue(IconStrokeThicknessProperty);
        set => SetValue(IconStrokeThicknessProperty, value);
    }
    public static readonly StyledProperty<double> IconStrokeThicknessProperty =
        AvaloniaProperty.Register<PhButton, double>(nameof(IconStrokeThickness));


    /// <summary>
    /// Gets, sets the text of this button.
    /// </summary>
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<PhButton, string>(nameof(Text));


    /// <summary>
    /// Gets, sets how the text is trimmed. Needs a width-constraining parent (e.g. a <c>*</c> column).
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }
    public static readonly StyledProperty<TextTrimming> TextTrimmingProperty =
        AvaloniaProperty.Register<PhButton, TextTrimming>(nameof(TextTrimming), TextTrimming.None);


    /// <summary>
    /// Gets, sets how the text is trimmed. Needs a width-constraining parent (e.g. a <c>*</c> column).
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<PhButton, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);


    /// <summary>
    /// Gets, sets the visual style variant of the button.
    /// </summary>
    public PhButtonVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
    public static readonly StyledProperty<PhButtonVariant> VariantProperty =
        AvaloniaProperty.Register<PhButton, PhButtonVariant>(nameof(Variant), PhButtonVariant.Default);


    /// <summary>
    /// Gets, sets the value indicates that the icon is visible.
    /// </summary>
    public bool IsIconVisible => IconData != null;
    public static readonly DirectProperty<PhButton, bool> IsIconVisibleProperty =
        AvaloniaProperty.RegisterDirect<PhButton, bool>(nameof(IsIconVisible), i => i.IsIconVisible);


    /// <summary>
    /// Gets, sets the value indicates that the text is visible.
    /// </summary>
    public bool IsTextVisible => !string.IsNullOrWhiteSpace(Text);
    public static readonly DirectProperty<PhButton, bool> IsTextVisibleProperty =
        AvaloniaProperty.RegisterDirect<PhButton, bool>(nameof(IsTextVisible), i => i.IsTextVisible);


    /// <summary>
    /// Gets, sets the value indicates that the button is checked on click.
    /// </summary>
    public bool IsCheckOnClick
    {
        get => GetValue(IsCheckOnClickProperty);
        set => SetValue(IsCheckOnClickProperty, value);
    }
    public static readonly StyledProperty<bool> IsCheckOnClickProperty =
        AvaloniaProperty.Register<PhButton, bool>(nameof(IsCheckOnClick));


    /// <summary>
    /// Gets, sets the value indicates that the dropdown menu is open on click.
    /// </summary>
    public bool IsOpenDropdownMenuOnClick
    {
        get => GetValue(IsOpenDropdownMenuOnClickProperty);
        set => SetValue(IsOpenDropdownMenuOnClickProperty, value);
    }
    public static readonly StyledProperty<bool> IsOpenDropdownMenuOnClickProperty =
        AvaloniaProperty.Register<PhButton, bool>(nameof(IsOpenDropdownMenuOnClick));


    /// <summary>
    /// Gets, sets the dropdown menu of this button.
    /// </summary>
    public ContextMenu? DropdownMenu
    {
        get => GetValue(DropdownMenuProperty);
        set
        {
            DropdownMenu?.Opening -= DropdownMenu_Opening;
            DropdownMenu?.Opened -= DropdownMenu_Opened;
            DropdownMenu?.Closing -= DropdownMenu_Closing;
            DropdownMenu?.Closed -= DropdownMenu_Closed;

            SetValue(DropdownMenuProperty, value);
        }
    }
    public static readonly StyledProperty<ContextMenu?> DropdownMenuProperty =
        AvaloniaProperty.Register<PhButton, ContextMenu?>(nameof(DropdownMenu));


    #endregion // Public Properties



    public PhButton()
    {
        Content = CreateContentElement();
    }




    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        OnIgLanguageChanged();
        UpdateOutlineBorderBrush();
        UpdateIconPaint();
        Core.ThemeChanged += Core_ThemeChanged;
        Core.LanguageChanged += Core_LanguageChanged;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Core.ThemeChanged -= Core_ThemeChanged;
        Core.LanguageChanged -= Core_LanguageChanged;
    }


    protected override void OnClick()
    {
        if (IsOpenDropdownMenuOnClick)
        {
            OpenDropdownMenu();
        }

        base.OnClick();
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == DropdownMenuProperty)
        {
            DropdownMenu?.Opening -= DropdownMenu_Opening;
            DropdownMenu?.Opened -= DropdownMenu_Opened;
            DropdownMenu?.Closing -= DropdownMenu_Closing;
            DropdownMenu?.Closed -= DropdownMenu_Closed;

            DropdownMenu?.Opening += DropdownMenu_Opening;
            DropdownMenu?.Opened += DropdownMenu_Opened;
            DropdownMenu?.Closing += DropdownMenu_Closing;
            DropdownMenu?.Closed += DropdownMenu_Closed;
        }
        else if (e.Property == TextProperty)
        {
            RaisePropertyChanged(IsTextVisibleProperty, default, IsTextVisible);
            UpdateContentSpacing();
        }
        else if (e.Property == IconDataProperty)
        {
            RaisePropertyChanged(IsIconVisibleProperty, default, IsIconVisible);
        }
        else if (e.Property == IsPressedProperty || e.Property == IsPointerOverProperty)
        {
            UpdateContentVisual();
        }
        else if (e.Property == ForegroundProperty || e.Property == IconStrokeThicknessProperty)
        {
            UpdateIconPaint();
        }
        else if (e.Property == VariantProperty)
        {
            ApplyVariant();
        }
    }


    /// <summary>
    /// Applies the look of the current <see cref="Variant"/>.
    /// </summary>
    private void ApplyVariant()
    {
        // reset everything a previous variant may have set
        Classes.Remove("accent");
        Classes.Remove("link");
        Classes.Remove("outline");
        ClearValue(ForegroundProperty);
        ClearValue(BackgroundProperty);
        ClearValue(BorderThicknessProperty);
        ClearValue(CursorProperty);

        switch (Variant)
        {
            case PhButtonVariant.Accent:
                Classes.Add("accent");
                this[!ForegroundProperty] = Resx.CreateBinding(ResxId.AccentButtonForeground);
                break;

            case PhButtonVariant.Link:
                Classes.Add("link");
                Background = Brushes.Transparent;
                BorderThickness = new Thickness(0);
                Cursor = new Cursor(StandardCursorType.Hand);
                this[!PaddingProperty] = this[!PaddingProperty];
                this[!ForegroundProperty] = Resx.CreateBinding(ResxId.IG_TextAccentColor);
                break;

            case PhButtonVariant.Outline:
                Classes.Add("outline");
                this[!ForegroundProperty] = Resx.CreateBinding(ResxId.IG_TextAccentColor);
                break;

            case PhButtonVariant.Default:
            default:
                break;
        }

        UpdateOutlineBorderBrush();
        UpdateContentVisual();
    }


    /// <summary>
    /// Builds the Outline border: the accent hue at two alphas (soft top/sides, stronger bottom).
    /// Done in code because SystemAccentColor is opaque, so a XAML gradient can't vary its alpha.
    /// </summary>
    private void UpdateOutlineBorderBrush()
    {
        if (Variant != PhButtonVariant.Outline)
        {
            Resources.Remove("PhOutlineButtonBorderBrush");
            return;
        }

        if (!this.TryFindResource("SystemAccentColor", ActualThemeVariant, out var res)
            || res is not Color accent)
        {
            return;
        }

        var soft = accent.WithAlpha(0x50);
        var strong = accent.WithAlpha(0xA0);

        Resources["PhOutlineButtonBorderBrush"] = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(soft, 0.0),
                new GradientStop(soft, 0.9),
                new GradientStop(strong, 1.0),
            },
        };
    }


    private void DropdownMenu_Opening(object? sender, CancelEventArgs e)
    {
        OnIgDropdownMenuOpening(e);
    }


    private void DropdownMenu_Opened(object? sender, RoutedEventArgs e)
    {
        OnIgDropdownMenuOpened(e);
    }


    private void DropdownMenu_Closing(object? sender, CancelEventArgs e)
    {
        OnIgDropdownMenuClosing(e);
    }


    private void DropdownMenu_Closed(object? sender, RoutedEventArgs e)
    {
        OnIgDropdownMenuClosed(e);
    }


    private void Core_ThemeChanged(object? sender, ThemePackChangedEventArgs e)
    {
        UpdateOutlineBorderBrush();
        OnIgThemeChanged(e);
    }


    private void Core_LanguageChanged(object? sender, EventArgs e)
    {
        OnIgLanguageChanged();
    }

    #endregion // Control Events



    #region Virtual Methods

    /// <summary>
    /// Occurs when the app theme is changed.
    /// </summary>
    protected virtual void OnIgThemeChanged(ThemePackChangedEventArgs e) { }


    /// <summary>
    /// Occurs when the app language is changed.
    /// </summary>
    protected virtual void OnIgLanguageChanged() { }


    /// <summary>
    /// Occurs when the value of the IsOpen property is changing from false to true.
    /// </summary>
    protected virtual void OnIgDropdownMenuOpening(CancelEventArgs e)
    {
        DropdownOpening?.Invoke(DropdownMenu!, e);
    }


    /// <summary>
    /// Occurs when the dropdown menu is opened.
    /// </summary>
    protected virtual void OnIgDropdownMenuOpened(RoutedEventArgs e)
    {
        DropdownOpened?.Invoke(DropdownMenu!, e);
    }


    /// <summary>
    /// Occurs when the value of the IsOpen property is changing from true to false.
    /// </summary>
    protected virtual void OnIgDropdownMenuClosing(CancelEventArgs e)
    {
        DropdownClosing?.Invoke(DropdownMenu!, e);
    }


    /// <summary>
    /// Occurs when the dropdown menu is closed.
    /// </summary>
    protected virtual void OnIgDropdownMenuClosed(RoutedEventArgs e)
    {
        DropdownClosed?.Invoke(DropdownMenu!, e);
    }

    #endregion // Virtual Methods



    #region Control Methods

    /// <summary>
    /// Creates default button content element with icon and text element.
    /// </summary>
    private DockPanel CreateContentElement()
    {
        var pathEl = _iconEl = new Path
        {
            Width = 14,
            Height = 14,
            Stretch = Stretch.Uniform,
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round,
            [!Path.DataProperty] = this[!IconDataProperty],
            [!Path.IsVisibleProperty] = this[!IsIconVisibleProperty],
        };
        DockPanel.SetDock(pathEl, Dock.Left);

        var textEl = _textEl = new TextBlock
        {
            [!TextBlock.TextProperty] = this[!TextProperty],
            [!TextBlock.ForegroundProperty] = this[!ForegroundProperty],
            [!TextBlock.IsVisibleProperty] = this[!IsTextVisibleProperty],
            [!TextBlock.TextTrimmingProperty] = this[!TextTrimmingProperty],
            [!TextBlock.TextWrappingProperty] = this[!TextWrappingProperty],
        };


        // DockPanel, not StackPanel: the filling text gets the leftover width, so it can trim
        var panel = new DockPanel
        {
            // center the content by default (so it stays centered when the button is stretched)
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        panel.Children.AddRange([pathEl, textEl]);

        return panel;
    }


    /// <summary>
    /// Puts the icon-to-text gap on the icon, only while there is text, so an icon-only button stays centered.
    /// </summary>
    private void UpdateContentSpacing()
    {
        if (_iconEl is null) return;

        _iconEl.Margin = IsTextVisible ? ICON_GAP : default;
    }


    /// <summary>
    /// Paints the icon from the button foreground: stroked when <see cref="IconStrokeThickness"/> is set, filled otherwise.
    /// </summary>
    private void UpdateIconPaint()
    {
        if (_iconEl is null) return;

        var isOutlined = IconStrokeThickness > 0;
        _iconEl.StrokeThickness = IconStrokeThickness;
        _iconEl.Fill = isOutlined ? null : Foreground;
        _iconEl.Stroke = isOutlined ? Foreground : null;
    }


    /// <summary>
    /// Press/hover feedback on the content (not the control, so the chrome stays
    /// put): nudge down + dim on press, and an underline on hover/press for links.
    /// </summary>
    private void UpdateContentVisual()
    {
        if (Content is Visual content)
        {
            content.RenderTransform = IsPressed ? new TranslateTransform(0, 0.5) : null;
            content.Opacity = IsPressed ? 0.6 : 1d;
        }

        // link: underline on hover/press only
        if (Variant == PhButtonVariant.Link && _textEl is not null)
        {
            _textEl.TextDecorations = IsPointerOver || IsPressed
                ? TextDecorations.Underline
                : null;
        }
    }


    /// <summary>
    /// Opens dropdown menu of this button if any.
    /// </summary>
    public void OpenDropdownMenu(PlacementMode? placement = null)
    {
        if (DropdownMenu is null) return;

        DropdownMenu.Placement = placement ?? PlacementMode.BottomEdgeAlignedRight;
        DropdownMenu.Open(this);
    }

    #endregion // Control Methods


}


/// <summary>
/// The visual style variant of a <see cref="PhButton"/>.
/// </summary>
public enum PhButtonVariant
{
    /// <summary>
    /// The standard button look.
    /// </summary>
    Default,

    /// <summary>
    /// A filled accent button (accent background + readable foreground).
    /// </summary>
    Accent,

    /// <summary>
    /// A borderless, link-style button (transparent background, accent text, hand cursor).
    /// </summary>
    Link,

    /// <summary>
    /// A bordered button with the default background but an accent border and accent text.
    /// </summary>
    Outline,
}
