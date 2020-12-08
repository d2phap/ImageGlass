/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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

using ImageGlass.Base;
using ImageGlass.Library;
using ImageGlass.Library.WinAPI;
using ImageGlass.Settings;
using ImageGlass.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace igcmd {
    public partial class frmFirstLaunch: Form {
        public frmFirstLaunch() {
            InitializeComponent();

            // Extract & install Theme packs
            InstallThemePacks();
            LoadThemeList();

            // first apply current theme
            ApplyTheme(Configs.Theme);
        }

        private readonly List<Theme> _themeList = new List<Theme>();
        private List<Language> _langList = new List<Language>();

        private Language _lang = new Language();
        private Theme _theme = new Theme((int)Configs.ToolbarIconHeight);
        private LayoutMode _layout = LayoutMode.Standard;

        #region Form events

        private void frmFirstLaunch_Load(object sender, EventArgs e) {
            // Load language list
            LoadLanguageList();
            ApplyLanguage(_lang);

            // Don't run again
            Configs.FirstLaunchVersion = Constants.FIRST_LAUNCH_VERSION;
        }

        private void tab1_SelectedIndexChanged(object sender, EventArgs e) {
            lblStepNumber.Text = string.Format(this._lang.Items[$"{Name}.lblStepNumber"], tab1.SelectedIndex + 1, tab1.TabCount);

            if (tab1.SelectedIndex == tab1.TabCount - 1) {
                btnNextStep.Text = _lang.Items[$"{Name}.btnNextStep._Done"];
            }
            else {
                btnNextStep.Text = _lang.Items[$"{Name}.btnNextStep"];
            }
        }

        private void btnNextStep_Click(object sender, EventArgs e) {
            // Done all configs, apply settings and launch ImageGlass
            if (tab1.SelectedIndex == tab1.TabCount - 1) {
                ApplySettings();

                // Get all processes of ImageGlass
                var igProcesses = Process.GetProcesses()
                    .Where(p =>
                        p.Id != Process.GetCurrentProcess().Id &&
                        p.ProcessName.Contains("ImageGlass")
                    )
                    .ToList();

                if (igProcesses.Count > 0) {
                    var result = MessageBox.Show(this._lang.Items[$"{Name}._ConfirmCloseProcess"], "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes) {
                        // Kill all processes
                        igProcesses.ForEach(p => p.Kill());

                        LaunchImageGlass();
                    }
                }
                else {
                    LaunchImageGlass();
                }

                this.Close();

                return;
            }

            tab1.SelectedIndex++;
            lblStepNumber.Text = string.Format(this._lang.Items[$"{this.Name}.lblStepNumber"], tab1.SelectedIndex + 1, tab1.TabCount);

            // Done
            if (tab1.SelectedIndex == tab1.TabCount - 1) {
                btnNextStep.Text = this._lang.Items[$"{this.Name}.btnNextStep._Done"];
            }
            // Next Step
            else {
                btnNextStep.Text = this._lang.Items[$"{this.Name}.btnNextStep"];
            }
        }

        private void lnkSkip_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            // Save configs to file
            Configs.Write();

            LaunchImageGlass();
            this.Close();
        }

        private void btnSetDefaultApp_Click(object sender, EventArgs e) {
            // Update extensions to registry
            using (var p = new Process()) {
                var formats = Configs.GetImageFormats(Configs.AllFormats);

                p.StartInfo.FileName = App.StartUpDir("igtasks.exe");
                p.StartInfo.Arguments = $"regassociations {formats}";

                try {
                    p.Start();
                }
                catch { }
            }
        }

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                this._lang = _langList[cmbLanguage.SelectedIndex];
            }
            catch {
                this._lang = new Language();
            }

            ApplyLanguage(this._lang);
        }

        private void cmbTheme_SelectedIndexChanged(object sender, EventArgs e) {
            var selectedTheme = new Theme();

            try {
                selectedTheme = this._themeList[cmbTheme.SelectedIndex];
            }
            catch { }

            ApplyTheme(selectedTheme);
            _theme = selectedTheme;
        }

        private void cmbLayout_SelectedIndexChanged(object sender, EventArgs e) {
            _layout = (LayoutMode)cmbLayout.SelectedIndex;
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Load language list
        /// </summary>
        private void LoadLanguageList() {
            cmbLanguage.Items.Clear();
            cmbLanguage.Items.Add("English");

            _langList = new List<Language>
            {
                new Language()
            };

            var langPath = App.StartUpDir(Dir.Languages);

            if (Directory.Exists(langPath)) {
                foreach (var f in Directory.GetFiles(langPath)) {
                    if (string.Equals(Path.GetExtension(f), ".iglang", StringComparison.CurrentCultureIgnoreCase)) {
                        var lang = new Language(f);
                        _langList.Add(lang);

                        var iLang = cmbLanguage.Items.Add(lang.LangName);
                        var curLang = Configs.Language.LangName;

                        // using current language pack
                        if (f.CompareTo(curLang) == 0) {
                            cmbLanguage.SelectedIndex = iLang;
                        }
                    }
                }
            }

            if (cmbLanguage.SelectedIndex == -1) {
                cmbLanguage.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Apply language
        /// </summary>
        /// <param name="lang"></param>
        private void ApplyLanguage(Language lang) {
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
        private void LaunchImageGlass() {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(App.IGExePath);
            p.Start();
        }

        /// <summary>
        /// Load theme list
        /// </summary>
        private void LoadThemeList() {
            cmbTheme.Items.Clear();

            _themeList.Clear();
            _themeList.AddRange(Theme.GetAllThemePacks());

            foreach (var th in _themeList) {
                cmbTheme.Items.Add(th.Name);

                if (Configs.Theme.FolderName.ToUpper().CompareTo(th.FolderName.ToUpper()) == 0) {
                    cmbTheme.SelectedIndex = cmbTheme.Items.Count - 1;
                }
            }
        }

        /// <summary>
        /// Apply theme
        /// </summary>
        /// <param name="th"></param>
        private void ApplyTheme(Theme th) {
            panFooter.BackColor = th.ToolbarBackgroundColor;
            panHeader.BackColor =
                tabLanguage.BackColor =
                tabLayoutMode.BackColor =
                tabTheme.BackColor =
                tabFileAssociation.BackColor =
                th.BackgroundColor;

            this.lblStepNumber.ForeColor =
                this.lblLanguage.ForeColor =
                this.lblLayout.ForeColor =
                this.lblTheme.ForeColor =
                this.lblDefaultApp.ForeColor =
                Theme.InvertBlackAndWhiteColor(th.BackgroundColor);

            // Logo
            picLogo.Image = th.Logo.Image;

            // apply form theme
            Configs.ApplyFormTheme(this, th);
        }

        /// <summary>
        /// Extract and install theme packs
        /// </summary>
        private void InstallThemePacks() {
            var themeFiles = Directory.GetFiles(App.StartUpDir(Dir.Themes), "*.igtheme");

            foreach (var file in themeFiles) {
                Theme.InstallTheme(file);
            }
        }

        /// <summary>
        /// Load layout list
        /// </summary>
        private void LoadLayoutList() {
            cmbLayout.Items.Clear();
            var list = Enum.GetNames(typeof(LayoutMode));

            foreach (var item in list) {
                cmbLayout.Items.Add(_lang.Items[$"{this.Name}.cmbLayout._{item}"]);
            }

            cmbLayout.SelectedIndex = 0;
        }

        /// <summary>
        /// Save and apply settings
        /// </summary>
        private void ApplySettings() {
            Configs.Language = this._lang;
            Configs.Theme = this._theme;
            Configs.BackgroundColor = this._theme.BackgroundColor;

            if (_layout == LayoutMode.Designer) {
                Configs.MouseWheelAction = MouseWheelActions.ScrollVertically;
                Configs.MouseWheelCtrlAction = MouseWheelActions.Zoom;
                Configs.MouseWheelShiftAction = MouseWheelActions.ScrollHorizontally;
                Configs.MouseWheelAltAction = MouseWheelActions.DoNothing;

                Configs.ZoomLockValue = 100f;
                Configs.IsShowColorPickerOnStartup = true;
            }
            else {
                Configs.MouseWheelAction = MouseWheelActions.Zoom;
                Configs.MouseWheelCtrlAction = MouseWheelActions.ScrollVertically;
                Configs.MouseWheelShiftAction = MouseWheelActions.ScrollHorizontally;
                Configs.MouseWheelAltAction = MouseWheelActions.DoNothing;

                Configs.ZoomLockValue = -1f;
            }

            // Save configs to file
            Configs.Write();
        }

        #endregion

    }
}
