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
using Avalonia.Interactivity;
using ImageGlass.Common;

namespace ImageGlass.UI;

public partial class ToolbarButton : PhToolButton, IToolbarItem
{
    public ToolbarItemModel VM => (ToolbarItemModel)DataContext!;


    public ToolbarButton()
    {
        DataContext = new ToolbarItemModel();
        InitializeComponent();
    }



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        Core.Config.PropertyChanged += Config_PropertyChanged;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Core.Config.PropertyChanged -= Config_PropertyChanged;
    }


    protected override void OnIgDropdownMenuOpened(RoutedEventArgs e)
    {
        base.OnIgDropdownMenuOpened(e);
        IsChecked = true;
    }


    protected override void OnIgDropdownMenuClosed(RoutedEventArgs e)
    {
        base.OnIgDropdownMenuClosed(e);
        IsChecked = false;
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);

        // a theme pack may not ship the icon this button asks for
        _ = VM.OnPropertyChanged(nameof(VM.ImagePath));
        _ = VM.OnPropertyChanged(nameof(VM.IsPlaceholderIconVisible));
    }


    private void Config_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (nameof(Core.Config.ToolbarIconHeight).Equals(e.PropertyName))
        {
            _ = VM.OnPropertyChanged(nameof(VM.InnerSpacing));
            _ = VM.OnPropertyChanged(nameof(VM.ItemPadding));
            _ = VM.OnPropertyChanged(nameof(VM.SeparatorEndPoint));
        }
    }

    #endregion Control Events


}