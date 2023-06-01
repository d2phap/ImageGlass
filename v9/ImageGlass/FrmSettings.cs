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
using ImageGlass.Base.PhotoBox;
using ImageGlass.Settings;
using System.Dynamic;

namespace ImageGlass;

public partial class FrmSettings : WebForm
{
    public FrmSettings()
    {
        InitializeComponent();
    }


    // Protected / override methods
    #region Protected / override methods

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode) return;

        PageName = "settings";
        Text = Config.Language[$"{nameof(FrmSettings)}._Text"];

        // load window placement from settings
        WindowSettings.SetPlacementToWindow(this, WindowSettings.GetFrmSettingsPlacementFromConfig());
    }


    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // save placement setting
        var wp = WindowSettings.GetPlacementFromWindow(this);
        WindowSettings.SetFrmSettingsPlacementConfig(wp);
    }


    protected override void OnWeb2Ready()
    {
        base.OnWeb2Ready();

        // get all settings as json string
        var configJsonObj = Config.PrepareJsonSettingsObject();
        var configJson = BHelper.ToJson(configJsonObj) as string;

        // get language as json string
        var configLangJson = BHelper.ToJson(Config.Language);

        // setting paths
        var startupDir = App.StartUpDir().Replace("\\", "\\\\");
        var configDir = App.ConfigDir(PathType.Dir).Replace("\\", "\\\\");
        var userConfigFilePath = App.ConfigDir(PathType.File, Source.UserFilename).Replace("\\", "\\\\");

        // enums
        var enumObj = new ExpandoObject();
        var enums = new Type[] {
            typeof(ImageOrderBy),
            typeof(ImageOrderType),
            typeof(ColorProfileOption),
            typeof(AfterEditAppAction),
            typeof(ImageInterpolation),
            typeof(MouseWheelAction),
            typeof(MouseWheelEvent),
            typeof(MouseClickEvent),
            typeof(Base.BackdropStyle),
            typeof(ToolbarItemModelType),
        };
        foreach (var item in enums)
        {
            var keys = Enum.GetNames(item);
            enumObj.TryAdd(item.Name, keys);
        }
        var enumsJson = BHelper.ToJson(enumObj);

        // language list
        var langListJson = GetLanguageListJson();


        _ = LoadWeb2ContentAsync(Settings.Properties.Resources.Page_Settings +
            @$"
             <script>
                window._pageSettings = {{
                    startUpDir: '{startupDir}',
                    configDir: '{configDir}',
                    userConfigFilePath: '{userConfigFilePath}',
                    enums: {enumsJson},
                    config: {configJson},
                    lang: {configLangJson},
                    langList: {langListJson},
                }};

                {Settings.Properties.Resources.Script_Settings}
             </script>
            ");
    }


    protected override void OnWeb2MessageReceived(string name, string data)
    {
        // Footer
        #region Footer
        if (name.Equals("BtnOK"))
        {
            //
        }
        else if (name.Equals("BtnApply"))
        {
            //
        }
        else if (name.Equals("BtnCancel"))
        {
            Close();
        }
        #endregion // Footer


        // Tab General
        #region Tab General
        else if (name.Equals("Lnk_StartupDir"))
        {
            BHelper.OpenFilePath(data);
        }
        else if (name.Equals("Lnk_ConfigDir"))
        {
            BHelper.OpenFilePath(data);
        }
        else if (name.Equals("Lnk_UserConfigFile"))
        {
            _ = OpenUserConfigFileAsync(data);
        }
        #endregion // Tab General


        // Tab Image
        #region Tab Image
        else if (name.Equals("Btn_BrowseColorProfile"))
        {
            var profileFilePath = SelectColorProfileFile();
            profileFilePath = profileFilePath.Replace("\\", "\\\\");

            if (!String.IsNullOrEmpty(profileFilePath))
            {
                PostMessage("Btn_BrowseColorProfile", $"\"{profileFilePath}\"");
            }
        }
        else if (name.Equals("Lnk_CustomColorProfile"))
        {
            BHelper.OpenFilePath(data);
        }
        #endregion // Tab Image


        // Tab Language
        #region Tab Language
        else if (name.Equals("Btn_RefreshLanguageList"))
        {
            var langListJson = GetLanguageListJson();
            PostMessage("Btn_RefreshLanguageList", langListJson);
        }
        else if (name.Equals("Lnk_InstallLanguage"))
        {
            _ = InstallLanguagePackAsync();
        }
        #endregion // Tab Language

    }

    #endregion // Protected / override methods


    private static string GetLanguageListJson()
    {
        var langList = Config.LoadLanguageList();
        var langListJson = BHelper.ToJson(langList.Select(i =>
        {
            var obj = new ExpandoObject();

            var langName = Path.GetFileName(i.FileName);
            if (string.IsNullOrEmpty(langName))
            {
                langName = i.Metadata.EnglishName;
            }

            obj.TryAdd(nameof(i.FileName), langName);
            obj.TryAdd(nameof(i.Metadata), i.Metadata);

            return obj;
        }));

        return langListJson;
    }


    private async Task InstallLanguagePackAsync()
    {
        using var o = new OpenFileDialog()
        {
            Filter = "ImageGlass language pack (*.iglang.json)|*.iglang.json",
            CheckFileExists = true,
            RestoreDirectory = true,
            Multiselect = true,
        };

        if (o.ShowDialog() != DialogResult.OK) return;

        var filePathsArgs = string.Join(" ", o.FileNames.Select(f => $"\"{f}\""));
        var result = await BHelper.RunIgcmd(
            $"{IgCommands.INSTALL_LANGUAGE_PACKS} {IgCommands.SHOW_UI} {filePathsArgs}",
            true);

        if (result == IgExitCode.Done)
        {
            var langListJson = GetLanguageListJson();
            PostMessage("Lnk_InstallLanguage", langListJson);
        }
    }


    private static async Task OpenUserConfigFileAsync(string filePath)
    {
        var result = await BHelper.RunExeCmd($"\"{filePath}\"", "", false);

        if (result == IgExitCode.Error)
        {
            result = await BHelper.RunExeCmd("notepad", $"\"{filePath}\"", false);
        }
    }


    private static string SelectColorProfileFile()
    {
        using var o = new OpenFileDialog()
        {
            Filter = "Color profile|*.icc;*.icm;|All files|*.*",
            CheckFileExists = true,
            RestoreDirectory = true,
        };

        if (o.ShowDialog() != DialogResult.OK) return string.Empty;

        return o.FileName;
    }

}
