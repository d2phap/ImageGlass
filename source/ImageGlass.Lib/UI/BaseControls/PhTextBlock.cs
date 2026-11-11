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
using Avalonia.Interactivity;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using System;

namespace ImageGlass.UI;

public class PhTextBlock : TextBlock
{

    protected override Type StyleKeyOverride => typeof(TextBlock);



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


    #endregion // Public properties



    #region Control Methods

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
    }


    private void Core_LanguageChanged(object? sender, System.EventArgs e)
    {
        LocalizeText();
    }


    /// <summary>
    /// Localize menu item text.
    /// </summary>
    private void LocalizeText()
    {
        var localizedText = Core.Lang[LangKey, LangParams];
        if (string.IsNullOrWhiteSpace(localizedText)) return;

        Text = localizedText;
    }

    #endregion // Control Methods


}

