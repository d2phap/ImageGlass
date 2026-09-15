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
using ImageGlass.Common.Actions;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.Tools;
using ImageGlass.UI;
using ImageGlass.UI.Viewer;
using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace ImageGlass.Common;

public partial class Config
{
    #region Public static properties

    /// <summary>
    /// App setting specs version, to check for compatibility.
    /// </summary>
    [JsonIgnore]
    public static float SPEC_VERSION => 10f;


    /// <summary>
    /// Gets the user config file name.
    /// </summary>
    [JsonIgnore]
    public static string CONFIG_USER => "igconfig.json";


    /// <summary>
    /// Gets the default config file name. Looked up in the startup dir first, then the config dir.
    /// </summary>
    [JsonIgnore]
    public static string CONFIG_DEFAULT => "igconfig.default.json";


    /// <summary>
    /// Gets the admin config file name. Looked up in the startup dir first, then the config dir.
    /// </summary>
    [JsonIgnore]
    public static string CONFIG_ADMIN => "igconfig.admin.json";


    /// <summary>
    /// Gets the exception while loading app settings.
    /// </summary>
    [JsonIgnore]
    public static Exception? LoadingException { get; private set; } = null;


    /// <summary>
    /// Gets the exception from the last <see cref="SaveAsync"/>, or <c>null</c> if it succeeded.
    /// </summary>
    [JsonIgnore]
    public static Exception? SavingException { get; private set; } = null;


    /// <summary>
    /// Path of the ignored incompatible user config file, or <c>null</c> when none.
    /// </summary>
    [JsonIgnore]
    public static string? IncompatibleUserConfigPath { get; private set; } = null;


    /// <summary>
    /// Config ids locked by <see cref="CONFIG_ADMIN"/>.
    /// </summary>
    [JsonIgnore]
    public static FrozenSet<ConfigId> AdminLockedConfigs { get; private set; } = FrozenSet<ConfigId>.Empty;


    /// <summary>
    /// API names locked by <see cref="CONFIG_ADMIN"/>; not a <see cref="ConfigId"/>, so no user-writable layer can set it.
    /// </summary>
    [JsonIgnore]
    public static FrozenSet<string> LockedFeatures { get; private set; } = FrozenSet<string>.Empty;


    /// <summary>
    /// Settings that need a Pro license.
    /// </summary>
    [JsonIgnore]
    public static FrozenSet<ConfigId> ProGatedConfigs { get; } = new[]
    {
        ConfigId.EnableFileWatcher,
        ConfigId.EnableAutoOpenNewAddedImage,
        ConfigId.MouseClickActions,
        ConfigId.EnableFreePan,
        ConfigId.PanMargin,
        ConfigId.EnableAutoInstallUpdate,
    }.ToFrozenSet();


    /// <summary>
    /// Gets the default image formats.
    /// </summary>
    [JsonIgnore]
    public static ReadOnlyCollection<string> DefaultFileFormats { get; } = new(
        Const.IMAGE_FORMATS.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    );


    /// <summary>
    /// Gets the default image info tags.
    /// </summary>
    [JsonIgnore]
    public static ReadOnlyCollection<string> DefaultImageInfoTags { get; } = new([
        nameof(AppStatusInfo.Name),
        nameof(AppStatusInfo.ListCount),
        nameof(AppStatusInfo.FrameCount),
        nameof(AppStatusInfo.Zoom),
        nameof(AppStatusInfo.Dimension),
        nameof(AppStatusInfo.FileSize),
        nameof(AppStatusInfo.ColorSpace),
        nameof(AppStatusInfo.HdrInfo),
        nameof(AppStatusInfo.ExifRating),
        nameof(AppStatusInfo.DateTimeAuto),
        nameof(AppStatusInfo.AppName),
    ]);


    /// <summary>
    /// Gets the default mouse wheel actions.
    /// </summary>
    [JsonIgnore]
    public static Dictionary<MouseWheelEvent, MouseWheelAction> DefaultMouseWheelActions { get; } = new()
    {
        [MouseWheelEvent.Scroll] = MouseWheelAction.Zoom,
        [MouseWheelEvent.CtrlAndScroll] = MouseWheelAction.PanVertically,
        [MouseWheelEvent.ShiftAndScroll] = MouseWheelAction.PanHorizontally,
        [MouseWheelEvent.AltAndScroll] = MouseWheelAction.BrowseImages,
    };


    /// <summary>
    /// Gets the default mouse click actions.
    /// </summary>
    [JsonIgnore]
    public static Dictionary<MouseClickEvent, SingleAction> DefaultMouseClickActions { get; } = new()
    {
        [MouseClickEvent.LeftDoubleClick] = new SingleAction(API.IG_SetZoomForMouseClick),
        [MouseClickEvent.RightClick] = new SingleAction(API.IG_OpenContextMenu),
        [MouseClickEvent.WheelClick] = new SingleAction(API.IG_Refresh),
        [MouseClickEvent.XButton1Click] = new SingleAction(API.IG_ViewPrevious),
        [MouseClickEvent.XButton2Click] = new SingleAction(API.IG_ViewNext),
    };


    /// <summary>
    /// Gets the default toolbar items.
    /// </summary>
    [JsonIgnore]
    public static ReadOnlyCollection<ToolbarItemModel> DefaultToolbarItems =>
    [
        // open file
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.OpenFile) }",
            Image = nameof(IgThemeIcon.OpenFile),
            Text = Lang.KeysMap[LangId.Menu_MnuOpenFile],
            Alignment = ToolbarItemAlignment.Right,
            OnClick = new(LangId.Menu_MnuOpenFile, API.IG_OpenFile),
        },



        // view previous
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ViewPreviousImage)}",
            Image = nameof(IgThemeIcon.ViewPreviousImage),
            Text = Lang.KeysMap[LangId.Menu_MnuViewPrevious],
            OnClick = new(LangId.Menu_MnuViewPrevious, API.IG_ViewPrevious),
        },
        // view next
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ViewNextImage)}",
            Image = nameof(IgThemeIcon.ViewNextImage),
            Text = Lang.KeysMap[LangId.Menu_MnuViewNext],
            OnClick = new(LangId.Menu_MnuViewNext, API.IG_ViewNext),
        },
        ToolbarItemModel.Separator,


        // rotate left
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.RotateLeft)}",
            Image = nameof(IgThemeIcon.RotateLeft),
            Text = Lang.KeysMap[LangId.Menu_MnuRotateLeft],
            OnClick = new(LangId.Menu_MnuRotateLeft, API.IG_Rotate, nameof(RotateOption.Left)),
        },
        // rotate right
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.RotateRight)}",
            Image = nameof(IgThemeIcon.RotateRight),
            Text = Lang.KeysMap[LangId.Menu_MnuRotateRight],
            OnClick = new(LangId.Menu_MnuRotateRight, API.IG_Rotate, nameof(RotateOption.Right)),
        },
        // flip horz
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.FlipHorz)}",
            Image = nameof(IgThemeIcon.FlipHorz),
            Text = Lang.KeysMap[LangId.Menu_MnuFlipHorizontal],
            OnClick = new(LangId.Menu_MnuFlipHorizontal, API.IG_FlipImage, nameof(FlipOptions.Horizontal)),
        },
        // flip vert
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.FlipVert)}",
            Image = nameof(IgThemeIcon.FlipVert),
            Text = Lang.KeysMap[LangId.Menu_MnuFlipVertical],
            OnClick = new(LangId.Menu_MnuFlipVertical, API.IG_FlipImage, nameof(FlipOptions.Vertical)),
        },
        // crop
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Crop)}",
            Image = nameof(IgThemeIcon.Crop),
            Text = Lang.KeysMap[LangId.Menu_MnuCropTool],
            ConfigBinding = nameof(Config.LastOpenedTool),
            ConfigBindingValue = CropImageToolControl.TOOL_ID,
            OnClick = new(LangId.Menu_MnuCropTool, API.IG_ToggleTool, CropImageToolControl.TOOL_ID),
        },
        // color picker
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ColorPicker)}",
            Image = nameof(IgThemeIcon.ColorPicker),
            Text = Lang.KeysMap[LangId.Menu_MnuColorPicker],
            ConfigBinding = nameof(Config.LastOpenedTool),
            ConfigBindingValue = ColorPickerToolControl.TOOL_ID,
            OnClick = new(LangId.Menu_MnuColorPicker, API.IG_ToggleTool, ColorPickerToolControl.TOOL_ID),
        },
        ToolbarItemModel.Separator,


        // refresh
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Refresh)}",
            Image = nameof(IgThemeIcon.Refresh),
            Text = Lang.KeysMap[LangId.Menu_MnuRefresh],
            OnClick = new(LangId.Menu_MnuRefresh, API.IG_Refresh),
        },
        // toggle gallery
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Gallery)}",
            Image = nameof(IgThemeIcon.Gallery),
            Text = Lang.KeysMap[LangId.Menu_MnuToggleGallery],
            ConfigBinding = nameof(Config.ShowGallery),
            ConfigBindingValue = "True",
            OnClick = new(LangId.Menu_MnuToggleGallery, API.IG_ToggleGallery),
        },
        // toggle checkerboard
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Checkerboard)}",
            Image = nameof(IgThemeIcon.Checkerboard),
            Text = Lang.KeysMap[LangId.Menu_MnuToggleCheckerboard],
            ConfigBinding = nameof(Config.CheckerboardMode),
            ConfigBindingValue = $"!{nameof(CheckerboardType.None)}",
            OnClick = new(LangId.Menu_MnuToggleCheckerboard, API.IG_ToggleCheckerboard),
        },
        // toggle fullscreen
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.FullScreen)}",
            Image = nameof(IgThemeIcon.FullScreen),
            Text = Lang.KeysMap[LangId.Menu_MnuFullScreen],
            ConfigBinding = nameof(Config.EnableFullScreen),
            ConfigBindingValue = "True",
            OnClick = new(LangId.Menu_MnuFullScreen, API.IG_ToggleFullScreen),
        },
        // toggle slildeshow
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Slideshow)}",
            Image = nameof(IgThemeIcon.Slideshow),
            Text = Lang.KeysMap[LangId.Menu_MnuSlideshow],
            ConfigBinding = nameof(Config.EnableSlideshow),
            ConfigBindingValue = "True",
            OnClick = new(LangId.Menu_MnuSlideshow, API.IG_ToggleSlideshow),
        },
        ToolbarItemModel.Separator,


        // delete
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Delete)}",
            Image = nameof(IgThemeIcon.Delete),
            Text = Lang.KeysMap[LangId.Menu_MnuMoveToRecycleBin],
            OnClick = new(LangId.Menu_MnuMoveToRecycleBin, API.IG_Delete),
        }
    ];


    /// <summary>
    /// Gets the catalog of all built-in toolbar buttons the user can add to the toolbar.
    /// Used by the Toolbar settings page to populate the "Available buttons" list.
    /// </summary>
    [JsonIgnore]
    public static ReadOnlyCollection<ToolbarItemModel> BuiltInToolbarItems =>
    [
        // File
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.OpenFile)}",
            Image = nameof(IgThemeIcon.OpenFile),
            Text = Lang.KeysMap[LangId.Menu_MnuOpenFile],
            OnClick = new(LangId.Menu_MnuOpenFile, API.IG_OpenFile),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Save)}",
            Image = nameof(IgThemeIcon.Save),
            Text = Lang.KeysMap[LangId.Menu_MnuSave],
            OnClick = new(LangId.Menu_MnuSave, API.IG_Save),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Print)}",
            Image = nameof(IgThemeIcon.Print),
            Text = Lang.KeysMap[LangId.Menu_MnuPrint],
            OnClick = new(LangId.Menu_MnuPrint, API.IG_Print),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Export)}",
            Image = nameof(IgThemeIcon.Export),
            Text = Lang.KeysMap[LangId.Menu_MnuExportFrames],
            OnClick = new(LangId.Menu_MnuExportFrames, API.IG_ExportImageFrames),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Edit)}",
            Image = nameof(IgThemeIcon.Edit),
            Text = Lang.KeysMap[LangId.Menu_MnuEdit],
            OnClick = new(LangId.Menu_MnuEdit, API.IG_OpenEditingApp),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Delete)}",
            Image = nameof(IgThemeIcon.Delete),
            Text = Lang.KeysMap[LangId.Menu_MnuMoveToRecycleBin],
            OnClick = new(LangId.Menu_MnuMoveToRecycleBin, API.IG_Delete, "true"),
        },

        // Navigation
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ViewPreviousImage)}",
            Image = nameof(IgThemeIcon.ViewPreviousImage),
            Text = Lang.KeysMap[LangId.Menu_MnuViewPrevious],
            OnClick = new(LangId.Menu_MnuViewPrevious, API.IG_ViewPrevious),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ViewNextImage)}",
            Image = nameof(IgThemeIcon.ViewNextImage),
            Text = Lang.KeysMap[LangId.Menu_MnuViewNext],
            OnClick = new(LangId.Menu_MnuViewNext, API.IG_ViewNext),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ViewFirstImage)}",
            Image = nameof(IgThemeIcon.ViewFirstImage),
            Text = Lang.KeysMap[LangId.Menu_MnuGoToFirst],
            OnClick = new(LangId.Menu_MnuGoToFirst, API.IG_GotoFirst),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ViewLastImage)}",
            Image = nameof(IgThemeIcon.ViewLastImage),
            Text = Lang.KeysMap[LangId.Menu_MnuGoToLast],
            OnClick = new(LangId.Menu_MnuGoToLast, API.IG_GotoLast),
        },

        // Zoom
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.AutoZoom)}",
            Image = nameof(IgThemeIcon.AutoZoom),
            Text = Lang.KeysMap[LangId.Menu_MnuAutoZoom],
            ConfigBinding = nameof(Config.ZoomMode),
            ConfigBindingValue = ZoomMode.AutoZoom.ToString(),
            OnClick = new(LangId.Menu_MnuAutoZoom, API.IG_SetZoomMode, nameof(ZoomMode.AutoZoom)),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.LockZoom)}",
            Image = nameof(IgThemeIcon.LockZoom),
            Text = Lang.KeysMap[LangId.Menu_MnuLockZoom],
            ConfigBinding = nameof(Config.ZoomMode),
            ConfigBindingValue = ZoomMode.LockZoom.ToString(),
            OnClick = new(LangId.Menu_MnuLockZoom, API.IG_SetZoomMode, nameof(ZoomMode.LockZoom)),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ScaleToWidth)}",
            Image = nameof(IgThemeIcon.ScaleToWidth),
            Text = Lang.KeysMap[LangId.Menu_MnuScaleToWidth],
            ConfigBinding = nameof(Config.ZoomMode),
            ConfigBindingValue = ZoomMode.ScaleToWidth.ToString(),
            OnClick = new(LangId.Menu_MnuScaleToWidth, API.IG_SetZoomMode, nameof(ZoomMode.ScaleToWidth)),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ScaleToHeight)}",
            Image = nameof(IgThemeIcon.ScaleToHeight),
            Text = Lang.KeysMap[LangId.Menu_MnuScaleToHeight],
            ConfigBinding = nameof(Config.ZoomMode),
            ConfigBindingValue = ZoomMode.ScaleToHeight.ToString(),
            OnClick = new(LangId.Menu_MnuScaleToHeight, API.IG_SetZoomMode, nameof(ZoomMode.ScaleToHeight)),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ScaleToFit)}",
            Image = nameof(IgThemeIcon.ScaleToFit),
            Text = Lang.KeysMap[LangId.Menu_MnuScaleToFit],
            ConfigBinding = nameof(Config.ZoomMode),
            ConfigBindingValue = ZoomMode.ScaleToFit.ToString(),
            OnClick = new(LangId.Menu_MnuScaleToFit, API.IG_SetZoomMode, nameof(ZoomMode.ScaleToFit)),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ScaleToFill)}",
            Image = nameof(IgThemeIcon.ScaleToFill),
            Text = Lang.KeysMap[LangId.Menu_MnuScaleToFill],
            ConfigBinding = nameof(Config.ZoomMode),
            ConfigBindingValue = ZoomMode.ScaleToFill.ToString(),
            OnClick = new(LangId.Menu_MnuScaleToFill, API.IG_SetZoomMode, nameof(ZoomMode.ScaleToFill)),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ActualSize)}",
            Image = nameof(IgThemeIcon.ActualSize),
            Text = Lang.KeysMap[LangId.Menu_MnuActualSize],
            OnClick = new(LangId.Menu_MnuActualSize, API.IG_SetZoom, "1"),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ZoomIn)}",
            Image = nameof(IgThemeIcon.ZoomIn),
            Text = Lang.KeysMap[LangId.Menu_MnuZoomIn],
            OnClick = new(LangId.Menu_MnuZoomIn, API.IG_ZoomIn),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ZoomOut)}",
            Image = nameof(IgThemeIcon.ZoomOut),
            Text = Lang.KeysMap[LangId.Menu_MnuZoomOut],
            OnClick = new(LangId.Menu_MnuZoomOut, API.IG_ZoomOut),
        },

        // Image transforms
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.RotateLeft)}",
            Image = nameof(IgThemeIcon.RotateLeft),
            Text = Lang.KeysMap[LangId.Menu_MnuRotateLeft],
            OnClick = new(LangId.Menu_MnuRotateLeft, API.IG_Rotate, nameof(RotateOption.Left)),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.RotateRight)}",
            Image = nameof(IgThemeIcon.RotateRight),
            Text = Lang.KeysMap[LangId.Menu_MnuRotateRight],
            OnClick = new(LangId.Menu_MnuRotateRight, API.IG_Rotate, nameof(RotateOption.Right)),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.FlipHorz)}",
            Image = nameof(IgThemeIcon.FlipHorz),
            Text = Lang.KeysMap[LangId.Menu_MnuFlipHorizontal],
            OnClick = new(LangId.Menu_MnuFlipHorizontal, API.IG_FlipImage, nameof(FlipOptions.Horizontal)),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.FlipVert)}",
            Image = nameof(IgThemeIcon.FlipVert),
            Text = Lang.KeysMap[LangId.Menu_MnuFlipVertical],
            OnClick = new(LangId.Menu_MnuFlipVertical, API.IG_FlipImage, nameof(FlipOptions.Vertical)),
        },

        // Tools
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.ColorPicker)}",
            Image = nameof(IgThemeIcon.ColorPicker),
            Text = Lang.KeysMap[LangId.Menu_MnuColorPicker],
            OnClick = new(LangId.Menu_MnuColorPicker, API.IG_ToggleTool, ColorPickerToolControl.TOOL_ID),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Crop)}",
            Image = nameof(IgThemeIcon.Crop),
            Text = Lang.KeysMap[LangId.Menu_MnuCropTool],
            OnClick = new(LangId.Menu_MnuCropTool, API.IG_ToggleTool, CropImageToolControl.TOOL_ID),
        },

        // View modes
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Refresh)}",
            Image = nameof(IgThemeIcon.Refresh),
            Text = Lang.KeysMap[LangId.Menu_MnuRefresh],
            OnClick = new(LangId.Menu_MnuRefresh, API.IG_Refresh),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Gallery)}",
            Image = nameof(IgThemeIcon.Gallery),
            Text = Lang.KeysMap[LangId.Menu_MnuToggleGallery],
            ConfigBinding = nameof(Config.ShowGallery),
            ConfigBindingValue = "True",
            OnClick = new(LangId.Menu_MnuToggleGallery, API.IG_ToggleGallery),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Checkerboard)}",
            Image = nameof(IgThemeIcon.Checkerboard),
            Text = Lang.KeysMap[LangId.Menu_MnuToggleCheckerboard],
            ConfigBinding = nameof(Config.CheckerboardMode),
            ConfigBindingValue = $"!{nameof(CheckerboardType.None)}",
            OnClick = new(LangId.Menu_MnuToggleCheckerboard, API.IG_ToggleCheckerboard),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.WindowFit)}",
            Image = nameof(IgThemeIcon.WindowFit),
            Text = Lang.KeysMap[LangId.Menu_MnuWindowFit],
            ConfigBinding = nameof(Config.EnableWindowFit),
            ConfigBindingValue = "True",
            OnClick = new(LangId.Menu_MnuWindowFit, API.IG_ToggleWindowFit),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.FullScreen)}",
            Image = nameof(IgThemeIcon.FullScreen),
            Text = Lang.KeysMap[LangId.Menu_MnuFullScreen],
            ConfigBinding = nameof(Config.EnableFullScreen),
            ConfigBindingValue = "True",
            OnClick = new(LangId.Menu_MnuFullScreen, API.IG_ToggleFullScreen),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Slideshow)}",
            Image = nameof(IgThemeIcon.Slideshow),
            Text = Lang.KeysMap[LangId.Menu_MnuSlideshow],
            ConfigBinding = nameof(Config.EnableSlideshow),
            ConfigBindingValue = "True",
            OnClick = new(LangId.Menu_MnuSlideshow, API.IG_ToggleSlideshow),
        },
        new() {
            Id = $"Btn_{nameof(IgThemeIcon.Exit)}",
            Image = nameof(IgThemeIcon.Exit),
            Text = Lang.KeysMap[LangId.Menu_MnuExit],
            OnClick = new(LangId.Menu_MnuExit, API.IG_Exit),
        },
    ];

    #endregion // Public static properties



    #region Public static methods

    /// <summary>
    /// Loads and merges configs from multiple sources.
    /// Priority (lowest -> highest):
    /// developer defaults -> igconfig.default.json -> igconfig.json -> CLI args -> igconfig.admin.json.
    /// The default and admin layers are included only when the Pro license is active.
    /// </summary>
    public static Config Load(string configFileName, string[]? cliArgs = null)
    {
        Config? appConfig = null;

        // read before (and independently of) the main load: a corrupt igconfig.json aborts it into the catch
        LoadAdminPolicies();

        try
        {
            var jsonOptions = BHelper.CreateJsonOptions();
            var jsonContext = new ConfigJsonContext(jsonOptions);

            // 1. read igconfig.default.json (BaseDir, then ConfigDir). Pro only.
            using var defaultDoc = ReadDefaultConfigDocument();

            // 2. read igconfig.json (Config Dir only)
            var userConfigPath = BHelper.ConfigDir(configFileName);
            using var userDoc = BHelper.ReadJsonDocFromFile(userConfigPath);

            // 3. parse CLI -p: args
            var cliOverrides = ParseCliConfigArgs(cliArgs);

            // 4. read igconfig.admin.json (BaseDir, then ConfigDir). Merge-only layer.
            using var adminDoc = ReadAdminConfigDocument();

            // 5. drop incompatible older layers; flag an incompatible user file so startup can warn
            var effectiveDefaultDoc = IsCompatibleConfigLayer(defaultDoc) ? defaultDoc : null;
            var effectiveAdminDoc = IsCompatibleConfigLayer(adminDoc) ? adminDoc : null;
            var effectiveUserDoc = userDoc;
            if (!IsCompatibleConfigLayer(userDoc))
            {
                effectiveUserDoc = null;
                IncompatibleUserConfigPath = userConfigPath;
            }

            // 6. merge the compatible layers into a single JSON byte array
            var mergedJson = MergeJsonLayers(effectiveDefaultDoc, effectiveUserDoc, cliOverrides, effectiveAdminDoc);

            // 7. deserialize the merged JSON into Config
            var config = JsonSerializer.Deserialize(mergedJson, jsonContext.Config)
                ?? throw new FileLoadException("IGE: Could not parse merged config.");

            // 8. migrate if config version changed
            appConfig = MigrateUserConfigFile(config);

            // 9. apply persisted tool configs derived from the merged config
            ApplyPersistedToolConfigs(appConfig);
        }
        catch (Exception ex)
        {
            LoadingException = ex;
        }

        appConfig ??= new();

        // seed pre-configured external tools; also runs on a failed load so they still work
        EnsurePredefinedTools(appConfig);

        return appConfig;
    }


    /// <summary>
    /// Whether <paramref name="id"/> is locked by the admin config layer (<see cref="AdminLockedConfigs"/>).
    /// </summary>
    public static bool IsConfigLocked(ConfigId id) => AdminLockedConfigs.Contains(id);


    /// <summary>
    /// Whether <paramref name="id"/> is a Pro-only setting the active license does not unlock.
    /// </summary>
    public static bool IsConfigProGated(ConfigId id) => !Core.IsProEnabled && ProGatedConfigs.Contains(id);



    /// <summary>
    /// Seeds <see cref="PredefinedExternalTools"/> into <see cref="Tools"/>, keeping user edits.
    /// Skipped when an admin defines the setting.
    /// </summary>
    private static void EnsurePredefinedTools(Config config)
    {
        if (IsConfigLocked(ConfigId.Tools)) return;

        // the getter falls back to a throwaway collection when unset, so write the result back
        var tools = config.Tools;
        if (!PredefinedExternalTools.Seed(tools)) return;

        config.Tools = tools;
    }


    /// <summary>
    /// Applies persisted tool configs that feed runtime state living outside <see cref="Config"/>.
    /// Runs inside <see cref="Load"/> so the merged (default/user/CLI/admin) values are used and
    /// applied even when the owning tool is never opened.
    /// </summary>
    private static void ApplyPersistedToolConfigs(Config config)
    {
        // HDR tone-mapping options -> Core.HdrToneMappingConfig (the source of truth the viewer reads)
        if (config.ToolSettings.TryGetValue(HdrToneMapperToolControl.TOOL_ID, out var hdrEl))
        {
            var opts = hdrEl.Deserialize(HdrToneMappingOptionsJsonContext.Default.HdrToneMappingOptions);
            if (opts is not null) Core.HdrToneMappingConfig = opts;
        }
    }


    /// <summary>
    /// Whether a config layer's spec version is not older than <see cref="SPEC_VERSION"/>.
    /// A <c>null</c> layer, or one with missing/unparsable version, is treated as compatible.
    /// </summary>
    private static bool IsCompatibleConfigLayer(JsonDocument? doc)
    {
        return doc == null || GetConfigLayerVersion(doc) >= SPEC_VERSION;
    }


    /// <summary>
    /// Reads the spec version from a config layer's <c>_Metadata.Version</c>.
    /// Returns <see cref="SPEC_VERSION"/> when the metadata is missing or unparsable.
    /// </summary>
    private static float GetConfigLayerVersion(JsonDocument doc)
    {
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Object
            && TryGetPropertyIgnoreCase(root, nameof(_Metadata), out var meta)
            && meta.ValueKind == JsonValueKind.Object
            && TryGetPropertyIgnoreCase(meta, nameof(ConfigMetadata.Version), out var ver)
            && ver.ValueKind == JsonValueKind.Number
            && ver.TryGetSingle(out var version))
        {
            return version;
        }

        return SPEC_VERSION;
    }


    /// <summary>
    /// Case-insensitive lookup of a JSON object property (matches the merge layer's case handling).
    /// </summary>
    private static bool TryGetPropertyIgnoreCase(JsonElement obj, string name, out JsonElement value)
    {
        foreach (var prop in obj.EnumerateObject())
        {
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }

        value = default;
        return false;
    }


    /// <summary>
    /// Applies CLI config overrides (<c>-p:Key=Value</c>) to the current config instance.
    /// Used when the first instance receives forwarded args from a second instance.
    /// </summary>
    public static void ApplyCliOverrides(Config config, string[]? cliArgs)
    {
        var overrides = ParseCliConfigArgs(cliArgs);
        if (overrides.Count == 0) return;

        try
        {
            var jsonOptions = BHelper.CreateJsonOptions();
            var jsonContext = new ConfigJsonContext(jsonOptions);

            // serialize current config to JSON
            var currentJson = JsonSerializer.SerializeToUtf8Bytes(config, jsonContext.Config);
            using var currentDoc = JsonDocument.Parse(currentJson);

            // merge CLI overrides on top
            var mergedJson = MergeJsonLayers(null, currentDoc, overrides, null);

            // deserialize back into a new Config
            var updated = JsonSerializer.Deserialize(mergedJson, jsonContext.Config);
            if (updated == null) return;

            // copy all values, but never let a forwarded CLI override mutate an admin-locked setting
            // (preserves the admin > CLI precedence of Load; mirrors SettingsViewModel.CommitAsync)
            foreach (var kvp in updated._values)
            {
                if (IsConfigLocked(kvp.Key)) continue;
                config.Set(kvp.Key, kvp.Value);
            }
        }
        catch { }
    }


    /// <summary>
    /// Applies a partial <see cref="CONFIG_USER"/>-shaped JSON object onto <paramref name="config"/>, returning the changed ids.
    /// </summary>
    /// <param name="json">A JSON object, e.g. <c>{ "WindowBackdrop": "Acrylic" }</c>.</param>
    public static IReadOnlyList<ConfigId> ApplyJsonOverrides(Config config, string? json)
    {
        // 1. parse the requested settings
        using var overrideDoc = ParseSettingsJson(json);

        // 2. resolve the keys, dropping the ones policy refuses
        var overrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var targetIds = new List<ConfigId>();
        var unknownKeys = new List<string>();

        foreach (var prop in overrideDoc.RootElement.EnumerateObject())
        {
            // a whole config file may be passed in; its metadata is not a setting
            if (string.Equals(prop.Name, nameof(_Metadata), StringComparison.OrdinalIgnoreCase)) continue;

            if (!Enum.TryParse<ConfigId>(prop.Name, ignoreCase: true, out var id))
            {
                unknownKeys.Add(prop.Name);
                continue;
            }

            if (_fileOnlyKeys.Contains(prop.Name))
            {
                System.Diagnostics.Debug.WriteLine($"[Config] Setting '{id}' ignored (not allowed outside a config file).");
                continue;
            }

            if (IsConfigLocked(id))
            {
                System.Diagnostics.Debug.WriteLine($"[Config] Setting '{id}' ignored (locked by {CONFIG_ADMIN}).");
                continue;
            }

            if (IsConfigProGated(id))
            {
                System.Diagnostics.Debug.WriteLine($"[Config] Setting '{id}' ignored (requires a Pro license).");
                continue;
            }

            // key by the canonical enum name so two case variants cannot both be written
            overrides[id.ToString()] = prop.Value.GetRawText();
            targetIds.Add(id);
        }

        // 3. refuse the whole batch on a bad name, rather than applying half of it
        if (unknownKeys.Count > 0)
        {
            // no paramName: the message is shown to the user as-is
            throw new ArgumentException($"Unknown setting name(s): {string.Join(", ", unknownKeys)}.");
        }

        if (targetIds.Count == 0) return [];

        // 4. round-trip through the merge layer so each value is parsed by its own converter
        var jsonOptions = BHelper.CreateJsonOptions();
        var jsonContext = new ConfigJsonContext(jsonOptions);

        var currentJson = JsonSerializer.SerializeToUtf8Bytes(config, jsonContext.Config);
        using var currentDoc = JsonDocument.Parse(currentJson);
        var mergedJson = MergeJsonLayers(null, currentDoc, overrides, null);

        var updated = JsonSerializer.Deserialize(mergedJson, jsonContext.Config)
            ?? throw new JsonException("IGE: Could not parse the merged config.");

        // 5. copy back ONLY the requested ids; the merged config also carries every current value
        var changedIds = new List<ConfigId>(targetIds.Count);
        foreach (var id in targetIds)
        {
            if (!updated._values.TryGetValue(id, out var value)) continue;

            config.Set(id, value);
            changedIds.Add(id);
        }

        return changedIds;
    }


    /// <summary>
    /// Parses an <see cref="ApplyJsonOverrides"/> payload, reporting the reason in the message the caller shows.
    /// </summary>
    private static JsonDocument ParseSettingsJson(string? json)
    {
        const string shape = "The settings JSON must be an object of setting names.";

        if (string.IsNullOrWhiteSpace(json)) throw new JsonException($"No settings were given. {shape}");

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new JsonException($"The settings JSON could not be parsed. {ex.Message}", ex);
        }

        if (doc.RootElement.ValueKind != JsonValueKind.Object)
        {
            doc.Dispose();
            throw new JsonException(shape);
        }

        return doc;
    }


    /// <summary>
    /// Migrates user config file.
    /// </summary>
    private static Config MigrateUserConfigFile(Config config)
    {
        var configVersion = config._Metadata.Version;

        // update config version
        config._Metadata.Version = SPEC_VERSION;

        // no change
        if (SPEC_VERSION <= configVersion) return config;


        // Migration v9 to v10
        if (configVersion < 10)
        {
            // ShowCheckerboard + ShowCheckerboardOnlyImageRegion: merged into CheckerboardMode
            // ZoomLevels: change type: number[] to string
        }

        return config;
    }


    /// <summary>
    /// Gets the user theme-packs folder (in the Config dir), where installed theme packs live.
    /// </summary>
    [JsonIgnore]
    public static string ThemePacksDir => BHelper.ConfigDir(Dir.Themes);


    /// <summary>
    /// Loads every installed theme pack: built-in packs from the app base dir and user packs from
    /// the Config dir. Packs are de-duplicated by folder name (a user pack shadows a built-in one of
    /// the same name); invalid packs are skipped. The result is sorted by display name.
    /// </summary>
    public static async Task<List<IgTheme>> LoadAllThemePacksAsync()
    {
        var found = new Dictionary<string, IgTheme>(StringComparer.OrdinalIgnoreCase);

        // base dir (built-in) first, then Config dir (user) so user packs win on a name clash
        foreach (var rootDir in new[] { BHelper.BaseDir(Dir.Themes), BHelper.ConfigDir(Dir.Themes) })
        {
            if (!Directory.Exists(rootDir)) continue;

            foreach (var themeDir in Directory.EnumerateDirectories(rootDir))
            {
                var th = await new IgTheme().LoadAsync(themeDir).ConfigureAwait(false);
                if (th.IsValid) found[th.FolderName] = th;
            }
        }

        return found.Values
            .OrderBy(t => t.Info.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }


    /// <summary>
    /// Whether a theme pack is a built-in (shipped under the app base dir) and therefore not removable.
    /// </summary>
    public static bool IsBuiltInThemePack(IgTheme theme)
    {
        var baseThemesDir = BHelper.BaseDir(Dir.Themes);
        return theme.FolderPath.StartsWith(baseThemesDir, StringComparison.OrdinalIgnoreCase);
    }


    /// <summary>
    /// Installs <c>.igtheme.zip</c> packs into the user theme folder, skipping incompatible ones.
    /// </summary>
    public static async Task<ThemePackInstallResult> InstallThemePacksAsync(IEnumerable<string> igThemeFilePaths)
    {
        var themesRoot = ThemePacksDir;
        Directory.CreateDirectory(themesRoot);

        return await Task.Run(() =>
        {
            var installed = 0;
            var incompatible = new List<string>();

            foreach (var file in igThemeFilePaths)
            {
                if (!File.Exists(file)) continue;

                if (InstallOneThemePack(file, themesRoot)) installed++;
                else incompatible.Add(GetThemePackFileName(file));
            }

            return new ThemePackInstallResult(installed, incompatible);
        }).ConfigureAwait(false);
    }


    /// <summary>
    /// Removes a user-installed theme pack folder. Built-in packs cannot be removed.
    /// Returns <c>true</c> when the pack folder was deleted.
    /// </summary>
    public static bool UninstallThemePack(IgTheme theme)
    {
        if (IsBuiltInThemePack(theme)) return false;
        if (string.IsNullOrEmpty(theme.FolderPath) || !Directory.Exists(theme.FolderPath)) return false;

        try
        {
            Directory.Delete(theme.FolderPath, true);
            return true;
        }
        catch { }

        return false;
    }


    #endregion // Public static methods



    #region Public methods


    /// <summary>
    /// Writes configs to file. Never throws: a losing writer must not kill a closing window.
    /// </summary>
    /// <returns><c>false</c> if the file was not written; the reason is <see cref="SavingException"/>.</returns>
    public async Task<bool> SaveAsync()
    {
        // portable mode could not claim its folder; the app is quitting, never write anywhere else
        if (ConfigMode.PortableError is not null) return false;

        var jsonFilePath = BHelper.ConfigDir(CONFIG_USER);
        var jsonOptions = BHelper.CreateJsonOptions();
        var jsonContext = new ConfigJsonContext(jsonOptions);

        try
        {
            await BHelper.WriteJsonToFileAsync(jsonFilePath, this, jsonContext.Config);
            SavingException = null;
            return true;
        }
        catch (Exception ex)
        {
            SavingException = ex;
            System.Diagnostics.Debug.WriteLine($"[Config] Could not save '{jsonFilePath}': {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// Loads app language <see cref="Config.Lang"/>.
    /// </summary>
    public async Task LoadCurrentLanguageAsync()
    {
        var langPath = Lang.ResolveFilePath(Language);
        var isInvalid = !Lang.IsPackFileCompatible(langPath);

        // an incompatible pack (e.g. a hand-edited config) falls back to built-in English
        if (isInvalid) langPath = string.Empty;

        var lang = new Lang(langPath);

        // load language pack
        await lang.LoadAsync();

        // set app language
        Core.Lang = lang;
        Core.Config.Language = lang.FileName;
    }


    /// <summary>
    /// Loads theme pack <see cref="Config.Theme"/>.
    /// </summary>
    /// <param name="darkMode">
    /// Determine which theme should be loaded: <see cref="DarkTheme"/> or <see cref="LightTheme"/>.
    /// </param>
    /// <param name="useFallBackTheme">
    /// If theme pack is invalid, should load the default theme pack <see cref="Const.DEFAULT_THEME"/>.
    /// </param>
    /// <param name="throwIfThemeInvalid">
    /// If theme pack is invalid, should throw exception.
    /// </param>
    /// <param name="forceUpdateBackground">Force updating background according to theme value</param>
    /// <exception cref="ArgumentException"></exception>
    public async Task<bool> LoadCurrentThemeAsync(bool darkMode,
        bool useFallBackTheme, bool throwIfThemeInvalid, bool forceUpdateBackground)
    {
        // 1. get the theme folder name
        var themeFolderName = darkMode ? DarkTheme : LightTheme;
        if (string.IsNullOrEmpty(themeFolderName))
        {
            themeFolderName = Const.DEFAULT_THEME;
        }

        // 2. check if theme pack is already loaded
        if (themeFolderName.Equals(Core.Theme.FolderName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 3. load theme pack
        var th = await FindAndLoadThemePackAsync(themeFolderName, useFallBackTheme, throwIfThemeInvalid);

        // 4. update the name of dark/light theme
        if (darkMode) DarkTheme = th.FolderName;
        else LightTheme = th.FolderName;


        // 5. follow the theme's bg unless the user set a custom one (= a stored value matching no
        //    theme it could have been following). The IsValid guard matters: at startup Core.Theme
        //    is a placeholder whose hardcoded bg would clobber a custom color equal to it.
        var hasStoredBg = HasValue(ConfigId.BackgroundColor) && !string.IsNullOrWhiteSpace(BackgroundColor);
        var currentBg = BHelper.ColorFromHex(BackgroundColor);
        var isThemePackBg = currentBg == BHelper.ColorFromHex(Core.Theme.Colors.BgColor);
        var isFollowingTheme = !hasStoredBg
            || (Core.Theme.IsValid && isThemePackBg)
            || currentBg == BHelper.ColorFromHex(th.Colors.BgColor);

        // if not matched, it may have followed the other OS-mode's theme in the previous session
        if (!isFollowingTheme && !forceUpdateBackground)
        {
            var otherName = darkMode ? LightTheme : DarkTheme;
            if (!string.IsNullOrEmpty(otherName)
                && !otherName.Equals(th.FolderName, StringComparison.OrdinalIgnoreCase))
            {
                var other = await FindAndLoadThemePackAsync(otherName, useFallBackTheme: false,
                    throwIfThemeInvalid: false);
                isFollowingTheme = other.IsValid && currentBg == BHelper.ColorFromHex(other.Colors.BgColor);
            }
        }

        if (isFollowingTheme || forceUpdateBackground)
        {
            BackgroundColor = th.Colors.BgColor;
        }


        // 6. set to the current theme
        var success = Core.SetTheme(th);
        return success;
    }


    /// <summary>
    /// Finds the correct location of theme name and loads it.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    private static async Task<IgTheme> FindAndLoadThemePackAsync(string themeFolderName,
        bool useFallBackTheme, bool throwIfThemeInvalid)
    {
        // 1. look for theme pack in the Config dir
        var themeConfigPath = BHelper.ConfigDir(Dir.Themes, themeFolderName);
        var th = await new IgTheme().LoadAsync(themeConfigPath);

        if (!th.IsValid)
        {
            // 2. look for theme pack in the base dir
            var baseThemeConfigPath = BHelper.BaseDir(Dir.Themes, themeFolderName);
            th = await new IgTheme().LoadAsync(baseThemeConfigPath);

            // 3. cannot find theme, use fall back theme
            if (!th.IsValid && useFallBackTheme)
            {
                // 4. load default theme
                baseThemeConfigPath = BHelper.BaseDir(Dir.Themes, Const.DEFAULT_THEME);
                th = await new IgTheme().LoadAsync(baseThemeConfigPath);
            }
        }

        // 5. throw error if theme is invalid
        if (!th.IsValid && throwIfThemeInvalid)
        {
            throw new ArgumentException($"IGE: Unable to load '{themeFolderName}' theme pack. " +
                $"Please make sure '{themeConfigPath}' file is valid.", nameof(themeFolderName));
        }

        return th;
    }


    /// <summary>
    /// Gets control layout position.
    /// </summary>
    public static LayoutPosition GetControlLayout(LayoutControl control)
    {
        var defaultPos = control == LayoutControl.Toolbar
            ? LayoutPosition.Top
            : LayoutPosition.Bottom;


        // 1. read control's layouts from setting
        var pos = Core.Config.Layout.GetValueOrDefault(control, defaultPos);


        // 2. standardize toolbar position
        if (control == LayoutControl.Toolbar)
        {
            if (pos is LayoutPosition.Left or LayoutPosition.Right)
            {
                pos = LayoutPosition.Top;
            }
        }

        return pos;
    }


    #endregion // Public methods



    #region Private static methods (config merge)

    /// <summary>
    /// Security-sensitive keys accepted only from a config file, never from CLI args or an action argument.
    /// </summary>
    private static readonly HashSet<string> _fileOnlyKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Tools),
        nameof(PluginTrust),
    };


    /// <summary>
    /// Resolves a config file: primary path first, then an optional fallback path.
    /// Returns <c>null</c> if neither exists.
    /// </summary>
    private static string? ResolveConfigJsonPath(string primaryPath, string? fallbackPath = null)
    {
        return File.Exists(primaryPath) ? primaryPath
            : (!string.IsNullOrEmpty(fallbackPath) && File.Exists(fallbackPath)) ? fallbackPath
            : null;
    }


    /// <summary>
    /// Reads a JSON config file: primary path first, then an optional fallback path.
    /// Returns <c>null</c> if neither exists.
    /// </summary>
    private static JsonDocument? ReadConfigJsonDocument(string primaryPath, string? fallbackPath = null)
    {
        var path = ResolveConfigJsonPath(primaryPath, fallbackPath);
        if (string.IsNullOrEmpty(path)) return null;

        return BHelper.ReadJsonDocFromFile(path);
    }


    /// <summary>
    /// Reads a Pro-only config layer: install BaseDir first, then ConfigDir, so a read-only install
    /// can still ship it. Returns <c>null</c> when the license is inactive or no file exists.
    /// </summary>
    private static JsonDocument? ReadProConfigDocument(string configFileName)
    {
        var primaryPath = BHelper.BaseDir(configFileName);
        var fallbackPath = BHelper.ConfigDir(configFileName);

        if (!Core.IsProEnabled)
        {
            // name the deployed file it refused: otherwise the layer vanishes with no trace
            var refusedPath = ResolveConfigJsonPath(primaryPath, fallbackPath);
            if (!string.IsNullOrEmpty(refusedPath))
            {
                System.Diagnostics.Debug.WriteLine($"[Config] '{refusedPath}' ignored (requires a Pro license).");
            }

            return null;
        }

        return ReadConfigJsonDocument(primaryPath, fallbackPath);
    }


    /// <summary>
    /// Reads <see cref="CONFIG_DEFAULT"/>, the sole reader of it. Pro only.
    /// </summary>
    private static JsonDocument? ReadDefaultConfigDocument() => ReadProConfigDocument(CONFIG_DEFAULT);


    /// <summary>
    /// Reads <see cref="CONFIG_ADMIN"/>, the sole reader of it. Pro only. The BaseDir copy always
    /// wins so a user cannot override a machine-wide admin config from AppData.
    /// </summary>
    private static JsonDocument? ReadAdminConfigDocument() => ReadProConfigDocument(CONFIG_ADMIN);


    /// <summary>
    /// Parses CLI arguments with <see cref="Const.CONFIG_CMD_PREFIX"/> prefix
    /// into a dictionary of property-name -> raw-JSON-value pairs.
    /// Example: <c>-p:ShowGallery=true</c> -> <c>{ "ShowGallery": "true" }</c>.
    /// </summary>
    private static Dictionary<string, string> ParseCliConfigArgs(string[]? args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (args == null) return result;

        foreach (var arg in args)
        {
            if (!arg.StartsWith(Const.CONFIG_CMD_PREFIX, StringComparison.OrdinalIgnoreCase))
                continue;

            // strip prefix, e.g. "-p:ShowGallery=true" -> "ShowGallery=true"
            var kvPart = arg[Const.CONFIG_CMD_PREFIX.Length..];
            var eqIdx = kvPart.IndexOf('=');
            if (eqIdx <= 0) continue;

            var key = kvPart[..eqIdx].Trim();
            var value = kvPart[(eqIdx + 1)..].Trim();
            if (key.Length == 0) continue;

            // Security: never accept security-sensitive keys from the command line.
            if (_fileOnlyKeys.Contains(key))
            {
                System.Diagnostics.Debug.WriteLine($"[Config] CLI override for '{key}' ignored (not allowed from the command line).");
                continue;
            }

            result[key] = value;
        }

        return result;
    }


    /// <summary>
    /// Merges JSON config layers into one UTF-8 byte array; a later layer replaces a whole top-level property.
    /// <paramref name="rawOverrides"/> is unparsed JSON text: CLI <c>-p:</c> args, or an <see cref="ApplyJsonOverrides"/> payload.
    /// </summary>
    private static byte[] MergeJsonLayers(
        JsonDocument? defaultDoc,
        JsonDocument? userDoc,
        Dictionary<string, string> rawOverrides,
        JsonDocument? adminDoc)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions
        {
            Indented = false,
            SkipValidation = false,
        });

        writer.WriteStartObject();

        // index each layer once for O(1) case-insensitive lookups (was O(n^2): a linear
        // EnumerateObject scan per key, up to 4x per key across the layers)
        var defaultMap = IndexProperties(defaultDoc);
        var userMap = IndexProperties(userDoc);
        var adminMap = IndexProperties(adminDoc);

        // collect all property names across all layers
        var allKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        allKeys.UnionWith(defaultMap.Keys);
        allKeys.UnionWith(userMap.Keys);
        foreach (var k in rawOverrides.Keys) allKeys.Add(k);
        allKeys.UnionWith(adminMap.Keys);

        foreach (var key in allKeys)
        {
            // admin > overrides > user > default (last wins)
            if (adminMap.TryGetValue(key, out var adminVal))
            {
                writer.WritePropertyName(key);
                adminVal.WriteTo(writer);
            }
            else if (rawOverrides.TryGetValue(key, out var raw))
            {
                WriteRawValue(writer, key, raw);
            }
            else if (userMap.TryGetValue(key, out var userVal))
            {
                writer.WritePropertyName(key);
                userVal.WriteTo(writer);
            }
            else if (defaultMap.TryGetValue(key, out var defVal))
            {
                writer.WritePropertyName(key);
                defVal.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
        writer.Flush();

        return buffer.WrittenSpan.ToArray();
    }


    /// <summary>
    /// Indexes a <see cref="JsonDocument"/>'s top-level properties into a case-insensitive map.
    /// First occurrence wins, matching the previous first-match lookup behavior.
    /// </summary>
    private static Dictionary<string, JsonElement> IndexProperties(JsonDocument? doc)
    {
        var map = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        if (doc == null) return map;

        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            map.TryAdd(prop.Name, prop.Value);
        }

        return map;
    }


    /// <summary>
    /// Captures <see cref="AdminLockedConfigs"/> and <see cref="LockedFeatures"/> from an own guarded read of <see cref="CONFIG_ADMIN"/>, so admin enforcement survives an unreadable user config.
    /// </summary>
    private static void LoadAdminPolicies()
    {
        try
        {
            using var adminDoc = ReadAdminConfigDocument();
            var effectiveAdminDoc = IsCompatibleConfigLayer(adminDoc) ? adminDoc : null;

            AdminLockedConfigs = BuildAdminLockedConfigs(effectiveAdminDoc);
            LockedFeatures = BuildLockedFeatures(effectiveAdminDoc);
        }
        catch
        {
            AdminLockedConfigs = FrozenSet<ConfigId>.Empty;
            LockedFeatures = FrozenSet<string>.Empty;
        }
    }


    /// <summary>
    /// Builds the set of <see cref="ConfigId"/>s the admin layer defines (and therefore locks).
    /// Only top-level property names that map to a known <see cref="ConfigId"/> are included;
    /// <c>_Metadata</c> and any unknown keys are ignored. Returns an empty set for a <c>null</c>
    /// or non-object admin document.
    /// </summary>
    private static FrozenSet<ConfigId> BuildAdminLockedConfigs(JsonDocument? adminDoc)
    {
        if (adminDoc is null || adminDoc.RootElement.ValueKind != JsonValueKind.Object)
            return FrozenSet<ConfigId>.Empty;

        var ids = new HashSet<ConfigId>();
        foreach (var prop in adminDoc.RootElement.EnumerateObject())
        {
            // match case-insensitively like the merge; _Metadata never parses to a ConfigId
            if (Enum.TryParse<ConfigId>(prop.Name, ignoreCase: true, out var id)) ids.Add(id);
        }

        return ids.ToFrozenSet();
    }


    /// <summary>
    /// Reads the <see cref="LockedFeatures"/> API names from the admin layer; empty unless the key holds an array.
    /// </summary>
    private static FrozenSet<string> BuildLockedFeatures(JsonDocument? adminDoc)
    {
        if (adminDoc is null || adminDoc.RootElement.ValueKind != JsonValueKind.Object)
            return FrozenSet<string>.Empty;

        if (!TryGetPropertyIgnoreCase(adminDoc.RootElement, nameof(LockedFeatures), out var value)
            || value.ValueKind != JsonValueKind.Array)
            return FrozenSet<string>.Empty;

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String) continue;

            var name = item.GetString()?.Trim();
            if (!string.IsNullOrEmpty(name)) names.Add(name);
        }

        return names.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }


    /// <summary>
    /// Writes an override value as a JSON property.
    /// Attempts to parse the value as JSON first; falls back to writing as a string.
    /// </summary>
    private static void WriteRawValue(Utf8JsonWriter writer, string key, string rawValue)
    {
        writer.WritePropertyName(key);

        // try parsing as valid JSON (handles true, false, null, numbers, arrays, objects)
        try
        {
            using var doc = JsonDocument.Parse(rawValue);
            doc.RootElement.WriteTo(writer);
        }
        catch (JsonException)
        {
            // not valid JSON (a bare CLI value), write as a quoted string
            writer.WriteStringValue(rawValue);
        }
    }

    #endregion // Private static methods (config merge)



    #region Private static methods (theme pack)

    /// <summary>
    /// Extracts one pack to a temp folder and copies it into <c>_themes</c> only if it contains a
    /// valid, current-version <c>igtheme.json</c>. Returns <c>false</c> for a missing, invalid, or
    /// older-version pack (the caller reports it as incompatible).
    /// </summary>
    private static bool InstallOneThemePack(string packageFile, string themesRoot)
    {
        var staging = Path.Combine(Path.GetTempPath(), "ig_theme_" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(staging);
            ZipFile.ExtractToDirectory(packageFile, staging, overwriteFiles: true);

            // locate the pack folder (archive root, or one directory below)
            var srcDir = FindThemePackDir(staging);
            if (srcDir is null) return false;

            // an incompatible/older pack fails to parse into the current model, or declares an older version
            var theme = new IgTheme().Load(srcDir);
            if (!theme.IsValid || theme._Metadata.Version < IgTheme.SPEC_VERSION) return false;

            // dest folder name = the wrapping folder, or the archive name when the json is at the root
            var folderName = string.Equals(srcDir, staging, StringComparison.OrdinalIgnoreCase)
                ? MakeSafeThemeFolderName(GetThemePackFileName(packageFile))
                : Path.GetFileName(srcDir);

            MoveThemeDirectory(srcDir, Path.Combine(themesRoot, folderName));
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            try { if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true); } catch { }
        }
    }


    /// <summary>
    /// Finds the folder that directly contains <see cref="IgTheme.CONFIG_FILE"/>: the archive root,
    /// or one directory below it.
    /// </summary>
    private static string? FindThemePackDir(string root)
    {
        if (File.Exists(Path.Combine(root, IgTheme.CONFIG_FILE))) return root;

        foreach (var sub in Directory.EnumerateDirectories(root))
        {
            if (File.Exists(Path.Combine(sub, IgTheme.CONFIG_FILE))) return sub;
        }
        return null;
    }


    /// <summary>
    /// Moves the staged pack into <paramref name="dest"/> (replacing any existing one), copying
    /// recursively when the source and destination are on different volumes.
    /// </summary>
    private static void MoveThemeDirectory(string src, string dest)
    {
        if (Directory.Exists(dest)) Directory.Delete(dest, recursive: true);

        try
        {
            Directory.Move(src, dest);
            return;
        }
        catch { }

        Directory.CreateDirectory(dest);
        foreach (var dir in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(dest, Path.GetRelativePath(src, dir)));
        }
        foreach (var f in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
        {
            File.Copy(f, Path.Combine(dest, Path.GetRelativePath(src, f)), overwrite: true);
        }
    }


    /// <summary>
    /// Gets the pack name from a package path, stripping the trailing <c>.igtheme.zip</c>.
    /// </summary>
    private static string GetThemePackFileName(string packageFile)
    {
        var name = Path.GetFileName(packageFile);
        const string suffix = ".igtheme.zip";
        if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) name = name[..^suffix.Length];
        return name;
    }


    /// <summary>
    /// Replaces characters invalid in a file name with underscores.
    /// </summary>
    private static string MakeSafeThemeFolderName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (Array.IndexOf(invalid, chars[i]) >= 0) chars[i] = '_';
        }
        return new string(chars);
    }

    #endregion // Private static methods (theme pack)


}



/// <summary>
/// Outcome of a theme-pack install batch (installed count + names of the incompatible packs).
/// </summary>
public readonly record struct ThemePackInstallResult(
    int InstalledCount,
    IReadOnlyList<string> IncompatiblePackNames);
