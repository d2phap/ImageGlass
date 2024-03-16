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

using ImageGlass.Base;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.Services;
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using ImageGlass.Tools;
using System.Collections.Frozen;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime;
using WicNet;

namespace ImageGlass;

public class Local
{
    private static CancellationTokenSource? _gcTokenSrc;
    private static FrmSettings _frmSetting;

    public static FrmMain? FrmMain;



    /// <summary>
    /// Gets the built-in toolbar buttons.
    /// </summary>
    public static readonly FrozenSet<ToolbarItemModel> BuiltInToolbarItems = FrozenSet.ToFrozenSet<ToolbarItemModel>([
        new() // MnuActualSize
        {
            Id = $"Btn_{nameof(FrmMain.MnuActualSize)}",
            Alignment = ToolStripItemAlignment.Left,
            Image = nameof(Config.Theme.ToolbarIcons.ActualSize),
            OnClick = new(nameof(FrmMain.MnuActualSize)),
        },
        new() // MnuAutoZoom
        {
            Id = $"Btn_{nameof(FrmMain.MnuAutoZoom)}",
            Image = nameof(Config.Theme.ToolbarIcons.AutoZoom),
            OnClick = new(nameof(FrmMain.MnuAutoZoom)),
        },
        new() // MnuToggleCheckerboard
        {
            Id = $"Btn_{nameof(FrmMain.MnuToggleCheckerboard)}",
            Image = nameof(Config.Theme.ToolbarIcons.Checkerboard),
            CheckableConfigBinding = nameof(Config.ShowCheckerboard),
            OnClick = new(nameof(FrmMain.MnuToggleCheckerboard)),
        },
        new() // MnuColorPicker
        {
            Id = $"Btn_{nameof(FrmMain.MnuColorPicker)}",
            Image = nameof(Config.Theme.ToolbarIcons.ColorPicker),
            OnClick = new(nameof(FrmMain.MnuColorPicker)),
        },
        new() // MnuCropTool
        {
            Id = $"Btn_{nameof(FrmMain.MnuCropTool)}",
            Image = nameof(Config.Theme.ToolbarIcons.Crop),
            OnClick = new(nameof(FrmMain.MnuCropTool)),
        },
        new() // MnuMoveToRecycleBin
        {
            Id = $"Btn_{nameof(FrmMain.MnuMoveToRecycleBin)}",
            Image = nameof(Config.Theme.ToolbarIcons.Delete),
            OnClick = new(nameof(FrmMain.MnuMoveToRecycleBin)),
        },
        new() // MnuEdit
        {
            Id = $"Btn_{nameof(FrmMain.MnuEdit)}",
            Image = nameof(Config.Theme.ToolbarIcons.Edit),
            OnClick = new(nameof(FrmMain.MnuEdit)),
        },
        new() // MnuExit
        {
            Id = $"Btn_{nameof(FrmMain.MnuExit)}",
            Image = nameof(Config.Theme.ToolbarIcons.Exit),
            OnClick = new(nameof(FrmMain.MnuExit)),
        },
        new() // MnuFlipHorizontal
        {
            Id = $"Btn_{nameof(FrmMain.MnuFlipHorizontal)}",
            Image = nameof(Config.Theme.ToolbarIcons.FlipHorz),
            OnClick = new(nameof(FrmMain.MnuFlipHorizontal)),
        },
        new() // MnuFlipVertical
        {
            Id = $"Btn_{nameof(FrmMain.MnuFlipVertical)}",
            Image = nameof(Config.Theme.ToolbarIcons.FlipVert),
            OnClick = new(nameof(FrmMain.MnuFlipVertical)),
        },
        new() // MnuFullScreen
        {
            Id = $"Btn_{nameof(FrmMain.MnuFullScreen)}",
            Image = nameof(Config.Theme.ToolbarIcons.FullScreen),
            CheckableConfigBinding = nameof(Config.EnableFullScreen),
            OnClick = new(nameof(FrmMain.MnuFullScreen)),
        },
        new() // MnuToggleGallery
        {
            Id = $"Btn_{nameof(FrmMain.MnuToggleGallery)}",
            Image = nameof(Config.Theme.ToolbarIcons.Gallery),
            CheckableConfigBinding = nameof(Config.ShowGallery),
            OnClick = new(nameof(FrmMain.MnuToggleGallery)),
        },
        new() // MnuLockZoom
        {
            Id = $"Btn_{nameof(FrmMain.MnuLockZoom)}",
            Image = nameof(Config.Theme.ToolbarIcons.LockZoom),
            OnClick = new(nameof(FrmMain.MnuLockZoom)),
        },
        new() // MnuOpenFile
        {
            Id = $"Btn_{nameof(FrmMain.MnuOpenFile)}",
            Alignment = ToolStripItemAlignment.Right,
            Image = nameof(Config.Theme.ToolbarIcons.OpenFile),
            OnClick = new(nameof(FrmMain.MnuOpenFile)),
        },
        new() // MnuPrint
        {
            Id = $"Btn_{nameof(FrmMain.MnuPrint)}",
            Image = nameof(Config.Theme.ToolbarIcons.Print),
            OnClick = new(nameof(FrmMain.MnuPrint)),
        },
        new() // MnuRefresh
        {
            Id = $"Btn_{nameof(FrmMain.MnuRefresh)}",
            Image = nameof(Config.Theme.ToolbarIcons.Refresh),
            OnClick = new(nameof(FrmMain.MnuRefresh)),
        },
        new() // MnuRotateLeft
        {
            Id = $"Btn_{nameof(FrmMain.MnuRotateLeft)}",
            Image = nameof(Config.Theme.ToolbarIcons.RotateLeft),
            OnClick = new(nameof(FrmMain.MnuRotateLeft)),
        },
        new() // MnuRotateRight
        {
            Id = $"Btn_{nameof(FrmMain.MnuRotateRight)}",
            Image = nameof(Config.Theme.ToolbarIcons.RotateRight),
            OnClick = new(nameof(FrmMain.MnuRotateRight)),
        },
        new() // MnuSave
        {
            Id = $"Btn_{nameof(FrmMain.MnuSave)}",
            Image = nameof(Config.Theme.ToolbarIcons.Save),
            OnClick = new(nameof(FrmMain.MnuSave)),
        },
        new() // MnuScaleToFill
        {
            Id = $"Btn_{nameof(FrmMain.MnuScaleToFill)}",
            Image = nameof(Config.Theme.ToolbarIcons.ScaleToFill),
            OnClick = new(nameof(FrmMain.MnuScaleToFill)),
        },
        new() // MnuScaleToFit
        {
            Id = $"Btn_{nameof(FrmMain.MnuScaleToFit)}",
            Image = nameof(Config.Theme.ToolbarIcons.ScaleToFit),
            OnClick = new(nameof(FrmMain.MnuScaleToFit)),
        },
        new() // MnuScaleToHeight
        {
            Id = $"Btn_{nameof(FrmMain.MnuScaleToHeight)}",
            Image = nameof(Config.Theme.ToolbarIcons.ScaleToHeight),
            OnClick = new(nameof(FrmMain.MnuScaleToHeight)),
        },
        new() // MnuScaleToWidth
        {
            Id = $"Btn_{nameof(FrmMain.MnuScaleToWidth)}",
            Image = nameof(Config.Theme.ToolbarIcons.ScaleToWidth),
            OnClick = new(nameof(FrmMain.MnuScaleToWidth)),
        },
        new() // MnuSlideshow
        {
            Id = $"Btn_{nameof(FrmMain.MnuSlideshow)}",
            Image = nameof(Config.Theme.ToolbarIcons.Slideshow),
            CheckableConfigBinding = nameof(Config.EnableSlideshow),
            OnClick = new(nameof(FrmMain.MnuSlideshow)),
        },
        new() // MnuViewNext
        {
            Id = $"Btn_{nameof(FrmMain.MnuViewNext)}",
            Image = nameof(Config.Theme.ToolbarIcons.ViewNextImage),
            OnClick = new(nameof(FrmMain.MnuViewNext)),
        },
        new() // MnuViewPrevious
        {
            Id = $"Btn_{nameof(FrmMain.MnuViewPrevious)}",
            Image = nameof(Config.Theme.ToolbarIcons.ViewPreviousImage),
            OnClick = new(nameof(FrmMain.MnuViewPrevious)),
        },
        new() // MnuWindowFit
        {
            Id = $"Btn_{nameof(FrmMain.MnuWindowFit)}",
            Image = nameof(Config.Theme.ToolbarIcons.WindowFit),
            OnClick = new(nameof(FrmMain.MnuWindowFit)),
        },
        new() // MnuZoomIn
        {
            Id = $"Btn_{nameof(FrmMain.MnuZoomIn)}",
            Image = nameof(Config.Theme.ToolbarIcons.ZoomIn),
            OnClick = new(nameof(FrmMain.MnuZoomIn)),
        },
        new() // MnuZoomOut
        {
            Id = $"Btn_{nameof(FrmMain.MnuZoomOut)}",
            Image = nameof(Config.Theme.ToolbarIcons.ZoomOut),
            OnClick = new(nameof(FrmMain.MnuZoomOut)),
        },
    ]);

    /// <summary>
    /// Gets the default toolbar button IDs.
    /// </summary>
    public static readonly FrozenSet<string> DefaultToolbarItemIds = FrozenSet.ToFrozenSet<string>([
        $"Btn_{nameof(FrmMain.MnuOpenFile)}",
        $"Btn_{nameof(FrmMain.MnuViewPrevious)}",
        $"Btn_{nameof(FrmMain.MnuViewNext)}",

        nameof(ToolbarItemModelType.Separator),
        $"Btn_{nameof(FrmMain.MnuRotateLeft)}",
        $"Btn_{nameof(FrmMain.MnuRotateRight)}",
        $"Btn_{nameof(FrmMain.MnuFlipHorizontal)}",
        $"Btn_{nameof(FrmMain.MnuFlipVertical)}",
        $"Btn_{nameof(FrmMain.MnuCropTool)}",

        nameof(ToolbarItemModelType.Separator),
        $"Btn_{nameof(FrmMain.MnuAutoZoom)}",
        $"Btn_{nameof(FrmMain.MnuLockZoom)}",
        $"Btn_{nameof(FrmMain.MnuScaleToWidth)}",
        $"Btn_{nameof(FrmMain.MnuScaleToHeight)}",
        $"Btn_{nameof(FrmMain.MnuScaleToFit)}",
        $"Btn_{nameof(FrmMain.MnuScaleToFill)}",

        nameof(ToolbarItemModelType.Separator),
        $"Btn_{nameof(FrmMain.MnuRefresh)}",
        $"Btn_{nameof(FrmMain.MnuToggleGallery)}",
        $"Btn_{nameof(FrmMain.MnuToggleCheckerboard)}",
        $"Btn_{nameof(FrmMain.MnuFullScreen)}",
        $"Btn_{nameof(FrmMain.MnuSlideshow)}",

        nameof(ToolbarItemModelType.Separator),
        $"Btn_{nameof(FrmMain.MnuMoveToRecycleBin)}",
    ]);



    #region LazyInitializer Properties

    /// <summary>
    /// Gets settings window.
    /// </summary>
    public static FrmSettings FrmSettings
    {
        get => LazyInitializer.EnsureInitialized(ref _frmSetting);
        set => _frmSetting = value;
    }

    #endregion


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
    public delegate void ImageUnloadedHandler(ImageEventArgs e);

    /// <summary>
    /// Occurs when the first image is reached.
    /// </summary>
    public static event FirstImageReachedHandler? FirstImageReached;
    public delegate void FirstImageReachedHandler(ImageEventArgs e);

    /// <summary>
    /// Occurs when the last image is reached.
    /// </summary>
    public static event LastImageReachedHandler? LastImageReached;
    public delegate void LastImageReachedHandler(ImageEventArgs e);

    /// <summary>
    /// Occurs when the FrmMain's state needs to be updated.
    /// </summary>
    public static event FrmMainUpdateRequestedHandler? FrmMainUpdateRequested;
    public delegate void FrmMainUpdateRequestedHandler(UpdateRequestEventArgs e);

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

        var filesList = new List<string>();
        if (Local.ToolPipeServers.Count > 0)
        {
            filesList.AddRange(Local.Images.FileNames);
        }

        var eventArgs = new IgImageListUpdatedEventArgs()
        {
            InitFilePath = e.InitFilePath,
            Files = filesList,
        };

        // emit event to all tools
        _ = BroadcastMessageToAllToolServersAsync(ImageGlassEvents.IMAGE_LIST_UPDATED, eventArgs);
    }


    /// <summary>
    /// Raise <see cref="ImageLoading"/> event.
    /// </summary>
    public static void RaiseImageLoadingEvent(ImageLoadingEventArgs e)
    {
        ImageLoading?.Invoke(e);

        // emit event to all tools
        _ = BroadcastMessageToAllToolServersAsync(ImageGlassEvents.IMAGE_LOADING, new IgImageLoadingEventArgs()
        {
            Index = e.Index,
            NewIndex = e.NewIndex,
            FilePath = e.FilePath,
            FrameIndex = e.FrameIndex,
            IsViewingSeparateFrame = e.IsViewingSeparateFrame,
        });
    }


    /// <summary>
    /// Raise <see cref="ImageLoaded"/> event.
    /// </summary>
    public static void RaiseImageLoadedEvent(ImageLoadedEventArgs e)
    {
        ImageLoaded?.Invoke(e);

        // emit event to all tools
        _ = BroadcastMessageToAllToolServersAsync(ImageGlassEvents.IMAGE_LOADED, new IgImageLoadedEventArgs()
        {
            Index = e.Index,
            FilePath = e.FilePath,
            FrameIndex = e.FrameIndex,
            IsViewingSeparateFrame = e.IsViewingSeparateFrame,
            IsError = e.Error != null,
        });
    }


    /// <summary>
    /// Raise <see cref="ImageUnloaded"/> event.
    /// </summary>
    public static void RaiseImageUnloadedEvent(ImageEventArgs e)
    {
        ImageUnloaded?.Invoke(e);

        // emit event to all tools
        _ = BroadcastMessageToAllToolServersAsync(ImageGlassEvents.IMAGE_UNLOADED, new IgImageUnloadedEventArgs()
        {
            Index = e.Index,
            FilePath = e.FilePath,
        });
    }


    /// <summary>
    /// Raise <see cref="FirstImageReached"/> event.
    /// </summary>
    public static void RaiseFirstImageReachedEvent(ImageEventArgs e)
    {
        FirstImageReached?.Invoke(e);
    }


    /// <summary>
    /// Raise <see cref="LastImageReached"/> event.
    /// </summary>
    public static void RaiseLastImageReachedEvent(ImageEventArgs e)
    {
        LastImageReached?.Invoke(e);
    }


    /// <summary>
    /// Raise <see cref="FrmMainUpdateRequested"/> event.
    /// </summary>
    public static void UpdateFrmMain(UpdateRequests requests, Action<UpdateRequests>? onUpdated = null)
    {
        FrmMainUpdateRequested?.Invoke(new UpdateRequestEventArgs()
        {
            Requests = requests,
            OnUpdated = onUpdated,
        });
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
    public static Dictionary<string, ToolForm?> Tools { get; set; } = [];

    /// <summary>
    /// Gets, sets the list of tool pipe servers.
    /// </summary>
    public static Dictionary<string, PipeServer?> ToolPipeServers { get; set; } = [];

    /// <summary>
    /// Gets, sets the metadata of the current image in the list.
    /// </summary>
    public static IgMetadata? Metadata { get; set; }

    /// <summary>
    /// Gets, sets app state.
    /// </summary>
    public static bool IsBusy { get; set; }

    /// <summary>
    /// Gets, sets images list
    /// </summary>
    public static ImageBooster Images { get; set; } = new();

    /// <summary>
    /// Gets, sets index of the viewing image
    /// </summary>
    public static int CurrentIndex { get; set; } = -1;

    /// <summary>
    /// Gets, sets the current frame index of the viewing image
    /// </summary>
    public static uint CurrentFrameIndex { get; set; }

    /// <summary>
    /// Gets, sets the changes of the current viewing image.
    /// </summary>
    public static ImgTransform ImageTransform = new();

    /// <summary>
    /// Gets, sets the value if the current image is error
    /// </summary>
    public static bool IsImageError { get; set; }

    /// <summary>
    /// <para>The current "initial" path (file or dir) we're viewing. Used when the user changes the sort settings: we need to rebuild the image list, but otherwise we don't know what image/folder we started with.</para>
    /// <para>Here's what happened: I opened a folder with subfolders (child folders enabled). I was going through the images, and decided I wanted to change the sort order. Since the _current_ image was in a sub-folder, after a rescan of the image list, only the _sub_-folders images were re-loaded!</para>
    /// <para>But if we reload the list using the original image, then the original folder's images, and the sub-folders, are reloaded.</para>
    /// </summary>
    public static string InitialInputPath { get; set; } = string.Empty;

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
    public static string SaveAsFilterExt { get; set; } = string.Empty;

    /// <summary>
    /// Gets, sets color channel of image
    /// </summary>
    public static ColorChannel ImageChannel { get; set; } = ColorChannel.All;

    /// <summary>
    /// Gets, sets value if image data was modified
    /// </summary>
    public static string ImageModifiedPath { get; set; } = string.Empty;

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
    public static List<string> StringClipboard { get; set; } = [];

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
            DistinctDirs = distinctDirsList ?? [],
        };
    }


    /// <summary>
    /// Quickly save the viewing image as temporary file.
    /// </summary>
    public static async Task<string?> SaveImageAsTempFileAsync(string ext = ".png")
    {
        // check if we can use the current clipboard image path
        if (File.Exists(TempImagePath))
        {
            var extension = Path.GetExtension(TempImagePath);

            if (extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                return TempImagePath;
            }
        }

        var tempDir = App.ConfigDir(PathType.Dir, Dir.Temporary);
        Directory.CreateDirectory(tempDir);

        var tempFilePath = Path.Combine(tempDir, $"temp_{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}{ext}");


        // save clipboard image
        if (ClipboardImage != null)
        {
            try
            {
                await PhotoCodec.SaveAsync(ClipboardImage, tempFilePath, Local.ImageTransform);

                TempImagePath = tempFilePath;
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
                await PhotoCodec.SaveAsync(img.ImgData.Image, tempFilePath, Local.ImageTransform);

                TempImagePath = tempFilePath;
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
        var msgData = BHelper.ToJson(data ?? string.Empty) ?? string.Empty;

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
    /// <exception cref="FileNotFoundException"></exception>
    public static async Task<PipeServer?> OpenPipedToolAsync(IgTool? tool)
    {
        if (tool == null || tool.IsEmpty) throw new FileNotFoundException();
        if (ToolPipeServers.TryGetValue(tool.ToolId, out PipeServer? server)) return server;

        var isIntegrated = tool.IsIntegrated ?? false;
        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        var args = tool.Argument?.Replace(Const.FILE_MACRO, $"\"{filePath}\"");

        // tool is not integrated
        if (!isIntegrated)
        {
            using var _ = ImageGlassTool.LaunchTool(tool.Executable, args, false);
            return null;
        }


        // Create new tool server
        #region Create new tool server

        // create pipe code to send to client
        // example: 9d5420cd322c49bc9abf8a48039d801a
        var pipeCode = Guid.NewGuid().ToString("N");

        // prepend tool prefix to create pipe name
        var pipeName = ImageGlassTool.CreateServerName(pipeCode);

        // create a new tool server
        var toolServer = new PipeServer(pipeName, PipeDirection.InOut);
        toolServer.ClientDisconnected += ToolServer_ClientDisconnected;

        Local.ToolPipeServers.Add(tool.ToolId, toolServer);

        // start the server
        toolServer.Start();
        await Config.WriteAsync();

        #endregion // Create new tool server


        // start tool client
        #region Start tool client
        Process? toolProc = null;

        try
        {
            var pipeCodeCmd = $"{ImageGlassTool.PIPE_CODE_CMD_LINE}{pipeCode}";

            toolProc = ImageGlassTool.LaunchTool(tool.Executable,
                $"{args} {pipeCodeCmd}", false);
        }
        catch (FileNotFoundException ex)
        {
            toolServer.Stop();
            toolServer.Dispose();
            toolServer = null;
            Local.ToolPipeServers.Remove(tool.ToolId);
            throw ex;
        }
        #endregion // Start tool client


        // wait for client connection
        await toolServer.WaitForConnectionAsync();
        toolServer.TagNumber = toolProc.Id; // save tool client process id
        await Task.Delay(1000); // wait for tool window ready

        // set tool client window top most
        WindowApi.SetWindowTopMost(toolProc.MainWindowHandle, Config.EnableWindowTopMost);
        toolProc?.Dispose();


        return toolServer;
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
            .Equals(e.PipeName, StringComparison.OrdinalIgnoreCase));

        if (item.Key is not string toolId) return;
        var toolServer = item.Value;

        // update menu state
        if (FrmMain.MnuTools.DropDownItems[toolId] is ToolStripMenuItem mnuItem)
        {
            mnuItem.Checked = false;
        }

        // remove events
        if (toolServer != null)
        {
            toolServer.Stop();
            toolServer.ClientDisconnected -= ToolServer_ClientDisconnected;
            toolServer.Dispose();
            toolServer = null;
        }

        // remove tool server
        Local.ToolPipeServers.Remove(toolId);
    }


    /// <summary>
    /// Closes <see cref="PipeServer"/> tool.
    /// </summary>
    public static async Task ClosePipedToolAsync(IgTool? tool,
        Action<PipeServer>? toolServerClosingCallBack = null)
    {
        if (tool == null
            || !Local.ToolPipeServers.TryGetValue(tool.ToolId, out var toolServer)
            || toolServer is null) return;

        if (toolServer.ServerStream.IsConnected)
        {
            var json = BHelper.ToJson(new IgToolTernimatingEventArgs());
            await toolServer.SendAsync(ImageGlassEvents.TOOL_TERMINATE, json);
        }

        toolServerClosingCallBack?.Invoke(toolServer);

        // remove tool server
        ToolServer_ClientDisconnected(null, new DisconnectedEventArgs(toolServer.PipeName));
    }


    /// <summary>
    /// Delay calling <see cref="GC.Collect"/> for <paramref name="delayMs"/> milliseconds.
    /// </summary>
    public static void GcCollect(int delayMs = 500)
    {
        _gcTokenSrc?.Cancel();
        _gcTokenSrc = new();

        _ = GCCollectAsync(delayMs, _gcTokenSrc.Token);
    }

    private static async Task GCCollectAsync(int delayMs, CancellationToken token)
    {
        try
        {
            // check if task is cancelled
            token.ThrowIfCancellationRequested();

            await Task.Delay(delayMs, token);

            // check if task is cancelled
            token.ThrowIfCancellationRequested();

            // Collect system garbage
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
    }


    #endregion // Public functions

}