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
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using ImageGlass.Common;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Types;

namespace ImageGlass.UI;

/// <summary>
/// The app logo from the current theme pack, marked with the Pro star on a Pro edition.
/// </summary>
public partial class PhLogo : PhControl
{
    // taken from the license hero, where a 48px logo carries a 8px star inset by 5px
    private const double PRO_STAR_RATIO = 8.0 / 48.0;
    private const double PRO_STAR_INSET_RATIO = 5.0 / 48.0;


    public PhLogo()
    {
        InitializeComponent();

        // the edition cannot change while a window is open: importing a license restarts the app
        PART_ProStar.IsVisible = Core.IsProEnabled;

        ApplyLogoSize();
        UpdateLogo();
    }


    #region Public Properties

    /// <summary>
    /// Gets, sets the logo edge length; the Pro star scales with it.
    /// </summary>
    public double LogoSize
    {
        get => GetValue(LogoSizeProperty);
        set => SetValue(LogoSizeProperty, value);
    }
    public static readonly StyledProperty<double> LogoSizeProperty =
        AvaloniaProperty.Register<PhLogo, double>(nameof(LogoSize), 48d);

    #endregion // Public Properties



    #region Overrides

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LogoSizeProperty) ApplyLogoSize();
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        UpdateLogo();
    }

    #endregion // Overrides



    #region Private Methods

    private void ApplyLogoSize()
    {
        if (PART_ProStar is null) return;

        Width = LogoSize;
        Height = LogoSize;

        var starSize = LogoSize * PRO_STAR_RATIO;
        var inset = LogoSize * PRO_STAR_INSET_RATIO;

        PART_ProStar.Width = starSize;
        PART_ProStar.Height = starSize;
        PART_ProStar.Margin = new Thickness(0, inset, inset, 0);
    }


    /// <summary>
    /// Loads the theme pack logo, falling back to the bundled app icon.
    /// </summary>
    private void UpdateLogo()
    {
        if (PART_Logo is null) return;

        // 1. try load theme logo
        try
        {
            var iconPath = Core.Theme.GetIconPath(IgThemeIcon.AppLogo);
            PART_Logo.Source = new SvgImage
            {
                Source = SvgSource.Load(iconPath),
            };
        }
        catch { }

        // 2. load the default logo
        if (PART_Logo.Source is null)
        {
            using var stream = Resx.GetDefaultWindowIconAsStream();
            if (stream is not null)
            {
                PART_Logo.Source = Bitmap.DecodeToHeight(stream, 256);
            }
        }
    }

    #endregion // Private Methods

}
