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

namespace ImageGlass.Tools;


/// <summary>
/// Contains messages of <see cref="PipeServer"/> to send to <see cref="PipeClient"/>.
/// </summary>
public static class ToolServerMsgs
{
    /// <summary>
    /// Requests <see cref="PipeClient"/> to terminate the process.
    /// </summary>
    public static string TOOL_TERMINATE => "igtool.cmd.tool_terminate";



    /// <summary>
    /// Occurs when an image is being loaded.
    /// </summary>
    public static string IMAGE_LOADING => "igtool.cmd.image_loading";

    /// <summary>
    /// Occurs when the image is loaded.
    /// </summary>
    public static string IMAGE_LOADED => "igtool.cmd.image_loaded";

    /// <summary>
    /// Occurs when the image is unloaded.
    /// </summary>
    public static string IMAGE_UNLOADED => "igtool.cmd.image_unloaded";



    /// <summary>
    /// Occurs when the image list is updated.
    /// </summary>
    public static string IMAGE_LIST_UPDATED => "igtool.cmd.list_updated";
}
