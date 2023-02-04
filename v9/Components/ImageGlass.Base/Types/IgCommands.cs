/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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


    // igcmd.exe
    public static string SET_WALLPAPER => "set-wallpaper";
    public static string SET_DEFAULT_PHOTO_VIEWER => "set-default-viewer";
    public static string UNSET_DEFAULT_PHOTO_VIEWER => "unset-default-viewer";
    public static string START_SLIDESHOW => "start-slideshow";
    public static string EXPORT_FRAMES => "export-frames";


    // igcmd10.exe
    public static string SET_LOCK_SCREEN => "set-lock-screen";
    public static string SHARE => "share";
}
