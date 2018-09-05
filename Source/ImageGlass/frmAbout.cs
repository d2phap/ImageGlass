/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
Project homepage: http://imageglass.org

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

using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using ImageGlass.Services.Configuration;

namespace ImageGlass
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
        }

        private readonly Color M_COLOR_MENU_ACTIVE = Color.FromArgb(255, 220, 220, 220);
        private readonly Color M_COLOR_MENU_HOVER = Color.FromArgb(255, 247, 247, 247);
        private readonly Color M_COLOR_MENU_NORMAL = Color.FromArgb(255, 240, 240, 240);

        #region MOUSE ENTER - HOVER - DOWN MENU
        private void lblMenu_MouseDown(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.BackColor = M_COLOR_MENU_ACTIVE;
        }

        private void lblMenu_MouseUp(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;

            if (int.Parse(lbl.Tag.ToString()) == 1)
            {
                lbl.BackColor = M_COLOR_MENU_ACTIVE;
            }
            else
            {
                lbl.BackColor = M_COLOR_MENU_HOVER;
            }
        }

        private void lblMenu_MouseEnter(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;

            if (int.Parse(lbl.Tag.ToString()) == 1)
            {
                lbl.BackColor = M_COLOR_MENU_ACTIVE;
            }
            else
            {
                lbl.BackColor = M_COLOR_MENU_HOVER;
            }

        }

        private void lblMenu_MouseLeave(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            if (int.Parse(lbl.Tag.ToString()) == 1)
            {
                lbl.BackColor = M_COLOR_MENU_ACTIVE;
            }
            else
            {
                lbl.BackColor = M_COLOR_MENU_NORMAL;
            }
        }
        #endregion

        private void lblMenu_Click(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;

            if (lbl.Name == "lblInfo")
            {
                tab1.SelectedTab = tpInfo;
            }
            else if (lbl.Name == "lblComponent")
            {
                tab1.SelectedTab = tpComponents;
            }
            else if (lbl.Name == "lblReferences")
            {
                tab1.SelectedTab = tpReferences;
            }
        }

        private void frmAbout_Load(object sender, EventArgs e)
        {
            //Remove tabs header
            tab1.Appearance = TabAppearance.FlatButtons;
            tab1.ItemSize = new Size(0, 1);
            tab1.SizeMode = TabSizeMode.Fixed;

            //this.RightToLeft = GlobalSetting.LangPack.IsRightToLeftLayout;
            lblAppName.Text = Application.ProductName;
            lblVersion.Text = String.Format(GlobalSetting.LangPack.Items["frmAbout.lblVersion"], Application.ProductVersion) + (GlobalSetting.IsPortableMode ? " " + GlobalSetting.LangPack.Items["frmAbout._PortableText"] : "");
            lblCopyright.Text = "Copyright © 2010-" + DateTime.Now.Year.ToString() + " by Dương Diệu Pháp\n" + "All rights reserved.";

            //Load item component
            txtComponents.Text = "";
            foreach (string f in Directory.GetFiles(Application.StartupPath))
            {
                if (Path.GetExtension(f).ToLower() == ".dll" ||
                    Path.GetExtension(f).ToLower() == ".exe")
                {
                    FileVersionInfo fi = FileVersionInfo.GetVersionInfo(f);

                    txtComponents.Text += $"{Path.GetFileName(f)} - {fi.FileVersion}\r\n" +
                        $"{fi.LegalCopyright}\r\n" +
                        $"{f}\r\n" +
                        $"-----------------------------------------\r\n\r\n";
                }
            }

            //Load language:
            lblSlogant.Text = GlobalSetting.LangPack.Items["frmAbout.lblSlogant"];
            lblInfo.Text = GlobalSetting.LangPack.Items["frmAbout.lblInfo"];
            lblComponent.Text = GlobalSetting.LangPack.Items["frmAbout.lblComponent"];
            lblReferences.Text = GlobalSetting.LangPack.Items["frmAbout.lblReferences"];
            lblInfoContact.Text = GlobalSetting.LangPack.Items["frmAbout.lblInfoContact"];
            lblSoftwareUpdate.Text = GlobalSetting.LangPack.Items["frmAbout.lblSoftwareUpdate"];
            lnkCheckUpdate.Text = GlobalSetting.LangPack.Items["frmAbout.lnkCheckUpdate"];
            Text = GlobalSetting.LangPack.Items["frmAbout._Text"];
            
        }

        #region IMAGEGLASS INFORMATION PANEL
        private void lnkEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("mailto:d2phap@gmail.com");
            }
            catch { }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("skype:d2phap");
            }
            catch { }
        }

        
        private void lnkIGHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string version = Application.ProductVersion.Replace(".", "_");
                Process.Start("http://www.imageglass.org?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_homepage");
            }
            catch { }
        }

        private void lnkProjectPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string version = Application.ProductVersion.Replace(".", "_");
                Process.Start("http://www.imageglass.org/source?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_source");
            }
            catch { }
        }

        private void lnkFacebook_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("https://www.facebook.com/ImageGlass");
            }
            catch { }
        }

        private void lnkCheckUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(Application.StartupPath, "igcmd.exe");
            p.StartInfo.Arguments = "igupdate";
            p.Start();
        }
        #endregion

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tab1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblInfo.Tag = 0;
            lblComponent.Tag = 0;
            lblReferences.Tag = 0;

            lblInfo.BackColor = M_COLOR_MENU_NORMAL;
            lblComponent.BackColor = M_COLOR_MENU_NORMAL;
            lblReferences.BackColor = M_COLOR_MENU_NORMAL;

            if (tab1.SelectedTab == tpInfo)
            {
                lblInfo.Tag = 1;
                lblInfo.BackColor = M_COLOR_MENU_ACTIVE;

            }
            else if (tab1.SelectedTab == tpComponents)
            {
                lblComponent.Tag = 1;
                lblComponent.BackColor = M_COLOR_MENU_ACTIVE;
            }
            else if (tab1.SelectedTab == tpReferences)
            {
                lblReferences.Tag = 1;
                lblReferences.BackColor = M_COLOR_MENU_ACTIVE;
            }
        }

        private void btnDonation_Click(object sender, EventArgs e)
        {
            try
            {
                string version = Application.ProductVersion.Replace(".", "_");
                Process.Start("http://www.imageglass.org/source#donation?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_donation");
            }
            catch { }
        }

        private void lnkCollaborator_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("https://github.com/fire-eggs");
            }
            catch { }
        }
    }
}
