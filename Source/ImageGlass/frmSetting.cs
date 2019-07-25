/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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
using System.Threading.Tasks;
using System.Threading;

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
        private List<Language> lstLanguages = new List<Language>();

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
            Rectangle rc = GlobalSetting.StringToRect(GlobalSetting.GetConfig($"{this.Name}.WindowsBound", "280,125,900,700"));

            if (!Helper.IsOnScreen(rc.Location))
            {
                rc.Location = new Point(280, 125);
            }
            Bounds = rc;

            //windows state--------------------------------------------------------------
            string s = GlobalSetting.GetConfig($"{this.Name}.WindowsState", "Normal");
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
            LoadTabEditConfig();
            LoadTabColorPicker();

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
            Text = lang[$"{Name}._Text"];


            #region Tabs label
            lblGeneral.Text = lang[$"{Name}.lblGeneral"];
            lblImage.Text = lang[$"{Name}.lblImage"];
            lblEdit.Text = lang[$"{Name}.lblEdit"];
            lblFileAssociations.Text = lang[$"{Name}.lblFileAssociations"];
            lblLanguage.Text = lang[$"{Name}.lblLanguage"];
            lblToolbar.Text = lang[$"{Name}.lblToolbar"];
            lblColorPicker.Text = lang[$"{Name}.lblColorPicker"];
            lblTheme.Text = lang[$"{Name}.lblTheme"];
            lblKeyboard.Text = lang[$"{Name}.lblKeyboard"];

            btnSave.Text = lang[$"{Name}.btnSave"];
            btnCancel.Text = lang[$"{Name}.btnCancel"];
            btnApply.Text = lang[$"{Name}.btnApply"];
            #endregion


            #region GENERAL TAB
            lblHeadStartup.Text = lang[$"{Name}.lblHeadStartup"];//
            chkWelcomePicture.Text = lang[$"{Name}.chkWelcomePicture"];
            chkLastSeenImage.Text = lang[$"{Name}.chkLastSeenImage"];
            chkShowToolBar.Text = lang[$"{Name}.chkShowToolBar"];
            chkAllowMultiInstances.Text = lang[$"{Name}.chkAllowMultiInstances"];


            lblHeadConfigDir.Text = lang[$"{Name}.lblHeadConfigDir"];//
            lnkConfigDir.Text = GlobalSetting.ConfigDir();


            lblHeadOthers.Text = lang[$"{Name}.lblHeadOthers"];//
            chkAutoUpdate.Text = lang[$"{Name}.chkAutoUpdate"];
            chkESCToQuit.Text = lang[$"{Name}.chkESCToQuit"];
            chkConfirmationDelete.Text = lang[$"{Name}.chkConfirmationDelete"];
            chkShowScrollbar.Text = lang[$"{Name}.chkShowScrollbar"];
            chkDisplayBasename.Text = lang[$"{Name}.chkDisplayBasename"];
            chkShowNavButtons.Text = lang[$"{Name}.chkShowNavButtons"];
            chkShowCheckerboardOnlyImage.Text = lang[$"{Name}.chkShowCheckerboardOnlyImage"];
            lblBackGroundColor.Text = lang[$"{Name}.lblBackGroundColor"];
            lnkResetBackgroundColor.Text = lang[$"{Name}.lnkResetBackgroundColor"];
            #endregion


            #region IMAGE TAB
            lblHeadImageLoading.Text = lang[$"{Name}.lblHeadImageLoading"];//
            chkFindChildFolder.Text = lang[$"{Name}.chkFindChildFolder"];
            chkShowHiddenImages.Text = lang[$"{Name}.chkShowHiddenImages"];
            chkLoopViewer.Text = lang[$"{Name}.chkLoopViewer"];
            chkIsCenterImage.Text = lang[$"{Name}.chkIsCenterImage"];
            lblImageLoadingOrder.Text = lang[$"{Name}.lblImageLoadingOrder"];
            chkUseFileExplorerSortOrder.Text = lang[$"{Name}.chkUseFileExplorerSortOrder"];
            lblImageBoosterCachedCount.Text = lang[$"{Name}.lblImageBoosterCachedCount"];

            lblColorManagement.Text = lang[$"{Name}.lblColorManagement"];//
            chkApplyColorProfile.Text = lang[$"{Name}.chkApplyColorProfile"];
            lblColorProfile.Text = lang[$"{Name}.lblColorProfile"];
            lnkColorProfileBrowse.Text = lang[$"{Name}.lnkColorProfileBrowse"];

            lblHeadMouseWheelActions.Text = lang[$"{Name}.lblHeadMouseWheelActions"];
            lblMouseWheel.Text = lang[$"{Name}.lblMouseWheel"];
            lblMouseWheelAlt.Text = lang[$"{Name}.lblMouseWheelAlt"];
            lblMouseWheelCtrl.Text = lang[$"{Name}.lblMouseWheelCtrl"];
            lblMouseWheelShift.Text = lang[$"{Name}.lblMouseWheelShift"];

            lblHeadZooming.Text = lang[$"{Name}.lblHeadZooming"];//
            lblGeneral_ZoomOptimization.Text = lang[$"{Name}.lblGeneral_ZoomOptimization"];
            lblZoomLevels.Text = lang[$"{Name}.lblZoomLevels"];

            lblHeadThumbnailBar.Text = lang[$"{Name}.lblHeadThumbnailBar"];//
            chkThumbnailVertical.Text = lang[$"{Name}.chkThumbnailVertical"];
            chkShowThumbnailScrollbar.Text = lang[$"{Name}.chkShowThumbnailScrollbar"];
            lblGeneral_ThumbnailSize.Text = lang[$"{Name}.lblGeneral_ThumbnailSize"];

            lblHeadSlideshow.Text = lang[$"{Name}.lblHeadSlideshow"];//
            chkLoopSlideshow.Text = lang[$"{Name}.chkLoopSlideshow"];
            lblSlideshowInterval.Text = string.Format(lang[$"{Name}.lblSlideshowInterval"], barInterval.Value);

            #endregion


            #region EDIT TAB
            chkSaveOnRotate.Text = lang[$"{Name}.chkSaveOnRotate"];
            chkSaveModifyDate.Text = lang[$"{Name}.chkSaveModifyDate"];
            lblSelectAppForEdit.Text = lang[$"{Name}.lblSelectAppForEdit"];
            btnEditEditExt.Text = lang[$"{Name}.btnEditEditExt"];
            btnEditResetExt.Text = lang[$"{Name}.btnEditResetExt"];
            btnEditEditAllExt.Text = lang[$"{Name}.btnEditEditAllExt"];
            clnFileExtension.Text = lang[$"{Name}.lvImageEditing.clnFileExtension"];
            clnAppName.Text = lang[$"{Name}.lvImageEditing.clnAppName"];
            clnAppPath.Text = lang[$"{Name}.lvImageEditing.clnAppPath"];
            clnAppArguments.Text = lang[$"{Name}.lvImageEditing.clnAppArguments"];
            #endregion


            #region FILE ASSOCIATION TAB
            var extList = GlobalSetting.DefaultImageFormats.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            lblExtensionsGroupDescription.Text = lang[$"{Name}.lblExtensionsGroupDescription"];
            lblSupportedExtension.Text = String.Format(lang[$"{Name}.lblSupportedExtension"], extList.Length);
            lnkOpenFileAssoc.Text = lang[$"{Name}.lnkOpenFileAssoc"];
            btnAddNewExt.Text = lang[$"{Name}.btnAddNewExt"];
            btnDeleteExt.Text = lang[$"{Name}.btnDeleteExt"];
            btnRegisterExt.Text = lang[$"{Name}.btnRegisterExt"];
            btnResetExt.Text = lang[$"{Name}.btnResetExt"];
            lvExtension.Groups[(int)ImageFormatGroup.Default].Header = lang["_.ImageFormatGroup.Default"];
            lvExtension.Groups[(int)ImageFormatGroup.Optional].Header = lang["_.ImageFormatGroup.Optional"];
            #endregion


            #region LANGUAGE TAB
            lblLanguageText.Text = lang[$"{Name}.lblLanguageText"];
            lnkRefresh.Text = lang[$"{Name}.lnkRefresh"];
            lblLanguageWarning.Text = string.Format(lang[$"{Name}.lblLanguageWarning"], "ImageGlass " + Application.ProductVersion);
            lnkInstallLanguage.Text = lang[$"{Name}.lnkInstallLanguage"];
            lnkCreateNew.Text = lang[$"{Name}.lnkCreateNew"];
            lnkEdit.Text = lang[$"{Name}.lnkEdit"];
            lnkGetMoreLanguage.Text = lang[$"{Name}.lnkGetMoreLanguage"];
            #endregion


            #region TOOLBAR TAB
            lblToolbarPosition.Text = lang[$"{Name}.lblToolbarPosition"];
            chkHorzCenterToolbarBtns.Text = lang[$"{Name}.chkHorzCenterToolbarBtns"];

            _separatorText = lang[$"{Name}.txtSeparator"];
            lblUsedBtns.Text = lang[$"{Name}.lblUsedBtns"];
            lblAvailBtns.Text = lang[$"{Name}.lblAvailBtns"];

            tip1.SetToolTip(lblToolbar, lang[$"{Name}.lblToolbarTT"]);
            tip1.SetToolTip(btnMoveUp, lang[$"{Name}.btnMoveUpTT"]);
            tip1.SetToolTip(btnMoveDown, lang[$"{Name}.btnMoveDownTT"]);
            tip1.SetToolTip(btnMoveLeft, lang[$"{Name}.btnMoveLeftTT"]);
            tip1.SetToolTip(btnMoveRight, lang[$"{Name}.btnMoveRightTT"]);
            #endregion


            #region COLOR PICKER TAB
            lblColorCodeFormat.Text = lang[$"{Name}.lblColorCodeFormat"];
            chkColorUseRGBA.Text = lang[$"{Name}.chkColorUseRGBA"];
            chkColorUseHEXA.Text = lang[$"{Name}.chkColorUseHEXA"];
            chkColorUseHSLA.Text = lang[$"{Name}.chkColorUseHSLA"];
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


            #region KEYBOARD TAB
            btnKeyReset.Text = lang[$"{Name}.btnKeyReset"];
            lblKeysSpaceBack.Text = lang[$"{Name}.lblKeysSpaceBack"];
            lblKeysPageUpDown.Text = lang[$"{Name}.lblKeysPageUpDown"];
            lblKeysUpDown.Text = lang[$"{Name}.lblKeysUpDown"];
            lblKeysLeftRight.Text = lang[$"{Name}.lblKeysLeftRight"];
            #endregion


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
                case "lblEdit":
                    tab1.SelectedTab = tabEdit;
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
                case "lblKeyboard":
                    tab1.SelectedTab = tabKeyboard;
                    break;
            }
        }


        private void tab1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblGeneral.Tag =
            lblImage.Tag =
            lblEdit.Tag =
            lblFileAssociations.Tag =
            lblLanguage.Tag =
            lblToolbar.Tag =
            lblColorPicker.Tag =
            lblTheme.Tag =
            lblKeyboard.Tag = 0;

            lblGeneral.BackColor =
            lblImage.BackColor =
            lblEdit.BackColor =
            lblFileAssociations.BackColor =
            lblLanguage.BackColor =
            lblToolbar.BackColor =
            lblColorPicker.BackColor =
            lblTheme.BackColor =
            lblKeyboard.BackColor = M_COLOR_MENU_NORMAL;

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
            else if (tab1.SelectedTab == tabEdit)
            {
                lblEdit.Tag = 1;
                lblEdit.BackColor = M_COLOR_MENU_ACTIVE;

                LoadTabEditConfig();
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
            else if (tab1.SelectedTab == tabKeyboard)
            {
                lblKeyboard.Tag = 1;
                lblKeyboard.BackColor = M_COLOR_MENU_ACTIVE;

                LoadTabKeyboard();
            }

        }


        #region TAB GENERAL

        /// <summary>
        /// Get and load value of General tab
        /// </summary>
        private void LoadTabGeneralConfig()
        {
            //Get value of chkLastSeenImage ----------------------------------------------------
            chkLastSeenImage.Checked = GlobalSetting.IsOpenLastSeenImage;
            GlobalSetting.SetConfig("LastSeenImagePath", "");
            GlobalSetting.SetConfig("IsOpenLastSeenImage", GlobalSetting.IsOpenLastSeenImage.ToString());

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

            //Get value of IsShowNavigationButtons
            chkShowNavButtons.Checked = GlobalSetting.IsShowNavigationButtons;

            //Get value of IsShowNavigationButtons
            chkShowCheckerboardOnlyImage.Checked = GlobalSetting.IsShowCheckerboardOnlyImageRegion;

            //Get background color
            picBackgroundColor.BackColor = GlobalSetting.BackgroundColor;
        }


        private void lnkConfigDir_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", GlobalSetting.ConfigDir());
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
            // Set value of chkFindChildFolder ---------------------------------------------
            chkFindChildFolder.Checked = GlobalSetting.IsRecursiveLoading;

            // Set value of chkShowHiddenImages
            chkShowHiddenImages.Checked = GlobalSetting.IsShowingHiddenImages;

            // Set value of chkLoopViewer
            chkLoopViewer.Checked = GlobalSetting.IsLoopBackViewer;

            // Set value of chkIsCenterImage
            chkIsCenterImage.Checked = GlobalSetting.IsCenterImage;

            // Set value of chkUseFileExplorerSortOrder
            chkUseFileExplorerSortOrder.Checked = GlobalSetting.IsUseFileExplorerSortOrder;


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


            #region Load items of cmbImageOrderType
            var orderTypesList = Enum.GetNames(typeof(ImageOrderType));
            cmbImageOrderType.Items.Clear();

            foreach (var item in orderTypesList)
            {
                cmbImageOrderType.Items.Add(GlobalSetting.LangPack.Items[$"{this.Name}.cmbImageOrderType._{item}"]);
            }

            //Get value of cmbImageOrder
            cmbImageOrderType.SelectedIndex = (int)GlobalSetting.ImageLoadingOrderType;
            #endregion


            // Set value of cmbImageBoosterCachedCount
            cmbImageBoosterCachedCount.SelectedIndex = GlobalSetting.ImageBoosterCachedCount;


            #region Color Management
            chkApplyColorProfile.Checked = GlobalSetting.IsApplyColorProfileForAll;

            // color profile list
            cmbColorProfile.Items.Clear();
            cmbColorProfile.Items.Add(GlobalSetting.LangPack.Items[$"{Name}.cmbColorProfile._None"]);
            cmbColorProfile.Items.AddRange(Heart.Helpers.GetBuiltInColorProfiles());
            cmbColorProfile.Items.Add(GlobalSetting.LangPack.Items[$"{Name}.cmbColorProfile._CustomProfileFile"]); // always last position


            // select the color profile
            if (File.Exists(GlobalSetting.ColorProfile))
            {
                cmbColorProfile.SelectedIndex = cmbColorProfile.Items.Count - 1;
                lnkColorProfilePath.Text = GlobalSetting.ColorProfile;

                lnkColorProfileBrowse.Visible = true;
                lnkColorProfilePath.Visible = true;
            }
            else
            {
                // first item selected default
                cmbColorProfile.SelectedIndex = 0;

                for (int i = 0; i < cmbColorProfile.Items.Count; i++)
                {
                    if (cmbColorProfile.Items[i].ToString() == GlobalSetting.ColorProfile)
                    {
                        cmbColorProfile.SelectedIndex = i;
                        break;
                    }
                }

                lnkColorProfilePath.Text = string.Empty;
                lnkColorProfileBrowse.Visible = false;
                lnkColorProfilePath.Visible = false;
            }

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


            #region Zooming

            // Load items of cmbZoomOptimization
            var zoomOptimizationList = Enum.GetNames(typeof(ZoomOptimizationMethods));
            cmbZoomOptimization.Items.Clear();
            foreach (var item in zoomOptimizationList)
            {
                cmbZoomOptimization.Items.Add(GlobalSetting.LangPack.Items[$"{this.Name}.cmbZoomOptimization._{item}"]);
            }

            // Get value of cmbZoomOptimization
            cmbZoomOptimization.SelectedIndex = (int)GlobalSetting.ZoomOptimizationMethod;

            // Load zoom levels text
            txtZoomLevels.Text = GlobalSetting.IntArrayToString(GlobalSetting.ZoomLevels);

            #endregion


            // Thumbnail bar on right side ----------------------------------------------------
            chkThumbnailVertical.Checked = !GlobalSetting.IsThumbnailHorizontal;

            // Show thumbnail scrollbar
            chkShowThumbnailScrollbar.Checked = GlobalSetting.IsShowThumbnailScrollbar;

            // load thumbnail dimension
            cmbThumbnailDimension.SelectedItem = GlobalSetting.ThumbnailDimension.ToString();

            // Set value of chkLoopSlideshow --------------------------------------------------
            chkLoopSlideshow.Checked = GlobalSetting.IsLoopBackSlideShow;

            // Set value of barInterval
            barInterval.Value = GlobalSetting.SlideShowInterval;
            lblSlideshowInterval.Text = string.Format(GlobalSetting.LangPack.Items[$"{Name}.lblSlideshowInterval"], barInterval.Value);

        }


        private void barInterval_Scroll(object sender, EventArgs e)
        {
            lblSlideshowInterval.Text = string.Format(GlobalSetting.LangPack.Items[$"{Name}.lblSlideshowInterval"], barInterval.Value);
        }


        private void cmbColorProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            // if Custom ICC/ICM profile file selected
            if (cmbColorProfile.SelectedIndex == cmbColorProfile.Items.Count - 1)
            {
                // show the Browse and ICC path link
                lnkColorProfileBrowse.Visible = true;
                lnkColorProfilePath.Visible = true;
            }
            else
            {
                lnkColorProfileBrowse.Visible = false;
                lnkColorProfilePath.Visible = false;
            }
        }


        private void lnkColorProfileBrowse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog()
            {
                Filter = "Supported files|*.icc;*.icm;|All files|*.*",
                CheckFileExists = true,

            };

            if (o.ShowDialog() == DialogResult.OK)
            {
                lnkColorProfilePath.Text = o.FileName;
            }
        }


        private void lnkColorProfilePath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(lnkColorProfilePath.Text))
            {
                Process.Start("explorer.exe", "/select,\"" +
                    lnkColorProfilePath.Text + "\"");
            }
        }

        #endregion


        #region TAB EDIT

        /// <summary>
        /// Get and load value of tab Edit
        /// </summary>
        private void LoadTabEditConfig()
        {
            //Get value of IsSaveAfterRotating
            chkSaveOnRotate.Checked = GlobalSetting.IsSaveAfterRotating;
            chkSaveModifyDate.Checked = GlobalSetting.PreserveModifiedDate;

            //Load Image Editing extension list
            LoadImageEditingAssociationList();
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
                FileExtension = $"<{string.Format(GlobalSetting.LangPack.Items[$"{Name}._allExtensions"])}>",
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


        #region TAB LANGUAGES
        private void lnkGetMoreLanguage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string version = Application.ProductVersion.Replace(".", "_");
                Process.Start("https://imageglass.org/languages?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_languagepack");
            }
            catch { }
        }

        private void lnkInstallLanguage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = GlobalSetting.StartUpDir("igtasks.exe");
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
            p.StartInfo.FileName = GlobalSetting.StartUpDir("igtasks.exe");
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
            p.StartInfo.FileName = GlobalSetting.StartUpDir("igtasks.exe");
            p.StartInfo.Arguments = "igeditlang \"" + GlobalSetting.LangPack.FileName + "\"";

            try
            {
                p.Start();
            }
            catch { }
        }

        private async void lnkRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cmbLanguage.Items.Clear();
            cmbLanguage.Items.Add("English");
            lstLanguages = new List<Language> {
                new Language()
            };

            string langPath = GlobalSetting.StartUpDir(Dir.Languages);

            if (Directory.Exists(langPath))
            {
                await Task.Run(() =>
                {
                    foreach (string f in Directory.GetFiles(langPath))
                    {
                        if (Path.GetExtension(f).ToLower() == ".iglang")
                        {
                            Language l = new Language(f);
                            lstLanguages.Add(l);
                        }
                    }
                });


                // start from 1, the first item is already hardcoded
                for (int i = 1; i < lstLanguages.Count; i++)
                {
                    int iLang = cmbLanguage.Items.Add(lstLanguages[i].LangName);
                    string curLang = GlobalSetting.LangPack.FileName;

                    //using current language pack
                    if (lstLanguages[i].FileName.CompareTo(curLang) == 0)
                    {
                        cmbLanguage.SelectedIndex = iLang;
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
            if (lang.MinVersion.CompareTo(lstLanguages[cmbLanguage.SelectedIndex].MinVersion) != 0)
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

            lblSupportedExtension.Text = String.Format(GlobalSetting.LangPack.Items[$"{Name}.lblSupportedExtension"], lvExtension.Items.Count);

            // Write suported image formats to settings -----------------------------------------
            // Load Default Image Formats
            GlobalSetting.SetConfig("DefaultImageFormats", GlobalSetting.DefaultImageFormats);
            // Load Optional Image Formats
            GlobalSetting.SetConfig("OptionalImageFormats", GlobalSetting.OptionalImageFormats);

            // build the hashset GlobalSetting.ImageFormatHashSet
            GlobalSetting.BuildImageFormatHashSet();
        }

        /// <summary>
        /// Register file associations and Web-to-App linking
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

            using (Process p = new Process())
            {
                var isError = true;

                p.StartInfo.FileName = GlobalSetting.StartUpDir("igtasks.exe");
                p.StartInfo.Arguments = $"regassociations {extensions}";
                p.Start();

                p.WaitForExit();
                isError = p.ExitCode != 0;

                if (isError)
                {
                    MessageBox.Show(GlobalSetting.LangPack.Items[$"{Name}._RegisterAppExtensions_Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(GlobalSetting.LangPack.Items[$"{Name}._RegisterAppExtensions_Success"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
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

            //Request frmMain to update
            LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.IMAGE_LIST;
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

                //Request frmMain to update
                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.IMAGE_LIST;
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

            //Update size of toolbar
            int newToolBarItemHeight = int.Parse(Math.Floor((toolMain.Height * 0.8)).ToString());

            // get correct icon height
            var hIcon = ThemeImage.GetCorrectIconHeight();

            foreach (var btn in lstToolbarButtons)
            {
                if (btn == "_sep_")
                {
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
                        var toolbarBtn = info.GetValue(form) as ToolStripItem;

                        // update the item siE
                        toolbarBtn.Size = new Size(newToolBarItemHeight, newToolBarItemHeight);

                        // add item to toolbar
                        toolMain.Items.Add(toolbarBtn);
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

        private async void RefreshThemeList()
        {
            string themeFolder = GlobalSetting.ConfigDir(Dir.Themes);

            lvTheme.Items.Clear();
            lvTheme.Items.Add("2017 (Dark)").Tag = "default";
            lvTheme.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

            if (Directory.Exists(themeFolder))
            {
                var lstThemes = new List<Theme.Theme>();

                await Task.Run(() =>
                {
                    foreach (string d in Directory.GetDirectories(themeFolder))
                    {
                        string configFile = Path.Combine(d, "config.xml");

                        if (File.Exists(configFile))
                        {
                            Theme.Theme th = new Theme.Theme(d);

                            //invalid theme
                            if (!th.IsThemeValid)
                            {
                                continue;
                            }

                            lstThemes.Add(th);
                        }
                    }
                });

                // add themes to the listview
                foreach (var th in lstThemes)
                {
                    var lvi = new ListViewItem(th.Name)
                    {
                        // folder name of the theme
                        Tag = Path.GetFileName(Path.GetDirectoryName(th.ThemeConfigFilePath)),
                        ImageKey = "_blank"
                    };

                    if (LocalSetting.Theme.ThemeConfigFilePath == th.ThemeConfigFilePath)
                    {
                        lvi.Selected = true;
                        lvi.Checked = true;
                    }

                    lvTheme.Items.Add(lvi);
                }

                //select the default theme
                if (lvTheme.Items.Count > 0 && lvTheme.SelectedItems.Count == 0)
                {
                    lvTheme.Items[0].Selected = true;
                }
            }
            else
            {
                Directory.CreateDirectory(themeFolder);
            }

            lvTheme.Enabled = true;
            this.Cursor = Cursors.Default;

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
                btnThemeSaveAs.Enabled = true;
                btnThemeUninstall.Enabled = true;

                string themeName = lvTheme.SelectedItems[0].Tag.ToString();
                if (themeName == "default")
                {
                    //btnThemeSaveAs.Enabled = false;
                    btnThemeUninstall.Enabled = false;
                }


                Theme.Theme t = new Theme.Theme(GlobalSetting.ConfigDir(Dir.Themes, themeName));
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

                    MessageBox.Show(GlobalSetting.LangPack.Items[$"{Name}.btnThemeInstall._Success"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(GlobalSetting.LangPack.Items[$"{Name}.btnThemeInstall._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void btnThemeUninstall_Click(object sender, EventArgs e)
        {
            if (lvTheme.SelectedItems.Count > 0)
            {
                string themeName = lvTheme.SelectedItems[0].Tag.ToString();
                var result = Theme.Theme.UninstallTheme(themeName);

                if (result == ThemeUninstallingResult.SUCCESS)
                {
                    RefreshThemeList();
                }
                else if (result == ThemeUninstallingResult.ERROR)
                {
                    MessageBox.Show(GlobalSetting.LangPack.Items[$"{Name}.btnThemeUninstall._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }


        private void lnkThemeDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string version = Application.ProductVersion.Replace(".", "_");
                Process.Start("https://imageglass.org/themes?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_download_theme");
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
                    string themeName = lvTheme.SelectedItems[0].Tag.ToString();
                    string configFilePath = GlobalSetting.ConfigDir(Dir.Themes, themeName, "config.xml");

                    if (!File.Exists(configFilePath))
                    {
                        configFilePath = GlobalSetting.StartUpDir(@"DefaultTheme\config.xml");
                    }

                    var themeDir = Path.GetDirectoryName(configFilePath);
                    var result = Theme.Theme.PackTheme(themeDir, s.FileName);

                    if (result == ThemePackingResult.SUCCESS)
                    {
                        MessageBox.Show(string.Format(GlobalSetting.LangPack.Items[$"{Name}.btnThemeSaveAs._Success"], s.FileName), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(GlobalSetting.LangPack.Items[$"{Name}.btnThemeSaveAs._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void btnThemeFolderOpen_Click(object sender, EventArgs e)
        {
            string themeFolder = GlobalSetting.ConfigDir(Dir.Themes);
            Process.Start("explorer.exe", themeFolder);
        }


        private void btnThemeApply_Click(object sender, EventArgs e)
        {
            if (lvTheme.SelectedItems.Count > 0)
            {
                string themeFolderName = lvTheme.SelectedItems[0].Tag.ToString();
                string themeFolderPath = GlobalSetting.ConfigDir(Dir.Themes, themeFolderName);

                var th = Theme.Theme.ApplyTheme(themeFolderPath);

                if (th.IsThemeValid)
                {
                    LocalSetting.Theme = th;
                    GlobalSetting.BackgroundColor = picBackgroundColor.BackColor = LocalSetting.Theme.BackgroundColor;

                    LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.THEME;

                    th = null;

                    MessageBox.Show(GlobalSetting.LangPack.Items[$"{Name}.btnThemeApply._Success"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(GlobalSetting.LangPack.Items[$"{Name}.btnThemeApply._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }



        #endregion


        #region TAB KEYBOARD
        private void LoadTabKeyboard()
        {
            GlobalSetting.LoadKeyAssignments();
            var lang = GlobalSetting.LangPack.Items;

            cmbKeysLeftRight.Items.Clear();
            cmbKeysLeftRight.Items.Add(lang[$"{Name}.KeyActions._PrevNextImage"]);
            cmbKeysLeftRight.Items.Add(lang[$"{Name}.KeyActions._PanLeftRight"]);

            cmbKeysUpDown.Items.Clear();
            cmbKeysUpDown.Items.Add(lang[$"{Name}.KeyActions._PanUpDown"]);
            cmbKeysUpDown.Items.Add(lang[$"{Name}.KeyActions._ZoomInOut"]);

            cmbKeysPgUpDown.Items.Clear();
            cmbKeysPgUpDown.Items.Add(lang[$"{Name}.KeyActions._PrevNextImage"]);
            cmbKeysPgUpDown.Items.Add(lang[$"{Name}.KeyActions._ZoomInOut"]);

            cmbKeysSpaceBack.Items.Clear();
            cmbKeysSpaceBack.Items.Add(lang[$"{Name}.KeyActions._PauseSlideshow"]);
            cmbKeysSpaceBack.Items.Add(lang[$"{Name}.KeyActions._PrevNextImage"]);

            // brute-forcing this. need better solution?
            mapKeyConfigToComboSelection(KeyCombos.LeftRight, cmbKeysLeftRight,
                lang[$"{Name}.KeyActions._PrevNextImage"]);
            mapKeyConfigToComboSelection(KeyCombos.PageUpDown, cmbKeysPgUpDown,
                lang[$"{Name}.KeyActions._PrevNextImage"]);
            mapKeyConfigToComboSelection(KeyCombos.UpDown, cmbKeysUpDown,
                lang[$"{Name}.KeyActions._PanUpDown"]);
            mapKeyConfigToComboSelection(KeyCombos.SpaceBack, cmbKeysSpaceBack,
                lang[$"{Name}.KeyActions._PauseSlideshow"]);
        }

        /// <summary>
        /// Translates the config value for a key assignment to a selected
        /// entry in a combobox.
        /// 
        /// If something wrong, sets the combobox to the provided default.
        /// </summary>
        /// <param name="which">the key action to match</param>
        /// <param name="control">the combobox to set selection in</param>
        /// <param name="defaultString">On misconfiguration, use this string</param>
        /// <returns></returns>
        private void mapKeyConfigToComboSelection(KeyCombos which, ComboBox control, string defaultString)
        {
            try
            {
                var lang = GlobalSetting.LangPack.Items;

                // Fetch the string from language based on the action value
                var act = GlobalSetting.GetKeyAction(which);
                var actionList = Enum.GetNames(typeof(AssignableActions));
                var lookup = $"{Name}.KeyActions._{actionList[(int)act]}";
                string val = lang[lookup];

                // select the appropriate entry in the combo. On misconfiguration,
                // set to the provided default.
                control.SelectedItem = val;
                if (control.SelectedIndex == -1)
                {
                    control.SelectedItem = defaultString;
                }
            }
            catch
            {
                // Some other situation (value out of range; not in strings; etc),
                // use provided default
                control.SelectedItem = defaultString;
            }
        }

        /// <summary>
        /// Save the keyboard configuration settings to the config file
        /// </summary>
        private void SaveKeyboardSettings()
        {
            // Brute-forcing this. Better solution?
            saveKeyConfigFromCombo(KeyCombos.LeftRight, cmbKeysLeftRight);
            saveKeyConfigFromCombo(KeyCombos.PageUpDown, cmbKeysPgUpDown);
            saveKeyConfigFromCombo(KeyCombos.UpDown, cmbKeysUpDown);
            saveKeyConfigFromCombo(KeyCombos.SpaceBack, cmbKeysSpaceBack);
            GlobalSetting.SaveKeyAssignments();
        }

        /// <summary>
        /// For a given combobox, update the key config value in GlobalSetting
        /// </summary>
        /// <param name="which"></param>
        /// <param name="control"></param>
        private void saveKeyConfigFromCombo(KeyCombos which, ComboBox control)
        {
            var selected = control.SelectedItem;
            if (selected == null)
                return; // user hasn't visited keyboard page, no changes

            var lang = GlobalSetting.LangPack.Items;

            // match the text of the selected combobox item against
            // the language string for the available actions
            var actionList = Enum.GetNames(typeof(AssignableActions));
            for (int i = 0; i < actionList.Length; i++)
            {
                var lookup = $"{Name}.KeyActions._{actionList[i]}";
                string val = lang[lookup];

                if (val == selected.ToString())
                {
                    GlobalSetting.SetKeyAction(which, i);
                    return;
                }
            }

        }

        /// <summary>
        /// Reset all key actions to their "default" (IG V6.0) behavior
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnKeyReset_Click(object sender, EventArgs e)
        {
            GlobalSetting.SetKeyAction(KeyCombos.LeftRight, (int)AssignableActions.PrevNextImage);
            GlobalSetting.SetKeyAction(KeyCombos.UpDown, (int)AssignableActions.PanUpDown);
            GlobalSetting.SetKeyAction(KeyCombos.PageUpDown, (int)AssignableActions.PrevNextImage);
            GlobalSetting.SetKeyAction(KeyCombos.SpaceBack, (int)AssignableActions.PauseSlideshow);
            GlobalSetting.SaveKeyAssignments();
            LoadTabKeyboard();
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
            // Save and close
            if (ApplySettings())
            {
                this.Close();
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            ApplySettings();
        }


        private bool ApplySettings()
        {
            // Variables for comparision
            var isSuccessful = true;
            int newInt;
            bool newBool;
            string newString;
            Color newColor;


            #region General tab --------------------------------------------
            // IsShowWelcome
            GlobalSetting.IsShowWelcome = chkWelcomePicture.Checked;
            GlobalSetting.SetConfig("IsShowWelcome", GlobalSetting.IsShowWelcome.ToString());

            GlobalSetting.IsOpenLastSeenImage = chkLastSeenImage.Checked;
            GlobalSetting.SetConfig("IsOpenLastSeenImage", GlobalSetting.IsOpenLastSeenImage.ToString());


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

            //IsShowNavigationButtons
            GlobalSetting.IsShowNavigationButtons = chkShowNavButtons.Checked;
            GlobalSetting.SetConfig("IsShowNavigationButtons", GlobalSetting.IsShowNavigationButtons.ToString());


            #region IsShowCheckerboardOnlyImageRegion: MainFormForceUpdateAction.OTHER_SETTINGS
            //IsShowCheckerboardOnlyImageRegion
            newBool = chkShowCheckerboardOnlyImage.Checked;
            if (GlobalSetting.IsShowCheckerboardOnlyImageRegion != newBool)
            {
                GlobalSetting.IsShowCheckerboardOnlyImageRegion = newBool;
                GlobalSetting.SetConfig("IsShowCheckerboardOnlyImageRegion", GlobalSetting.IsShowCheckerboardOnlyImageRegion.ToString());

                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.OTHER_SETTINGS;
            }
            #endregion


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
                GlobalSetting.SetConfig("BackgroundColor", Theme.Theme.ConvertColorToHEX(GlobalSetting.BackgroundColor, true));
                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.OTHER_SETTINGS;
            }
            #endregion


            #endregion


            #region Image tab ----------------------------------------------

            #region IsRecursiveLoading: MainFormForceUpdateAction.IMAGE_LIST or IMAGE_LIST_NO_RECURSIVE
            newBool = chkFindChildFolder.Checked;
            if (GlobalSetting.IsRecursiveLoading != newBool) // Only change when the new value selected  
            {
                GlobalSetting.IsRecursiveLoading = newBool;
                GlobalSetting.SetConfig("IsRecursiveLoading", GlobalSetting.IsRecursiveLoading.ToString());

                // Request frmMain to update the thumbnail bar
                if (GlobalSetting.IsRecursiveLoading)
                {
                    LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.IMAGE_LIST;
                }
                else
                {
                    LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.IMAGE_LIST_NO_RECURSIVE;
                }
            }
            #endregion


            // IsShowingHiddenImages
            GlobalSetting.IsShowingHiddenImages = chkShowHiddenImages.Checked;
            GlobalSetting.SetConfig("IsShowingHiddenImages", GlobalSetting.IsShowingHiddenImages.ToString());

            // IsLoopBackViewer
            GlobalSetting.IsLoopBackViewer = chkLoopViewer.Checked;
            GlobalSetting.SetConfig("IsLoopBackViewer", GlobalSetting.IsLoopBackViewer.ToString());


            // IsCenterImage
            GlobalSetting.IsCenterImage = chkIsCenterImage.Checked;
            GlobalSetting.SetConfig("IsCenterImage", GlobalSetting.IsCenterImage.ToString());


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

            newInt = cmbImageOrderType.SelectedIndex;
            if (Enum.TryParse(newInt.ToString(), out ImageOrderType newOrderType))
            {
                if (GlobalSetting.ImageLoadingOrderType != newOrderType) //Only change when the new value selected  
                {
                    GlobalSetting.ImageLoadingOrderType = newOrderType;
                    GlobalSetting.SetConfig("ImageLoadingOrderType", newInt.ToString());

                    //Request frmMain to update
                    LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.IMAGE_LIST;
                }
            }

            // IsUseFileExplorerSortOrder
            GlobalSetting.IsUseFileExplorerSortOrder = chkUseFileExplorerSortOrder.Checked;
            GlobalSetting.SetConfig("IsUseFileExplorerSortOrder", GlobalSetting.IsUseFileExplorerSortOrder.ToString());

            #endregion


            // ImageBoosterCachedCount
            GlobalSetting.ImageBoosterCachedCount = cmbImageBoosterCachedCount.SelectedIndex;
            GlobalSetting.SetConfig("ImageBoosterCachedCount", GlobalSetting.ImageBoosterCachedCount.ToString());
            GlobalSetting.ImageList.MaxQueue = GlobalSetting.ImageBoosterCachedCount;


            #region Color Management

            // apply color profile for all
            GlobalSetting.IsApplyColorProfileForAll = chkApplyColorProfile.Checked;
            GlobalSetting.SetConfig("IsApplyColorProfileForAll", GlobalSetting.IsApplyColorProfileForAll.ToString());

            // color profile
            if (cmbColorProfile.SelectedIndex == cmbColorProfile.Items.Count - 1)
            {
                // custom color profile file
                GlobalSetting.ColorProfile = lnkColorProfilePath.Text;
            }
            else
            {
                // built-in color profile
                GlobalSetting.ColorProfile = cmbColorProfile.SelectedItem.ToString();
            }

            GlobalSetting.SetConfig("ColorProfile", GlobalSetting.ColorProfile);

            #endregion


            #region Mouse wheel actions
            GlobalSetting.MouseWheelAction = (MouseWheelActions)cmbMouseWheel.SelectedIndex;
            GlobalSetting.MouseWheelCtrlAction = (MouseWheelActions)cmbMouseWheelCtrl.SelectedIndex;
            GlobalSetting.MouseWheelShiftAction = (MouseWheelActions)cmbMouseWheelShift.SelectedIndex;
            GlobalSetting.MouseWheelAltAction = (MouseWheelActions)cmbMouseWheelAlt.SelectedIndex;

            GlobalSetting.SetConfig("MouseWheelAction", ((int)GlobalSetting.MouseWheelAction).ToString());
            GlobalSetting.SetConfig("MouseWheelCtrlAction", ((int)GlobalSetting.MouseWheelCtrlAction).ToString());
            GlobalSetting.SetConfig("MouseWheelShiftAction", ((int)GlobalSetting.MouseWheelShiftAction).ToString());
            GlobalSetting.SetConfig("MouseWheelAltAction", ((int)GlobalSetting.MouseWheelAltAction).ToString());
            #endregion


            // ZoomOptimization
            GlobalSetting.ZoomOptimizationMethod = (ZoomOptimizationMethods)cmbZoomOptimization.SelectedIndex;
            GlobalSetting.SetConfig("ZoomOptimization", ((int)GlobalSetting.ZoomOptimizationMethod).ToString(GlobalSetting.NumberFormat));


            #region ZoomLevels: MainFormForceUpdateAction.OTHER_SETTINGS;
            newString = txtZoomLevels.Text.Trim();

            if (string.IsNullOrEmpty(newString))
            {
                txtZoomLevels.Text = GlobalSetting.IntArrayToString(GlobalSetting.ZoomLevels);
            }
            else if (GlobalSetting.IntArrayToString(GlobalSetting.ZoomLevels) != newString)
            {
                try
                {
                    GlobalSetting.ZoomLevels = GlobalSetting.StringToIntArray(newString, unsignedOnly: true, distinct: true);
                    GlobalSetting.SetConfig("ZoomLevels", newString);

                    LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.OTHER_SETTINGS;
                }
                catch (Exception ex)
                {
                    isSuccessful = false;
                    txtZoomLevels.Text = GlobalSetting.IntArrayToString(GlobalSetting.ZoomLevels);
                    var msg = string.Format(GlobalSetting.LangPack.Items[$"{Name}.txtZoomLevels._Error"], ex.Message);

                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            #endregion


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


            // IsLoopBackSlideShow
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


            #endregion


            #region Edit tab -----------------------------------------------

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
            if (lstLanguages.Count > 0)
            {
                newString = lstLanguages[cmbLanguage.SelectedIndex].FileName.ToLower();

                if (GlobalSetting.LangPack.FileName.ToLower().CompareTo(newString) != 0)
                {
                    GlobalSetting.LangPack = lstLanguages[cmbLanguage.SelectedIndex];
                    GlobalSetting.SetConfig("Language", Path.GetFileName(GlobalSetting.LangPack.FileName));

                    LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.LANGUAGE;
                }
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

            GlobalSetting.IsColorPickerHSLA = chkColorUseHSLA.Checked;
            GlobalSetting.SetConfig("IsColorPickerHSLA", GlobalSetting.IsColorPickerHSLA.ToString());

            #endregion


            SaveKeyboardSettings();


            return isSuccessful;
        }




        #endregion


    }
}
