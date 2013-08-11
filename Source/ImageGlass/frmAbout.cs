/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;

namespace ImageGlass
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
        }

        private Color M_COLOR_MENU_ACTIVE = Color.FromArgb(255, 0, 123, 176);
        private Color M_COLOR_MENU_HOVER = Color.FromArgb(255, 0, 160, 220);
        private Color M_COLOR_MENU_NORMAL = Color.Silver;
        private Point M_PANEL_LOCATION = new Point(12, 140);
        

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
                lbl.BackColor = Color.FromArgb(255, M_COLOR_MENU_ACTIVE.R,
                                                M_COLOR_MENU_ACTIVE.G - 20,
                                                M_COLOR_MENU_ACTIVE.B - 20);
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
                lbl.BackColor = Color.FromArgb(255, M_COLOR_MENU_ACTIVE.R + 20,
                                                M_COLOR_MENU_ACTIVE.G + 20,
                                                M_COLOR_MENU_ACTIVE.B + 20);
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

            lblInfo.Tag = 0;
            lblComponent.Tag = 0;
            lblReferences.Tag = 0;

            lblInfo.BackColor = M_COLOR_MENU_NORMAL;
            lblComponent.BackColor = M_COLOR_MENU_NORMAL;
            lblReferences.BackColor = M_COLOR_MENU_NORMAL;

            lbl.Tag = 1;
            lbl.BackColor = M_COLOR_MENU_ACTIVE;

            if (lbl.Name == "lblInfo")
            {
                panInfo.Location = M_PANEL_LOCATION;
                panInfo.BringToFront();
                panInfo.Visible = true;

                panComponent.Visible = false;
                panReferences.Visible = false;
            }
            else if (lbl.Name == "lblComponent")
            {
                panComponent.Location = M_PANEL_LOCATION;
                panComponent.BringToFront();
                panComponent.Visible = true;

                panInfo.Visible = false;
                panReferences.Visible = false;
            }
            else if (lbl.Name == "lblReferences")
            {
                panReferences.Location = M_PANEL_LOCATION;
                panReferences.BringToFront();
                panReferences.Visible = true;

                panInfo.Visible = false;
                panComponent.Visible = false;
            }
        }


        private void frmAbout_Load(object sender, EventArgs e)
        {
            lblVersion.Text = String.Format(Setting.LangPack.Items["frmAbout.lblVersion"], 
                                            Application.ProductVersion);
            lblCopyright.Text = "Copyright © 2010-" + DateTime.Now.Year.ToString() + " by Dương Diệu Pháp\n" +
                                "All rights reserved.";

            //Load item component
            foreach (string f in Directory.GetFiles(Application.StartupPath))
            {
                if (Path.GetExtension(f).ToLower() == ".dll" ||
                    Path.GetExtension(f).ToLower() == ".exe")
                {
                    fileList1.AddItems(f);
                }
            }
            fileList1.ReLoadItems();

            lblMenu_Click(lblInfo, EventArgs.Empty);

            //Load language:
            lblSlogant.Text = Setting.LangPack.Items["frmAbout.lblSlogant"];
            lblInfo.Text = Setting.LangPack.Items["frmAbout.lblInfo"];
            lblComponent.Text = Setting.LangPack.Items["frmAbout.lblComponent"];
            lblReferences.Text = Setting.LangPack.Items["frmAbout.lblReferences"];
            lblInfoContact.Text = Setting.LangPack.Items["frmAbout.lblInfoContact"];
            lblSoftwareUpdate.Text = Setting.LangPack.Items["frmAbout.lblSoftwareUpdate"];
            lnkCheckUpdate.Text = Setting.LangPack.Items["frmAbout.lnkCheckUpdate"];
            this.Text = Setting.LangPack.Items["frmAbout._Text"];

        }

        #region IMAGEGLASS INFORMATION PANEL
        private void lnkEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("mailto:d2phap@gmal.com");
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

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("tel:+841674710360");
            }
            catch { }
        }
        private void lnkIGHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://www.imageglass.org");
            }
            catch { }
        }

        private void lnkProjectPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://www.imageglass.org/source");
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
            p.StartInfo.FileName = (Application.StartupPath + "\\").Replace("\\\\", "\\") + "igcmd.exe";
            p.StartInfo.Arguments = "igupdate";
            p.Start();
        }
        #endregion

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }








    }
}