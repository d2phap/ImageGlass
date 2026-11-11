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
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.UI.Viewer;
using ImageGlass.UI.Windowing;
using System.Threading.Tasks;

namespace ImageGlass.Tools;

/// <summary>
/// Non-hosted tool for Lossless Compression.
/// </summary>
public sealed class LosslessCompressionTool : ITool
{
    public static string TOOL_ID => "Tool_LosslessCompression";

    public string ToolId => TOOL_ID;
    public bool IsHosted => false;
    public object? Settings { get; private set; }
    public ViewerControl Viewer { get; set; } = null!;


    public async Task ExecuteAsync(ToolExecutionContext context)
    {
        if (Core.IsBusy || Core.Photos.Count == 0 || Core.ClipboardImage != null) return;

        var filePath = Core.Photos.CurrentFilePath;

        // check if image format not supported
        if (!MagickCodec.IsLosslessCompressSupported(filePath))
        {
            _ = await ModalWindow.ShowInfoAsync(context.Window, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Menu_MnuLosslessCompression],
                Heading = Core.Lang[LangId._NotSupported],
                Description = filePath,
                Thumbnail = Core.Photos.Current?.GalleryThumbnail,
            });

            return;
        }

        // perform lossless compression
        Core.IsBusy = true;

        try
        {
            var compressionWindow = new LosslessCompressionWindow(filePath);
            _ = await compressionWindow.ShowAsync(context.Window);
        }
        finally
        {
            // never leave the app stuck in the busy state
            Core.IsBusy = false;
        }
    }
}
