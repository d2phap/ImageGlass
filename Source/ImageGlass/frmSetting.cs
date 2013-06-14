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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Security.Principal;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace ImageGlass
{
    public partial class frmSetting : Form
    {
        public frmSetting()
        {
            InitializeComponent();
        }

        private Color M_COLOR_MENU_ACTIVE = Color.FromArgb(255, 0, 123, 176);
        private Color M_COLOR_MENU_HOVER = Color.FromArgb(255, 0, 160, 220);
        private Color M_COLOR_MENU_NORMAL = Color.Silver;        
        private string hkey = "HKEY_CURRENT_USER\\Software\\PhapSoftware\\ImageGlass\\";
        

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
                lbl.BackColor = Color.FromArgb(255, M_COLOR_MENU_ACTIVE.R + 20,
                                                M_COLOR_MENU_ACTIVE.G + 20,
                                                M_COLOR_MENU_ACTIVE.B + 20);
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

        #region MOUSE ENTER - HOVER - DOWN BUTTON
        private void lblButton_MouseDown(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.BackColor = M_COLOR_MENU_ACTIVE;            
        }

        private void lblButton_MouseUp(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.BackColor = M_COLOR_MENU_HOVER;
        }

        private void lblButton_MouseEnter(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.BackColor = M_COLOR_MENU_HOVER;
        }

        private void lblButton_MouseLeave(object sender, EventArgs e)
        {
            Label lbl = (Label)sender; 
            lbl.BackColor = M_COLOR_MENU_NORMAL;            
        }
        #endregion


        private void frmSetting_Load(object sender, EventArgs e)
        {
            lblMenu_Click(lblGeneral, EventArgs.Empty);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmSetting_SizeChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }


        private void lblMenu_Click(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;

            lblGeneral.Tag = 0;
            lblContextMenu.Tag = 0;
            lblLanguage.Tag = 0;
            lblExtension.Tag = 0;

            lblGeneral.BackColor = M_COLOR_MENU_NORMAL;
            lblContextMenu.BackColor = M_COLOR_MENU_NORMAL;
            lblLanguage.BackColor = M_COLOR_MENU_NORMAL;
            lblExtension.BackColor = M_COLOR_MENU_NORMAL;

            lbl.Tag = 1;
            lbl.BackColor = M_COLOR_MENU_ACTIVE;

            if (lbl.Name == "lblGeneral")
            {
                tab1.SelectedTab = tabGeneral;
                LoadTabGeneralConfig();
            }
            else if (lbl.Name == "lblContextMenu")
            {
                tab1.SelectedTab = tabContextMenu;
            }
            else if (lbl.Name == "lblLanguage")
            {
                tab1.SelectedTab = tabLaguage;
            }
            else if (lbl.Name == "lblExtension")
            {
                tab1.SelectedTab = tabExtension;
            }
        }


        #region TAB GENERAL
        private void chkLockWorkspace_CheckedChanged(object sender, EventArgs e)
        {
            Registry.SetValue(hkey, "LockToEdge", chkLockWorkspace.Checked.ToString());
            Setting.IsLockWorkspaceEdges = chkLockWorkspace.Checked;
        }

        private void chkAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoUpdate.Checked)
            {
                Registry.SetValue(hkey, "AutoUpdate", chkAutoUpdate.Checked.ToString());
            }
            else
            {
                Registry.SetValue(hkey, "AutoUpdate", "0");
            }
        }

        private void chkFindChildFolder_CheckedChanged(object sender, EventArgs e)
        {
            Registry.SetValue(hkey, "Recursive", chkFindChildFolder.Checked.ToString());
        }

        private void cmbZoomOptimization_SelectedIndexChanged(object sender, EventArgs e)
        {
            Registry.SetValue(hkey, "ZoomOptimize", cmbZoomOptimization.SelectedIndex);

            if (cmbZoomOptimization.SelectedIndex == 1)
            {
                Setting.ZoomOptimizationMethod = ZoomOptimizationValue.SmoothPixels;
            }
            else if (cmbZoomOptimization.SelectedIndex == 2)
            {
                Setting.ZoomOptimizationMethod = ZoomOptimizationValue.ClearPixels;
            }
            else
            {
                Setting.ZoomOptimizationMethod = ZoomOptimizationValue.Auto;
            }
        }

        private void chkWelcomePicture_CheckedChanged(object sender, EventArgs e)
        {
            Registry.SetValue(hkey, "Welcome", chkWelcomePicture.Checked.ToString());
            Setting.IsWelcomePicture = chkWelcomePicture.Checked;
        }

        private void barInterval_Scroll(object sender, EventArgs e)
        {
            Registry.SetValue(hkey, "Interval", barInterval.Value);
            lblSlideshowInterval.Text = "Slide show interval: " + barInterval.Value.ToString();
        }

        private void numMaxThumbSize_ValueChanged(object sender, EventArgs e)
        {
            Registry.SetValue(hkey, "MaxThumbnailFileSize", numMaxThumbSize.Value);
        }

        private void cmbImageOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            Registry.SetValue(hkey, "ImageLoadingOrder", cmbImageOrder.SelectedIndex);
            Setting.LoadImageOrderConfig();
        }

        /// <summary>
        /// Get and load value of General tab
        /// </summary>
        private void LoadTabGeneralConfig()
        {
            //Get value of chkLockWorkspace
            chkLockWorkspace.Checked = bool.Parse(Registry.GetValue(hkey, "LockToEdge", "true").ToString());

            //Get value of chkFindChildFolder
            chkFindChildFolder.Checked = bool.Parse(Registry.GetValue(hkey, "Recursive", "false").ToString());

            //Get value of cmbAutoUpdate
            string s = Microsoft.Win32.Registry.GetValue(hkey, "AutoUpdate", "true").ToString();
            if (s != "0")
            {
                chkAutoUpdate.Checked = true;
            }
            else
            {
                chkAutoUpdate.Checked = false;
            }

            //Get value of chkWelcomePicture
            chkWelcomePicture.Checked = bool.Parse(Registry.GetValue(hkey, "Welcome", "true").ToString());

            //Get value of cmbZoomOptimization
            s = Registry.GetValue(hkey, "ZoomOptimize", "0").ToString();
            int i = 0;
            if (int.TryParse(s, out i))
            {
                if (-1 < i && i < cmbZoomOptimization.Items.Count)
                {}
                else
                {
                    i = 0;
                }
            }
            cmbZoomOptimization.SelectedIndex = i;

            //Get value of barInterval
            i = int.Parse(Registry.GetValue(hkey, "Interval", "5").ToString());
            if (0 < i && i < 61)
            {
                barInterval.Value = i;
            }
            else
            {
                barInterval.Value = 5;
            }

            lblSlideshowInterval.Text = "Slide show interval: " + barInterval.Value.ToString();

            //Get value of numMaxThumbSize
            s = Registry.GetValue(hkey, "MaxThumbnailFileSize", "1").ToString();
            i = 1;
            if (int.TryParse(s, out i))
            {}
            numMaxThumbSize.Value = i;

            //Get value of cmbImageOrder
            s = Registry.GetValue(hkey, "ImageLoadingOrder", "0").ToString();
            i = 0;
            if (int.TryParse(s, out i))
            {
                if (-1 < i && i < cmbImageOrder.Items.Count)
                { }
                else
                {
                    i = 0;
                }
            }
            cmbImageOrder.SelectedIndex = i;

            //Get background color
            picBackgroundColor.BackColor = Setting.BackgroundColor;
        }
        #endregion

        #region TAB CONTEXT MENU
        [PrincipalPermissionAttribute(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        private void lblAddContextMenu_Click(object sender, EventArgs e)
        {
            string hkey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\*\\Shell\\Open with ImageGlass\\Command\\";
            Registry.SetValue(hkey, "", Application.ExecutablePath + " %1");
        }


        [PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        private void lblRemoveContextMenu_Click(object sender, EventArgs e)
        {
            RegistryKey r;
            r = Registry.LocalMachine.OpenSubKey("Software\\Classes\\*\\Shell", true);
            r.DeleteSubKeyTree("Open with ImageGlass");
        }


        #endregion





        private void frmSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            Setting.IsForcedActive = true;
        }

        private void lnkGetMoreLanguage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.imageglass.org/languages.php" +
                    "?utm_source=imageglass&utm_medium=language_click&utm_campaign=from_app_" +
                    Application.ProductVersion.Replace(".", "_"));
            }
            catch { }
        }

        private void picBackgroundColor_Click(object sender, EventArgs e)
        {
            ColorDialog c = new ColorDialog();
            c.AllowFullOpen = true;

            if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                picBackgroundColor.BackColor = c.Color;
                Setting.BackgroundColor = c.Color;

                //Luu background color
                Registry.SetValue(hkey, "BackgroundColor", Setting.BackgroundColor.ToArgb());
            }
        }

        

        





    }
}
