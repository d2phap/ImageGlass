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
using Avalonia.Controls;
using Avalonia.Interactivity;
using ImageGlass.Common;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.Common.Windows;
using ImageGlass.Win32.Common;
using ImageGlass.Win32.Common.ServiceProviders;

namespace ImageGlass.Win32.Windows;

public partial class MainWindow32 : MainWindow
{

    public override bool UseCustomBackdrop => true; // use Win32 API for the backdrop


    public MainWindow32()
    {

    }



    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // initialize Windows color profile service
        Core.ColorProfileProvider = new Win32ColorProfileProvider();
        Core.ColorProfileProvider.Changed += ColorProfileProvider_Changed;
        Core.ColorProfileProvider.Initialize(this);
    }


    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        Core.ColorProfileProvider?.Changed -= ColorProfileProvider_Changed;
    }


    protected override void OnIgBackdropStyleChanged(BackdropStyle style)
    {
        // check if we can apply window backdrop
        _canUseBackdrop = !BHelper.IsWindows10 && style != BackdropStyle.None;

        UpdateBackground(true);

        var type = style switch
        {
            BackdropStyle.Mica => SystemBackdropType.Mica,
            BackdropStyle.MicaAlt => SystemBackdropType.MicaAlt,
            BackdropStyle.Acrylic => SystemBackdropType.Acrylic,
            BackdropStyle.None => SystemBackdropType.Auto,
            _ => SystemBackdropType.Auto,
        };

        // use Win32 API for the backdrop
        Win32WindowApi.SetWindowBackdrop(Handle, type);
    }


    protected override void OnIgTitleBarIconVisibilityChanged(bool show)
    {
        Win32WindowApi.SetTitleBarIconVisible(Handle, show);
    }


    private void ColorProfileProvider_Changed(IWindowColorProfileProvider sender, ColorProfileChangedEventArgs e)
    {
        // Update the profile for later loads only. Re-decoding the on-screen photo here would
        // flash the viewer every time the window is dragged to another monitor.
        if (Core.Config.ColorProfile == nameof(ColorProfileOption.CurrentMonitorProfile))
        {
            Core.UpdateDestColorProfile(requiresPhotoReload: false);
        }
    }




}
