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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ImageGlass.Base;
using ImageGlass.Settings;

namespace ImageGlass {
    public partial class frmAbout: Form {
        public frmAbout() {
            InitializeComponent();

            // Apply theme
            LoadTheme();
        }

        private void LoadTheme() {
            // load theme colors
            lblAppName.ForeColor = Configs.Theme.AccentDarkColor;
            lblCodeName.ForeColor = Configs.Theme.AccentColor;

            // Logo
            picLogo.Image = Configs.Theme.Logo.Image;

            // Apply theme
            Configs.ApplyFormTheme(this, Configs.Theme);
        }


        private readonly Color M_COLOR_MENU_SELECTED = Color.FromArgb(255, 198, 203, 204);
        private readonly Color M_COLOR_MENU_ACTIVE = Color.FromArgb(255, 145, 150, 153);
        private readonly Color M_COLOR_MENU_HOVER = Color.FromArgb(255, 176, 181, 183);
        private readonly Color M_COLOR_MENU_NORMAL = Color.FromArgb(255, 160, 165, 168);

        #region MOUSE ENTER - HOVER - DOWN MENU
        private void lblMenu_MouseDown(object sender, MouseEventArgs e) {
            var lbl = (Label)sender;
            lbl.BackColor = M_COLOR_MENU_ACTIVE;
        }

        private void lblMenu_MouseUp(object sender, MouseEventArgs e) {
            var lbl = (Label)sender;

            if (int.Parse(lbl.Tag.ToString()) == 1) {
                lbl.BackColor = M_COLOR_MENU_SELECTED;
            }
            else {
                lbl.BackColor = M_COLOR_MENU_HOVER;
            }
        }

        private void lblMenu_MouseEnter(object sender, EventArgs e) {
            var lbl = (Label)sender;

            if (int.Parse(lbl.Tag.ToString()) == 1) {
                lbl.BackColor = M_COLOR_MENU_SELECTED;
            }
            else {
                lbl.BackColor = M_COLOR_MENU_HOVER;
            }
        }

        private void lblMenu_MouseLeave(object sender, EventArgs e) {
            var lbl = (Label)sender;
            if (int.Parse(lbl.Tag.ToString()) == 1) {
                lbl.BackColor = M_COLOR_MENU_SELECTED;
            }
            else {
                lbl.BackColor = M_COLOR_MENU_NORMAL;
            }
        }
        #endregion

        private void lblMenu_Click(object sender, EventArgs e) {
            var lbl = (Label)sender;

            if (lbl.Name == "lblInfo") {
                tab1.SelectedTab = tpInfo;
            }
            else if (lbl.Name == "lblComponent") {
                tab1.SelectedTab = tpComponents;
            }
            else if (lbl.Name == "lblReferences") {
                tab1.SelectedTab = tpReferences;
            }
        }

        private void frmAbout_Load(object sender, EventArgs e) {
            var lang = Configs.Language.Items;

            // this.RightToLeft = Configs.Language.IsRightToLeftLayout;
            lblAppName.Text = Application.ProductName;
            lblVersion.Text = string.Format(lang["frmAbout.lblVersion"], App.Version)
                + (App.IsPortable ? " " + lang["frmAbout._PortableText"] : "");

            lblCopyright.Text = "Copyright © 2010-" + DateTime.Now.Year.ToString() + " by Dương Diệu Pháp\nAll rights reserved.";

            // Load item component
            txtComponents.Text = "\r\n";
            foreach (var f in Directory.GetFiles(Application.StartupPath)) {
                var ext = Path.GetExtension(f).ToLower();

                if (ext == ".dll" || ext == ".exe") {
                    var fi = FileVersionInfo.GetVersionInfo(f);

                    txtComponents.Text += $"{Path.GetFileName(f)} - {fi.FileVersion}\r\n" +
                        $"{fi.LegalCopyright}\r\n" +
                        $"{f}\r\n" +
                        "-----------------------------------------\r\n\r\n";
                }
            }
            txtComponents.Text += "\r\n";

            // Load language:
            this.Text = lang["frmAbout._Text"];
            lblSlogant.Text = lang["frmAbout.lblSlogant"];
            lblInfo.Text = lang["frmAbout.lblInfo"];
            lblComponent.Text = lang["frmAbout.lblComponent"];
            lblReferences.Text = lang["frmAbout.lblReferences"];
            lblInfoContact.Text = lang["frmAbout.lblInfoContact"];
            lblSoftwareUpdate.Text = lang["frmAbout.lblSoftwareUpdate"];
            lnkCheckUpdate.Text = lang["frmAbout.lnkCheckUpdate"];
        }

        #region IMAGEGLASS INFORMATION PANEL
        private void lnkEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start("mailto:phap@imageglass.org");
            }
            catch { }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start("https://twitter.com/duongdieuphap");
            }
            catch { }
        }

        private void lnkIGHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                var version = App.Version;

                Process.Start("https://imageglass.org?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_homepage");
            }
            catch { }
        }

        private void lnkProjectPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                var version = App.Version;

                Process.Start("https://imageglass.org/source?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_source");
            }
            catch { }
        }

        private void lnkFacebook_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start("https://www.facebook.com/ImageGlass");
            }
            catch { }
        }

        private void lnkCheckUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Program.CheckForUpdate();
        }
        #endregion

        private void btnClose_Click(object sender, EventArgs e) {
            Close();
        }

        private void tab1_SelectedIndexChanged(object sender, EventArgs e) {
            lblInfo.Tag = 0;
            lblComponent.Tag = 0;
            lblReferences.Tag = 0;

            lblInfo.BackColor = M_COLOR_MENU_NORMAL;
            lblComponent.BackColor = M_COLOR_MENU_NORMAL;
            lblReferences.BackColor = M_COLOR_MENU_NORMAL;

            if (tab1.SelectedTab == tpInfo) {
                lblInfo.Tag = 1;
                lblInfo.BackColor = M_COLOR_MENU_ACTIVE;
            }
            else if (tab1.SelectedTab == tpComponents) {
                lblComponent.Tag = 1;
                lblComponent.BackColor = M_COLOR_MENU_ACTIVE;
            }
            else if (tab1.SelectedTab == tpReferences) {
                lblReferences.Tag = 1;
                lblReferences.BackColor = M_COLOR_MENU_ACTIVE;
            }
        }

        private void btnDonation_Click(object sender, EventArgs e) {
            try {
                Process.Start("https://imageglass.org/source#donation?utm_source=app_" + App.Version + "&utm_medium=app_click&utm_campaign=app_donation");
            }
            catch { }
        }

        private void lnkCollaborator_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start("https://github.com/fire-eggs");
            }
            catch { }
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start("https://www.patreon.com/d2phap?utm_source=app_" + App.Version + "&utm_medium=app_click&utm_campaign=app_patreon");
            }
            catch { }
        }
    }
}
