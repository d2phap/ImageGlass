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
using ImageGlass;
using ImageGlass.Base;
using ImageGlass.Settings;

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

        _ = Config.UpdateFormIcon(this);
        Web2.PageName = "quick-setup";
        Text = Config.Language[$"{nameof(FrmQuickSetup)}._Text"];
        CloseFormHotkey = Keys.Escape;

        // center window in screen
        var workingArea = Screen.FromControl(this).WorkingArea;
        var rect = BHelper.CenterRectToRect(Bounds, workingArea);
        Location = rect.Location;
    }


    protected override void OnRequestUpdatingLanguage()
    {
        base.OnRequestUpdatingLanguage();

        Text = Config.Language[$"{Name}._Text"];
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
        var langListJson = BHelper.ToJson(WebUI.LangList);

        await Web2.ExecuteScriptAsync(@$"
            window._page.loadData({{
                appLogo: 'data:image/png;base64,{base64Logo}',
                langList: {langListJson},
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
            Config.QuickSetupVersion = Const.QUICK_SETUP_VERSION;
            await Config.WriteAsync();

            CmdHelper.LaunchImageGlass();
            Close();
        }
        else if (e.Name.Equals("LOAD_LANGUAGE", StringComparison.InvariantCultureIgnoreCase))
        {
            Config.Language = new IgLang(e.Data, App.StartUpDir(Dir.Language));
            Config.TriggerRequestUpdatingLanguage();
        }
        else if (e.Name.Equals("SET_DEFAULT_VIEWER", StringComparison.InvariantCultureIgnoreCase))
        {
            await Config.SetDefaultPhotoViewerAsync(true);
        }
    }

    #endregion // Protected / override methods


    // Private methods
    #region Private methods
    private async Task ApplySettingsAsync(string settingJson)
    {
        // try to kill all ImageGlass processes
        if (!CmdHelper.KillImageGlassProcessesAsync(this, true)) return;


        // don't auto-show Quick Setup again
        Config.QuickSetupVersion = Const.QUICK_SETUP_VERSION;


        // Parse settings JSON
        #region Parse settings JSON
        var dict = BHelper.ParseJson<Dictionary<string, object?>>(settingJson);

        if (dict.TryGetValue(nameof(Config.ColorProfile), out var enableColorProfileObj))
        {
            var enableColorProfile = enableColorProfileObj
                ?.ToString()
                .Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;

            Config.ColorProfile = enableColorProfile
                ? nameof(ColorProfileOption.CurrentMonitorProfile)
                : nameof(ColorProfileOption.None);
        }

        if (dict.TryGetValue(nameof(Config.UseEmbeddedThumbnailRawFormats), out var useThumbnailRawFormatsObj))
        {
            Config.UseEmbeddedThumbnailRawFormats = useThumbnailRawFormatsObj
                ?.ToString()
                .Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? true;
        }

        #endregion // Parse settings JSON


        // apply and restart ImageGlass
        await ApplyAndCloseAsync();
    }


    private async Task ApplyAndCloseAsync()
    {
        // write settings
        await Config.WriteAsync();

        CmdHelper.LaunchImageGlass();
        Close();
    }

    #endregion // Private methods

}
