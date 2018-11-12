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
using System.Reflection;

namespace ImageGlass
{
    public partial class frmSetting : Form
    {
        public frmSetting()
        {
            InitializeComponent();

            imglGeneral.ImageSize = new Size(10, DPIScaling.TransformNumber(30));
            imglGeneral.Images.Add("_blank", new Bitmap(10, DPIScaling.TransformNumber(30)));
            
        }

        #region PROPERTIES
        private Color M_COLOR_MENU_ACTIVE = Color.FromArgb(255, 220, 220, 220);
        private Color M_COLOR_MENU_HOVER = Color.FromArgb(255, 247, 247, 247);
        private Color M_COLOR_MENU_NORMAL = Color.FromArgb(255, 240, 240, 240);
        private List<Language> dsLanguages = new List<Language>();

        #region Toolbar
        private string _separatorText; // Text used in lists to represent separator bar
        private ImageList _lstToolbarImg;
        private List<ListViewItem> _lstMasterUsed;


        // instance of frmMain, for reflection
        public frmMain MainInstance { get; internal set; }
        #endregion

        #endregion


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


        #region FRMSETTING FORM EVENTS
        private void frmSetting_Load(object sender, EventArgs e)
        {
            //Remove tabs header
            tab1.Appearance = TabAppearance.FlatButtons;
            tab1.ItemSize = new Size(0, 1);
            tab1.SizeMode = TabSizeMode.Fixed;

            //Load config
            //Windows Bound (Position + Size)-------------------------------------------
            Rectangle rc = GlobalSetting.StringToRect(GlobalSetting.GetConfig($"{Name}.WindowsBound", "280,125,900,700"));

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

            InitLanguagePack(); // Needs to be done before setting up the initial tab

            //Get the last view of tab --------------------------------------------------
            tab1.SelectedIndex = LocalSetting.SettingsTabLastView;
            // KBR prevent loading tab config twice tab1_SelectedIndexChanged(tab1, null); //Load tab's configs

            //Load configs
            LoadTabGeneralConfig();
            LoadTabImageConfig();
            lnkRefresh_LinkClicked(null, null);

            //to prevent the setting: ToolbarPosition = -1, we load this onLoad event
            LoadTabToolbar();
        }


        private void frmSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Save config---------------------------------
            if (WindowState == FormWindowState.Normal)
            {
                //Windows Bound-------------------------------------------------------------------
                GlobalSetting.SetConfig(Name + ".WindowsBound", GlobalSetting.RectToString(Bounds));
            }


            GlobalSetting.SetConfig(Name + ".WindowsState", WindowState.ToString());
            GlobalSetting.SaveConfigOfImageEditingAssociationList();

            //Tabs State---------------------------------------------------------------------------
            LocalSetting.SettingsTabLastView = tab1.SelectedIndex;


        }


        private void frmSetting_KeyDown(object sender, KeyEventArgs e)
        {
            //close dialog
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)
            {
                Close();
            }
        }


        private void frmSetting_SizeChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        #endregion




        /// <summary>
        /// Load language pack
        /// </summary>
        private void InitLanguagePack()
        {
            var lang = GlobalSetting.LangPack.Items;

            RightToLeft = GlobalSetting.LangPack.IsRightToLeftLayout;
            Text = lang["frmSetting._Text"];


            #region Tabs label
            lblGeneral.Text = lang["frmSetting.lblGeneral"];
            lblImage.Text = lang["frmSetting.lblImage"];
            lblFileAssociations.Text = lang["frmSetting.lblFileAssociations"];
            lblLanguage.Text = lang["frmSetting.lblLanguage"];
            lblToolbar.Text = lang["frmSetting.lblToolbar"];
            lblColorPicker.Text = lang["frmSetting.lblColorPicker"];
            lblTheme.Text = lang["frmSetting.lblTheme"];

            btnSave.Text = lang["frmSetting.btnSave"];
            btnCancel.Text = lang["frmSetting.btnCancel"];
            btnApply.Text = lang["frmSetting.btnApply"];
            #endregion


            #region GENERAL TAB
            lblHeadStartup.Text = lang["frmSetting.lblHeadStartup"];//
            chkWelcomePicture.Text = lang["frmSetting.chkWelcomePicture"];
            chkShowToolBar.Text = lang["frmSetting.chkShowToolBar"];
            chkAllowMultiInstances.Text = lang["frmSetting.chkAllowMultiInstances"];


            lblHeadConfigDir.Text = lang["frmSetting.lblHeadConfigDir"];//
            lnkConfigDir.Text = GlobalSetting.ConfigDir;


            lblHeadOthers.Text = lang["frmSetting.lblHeadOthers"];//
            chkAutoUpdate.Text = lang["frmSetting.chkAutoUpdate"];
            chkESCToQuit.Text = lang["frmSetting.chkESCToQuit"];
            chkConfirmationDelete.Text = lang["frmSetting.chkConfirmationDelete"];
            chkShowScrollbar.Text = lang["frmSetting.chkShowScrollbar"];
            chkDisplayBasename.Text = lang["frmSetting.chkDisplayBasename"];
            lblBackGroundColor.Text = lang["frmSetting.lblBackGroundColor"];
            lnkResetBackgroundColor.Text = lang["frmSetting.lnkResetBackgroundColor"];
            #endregion


            #region IMAGE TAB
            lblHeadImageLoading.Text = lang["frmSetting.lblHeadImageLoading"];//
            chkFindChildFolder.Text = lang["frmSetting.chkFindChildFolder"];
            chkShowHiddenImages.Text = lang["frmSetting.chkShowHiddenImages"];
            chkLoopViewer.Text = lang["frmSetting.chkLoopViewer"];
            chkImageBoosterBack.Text = lang["frmSetting.chkImageBoosterBack"];
            lblImageLoadingOrder.Text = lang["frmSetting.lblImageLoadingOrder"];

            lblHeadMouseWheelActions.Text = lang["frmSetting.lblHeadMouseWheelActions"];
            lblMouseWheel.Text = lang["frmSetting.lblMouseWheel"];
            lblMouseWheelAlt.Text = lang["frmSetting.lblMouseWheelAlt"];
            lblMouseWheelCtrl.Text = lang["frmSetting.lblMouseWheelCtrl"];
            lblMouseWheelShift.Text = lang["frmSetting.lblMouseWheelShift"];

            lblHeadZooming.Text = lang["frmSetting.lblHeadZooming"];//
            //chkMouseNavigation.Text = lang["frmSetting.chkMouseNavigation"];
            lblGeneral_ZoomOptimization.Text = lang["frmSetting.lblGeneral_ZoomOptimization"];

            lblHeadThumbnailBar.Text = lang["frmSetting.lblHeadThumbnailBar"];//
            chkThumbnailVertical.Text = lang["frmSetting.chkThumbnailVertical"];
            chkShowThumbnailScrollbar.Text = lang["frmSetting.chkShowThumbnailScrollbar"];
            lblGeneral_ThumbnailSize.Text = lang["frmSetting.lblGeneral_ThumbnailSize"];

            lblHeadSlideshow.Text = lang["frmSetting.lblHeadSlideshow"];//
            chkLoopSlideshow.Text = lang["frmSetting.chkLoopSlideshow"];
            lblSlideshowInterval.Text = string.Format(lang["frmSetting.lblSlideshowInterval"], barInterval.Value);

            lblHeadImageEditing.Text = lang["frmSetting.lblHeadImageEditing"];//
            chkSaveOnRotate.Text = lang["frmSetting.chkSaveOnRotate"];
            chkSaveModifyDate.Text = lang["frmSetting.chkSaveModifyDate"];
            lblSelectAppForEdit.Text = lang["frmSetting.lblSelectAppForEdit"];
            btnEditEditExt.Text = lang["frmSetting.btnEditEditExt"];
            btnEditResetExt.Text = lang["frmSetting.btnEditResetExt"];
            btnEditEditAllExt.Text = lang["frmSetting.btnEditEditAllExt"];
            clnFileExtension.Text = lang["frmSetting.lvImageEditing.clnFileExtension"];
            clnAppName.Text = lang["frmSetting.lvImageEditing.clnAppName"];
            clnAppPath.Text = lang["frmSetting.lvImageEditing.clnAppPath"];
            clnAppArguments.Text = lang["frmSetting.lvImageEditing.clnAppArguments"];
            #endregion


            #region FILE ASSOCIATION TAB
            var extList = GlobalSetting.DefaultImageFormats.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            lblExtensionsGroupDescription.Text = lang["frmSetting.lblExtensionsGroupDescription"];
            lblSupportedExtension.Text = String.Format(lang["frmSetting.lblSupportedExtension"], extList.Length);
            lnkOpenFileAssoc.Text = lang["frmSetting.lnkOpenFileAssoc"];
            btnAddNewExt.Text = lang["frmSetting.btnAddNewExt"];
            btnDeleteExt.Text = lang["frmSetting.btnDeleteExt"];
            btnRegisterExt.Text = lang["frmSetting.btnRegisterExt"];
            btnResetExt.Text = lang["frmSetting.btnResetExt"];
            lvExtension.Groups[(int)ImageFormatGroup.Default].Header = lang["_.ImageFormatGroup.Default"];
            lvExtension.Groups[(int)ImageFormatGroup.Optional].Header = lang["_.ImageFormatGroup.Optional"];
            #endregion


            #region LANGUAGE TAB
            lblLanguageText.Text = lang["frmSetting.lblLanguageText"];
            lnkRefresh.Text = lang["frmSetting.lnkRefresh"];
            lblLanguageWarning.Text = string.Format(lang["frmSetting.lblLanguageWarning"], "ImageGlass " + Application.ProductVersion);
            lnkInstallLanguage.Text = lang["frmSetting.lnkInstallLanguage"];
            lnkCreateNew.Text = lang["frmSetting.lnkCreateNew"];
            lnkEdit.Text = lang["frmSetting.lnkEdit"];
            lnkGetMoreLanguage.Text = lang["frmSetting.lnkGetMoreLanguage"];
            #endregion


            #region TOOLBAR TAB
            lblToolbarPosition.Text = lang["frmSetting.lblToolbarPosition"];
            chkHorzCenterToolbarBtns.Text = lang["frmSetting.chkHorzCenterToolbarBtns"];

            _separatorText = lang["frmSetting.txtSeparator"];
            lblUsedBtns.Text = lang["frmSetting.lblUsedBtns"];
            lblAvailBtns.Text = lang["frmSetting.lblAvailBtns"];

            tip1.SetToolTip(lblToolbar, lang["frmSetting.lblToolbarTT"]);
            tip1.SetToolTip(btnMoveUp, lang["frmSetting.btnMoveUpTT"]);
            tip1.SetToolTip(btnMoveDown, lang["frmSetting.btnMoveDownTT"]);
            tip1.SetToolTip(btnMoveLeft, lang["frmSetting.btnMoveLeftTT"]);
            tip1.SetToolTip(btnMoveRight, lang["frmSetting.btnMoveRightTT"]);
            #endregion


            #region COLOR PICKER TAB
            lblColorCodeFormat.Text = lang["frmSetting.lblColorCodeFormat"];
            chkColorUseRGBA.Text = lang["frmSetting.chkColorUseRGBA"];
            chkColorUseHEXA.Text = lang["frmSetting.chkColorUseHEXA"];
            chkColorUseHSLA.Text = lang["frmSetting.chkColorUseHSLA"];
            #endregion


            #region THEME TAB
            lblInstalledThemes.Text = string.Format(lang[$"{this.Name}.lblInstalledThemes"], "");
            lnkThemeDownload.Text = lang[$"{this.Name}.lnkThemeDownload"];

            btnThemeRefresh.Text = lang[$"{this.Name}.btnThemeRefresh"];
            btnThemeInstall.Text = lang[$"{this.Name}.btnThemeInstall"];
            btnThemeUninstall.Text = lang[$"{this.Name}.btnThemeUninstall"];
            btnThemeSaveAs.Text = lang[$"{this.Name}.btnThemeSaveAs"];
            btnThemeFolderOpen.Text = lang[$"{this.Name}.btnThemeFolderOpen"];

            btnThemeEdit.Text = lang[$"{this.Name}.btnThemeEdit._Edit"];
            btnThemeApply.Text = lang[$"{this.Name}.btnThemeApply"];

            #endregion


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
            switch (lbl.Name)
            {
                case "lblGeneral":
                    tab1.SelectedTab = tabGeneral;
                    break;
                case "lblImage":
                    tab1.SelectedTab = tabImage;
                    break;
                case "lblFileAssociations":
                    tab1.SelectedTab = tabFileAssociation;
                    break;
                case "lblLanguage":
                    tab1.SelectedTab = tabLanguage;
                    break;
                case "lblToolbar":
                    tab1.SelectedTab = tabToolbar;
                    break;
                case "lblColorPicker":
                    tab1.SelectedTab = tabColorPicker;
                    break;
                case "lblTheme":
                    tab1.SelectedTab = tabTheme;
                    break;
            }
        }


        private void tab1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblGeneral.Tag =
            lblImage.Tag =
            lblFileAssociations.Tag =
            lblLanguage.Tag =
            lblToolbar.Tag =
            lblColorPicker.Tag =
            lblTheme.Tag = 0;

            lblGeneral.BackColor =
            lblImage.BackColor =
            lblFileAssociations.BackColor =
            lblLanguage.BackColor =
            lblToolbar.BackColor =
            lblColorPicker.BackColor =
            lblTheme.BackColor = M_COLOR_MENU_NORMAL;

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

                RenderTheme r = new RenderTheme();
                r.ApplyTheme(lvImageEditing);
            }
            else if (tab1.SelectedTab == tabFileAssociation)
            {
                lblFileAssociations.Tag = 1;
                lblFileAssociations.BackColor = M_COLOR_MENU_ACTIVE;

                // Load image formats to the list
                LoadExtensionList();

                lvExtension.TileSize = new Size(100, DPIScaling.TransformNumber(30));

                RenderTheme r = new RenderTheme();
                r.ApplyTheme(lvExtension);
            }
            else if (tab1.SelectedTab == tabLanguage)
            {
                lblLanguage.Tag = 1;
                lblLanguage.BackColor = M_COLOR_MENU_ACTIVE;

                lnkRefresh_LinkClicked(null, null);
            }
            else if (tab1.SelectedTab == tabToolbar)
            {
                lblToolbar.Tag = 1;
                lblToolbar.BackColor = M_COLOR_MENU_ACTIVE;

                LoadTabToolbar();
            }
            else if (tab1.SelectedTab == tabColorPicker)
            {
                lblColorPicker.Tag = 1;
                lblColorPicker.BackColor = M_COLOR_MENU_ACTIVE;

                LoadTabColorPicker();
            }
            else if (tab1.SelectedTab == tabTheme)
            {
                lblTheme.Tag = 1;
                lblTheme.BackColor = M_COLOR_MENU_ACTIVE;

                LoadTabTheme();
                
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

            //Get value of IsScrollbarsVisible
            chkShowScrollbar.Checked = GlobalSetting.IsScrollbarsVisible;

            //Get value of IsDisplayBasenameOfImage
            chkDisplayBasename.Checked = GlobalSetting.IsDisplayBasenameOfImage;

            //Get background color
            picBackgroundColor.BackColor = GlobalSetting.BackgroundColor;
        }


        private void lnkConfigDir_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", GlobalSetting.ConfigDir);
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
            }
        }

        private void lnkResetBackgroundColor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            picBackgroundColor.BackColor = LocalSetting.Theme.BackgroundColor;
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

            //Get value of chkShowHiddenImages
            chkShowHiddenImages.Checked = GlobalSetting.IsShowingHiddenImages;

            //Get value of chkLoopViewer
            chkLoopViewer.Checked = GlobalSetting.IsLoopBackViewer;

            //Get value of chkImageBoosterBack
            chkImageBoosterBack.Checked = GlobalSetting.IsImageBoosterBack;

            #region Load items of cmbImageOrder
            var loadingOrderList = Enum.GetNames(typeof(ImageOrderBy));
            cmbImageOrder.Items.Clear();
            foreach (var item in loadingOrderList)
            {
                cmbImageOrder.Items.Add(GlobalSetting.LangPack.Items[$"{this.Name}.cmbImageOrder._{item}"]);
            }

            //Get value of cmbImageOrder
            cmbImageOrder.SelectedIndex = (int)GlobalSetting.ImageLoadingOrder;
            #endregion


            #region Get mouse wheel actions

            //mouse wheel actions (with no control keys pressed)
            cmbMouseWheel.Items.Clear();

            //mouse wheel actions with <Ctrl> key pressed
            cmbMouseWheelCtrl.Items.Clear();

            //mouse wheel actions with <Shift> key pressed
            cmbMouseWheelShift.Items.Clear();

            //mouse wheel actions with <Alt> key pressed
            cmbMouseWheelAlt.Items.Clear();

            var mouseWheelActionsList = Enum.GetNames(typeof(MouseWheelActions));
            foreach (var item in mouseWheelActionsList)
            {
                cmbMouseWheel.Items.Add(GlobalSetting.LangPack.Items[$"{this.Name}.cmbMouseWheel._{item}"]);
                cmbMouseWheelCtrl.Items.Add(GlobalSetting.LangPack.Items[$"{this.Name}.cmbMouseWheel._{item}"]);
                cmbMouseWheelShift.Items.Add(GlobalSetting.LangPack.Items[$"{this.Name}.cmbMouseWheel._{item}"]);
                cmbMouseWheelAlt.Items.Add(GlobalSetting.LangPack.Items[$"{this.Name}.cmbMouseWheel._{item}"]);
            }

            //Get value of cmbMouseWheel
            cmbMouseWheel.SelectedIndex = (int)GlobalSetting.MouseWheelAction;

            //Get value of cmbMouseWheelCtrl
            cmbMouseWheelCtrl.SelectedIndex = (int)GlobalSetting.MouseWheelCtrlAction;

            //Get value of cmbMouseWheelShift
            cmbMouseWheelShift.SelectedIndex = (int)GlobalSetting.MouseWheelShiftAction;

            //Get value of cmbMouseWheelAlt
            cmbMouseWheelAlt.SelectedIndex = (int)GlobalSetting.MouseWheelAltAction;

            #endregion


            #region Load items of cmbZoomOptimization

            var zoomOptimizationList = Enum.GetNames(typeof(ZoomOptimizationMethods));
            cmbZoomOptimization.Items.Clear();
            foreach (var item in zoomOptimizationList)
            {
                cmbZoomOptimization.Items.Add(GlobalSetting.LangPack.Items[$"{this.Name}.cmbZoomOptimization._{item}"]);
            }

            //Get value of cmbZoomOptimization
            cmbZoomOptimization.SelectedIndex = (int)GlobalSetting.ZoomOptimizationMethod;

            #endregion


            //Thumbnail bar on right side ----------------------------------------------------
            chkThumbnailVertical.Checked = !GlobalSetting.IsThumbnailHorizontal;

            //Show thumbnail scrollbar
            chkShowThumbnailScrollbar.Checked = GlobalSetting.IsShowThumbnailScrollbar;

            //load thumbnail dimension
            cmbThumbnailDimension.SelectedItem = GlobalSetting.ThumbnailDimension.ToString();

            //Get value of chkLoopSlideshow --------------------------------------------------
            chkLoopSlideshow.Checked = GlobalSetting.IsLoopBackSlideShow;

            //Get value of barInterval
            barInterval.Value = GlobalSetting.SlideShowInterval;
            lblSlideshowInterval.Text = string.Format(GlobalSetting.LangPack.Items["frmSetting.lblSlideshowInterval"], barInterval.Value);

            //Get value of IsSaveAfterRotating
            chkSaveOnRotate.Checked = GlobalSetting.IsSaveAfterRotating;
            chkSaveModifyDate.Checked = GlobalSetting.PreserveModifiedDate;

            //Load Image Editing extension list
            LoadImageEditingAssociationList();


        }


        private void barInterval_Scroll(object sender, EventArgs e)
        {
            lblSlideshowInterval.Text = string.Format(GlobalSetting.LangPack.Items["frmSetting.lblSlideshowInterval"], barInterval.Value);
        }


        /// <summary>
        /// Load ImageEditingAssociation list
        /// </summary>
        /// <param name="isResetToDefault">True to reset the list to default (empty)</param>
        private void LoadImageEditingAssociationList(bool @isResetToDefault = false)
        {
            lvImageEditing.Items.Clear();
            var newEditingAssocList = new List<ImageEditingAssociation>();

            // Load Default group
            var extList = GlobalSetting.AllImageFormats.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var ext in extList)
            {
                var li = new ListViewItem();
                li.Text = ext;

                //Build new list
                var newEditingAssoc = new ImageEditingAssociation()
                {
                    Extension = ext
                };

                if (!isResetToDefault)
                {
                    //Find the extension in the settings
                    var editingExt = GlobalSetting.ImageEditingAssociationList.FirstOrDefault(item => item?.Extension == ext);

                    li.SubItems.Add(editingExt?.AppName);
                    li.SubItems.Add(editingExt?.AppPath);
                    li.SubItems.Add(editingExt?.AppArguments);

                    //Build new list
                    newEditingAssoc.AppName = editingExt?.AppName;
                    newEditingAssoc.AppPath = editingExt?.AppPath;
                    newEditingAssoc.AppArguments = editingExt?.AppArguments;
                }

                newEditingAssocList.Add(newEditingAssoc);
                lvImageEditing.Items.Add(li);
            }

            //Update the new full list
            GlobalSetting.ImageEditingAssociationList = newEditingAssocList;
        }

        private void btnEditResetExt_Click(object sender, EventArgs e)
        {
            LoadImageEditingAssociationList(true);
            GlobalSetting.SaveConfigOfImageEditingAssociationList();
        }

        private void btnEditEditExt_Click(object sender, EventArgs e)
        {
            if (lvImageEditing.SelectedItems.Count == 0)
                return;

            //Get select Association item
            var assoc = GlobalSetting.GetImageEditingAssociationFromList(lvImageEditing.SelectedItems[0].Text);

            if (assoc == null)
                return;

            frmEditEditingAssocisation f = new frmEditEditingAssocisation()
            {
                FileExtension = assoc.Extension,
                AppName = assoc.AppName,
                AppPath = assoc.AppPath,
                AppArguments = assoc.AppArguments,
                TopMost = this.TopMost
            };

            if (f.ShowDialog() == DialogResult.OK)
            {
                assoc.AppName = f.AppName;
                assoc.AppPath = f.AppPath;
                assoc.AppArguments = f.AppArguments;

                LoadImageEditingAssociationList();
            }

            f.Dispose();

        }

        private void btnEditEditAllExt_Click(object sender, EventArgs e)
        {
            frmEditEditingAssocisation f = new frmEditEditingAssocisation()
            {
                FileExtension = $"<{string.Format(GlobalSetting.LangPack.Items["frmSetting._allExtensions"])}>",
                TopMost = this.TopMost
            };

            if (f.ShowDialog() == DialogResult.OK)
            {
                foreach (var assoc in GlobalSetting.ImageEditingAssociationList)
                {
                    assoc.AppName = f.AppName;
                    assoc.AppPath = f.AppPath;
                    assoc.AppArguments = f.AppArguments;
                }

                LoadImageEditingAssociationList();
            }

            f.Dispose();
        }

        private void lvlvImageEditing_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEditEditExt.Enabled = (lvImageEditing.SelectedItems.Count > 0);
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

            try
            {
                p.Start();
            }
            catch { }

        }

        private void lnkCreateNew_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = p.StartInfo.FileName = Path.Combine(GlobalSetting.StartUpDir, "igtasks.exe");
            p.StartInfo.Arguments = "ignewlang";

            try
            {
                p.Start();
            }
            catch { }
        }

        private void lnkEdit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = p.StartInfo.FileName = Path.Combine(GlobalSetting.StartUpDir, "igtasks.exe");
            p.StartInfo.Arguments = "igeditlang \"" + GlobalSetting.LangPack.FileName + "\"";

            try
            {
                p.Start();
            }
            catch { }
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

            //check compatibility
            var lang = new Language();
            if (lang.MinVersion.CompareTo(dsLanguages[cmbLanguage.SelectedIndex].MinVersion) != 0)
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

            // build the hashset GlobalSetting.ImageFormatHashSet
            GlobalSetting.BuildImageFormatHashSet();
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

            try
            {
                p.Start();
            }
            catch { }
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
            if (lvExtension.SelectedItems.Count == 0)
                return;

            var selectedDefaultExts = new StringBuilder();
            var selectedOptionalExts = new StringBuilder();

            // Get checked extensions in the list then
            // remove extensions from settings
            foreach (ListViewItem li in lvExtension.SelectedItems)
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

            //RegisterFileAssociations(GlobalSetting.AllImageFormats);
            LoadExtensionList();
        }

        private void btnAddNewExt_Click(object sender, EventArgs e)
        {
            frmAddNewFormat f = new frmAddNewFormat()
            {
                FileFormat = ".svg",
                FormatGroup = ImageFormatGroup.Default,
                TopMost = this.TopMost
            };

            if (f.ShowDialog() == DialogResult.OK)
            {
                // If the ext exist
                if (GlobalSetting.AllImageFormats.Contains(f.FileFormat))
                    return;

                if (f.FormatGroup == ImageFormatGroup.Default)
                {
                    GlobalSetting.DefaultImageFormats += f.FileFormat;
                }
                else if (f.FormatGroup == ImageFormatGroup.Optional)
                {
                    GlobalSetting.OptionalImageFormats += f.FileFormat;
                }

                //RegisterFileAssociations(GlobalSetting.AllImageFormats);
                LoadExtensionList();
            }

            f.Dispose();
        }

        private void btnRegisterExt_Click(object sender, EventArgs e)
        {
            RegisterFileAssociations(GlobalSetting.AllImageFormats);
        }

        private void lvExtension_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnDeleteExt.Enabled = (lvExtension.SelectedItems.Count > 0);
        }







        #endregion


        #region TAB TOOLBAR
        /*
        How to add a new toolbar button:
        1. A SVG image in the theme is necessary.
        2. A tooltip string in the language set is necessary.
        3. Add a ToolStripButton field to frmMain. Note the field name (e.g. "btnRename").
           The button does NOT need to be added to the toolstrip, or created by the designer.
           It can be created and initialized in code, either by, or before, UpdateToolbarButtons
           is invoked. The image, tooltip and Click event must all be specified!
        4. Add a new enum to ToolbarButtons below, with the same name as the field name assigned in
           the step above (e.g. "btnRename"). The new enum goes BEFORE the MAX entry.

        The new toolbar button will now be available, the user would use the toolbar config
        tab to add it to their toolbar settings.
        */

        private void LoadTabToolbar()
        {
            var lang = GlobalSetting.LangPack.Items;

            // Load toolbar position
            cmbToolbarPosition.Items.Clear();
            var toolbarPositions = Enum.GetNames(typeof(ToolbarPosition));
            foreach (var pos in toolbarPositions)
            {
                cmbToolbarPosition.Items.Add(lang[$"{this.Name}.cmbToolbarPosition._{pos}"]);
            }
            
            cmbToolbarPosition.SelectedIndex = (int)GlobalSetting.ToolbarPosition;

            chkHorzCenterToolbarBtns.Checked = GlobalSetting.IsCenterToolbar;

            // Apply Windows System theme to listview
            RenderTheme th = new RenderTheme();
            th.ApplyTheme(lvAvailButtons);
            th.ApplyTheme(lvUsedButtons);

            // Apply ImageGlass theme to buttons list
            lvAvailButtons.BackColor = lvUsedButtons.BackColor = LocalSetting.Theme.ToolbarBackgroundColor;
            lvAvailButtons.ForeColor = lvUsedButtons.ForeColor = LocalSetting.Theme.TextInfoColor;


            BuildToolbarImageList();
            InitUsedList();
            InitAvailList();
            UpdateNavigationButtonsState();
        }


        /// <summary>
        /// Fetch all the toolbar images via reflection from the ToolStripButton
        /// instances in the frmMain instance. This is why the enum name MUST
        /// match the frmMain field name!
        /// </summary>
        private void BuildToolbarImageList()
        {
            if (_lstToolbarImg != null)
                return;

            _lstToolbarImg = new ImageList();
            _lstToolbarImg.ColorDepth = ColorDepth.Depth32Bit; // max out image quality

            var iconHeight = ThemeImage.GetCorrectIconHeight();
            _lstToolbarImg.ImageSize = new Size(iconHeight, iconHeight); // TODO empirically determined (can get from ImageGlass.Theme)

            Type mainType = typeof(frmMain);
            for (int i = 0; i < (int)ToolbarButtons.MAX; i++)
            {
                var fieldName = ((ToolbarButtons)i).ToString();

                try
                {
                    var info = mainType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                    ToolStripButton val = info.GetValue(MainInstance) as ToolStripButton;
                    _lstToolbarImg.Images.Add(val.Image);
                }
                catch (Exception)
                {
                    // GetField may fail if someone renames a toolbar button w/o updating the customize toolbar logic
                }
            }
        }


        /// <summary>
        /// Build the list of "currently used" toolbar buttons
        /// </summary>
        private void InitUsedList()
        {
            lvUsedButtons.View = View.Tile;
            lvUsedButtons.LargeImageList = _lstToolbarImg;

            lvUsedButtons.Items.Clear();

            string currentSet = GlobalSetting.ToolbarButtons;
            var enumList = TranslateToolbarButtonsFromConfig(currentSet);

            _lstMasterUsed = new List<ListViewItem>(enumList.Count);
            for (int i = 0; i < enumList.Count; i++)
            {
                ListViewItem lvi;

                if (enumList[i] == ToolbarButtons.Separator)
                {
                    lvi = BuildSeparatorItem();
                }
                else
                {
                    lvi = BuildToolbarListItem(enumList[i]);
                }

                _lstMasterUsed.Add(lvi);
            }

            lvUsedButtons.Items.AddRange(_lstMasterUsed.ToArray());
        }


        /// <summary>
        /// Build the list of "not currently used" toolbar buttons
        /// </summary>
        private void InitAvailList()
        {
            lvAvailButtons.View = View.Tile;
            lvAvailButtons.LargeImageList = _lstToolbarImg;

            lvAvailButtons.Items.Clear();

            // Build by adding each button NOT in the 'used' list
            string currentSet = GlobalSetting.ToolbarButtons;
            var enumList = TranslateToolbarButtonsFromConfig(currentSet);
            for (int i = 0; i < (int)ToolbarButtons.MAX; i++)
            {
                if (!enumList.Contains((ToolbarButtons)i))
                {
                    lvAvailButtons.Items.Add(BuildToolbarListItem((ToolbarButtons)i));
                }
            }

            // separator is always available
            lvAvailButtons.Items.Add(BuildSeparatorItem());
        }


        /// <summary>
        /// Fetch the toolbar string via reflection from the ToolStripButton
        /// instance in the frmMain instance. This is why the enum name MUST
        /// match the frmMain field name!
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns></returns>
        private ListViewItem BuildToolbarListItem(ToolbarButtons buttonType)
        {
            ListViewItem lvi = new ListViewItem
            {
                ImageIndex = (int)buttonType,
                Tag = buttonType
            };


            var fieldName = buttonType.ToString();
            Type mainType = typeof(frmMain);

            try
            {
                var info = mainType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                ToolStripButton val = info.GetValue(MainInstance) as ToolStripButton;
                lvi.Text = lvi.ToolTipText = val.ToolTipText;
            }
            catch (Exception)
            {
                // GetField may fail if someone renames a toolbar button w/o updating the customize toolbar logic
                return null;
            }

            return lvi;
        }


        /// <summary>
        /// Build Separator for Toolbar listview
        /// </summary>
        /// <returns></returns>
        private ListViewItem BuildSeparatorItem()
        {
            var lvi = new ListViewItem();
            lvi.Text = _separatorText;
            lvi.ToolTipText = _separatorText;
            lvi.Tag = ToolbarButtons.Separator;
            return lvi;
        }


        /// <summary>
        /// The toolbar config string is stored as a comma-separated list of int values for convenience.
        /// Here, we translate that string to a list of button enums.
        /// </summary>
        /// <param name="configVal"></param>
        /// <returns></returns>
        private static List<ToolbarButtons> TranslateToolbarButtonsFromConfig(string configVal)
        {
            List<ToolbarButtons> outVal = new List<ToolbarButtons>();
            string[] splitvals = configVal.Split(new[] { ',' });

            foreach (var splitval in splitvals)
            {
                if (int.TryParse(splitval, out int numVal))
                {
                    try
                    {
                        ToolbarButtons enumVal = (ToolbarButtons)numVal;
                        outVal.Add(enumVal);
                    }
                    catch (Exception)
                    {
                        // when the enumeration value doesn't exist, don't add it!
                    }
                }
            }
            return outVal;
        }


        /// <summary>
        /// Update Navagation buttons of toolbar buttons list's state
        /// </summary>
        private void UpdateNavigationButtonsState()
        {
            // 'Move right' active for at least one selected item in left list.
            // 'Move left' active for at least one selected item in left list.
            // 'Move up/down' active for ONLY one selected item in right list.

            btnMoveRight.Enabled = lvAvailButtons.SelectedItems.Count > 0;
            btnMoveLeft.Enabled = lvUsedButtons.SelectedItems.Count > 0;
            btnMoveUp.Enabled = lvUsedButtons.SelectedItems.Count == 1;
            btnMoveDown.Enabled = lvUsedButtons.SelectedItems.Count == 1;
        }


        /// <summary>
        /// Apply all button changes in Toolbar
        /// </summary>
        private void ApplyToolbarChanges()
        {
            // User hasn't actually visited the toolbar tab, don't do anything!
            // (Discovered by clicking 'Save' w/o having visited the toolbar tab...
            if (lvUsedButtons.Items.Count == 0 && lvAvailButtons.Items.Count == 0)
                return;

            // Save the current set of 'used' buttons to the comma-separated list of integers.
            StringBuilder sb = new StringBuilder();
            bool first = true;

            foreach (ListViewItem item in lvUsedButtons.Items)
            {
                string val = ((int)item.Tag).ToString();

                if (!first)
                    sb.Append(",");

                first = false;
                sb.Append(val);
            }

            //Only make change if any
            if (GlobalSetting.ToolbarButtons.ToLower().CompareTo(sb.ToString().ToLower()) != 0)
            {
                GlobalSetting.ToolbarButtons = sb.ToString();
                GlobalSetting.SetConfig("ToolbarButtons", GlobalSetting.ToolbarButtons);

                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.TOOLBAR;
            }
        }


        #region Events
        private void ButtonsListView_Resize(object sender, EventArgs e)
        {
            var lv = (ListView)sender;
            UpdateButtonsListViewItemSize(lv);
        }


        /// <summary>
        /// Make the list view item bigger, adapted to icon size
        /// </summary>
        /// <param name="lv"></param>
        private void UpdateButtonsListViewItemSize(ListView lv)
        {
            var width = (int)(lv.Width * 0.85); // reserve right gap for multiple selection
            var height = ThemeImage.GetCorrectIconHeight() * 2;

            lv.TileSize = new Size(width, height);

            // TODO: Issue:
            // The Listview layout is broken when user shrinks the window
            // then click Maximize button
        }


        private void lvUsedButtons_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateNavigationButtonsState();
        }

        private void lvAvailButtons_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateNavigationButtonsState();
        }

        private void btnMoveRight_Click(object sender, EventArgs e)
        {
            // 'Move' the selected entry in the LEFT list to the bottom of the RIGHT list
            // An exception is 'separator' which always remains available in the left list.

            for (int i = 0; i < lvAvailButtons.SelectedItems.Count; i++)
            {
                var lvi = lvAvailButtons.SelectedItems[i];
                _lstMasterUsed.Add(lvi.Clone() as ListViewItem);
            }

            for (int i = lvAvailButtons.SelectedItems.Count - 1; i >= 0; i--)
            {
                var lvi = lvAvailButtons.SelectedItems[i];
                if ((ToolbarButtons)lvi.Tag != ToolbarButtons.Separator)
                    lvAvailButtons.Items.Remove(lvi);
            }

            lvAvailButtons.SelectedIndices.Clear();
            RebuildUsedButtonsList(_lstMasterUsed.Count - 1);
            lvUsedButtons.EnsureVisible(_lstMasterUsed.Count - 1);
        }

        private void btnMoveLeft_Click(object sender, EventArgs e)
        {
            // 'Move' the selected entry in the RIGHT list to the bottom of the LEFT list
            // An exception is 'separator' which always remains available in the left list.

            for (int i = 0; i < lvUsedButtons.SelectedItems.Count; i++)
            {
                var lvi = lvUsedButtons.SelectedItems[i];
                if ((ToolbarButtons)lvi.Tag != ToolbarButtons.Separator)
                    lvAvailButtons.Items.Add(lvi.Clone() as ListViewItem);

                _lstMasterUsed.Remove(lvi);
            }

            RebuildUsedButtonsList(-1);
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            MoveUsedEntry(+1);
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            MoveUsedEntry(-1);
        }

        /// <summary>
        /// Move an item in the 'used' list
        /// </summary>
        /// <param name="delta"></param>
        private void MoveUsedEntry(int delta)
        {
            var currentIndex = lvUsedButtons.SelectedItems[0].Index;

            // do not wrap around
            if (delta < 0)
            {
                if (currentIndex <= 0)
                    return;
            }
            else
            {
                if (currentIndex >= _lstMasterUsed.Count - 1)
                    return;
            }

            var item = lvUsedButtons.Items[currentIndex];

            _lstMasterUsed.RemoveAt(currentIndex);
            _lstMasterUsed.Insert(currentIndex + delta, item);
            RebuildUsedButtonsList(currentIndex + delta);

            // Make sure the new position, plus some context, is visible after rebuild
            lvUsedButtons.EnsureVisible(Math.Min(currentIndex + 2, _lstMasterUsed.Count - 1));
        }

        private void RebuildUsedButtonsList(int toSelect)
        {
            // This is annoying. To show the desired appearance in the listview, we need
            // to use 'SmallIcons' mode [image + text on a single line]. 'SmallIcons' mode
            // will NOT repaint the listview after changes to the Items list!!! Thus, we
            // teardown and rebuild the listview here.

            lvUsedButtons.BeginUpdate();
            lvUsedButtons.SelectedIndices.Clear();
            lvUsedButtons.Items.Clear();
            lvUsedButtons.Items.AddRange(_lstMasterUsed.ToArray());

            if (toSelect >= 0)
            {
                lvUsedButtons.Items[toSelect].Selected = true;
            }

            lvUsedButtons.EndUpdate();
        }

        #endregion


        #region PUBLIC METHODS used in [frmMain]
        /// <summary>
        /// This method is used by the main form to actually initialize the toolbar. 
        /// The toolbar buttons setting is translated to a list of field NAMES from 
        /// the frmMain class. The need of a separator is indicated by the magic string "_sep_".
        /// </summary>
        /// <returns></returns>
        public static List<string> LoadToolbarConfig()
        {
            var xlated = TranslateToolbarButtonsFromConfig(GlobalSetting.ToolbarButtons);
            List<string> lstToolbarButtonNames = new List<string>();

            foreach (var btnEnum in xlated)
            {
                switch (btnEnum)
                {
                    case ToolbarButtons.Separator:
                        lstToolbarButtonNames.Add("_sep_");
                        break;

                    default:
                        // enum name *must* match the field name in frmMain AND the resource name, e.g. "frmMain.btnBack"
                        lstToolbarButtonNames.Add(btnEnum.ToString());
                        break;
                }
            }
            return lstToolbarButtonNames;
        }


        /// <summary>
        /// Load toolbar configs and update the buttons
        /// </summary>
        /// <param name="toolMain"></param>
        /// <param name="form"></param>
        public static void UpdateToolbarButtons(ToolStrip toolMain, frmMain form)
        {
            toolMain.Items.Clear();

            List<string> lstToolbarButtons = LoadToolbarConfig();
            Type frmMainType = typeof(frmMain);

            foreach (var btn in lstToolbarButtons)
            {
                if (btn == "_sep_")
                {
                    var hIcon = ThemeImage.GetCorrectIconHeight();

                    ToolStripSeparator sep = new ToolStripSeparator
                    {
                        AutoSize = false,
                        Margin = new Padding((int)(hIcon * 0.15), 0, (int)(hIcon * 0.15), 0),
                        Height = (int)(hIcon * 1.2)
                    };

                    toolMain.Items.Add(sep);
                }
                else
                {
                    try
                    {
                        var info = frmMainType.GetField(btn, BindingFlags.Instance | BindingFlags.NonPublic);
                        var val = info.GetValue(form);

                        toolMain.Items.Add(val as ToolStripItem);
                    }
                    catch (Exception)
                    {
                        // GetField may fail if someone renames a toolbar button w/o updating the customize toolbar logic
                    }
                }
            }
        }


#if false

        // The following code is disabled as it was an early attempt to provide toolbar buttons for
        // rename, recycle, and edit. The menu images for those menu entries have been removed,
        // so this code cannot be used until SVG images are provided in the theme for those functions.

        private void MakeMenuButtons()
        {
            // These buttons were not part of the initial toolbar button set. Set up and initialize
            // as if they were created via the designer.

            ComponentResourceManager resources = new ComponentResourceManager(typeof(frmMain));

            string txt = GlobalSetting.LangPack.Items["frmMain.mnuMainMoveToRecycleBin"];
            btnRecycleBin = new ToolStripButton();
            MakeMenuButton(btnRecycleBin, "btnRecycleBin", txt);
            btnRecycleBin.Image = ((Image)(resources.GetObject("mnuMainMoveToRecycleBin.Image")));
            btnRecycleBin.Click += mnuMainMoveToRecycleBin_Click;

            txt = GlobalSetting.LangPack.Items["frmMain.mnuMainRename"];
            btnRename = new ToolStripButton();
            MakeMenuButton(btnRename, "btnRename", txt);
            btnRename.Image = ((Image)(resources.GetObject("mnuMainRename.Image")));
            btnRename.Click += mnuMainRename_Click;

            txt = GlobalSetting.LangPack.Items["frmMain.mnuMainEditImage"];
            btnEditImage = new ToolStripButton();
            MakeMenuButton(btnEditImage, "btnEditImage", txt);
            btnEditImage.Image = ((Image)(resources.GetObject("mnuMainEditImage.Image")));
            btnEditImage.Click += mnuMainEditImage_Click;
        }

        // NOTE: the field names here _must_ match the names in frmCustToolbar.cs/enum allBtns
        private ToolStripButton btnRecycleBin;
        private ToolStripButton btnRename;
        private ToolStripButton btnEditImage;

        private void MakeMenuButton(ToolStripButton btn, string name, string ttText)
        {
            btn.AutoSize = false;
            btn.BackColor = Color.Transparent;
            btn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btn.ImageScaling = ToolStripItemImageScaling.None;
            btn.ImageTransparentColor = Color.Magenta;
            btn.Margin = new Padding(3, 0, 0, 0);
            btn.Name = name;
            btn.Size = new Size(28, 28);
            btn.ToolTipText = ttText;
        }

#endif


        #endregion

        #endregion


        #region TAB COLOR PICKER
        private void LoadTabColorPicker()
        {
            chkColorUseRGBA.Checked = GlobalSetting.IsColorPickerRGBA;
            chkColorUseHEXA.Checked = GlobalSetting.IsColorPickerHEXA;
            chkColorUseHSLA.Checked = GlobalSetting.IsColorPickerHSLA;
        }


        #endregion


        #region TAB THEME
        private void LoadTabTheme()
        {
            if (lvTheme.Items.Count == 0)
            {
                RenderTheme r = new RenderTheme();
                r.ApplyTheme(lvTheme);

                RefreshThemeList();
            }
        }

        private void RefreshThemeList()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ImageGlass\Themes\");

            lvTheme.Items.Clear();
            lvTheme.Items.Add("2017 (Dark)").Tag = "Default";

            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetDirectories(dir))
                {
                    string configFile = Path.Combine(d, "config.xml");

                    if (File.Exists(configFile))
                    {
                        Theme.Theme th = new Theme.Theme();

                        //invalid theme
                        if (!th.LoadTheme(configFile))
                        {
                            continue;
                        }

                        var lvi = new ListViewItem(th.Name);
                        lvi.Tag = configFile;
                        lvi.ImageKey = "_blank";

                        if (LocalSetting.Theme.ThemeConfigFilePath == configFile)
                        {
                            lvi.Selected = true;
                            lvi.Checked = true;
                        }

                        lvTheme.Items.Add(lvi);
                    }
                }

                //select the default theme
                if (lvTheme.Items.Count > 0 && lvTheme.SelectedItems.Count == 0)
                {
                    lvTheme.Items[0].Selected = true;
                }
            }
            else
            {
                Directory.CreateDirectory(dir);
            }


            lblInstalledThemes.Text = string.Format(GlobalSetting.LangPack.Items[$"{this.Name}.lblInstalledThemes"], lvTheme.Items.Count.ToString());
        }


        private void btnThemeRefresh_Click(object sender, EventArgs e)
        {
            RefreshThemeList();
        }


        private void lvTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lang = GlobalSetting.LangPack.Items;

            if (lvTheme.SelectedIndices.Count > 0)
            {
                string file = lvTheme.SelectedItems[0].Tag.ToString();
                btnThemeSaveAs.Enabled = true;
                btnThemeUninstall.Enabled = true;

                if (file == "Default")
                {
                    file = Path.Combine(GlobalSetting.StartUpDir, @"DefaultTheme\config.xml");
                    //btnThemeSaveAs.Enabled = false;
                    btnThemeUninstall.Enabled = false;
                }


                string dir = Path.GetDirectoryName(file) + "\\";

                Theme.Theme t = new Theme.Theme(file);
                picPreview.BackgroundImage = t.PreviewImage.Image;

                txtThemeInfo.Text = 
                    $"{lang[$"{this.Name}.txtThemeInfo._Name"]}: {t.Name}\r\n" + 
                    $"{lang[$"{this.Name}.txtThemeInfo._Version"]}: {t.Version}\r\n" + 
                    $"{lang[$"{this.Name}.txtThemeInfo._Author"]}: {t.Author}\r\n" + 
                    $"{lang[$"{this.Name}.txtThemeInfo._Email"]}: {t.Email}\r\n" + 
                    $"{lang[$"{this.Name}.txtThemeInfo._Website"]}: {t.Website}\r\n" + 
                    $"{lang[$"{this.Name}.txtThemeInfo._Compatibility"]}: {t.Compatibility}\r\n" + 
                    $"{lang[$"{this.Name}.txtThemeInfo._Description"]}: {t.Description}";
                    
                txtThemeInfo.Visible = true;

                btnThemeEdit.Text = lang[$"{this.Name}.btnThemeEdit._Edit"];
            }
            else
            {
                picPreview.Image = null;
                txtThemeInfo.Visible = false;
                txtThemeInfo.Text = "";
                btnThemeSaveAs.Enabled = false;
                btnThemeUninstall.Enabled = false;
                btnThemeEdit.Text = lang[$"{this.Name}.btnThemeEdit._New"];
            }
        }


        private void btnThemeInstall_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "ImageGlass theme (*.igtheme)|*.igtheme|All files (*.*)|*.*";

            if (o.ShowDialog() == DialogResult.OK && File.Exists(o.FileName))
            {
                var result = Theme.Theme.InstallTheme(o.FileName);

                if (result == ThemeInstallingResult.SUCCESS)
                {
                    RefreshThemeList();

                    MessageBox.Show(GlobalSetting.LangPack.Items["frmSetting.btnThemeInstall._Success"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(GlobalSetting.LangPack.Items["frmSetting.btnThemeInstall._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void btnThemeUninstall_Click(object sender, EventArgs e)
        {
            if (lvTheme.SelectedItems.Count > 0)
            {
                var result = Theme.Theme.UninstallTheme(lvTheme.SelectedItems[0].Tag.ToString());

                if (result == ThemeUninstallingResult.SUCCESS)
                {
                    RefreshThemeList();
                }
                else if (result == ThemeUninstallingResult.ERROR)
                {
                    MessageBox.Show(GlobalSetting.LangPack.Items["frmSetting.btnThemeUninstall._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }


        private void lnkThemeDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string version = GlobalSetting.GetConfig("AppVersion", "1.0").Replace(".", "_");
                Process.Start("http://www.imageglass.org/download/themes?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_download_theme");
            }
            catch { }
        }


        private void btnThemeSaveAs_Click(object sender, EventArgs e)
        {
            if (lvTheme.SelectedItems.Count > 0)
            {
                SaveFileDialog s = new SaveFileDialog();
                s.Filter = "ImageGlass theme (*.igtheme)|*.igtheme";

                if (s.ShowDialog() == DialogResult.OK)
                {
                    var themeConfig = lvTheme.SelectedItems[0].Tag.ToString();
                    if (!File.Exists(themeConfig))
                    {
                        themeConfig = Path.Combine(GlobalSetting.StartUpDir, @"DefaultTheme\config.xml");
                    }

                    var themeDir = Path.GetDirectoryName(themeConfig);
                    var result = Theme.Theme.PackTheme(themeDir, s.FileName);

                    if (result == ThemePackingResult.SUCCESS)
                    {
                        MessageBox.Show(string.Format(GlobalSetting.LangPack.Items["frmSetting.btnThemeSaveAs._Success"], s.FileName), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(GlobalSetting.LangPack.Items["frmSetting.btnThemeSaveAs._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void btnThemeFolderOpen_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ImageGlass\Themes\"));
        }
        

        private void btnThemeApply_Click(object sender, EventArgs e)
        {
            if (lvTheme.SelectedItems.Count > 0)
            {
                var th = Theme.Theme.ApplyTheme(lvTheme.SelectedItems[0].Tag.ToString());

                if (th != null)
                {
                    LocalSetting.Theme = th;
                    GlobalSetting.BackgroundColor = picBackgroundColor.BackColor = LocalSetting.Theme.BackgroundColor;

                    LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.THEME;

                    th = null;

                    MessageBox.Show(GlobalSetting.LangPack.Items["frmSetting.btnThemeApply._Success"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(GlobalSetting.LangPack.Items["frmSetting.btnThemeApply._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }



        #endregion


        #region ACTION BUTTONS
        private void btnCancel_Click(object sender, EventArgs e)
        {
            //close without saving
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Save and close
            btnApply_Click(sender, null);
            this.Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            //Variables for comparision
            int newInt;
            bool newBool;
            string newString;
            Color newColor;

            
            #region General tab --------------------------------------------
            // IsShowWelcome
            GlobalSetting.IsShowWelcome = chkWelcomePicture.Checked;
            GlobalSetting.SetConfig("IsShowWelcome", GlobalSetting.IsShowWelcome.ToString());

            //IsShowToolBar
            GlobalSetting.IsShowToolBar = chkShowToolBar.Checked;
            GlobalSetting.SetConfig("IsShowToolbar", GlobalSetting.IsShowToolBar.ToString());


            //AutoUpdate
            if (chkAutoUpdate.Checked)
            {
                GlobalSetting.SetConfig("AutoUpdate", DateTime.Now.ToString());
            }
            else
            {
                GlobalSetting.SetConfig("AutoUpdate", "0");
            }

            //IsAllowMultiInstances
            GlobalSetting.IsAllowMultiInstances = chkAllowMultiInstances.Checked;
            GlobalSetting.SetConfig("IsAllowMultiInstances", GlobalSetting.IsAllowMultiInstances.ToString());

            //IsPressESCToQuit
            GlobalSetting.IsPressESCToQuit = chkESCToQuit.Checked;
            GlobalSetting.SetConfig("IsPressESCToQuit", GlobalSetting.IsPressESCToQuit.ToString());

            //IsConfirmationDelete
            GlobalSetting.IsConfirmationDelete = chkConfirmationDelete.Checked;
            GlobalSetting.SetConfig("IsConfirmationDelete", GlobalSetting.IsConfirmationDelete.ToString());

            //IsDisplayBasenameOfImage
            GlobalSetting.IsDisplayBasenameOfImage = chkDisplayBasename.Checked;
            GlobalSetting.SetConfig("IsDisplayBasenameOfImage", GlobalSetting.IsDisplayBasenameOfImage.ToString());


            #region IsScrollbarsVisible: MainFormForceUpdateAction.OTHER_SETTINGS
            //IsScrollbarsVisible
            newBool = chkShowScrollbar.Checked;
            if (GlobalSetting.IsScrollbarsVisible != newBool)
            {
                GlobalSetting.IsScrollbarsVisible = newBool;
                GlobalSetting.SetConfig("IsScrollbarsVisible", GlobalSetting.IsScrollbarsVisible.ToString());

                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.OTHER_SETTINGS;
            }
            #endregion


            #region BackgroundColor: MainFormForceUpdateAction.OTHER_SETTINGS
            //BackgroundColor
            newColor = picBackgroundColor.BackColor;
            if (GlobalSetting.BackgroundColor != newColor)
            {
                GlobalSetting.BackgroundColor = picBackgroundColor.BackColor;
                GlobalSetting.SetConfig("BackgroundColor", GlobalSetting.BackgroundColor.ToArgb().ToString(GlobalSetting.NumberFormat));
                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.OTHER_SETTINGS;
            }
            #endregion


            #endregion


            #region Image tab ----------------------------------------------

            #region IsRecursiveLoading: MainFormForceUpdateAction.IMAGE_FOLDER
            newBool = chkFindChildFolder.Checked;
            if (GlobalSetting.IsRecursiveLoading != newBool) //Only change when the new value selected  
            {
                GlobalSetting.IsRecursiveLoading = newBool;
                GlobalSetting.SetConfig("IsRecursiveLoading", GlobalSetting.IsRecursiveLoading.ToString());

                //Request frmMain to update the thumbnail bar
                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.IMAGE_FOLDER;
            }
            #endregion


            //IsShowingHiddenImages
            GlobalSetting.IsShowingHiddenImages = chkShowHiddenImages.Checked;
            GlobalSetting.SetConfig("IsShowingHiddenImages", GlobalSetting.IsShowingHiddenImages.ToString());

            //IsLoopBackViewer
            GlobalSetting.IsLoopBackViewer = chkLoopViewer.Checked;
            GlobalSetting.SetConfig("IsLoopBackViewer", GlobalSetting.IsLoopBackViewer.ToString());

            //IsImageBoosterBack
            GlobalSetting.IsImageBoosterBack = chkImageBoosterBack.Checked;
            GlobalSetting.SetConfig("IsImageBoosterBack", GlobalSetting.IsImageBoosterBack.ToString());
            

            #region ImageLoadingOrder: MainFormForceUpdateAction.IMAGE_LIST
            newInt = cmbImageOrder.SelectedIndex;

            if (Enum.TryParse(newInt.ToString(), out ImageOrderBy newOrder))
            {
                if (GlobalSetting.ImageLoadingOrder != newOrder) //Only change when the new value selected  
                {
                    GlobalSetting.ImageLoadingOrder = newOrder;
                    GlobalSetting.SetConfig("ImageLoadingOrder", newInt.ToString());

                    //Request frmMain to update
                    LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.IMAGE_LIST;
                }
            }
            
            #endregion
            

            //Mouse wheel actions
            GlobalSetting.MouseWheelAction = (MouseWheelActions)cmbMouseWheel.SelectedIndex;
            GlobalSetting.MouseWheelCtrlAction = (MouseWheelActions)cmbMouseWheelCtrl.SelectedIndex;
            GlobalSetting.MouseWheelShiftAction = (MouseWheelActions)cmbMouseWheelShift.SelectedIndex;
            GlobalSetting.MouseWheelAltAction = (MouseWheelActions)cmbMouseWheelAlt.SelectedIndex;

            GlobalSetting.SetConfig("MouseWheelAction", ((int)GlobalSetting.MouseWheelAction).ToString());
            GlobalSetting.SetConfig("MouseWheelCtrlAction", ((int)GlobalSetting.MouseWheelCtrlAction).ToString());
            GlobalSetting.SetConfig("MouseWheelShiftAction", ((int)GlobalSetting.MouseWheelShiftAction).ToString());
            GlobalSetting.SetConfig("MouseWheelAltAction", ((int)GlobalSetting.MouseWheelAltAction).ToString());

            
            //ZoomOptimization
            GlobalSetting.ZoomOptimizationMethod = (ZoomOptimizationMethods)cmbZoomOptimization.SelectedIndex;
            GlobalSetting.SetConfig("ZoomOptimization", ((int)GlobalSetting.ZoomOptimizationMethod).ToString(GlobalSetting.NumberFormat));


            #region THUMBNAIL BAR

            #region IsThumbnailHorizontal: MainFormForceUpdateAction.THUMBNAIL_BAR

            //IsThumbnailHorizontal
            newBool = !chkThumbnailVertical.Checked;
            if (GlobalSetting.IsThumbnailHorizontal != newBool) //Only change when the new value selected  
            {
                GlobalSetting.IsThumbnailHorizontal = newBool;
                GlobalSetting.SetConfig("IsThumbnailHorizontal", GlobalSetting.IsThumbnailHorizontal.ToString());

                //Request frmMain to update the thumbnail bar
                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.THUMBNAIL_BAR;
            }
            #endregion


            #region IsShowThumbnailScrollbar: MainFormForceUpdateAction.THUMBNAIL_BAR

            //IsShowThumbnailScrollbar
            newBool = chkShowThumbnailScrollbar.Checked;
            if (GlobalSetting.IsShowThumbnailScrollbar != newBool) //Only change when the new value selected  
            {
                GlobalSetting.IsShowThumbnailScrollbar = newBool;
                GlobalSetting.SetConfig("IsShowThumbnailScrollbar", GlobalSetting.IsShowThumbnailScrollbar.ToString());

                //Request frmMain to update the thumbnail bar
                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.THUMBNAIL_BAR;
            }
            #endregion


            #region ThumbnailDimension: MainFormForceUpdateAction.THUMBNAIL_ITEMS

            //ThumbnailDimension
            newInt = cmbThumbnailDimension.SelectedItem.ToString() == "" ? GlobalSetting.ThumbnailDimension : int.Parse(cmbThumbnailDimension.SelectedItem.ToString(), GlobalSetting.NumberFormat); 
            
            if (GlobalSetting.ThumbnailDimension != newInt) //Only change when the new value selected
            {
                GlobalSetting.ThumbnailDimension = newInt;
                GlobalSetting.SetConfig("ThumbnailDimension", GlobalSetting.ThumbnailDimension.ToString(GlobalSetting.NumberFormat));

                //Request frmMain to update the thumbnail bar
                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.THUMBNAIL_ITEMS;
            }
            #endregion

            #endregion


            //IsLoopBackSlideShow
            GlobalSetting.IsLoopBackSlideShow = chkLoopSlideshow.Checked;
            GlobalSetting.SetConfig("IsLoopBackSlideShow", GlobalSetting.IsLoopBackSlideShow.ToString());


            #region SlideShowInterval: MainFormForceUpdateAction.OTHER_SETTINGS

            //SlideShowInterval
            newInt = barInterval.Value;

            if (GlobalSetting.SlideShowInterval != newInt)
            {
                GlobalSetting.SlideShowInterval = newInt;
                GlobalSetting.SetConfig("SlideShowInterval", GlobalSetting.SlideShowInterval.ToString(GlobalSetting.NumberFormat));

                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.OTHER_SETTINGS;
            }
            #endregion


            //IsSaveAfterRotating
            GlobalSetting.IsSaveAfterRotating = chkSaveOnRotate.Checked;
            GlobalSetting.SetConfig("IsSaveAfterRotating", GlobalSetting.IsSaveAfterRotating.ToString());

            // PreserveModifiedDate
            GlobalSetting.PreserveModifiedDate = chkSaveModifyDate.Checked;
            GlobalSetting.SetConfig("PreserveModifiedDate", GlobalSetting.PreserveModifiedDate.ToString());

            #endregion


            #region Language tab -------------------------------------------

            #region Language: MainFormForceUpdateAction.LANGUAGE
            //Language
            newString = dsLanguages[cmbLanguage.SelectedIndex].FileName.ToLower();

            if (GlobalSetting.LangPack.FileName.ToLower().CompareTo(newString) != 0)
            {
                GlobalSetting.LangPack = dsLanguages[cmbLanguage.SelectedIndex];
                GlobalSetting.SetConfig("Language", GlobalSetting.LangPack.FileName);

                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.LANGUAGE;
            }
            #endregion



            #endregion


            #region Toolbar tab --------------------------------------------

            #region ToolbarPosition: MainFormForceUpdateAction.TOOLBAR_POSITION
            newInt = cmbToolbarPosition.SelectedIndex;

            if (Enum.TryParse(newInt.ToString(), out ToolbarPosition newPosition))
            {
                if (GlobalSetting.ToolbarPosition != newPosition) //Only change when the new value selected  
                {
                    GlobalSetting.ToolbarPosition = newPosition;
                    GlobalSetting.SetConfig("ToolbarPosition", newInt.ToString());

                    //Request frmMain to update
                    LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.TOOLBAR_POSITION;
                }
            }

            #endregion

            #region HorzCenterToolbarBtns: MainFormForceUpdateAction.TOOLBAR_POSITION

            newBool = chkHorzCenterToolbarBtns.Checked;
            if (GlobalSetting.IsCenterToolbar != newBool)
            {
                GlobalSetting.IsCenterToolbar = newBool;
                GlobalSetting.SetConfig("IsCenterToolbar", GlobalSetting.IsCenterToolbar.ToString());

                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.TOOLBAR_POSITION;
            }
            #endregion


            ApplyToolbarChanges();
            #endregion


            #region Color Picker tab ---------------------------------------
            GlobalSetting.IsColorPickerRGBA = chkColorUseRGBA.Checked;
            GlobalSetting.SetConfig("IsColorPickerRGBA", GlobalSetting.IsColorPickerRGBA.ToString());

            GlobalSetting.IsColorPickerHEXA = chkColorUseHEXA.Checked;
            GlobalSetting.SetConfig("IsColorPickerHEXA", GlobalSetting.IsColorPickerHEXA.ToString());

            GlobalSetting.IsColorPickerHSLA = chkColorUseHEXA.Checked;
            GlobalSetting.SetConfig("IsColorPickerHSLA", GlobalSetting.IsColorPickerHSLA.ToString());

            #endregion

        }





        #endregion

        
    }
}
