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
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using System;

namespace ImageGlass.UI;

public class PhMenuItem : MenuItem
{
    protected override Type StyleKeyOverride => typeof(MenuItem);

    // whether the current disabled state came from a gate, not from the owner
    private bool _disabledByGate;


    #region Public properties

    /// <summary>
    /// Gets, sets the language key for localization.
    /// </summary>
    public LangId? LangKey
    {
        get => GetValue(LangKeyProperty);
        set => SetValue(LangKeyProperty, value);
    }
    public static readonly StyledProperty<LangId?> LangKeyProperty =
        AvaloniaProperty.Register<PhMenuItem, LangId?>(nameof(LangKey));


    /// <summary>
    /// Gets, sets the language param for localization.
    /// </summary>
    public object? LangParams
    {
        get => GetValue(LangParamsProperty);
        set => SetValue(LangParamsProperty, value);
    }
    public static readonly StyledProperty<object?> LangParamsProperty =
        AvaloniaProperty.Register<PhMenuItem, object?>(nameof(LangParams));


    /// <summary>
    /// Gets, sets the display text for hotkeys.
    /// </summary>
    public string? HotkeyText
    {
        get => GetValue(HotkeyTextProperty);
        set => SetValue(HotkeyTextProperty, value);
    }
    public static readonly StyledProperty<string?> HotkeyTextProperty =
        AvaloniaProperty.Register<PhMenuItem, string?>(nameof(HotkeyText));


    /// <summary>
    /// Gets, sets the icon geometry, shown as a <see cref="PathIcon"/> in <see cref="MenuItem.Icon"/>.
    /// </summary>
    public Geometry? IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    public static readonly StyledProperty<Geometry?> IconDataProperty =
        AvaloniaProperty.Register<PhMenuItem, Geometry?>(nameof(IconData));


    #endregion // Public properties


    public PhMenuItem() { }


    public PhMenuItem(object? header = null)
    {
        Header = header;
    }


    #region Control Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // override InputGesture: to accept free text
        if (e.NameScope.Find<TextBlock>("PART_InputGestureText") is not TextBlock gestureEl) return;
        gestureEl.ClearValue(TextBlock.TextProperty);
        gestureEl[!TextBlock.TextProperty] = this[!HotkeyTextProperty];
        gestureEl.FontSize = Const.FONT_SIZE_SMALL;

        // override ChevronPath: to make it smaller
        if (e.NameScope.Find<Path>("PART_ChevronPath") is not Path chevronEl) return;
        chevronEl.Width = 10;
        chevronEl.Height = 10;
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        LocalizeText();

        Core.LanguageChanged += Core_LanguageChanged;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Core.LanguageChanged -= Core_LanguageChanged;
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == LangKeyProperty || e.Property == LangParamsProperty)
        {
            LocalizeText();
        }
        else if (e.Property == IconDataProperty)
        {
            Icon = IconData is null ? null : new PathIcon { Data = IconData };
        }
    }


    private void Core_LanguageChanged(object? sender, System.EventArgs e)
    {
        LocalizeText();
    }


    /// <summary>
    /// Localize menu item text. Public so an owner can re-localize items whose popup has never been
    /// opened (they are not in the visual tree, so they never receive the language-changed event).
    /// </summary>
    public void LocalizeText()
    {
        var localizedText = Core.Lang[LangKey, LangParams];
        if (string.IsNullOrWhiteSpace(localizedText)) return;

        // admin-locked: forbidden
        if (FeatureManager.IsLocked(LangKey))
        {
            Header = $"{localizedText} 🔒";
            Disable();
        }
        // Pro feature not yet unlocked: badged; disabled unless previewable
        else if (FeatureManager.IsProGated(LangKey))
        {
            Header = new TextBlock
            {
                Inlines = new InlineCollection
                {
                    new Run(localizedText),
                    new Run(" ✦") { Foreground = new SolidColorBrush(Core.AccentColor) },
                },
            };

            if (FeatureManager.IsProBlocked(LangKey)) Disable();
            else RestoreEnabled();
        }
        else
        {
            Header = localizedText;
            RestoreEnabled();
        }
    }


    /// <summary>
    /// Disables the item because of a gate (admin lock or Pro), remembering that we did it.
    /// </summary>
    private void Disable()
    {
        _disabledByGate = true;
        IsEnabled = false;
    }


    /// <summary>
    /// Re-enables the item only if a gate disabled it, so an owner-driven disabled state
    /// (e.g. a menu item turned off for the current photo) is never clobbered.
    /// </summary>
    private void RestoreEnabled()
    {
        if (!_disabledByGate) return;

        _disabledByGate = false;
        IsEnabled = true;
    }

    #endregion // Control Methods


}





