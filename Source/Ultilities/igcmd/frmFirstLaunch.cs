/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using ImageGlass.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ImageGlass.Library;
using ImageGlass.Theme;

namespace igcmd
{
    public partial class frmFirstLaunch : Form
    {
        public frmFirstLaunch()
        {
            InitializeComponent();

        }

        private List<Theme> _themeList = new List<Theme>();
        private List<Language> _langList = new List<Language>();

        private Language _lang = new Language();
        private Theme _theme = new Theme();
        private LayoutMode _layout = LayoutMode.Standard;


        #region Events

        private void frmFirstLaunch_Load(object sender, EventArgs e)
        {
            //Load language list
            LoadLanguageList();
            ApplyLanguage(_lang);

            //Extract & install Theme packs
            InstallThemePacks();

            //Load theme list
            LoadThemeList();


            //Don't run again
            GlobalSetting.SetConfig("FirstLaunchVersion", GlobalSetting.FIRST_LAUNCH_VERSION.ToString());
        }


        private void tab1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblStepNumber.Text = string.Format(this._lang.Items[$"{this.Name}.lblStepNumber"], tab1.SelectedIndex + 1, tab1.TabCount);


            if (tab1.SelectedIndex == tab1.TabCount - 1)
            {
                btnNextStep.Text = this._lang.Items[$"{this.Name}.btnNextStep._Done"];
            }
            else
            {
                btnNextStep.Text = this._lang.Items[$"{this.Name}.btnNextStep"];
            }
        }


        private void btnNextStep_Click(object sender, EventArgs e)
        {
            //Done all configs, apply settings and launch ImageGlass
            if (tab1.SelectedIndex == tab1.TabCount - 1)
            {
                ApplySettings();

                LaunchImageGlass();
                this.Close();
            }

            tab1.SelectedIndex++;
            lblStepNumber.Text = string.Format(this._lang.Items[$"{this.Name}.lblStepNumber"], tab1.SelectedIndex + 1, tab1.TabCount);

            //Done
            if (tab1.SelectedIndex == tab1.TabCount - 1)
            {
                btnNextStep.Text = this._lang.Items[$"{this.Name}.btnNextStep._Done"];
            }
            //Next Step
            else
            {
                btnNextStep.Text = this._lang.Items[$"{this.Name}.btnNextStep"];
            }
        }


        private void lnkSkip_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchImageGlass();
            this.Close();
        }


        private void btnSetDefaultApp_Click(object sender, EventArgs e)
        {
            GlobalSetting.LoadBuiltInImageFormats();


            // Update extensions to registry
            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(GlobalSetting.StartUpDir, "igtasks.exe");
            p.StartInfo.Arguments = $"regassociations {GlobalSetting.AllImageFormats}";

            try
            {
                p.Start();
            }
            catch { }
        }


        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this._lang = _langList[cmbLanguage.SelectedIndex];
            }
            catch
            {
                this._lang = new Language();
            }

            ApplyLanguage(this._lang);
        }


        private void cmbTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTheme = new Theme();

            try
            {
                selectedTheme = this._themeList[cmbTheme.SelectedIndex];
            }
            catch { }

            ApplyTheme(selectedTheme);
            _theme = selectedTheme;
        }


        private void cmbLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            _layout = (LayoutMode)cmbLayout.SelectedIndex;
        }

        #endregion



        #region Private Functions
        /// <summary>
        /// Load language list
        /// </summary>
        private void LoadLanguageList()
        {
            cmbLanguage.Items.Clear();
            cmbLanguage.Items.Add("English");


            _langList = new List<Language>
            {
                new Language()
            };

            string langPath = Path.Combine(GlobalSetting.StartUpDir, "Languages");

            if (!Directory.Exists(langPath))
            {
                Directory.CreateDirectory(langPath);
            }
            else
            {
                foreach (string f in Directory.GetFiles(langPath))
                {
                    if (Path.GetExtension(f).ToLower() == ".iglang")
                    {
                        Language l = new Language(f);
                        _langList.Add(l);

                        int iLang = cmbLanguage.Items.Add(l.LangName);
                        string curLang = GlobalSetting.GetConfig("Language", "English");

                        //using current language pack
                        if (f.CompareTo(curLang) == 0)
                        {
                            cmbLanguage.SelectedIndex = iLang;
                        }
                    }
                }
            }

            if (cmbLanguage.SelectedIndex == -1)
            {
                cmbLanguage.SelectedIndex = 0;
            }
        }


        /// <summary>
        /// Apply language
        /// </summary>
        /// <param name="lang"></param>
        private void ApplyLanguage(Language lang)
        {
            this._lang = lang;

            this.Text = _lang.Items[$"{this.Name}._Text"];
            lblStepNumber.Text = string.Format(_lang.Items[$"{this.Name}.lblStepNumber"], 1, tab1.TabCount);
            btnNextStep.Text = _lang.Items[$"{this.Name}.btnNextStep"];
            lnkSkip.Text = _lang.Items[$"{this.Name}.lnkSkip"];

            lblLanguage.Text = _lang.Items[$"{this.Name}.lblLanguage"];
            lblLayout.Text = _lang.Items[$"{this.Name}.lblLayout"];
            lblTheme.Text = _lang.Items[$"{this.Name}.lblTheme"];
            lblDefaultApp.Text = _lang.Items[$"{this.Name}.lblDefaultApp"];
            btnSetDefaultApp.Text = _lang.Items[$"{this.Name}.btnSetDefaultApp"];

            LoadLayoutList();
        }


        /// <summary>
        /// Launch ImageGlass app
        /// </summary>
        private void LaunchImageGlass()
        {
            var appExe = Path.Combine(GlobalSetting.StartUpDir, "ImageGlass.exe");

            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(appExe);
            p.Start();
        }


        /// <summary>
        /// Load theme list
        /// </summary>
        private void LoadThemeList()
        {
            //add default theme
            var defaultTheme = new Theme(Path.Combine(GlobalSetting.StartUpDir, @"DefaultTheme\config.xml"));
            _themeList.Add(defaultTheme);
            cmbTheme.Items.Clear();
            cmbTheme.Items.Add(defaultTheme.Name);
            cmbTheme.SelectedIndex = 0;


            string themeFolder = Path.Combine(GlobalSetting.ConfigDir, "Themes");

            //get the current theme
            var currentTheme = GlobalSetting.GetConfig("Theme", "default");


            if (Directory.Exists(themeFolder))
            {
                foreach (string d in Directory.GetDirectories(themeFolder))
                {
                    string configFile = Path.Combine(d, "config.xml");

                    if (File.Exists(configFile))
                    {
                        Theme th = new Theme(d);

                        //invalid theme
                        if (!th.IsThemeValid)
                        {
                            continue;
                        }

                        _themeList.Add(th);
                        cmbTheme.Items.Add(th.Name);

                        if (currentTheme.ToLower().CompareTo(th.ThemeFolderName.ToLower()) == 0)
                        {
                            cmbTheme.SelectedIndex = cmbTheme.Items.Count - 1;
                        }
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(themeFolder);
            }


        }


        /// <summary>
        /// Apply theme
        /// </summary>
        /// <param name="th"></param>
        private void ApplyTheme(Theme th)
        {
            panFooter.BackColor = th.ToolbarBackgroundColor;
            panHeader.BackColor =
                tabLanguage.BackColor =
                tabLayoutMode.BackColor =
                tabTheme.BackColor =
                tabFileAssociation.BackColor =
                th.BackgroundColor;

            lblStepNumber.ForeColor =
                lblLanguage.ForeColor =
                lblLayout.ForeColor =
                lblTheme.ForeColor =
                lblDefaultApp.ForeColor =
                Theme.InvertBlackAndWhiteColor(th.BackgroundColor);

        }


        /// <summary>
        /// Extract and install theme packs
        /// </summary>
        private void InstallThemePacks()
        {
            var themeFiles = Directory.GetFiles(Path.Combine(GlobalSetting.StartUpDir, "DefaultTheme"), "*.igtheme");

            foreach (var file in themeFiles)
            {
                Theme.InstallTheme(file);
            }
        }


        /// <summary>
        /// Load layout list
        /// </summary>
        private void LoadLayoutList()
        {
            cmbLayout.Items.Clear();
            var list = Enum.GetNames(typeof(LayoutMode));

            foreach (var item in list)
            {
                cmbLayout.Items.Add(_lang.Items[$"{this.Name}.cmbLayout._{item}"]);
            }

            cmbLayout.SelectedIndex = 0;
        }


        /// <summary>
        /// Save and apply settings
        /// </summary>
        private void ApplySettings()
        {
            GlobalSetting.SetConfig("Language", Path.GetFileName(_lang.FileName));
            GlobalSetting.SetConfig("Theme", _theme.ThemeFolderName);


            if (_layout == LayoutMode.Designer)
            {
                GlobalSetting.SetConfig("ToolbarButtons", GlobalSetting.ToolbarButtons);

                GlobalSetting.SetConfig("MouseWheelAction", ((int)MouseWheelActions.ScrollVertically).ToString());
                GlobalSetting.SetConfig("MouseWheelCtrlAction", ((int)MouseWheelActions.Zoom).ToString());
                GlobalSetting.SetConfig("MouseWheelShiftAction", ((int)MouseWheelActions.ScrollHorizontally).ToString());
                GlobalSetting.SetConfig("MouseWheelAltAction", ((int)MouseWheelActions.DoNothing).ToString());

                GlobalSetting.SetConfig("ZoomLockValue", "100"); //lock zoom at 100%
                GlobalSetting.SetConfig("IsShowColorPickerOnStartup", "True");
            }
            else
            {
                GlobalSetting.SetConfig("ToolbarButtons", GlobalSetting.ToolbarButtons);

                GlobalSetting.SetConfig("MouseWheelAction", ((int)MouseWheelActions.Zoom).ToString());
                GlobalSetting.SetConfig("MouseWheelCtrlAction", ((int)MouseWheelActions.ScrollVertically).ToString());
                GlobalSetting.SetConfig("MouseWheelShiftAction", ((int)MouseWheelActions.ScrollHorizontally).ToString());
                GlobalSetting.SetConfig("MouseWheelAltAction", ((int)MouseWheelActions.DoNothing).ToString());

                GlobalSetting.SetConfig("ZoomLockValue", "-1"); //do not lock zoom
            }

        }



        #endregion


    }
}
