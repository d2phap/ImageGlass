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
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Loggers;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.ServiceProviders.FileSearchService;
using ImageGlass.Linux.Common.ServiceProviders;
using System;

namespace ImageGlass.Linux;

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

        var isHandled = App.InitializeAppInstance(args, () =>
        {
            // initialize service providers
            Core.FileSearchProvider = new FileSearchProvider();
            Core.PreviewProvider = new PhotoPreviewProvider();
            Core.ShellProvider = new LinuxShellProvider();
            Core.PrintProvider = new LinuxPrintProvider();
            Core.UpdateProvider = new LinuxUpdateProvider();
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
        .UsePlatformDetect()
#else
        .UseX11()
#endif
        .UseSkia()
        .UseHarfBuzz()
        .WithInterFont()
        .With(new FontManagerOptions
        {
            DefaultFamilyName = "Inter",
        })
        .With(new SkiaOptions
        {
            MaxGpuResourceSizeBytes = long.MaxValue,
        });
}

