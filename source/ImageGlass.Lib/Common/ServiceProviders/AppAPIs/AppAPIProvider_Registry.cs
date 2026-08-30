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
using ImageGlass.Common.Commands;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders;

public partial class AppAPIProvider
{

    private FrozenDictionary<API, IPhCommand> _apis => new Dictionary<API, IPhCommand>()
    {
        { API.IG_OpenMainMenu,          PhCommands.Create(IG_OpenMainMenu) },

        { API.IG_OpenFolder,            PhCommands.Create(IG_OpenFolderAsync) },
        { API.IG_OpenPath,              PhCommands.Create(IG_OpenPath) },
        { API.IG_ApplySettings,         PhCommands.Create(IG_ApplySettingsAsync) },


        // Main Menu
        { API.IG_OpenFile,              PhCommands.Create(IG_OpenFileAsync) },
        { API.IG_NewWindow,             PhCommands.Create(IG_NewWindow) },
        { API.IG_Save,                  PhCommands.Create(IG_SaveAsync) },
        { API.IG_SaveAs,                PhCommands.Create(IG_SaveAsAsync) },
        { API.IG_ExportImageFrames,     PhCommands.Create(IG_ExportImageFrames) },
        { API.IG_OpenWith,              PhCommands.Create(IG_OpenWithAsync) },
        { API.IG_Print,                 PhCommands.Create(IG_PrintAsync) },
        { API.IG_Share,                 PhCommands.Create(IG_ShareAsync) },
        { API.IG_OpenLocation,          PhCommands.Create(IG_OpenLocation) },
        { API.IG_Rename,                PhCommands.Create(IG_RenameAsync) },
        { API.IG_Delete,                PhCommands.Create(IG_DeleteAsync) },
        { API.IG_OpenProperties,        PhCommands.Create(IG_OpenProperties) },


        // Navigation
        { API.IG_ViewNext,              PhCommands.Create(IG_ViewNext) },
        { API.IG_ViewPrevious,          PhCommands.Create(IG_ViewPrevious) },
        { API.IG_Goto,                  PhCommands.Create(IG_GoToAsync) },
        { API.IG_GotoFirst,             PhCommands.Create(IG_GoToFirst) },
        { API.IG_GotoLast,              PhCommands.Create(IG_GoToLast) },
        { API.IG_ViewFrame,             PhCommands.Create(IG_ViewFrame) },
        { API.IG_ViewNextFrame,         PhCommands.Create(IG_ViewNextFrame) },
        { API.IG_ViewPreviousFrame,     PhCommands.Create(IG_ViewPreviousFrame) },
        { API.IG_ViewFirstFrame,        PhCommands.Create(IG_ViewFirstFrame) },
        { API.IG_ViewLastFrame,         PhCommands.Create(IG_ViewLastFrame) },

        { API.IG_ViewByStep,            PhCommands.Create(IG_ViewByStep) },
        { API.IG_ViewByIndex,           PhCommands.Create(IG_ViewByIndex) },


        // Zoom
        { API.IG_CustomZoom,            PhCommands.Create(IG_CustomZoomAsync) },
        { API.IG_SetZoom,               PhCommands.Create(IG_SetZoom) },
        { API.IG_SetZoomForMouseClick,  PhCommands.Create(IG_SetZoomForMouseClick) },
        { API.IG_ZoomIn,                PhCommands.Create(IG_ZoomIn) },
        { API.IG_ZoomOut,               PhCommands.Create(IG_ZoomOut) },
        { API.IG_SetZoomMode,           PhCommands.Create(IG_SetZoomMode) },


        // Panning
        { API.IG_PanLeft,               PhCommands.Create(IG_PanLeft) },
        { API.IG_PanRight,              PhCommands.Create(IG_PanRight) },
        { API.IG_PanUp,                 PhCommands.Create(IG_PanUp) },
        { API.IG_PanDown,               PhCommands.Create(IG_PanDown) },
        { API.IG_PanToLeft,             PhCommands.Create(IG_PanToLeft) },
        { API.IG_PanToRight,            PhCommands.Create(IG_PanToRight) },
        { API.IG_PanToTop,              PhCommands.Create(IG_PanToTop) },
        { API.IG_PanToBottom,           PhCommands.Create(IG_PanToBottom) },


        // Image
        { API.IG_Refresh,                       PhCommands.Create(IG_Refresh) },
        { API.IG_Reload,                        PhCommands.Create(IG_Reload) },
        { API.IG_ReloadList,                    PhCommands.Create(IG_ReloadList) },
        { API.IG_Unload,                        PhCommands.Create(IG_UnloadAsync) },
        { API.IG_ToggleExplorerSortOrder,       PhCommands.Create(IG_ToggleExplorerSortOrder) },
        { API.IG_SetLoadingOrderBy,             PhCommands.Create(IG_SetLoadingOrderBy) },
        { API.IG_SetLoadingOrderType,           PhCommands.Create(IG_SetLoadingOrderType) },
        { API.IG_SetColorChannels,              PhCommands.Create(IG_SetColorChannels) },
        { API.IG_OpenEditingApp,                PhCommands.Create(IG_OpenEditingAppAsync) },
        { API.IG_InvertColors,                  PhCommands.Create(IG_InvertColors) },
        { API.IG_ToggleImageAnimation,          PhCommands.Create(IG_ToggleImageAnimationAsync) },
        { API.IG_Rotate,                        PhCommands.Create(IG_Rotate) },
        { API.IG_FlipImage,                     PhCommands.Create(IG_FlipImage) },
        { API.IG_SetDesktopBackground,          PhCommands.Create(IG_SetDesktopBackgroundAsync) },
        { API.IG_SetLockScreenImage,            PhCommands.Create(IG_SetLockScreenImageAsync) },


        // Clipboard
        { API.IG_PasteImage,            PhCommands.Create(IG_PasteImageAsync) },
        { API.IG_CopyImagePixels,       PhCommands.Create(IG_CopyImagePixelsAsync) },
        { API.IG_CopyFiles,             PhCommands.Create(IG_CopyFilesAsync) },
        { API.IG_CutFiles,              PhCommands.Create(IG_CutFilesAsync) },
        { API.IG_CopyImagePath,         PhCommands.Create(IG_CopyImagePathAsync) },
        { API.IG_ClearClipboard,        PhCommands.Create(IG_ClearClipboardAsync) },


        // Window modes
        { API.IG_ToggleWindowFit,           PhCommands.Create(IG_ToggleWindowFit) },
        { API.IG_ToggleFrameless,           PhCommands.Create(IG_ToggleFrameless) },
        { API.IG_ToggleFullScreen,          PhCommands.Create(IG_ToggleFullScreen) },
        { API.IG_ToggleSlideshow,           PhCommands.Create(IG_ToggleSlideshow) },
        { API.IG_ToggleSlideshowPlayback,   PhCommands.Create(IG_ToggleSlideshowPlayback) },
        { API.IG_ToggleSlideshowCountdown,  PhCommands.Create(IG_ToggleSlideshowCountdown) },


        // Layout
        { API.IG_ToggleToolbar,         PhCommands.Create(IG_ToggleToolbar) },
        { API.IG_ToggleGallery,         PhCommands.Create(IG_ToggleGalleryAsync) },
        { API.IG_ToggleCheckerboard,    PhCommands.Create(IG_ToggleCheckerboard) },
        { API.IG_ToggleWindowTopMost,   PhCommands.Create(IG_ToggleWindowTopMost) },
        { API.IG_SetBackgroundColor,    PhCommands.Create(IG_SetBackgroundColorAsync) },


        // Plugins
        { API.IG_CloseCurrentTool,        PhCommands.Create(IG_CloseCurrentTool) },
        { API.IG_ToggleTool,              PhCommands.Create(IG_ToggleTool) },
        { API.IG_OpenTool,                PhCommands.Create(IG_OpenTool) },
        { API.IG_CloseTool,               PhCommands.Create(IG_CloseTool) },


        // Settings
        { API.IG_OpenSettings,              PhCommands.Create(IG_OpenSettingsAsync) },


        // Help
        { API.IG_OpenAboutWindow,           PhCommands.Create(IG_OpenAboutWindowAsync) },
        { API.IG_ManageLicense,             PhCommands.Create(IG_ManageLicenseAsync) },
        { API.IG_CheckForUpdate,            PhCommands.Create(IG_CheckForUpdateAsync) },
        { API.IG_RestartToUpdate,           PhCommands.Create(IG_RestartToUpdateAsync) },
        { API.IG_ReportIssue,               PhCommands.Create(IG_ReportIssue) },
        { API.IG_QuickSetup,                PhCommands.Create(IG_QuickSetupAsync) },
        { API.IG_SetDefaultPhotoViewer,     PhCommands.Create(IG_SetDefaultPhotoViewerAsync) },
        { API.IG_RemoveDefaultPhotoViewer,  PhCommands.Create(IG_RemoveDefaultPhotoViewerAsync) },


        // Exit
        { API.IG_Exit,                      PhCommands.Create(IG_Exit) },


        // Others
        { API.IG_SetFileWatcher,            PhCommands.Create(IG_SetFileWatcher) },
        { API.IG_OpenContextMenu,           PhCommands.Create(IG_OpenContextMenu) },

    }.ToFrozenDictionary();



    #region API Command Methods

    /// <summary>
    /// Gets the API command.
    /// </summary>
    public IPhCommand? GetApiCommand(string? apiName)
    {
        if (Enum.TryParse<API>(apiName, out var api))
        {
            return GetApiCommand(api);
        }

        return null;
    }


    /// <summary>
    /// Gets the API command, wrapped so a direct <c>ICommand</c> binding (e.g. a context-menu item)
    /// is disabled and refuses to run while the API is LockedFeatures-locked. Command bindings skip
    /// <see cref="RunApiAsync(API, string?)"/>, so this restores the lock gate for them.
    /// </summary>
    public IPhCommand? GetApiCommand(API api)
    {
        var cmd = _apis.GetValueOrDefault(api);
        return cmd is null ? null : new LockAwareApiCommand(api, cmd);
    }


    /// <summary>
    /// Wraps an API command to enforce the feature lock on direct <c>ICommand</c> bindings that
    /// bypass <see cref="RunApiAsync(API, string?)"/> (menu items bound via <see cref="GetApiCommand(API)"/>).
    /// <see cref="CanExecute"/> reports <c>false</c> when locked (so the bound item renders disabled),
    /// and <see cref="Execute"/>/<see cref="ExecuteAsync"/> no-op when locked as a hard backstop.
    /// </summary>
    private sealed class LockAwareApiCommand(API api, IPhCommand inner) : IPhCommand
    {
        public bool IsAsync => inner.IsAsync;

        public event EventHandler? CanExecuteChanged
        {
            add => inner.CanExecuteChanged += value;
            remove => inner.CanExecuteChanged -= value;
        }

        public bool CanExecute(object? parameter)
            => !FeatureManager.IsLocked(api) && inner.CanExecute(parameter);

        public void Execute(object? parameter)
        {
            if (!FeatureManager.IsLocked(api)) inner.Execute(parameter);
        }

        public Task ExecuteAsync(object? parameter)
            => FeatureManager.IsLocked(api) ? Task.CompletedTask : inner.ExecuteAsync(parameter);
    }


    /// <summary>
    /// APIs an external tool must NOT invoke over the IPC pipe: destructive,
    /// process-spawning, or system-modifying. The trusted hotkey/menu path is
    /// unaffected; this gate applies only to <c>ToolPipeServer</c>'s RUN_API handler.
    /// </summary>
    private static readonly FrozenSet<API> _toolBlockedApis = new[]
    {
        API.IG_Delete,
        API.IG_Rename,
        API.IG_Save,
        API.IG_SaveAs,
        API.IG_NewWindow,
        API.IG_OpenWith,
        API.IG_OpenEditingApp,
        API.IG_SetDesktopBackground,
        API.IG_SetLockScreenImage,
        API.IG_SetDefaultPhotoViewer,
        API.IG_RemoveDefaultPhotoViewer,
        API.IG_ApplySettings,
        API.IG_Exit,
    }.ToFrozenSet();


    /// <summary>
    /// Returns <c>false</c> for APIs an external tool is not permitted to run over the
    /// IPC pipe. Unknown names are allowed through here and fall to the normal
    /// <c>ApiNotFound</c> handling in <see cref="RunApiAsync(string?, string?)"/>.
    /// </summary>
    public bool IsApiAllowedForTool(string? apiName)
    {
        if (Enum.TryParse<API>(apiName, out var api))
        {
            return !_toolBlockedApis.Contains(api);
        }

        return true;
    }


    /// <summary>
    /// Executes the given built-in API command.
    /// </summary>
    public async Task<ActionResult> RunApiAsync(API api, string? args = null)
    {
        // Check if the API is locked
        if (FeatureManager.IsLocked(api))
            return new ActionResult(ActionExitCode.Locked);

        var cmd = GetApiCommand(api);

        return await RunApiCommandAsync_(cmd, args);
    }


    /// <summary>
    /// Executes the given built-in API command.
    /// </summary>
    public async Task<ActionResult> RunApiAsync(string? apiName, string? args = null)
    {
        // Check if the API is locked
        if (FeatureManager.IsLocked(apiName))
            return new ActionResult(ActionExitCode.Locked);

        var cmd = GetApiCommand(apiName);

        return await RunApiCommandAsync_(cmd, args);
    }


    /// <summary>
    /// Executes the API command and returns results
    /// </summary>
    private static async Task<ActionResult> RunApiCommandAsync_(IPhCommand? cmd, string? args = null)
    {
        // get the command from API name
        if (cmd is null)
            return new ActionResult(ActionExitCode.ApiNotFound);

        // check if the command can be executed
        if (!cmd.CanExecute(args))
            return new ActionResult(ActionExitCode.Cancelled);

        try
        {
            // execute the command
            if (cmd.IsAsync)
            {
                await cmd.ExecuteAsync(args);
            }
            else
            {
                cmd.Execute(args);
            }
        }
        catch (Exception ex)
        {
            return new ActionResult(ActionExitCode.Error, ex);
        }

        return new ActionResult(ActionExitCode.Success);
    }


    /// <summary>
    /// Executes a single action, shows error popup.
    /// </summary>
    public async Task RunActionAsync(SingleAction? ac, string? customArg = null)
    {
        _ = await RunActionAsync(ac, true, customArg);
    }


    /// <summary>
    /// Executes a single action.
    /// </summary>
    public async Task<Exception?> RunActionAsync(SingleAction? ac, bool showError, string? customArg = null)
    {
        if (string.IsNullOrWhiteSpace(ac?.Executable)) return null;


        // route a blocked Pro feature to the upgrade prompt instead of running it (hotkey, toolbar
        // and menu; startup-restore/IPC go through RunApiAsync)
        if (FeatureManager.IsProBlocked(Lang.GetKey(ac.LangKey)))
        {
            _ = await RunApiAsync(API.IG_ManageLicense);
            return null;
        }


        // 1. run the current action
        var acArgs = customArg ?? ac.Argument;
        var acResults = await RunApiAsync(ac.Executable, acArgs);


        // 2. exit if the action was cancelled.
        if (acResults.ExitCode == ActionExitCode.Cancelled) return null;


        // 3. exit silently if the action is locked (feature is invisible to user)
        if (acResults.ExitCode == ActionExitCode.Locked) return null;


        // 4. run next action on success
        if (acResults.ExitCode == ActionExitCode.Success)
        {
            return await RunActionAsync(ac.NextAction, showError);
        }


        // 5. if there was an error from API
        Exception? error = null;
        if (acResults.ExitCode == ActionExitCode.Error && acResults.Error != null)
        {
            error = acResults.Error;
        }


        // 6. if action name is not an built-in API
        // try to run with Shell (but NOT if locked - locked means invisible)
        else if (acResults.ExitCode == ActionExitCode.ApiNotFound)
        {
            var args = string.Join(string.Empty, acArgs) ?? string.Empty;
            var exeInfo = BHelper.BuildExeArgList(ac.Executable, args, Core.Photos.CurrentFilePath);

            var exeCode = await BHelper.RunExeCmd(exeInfo.Executable, exeInfo.Args, false);
            if (exeCode != IgExitCode.Done)
            {
                var errorMsg = Core.Lang[LangId._UserAction_Win32ExeError, ac.Executable];
                error = new Win32Exception(errorMsg);
            }
        }


        // 6. show error message
        if (error is not null && showError)
        {
            // get the language string for error title
            var errorTitle = Core.Lang[ac.LangKey];

            _ = await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = errorTitle,
                Description = error.Message,
            });
        }

        return error;
    }

    #endregion // API Command Methods


}
