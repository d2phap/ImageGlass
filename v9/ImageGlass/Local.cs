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

using ImageGlass.Base;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.Services;
using ImageGlass.Settings;
using ImageGlass.Tools;
using System.IO.Pipes;
using WicNet;

namespace ImageGlass;

internal class Local
{
    public static FrmMain? FrmMain;


    #region Public events

    /// <summary>
    /// Occurs when <see cref="Images"/> is loaded.
    /// </summary>
    public static event ImageListLoadedHandler? ImageListLoaded;
    public delegate void ImageListLoadedHandler(ImageListLoadedEventArgs e);

    /// <summary>
    /// Occurs when the requested image is being loaded.
    /// </summary>
    public static event ImageLoadingHandler? ImageLoading;
    public delegate void ImageLoadingHandler(ImageLoadingEventArgs e);

    /// <summary>
    /// Occurs when the requested image is loaded.
    /// </summary>
    public static event ImageLoadedHandler? ImageLoaded;
    public delegate void ImageLoadedHandler(ImageLoadedEventArgs e);

    /// <summary>
    /// Occurs when the image is unloaded.
    /// </summary>
    public static event ImageUnloadedHandler? ImageUnloaded;
    public delegate void ImageUnloadedHandler(ImageUnloadedEventArgs e);

    /// <summary>
    /// Occurs when the first image is reached.
    /// </summary>
    public static event FirstImageReachedHandler? FirstImageReached;
    public delegate void FirstImageReachedHandler();

    /// <summary>
    /// Occurs when the last image is reached.
    /// </summary>
    public static event LastImageReachedHandler? LastImageReached;
    public delegate void LastImageReachedHandler();

    /// <summary>
    /// Occurs when the FrmMain's state needs to be updated.
    /// </summary>
    public static event FrmMainUpdateRequestedHandler? RequestUpdateFrmMain;
    public delegate void FrmMainUpdateRequestedHandler(UpdateRequests e);

    /// <summary>
    /// Occurs when the FrmMain's state needs to be updated.
    /// </summary>
    public static event ImageSavedHandler? ImageSaved;
    public delegate void ImageSavedHandler(ImageSaveEventArgs e);


    /// <summary>
    /// Raise <see cref="ImageListLoaded"/> event.
    /// </summary>
    public static void RaiseImageListLoadedEvent(ImageListLoadedEventArgs e)
    {
        ImageListLoaded?.Invoke(e);

        // emit event to all tools
        _ = BroadcastMessageToAllToolServersAsync(ToolServerMsgs.IMAGE_LIST_UPDATED, e);
    }


    /// <summary>
    /// Raise <see cref="ImageLoading"/> event.
    /// </summary>
    public static void RaiseImageLoadingEvent(ImageLoadingEventArgs e)
    {
        ImageLoading?.Invoke(e);

        // emit event to all tools
        _ = BroadcastMessageToAllToolServersAsync(ToolServerMsgs.IMAGE_LOADING, e);
    }


    /// <summary>
    /// Raise <see cref="ImageLoaded"/> event.
    /// </summary>
    public static void RaiseImageLoadedEvent(ImageLoadedEventArgs e)
    {
        ImageLoaded?.Invoke(e);

        // emit event to all tools
        _ = BroadcastMessageToAllToolServersAsync(ToolServerMsgs.IMAGE_LOADED, e);
    }


    /// <summary>
    /// Raise <see cref="ImageUnloaded"/> event.
    /// </summary>
    public static void RaiseImageUnloadedEvent(ImageUnloadedEventArgs e)
    {
        ImageUnloaded?.Invoke(e);

        // emit event to all tools
        _ = BroadcastMessageToAllToolServersAsync(ToolServerMsgs.IMAGE_UNLOADED, e);
    }


    /// <summary>
    /// Raise <see cref="FirstImageReached"/> event.
    /// </summary>
    public static void RaiseFirstImageReachedEvent()
    {
        FirstImageReached?.Invoke();
    }


    /// <summary>
    /// Raise <see cref="LastImageReached"/> event.
    /// </summary>
    public static void RaiseLastImageReachedEvent()
    {
        LastImageReached?.Invoke();
    }


    /// <summary>
    /// Raise <see cref="RequestUpdateFrmMain"/> event.
    /// </summary>
    public static void UpdateFrmMain(UpdateRequests e)
    {
        RequestUpdateFrmMain?.Invoke(e);
    }


    /// <summary>
    /// Raise <see cref="ImageSaved"/> event.
    /// </summary>
    public static void RaiseImageSavedEvent(ImageSaveEventArgs e)
    {
        ImageSaved?.Invoke(e);
    }


    #endregion // Public events


    #region Public properties

    /// <summary>
    /// Gets, sets the tools.
    /// </summary>
    public static Dictionary<string, ToolForm?> Tools { get; set; } = new();

    /// <summary>
    /// Gets, sets the list of slideshow pipe servers.
    /// </summary>
    public static List<PipeServer?> SlideshowPipeServers { get; set; } = new();

    /// <summary>
    /// Gets, sets the list of tool pipe servers.
    /// </summary>
    public static Dictionary<string, PipeServer?> ToolPipeServers { get; set; } = new();

    /// <summary>
    /// Gets, sets the metadata of the current image in the list.
    /// </summary>
    public static IgMetadata? Metadata { get; set; }

    /// <summary>
    /// Gets, sets app state
    /// </summary>
    public static bool IsBusy { get; set; } = false;

    /// <summary>
    /// Gets, sets images list
    /// </summary>
    public static ImageBooster Images { get; set; } = new();

    /// <summary>
    /// Gets, sets index of the viewing image
    /// </summary>
    public static int CurrentIndex { get; set; } = -1;

    /// <summary>
    /// Gets, sets the changes of the current viewing image.
    /// </summary>
    public static ImgTransform ImageTransform = new();


    /// <summary>
    /// Gets, sets the value if the current image is error
    /// </summary>
    public static bool IsImageError { get; set; } = false;

    /// <summary>
    /// <para>The current "initial" path (file or dir) we're viewing. Used when the user changes the sort settings: we need to rebuild the image list, but otherwise we don't know what image/folder we started with.</para>
    /// <para>Here's what happened: I opened a folder with subfolders (child folders enabled). I was going through the images, and decided I wanted to change the sort order. Since the _current_ image was in a sub-folder, after a rescan of the image list, only the _sub_-folders images were re-loaded!</para>
    /// <para>But if we reload the list using the original image, then the original folder's images, and the sub-folders, are reloaded.</para>
    /// </summary>
    public static string InitialInputPath { get; set; } = "";

    /// <summary>
    /// The 'current' image sorting order. A reconciliation between the user's Settings selection and the sorting order from Windows Explorer, to be used to sort the active image list.
    /// </summary>
    public static ImageOrderBy ActiveImageLoadingOrder { get; set; }

    /// <summary>
    /// The 'current' image sorting direction. A reconciliation between the user's Settings selection and the sorting direction from Windows Explorer, to be used to sort the active image list.
    /// </summary>
    public static ImageOrderType ActiveImageLoadingOrderType { get; set; }

    /// <summary>
    /// Remember for this session the last-used "Save As" extension. When the user is iterating
    /// through a set of images and using "Save As" to always save to the same file type, this
    /// memory prevents them from having to manually re-select their desired extension.
    /// </summary>
    public static string SaveAsFilterExt { get; set; } = "";

    /// <summary>
    /// Gets, sets color channel of image
    /// </summary>
    public static ColorChannel ImageChannel { get; set; } = ColorChannel.All;

    /// <summary>
    /// Gets, sets value if image data was modified
    /// </summary>
    public static string ImageModifiedPath { get; set; } = "";

    /// <summary>
    /// Gets, sets the temporary image data
    /// </summary>
    public static WicBitmapSource? ClipboardImage { get; set; }

    /// <summary>
    /// Gets, sets the path of the temporary image
    /// (clipboard image, temp imgae for printing, background,...)
    /// </summary>
    public static string? TempImagePath { get; set; }

    /// <summary>
    /// Gets, sets copied filename collection (multi-copy)
    /// </summary>
    public static List<string> StringClipboard { get; set; } = new();

    #endregion // Public properties


    #region Public Functions

    /// <summary>
    /// Initialize image list.
    /// </summary>
    public static void InitImageList(
        IEnumerable<string>? list = null,
        List<string>? distinctDirsList = null)
    {
        Images.Dispose();
        Images = new(list)
        {
            MaxQueue = Config.ImageBoosterCacheCount,
            MaxImageDimensionToCache = Config.ImageBoosterCacheMaxDimension,
            MaxFileSizeInMbToCache = Config.ImageBoosterCacheMaxFileSizeInMb,

            ImageChannel = ImageChannel,
            DistinctDirs = distinctDirsList ?? new(0),
        };
    }


    /// <summary>
    /// Save the viewing image as temporary file
    /// with the <see cref="Config.ImageEditQuality"/> quality.
    /// </summary>
    public static async Task<string?> SaveImageAsTempFileAsync(string ext = ".png", int? quality = null)
    {
        // check if we can use the current clipboard image path
        if (File.Exists(TempImagePath))
        {
            var extension = Path.GetExtension(TempImagePath);

            if (extension.Equals(ext, StringComparison.InvariantCultureIgnoreCase))
            {
                return TempImagePath;
            }
        }

        var tempDir = App.ConfigDir(PathType.Dir, Dir.Temporary);
        Directory.CreateDirectory(tempDir);

        var filename = Path.Combine(tempDir, $"temp_{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}{ext}");
        quality ??= Config.ImageEditQuality;


        // save clipboard image
        if (ClipboardImage != null)
        {
            try
            {
                await PhotoCodec.SaveAsync(ClipboardImage, filename, Local.ImageTransform, quality.Value);

                TempImagePath = filename;
            }
            catch
            {
                TempImagePath = null;
            }

            return TempImagePath;
        }


        // save the current viewing image file
        var img = await Images.GetAsync(CurrentIndex);
        if (img?.ImgData?.Image != null)
        {
            try
            {
                await PhotoCodec.SaveAsync(img.ImgData.Image, filename, Local.ImageTransform, quality.Value);

                TempImagePath = filename;
            }
            catch
            {
                TempImagePath = null;
            }
        }

        return TempImagePath;
    }


    /// <summary>
    /// Sends message to all tool servers
    /// </summary>
    public static async Task BroadcastMessageToAllToolServersAsync(string msgName, object? data)
    {
        if (Local.ToolPipeServers.Count == 0) return;
        var msgData = BHelper.ToJson(data ?? "") ?? "";

        // emit event to all tools
        await Parallel.ForEachAsync(
            Local.ToolPipeServers,
            new ParallelOptions() { MaxDegreeOfParallelism = 20 },
            async (server, token) =>
            {
                if (server.Value is PipeServer tool)
                {
                    await tool.SendAsync(msgName, msgData);
                }
            });
    }

    /// <summary>
    /// Opens tool as a <see cref="PipeServer"/>.
    /// </summary>
    public static async Task OpenPipedToolAsync(IgTool? tool)
    {
        if (tool == null || tool.IsEmpty
            || Local.ToolPipeServers.ContainsKey(tool.ToolId)) return;

        // prepend tool prefix to create pipe name
        var pipeName = $"{ImageGlassTool.PIPENAME_PREFIX}{tool.Executable}";

        // create a new tool server
        var toolServer = new PipeServer(pipeName, PipeDirection.InOut);
        toolServer.ClientDisconnected += ToolServer_ClientDisconnected;
        Local.ToolPipeServers.Add(tool.ToolId, toolServer);

        // start the server
        toolServer.Start();
        await Config.WriteAsync();

        // start tool client
        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        var args = tool.Argument?.Replace(Constants.FILE_MACRO, $"\"{filePath}\"");
        _ = BHelper.RunExeCmd($"{tool.Executable}", $"-EnableWindowTopMost={Config.EnableWindowTopMost} {args}", false);

        // wait for client connection
        await toolServer.WaitForConnectionAsync();
    }


    private static void ToolServer_ClientDisconnected(object? sender, DisconnectedEventArgs e)
    {
        if (FrmMain.InvokeRequired)
        {
            FrmMain.Invoke(delegate
            {
                ToolServer_ClientDisconnected(sender, e);
            });
            return;
        }

        // get tool info
        var item = Local.ToolPipeServers.FirstOrDefault(i => i.Value.PipeName
            .Equals(e.PipeName, StringComparison.InvariantCultureIgnoreCase));

        var toolId = item.Key;
        var toolServer = item.Value;

        // update menu state
        if (FrmMain.MnuTools.DropDownItems[toolId] is ToolStripMenuItem mnuItem)
        {
            mnuItem.Checked = false;
        }

        // remove events
        toolServer.Stop();
        toolServer.ClientDisconnected -= ToolServer_ClientDisconnected;
        toolServer.Dispose();
        toolServer = null;

        // remove tool server
        Local.ToolPipeServers.Remove(toolId);
    }


    /// <summary>
    /// Closes <see cref="PipeServer"/> tool.
    /// </summary>
    public static async Task ClosePipedToolAsync(IgTool? tool)
    {
        if (tool == null
            || !Local.ToolPipeServers.TryGetValue(tool.ToolId, out var toolServer)
            || toolServer is not PipeServer) return;

        if (toolServer.ServerStream.IsConnected)
        {
            await toolServer.SendAsync(ToolServerMsgs.TOOL_TERMINATE);

            // wait for 3 seconds for client to disconnect
            await Task.Delay(3000);
        }

        // remove tool server
        ToolServer_ClientDisconnected(null, new DisconnectedEventArgs(toolServer.PipeName));
    }

    #endregion

}