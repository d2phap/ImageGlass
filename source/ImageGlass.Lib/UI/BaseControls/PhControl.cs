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
using Avalonia.Threading;
using ImageGlass.Common;
using System;

namespace ImageGlass.UI;

public partial class PhControl : ContentControl
{
    protected override Type StyleKeyOverride => typeof(ContentControl);


    #region Public Properties

    /// <summary>
    /// Gets the DPI scale value.
    /// </summary>
    public double Dpi => TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;


    /// <summary>
    /// Gets, sets the visibility.
    /// </summary>
    public bool IsContentVisible
    {
        get => GetValue(IsContentVisibleProperty);
        set => SetValue(IsContentVisibleProperty, value);
    }
    public static readonly StyledProperty<bool> IsContentVisibleProperty =
        AvaloniaProperty.Register<PhControl, bool>(nameof(IsContentVisible), true,
            coerce: (sender, value) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var control = (PhControl)sender;
                    if (control.Content is Control contentEl)
                    {
                        contentEl.IsVisible = value;
                    }
                });

                return value;
            });

    #endregion // Public Properties



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        OnIgLanguageChanged();

        Core.ThemeChanged += Core_ThemeChanged;
        Core.LanguageChanged += Core_LanguageChanged;
        TopLevel.GetTopLevel(this)?.ScalingChanged += TopLevel_ScalingChanged;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Core.ThemeChanged -= Core_ThemeChanged;
        Core.LanguageChanged -= Core_LanguageChanged;
        TopLevel.GetTopLevel(this)?.ScalingChanged -= TopLevel_ScalingChanged;
    }


    private void Core_ThemeChanged(object? sender, ThemePackChangedEventArgs e)
    {
        OnIgThemeChanged(e);
    }


    private void Core_LanguageChanged(object? sender, EventArgs e)
    {
        OnIgLanguageChanged();
    }


    private void TopLevel_ScalingChanged(object? sender, EventArgs e)
    {
        OnIgDpiChanged();
    }


    #endregion // Control Events



    #region Virtual Methods

    /// <summary>
    /// Occurs when DPI is changed.
    /// </summary>
    protected virtual void OnIgDpiChanged() { }


    /// <summary>
    /// Occurs when the app theme is changed.
    /// </summary>
    protected virtual void OnIgThemeChanged(ThemePackChangedEventArgs e) { }


    /// <summary>
    /// Occurs when the app language is changed.
    /// </summary>
    protected virtual void OnIgLanguageChanged() { }

    #endregion // Virtual Methods



    #region Public Methods

    /// <summary>
    /// Scales the given number on the DPI scaling factor.
    /// </summary>
    public double DpiScale(double value, double? scaleFactor = null) => (scaleFactor ?? Dpi) * value;


    /// <summary>
    /// Scales the given size based on the DPI scaling factor.
    /// </summary>
    public Size DpiScale(Size value, double? scaleFactor = null) => new Size(DpiScale(value.Width, scaleFactor), DpiScale(value.Height, scaleFactor));


    /// <summary>
    /// Scales the given point on the DPI scaling factor.
    /// </summary>
    public Point DpiScale(Point value, double? scaleFactor = null) => new Point(DpiScale(value.X, scaleFactor), DpiScale(value.Y, scaleFactor));

    #endregion // Public Methods



}
