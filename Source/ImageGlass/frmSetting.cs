/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017 DUONG DIEU PHAP
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
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using ImageGlass.Services.Configuration;
using ImageGlass.Library;
using System.Linq;
using ImageGlass.Theme;
using System.Text;

namespace ImageGlass
{
    public partial class frmSetting : Form
    {
        public frmSetting()
        {
            InitializeComponent();

            RenderTheme r = new RenderTheme();
            r.ApplyTheme(lvExtension);
        }

        private Color M_COLOR_MENU_ACTIVE = Color.FromArgb(255, 220, 220, 220);
        private Color M_COLOR_MENU_HOVER = Color.FromArgb(255, 247, 247, 247);
        private Color M_COLOR_MENU_NORMAL = Color.FromArgb(255, 240, 240, 240);
        private List<Language> dsLanguages = new List<Language>();

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
            //Load config
            //Windows Bound (Position + Size)------------------------------------------------
            Rectangle rc = GlobalSetting.StringToRect(GlobalSetting.GetConfig($"{Name}.WindowsBound", "280,125,610,570"));

            if (!Helper.IsOnScreen(rc.Location))
            {
                rc.Location = new Point(280, 125);
            }
            Bounds = rc;

            //windows state--------------------------------------------------------------
            string s = GlobalSetting.GetConfig(Name + ".WindowsState", "Normal");
            if (s == "Normal")
            {
                WindowState = FormWindowState.Normal;
            }
            else if (s == "Maximized")
            {
                WindowState = FormWindowState.Maximized;
            }

            LoadTabGeneralConfig();
            InitLanguagePack();
        }

        private void frmSetting_SizeChanged(object sender, EventArgs e)
        {
            Refresh();
        }
        
        private void frmSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Save config---------------------------------
            if (WindowState == FormWindowState.Normal)
            {
                //Windows Bound-------------------------------------------------------------------
                GlobalSetting.SetConfig(Name + ".WindowsBound", GlobalSetting.RectToString(Bounds));
            }

            //Windows State-------------------------------------------------------------------
            GlobalSetting.SetConfig(Name + ".WindowsState", WindowState.ToString());
            

            //Force to apply the configurations
            GlobalSetting.IsForcedActive = true;
        }

        private void frmSetting_KeyDown(object sender, KeyEventArgs e)
        {
            //close dialog
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)
            {
                Close();
            }
        }

        /// <summary>
        /// Load language pack
        /// </summary>
        private void InitLanguagePack()
        {
            RightToLeft = GlobalSetting.LangPack.IsRightToLeftLayout;
            Text = GlobalSetting.LangPack.Items["frmSetting._Text"];

            lblGeneral.Text = GlobalSetting.LangPack.Items["frmSetting.lblGeneral"];
            lblImage.Text = GlobalSetting.LangPack.Items["frmSetting.lblImage"];
            lblFileAssociations.Text = GlobalSetting.LangPack.Items["frmSetting.lblFileAssociations"];
            lblLanguage.Text = GlobalSetting.LangPack.Items["frmSetting.lblLanguage"];


            //General tab
            lblHeadStartup.Text = GlobalSetting.LangPack.Items["frmSetting.lblHeadStartup"];//
            chkWelcomePicture.Text = GlobalSetting.LangPack.Items["frmSetting.chkWelcomePicture"];
            chkShowToolBar.Text = GlobalSetting.LangPack.Items["frmSetting.chkShowToolBar"];
            chkAllowMultiInstances.Text = GlobalSetting.LangPack.Items["frmSetting.chkAllowMultiInstances"];

            lblHeadPortableMode.Text = GlobalSetting.LangPack.Items["frmSetting.lblHeadPortableMode"];//
            chkPortableMode.Text = GlobalSetting.LangPack.Items["frmSetting.chkPortableMode"];

            lblHeadOthers.Text = GlobalSetting.LangPack.Items["frmSetting.lblHeadOthers"];//
            chkAutoUpdate.Text = GlobalSetting.LangPack.Items["frmSetting.chkAutoUpdate"];
            chkESCToQuit.Text = GlobalSetting.LangPack.Items["frmSetting.chkESCToQuit"];
            chkConfirmationDelete.Text = GlobalSetting.LangPack.Items["frmSetting.chkConfirmationDelete"];
            lblBackGroundColor.Text = GlobalSetting.LangPack.Items["frmSetting.lblBackGroundColor"];


            //Image tab
            lblHeadImageLoading.Text = GlobalSetting.LangPack.Items["frmSetting.lblHeadImageLoading"];//
            chkFindChildFolder.Text = GlobalSetting.LangPack.Items["frmSetting.chkFindChildFolder"];
            chkLoopViewer.Text = GlobalSetting.LangPack.Items["frmSetting.chkLoopViewer"];
            chkImageBoosterBack.Text = GlobalSetting.LangPack.Items["frmSetting.chkImageBoosterBack"];
            lblImageLoadingOrder.Text = GlobalSetting.LangPack.Items["frmSetting.lblImageLoadingOrder"];

            lblHeadZooming.Text = GlobalSetting.LangPack.Items["frmSetting.lblHeadZooming"];//
            chkMouseNavigation.Text = GlobalSetting.LangPack.Items["frmSetting.chkMouseNavigation"];
            lblGeneral_ZoomOptimization.Text = GlobalSetting.LangPack.Items["frmSetting.lblGeneral_ZoomOptimization"];

            lblHeadThumbnailBar.Text = GlobalSetting.LangPack.Items["frmSetting.lblHeadThumbnailBar"];//
            chkThumbnailVertical.Text = GlobalSetting.LangPack.Items["frmSetting.chkThumbnailVertical"];
            lblGeneral_MaxFileSize.Text = GlobalSetting.LangPack.Items["frmSetting.lblGeneral_MaxFileSize"];
            lblGeneral_ThumbnailSize.Text = GlobalSetting.LangPack.Items["frmSetting.lblGeneral_ThumbnailSize"];

            lblHeadSlideshow.Text = GlobalSetting.LangPack.Items["frmSetting.lblHeadSlideshow"];//
            chkLoopSlideshow.Text = GlobalSetting.LangPack.Items["frmSetting.chkLoopSlideshow"];
            lblSlideshowInterval.Text = string.Format(GlobalSetting.LangPack.Items["frmSetting.lblSlideshowInterval"], barInterval.Value);


            //File Associations tab
            var extList = GlobalSetting.DefaultImageFormats.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            lblExtensionsGroupDescription.Text = GlobalSetting.LangPack.Items["frmSetting.lblExtensionsGroupDescription"];
            lblSupportedExtension.Text = String.Format(GlobalSetting.LangPack.Items["frmSetting.lblSupportedExtension"], extList.Length);
            lnkOpenFileAssoc.Text = GlobalSetting.LangPack.Items["frmSetting.lnkOpenFileAssoc"];
            btnAddNewExt.Text = GlobalSetting.LangPack.Items["frmSetting.btnAddNewExt"];
            btnDeleteExt.Text = GlobalSetting.LangPack.Items["frmSetting.btnDeleteExt"];
            btnRegisterExt.Text = GlobalSetting.LangPack.Items["frmSetting.btnRegisterExt"];
            btnResetExt.Text = GlobalSetting.LangPack.Items["frmSetting.btnResetExt"];
            lvExtension.Groups[(int)ImageExtensionGroup.Default].Header = GlobalSetting.LangPack.Items["_.ImageFormatGroup.Default"];
            lvExtension.Groups[(int)ImageExtensionGroup.Optional].Header = GlobalSetting.LangPack.Items["_.ImageFormatGroup.Optional"];


            //Language tab
            lblLanguageText.Text = GlobalSetting.LangPack.Items["frmSetting.lblLanguageText"];
            lnkRefresh.Text = GlobalSetting.LangPack.Items["frmSetting.lnkRefresh"];
            lblLanguageWarning.Text = string.Format(GlobalSetting.LangPack.Items["frmSetting.lblLanguageWarning"], "ImageGlass " + Application.ProductVersion);
            lnkInstallLanguage.Text = GlobalSetting.LangPack.Items["frmSetting.lnkInstallLanguage"];
            lnkCreateNew.Text = GlobalSetting.LangPack.Items["frmSetting.lnkCreateNew"];
            lnkEdit.Text = GlobalSetting.LangPack.Items["frmSetting.lnkEdit"];
            lnkGetMoreLanguage.Text = GlobalSetting.LangPack.Items["frmSetting.lnkGetMoreLanguage"];


            extList = null;
        }

        /// <summary>
        /// TAB LABEL CLICK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblMenu_Click(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;

            if (lbl.Name == "lblGeneral")
            {
                tab1.SelectedTab = tabGeneral;
            }
            else if (lbl.Name == "lblImage")
            {
                tab1.SelectedTab = tabImage;
            }
            else if (lbl.Name == "lblFileAssociations")
            {
                tab1.SelectedTab = tabFileAssociation;
            }
            else if (lbl.Name == "lblLanguage")
            {
                tab1.SelectedTab = tabLanguage;
            }
        }

        private void tab1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblGeneral.Tag = 0;
            lblImage.Tag = 0;
            lblFileAssociations.Tag = 0;
            lblLanguage.Tag = 0;

            lblGeneral.BackColor = M_COLOR_MENU_NORMAL;
            lblImage.BackColor = M_COLOR_MENU_NORMAL;
            lblFileAssociations.BackColor = M_COLOR_MENU_NORMAL;
            lblLanguage.BackColor = M_COLOR_MENU_NORMAL;

            if (tab1.SelectedTab == tabGeneral)
            {
                lblGeneral.Tag = 1;
                lblGeneral.BackColor = M_COLOR_MENU_ACTIVE;

                LoadTabGeneralConfig();
            }
            else if (tab1.SelectedTab == tabImage)
            {
                lblImage.Tag = 1;
                lblImage.BackColor = M_COLOR_MENU_ACTIVE;

                LoadTabImageConfig();
            }
            else if (tab1.SelectedTab == tabFileAssociation)
            {
                lblFileAssociations.Tag = 1;
                lblFileAssociations.BackColor = M_COLOR_MENU_ACTIVE;
                
                // Load image formats to the list
                LoadExtensionList();
            }
            else if (tab1.SelectedTab == tabLanguage)
            {
                lblLanguage.Tag = 1;
                lblLanguage.BackColor = M_COLOR_MENU_ACTIVE;

                lnkRefresh_LinkClicked(null, null);
            }
        }


        #region TAB GENERAL

        /// <summary>
        /// Get and load value of General tab
        /// </summary>
        private void LoadTabGeneralConfig()
        {
            //Get value of chkWelcomePicture ----------------------------------------------------
            chkWelcomePicture.Checked = GlobalSetting.IsShowWelcome;

            //Get value of chkShowToolBar
            chkShowToolBar.Checked = GlobalSetting.IsShowToolBar;

            //Get Portable mode value -----------------------------------------------------------
            chkPortableMode.Checked = GlobalSetting.IsPortableMode;
            if (!GlobalSetting.CheckStartUpDirWritable())
            {
                chkPortableMode.Enabled = false;
            }

            //Get value of cmbAutoUpdate --------------------------------------------------------
            string configValue = GlobalSetting.GetConfig("AutoUpdate", DateTime.Now.ToString());
            if (configValue != "0")
            {
                chkAutoUpdate.Checked = true;
            }
            else
            {
                chkAutoUpdate.Checked = false;
            }

            //Get value of IsAllowMultiInstances
            chkAllowMultiInstances.Checked = GlobalSetting.IsAllowMultiInstances;

            //Get value of IsPressESCToQuit
            chkESCToQuit.Checked = GlobalSetting.IsPressESCToQuit;

            //Get value of IsConfirmationDelete
            chkConfirmationDelete.Checked = GlobalSetting.IsConfirmationDelete;
            
            //Get background color
            picBackgroundColor.BackColor = GlobalSetting.BackgroundColor;
        }

        
        private void chkWelcomePicture_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsShowWelcome = chkWelcomePicture.Checked;
            GlobalSetting.SetConfig("IsShowWelcome", GlobalSetting.IsShowWelcome.ToString());
        }

        private void chkShowToolBar_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsShowToolBar = chkShowToolBar.Checked;
            GlobalSetting.SetConfig("IsShowToolbar", GlobalSetting.IsShowToolBar.ToString());
        }

        private void chkPortableMode_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsPortableMode = chkPortableMode.Checked;

            // Check if user ia using temporary Portable mode from param
            if (Environment.GetCommandLineArgs().ToList().IndexOf("--portable") == -1)
            {
                GlobalSetting.SetConfig("IsPortableMode", GlobalSetting.IsPortableMode.ToString(), true);
            }
        }

        private void chkAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoUpdate.Checked)
            {
                GlobalSetting.SetConfig("AutoUpdate", DateTime.Now.ToString());
            }
            else
            {
                GlobalSetting.SetConfig("AutoUpdate", "0");
            }
        }

        private void chkAllowMultiInstances_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsAllowMultiInstances = chkAllowMultiInstances.Checked;
            GlobalSetting.SetConfig("IsAllowMultiInstances", GlobalSetting.IsAllowMultiInstances.ToString());
        }

        private void chkESCToQuit_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsPressESCToQuit = chkESCToQuit.Checked;
            GlobalSetting.SetConfig("IsPressESCToQuit", GlobalSetting.IsPressESCToQuit.ToString());
        }

        private void chkConfirmationDelete_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsConfirmationDelete = chkConfirmationDelete.Checked;
            GlobalSetting.SetConfig("IsConfirmationDelete", GlobalSetting.IsConfirmationDelete.ToString());
        }

        private void picBackgroundColor_Click(object sender, EventArgs e)
        {
            ColorDialog c = new ColorDialog()
            {
                AllowFullOpen = true
            };

            if (c.ShowDialog() == DialogResult.OK)
            {
                picBackgroundColor.BackColor = c.Color;
                GlobalSetting.BackgroundColor = c.Color;

                //Save background color
                GlobalSetting.SetConfig("BackgroundColor", GlobalSetting.BackgroundColor.ToArgb().ToString());
            }
        }


        #endregion


        #region TAB IMAGE

        /// <summary>
        /// Get and load value of Image tab
        /// </summary>
        private void LoadTabImageConfig()
        {
            //Get value of chkFindChildFolder ---------------------------------------------
            chkFindChildFolder.Checked = GlobalSetting.IsRecursiveLoading;            

            //Get value of chkLoopViewer
            chkLoopViewer.Checked = GlobalSetting.IsLoopBackViewer;

            //Get value of chkImageBoosterBack
            chkImageBoosterBack.Checked = GlobalSetting.IsImageBoosterBack;

            //Load items of cmbImageOrder
            cmbImageOrder.Items.Clear();
            cmbImageOrder.Items.Add(GlobalSetting.LangPack.Items["frmSetting.cmbImageOrder._Name"]);
            cmbImageOrder.Items.Add(GlobalSetting.LangPack.Items["frmSetting.cmbImageOrder._Length"]);
            cmbImageOrder.Items.Add(GlobalSetting.LangPack.Items["frmSetting.cmbImageOrder._CreationTime"]);
            cmbImageOrder.Items.Add(GlobalSetting.LangPack.Items["frmSetting.cmbImageOrder._LastAccessTime"]);
            cmbImageOrder.Items.Add(GlobalSetting.LangPack.Items["frmSetting.cmbImageOrder._LastWriteTime"]);
            cmbImageOrder.Items.Add(GlobalSetting.LangPack.Items["frmSetting.cmbImageOrder._Extension"]);
            cmbImageOrder.Items.Add(GlobalSetting.LangPack.Items["frmSetting.cmbImageOrder._Random"]);

            //Get value of cmbImageOrder
            cmbImageOrder.SelectedIndex = (int)GlobalSetting.ImageLoadingOrder;

            //Use mouse wheel to browse images ----------------------------------------------
            chkMouseNavigation.Checked = GlobalSetting.IsMouseNavigation;

            //Load items of cmbZoomOptimization 
            cmbZoomOptimization.Items.Clear();
            cmbZoomOptimization.Items.Add(GlobalSetting.LangPack.Items["frmSetting.cmbZoomOptimization._Auto"]);
            cmbZoomOptimization.Items.Add(GlobalSetting.LangPack.Items["frmSetting.cmbZoomOptimization._SmoothPixels"]);
            cmbZoomOptimization.Items.Add(GlobalSetting.LangPack.Items["frmSetting.cmbZoomOptimization._ClearPixels"]);

            //Get value of cmbZoomOptimization
            cmbZoomOptimization.SelectedIndex = (int)GlobalSetting.ZoomOptimizationMethod;

            //Thumbnail bar on right side ----------------------------------------------------
            chkThumbnailVertical.Checked = !GlobalSetting.IsThumbnailHorizontal;

            //load thumbnail dimension
            cmbThumbnailDimension.SelectedItem = GlobalSetting.ThumbnailDimension.ToString();
            
            //Get value of chkLoopSlideshow --------------------------------------------------
            chkLoopSlideshow.Checked = GlobalSetting.IsLoopBackSlideShow;

            //Get value of barInterval
            barInterval.Value = GlobalSetting.SlideShowInterval;
            lblSlideshowInterval.Text = string.Format(GlobalSetting.LangPack.Items["frmSetting.lblSlideshowInterval"], barInterval.Value);
            
        }

        private void chkFindChildFolder_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsRecursiveLoading = chkFindChildFolder.Checked;
            GlobalSetting.SetConfig("IsRecursiveLoading", GlobalSetting.IsRecursiveLoading.ToString());
        }

        private void chkLoopViewer_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsLoopBackViewer = chkLoopViewer.Checked;
            GlobalSetting.SetConfig("IsLoopBackViewer", GlobalSetting.IsLoopBackViewer.ToString());
        }

        private void chkImageBoosterBack_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsImageBoosterBack = chkImageBoosterBack.Checked;
            GlobalSetting.SetConfig("IsImageBoosterBack", GlobalSetting.IsImageBoosterBack.ToString());
        }

        private void cmbImageOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            GlobalSetting.SetConfig("ImageLoadingOrder", cmbImageOrder.SelectedIndex.ToString());
            GlobalSetting.LoadImageOrderConfig();
        }

        private void chkMouseNavigation_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsMouseNavigation = chkMouseNavigation.Checked;
            GlobalSetting.SetConfig("IsMouseNavigation", GlobalSetting.IsMouseNavigation.ToString());
        }

        private void cmbZoomOptimization_SelectedIndexChanged(object sender, EventArgs e)
        {
            GlobalSetting.ZoomOptimizationMethod = (ZoomOptimizationValue)cmbZoomOptimization.SelectedIndex;
            GlobalSetting.SetConfig("ZoomOptimization", ((int)GlobalSetting.ZoomOptimizationMethod).ToString());
        }

        private void chkThumbnailVertical_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsThumbnailHorizontal = !chkThumbnailVertical.Checked;
            GlobalSetting.SetConfig("IsThumbnailHorizontal", GlobalSetting.IsThumbnailHorizontal.ToString());
        }

        private void numMaxThumbSize_ValueChanged(object sender, EventArgs e)
        {
            GlobalSetting.SetConfig("MaxThumbnailFileSize", numMaxThumbSize.Value.ToString());
        }

        private void cmbThumbnailDimension_SelectedIndexChanged(object sender, EventArgs e)
        {
            GlobalSetting.ThumbnailDimension = cmbThumbnailDimension.SelectedItem.ToString() == "" ? GlobalSetting.ThumbnailDimension : int.Parse(cmbThumbnailDimension.SelectedItem.ToString());
            GlobalSetting.SetConfig("ThumbnailDimension", GlobalSetting.ThumbnailDimension.ToString());
        }

        private void chkLoopSlideshow_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSetting.IsLoopBackSlideShow = chkLoopSlideshow.Checked;
            GlobalSetting.SetConfig("IsLoopBackSlideShow", GlobalSetting.IsLoopBackSlideShow.ToString());
        }

        private void barInterval_Scroll(object sender, EventArgs e)
        {
            GlobalSetting.SlideShowInterval = barInterval.Value;
            GlobalSetting.SetConfig("SlideShowInterval", GlobalSetting.SlideShowInterval.ToString());
            lblSlideshowInterval.Text = string.Format(GlobalSetting.LangPack.Items["frmSetting.lblSlideshowInterval"], barInterval.Value);
        }

        #endregion


        #region TAB LANGUAGE
        private void lnkGetMoreLanguage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string version = Application.ProductVersion.Replace(".", "_");
                Process.Start("http://www.imageglass.org/download/languagepacks?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_languagepack");
            }
            catch { }
        }

        private void lnkInstallLanguage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = p.StartInfo.FileName = Path.Combine(GlobalSetting.StartUpDir, "igtasks.exe");
            p.StartInfo.Arguments = "iginstalllang";
            p.Start();
        }

        private void lnkCreateNew_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = p.StartInfo.FileName = Path.Combine(GlobalSetting.StartUpDir, "igtasks.exe");
            p.StartInfo.Arguments = "ignewlang";
            p.Start();
        }

        private void lnkEdit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = p.StartInfo.FileName = Path.Combine(GlobalSetting.StartUpDir, "igtasks.exe");
            p.StartInfo.Arguments = "igeditlang \"" + GlobalSetting.LangPack.FileName + "\"";
            p.Start();
        }

        private void lnkRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cmbLanguage.Items.Clear();
            cmbLanguage.Items.Add("English");
            dsLanguages = new List<Library.Language>
            {
                new Library.Language()
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
                        dsLanguages.Add(l);

                        int iLang = cmbLanguage.Items.Add(l.LangName);
                        string curLang = GlobalSetting.LangPack.FileName;

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
        
        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblLanguageWarning.Visible = false;
            GlobalSetting.LangPack = dsLanguages[cmbLanguage.SelectedIndex];

            //check compatibility
            var lang = new Language();
            if(lang.MinVersion.CompareTo(GlobalSetting.LangPack.MinVersion) != 0)
            {
                lblLanguageWarning.Visible = true;
            }
        }

        





        #endregion


        #region TAB FILE ASSOCIATIONS
        /// <summary>
        /// Load built-in extensions to the list view
        /// </summary>
        /// <param name="builtInImageFormats"></param>
        private void LoadExtensionList(string builtInImageFormats)
        {
            var list = builtInImageFormats.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            GlobalSetting.DefaultImageFormats = list[0];
            GlobalSetting.OptionalImageFormats = list[1];

            LoadExtensionList();
        }

        /// <summary>
        /// Load extensions from settings to the list view
        /// </summary>
        private void LoadExtensionList()
        {
            lvExtension.Items.Clear();

            // Load Default group
            var extList = GlobalSetting.DefaultImageFormats.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var ext in extList)
            {
                var li = new ListViewItem(lvExtension.Groups["Default"])
                {
                    Text = ext
                };

                lvExtension.Items.Add(li);
            }

            // Load Optional group
            extList = GlobalSetting.OptionalImageFormats.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var ext in extList)
            {
                var li = new ListViewItem(lvExtension.Groups["Optional"])
                {
                    Text = ext
                };

                lvExtension.Items.Add(li);
            }

            lblSupportedExtension.Text = String.Format(GlobalSetting.LangPack.Items["frmSetting.lblSupportedExtension"], lvExtension.Items.Count);

            // Write suported image formats to settings -----------------------------------------
            // Load Default Image Formats
            GlobalSetting.SetConfig("DefaultImageFormats", GlobalSetting.DefaultImageFormats);
            // Load Optional Image Formats
            GlobalSetting.SetConfig("OptionalImageFormats", GlobalSetting.OptionalImageFormats);
        }
        
        /// <summary>
        /// Register file associations
        /// </summary>
        /// <param name="extensions">Extensions to be registered, ex: *.png;*.bmp;</param>
        private void RegisterFileAssociations(string extensions, bool @isBuiltinExtension = false)
        {
            if (isBuiltinExtension)
            {
                LoadExtensionList(extensions);
            }
            else
            {
                LoadExtensionList();
            }
            

            // Update extensions to registry
            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(GlobalSetting.StartUpDir, "igtasks.exe");
            p.StartInfo.Arguments = $"regassociations {extensions}";
            p.Start();
        }

        private void lnkOpenFileAssoc_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string controlpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "control.exe"); // path to %windir%\system32\control.exe (ensures the correct control.exe)

            Process.Start(controlpath, "/name Microsoft.DefaultPrograms /page pageFileAssoc");
        }

        private void btnResetExt_Click(object sender, EventArgs e)
        {
            RegisterFileAssociations(GlobalSetting.BuiltInImageFormats, true);
        }

        private void btnDeleteExt_Click(object sender, EventArgs e)
        {
            if (lvExtension.CheckedItems.Count == 0)
                return;
            
            var selectedDefaultExts = new StringBuilder();
            var selectedOptionalExts = new StringBuilder();

            // Get checked extensions in the list then
            // remove extensions from settings
            foreach (ListViewItem li in lvExtension.CheckedItems)
            {
                if (li.Group.Name == "Default")
                {
                    GlobalSetting.DefaultImageFormats = GlobalSetting.DefaultImageFormats.Replace($"*{li.Text};", "");
                }
                else if (li.Group.Name == "Optional")
                {
                    GlobalSetting.OptionalImageFormats = GlobalSetting.OptionalImageFormats.Replace($"*{li.Text};", "");
                }
            }

            RegisterFileAssociations(GlobalSetting.AllImageFormats);
        }

        private void btnAddNewExt_Click(object sender, EventArgs e)
        {
            frmAddNewFormat f = new frmAddNewFormat()
            {
                ImageExtension = ".svg",
                ExtensionGroup = ImageExtensionGroup.Default
            };

            do
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    // If the ext exist
                    if (GlobalSetting.AllImageFormats.Contains(f.ImageExtension))
                        return;

                    if (f.ExtensionGroup == ImageExtensionGroup.Default)
                    {
                        GlobalSetting.DefaultImageFormats += f.ImageExtension;
                    }
                    else if (f.ExtensionGroup == ImageExtensionGroup.Optional)
                    {
                        GlobalSetting.OptionalImageFormats += f.ImageExtension;
                    }

                    RegisterFileAssociations(GlobalSetting.AllImageFormats);
                }
            }
            while (f.DialogResult == DialogResult.Retry);
            
        }

        private void btnRegisterExt_Click(object sender, EventArgs e)
        {
            RegisterFileAssociations(GlobalSetting.AllImageFormats);
        }


        #endregion
    }
}
