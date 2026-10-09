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
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Photoing;
using ImageGlass.UI.Viewer;
using ImageGlass.UI.Windowing;
using System.Threading.Tasks;

namespace ImageGlass.Tools;

/// <summary>
/// Non-hosted tool for the Image Resizer.
/// </summary>
public sealed class ImageResizerTool : ITool
{
    public static string TOOL_ID => "Tool_ImageResizer";

    public string ToolId => TOOL_ID;
    public bool IsHosted => false;
    public object? Settings { get; private set; }
    public ViewerControl Viewer { get; set; } = null!;


    public async Task ExecuteAsync(ToolExecutionContext context)
    {
        if (Core.IsBusy) return;

        Core.IsBusy = true;

        try
        {
            // get current bitmap
            using var shownState = Viewer.CaptureImageState();
            using var srcBmp = shownState?.CopyPixels();
            if (srcBmp.IsDisposed()) return;

            // show resizer window
            var resizerWindow = new ImageResizerWindow(srcBmp);
            var result = await resizerWindow.ShowAsync(context.Window);

            // load the output image
            if (result == DialogExitCode.OK && !resizerWindow.OutputBitmap.IsDisposed())
            {
                var photo = new Photo(resizerWindow.OutputBitmap);
                await Core.LoadClipboardPhotoAsync(photo);
            }
        }
        finally
        {
            Core.IsBusy = false;
        }
    }
}
