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
using ImageGlass;
using ImageGlass.Base;
using ImageGlass.Settings;
using System.Diagnostics;

namespace igcmd.Tools;

public partial class FrmQuickSetup : WebForm
{
    public FrmQuickSetup()
    {
        InitializeComponent();
    }



    // Protected / override methods
    #region Protected / override methods

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode) return;

        Config.UpdateFormIcon(this);
        Web2.PageName = "quick-setup";
        Text = Config.Language[$"{nameof(FrmQuickSetup)}._Text"];
        CloseFormHotkey = Keys.Escape;

        // center window in screen
        var workingArea = Screen.FromControl(this).WorkingArea;
        var rect = BHelper.CenterRectToRect(Bounds, workingArea);
        Location = rect.Location;
    }


    protected override async Task OnWeb2ReadyAsync()
    {
        await base.OnWeb2ReadyAsync();

        var htmlFilePath = App.StartUpDir(Dir.WebUI, $"{nameof(FrmQuickSetup)}.html");
        Web2.Source = new Uri(htmlFilePath);
    }


    protected override async Task OnWeb2NavigationCompleted()
    {
        await base.OnWeb2NavigationCompleted();

        // load data
        var base64Logo = BHelper.ToBase64Png(Config.Theme.Settings.AppLogo);
        WebUI.UpdateLangListJson();

        await Web2.ExecuteScriptAsync(@$"
            window._page.loadData({{
                appLogo: 'data:image/png;base64,{base64Logo}',
                langList: {WebUI.LangListJson},
                currentLangName: '{Config.Language.FileName}',
            }});
        ");
    }


    protected override async Task OnWeb2MessageReceivedAsync(Web2MessageReceivedEventArgs e)
    {
        await base.OnWeb2MessageReceivedAsync(e);

        if (e.Name.Equals("APPLY_SETTINGS", StringComparison.InvariantCultureIgnoreCase))
        {
            await ApplySettingsAsync(e.Data);
        }
        else if (e.Name.Equals("SKIP_AND_LAUNCH", StringComparison.InvariantCultureIgnoreCase))
        {
            Close();
        }
        else if (e.Name.Equals("LOAD_LANGUAGE", StringComparison.InvariantCultureIgnoreCase))
        {
            Config.Language = new IgLang(e.Data, App.StartUpDir(Dir.Language));
            Config.TriggerRequestUpdatingLanguage();
            WebUI.UpdateLangJson(true);
        }
        else if (e.Name.Equals("SET_DEFAULT_VIEWER", StringComparison.InvariantCultureIgnoreCase))
        {
            await Config.SetDefaultPhotoViewerAsync(true);
        }
    }

    #endregion // Protected / override methods


    private async Task ApplySettingsAsync(string settingJson)
    {
        // Parse settings JSON
        #region Parse settings JSON
        var dict = BHelper.ParseJson<Dictionary<string, object?>>(settingJson);

        if (dict.TryGetValue("Language", out var lang))
        {
            Config.Language = new IgLang(lang?.ToString(), App.StartUpDir(Dir.Language));
        }

        if (dict.TryGetValue("ColorProfile", out var enableColorProfileObj))
        {
            var enableColorProfile = enableColorProfileObj
                ?.ToString()
                .Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;

            Config.ColorProfile = enableColorProfile
                ? nameof(ColorProfileOption.CurrentMonitorProfile)
                : nameof(ColorProfileOption.None);
        }

        if (dict.TryGetValue("UseEmbeddedThumbnailRawFormats", out var useThumbnailRawFormatsObj))
        {
            Config.UseEmbeddedThumbnailRawFormats = useThumbnailRawFormatsObj
                ?.ToString()
                .Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? true;
        }

        #endregion // Parse settings JSON


        // Apply settings
        await Config.WriteAsync();


        // kill all ImageGlass processes and relaunch
        var igProcesses = Process.GetProcesses()
        .Where(p =>
            p.Id != Environment.ProcessId &&
            p.ProcessName.Contains("ImageGlass")
        ).ToList();

        if (igProcesses.Count > 0)
        {
            var result = Config.ShowInfo(this,
                title: Text,
                heading: Config.Language[$"{nameof(FrmQuickSetup)}._ConfirmCloseProcess"],
                icon: ImageGlass.Base.WinApi.SHSTOCKICONID.SIID_HELP,
                buttons: PopupButton.Yes_No);

            if (result.ExitResult == PopupExitResult.OK)
            {
                // Kill all processes
                igProcesses.ForEach(p => p.Kill());

                LaunchImageGlass();
            }
        }
        else
        {
            LaunchImageGlass();
        }

        Close();
    }


    /// <summary>
    /// Launch ImageGlass app
    /// </summary>
    private static void LaunchImageGlass()
    {
        using var p = new Process();
        p.StartInfo.FileName = App.IGExePath;
        p.Start();
    }

}
