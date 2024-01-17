/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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

namespace ImageGlass.Base;

public static class IgCommands
{
    // UI result options
    public static string SHOW_UI => "--ui";
    public static string HIDE_ADMIN_REQUIRED_ERROR_UI => "--hide-admin-error-ui";
    public static string PER_MACHINE => "--per-machine";


    // igcmd.exe
    public static string SET_WALLPAPER => "set-wallpaper";
    public static string SET_LOCK_SCREEN => "set-lock-screen";
    public static string SET_DEFAULT_PHOTO_VIEWER => "set-default-viewer";
    public static string REMOVE_DEFAULT_PHOTO_VIEWER => "remove-default-viewer";
    public static string START_SLIDESHOW => "start-slideshow";
    public static string EXPORT_FRAMES => "export-frames";
    public static string LOSSLESS_COMPRESS => "lossless-compress";


    public static string QUICK_SETUP => "quick-setup";
    public static string CHECK_FOR_UPDATE => "check-for-update";
    public static string INSTALL_LANGUAGES => "install-languages";
    public static string INSTALL_THEMES => "install-themes";
    public static string UNINSTALL_THEME => "uninstall-theme";

}
