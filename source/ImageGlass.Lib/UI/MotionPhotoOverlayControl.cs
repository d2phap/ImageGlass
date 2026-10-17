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
using Avalonia.Interactivity;
using Avalonia.Layout;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;

namespace ImageGlass.UI;

/// <summary>
/// Shows a button on the viewer that plays the motion video of a live photo.
/// </summary>
public class MotionPhotoOverlayControl : PhOverlay
{
    private const double ICON_SIZE = 20;

    private readonly PhToolButton _btnPlay;


    static MotionPhotoOverlayControl()
    {
        // a button a little above the bottom edge, rising out of it
        HorizontalAlignmentProperty.OverrideDefaultValue<MotionPhotoOverlayControl>(HorizontalAlignment.Center);
        VerticalAlignmentProperty.OverrideDefaultValue<MotionPhotoOverlayControl>(VerticalAlignment.Bottom);
        MarginProperty.OverrideDefaultValue<MotionPhotoOverlayControl>(new Thickness(0, 0, 0, 10));
        TransitionDirectionProperty.OverrideDefaultValue<MotionPhotoOverlayControl>(PhOverlayDirection.Bottom);

        // the inset keeps the button's rounded hover concentric with the overlay's corners
        PaddingProperty.OverrideDefaultValue<MotionPhotoOverlayControl>(new Thickness(1));
    }


    public MotionPhotoOverlayControl()
    {
        _btnPlay = new PhToolButton
        {
            Padding = new Thickness(8),
            Focusable = false,
            Content = new PathIcon
            {
                Width = ICON_SIZE,
                Height = ICON_SIZE,
                Data = Resx.GetIcon(ResxIconId.IconLivePhoto),
            },
        };
        _btnPlay.Click += BtnPlay_Click;
        Content = _btnPlay;

        UpdateTooltip();
    }



    #region Control Events

    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();
        UpdateTooltip();
    }


    private async void BtnPlay_Click(object? sender, RoutedEventArgs e)
    {
        _ = await Core.API.RunApiAsync(API.IG_ToggleImageAnimation);
    }

    #endregion // Control Events



    #region Private Methods

    /// <summary>
    /// Names the button's action with its hotkey.
    /// </summary>
    private void UpdateTooltip()
    {
        var tooltip = AppAPIProvider.GetMenuTooltipText(LangId._PlayMotionVideo, LangId.Menu_MnuToggleImageAnimation);
        ToolTip.SetTip(_btnPlay, tooltip);
    }

    #endregion // Private Methods

}
