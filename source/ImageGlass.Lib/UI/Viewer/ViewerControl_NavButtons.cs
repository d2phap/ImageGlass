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
using ImageGlass.Common.Types;

namespace ImageGlass.UI.Viewer;

public partial class ViewerControl
{
    // navigation buttons
    internal NavButtonsInfo _navButtons = new();


    /// <summary>
    /// Enables or disables navigation buttons.
    /// </summary>
    public bool EnableNavButtons
    {
        get => GetValue(EnableNavButtonsProperty);
        set => SetValue(EnableNavButtonsProperty, value);
    }

    public static readonly StyledProperty<bool> EnableNavButtonsProperty =
        AvaloniaProperty.Register<ViewerControl, bool>(nameof(EnableNavButtons), defaultValue: true);


    /// <summary>
    /// Occurs when a navigation button is clicked.
    /// </summary>
    public event TEventHandler<ViewerControl, NavButtonClickedEventArgs>? NavButtonClicked;


    /// <summary>
    /// Called by <see cref="NavButtonsOverlay"/> when a nav button is clicked.
    /// </summary>
    internal void OnNavButtonClicked(NavButtonDirection direction)
    {
        NavButtonClicked?.Invoke(this, new NavButtonClickedEventArgs(direction));
    }
}
