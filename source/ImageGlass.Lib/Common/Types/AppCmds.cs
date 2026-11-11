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

namespace ImageGlass.Common.Types;

public static class AppCmds
{
    /// <summary>
    /// Single instance message
    /// </summary>
    /// <remarks>
    /// Example:
    /// <code>ImageGlass.exe --ig-single-instance</code>
    /// </remarks>
    public static string SINGLE_INSTANCE => "--ig-single-instance";

    /// <summary>
    /// Opt-in startup profiler (see StartupTrace); writes ig_startup_trace.log to the config dir
    /// </summary>
    /// <remarks>
    /// Example:
    /// <code>ImageGlass.exe --ig-startup-trace</code>
    /// </remarks>
    public static string STARTUP_TRACE => "--ig-startup-trace";

    /// <summary>
    /// Opt-in photo-loading profiler (see PhotoTrace); writes ig_photo_trace.log to the config dir
    /// </summary>
    /// <remarks>
    /// Example:
    /// <code>ImageGlass.exe --ig-photo-trace</code>
    /// </remarks>
    public static string PHOTO_TRACE => "--ig-photo-trace";

    /// <summary>
    /// Suppresses the forced startup Quick Setup for this launch.
    /// </summary>
    /// <remarks>
    /// Example:
    /// <code>ImageGlass.exe --ig-no-quick-setup</code>
    /// </remarks>
    public static string NO_QUICK_SETUP => "--ig-no-quick-setup";

    /// <summary>
    /// Registers the app as the default photo viewer for the given extensions.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// <code>
    /// ImageGlass.exe --ig-set-default-viewer
    /// ImageGlass.exe --ig-set-default-viewer .jpg;.png;.webp
    /// </code>
    /// </remarks>
    public static string SET_DEFAULT_PHOTO_VIEWER => "--ig-set-default-viewer";

    /// <summary>
    /// Unregisters the app as the default photo viewer for the given extensions.
    /// </summary>
    /// <remarks>
    /// Example:
    /// <code>ImageGlass.exe --ig-remove-default-viewer</code>
    /// </remarks>
    public static string REMOVE_DEFAULT_PHOTO_VIEWER => "--ig-remove-default-viewer";


    //public static string SET_WALLPAPER => "--ig-set-wallpaper";
    //public static string SET_LOCK_SCREEN => "--ig-set-lock-screen";
    //public static string START_SLIDESHOW => "--ig-start-slideshow";
    //public static string EXPORT_FRAMES => "--ig-export-frames";
    //public static string LOSSLESS_COMPRESS => "--ig-lossless-compress";

    //public static string CHECK_FOR_UPDATE => "--ig-check-for-update";
    //public static string INSTALL_LANGUAGES => "--ig-install-languages";
    //public static string INSTALL_THEMES => "--ig-install-themes";
    //public static string UNINSTALL_THEME => "--ig-uninstall-theme";

}
