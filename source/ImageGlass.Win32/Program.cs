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
using Avalonia.Input;
using ImageGlass.Common;
using ImageGlass.Common.Loggers;
using ImageGlass.ViewModels;
using ImageGlass.Win32.Common.ServiceProviders;
using ImageGlass.Win32.Windows;
using System;

namespace ImageGlass.Win32;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        StartupTrace.Mark("Main:start");
        Core.BuildInfo = new AppBuildInfo();

        // must precede InitializeAppInstance: the license is resolved in there, before the
        // providers below are installed
        Core.StoreEntitlementProvider = new Win32StoreEntitlementProvider();

        var isHandled = App.InitializeAppInstance(args, () =>
        {
            // initialize service providers
            Core.FileSearchProvider = new Win32FileSearchProvider();
            Core.PreviewProvider = new Win32PhotoPreviewProvider();
            Core.ShellProvider = new Win32ShellProvider();
            Core.PrintProvider = new Win32PrintProvider();
            Core.PipeSecurityProvider = new Win32PipeSecurityProvider();
            Core.UpdateProvider = new Win32UpdateProvider();
        });

        if (isHandled) return 0;

        StartupTrace.Mark("Avalonia:start");
        return BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }



    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
#if DEBUG
        .LogToTrace()
        .WithDeveloperTools(o =>
        {
            o.ApplicationName = BHelper.AppDisplayName;
            o.Gesture = new KeyGesture(Key.I, KeyModifiers.Control | KeyModifiers.Shift);
        })
#endif
        .UseWin32()
        .UseSkia()
        .UseHarfBuzz()
        .With(new SkiaOptions
        {
            MaxGpuResourceSizeBytes = long.MaxValue,
        })
        .AfterSetup(builder =>
        {
            var app = (App?)builder.Instance;
            app?.CreateMainWindowFn = () =>
            {
                // create main window
                var mainWindow = new MainWindow32();
                mainWindow.DataContext = new MainWindowModel(mainWindow);

                return mainWindow;
            };
        });
}
