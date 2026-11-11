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
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;

namespace ImageGlass.UI;

/// <summary>
/// A "✦" badge marking an ImageGlass Pro feature: accent glyph, a padded hit area bigger than
/// the glyph, a soft accent hover background, and the Pro-feature tooltip.
/// </summary>
public class PhProBadge : PhControl
{
    public PhProBadge()
    {
        Padding = new Thickness(5, 1);
        CornerRadius = new CornerRadius(4);
        VerticalAlignment = VerticalAlignment.Center;
        HorizontalContentAlignment = HorizontalAlignment.Center;
        VerticalContentAlignment = VerticalAlignment.Center;
        Background = Brushes.Transparent;
        Cursor = new Cursor(StandardCursorType.Help);

        Content = new TextBlock
        {
            Text = "✦",
            FontWeight = FontWeight.Bold,
            [!TextBlock.ForegroundProperty] = Resx.CreateBinding(ResxId.IG_TextAccentColor),
        };

        PointerEntered += (_, _) => Background = new SolidColorBrush(Core.AccentColor, 0.15);
        PointerExited += (_, _) => Background = Brushes.Transparent;
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();
        ToolTip.SetTip(this, Core.Lang[LangId.Settings_ProFeatureHint]);
    }
}
