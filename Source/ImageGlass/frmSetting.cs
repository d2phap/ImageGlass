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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageGlass.Base;
using ImageGlass.Library;
using ImageGlass.Library.Image;
using ImageGlass.Settings;
using ImageGlass.UI;
using ImageGlass.UI.Renderers;

namespace ImageGlass {
    public partial class frmSetting: Form {
        public frmSetting() {
            InitializeComponent();

            imglGeneral.ImageSize = new Size(10, DPIScaling.Transform(30));
            imglGeneral.Images.Add("_blank", new Bitmap(10, DPIScaling.Transform(30)));

            // Filter user input for zoom levels
            txtZoomLevels.KeyPress += TxtZoomLevels_KeyPress;

            // Apply theme
            Configs.ApplyFormTheme(this, Configs.Theme);
        }


        #region PROPERTIES
        private readonly Color M_COLOR_MENU_SELECTED = Color.FromArgb(255, 198, 203, 204);
        private readonly Color M_COLOR_MENU_ACTIVE = Color.FromArgb(255, 145, 150, 153);
        private readonly Color M_COLOR_MENU_HOVER = Color.FromArgb(255, 176, 181, 183);
        private readonly Color M_COLOR_MENU_NORMAL = Color.FromArgb(255, 160, 165, 168);

        private List<Language> lstLanguages = new();

        #region Toolbar
        private string _separatorText; // Text used in lists to represent separator bar
        private ImageList _lstToolbarImg;
        private List<ListViewItem> _lstMasterUsed;

        // instance of frmMain, for reflection
        public frmMain MainInstance { get; internal set; }
        #endregion

        #endregion

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

        #region FRMSETTING FORM EVENTS
        private void frmSetting_Load(object sender, EventArgs e) {
            // Remove tabs header
            tab1.Appearance = TabAppearance.FlatButtons;
            tab1.ItemSize = new Size(0, 1);
            tab1.SizeMode = TabSizeMode.Fixed;

            // Load config
            // Windows Bound (Position + Size)-------------------------------------------
            Bounds = Configs.FrmSettingsWindowBound;

            // windows state--------------------------------------------------------------
            WindowState = Configs.FrmSettingsWindowState;

            LoadLanguagePack(); // Needs to be done before setting up the initial tab

            // Get the last view of tab --------------------------------------------------
            tab1.SelectedIndex = Local.SettingsTabLastView;

            // Load configs
            LoadTabGeneralConfig();
            LoadTabImageConfig();
            LoadTabEditConfig();
            LoadTabTools();

            // to prevent the setting: ToolbarPosition = -1, we load this onLoad event
            LoadTabToolbar();
        }

        private void frmSetting_FormClosing(object sender, FormClosingEventArgs e) {
            // Save config---------------------------------
            if (WindowState == FormWindowState.Normal) {
                // Windows Bound---------------------------------------------------
                Configs.FrmSettingsWindowBound = Bounds;
            }

            Configs.FrmSettingsWindowState = WindowState;

            // Tabs State----------------------------------------------------------
            Local.SettingsTabLastView = tab1.SelectedIndex;
        }

        private void frmSetting_KeyDown(object sender, KeyEventArgs e) {
            // close dialog
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt) {
                Close();
            }
        }

        private void frmSetting_SizeChanged(object sender, EventArgs e) {
            Refresh();
        }

        #endregion

        /// <summary>
        /// Load language pack
        /// </summary>
        private void LoadLanguagePack() {
            var lang = Configs.Language.Items;

            RightToLeft = Configs.Language.IsRightToLeftLayout;
            Text = lang[$"{Name}._Text"];

            #region Tabs label
            lblGeneral.Text = lang[$"{Name}.{nameof(lblGeneral)}"];
            lblImage.Text = lang[$"{Name}.{nameof(lblImage)}"];
            lblEdit.Text = lang[$"{Name}.{nameof(lblEdit)}"];
            lblFileTypeAssoc.Text = lang[$"{Name}.{nameof(lblFileTypeAssoc)}"];
            lblLanguage.Text = lang[$"{Name}.{nameof(lblLanguage)}"];
            lblToolbar.Text = lang[$"{Name}.{nameof(lblToolbar)}"];
            lblTools.Text = lang["frmMain.mnuMainTools"];
            lblTheme.Text = lang[$"{Name}.{nameof(lblTheme)}"];
            lblKeyboard.Text = lang[$"{Name}.{nameof(lblKeyboard)}"];

            btnSave.Text = lang[$"{Name}.{nameof(btnSave)}"];
            btnCancel.Text = lang[$"{Name}.{nameof(btnCancel)}"];
            btnApply.Text = lang[$"{Name}.{nameof(btnApply)}"];
            #endregion

            #region GENERAL TAB
            // Startup
            lblHeadStartup.Text = lang[$"{Name}.{nameof(lblHeadStartup)}"];
            chkWelcomePicture.Text = lang[$"{Name}.{nameof(chkWelcomePicture)}"];
            chkLastSeenImage.Text = lang[$"{Name}.{nameof(chkLastSeenImage)}"];
            chkShowToolBar.Text = lang[$"{Name}.{nameof(chkShowToolBar)}"];
            chkAllowMultiInstances.Text = lang[$"{Name}.{nameof(chkAllowMultiInstances)}"];

            // Configuration
            lblHeadConfigDir.Text = lang[$"{Name}.{nameof(lblHeadConfigDir)}"];
            lnkConfigDir.Text = App.ConfigDir(PathType.Dir);

            // Viewer
            lblHeadViewer.Text = lang[$"{Name}.{nameof(lblHeadViewer)}"];
            chkShowScrollbar.Text = lang[$"{Name}.{nameof(chkShowScrollbar)}"];
            chkShowNavButtons.Text = lang[$"{Name}.{nameof(chkShowNavButtons)}"];
            chkDisplayBasename.Text = lang[$"{Name}.{nameof(chkDisplayBasename)}"];
            chkShowCheckerboardOnlyImage.Text = lang[$"{Name}.{nameof(chkShowCheckerboardOnlyImage)}"];
            chkUseTouchGesture.Text = lang[$"{Name}.{nameof(chkUseTouchGesture)}"];
            lblBackGroundColor.Text = lang[$"{Name}.{nameof(lblBackGroundColor)}"];
            lnkResetBackgroundColor.Text = lang[$"{Name}.{nameof(lnkResetBackgroundColor)}"];

            // Others
            lblHeadOthers.Text = lang[$"{Name}.{nameof(lblHeadOthers)}"];
            chkAutoUpdate.Text = lang[$"{Name}.{nameof(chkAutoUpdate)}"];
            chkESCToQuit.Text = lang[$"{Name}.{nameof(chkESCToQuit)}"];
            chkConfirmationDelete.Text = lang[$"{Name}.{nameof(chkConfirmationDelete)}"];
            chkCenterWindowFit.Text = lang[$"{Name}.{nameof(chkCenterWindowFit)}"];
            chkShowToast.Text = lang[$"{Name}.{nameof(chkShowToast)}"];

            #endregion

            #region IMAGE TAB
            lblHeadImageLoading.Text = lang[$"{Name}.{nameof(lblHeadImageLoading)}"];//
            chkFindChildFolder.Text = lang[$"{Name}.{nameof(chkFindChildFolder)}"];
            chkShowHiddenImages.Text = lang[$"{Name}.{nameof(chkShowHiddenImages)}"];
            chkLoopViewer.Text = lang[$"{Name}.{nameof(chkLoopViewer)}"];
            chkIsCenterImage.Text = lang[$"{Name}.{nameof(chkIsCenterImage)}"];
            lblImageLoadingOrder.Text = lang[$"{Name}.{nameof(lblImageLoadingOrder)}"];
            chkUseFileExplorerSortOrder.Text = lang[$"{Name}.{nameof(chkUseFileExplorerSortOrder)}"];
            chkGroupByDirectory.Text = lang[$"{Name}.{nameof(chkGroupByDirectory)}"];
            lblImageBoosterCachedCount.Text = lang[$"{Name}.{nameof(lblImageBoosterCachedCount)}"];

            lblColorManagement.Text = lang[$"{Name}.{nameof(lblColorManagement)}"];//
            chkApplyColorProfile.Text = lang[$"{Name}.{nameof(chkApplyColorProfile)}"];
            lblColorProfile.Text = lang[$"{Name}.{nameof(lblColorProfile)}"];
            lnkColorProfileBrowse.Text = lang[$"{Name}.{nameof(lnkColorProfileBrowse)}"];

            lblHeadMouseWheelActions.Text = lang[$"{Name}.{nameof(lblHeadMouseWheelActions)}"];
            lblMouseWheel.Text = lang[$"{Name}.{nameof(lblMouseWheel)}"];
            lblMouseWheelAlt.Text = lang[$"{Name}.{nameof(lblMouseWheelAlt)}"];
            lblMouseWheelCtrl.Text = lang[$"{Name}.{nameof(lblMouseWheelCtrl)}"];
            lblMouseWheelShift.Text = lang[$"{Name}.{nameof(lblMouseWheelShift)}"];

            lblHeadZooming.Text = lang[$"{Name}.{nameof(lblHeadZooming)}"];//
            lblGeneral_ZoomOptimization.Text = lang[$"{Name}.{nameof(lblGeneral_ZoomOptimization)}"];
            lblZoomLevels.Text = lang[$"{Name}.{nameof(lblZoomLevels)}"];

            lblHeadThumbnailBar.Text = lang[$"{Name}.{nameof(lblHeadThumbnailBar)}"];//
            chkThumbnailVertical.Text = lang[$"{Name}.{nameof(chkThumbnailVertical)}"];
            chkShowThumbnailScrollbar.Text = lang[$"{Name}.{nameof(chkShowThumbnailScrollbar)}"];
            lblGeneral_ThumbnailSize.Text = lang[$"{Name}.{nameof(lblGeneral_ThumbnailSize)}"];

            lblHeadSlideshow.Text = lang[$"{Name}.{nameof(lblHeadSlideshow)}"];//
            chkLoopSlideshow.Text = lang[$"{Name}.{nameof(chkLoopSlideshow)}"];
            chkRandomSlideshowInterval.Text = lang[$"{Name}.{nameof(chkRandomSlideshowInterval)}"];
            chkShowSlideshowCountdown.Text = lang[$"{Name}.{nameof(chkShowSlideshowCountdown)}"];
            lblSlideshowIntervalTo.Text = lang[$"{Name}.{nameof(lblSlideshowIntervalTo)}"];
            numSlideShowInterval_ValueChanged(null, null); // format interval value

            #endregion

            #region EDIT TAB
            chkSaveOnRotate.Text = lang[$"{Name}.{nameof(chkSaveOnRotate)}"];
            chkSaveModifyDate.Text = lang[$"{Name}.{nameof(chkSaveModifyDate)}"];
            lblAfterEditingApp.Text = lang[$"{Name}.{nameof(lblAfterEditingApp)}"];
            lblImageQuality.Text = lang[$"{Name}.{nameof(lblImageQuality)}"];

            lblSelectAppForEdit.Text = lang[$"{Name}.{nameof(lblSelectAppForEdit)}"];
            btnEditEditExt.Text = lang[$"{Name}.{nameof(btnEditEditExt)}"];
            btnEditResetExt.Text = lang[$"{Name}.{nameof(btnEditResetExt)}"];
            btnEditEditAllExt.Text = lang[$"{Name}.{nameof(btnEditEditAllExt)}"];
            clnFileExtension.Text = lang[$"{Name}.{nameof(lvImageEditing)}.clnFileExtension"];
            clnAppName.Text = lang[$"{Name}.{nameof(lvImageEditing)}.clnAppName"];
            clnAppPath.Text = lang[$"{Name}.{nameof(lvImageEditing)}.clnAppPath"];
            clnAppArguments.Text = lang[$"{Name}.{nameof(lvImageEditing)}.clnAppArguments"];
            #endregion

            #region FILE TYPE ASSOCIATION TAB

            lblSupportedExtension.Text = string.Format(lang[$"{Name}.{nameof(lblSupportedExtension)}"], Configs.AllFormats.Count);
            lnkOpenFileAssoc.Text = lang[$"{Name}.{nameof(lnkOpenFileAssoc)}"];
            btnAddNewExt.Text = lang[$"{Name}.{nameof(btnAddNewExt)}"];
            btnDeleteExt.Text = lang[$"{Name}.{nameof(btnDeleteExt)}"];
            btnResetExt.Text = lang[$"{Name}.{nameof(btnResetExt)}"];
            btnRegisterExt.Text = lang[$"{Name}.{nameof(btnRegisterExt)}"];
            btnUnregisterExt.Text = lang[$"{Name}.{nameof(btnUnregisterExt)}"];
            #endregion

            #region LANGUAGE TAB
            lblLanguageText.Text = lang[$"{Name}.{nameof(lblLanguageText)}"];
            lnkRefresh.Text = lang[$"{Name}.{nameof(lnkRefresh)}"];
            lblLanguageWarning.Text = string.Format(lang[$"{Name}.{nameof(lblLanguageWarning)}"], "ImageGlass " + Application.ProductVersion);
            lnkInstallLanguage.Text = lang[$"{Name}.{nameof(lnkInstallLanguage)}"];
            lnkCreateNew.Text = lang[$"{Name}.{nameof(lnkCreateNew)}"];
            lnkEdit.Text = lang[$"{Name}.{nameof(lnkEdit)}"];
            lnkGetMoreLanguage.Text = lang[$"{Name}.{nameof(lnkGetMoreLanguage)}"];
            #endregion

            #region TOOLBAR TAB
            lblToolbarPosition.Text = lang[$"{Name}.{nameof(lblToolbarPosition)}"];
            lblToolbarIconHeight.Text = lang[$"{Name}.{nameof(lblToolbarIconHeight)}"];
            chkHorzCenterToolbarBtns.Text = lang[$"{Name}.{nameof(chkHorzCenterToolbarBtns)}"];
            chkHideTooltips.Text = lang[$"{Name}.{nameof(chkHideTooltips)}"];

            _separatorText = lang[$"{Name}._separator"];
            lblUsedBtns.Text = lang[$"{Name}.{nameof(lblUsedBtns)}"];
            lblAvailBtns.Text = lang[$"{Name}.{nameof(lblAvailBtns)}"];

            tip1.SetToolTip(lblToolbar, lang[$"{Name}.{nameof(lblToolbar)}._Tooltip"]);
            tip1.SetToolTip(btnMoveUp, lang[$"{Name}.{nameof(btnMoveUp)}._Tooltip"]);
            tip1.SetToolTip(btnMoveDown, lang[$"{Name}.{nameof(btnMoveDown)}._Tooltip"]);
            tip1.SetToolTip(btnMoveLeft, lang[$"{Name}.{nameof(btnMoveLeft)}._Tooltip"]);
            tip1.SetToolTip(btnMoveRight, lang[$"{Name}.{nameof(btnMoveRight)}._Tooltip"]);
            #endregion

            #region TOOLS TAB
            lblColorPicker.Text = lang[$"{nameof(frmMain)}.mnuMainColorPicker"];
            chkColorUseRGBA.Text = lang[$"{Name}.{nameof(chkColorUseRGBA)}"];
            chkColorUseHEXA.Text = lang[$"{Name}.{nameof(chkColorUseHEXA)}"];
            chkColorUseHSLA.Text = lang[$"{Name}.{nameof(chkColorUseHSLA)}"];
            chkColorUseHSVA.Text = lang[$"{Name}.{nameof(chkColorUseHSVA)}"];

            lblPageNav.Text = lang[$"{nameof(frmMain)}.mnuMainPageNav"];
            chkShowPageNavAuto.Text = lang[$"{Name}.{nameof(chkShowPageNavAuto)}"];

            lblExifTool.Text = lang[$"{nameof(frmMain)}.mnuExifTool"] + " (https://exiftool.org)";
            chkExifToolAlwaysOnTop.Text = lang[$"{Name}.{nameof(chkExifToolAlwaysOnTop)}"];
            lnkSelectExifTool.Text = lang[$"{Name}.{nameof(lnkSelectExifTool)}"];
            #endregion

            #region THEME TAB
            lblInstalledThemes.Text = string.Format(lang[$"{this.Name}.{nameof(lblInstalledThemes)}"], "");
            lnkThemeDownload.Text = lang[$"{this.Name}.{nameof(lnkThemeDownload)}"];

            btnThemeRefresh.Text = lang[$"{this.Name}.{nameof(btnThemeRefresh)}"];
            btnThemeInstall.Text = lang[$"{this.Name}.{nameof(btnThemeInstall)}"];
            btnThemeUninstall.Text = lang[$"{this.Name}.{nameof(btnThemeUninstall)}"];
            btnThemeSaveAs.Text = lang[$"{this.Name}.{nameof(btnThemeSaveAs)}"];
            btnThemeFolderOpen.Text = lang[$"{this.Name}.{nameof(btnThemeFolderOpen)}"];

            btnThemeApply.Text = lang[$"{this.Name}.{nameof(btnThemeApply)}"];

            #endregion

            #region KEYBOARD TAB
            btnKeyReset.Text = lang[$"{Name}.{nameof(btnKeyReset)}"];
            lblKeysSpaceBack.Text = lang[$"{Name}.{nameof(lblKeysSpaceBack)}"];
            lblKeysPageUpDown.Text = lang[$"{Name}.{nameof(lblKeysPageUpDown)}"];
            lblKeysUpDown.Text = lang[$"{Name}.{nameof(lblKeysUpDown)}"];
            lblKeysLeftRight.Text = lang[$"{Name}.{nameof(lblKeysLeftRight)}"];
            #endregion

        }

        /// <summary>
        /// TAB LABEL CLICK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblMenu_Click(object sender, EventArgs e) {
            var lbl = (Label)sender;

            switch (lbl.Name) {
                case nameof(lblGeneral):
                    tab1.SelectedTab = tabGeneral;
                    break;
                case nameof(lblImage):
                    tab1.SelectedTab = tabImage;
                    break;
                case nameof(lblEdit):
                    tab1.SelectedTab = tabEdit;
                    break;
                case nameof(lblFileTypeAssoc):
                    tab1.SelectedTab = tabFileTypeAssoc;
                    break;
                case nameof(lblLanguage):
                    tab1.SelectedTab = tabLanguage;
                    break;
                case nameof(lblToolbar):
                    tab1.SelectedTab = tabToolbar;
                    break;
                case nameof(lblTools):
                    tab1.SelectedTab = tabTools;
                    break;
                case nameof(lblTheme):
                    tab1.SelectedTab = tabTheme;
                    break;
                case nameof(lblKeyboard):
                    tab1.SelectedTab = tabKeyboard;
                    break;
            }
        }

        private void tab1_SelectedIndexChanged(object sender, EventArgs e) {
            lblGeneral.Tag =
            lblImage.Tag =
            lblEdit.Tag =
            lblFileTypeAssoc.Tag =
            lblLanguage.Tag =
            lblToolbar.Tag =
            lblTools.Tag =
            lblTheme.Tag =
            lblKeyboard.Tag = 0;

            lblGeneral.BackColor =
            lblImage.BackColor =
            lblEdit.BackColor =
            lblFileTypeAssoc.BackColor =
            lblLanguage.BackColor =
            lblToolbar.BackColor =
            lblTools.BackColor =
            lblTheme.BackColor =
            lblKeyboard.BackColor = M_COLOR_MENU_NORMAL;

            if (tab1.SelectedTab == tabGeneral) {
                lblGeneral.Tag = 1;
                lblGeneral.BackColor = M_COLOR_MENU_SELECTED;

                LoadTabGeneralConfig();
            }
            else if (tab1.SelectedTab == tabImage) {
                lblImage.Tag = 1;
                lblImage.BackColor = M_COLOR_MENU_SELECTED;

                LoadTabImageConfig();
            }
            else if (tab1.SelectedTab == tabEdit) {
                lblEdit.Tag = 1;
                lblEdit.BackColor = M_COLOR_MENU_SELECTED;

                LoadTabEditConfig();
            }
            else if (tab1.SelectedTab == tabFileTypeAssoc) {
                lblFileTypeAssoc.Tag = 1;
                lblFileTypeAssoc.BackColor = M_COLOR_MENU_SELECTED;

                lvExtension.TileSize = new Size(100, DPIScaling.Transform(30));

                // Load image formats to the list
                LoadExtensionList();

                SystemRenderer.ApplyTheme(lvExtension);
            }
            else if (tab1.SelectedTab == tabLanguage) {
                lblLanguage.Tag = 1;
                lblLanguage.BackColor = M_COLOR_MENU_SELECTED;

                lnkRefresh_LinkClicked(null, null);
            }
            else if (tab1.SelectedTab == tabToolbar) {
                lblToolbar.Tag = 1;
                lblToolbar.BackColor = M_COLOR_MENU_SELECTED;

                LoadTabToolbar();
            }
            else if (tab1.SelectedTab == tabTools) {
                lblTools.Tag = 1;
                lblTools.BackColor = M_COLOR_MENU_SELECTED;

                LoadTabTools();
            }
            else if (tab1.SelectedTab == tabTheme) {
                lblTheme.Tag = 1;
                lblTheme.BackColor = M_COLOR_MENU_SELECTED;

                LoadTabTheme();
            }
            else if (tab1.SelectedTab == tabKeyboard) {
                lblKeyboard.Tag = 1;
                lblKeyboard.BackColor = M_COLOR_MENU_SELECTED;

                LoadTabKeyboard(Configs.KeyComboActions);
            }
        }

        #region TAB GENERAL

        /// <summary>
        /// Get and load value of General tab
        /// </summary>
        private void LoadTabGeneralConfig() {
            chkLastSeenImage.Checked = Configs.IsOpenLastSeenImage;
            chkWelcomePicture.Checked = Configs.IsShowWelcome;
            chkShowToolBar.Checked = Configs.IsShowToolBar;
            chkAutoUpdate.Checked = Configs.AutoUpdate != "0";
            chkAllowMultiInstances.Checked = Configs.IsAllowMultiInstances;
            chkESCToQuit.Checked = Configs.IsPressESCToQuit;
            chkConfirmationDelete.Checked = Configs.IsConfirmationDelete;
            chkShowScrollbar.Checked = Configs.IsScrollbarsVisible;
            chkDisplayBasename.Checked = Configs.IsDisplayBasenameOfImage;
            chkShowNavButtons.Checked = Configs.IsShowNavigationButtons;
            chkShowCheckerboardOnlyImage.Checked = Configs.IsShowCheckerboardOnlyImageRegion;
            chkCenterWindowFit.Checked = Configs.IsCenterWindowFit;
            chkShowToast.Checked = Configs.IsShowToast;
            chkUseTouchGesture.Checked = Configs.IsUseTouchGesture;
            picBackgroundColor.BackColor = Configs.BackgroundColor;
        }

        private void lnkConfigDir_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("explorer.exe", App.ConfigDir(PathType.Dir));
        }

        private void picBackgroundColor_Click(object sender, EventArgs e) {
            var c = new ColorDialog() {
                AllowFullOpen = true
            };

            // Initialize to the existing color value instead of default black
            c.Color = picBackgroundColor.BackColor;
            c.FullOpen = true;

            if (c.ShowDialog() == DialogResult.OK) {
                picBackgroundColor.BackColor = c.Color;
            }
        }

        private void lnkResetBackgroundColor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            picBackgroundColor.BackColor = Configs.Theme.BackgroundColor;
        }

        #endregion

        #region TAB IMAGE

        /// <summary>
        /// Get and load value of Image tab
        /// </summary>
        private void LoadTabImageConfig() {
            // Set value of chkFindChildFolder ---------------------------------------------
            chkFindChildFolder.Checked = Configs.IsRecursiveLoading;

            // Set value of chkShowHiddenImages
            chkShowHiddenImages.Checked = Configs.IsShowingHiddenImages;

            // Set value of chkLoopViewer
            chkLoopViewer.Checked = Configs.IsLoopBackViewer;

            // Set value of chkIsCenterImage
            chkIsCenterImage.Checked = Configs.IsCenterImage;

            // Set value of chkUseFileExplorerSortOrder
            chkUseFileExplorerSortOrder.Checked = Configs.IsUseFileExplorerSortOrder;

            // Set value of chkGroupByDirectory
            chkGroupByDirectory.Checked = Configs.IsGroupImagesByDirectory;

            #region Load items of cmbImageOrder
            var loadingOrderList = Enum.GetNames(typeof(ImageOrderBy));
            cmbImageOrder.Items.Clear();

            foreach (var item in loadingOrderList) {
                cmbImageOrder.Items.Add(Configs.Language.Items[$"_.{nameof(ImageOrderBy)}._{item}"]);
            }

            //Get value of cmbImageOrder
            cmbImageOrder.SelectedIndex = (int)Configs.ImageLoadingOrder;
            #endregion

            #region Load items of cmbImageOrderType
            var orderTypesList = Enum.GetNames(typeof(ImageOrderType));
            cmbImageOrderType.Items.Clear();

            foreach (var item in orderTypesList) {
                cmbImageOrderType.Items.Add(Configs.Language.Items[$"_.{nameof(ImageOrderType)}._{item}"]);
            }

            //Get value of cmbImageOrder
            cmbImageOrderType.SelectedIndex = (int)Configs.ImageLoadingOrderType;
            #endregion

            // Set value of cmbImageBoosterCachedCount
            cmbImageBoosterCachedCount.SelectedIndex = (int)Configs.ImageBoosterCachedCount;

            #region Color Management
            chkApplyColorProfile.Checked = Configs.IsApplyColorProfileForAll;

            // color profile list
            cmbColorProfile.Items.Clear();
            cmbColorProfile.Items.Add(Configs.Language.Items[$"{Name}.cmbColorProfile._None"]);
            cmbColorProfile.Items.AddRange(Heart.Helpers.GetBuiltInColorProfiles());
            cmbColorProfile.Items.Add(Configs.Language.Items[$"{Name}.cmbColorProfile._CustomProfileFile"]); // always last position

            // select the color profile
            if (File.Exists(Configs.ColorProfile)) {
                cmbColorProfile.SelectedIndex = cmbColorProfile.Items.Count - 1;
                lnkColorProfilePath.Text = Configs.ColorProfile;

                lnkColorProfileBrowse.Visible = true;
                lnkColorProfilePath.Visible = true;
            }
            else {
                // first item selected default
                cmbColorProfile.SelectedIndex = 0;

                for (var i = 0; i < cmbColorProfile.Items.Count; i++) {
                    if (cmbColorProfile.Items[i].ToString() == Configs.ColorProfile) {
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

            foreach (var item in Enum.GetNames(typeof(MouseWheelActions))) {
                cmbMouseWheel.Items.Add(Configs.Language.Items[$"{this.Name}.cmbMouseWheel._{item}"]);
                cmbMouseWheelCtrl.Items.Add(Configs.Language.Items[$"{this.Name}.cmbMouseWheel._{item}"]);
                cmbMouseWheelShift.Items.Add(Configs.Language.Items[$"{this.Name}.cmbMouseWheel._{item}"]);
                cmbMouseWheelAlt.Items.Add(Configs.Language.Items[$"{this.Name}.cmbMouseWheel._{item}"]);
            }

            //Get value of cmbMouseWheel
            cmbMouseWheel.SelectedIndex = (int)Configs.MouseWheelAction;

            //Get value of cmbMouseWheelCtrl
            cmbMouseWheelCtrl.SelectedIndex = (int)Configs.MouseWheelCtrlAction;

            //Get value of cmbMouseWheelShift
            cmbMouseWheelShift.SelectedIndex = (int)Configs.MouseWheelShiftAction;

            //Get value of cmbMouseWheelAlt
            cmbMouseWheelAlt.SelectedIndex = (int)Configs.MouseWheelAltAction;

            #endregion

            #region Zooming

            // Load items of cmbZoomOptimization
            var zoomOptimizationList = Enum.GetNames(typeof(ZoomOptimizationMethods));
            cmbZoomOptimization.Items.Clear();
            foreach (var item in zoomOptimizationList) {
                cmbZoomOptimization.Items.Add(Configs.Language.Items[$"{this.Name}.cmbZoomOptimization._{item}"]);
            }

            // Get value of cmbZoomOptimization
            cmbZoomOptimization.SelectedIndex = (int)Configs.ZoomOptimizationMethod;

            // Load zoom levels text
            txtZoomLevels.Text = Helpers.IntArrayToString(Configs.ZoomLevels);

            #endregion

            // Thumbnail bar on right side ----------------------------------------------------
            chkThumbnailVertical.Checked = !Configs.IsThumbnailHorizontal;

            // Show thumbnail scrollbar
            chkShowThumbnailScrollbar.Checked = Configs.IsShowThumbnailScrollbar;

            // load thumbnail dimension
            cmbThumbnailDimension.SelectedItem = Configs.ThumbnailDimension.ToString();

            // Set value of chkLoopSlideshow --------------------------------------------------
            chkLoopSlideshow.Checked = Configs.IsLoopBackSlideshow;
            chkShowSlideshowCountdown.Checked = Configs.IsShowSlideshowCountdown;
            chkRandomSlideshowInterval.Checked = Configs.IsRandomSlideshowInterval;

            // Set value of slideshow intervals
            numSlideShowInterval.Value = Configs.SlideShowInterval;
            numSlideshowIntervalTo.Value = Configs.SlideShowIntervalTo;
            numSlideShowInterval_ValueChanged(null, null); // format interval value
        }

        private void chkRandomSlideshowInterval_CheckedChanged(object sender, EventArgs e) {
            lblSlideshowIntervalTo.Visible =
                numSlideshowIntervalTo.Visible =
                chkRandomSlideshowInterval.Checked;
        }

        private void numSlideShowInterval_ValueChanged(object sender, EventArgs e) {
            // set the minimum value of To
            numSlideshowIntervalTo.Minimum = numSlideShowInterval.Value;

            // format value
            var time = TimeSpan.FromSeconds((double)numSlideShowInterval.Value).ToString("mm':'ss");

            if (chkRandomSlideshowInterval.Checked) {
                var timeTo = TimeSpan.FromSeconds((double)numSlideshowIntervalTo.Value).ToString("mm':'ss");

                time = $"{time} - {timeTo}";
            }

            lblSlideshowInterval.Text = string.Format(Configs.Language.Items[$"{Name}.lblSlideshowInterval"], time);
        }

        private void numSlideshowIntervalTo_ValueChanged(object sender, EventArgs e) {
            // format value
            var time = TimeSpan.FromSeconds((double)numSlideShowInterval.Value).ToString("mm':'ss");

            var timeTo = TimeSpan.FromSeconds((double)numSlideshowIntervalTo.Value).ToString("mm':'ss");

            time = $"{time} - {timeTo}";

            lblSlideshowInterval.Text = string.Format(Configs.Language.Items[$"{Name}.lblSlideshowInterval"], time);
        }

        private void cmbColorProfile_SelectedIndexChanged(object sender, EventArgs e) {
            // if Custom ICC/ICM profile file selected
            if (cmbColorProfile.SelectedIndex == cmbColorProfile.Items.Count - 1) {
                // show the Browse and ICC path link
                lnkColorProfileBrowse.Visible = true;
                lnkColorProfilePath.Visible = true;
            }
            else {
                lnkColorProfileBrowse.Visible = false;
                lnkColorProfilePath.Visible = false;
            }
        }

        private void lnkColorProfileBrowse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            var o = new OpenFileDialog() {
                Filter = "Supported files|*.icc;*.icm;|All files|*.*",
                CheckFileExists = true,
            };

            if (o.ShowDialog() == DialogResult.OK) {
                lnkColorProfilePath.Text = o.FileName;
            }
        }

        private void lnkColorProfilePath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            if (File.Exists(lnkColorProfilePath.Text)) {
                Process.Start("explorer.exe", "/select,\"" +
                    lnkColorProfilePath.Text + "\"");
            }
        }

        // Part of issue #677: zoom levels MUST be positive integers, delimited by semicolons.
        // Prevent the user from entering anything else!
        private void TxtZoomLevels_KeyPress(object sender, KeyPressEventArgs e) {
            var keyval = e.KeyChar;
            var accept = char.IsDigit(keyval) ||
                          keyval == ';' ||
                          keyval == (char)Keys.Back ||
                          keyval == (char)Keys.Space;
            if (!accept)
                e.Handled = true;
        }

        #endregion

        #region TAB EDIT

        /// <summary>
        /// Get and load value of tab Edit
        /// </summary>
        private void LoadTabEditConfig() {
            chkSaveOnRotate.Checked = Configs.IsSaveAfterRotating;
            chkSaveModifyDate.Checked = Configs.IsPreserveModifiedDate;
            numImageQuality.Value = Configs.ImageEditQuality;

            #region Load items of cmbAfterEditingApp
            var actionsList = Enum.GetNames(typeof(AfterOpeningEditAppAction));
            cmbAfterEditingApp.Items.Clear();

            foreach (var item in actionsList) {
                cmbAfterEditingApp.Items.Add(Configs.Language.Items[$"_.{nameof(AfterOpeningEditAppAction)}._{item}"]);
            }

            // Get value of cmbAfterEditingApp
            cmbAfterEditingApp.SelectedIndex = (int)Configs.AfterEditingAction;
            #endregion


            // Load image editing apps list
            LoadEditApps();

            SystemRenderer.ApplyTheme(lvImageEditing);
        }

        /// <summary>
        /// Load image editing apps list
        /// </summary>
        /// <param name="isResetToDefault">True to reset the list to default (empty)</param>
        private void LoadEditApps(bool isResetToDefault = false) {
            lvImageEditing.Items.Clear();
            var newEditingAssocList = new List<EditApp>();

            foreach (var ext in Configs.AllFormats) {
                var li = new ListViewItem {
                    Text = ext
                };

                // Build new list
                var newEditingAssoc = new EditApp() {
                    Extension = ext
                };

                if (!isResetToDefault) {
                    // Find the extension in the settings
                    var editingExt = Configs.EditApps.Find(item => item?.Extension == ext);

                    li.SubItems.Add(editingExt?.AppName);
                    li.SubItems.Add(editingExt?.AppPath);
                    li.SubItems.Add(editingExt?.AppArguments);

                    // Build new list
                    newEditingAssoc.AppName = editingExt?.AppName;
                    newEditingAssoc.AppPath = editingExt?.AppPath;
                    newEditingAssoc.AppArguments = editingExt?.AppArguments;
                }

                newEditingAssocList.Add(newEditingAssoc);
                lvImageEditing.Items.Add(li);
            }

            // Update the new full list
            Configs.EditApps = newEditingAssocList;
        }

        private void btnEditResetExt_Click(object sender, EventArgs e) {
            LoadEditApps(true);
        }

        private void btnEditEditExt_Click(object sender, EventArgs e) {
            if (lvImageEditing.SelectedItems.Count == 0)
                return;

            // Get select Association item
            var assoc = Configs.GetEditApp(lvImageEditing.SelectedItems[0].Text);

            if (assoc == null)
                return;

            using var frm = new frmEditApp() {
                FileExtension = assoc.Extension,
                AppName = assoc.AppName,
                AppPath = assoc.AppPath,
                AppArguments = assoc.AppArguments,
                TopMost = this.TopMost
            };
            if (frm.ShowDialog() == DialogResult.OK) {
                assoc.AppName = frm.AppName;
                assoc.AppPath = frm.AppPath;
                assoc.AppArguments = frm.AppArguments;

                LoadEditApps();
            }
        }

        private void btnEditEditAllExt_Click(object sender, EventArgs e) {
            using var frm = new frmEditApp() {
                FileExtension = $"<{string.Format(Configs.Language.Items[$"{Name}._allExtensions"])}>",
                TopMost = this.TopMost
            };
            if (frm.ShowDialog() == DialogResult.OK) {
                foreach (var assoc in Configs.EditApps) {
                    assoc.AppName = frm.AppName;
                    assoc.AppPath = frm.AppPath;
                    assoc.AppArguments = frm.AppArguments;
                }

                LoadEditApps();
            }
        }

        private void lvlvImageEditing_SelectedIndexChanged(object sender, EventArgs e) {
            btnEditEditExt.Enabled = lvImageEditing.SelectedItems.Count > 0;
        }

        #endregion

        #region TAB LANGUAGES
        private void lnkGetMoreLanguage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start($"https://imageglass.org/languages?utm_source=app_{App.Version}&utm_medium=app_click&utm_campaign=app_languagepack");
            }
            catch { }
        }

        private void lnkInstallLanguage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            using var p = new Process();
            p.StartInfo.FileName = App.StartUpDir("igtasks.exe");
            p.StartInfo.Arguments = "iginstalllang";

            try {
                p.Start();
            }
            catch { }
        }

        private void lnkCreateNew_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            using var p = new Process();
            p.StartInfo.FileName = App.StartUpDir("igtasks.exe");
            p.StartInfo.Arguments = "ignewlang";

            try {
                p.Start();
            }
            catch { }
        }

        private void lnkEdit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            using var p = new Process();
            p.StartInfo.FileName = App.StartUpDir("igtasks.exe");
            p.StartInfo.Arguments = "igeditlang \"" + Configs.Language.FileName + "\"";

            try {
                p.Start();
            }
            catch { }
        }

        private async void lnkRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            cmbLanguage.Items.Clear();
            cmbLanguage.Items.Add("English");
            lstLanguages = new List<Language> {
                new Language()
            };

            var langPath = App.StartUpDir(Dir.Languages);

            if (Directory.Exists(langPath)) {
                await Task.Run(() => {
                    foreach (var f in Directory.GetFiles(langPath)) {
                        if (string.Equals(Path.GetExtension(f), ".iglang", StringComparison.CurrentCultureIgnoreCase)) {
                            var l = new Language(f);
                            lstLanguages.Add(l);
                        }
                    }
                }).ConfigureAwait(true);

                // start from 1, the first item is already hardcoded
                for (var i = 1; i < lstLanguages.Count; i++) {
                    var iLang = cmbLanguage.Items.Add(lstLanguages[i].LangName);
                    var curLang = Configs.Language.FileName;

                    // using current language pack
                    if (lstLanguages[i].FileName.CompareTo(curLang) == 0) {
                        cmbLanguage.SelectedIndex = iLang;
                    }
                }
            }

            if (cmbLanguage.SelectedIndex == -1) {
                cmbLanguage.SelectedIndex = 0;
            }
        }

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e) {
            lblLanguageWarning.Visible = false;

            // check compatibility
            var lang = new Language();
            if (lang.MinVersion.CompareTo(lstLanguages[cmbLanguage.SelectedIndex].MinVersion) != 0) {
                lblLanguageWarning.Visible = true;
            }
        }

        #endregion

        #region TAB FILE ASSOCIATIONS

        /// <summary>
        /// Load extensions from settings to the list view
        /// </summary>
        private void LoadExtensionList(bool resetFormatList = false) {
            lvExtension.Items.Clear();

            if (resetFormatList) {
                Configs.AllFormats = Configs.GetImageFormats(Constants.IMAGE_FORMATS);
            }

            foreach (var ext in Configs.AllFormats) {
                var extDisplay = ext.Substring(1).ToUpper();
                var li = new ListViewItem(ext);
                _ = li.SubItems.Add($"ImageGlass {extDisplay} File");

                lvExtension.Items.Add(li);
            }

            lblSupportedExtension.Text = string.Format(Configs.Language.Items[$"{Name}.lblSupportedExtension"], lvExtension.Items.Count);
        }

        /// <summary>
        /// Register file associations and Web-to-App linking
        /// </summary>
        /// <param name="resetFormatList">Set it to TRUE if you want to reset the formats list to default</param>
        private void RegisterFileAssociations(bool resetFormatList = false) {
            LoadExtensionList(resetFormatList);

            try {
                using var p = new Process();
                var isError = true;
                var formats = Configs.GetImageFormats(Configs.AllFormats);

                p.StartInfo.FileName = App.StartUpDir("igtasks.exe");
                p.StartInfo.Arguments = $"regassociations {formats}";
                p.Start();

                try {
                    p.Start();
                }
                catch {
                    // Clicking 'Cancel' in the "User Account Control" dialog throws a
                    // "User cancelled" exception. Just continue quietly in that case.
                    return;
                }

                p.WaitForExit();
                isError = p.ExitCode != 0;

                if (isError) {
                    MessageBox.Show(Configs.Language.Items[$"{Name}._RegisterAppExtensions_Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else {
                    MessageBox.Show(Configs.Language.Items[$"{Name}._RegisterAppExtensions_Success"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch { }
        }

        private void lnkOpenFileAssoc_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            // path to %windir%\system32\control.exe (ensures the correct control.exe)
            var controlpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "control.exe");

            Process.Start(controlpath, "/name Microsoft.DefaultPrograms /page pageFileAssoc");
        }

        private void btnResetExt_Click(object sender, EventArgs e) {
            RegisterFileAssociations(true);
        }

        private void btnDeleteExt_Click(object sender, EventArgs e) {
            if (lvExtension.SelectedItems.Count == 0)
                return;

            // Get checked extensions in the list then
            // remove extensions from settings
            foreach (ListViewItem li in lvExtension.SelectedItems) {
                Configs.AllFormats.Remove(li.Text);
            }

            // update the list
            LoadExtensionList();

            // Request frmMain to update
            Local.ForceUpdateActions |= ForceUpdateActions.IMAGE_LIST;
        }

        private void btnAddNewExt_Click(object sender, EventArgs e) {
            using var frm = new frmAddNewFormat() {
                FileFormat = ".svg",
                TopMost = this.TopMost
            };
            if (frm.ShowDialog() == DialogResult.OK) {
                // If the ext exist
                if (Configs.AllFormats.Contains(frm.FileFormat))
                    return;

                Configs.AllFormats.Add(frm.FileFormat);

                // update the list
                LoadExtensionList();

                // Request frmMain to update
                Local.ForceUpdateActions |= ForceUpdateActions.IMAGE_LIST;
            }
        }

        private void btnRegisterExt_Click(object sender, EventArgs e) {
            RegisterFileAssociations();
        }

        private void BtnUnregisteredExt_Click(object sender, EventArgs e) {
            try {
                using var p = new Process();
                var isError = true;

                p.StartInfo.FileName = App.StartUpDir("igtasks.exe");
                p.StartInfo.Arguments = $"delassociations";
                p.Start();

                try {
                    p.Start();
                }
                catch {
                    // Clicking 'Cancel' in the "User Account Control" dialog throws a
                    // "User cancelled" exception. Just continue quietly in that case.
                    return;
                }

                p.WaitForExit();
                isError = p.ExitCode != 0;

                if (isError) {
                    MessageBox.Show(Configs.Language.Items[$"{Name}._UnregisterAppExtensions_Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else {
                    MessageBox.Show(Configs.Language.Items[$"{Name}._UnregisterAppExtensions_Success"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch { }

        }

        private void lvExtension_SelectedIndexChanged(object sender, EventArgs e) {
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
        4. Add a new enum to ToolbarButtons (see Enums.cs), with the same name as the field name assigned in
           the step above (e.g. "btnRename"). The new enum goes BEFORE the MAX entry.

        The new toolbar button will now be available, the user would use the toolbar config
        tab to add it to their toolbar settings.
        */

        private void LoadTabToolbar() {
            var lang = Configs.Language.Items;

            // Load toolbar position
            cmbToolbarPosition.Items.Clear();
            foreach (var pos in Enum.GetNames(typeof(ToolbarPosition))) {
                cmbToolbarPosition.Items.Add(lang[$"{this.Name}.cmbToolbarPosition._{pos}"]);
            }

            cmbToolbarPosition.SelectedIndex = (int)Configs.ToolbarPosition;
            chkHorzCenterToolbarBtns.Checked = Configs.IsCenterToolbar;
            chkHideTooltips.Checked = Configs.IsHideTooltips;
            numToolbarIconHeight.Value = (int)Configs.ToolbarIconHeight;

            // Apply Windows System theme to listview
            SystemRenderer.ApplyTheme(lvAvailButtons);
            SystemRenderer.ApplyTheme(lvUsedButtons);

            // Apply ImageGlass theme to buttons list
            lvAvailButtons.BackColor = lvUsedButtons.BackColor = Configs.Theme.ToolbarBackgroundColor;
            lvAvailButtons.ForeColor = lvUsedButtons.ForeColor = Configs.Theme.TextInfoColor;

            BuildToolbarImageList();
            LoadUsedToolbarBtnsList();
            LoadAvailableToolbarBtnsList();
            UpdateNavigationButtonsState();
        }

        /// <summary>
        /// Fetch all the toolbar images via reflection from the ToolStripButton
        /// instances in the frmMain instance. This is why the enum name MUST
        /// match the frmMain field name!
        /// </summary>
        private void BuildToolbarImageList() {
            if (_lstToolbarImg != null)
                return;

            var iconHeight = DPIScaling.Transform(Constants.DEFAULT_TOOLBAR_ICON_HEIGHT);
            _lstToolbarImg = new ImageList {
                ColorDepth = ColorDepth.Depth32Bit, // max out image quality
                ImageSize = new Size(iconHeight, iconHeight)
            };

            var mainType = typeof(frmMain);
            for (var i = 0; i < (int)ToolbarButton.MAX; i++) {
                var fieldName = ((ToolbarButton)i).ToString();

                try {
                    var info = mainType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

                    var btn = info.GetValue(MainInstance) as ToolStripButton;
                    _lstToolbarImg.Images.Add(btn.Image);
                }
                catch { }
            }
        }

        /// <summary>
        /// Build the list of "currently used" toolbar buttons
        /// </summary>
        private void LoadUsedToolbarBtnsList() {
            lvUsedButtons.View = View.Tile;
            lvUsedButtons.LargeImageList = _lstToolbarImg;

            lvUsedButtons.Items.Clear();

            _lstMasterUsed = new List<ListViewItem>(Configs.ToolbarButtons.Count);

            for (var i = 0; i < Configs.ToolbarButtons.Count; i++) {
                ListViewItem lvi;

                if (Configs.ToolbarButtons[i] == ToolbarButton.Separator) {
                    lvi = BuildSeparatorItem();
                }
                else {
                    lvi = BuildToolbarListItem(Configs.ToolbarButtons[i]);
                }

                _lstMasterUsed.Add(lvi);
            }

            lvUsedButtons.Items.AddRange(_lstMasterUsed.ToArray());
        }

        /// <summary>
        /// Build the list of "not currently used" toolbar buttons
        /// </summary>
        private void LoadAvailableToolbarBtnsList() {
            lvAvailButtons.View = View.Tile;
            lvAvailButtons.LargeImageList = _lstToolbarImg;

            lvAvailButtons.Items.Clear();

            for (var i = 0; i < (int)ToolbarButton.MAX; i++) {
                if (!Configs.ToolbarButtons.Contains((ToolbarButton)i)) {
                    lvAvailButtons.Items.Add(BuildToolbarListItem((ToolbarButton)i));
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
        private ListViewItem BuildToolbarListItem(ToolbarButton buttonType) {
            var lvi = new ListViewItem {
                ImageIndex = (int)buttonType,
                Tag = buttonType
            };

            var fieldName = buttonType.ToString();
            var mainType = typeof(frmMain);

            try {
                var info = mainType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                var btn = info.GetValue(MainInstance) as ToolStripButton;

                lvi.Text = lvi.ToolTipText = btn.ToolTipText;
            }
            catch (Exception) {
                return null;
            }

            return lvi;
        }

        /// <summary>
        /// Build Separator for Toolbar listview
        /// </summary>
        /// <returns></returns>
        private ListViewItem BuildSeparatorItem() {
            return new ListViewItem {
                Text = _separatorText,
                ToolTipText = _separatorText,
                Tag = ToolbarButton.Separator
            };
        }

        /// <summary>
        /// Update Navagation buttons of toolbar buttons list's state
        /// </summary>
        private void UpdateNavigationButtonsState() {
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
        private void ApplyToolbarChanges() {
            // User hasn't actually visited the toolbar tab, don't do anything!
            // (Discovered by clicking 'Save' w/o having visited the toolbar tab...
            if (lvUsedButtons.Items.Count == 0 && lvAvailButtons.Items.Count == 0)
                return;

            var list = new List<ToolbarButton>();
            foreach (ListViewItem item in lvUsedButtons.Items) {
                var btn = Configs.ConvertType<ToolbarButton>(item.Tag.ToString());
                list.Add(btn);
            }

            var oldSet = Configs.GetToolbarButtons(Configs.ToolbarButtons);
            var newSet = Configs.GetToolbarButtons(list);

            // Only make change if any
            if (newSet != oldSet) {
                Configs.ToolbarButtons = list;
                Local.ForceUpdateActions |= ForceUpdateActions.TOOLBAR;
            }
        }

        #region Events
        private void ButtonsListView_Resize(object sender, EventArgs e) {
            var lv = (ListView)sender;
            UpdateButtonsListViewItemSize(lv);
        }

        /// <summary>
        /// Make the list view item bigger, adapted to icon size
        /// </summary>
        /// <param name="lv"></param>
        private void UpdateButtonsListViewItemSize(ListView lv) {
            var width = (int)(lv.Width * 0.85); // reserve right gap for multiple selection
            var height = DPIScaling.Transform(Constants.DEFAULT_TOOLBAR_ICON_HEIGHT * 2);

            lv.TileSize = new Size(width, height);

            // TODO: Issue:
            // The Listview layout is broken when user shrinks the window
            // then click Maximize button
        }

        private void lvUsedButtons_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateNavigationButtonsState();
        }

        private void lvAvailButtons_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateNavigationButtonsState();
        }

        private void btnMoveRight_Click(object sender, EventArgs e) {
            // 'Move' the selected entry in the LEFT list to the bottom of the RIGHT list
            // An exception is 'separator' which always remains available in the left list.

            for (var i = 0; i < lvAvailButtons.SelectedItems.Count; i++) {
                var lvi = lvAvailButtons.SelectedItems[i];
                _lstMasterUsed.Add(lvi.Clone() as ListViewItem);
            }

            for (var i = lvAvailButtons.SelectedItems.Count - 1; i >= 0; i--) {
                var lvi = lvAvailButtons.SelectedItems[i];
                if ((ToolbarButton)lvi.Tag != ToolbarButton.Separator)
                    lvAvailButtons.Items.Remove(lvi);
            }

            lvAvailButtons.SelectedIndices.Clear();
            RebuildUsedButtonsList(_lstMasterUsed.Count - 1);
            lvUsedButtons.EnsureVisible(_lstMasterUsed.Count - 1);
        }

        private void btnMoveLeft_Click(object sender, EventArgs e) {
            // 'Move' the selected entry in the RIGHT list to the bottom of the LEFT list
            // An exception is 'separator' which always remains available in the left list.

            for (var i = 0; i < lvUsedButtons.SelectedItems.Count; i++) {
                var lvi = lvUsedButtons.SelectedItems[i];
                if ((ToolbarButton)lvi.Tag != ToolbarButton.Separator)
                    lvAvailButtons.Items.Add(lvi.Clone() as ListViewItem);

                _lstMasterUsed.Remove(lvi);
            }

            RebuildUsedButtonsList(-1);
        }

        private void btnMoveDown_Click(object sender, EventArgs e) {
            MoveUsedEntry(+1);
        }

        private void btnMoveUp_Click(object sender, EventArgs e) {
            MoveUsedEntry(-1);
        }

        /// <summary>
        /// Move an item in the 'used' list
        /// </summary>
        /// <param name="delta"></param>
        private void MoveUsedEntry(int delta) {
            var currentIndex = lvUsedButtons.SelectedItems[0].Index;

            // do not wrap around
            if (delta < 0) {
                if (currentIndex <= 0)
                    return;
            }
            else {
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

        private void RebuildUsedButtonsList(int toSelect) {
            // This is annoying. To show the desired appearance in the listview, we need
            // to use 'SmallIcons' mode [image + text on a single line]. 'SmallIcons' mode
            // will NOT repaint the listview after changes to the Items list!!! Thus, we
            // teardown and rebuild the listview here.

            lvUsedButtons.BeginUpdate();
            lvUsedButtons.SelectedIndices.Clear();
            lvUsedButtons.Items.Clear();
            lvUsedButtons.Items.AddRange(_lstMasterUsed.ToArray());

            if (toSelect >= 0) {
                lvUsedButtons.Items[toSelect].Selected = true;
            }

            lvUsedButtons.EndUpdate();
        }

        #endregion

        #endregion

        #region TAB TOOLS
        private void LoadTabTools() {
            chkColorUseRGBA.Checked = Configs.IsColorPickerRGBA;
            chkColorUseHEXA.Checked = Configs.IsColorPickerHEXA;
            chkColorUseHSLA.Checked = Configs.IsColorPickerHSLA;
            chkColorUseHSVA.Checked = Configs.IsColorPickerHSVA;

            chkShowPageNavAuto.Checked = Configs.IsShowPageNavAuto;

            chkExifToolAlwaysOnTop.Checked = Configs.IsExifToolAlwaysOnTop;
            lblExifToolPath.Text = Configs.ExifToolExePath;
        }

        private void lnkSelectExifTool_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            var ofd = new OpenFileDialog() {
                CheckFileExists = true,
                Filter = "exiftool.exe file|*.exe",
            };

            if (ofd.ShowDialog() == DialogResult.OK) {
                var exif = new ExifToolWrapper(ofd.FileName);

                if (!exif.CheckExists()) {
                    var msg = string.Format(
                        Configs.Language.Items[$"{Name}.{nameof(lnkSelectExifTool)}._NotFound"],
                        ofd.FileName);

                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else {
                    Configs.ExifToolExePath =
                        lblExifToolPath.Text =
                        ofd.FileName;
                }
            }
        }

        #endregion

        #region TAB THEME
        private void LoadTabTheme() {
            if (lvTheme.Items.Count == 0) {
                SystemRenderer.ApplyTheme(lvTheme);

                _ = RefreshThemeListAsync();
            }
        }

        private async Task RefreshThemeListAsync() {
            lvTheme.Items.Clear();
            lvTheme.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            // get default theme dir
            var defaultThemePath = App.StartUpDir(Dir.Themes, Constants.DEFAULT_THEME, Theme.CONFIG_FILE);

            // load all theme packs, sorted the default theme first
            var lstThemes = (await Theme.GetAllThemePacksAsync())
                .OrderBy(i => i.ConfigFilePath != defaultThemePath)
                .ToList();

            // add themes to the listview
            for (var i = 0; i < lstThemes.Count(); i++) {
                var isDefault = i == 0;
                var th = lstThemes[i];
                var themePath = Path.GetDirectoryName(th.ConfigFilePath);
                var themeName = th.Name + (isDefault ? " ⭐⭐⭐" : "");

                var lvi = new ListViewItem(themeName) {
                    // store full path of theme
                    Tag = themePath,
                    ImageKey = "_blank",
                    ToolTipText = themePath,
                };

                if (Configs.Theme.ConfigFilePath == th.ConfigFilePath) {
                    lvi.Selected = true;
                    lvi.Checked = true;
                }

                lvTheme.Items.Add(lvi);
            }


            lvTheme.Enabled = true;
            this.Cursor = Cursors.Default;

            lblInstalledThemes.Text = string.Format(Configs.Language.Items[$"{Name}.lblInstalledThemes"], lvTheme.Items.Count.ToString());
        }

        private void btnThemeRefresh_Click(object sender, EventArgs e) {
            _ = RefreshThemeListAsync();
        }

        private void lvTheme_SelectedIndexChanged(object sender, EventArgs e) {
            var lang = Configs.Language.Items;

            if (lvTheme.SelectedIndices.Count > 0) {
                var defaultThemeDir = App.StartUpDir(Dir.Themes, Constants.DEFAULT_THEME);
                var themeDir = lvTheme.SelectedItems[0].Tag.ToString();
                var th = new Theme((int)Configs.ToolbarIconHeight, themeDir);

                btnThemeSaveAs.Enabled = true;
                btnThemeUninstall.Enabled = defaultThemeDir.CompareTo(themeDir) != 0;
                picPreview.BackgroundImage = th.PreviewImage.Image;

                txtThemeInfo.Text =
                    $"{lang[$"{Name}.txtThemeInfo._Name"]}: {th.Name}\r\n" +
                    $"{lang[$"{Name}.txtThemeInfo._Version"]}: {th.Version}\r\n" +
                    $"{lang[$"{Name}.txtThemeInfo._Author"]}: {th.Author}\r\n" +
                    $"{lang[$"{Name}.txtThemeInfo._Email"]}: {th.Email}\r\n" +
                    $"{lang[$"{Name}.txtThemeInfo._Website"]}: {th.Website}\r\n" +
                    $"{lang[$"{Name}.txtThemeInfo._Compatibility"]}: {th.ConfigVersion}\r\n" +
                    $"{lang[$"{Name}.txtThemeInfo._Description"]}: {th.Description}";

                txtThemeInfo.Visible = true;
            }
            else {
                picPreview.Image = null;
                txtThemeInfo.Visible = false;
                txtThemeInfo.Text = "";
                btnThemeSaveAs.Enabled = false;
                btnThemeUninstall.Enabled = false;
            }
        }

        private void btnThemeInstall_Click(object sender, EventArgs e) {
            using var o = new OpenFileDialog {
                Filter = "ImageGlass theme (*.igtheme)|*.igtheme|All files (*.*)|*.*"
            };
            if (o.ShowDialog() == DialogResult.OK && File.Exists(o.FileName)) {
                var result = Theme.InstallTheme(o.FileName);

                if (result == ThemeInstallingResult.SUCCESS) {
                    _ = RefreshThemeListAsync();

                    MessageBox.Show(Configs.Language.Items[$"{Name}.btnThemeInstall._Success"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else {
                    MessageBox.Show(Configs.Language.Items[$"{Name}.btnThemeInstall._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnThemeUninstall_Click(object sender, EventArgs e) {
            if (lvTheme.SelectedItems.Count > 0) {
                var themeDir = lvTheme.SelectedItems[0].Tag.ToString();
                var result = Theme.UninstallTheme(themeDir);

                if (result == ThemeUninstallingResult.SUCCESS) {
                    _ = RefreshThemeListAsync();
                }
                else if (result == ThemeUninstallingResult.ERROR) {
                    MessageBox.Show(Configs.Language.Items[$"{Name}.btnThemeUninstall._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void lnkThemeDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start($"https://imageglass.org/themes?utm_source=app_{App.Version}&utm_medium=app_click&utm_campaign=app_download_theme");
            }
            catch { }
        }

        private void btnThemeSaveAs_Click(object sender, EventArgs e) {
            if (lvTheme.SelectedItems.Count > 0) {
                var s = new SaveFileDialog {
                    Filter = "ImageGlass theme (*.igtheme)|*.igtheme"
                };

                if (s.ShowDialog() == DialogResult.OK) {
                    var themeDir = lvTheme.SelectedItems[0].Tag.ToString();
                    var result = Theme.PackTheme(themeDir, s.FileName);

                    if (result == ThemePackingResult.SUCCESS) {
                        MessageBox.Show(string.Format(Configs.Language.Items[$"{Name}.btnThemeSaveAs._Success"], s.FileName), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else {
                        MessageBox.Show(Configs.Language.Items[$"{Name}.btnThemeSaveAs._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnThemeFolderOpen_Click(object sender, EventArgs e) {
            var themeFolder = App.ConfigDir(PathType.Dir, Dir.Themes);
            Process.Start("explorer.exe", themeFolder);
        }

        private void btnThemeApply_Click(object sender, EventArgs e) {
            if (lvTheme.SelectedItems.Count > 0) {
                var themeDir = lvTheme.SelectedItems[0].Tag.ToString();

                var th = new Theme((int)Configs.ToolbarIconHeight, themeDir);

                if (th.IsValid) {
                    Configs.Theme = th;
                    Configs.BackgroundColor =
                        picBackgroundColor.BackColor =
                        Configs.Theme.BackgroundColor;

                    Local.ForceUpdateActions |= ForceUpdateActions.THEME;

                    // Apply theme
                    Configs.ApplyFormTheme(this, Configs.Theme);

                    MessageBox.Show(Configs.Language.Items[$"{Name}.btnThemeApply._Success"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else {
                    MessageBox.Show(Configs.Language.Items[$"{Name}.btnThemeApply._Error"], "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        #endregion

        #region TAB KEYBOARD
        private void LoadTabKeyboard(Dictionary<KeyCombos, AssignableActions> source) {
            var lang = Configs.Language.Items;

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
            MapKeyConfigToComboSelection(KeyCombos.LeftRight, cmbKeysLeftRight,
                lang[$"{Name}.KeyActions._PrevNextImage"], source);
            MapKeyConfigToComboSelection(KeyCombos.PageUpDown, cmbKeysPgUpDown,
                lang[$"{Name}.KeyActions._PrevNextImage"], source);
            MapKeyConfigToComboSelection(KeyCombos.UpDown, cmbKeysUpDown,
                lang[$"{Name}.KeyActions._PanUpDown"], source);
            MapKeyConfigToComboSelection(KeyCombos.SpaceBack, cmbKeysSpaceBack,
                lang[$"{Name}.KeyActions._PauseSlideshow"], source);
        }

        /// <summary>
        /// Translates the config value for a key assignment to a selected
        /// entry in a combobox. If something wrong, sets the combobox to the provided default.
        /// </summary>
        /// <param name="keyCombo">the key action to match</param>
        /// <param name="control">the combobox to set selection in</param>
        /// <param name="defaultString">On misconfiguration, use this string</param>
        /// <returns></returns>
        private void MapKeyConfigToComboSelection(KeyCombos keyCombo, ComboBox control, string defaultString, Dictionary<KeyCombos, AssignableActions> source) {
            try {
                var lang = Configs.Language.Items;

                // Fetch the string from language based on the action value
                var act = source[keyCombo];
                var actionList = Enum.GetNames(typeof(AssignableActions));
                var lookup = $"{Name}.KeyActions._{actionList[(int)act]}";

                // select the appropriate entry in the combo. On misconfiguration,
                // set to the provided default.
                control.SelectedItem = lang[lookup];
                if (control.SelectedIndex == -1) {
                    control.SelectedItem = defaultString;
                }
            }
            catch {
                // Some other situation (value out of range; not in strings; etc),
                // use provided default
                control.SelectedItem = defaultString;
            }
        }

        /// <summary>
        /// Save the keyboard configuration settings to the config file
        /// </summary>
        private void SaveKeyboardSettings() {
            // Brute-forcing this. Better solution?
            SaveKeyConfigFromCombo(KeyCombos.LeftRight, cmbKeysLeftRight);
            SaveKeyConfigFromCombo(KeyCombos.PageUpDown, cmbKeysPgUpDown);
            SaveKeyConfigFromCombo(KeyCombos.UpDown, cmbKeysUpDown);
            SaveKeyConfigFromCombo(KeyCombos.SpaceBack, cmbKeysSpaceBack);
        }

        /// <summary>
        /// For a given combobox, update the key config value in Configs
        /// </summary>
        /// <param name="keyCombo"></param>
        /// <param name="control"></param>
        private void SaveKeyConfigFromCombo(KeyCombos keyCombo, ComboBox control) {
            var selected = control.SelectedItem;
            if (selected == null)
                return; // user hasn't visited keyboard page, no changes

            var lang = Configs.Language.Items;

            // match the text of the selected combobox item against
            // the language string for the available actions
            var actionList = Enum.GetNames(typeof(AssignableActions));
            for (var i = 0; i < actionList.Length; i++) {
                var lookup = $"{Name}.KeyActions._{actionList[i]}";
                var val = lang[lookup];

                if (val == selected.ToString()) {
                    Configs.KeyComboActions[keyCombo] = (AssignableActions)i;
                    return;
                }
            }
        }

        /// <summary>
        /// Reset all key actions to their "default" (IG V6.0) behavior
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnKeyReset_Click(object sender, EventArgs e) {
            LoadTabKeyboard(Constants.DefaultKeycomboActions);
        }

        #endregion

        #region ACTION BUTTONS
        private void btnCancel_Click(object sender, EventArgs e) {
            //close without saving
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e) {
            // Save and close
            if (ApplySettings()) {
                this.Close();
            }
        }

        private void btnApply_Click(object sender, EventArgs e) {
            ApplySettings();
        }

        private bool ApplySettings() {
            // Variables for comparision
            var isSuccessful = true;
            int newInt;
            uint newUInt;
            bool newBool;
            string newString;
            Color newColor;

            #region General tab --------------------------------------------
            Configs.IsShowWelcome = chkWelcomePicture.Checked;
            Configs.IsOpenLastSeenImage = chkLastSeenImage.Checked;
            Configs.IsShowToolBar = chkShowToolBar.Checked;

            // AutoUpdate
            Configs.AutoUpdate = chkAutoUpdate.Checked ? DateTime.Now.ToString("M/d/yyyy HH:mm:ss") : "0";

            Configs.IsAllowMultiInstances = chkAllowMultiInstances.Checked;
            Configs.IsPressESCToQuit = chkESCToQuit.Checked;
            Configs.IsConfirmationDelete = chkConfirmationDelete.Checked;
            Configs.IsDisplayBasenameOfImage = chkDisplayBasename.Checked;
            Configs.IsCenterWindowFit = chkCenterWindowFit.Checked;
            Configs.IsShowToast = chkShowToast.Checked;
            Configs.IsUseTouchGesture = chkUseTouchGesture.Checked;

            #region IsShowNavigationButtons: MainFormForceUpdateAction.OTHER_SETTINGS
            // IsShowNavigationButtons
            newBool = chkShowNavButtons.Checked;
            if (Configs.IsShowNavigationButtons != newBool) {
                Configs.IsShowNavigationButtons = newBool;
                Local.ForceUpdateActions |= ForceUpdateActions.OTHER_SETTINGS;
            }
            #endregion

            #region IsShowCheckerboardOnlyImageRegion: MainFormForceUpdateAction.OTHER_SETTINGS
            // IsShowCheckerboardOnlyImageRegion
            newBool = chkShowCheckerboardOnlyImage.Checked;
            if (Configs.IsShowCheckerboardOnlyImageRegion != newBool) {
                Configs.IsShowCheckerboardOnlyImageRegion = newBool;
                Local.ForceUpdateActions |= ForceUpdateActions.OTHER_SETTINGS;
            }
            #endregion

            #region IsScrollbarsVisible: MainFormForceUpdateAction.OTHER_SETTINGS
            // IsScrollbarsVisible
            newBool = chkShowScrollbar.Checked;
            if (Configs.IsScrollbarsVisible != newBool) {
                Configs.IsScrollbarsVisible = newBool;
                Local.ForceUpdateActions |= ForceUpdateActions.OTHER_SETTINGS;
            }
            #endregion

            #region BackgroundColor: MainFormForceUpdateAction.OTHER_SETTINGS
            // BackgroundColor
            newColor = picBackgroundColor.BackColor;
            if (Configs.BackgroundColor != newColor) {
                Configs.BackgroundColor = picBackgroundColor.BackColor;
                Local.ForceUpdateActions |= ForceUpdateActions.OTHER_SETTINGS;
            }
            #endregion

            #endregion

            #region Image tab ----------------------------------------------

            #region IsRecursiveLoading: MainFormForceUpdateAction.IMAGE_LIST or IMAGE_LIST_NO_RECURSIVE
            newBool = chkFindChildFolder.Checked;
            if (Configs.IsRecursiveLoading != newBool) // Only change when the new value selected  
            {
                Configs.IsRecursiveLoading = newBool;

                // Request frmMain to update the thumbnail bar
                if (Configs.IsRecursiveLoading) {
                    Local.ForceUpdateActions |= ForceUpdateActions.IMAGE_LIST;
                }
                else {
                    Local.ForceUpdateActions |= ForceUpdateActions.IMAGE_LIST_NO_RECURSIVE;
                }
            }
            #endregion

            Configs.IsShowingHiddenImages = chkShowHiddenImages.Checked;
            Configs.IsLoopBackViewer = chkLoopViewer.Checked;

            // IsCenterImage
            newBool = chkIsCenterImage.Checked;
            if (Configs.IsCenterImage != newBool) {
                Configs.IsCenterImage = newBool;
                Local.ForceUpdateActions |= ForceUpdateActions.OTHER_SETTINGS;
            }

            #region ImageLoadingOrder: MainFormForceUpdateAction.IMAGE_LIST
            newInt = cmbImageOrder.SelectedIndex;

            if (Enum.TryParse(newInt.ToString(), out ImageOrderBy newOrder)) {
                if (Configs.ImageLoadingOrder != newOrder) //Only change when the new value selected  
                {
                    Configs.ImageLoadingOrder = newOrder;
                    Local.ForceUpdateActions |= ForceUpdateActions.IMAGE_LIST;
                }
            }

            newInt = cmbImageOrderType.SelectedIndex;
            if (Enum.TryParse(newInt.ToString(), out ImageOrderType newOrderType)) {
                if (Configs.ImageLoadingOrderType != newOrderType) //Only change when the new value selected  
                {
                    Configs.ImageLoadingOrderType = newOrderType;
                    Local.ForceUpdateActions |= ForceUpdateActions.IMAGE_LIST;
                }
            }

            Configs.IsUseFileExplorerSortOrder = chkUseFileExplorerSortOrder.Checked;
            if (Configs.IsGroupImagesByDirectory != chkGroupByDirectory.Checked) {
                Configs.IsGroupImagesByDirectory = chkGroupByDirectory.Checked;
                Local.ForceUpdateActions |= ForceUpdateActions.IMAGE_LIST;
            }

            #endregion

            // ImageBoosterCachedCount
            Configs.ImageBoosterCachedCount = (uint)cmbImageBoosterCachedCount.SelectedIndex;
            Local.ImageList.MaxQueue = Configs.ImageBoosterCachedCount;

            #region Color Management

            // apply color profile for all
            Configs.IsApplyColorProfileForAll = chkApplyColorProfile.Checked;

            // color profile
            if (cmbColorProfile.SelectedIndex == cmbColorProfile.Items.Count - 1) {
                // custom color profile file
                Configs.ColorProfile = lnkColorProfilePath.Text;
            }
            else {
                // built-in color profile
                Configs.ColorProfile = cmbColorProfile.SelectedItem.ToString();
            }

            #endregion

            #region Mouse wheel actions
            Configs.MouseWheelAction = (MouseWheelActions)cmbMouseWheel.SelectedIndex;
            Configs.MouseWheelCtrlAction = (MouseWheelActions)cmbMouseWheelCtrl.SelectedIndex;
            Configs.MouseWheelShiftAction = (MouseWheelActions)cmbMouseWheelShift.SelectedIndex;
            Configs.MouseWheelAltAction = (MouseWheelActions)cmbMouseWheelAlt.SelectedIndex;
            #endregion

            // ZoomOptimization
            Configs.ZoomOptimizationMethod = (ZoomOptimizationMethods)cmbZoomOptimization.SelectedIndex;

            #region ZoomLevels: MainFormForceUpdateAction.OTHER_SETTINGS;
            newString = txtZoomLevels.Text.Trim();

            if (string.IsNullOrEmpty(newString)) {
                txtZoomLevels.Text = Helpers.IntArrayToString(Configs.ZoomLevels);
            }
            else if (Helpers.IntArrayToString(Configs.ZoomLevels) != newString) {
                try {
                    Configs.ZoomLevels = Helpers.StringToIntArray(newString, unsignedOnly: true, distinct: true);
                    Local.ForceUpdateActions |= ForceUpdateActions.OTHER_SETTINGS;
                }
                catch (Exception ex) {
                    isSuccessful = false;
                    txtZoomLevels.Text = Helpers.IntArrayToString(Configs.ZoomLevels);
                    var msg = string.Format(Configs.Language.Items[$"{Name}.txtZoomLevels._Error"], ex.Message);

                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            #endregion

            #region THUMBNAIL BAR

            #region IsThumbnailHorizontal: MainFormForceUpdateAction.THUMBNAIL_BAR

            // IsThumbnailHorizontal
            newBool = !chkThumbnailVertical.Checked;
            if (Configs.IsThumbnailHorizontal != newBool) // Only change when the new value selected  
            {
                Configs.IsThumbnailHorizontal = newBool;
                Local.ForceUpdateActions |= ForceUpdateActions.THUMBNAIL_BAR;
            }
            #endregion

            #region IsShowThumbnailScrollbar: MainFormForceUpdateAction.THUMBNAIL_BAR

            // IsShowThumbnailScrollbar
            newBool = chkShowThumbnailScrollbar.Checked;
            if (Configs.IsShowThumbnailScrollbar != newBool) // Only change when the new value selected  
            {
                Configs.IsShowThumbnailScrollbar = newBool;
                Local.ForceUpdateActions |= ForceUpdateActions.THUMBNAIL_BAR;
            }
            #endregion

            #region ThumbnailDimension: MainFormForceUpdateAction.THUMBNAIL_ITEMS

            // ThumbnailDimension
            newUInt = (cmbThumbnailDimension.SelectedItem.ToString().Length == 0)
                ? Configs.ThumbnailDimension
                : uint.Parse(cmbThumbnailDimension.SelectedItem.ToString(), Constants.NumberFormat);

            if (Configs.ThumbnailDimension != newUInt) // Only change when the new value selected
            {
                Configs.ThumbnailDimension = newUInt;
                Local.ForceUpdateActions |= ForceUpdateActions.THUMBNAIL_ITEMS;
            }
            #endregion

            #endregion

            // slideshow
            Configs.IsLoopBackSlideshow = chkLoopSlideshow.Checked;
            Configs.IsShowSlideshowCountdown = chkShowSlideshowCountdown.Checked;
            Configs.IsRandomSlideshowInterval = chkRandomSlideshowInterval.Checked;

            Configs.SlideShowInterval = (uint)numSlideShowInterval.Value;
            Configs.SlideShowIntervalTo = (uint)numSlideshowIntervalTo.Value;

            #endregion

            #region Edit tab -----------------------------------------------
            Configs.IsSaveAfterRotating = chkSaveOnRotate.Checked;
            Configs.IsPreserveModifiedDate = chkSaveModifyDate.Checked;
            Configs.ImageEditQuality = (int)numImageQuality.Value;

            // AfterEditingAction
            if (Enum.TryParse(cmbAfterEditingApp.SelectedIndex.ToString(), out AfterOpeningEditAppAction newAction)) {
                Configs.AfterEditingAction = newAction;
            }

            #endregion

            #region Language tab -------------------------------------------

            #region Language: MainFormForceUpdateAction.LANGUAGE
            //Language
            if (lstLanguages.Count > 0) {
                newString = lstLanguages[cmbLanguage.SelectedIndex].FileName.ToLower();

                if (Configs.Language.FileName.ToLower().CompareTo(newString) != 0) {
                    Configs.Language = lstLanguages[cmbLanguage.SelectedIndex];
                    Local.ForceUpdateActions |= ForceUpdateActions.LANGUAGE;
                }
            }
            #endregion

            #endregion

            #region Toolbar tab --------------------------------------------

            #region ToolbarPosition: MainFormForceUpdateAction.TOOLBAR_POSITION
            newInt = cmbToolbarPosition.SelectedIndex;

            if (Enum.TryParse(newInt.ToString(), out ToolbarPosition newPosition)) {
                if (Configs.ToolbarPosition != newPosition) // Only change when the new value selected  
                {
                    Configs.ToolbarPosition = newPosition;
                    Local.ForceUpdateActions |= ForceUpdateActions.TOOLBAR_POSITION;
                }
            }

            #endregion

            #region HorzCenterToolbarBtns: MainFormForceUpdateAction.TOOLBAR_POSITION
            newBool = chkHorzCenterToolbarBtns.Checked;

            if (Configs.IsCenterToolbar != newBool) {
                Configs.IsCenterToolbar = newBool;
                Local.ForceUpdateActions |= ForceUpdateActions.TOOLBAR_POSITION;
            }
            #endregion

            #region HideToolbarTooltips: MainFormForceUpdateAction.TOOLBAR_POSITION
            newBool = chkHideTooltips.Checked;

            if (Configs.IsHideTooltips != newBool) {
                Configs.IsHideTooltips = newBool;
                Local.ForceUpdateActions |= ForceUpdateActions.TOOLBAR_POSITION;
            }
            #endregion

            #region ToolbarIconHeight: MainFormForceUpdateAction.TOOLBAR_ICON_HEIGHT
            newUInt = (uint)numToolbarIconHeight.Value;

            if (Configs.ToolbarIconHeight != newUInt) {
                Configs.ToolbarIconHeight = newUInt;
                Local.ForceUpdateActions |= ForceUpdateActions.TOOLBAR_ICON_HEIGHT;
            }
            #endregion

            ApplyToolbarChanges();
            #endregion

            #region Tools tab ---------------------------------------
            Configs.IsColorPickerRGBA = chkColorUseRGBA.Checked;
            Configs.IsColorPickerHEXA = chkColorUseHEXA.Checked;
            Configs.IsColorPickerHSLA = chkColorUseHSLA.Checked;
            Configs.IsColorPickerHSVA = chkColorUseHSVA.Checked;

            Configs.IsShowPageNavAuto = chkShowPageNavAuto.Checked;
            Configs.IsExifToolAlwaysOnTop = chkExifToolAlwaysOnTop.Checked;
            #endregion

            SaveKeyboardSettings();

            return isSuccessful;
        }

        #endregion

    }
}
