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
using Avalonia.Controls;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.ServiceProviders.Update;
using ImageGlass.UI.Windowing;
using ImageGlass.Windows;
using System;
using System.IO;

namespace ImageGlass.Common.Windows;

public partial class GeneralSettingsView : SettingsPageView
{
    public GeneralSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public GeneralSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        var startupDir = BHelper.BasePath;

        // real physical path (MSIX-aware) so the links open where the files actually live
        var configDir = BHelper.GetRealPlatformConfigDir();
        var userConfig = BHelper.GetRealPlatformConfigDir(Config.CONFIG_USER);

        // Locations
        BindLink(PART_StartupDir, LangId.Settings_StartupDir, startupDir,
            () => BHelper.OpenFolderPath(startupDir));
        BindLink(PART_ConfigDir, LangId.Settings_ConfigDir, configDir,
            () => BHelper.OpenFolderPath(configDir));
        BindLink(PART_UserConfig, LangId.Settings_UserConfigFile, userConfig,
            () => OpenUserConfigFile(userConfig));

        // Actions
        BindLink(PART_GetHelp, LangId._GetHelp, OpenHelpDocs);
        BindLink(PART_ResetSettings, LangId.Settings_ResetSettings, ShowQuickSetup);

        // Startup
        BindToggle(PART_WelcomeImage, ConfigId.EnableWelcomeImage,
            LangId.Settings_EnableWelcomeImage, LangId.Settings_Startup);
        BindToggle(PART_LastSeenImage, ConfigId.EnableLastSeenImage,
            LangId.Settings_EnableLastSeenImage, LangId.Settings_Startup);
        BindToggle(PART_MultiInstances, ConfigId.EnableMultiInstances,
            LangId.Settings_EnableMultiInstances, LangId.Settings_Startup);

        // App update — AutoUpdate is stored as a date string; "0" means disabled.
        BindAutoUpdateToggle(PART_AutoUpdate, ConfigId.AutoUpdate,
            LangId.Settings_AutoUpdate, LangId.Settings_AppUpdate);
        BindLink(PART_SeeWhatIsSent, LangId.Settings_SeeWhatIsSent, ShowUsageStatsPreview);

        BindToggle(PART_AutoInstallUpdate, ConfigId.EnableAutoInstallUpdate,
            LangId.Settings_EnableAutoInstallUpdate, LangId.Settings_AppUpdate);
        ProGate(ConfigId.EnableAutoInstallUpdate, PART_AutoInstallUpdateBadge, PART_AutoInstallUpdate);

        // nothing to offer on a channel that cannot install its own updates, or when an admin locked it
        var canSelfUpdate = UpdateProvider.CanInstallUpdate;
        PART_AutoInstallUpdateRow.IsVisible = canSelfUpdate;

        // Others
        BindIntInput(PART_MsgDuration, ConfigId.InAppMessageDuration,
            LangId.Settings_InAppMessageDuration, LangId.Settings_Others);
    }


    /// <summary>
    /// Binds a checkbox to the string-based <c>AutoUpdate</c> config (date string vs. "0").
    /// </summary>
    private void BindAutoUpdateToggle(CheckBox chk, ConfigId id, LangId label, LangId? section)
    {
        var current = VM.GetValue(id, "0");
        chk.IsChecked = !string.Equals(current, "0", StringComparison.OrdinalIgnoreCase);
        chk.IsCheckedChanged += (_, _) =>
            VM.SetValue(id, (chk.IsChecked ?? false) ? DateTime.UtcNow.ToString() : "0");

        RegisterSearchKey(chk, label, id, section);
    }


    /// <summary>
    /// Shows the exact User-Agent this install would send on a scheduled update check.
    /// </summary>
    private async void ShowUsageStatsPreview()
    {
        try
        {
            var owner = TopLevel.GetTopLevel(this) as PhWindow;

            _ = await ModalWindow.ShowInfoAsync(owner, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Settings_SeeWhatIsSent],
                Details = UsageStatsAgent.Build(withStats: true),
            });
        }
        catch
        {
            // an async void handler must never reach the unhandled-error dialog
        }
    }


    /// <summary>
    /// Opens the online documentation.
    /// </summary>
    private async void OpenHelpDocs()
    {
        try
        {
            await BHelper.OpenUrlAsync(this, "https://imageglass.org/docs", $"from_setting_{NavId}");
        }
        catch
        {
            // an async void handler must never reach the unhandled-error dialog
        }
    }


    /// <summary>
    /// Opens the Quick Setup wizard, which owns the reset-to-defaults flow.
    /// </summary>
    private async void ShowQuickSetup()
    {
        try
        {
            var quickSetup = new QuickSetupWindow();
            _ = await quickSetup.ShowAsync(TopLevel.GetTopLevel(this) as PhWindow);
        }
        catch
        {
            // an async void handler must never reach the unhandled-error dialog
        }
    }


    /// <summary>
    /// Opens the user config file in the system's default editor.
    /// Falls back to revealing it in the file explorer when no editor is associated.
    /// </summary>
    private static async void OpenUserConfigFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            // open the file's own (already-resolved) folder, not the unresolved config dir
            BHelper.OpenFolderPath(Path.GetDirectoryName(filePath));
            return;
        }

        try
        {
            // open the JSON file in the app associated with its file type (shell-execute)
            var exitCode = await BHelper.RunExeAsync(filePath);
            if (exitCode == 0) return;
        }
        catch { }

        // no associated editor -> reveal in explorer instead
        BHelper.OpenFilePath(filePath);
    }

}
