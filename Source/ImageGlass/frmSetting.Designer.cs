﻿
namespace ImageGlass
{
    partial class frmSetting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Default extensions", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Optional extensions", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(".123");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(".abc");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(".def");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSetting));
            this.imglTheme = new System.Windows.Forms.ImageList(this.components);
            this.lblLanguage = new System.Windows.Forms.Label();
            this.lblFileAssociations = new System.Windows.Forms.Label();
            this.lblGeneral = new System.Windows.Forms.Label();
            this.tip1 = new System.Windows.Forms.ToolTip(this.components);
            this.picBackgroundColor = new System.Windows.Forms.PictureBox();
            this.tabLanguage = new System.Windows.Forms.TabPage();
            this.lblLanguageWarning = new System.Windows.Forms.Label();
            this.lnkInstallLanguage = new System.Windows.Forms.LinkLabel();
            this.lnkRefresh = new System.Windows.Forms.LinkLabel();
            this.lnkEdit = new System.Windows.Forms.LinkLabel();
            this.lnkCreateNew = new System.Windows.Forms.LinkLabel();
            this.lnkGetMoreLanguage = new System.Windows.Forms.LinkLabel();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.lblLanguageText = new System.Windows.Forms.Label();
            this.tabFileAssociation = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnRegisterExt = new System.Windows.Forms.Button();
            this.btnResetExt = new System.Windows.Forms.Button();
            this.btnAddNewExt = new System.Windows.Forms.Button();
            this.btnDeleteExt = new System.Windows.Forms.Button();
            this.lblExtensionsGroupDescription = new System.Windows.Forms.Label();
            this.lvExtension = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblSupportedExtension = new System.Windows.Forms.Label();
            this.lnkOpenFileAssoc = new System.Windows.Forms.LinkLabel();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.chkShowCheckerboardOnlyImage = new System.Windows.Forms.CheckBox();
            this.chkShowNavButtons = new System.Windows.Forms.CheckBox();
            this.chkLastSeenImage = new System.Windows.Forms.CheckBox();
            this.lnkConfigDir = new System.Windows.Forms.LinkLabel();
            this.chkDisplayBasename = new System.Windows.Forms.CheckBox();
            this.chkShowScrollbar = new System.Windows.Forms.CheckBox();
            this.lnkResetBackgroundColor = new System.Windows.Forms.LinkLabel();
            this.lblHeadOthers = new System.Windows.Forms.Label();
            this.lblHeadConfigDir = new System.Windows.Forms.Label();
            this.lblHeadStartup = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkConfirmationDelete = new System.Windows.Forms.CheckBox();
            this.chkAllowMultiInstances = new System.Windows.Forms.CheckBox();
            this.chkESCToQuit = new System.Windows.Forms.CheckBox();
            this.chkShowToolBar = new System.Windows.Forms.CheckBox();
            this.lblBackGroundColor = new System.Windows.Forms.Label();
            this.chkWelcomePicture = new System.Windows.Forms.CheckBox();
            this.chkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.tab1 = new System.Windows.Forms.TabControl();
            this.tabImage = new System.Windows.Forms.TabPage();
            this.chkDisplayLoadMessage = new System.Windows.Forms.CheckBox();
            this.lnkColorProfilePath = new System.Windows.Forms.LinkLabel();
            this.lnkColorProfileBrowse = new System.Windows.Forms.LinkLabel();
            this.lblColorManagement = new System.Windows.Forms.Label();
            this.chkApplyColorProfile = new System.Windows.Forms.CheckBox();
            this.cmbColorProfile = new System.Windows.Forms.ComboBox();
            this.lblColorProfile = new System.Windows.Forms.Label();
            this.chkShowThumbnailScrollbar = new System.Windows.Forms.CheckBox();
            this.cmbMouseWheelAlt = new System.Windows.Forms.ComboBox();
            this.cmbMouseWheelShift = new System.Windows.Forms.ComboBox();
            this.cmbMouseWheelCtrl = new System.Windows.Forms.ComboBox();
            this.cmbMouseWheel = new System.Windows.Forms.ComboBox();
            this.lblMouseWheelAlt = new System.Windows.Forms.Label();
            this.lblMouseWheelShift = new System.Windows.Forms.Label();
            this.lblMouseWheelCtrl = new System.Windows.Forms.Label();
            this.lblMouseWheel = new System.Windows.Forms.Label();
            this.lblHeadMouseWheelActions = new System.Windows.Forms.Label();
            this.chkShowHiddenImages = new System.Windows.Forms.CheckBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblHeadZooming = new System.Windows.Forms.Label();
            this.lblHeadSlideshow = new System.Windows.Forms.Label();
            this.lblHeadThumbnailBar = new System.Windows.Forms.Label();
            this.lblHeadImageLoading = new System.Windows.Forms.Label();
            this.chkLoopViewer = new System.Windows.Forms.CheckBox();
            this.lblGeneral_ZoomOptimization = new System.Windows.Forms.Label();
            this.cmbZoomOptimization = new System.Windows.Forms.ComboBox();
            this.chkThumbnailVertical = new System.Windows.Forms.CheckBox();
            this.lblGeneral_ThumbnailSize = new System.Windows.Forms.Label();
            this.cmbThumbnailDimension = new System.Windows.Forms.ComboBox();
            this.chkImageBoosterBack = new System.Windows.Forms.CheckBox();
            this.chkLoopSlideshow = new System.Windows.Forms.CheckBox();
            this.lblImageLoadingOrder = new System.Windows.Forms.Label();
            this.cmbImageOrder = new System.Windows.Forms.ComboBox();
            this.barInterval = new System.Windows.Forms.TrackBar();
            this.lblSlideshowInterval = new System.Windows.Forms.Label();
            this.chkFindChildFolder = new System.Windows.Forms.CheckBox();
            this.tabEdit = new System.Windows.Forms.TabPage();
            this.lblSelectAppForEdit = new System.Windows.Forms.Label();
            this.chkSaveOnRotate = new System.Windows.Forms.CheckBox();
            this.chkSaveModifyDate = new System.Windows.Forms.CheckBox();
            this.btnEditEditAllExt = new System.Windows.Forms.Button();
            this.btnEditResetExt = new System.Windows.Forms.Button();
            this.btnEditEditExt = new System.Windows.Forms.Button();
            this.lvImageEditing = new System.Windows.Forms.ListView();
            this.clnFileExtension = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnAppName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnAppPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnAppArguments = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabToolbar = new System.Windows.Forms.TabPage();
            this.panel5 = new System.Windows.Forms.Panel();
            this.lblAvailBtns = new System.Windows.Forms.Label();
            this.btnMoveRight = new System.Windows.Forms.Button();
            this.btnMoveLeft = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.lvAvailButtons = new System.Windows.Forms.ListView();
            this.lblUsedBtns = new System.Windows.Forms.Label();
            this.lvUsedButtons = new System.Windows.Forms.ListView();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.cmbToolbarPosition = new System.Windows.Forms.ComboBox();
            this.lblToolbarPosition = new System.Windows.Forms.Label();
            this.chkHorzCenterToolbarBtns = new System.Windows.Forms.CheckBox();
            this.tabColorPicker = new System.Windows.Forms.TabPage();
            this.chkColorUseHSLA = new System.Windows.Forms.CheckBox();
            this.lblColorCodeFormat = new System.Windows.Forms.Label();
            this.chkColorUseHEXA = new System.Windows.Forms.CheckBox();
            this.chkColorUseRGBA = new System.Windows.Forms.CheckBox();
            this.tabTheme = new System.Windows.Forms.TabPage();
            this.panelThemeActions = new System.Windows.Forms.Panel();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.btnThemeFolderOpen = new System.Windows.Forms.Button();
            this.btnThemeSaveAs = new System.Windows.Forms.Button();
            this.txtThemeInfo = new System.Windows.Forms.TextBox();
            this.btnThemeRefresh = new System.Windows.Forms.Button();
            this.btnThemeInstall = new System.Windows.Forms.Button();
            this.btnThemeUninstall = new System.Windows.Forms.Button();
            this.lvTheme = new System.Windows.Forms.ListView();
            this.imglGeneral = new System.Windows.Forms.ImageList(this.components);
            this.btnThemeApply = new System.Windows.Forms.Button();
            this.lnkThemeDownload = new System.Windows.Forms.LinkLabel();
            this.btnThemeEdit = new System.Windows.Forms.Button();
            this.lblInstalledThemes = new System.Windows.Forms.Label();
            this.imglOpenWith = new System.Windows.Forms.ImageList(this.components);
            this.sp1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblImage = new System.Windows.Forms.Label();
            this.lblToolbar = new System.Windows.Forms.Label();
            this.lblColorPicker = new System.Windows.Forms.Label();
            this.lblEdit = new System.Windows.Forms.Label();
            this.lblTheme = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.tblayout = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundColor)).BeginInit();
            this.tabLanguage.SuspendLayout();
            this.tabFileAssociation.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tab1.SuspendLayout();
            this.tabImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barInterval)).BeginInit();
            this.tabEdit.SuspendLayout();
            this.tabToolbar.SuspendLayout();
            this.panel5.SuspendLayout();
            this.tabColorPicker.SuspendLayout();
            this.tabTheme.SuspendLayout();
            this.panelThemeActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sp1)).BeginInit();
            this.sp1.Panel1.SuspendLayout();
            this.sp1.Panel2.SuspendLayout();
            this.sp1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tblayout.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // imglTheme
            // 
            this.imglTheme.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imglTheme.ImageSize = new System.Drawing.Size(32, 32);
            this.imglTheme.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // lblLanguage
            // 
            this.lblLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLanguage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblLanguage.Location = new System.Drawing.Point(0, 198);
            this.lblLanguage.Margin = new System.Windows.Forms.Padding(0);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblLanguage.Size = new System.Drawing.Size(169, 33);
            this.lblLanguage.TabIndex = 7;
            this.lblLanguage.Tag = "0";
            this.lblLanguage.Text = "Language";
            this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblLanguage.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblLanguage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblLanguage.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblLanguage.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblLanguage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblFileAssociations
            // 
            this.lblFileAssociations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFileAssociations.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblFileAssociations.Location = new System.Drawing.Point(0, 99);
            this.lblFileAssociations.Margin = new System.Windows.Forms.Padding(0);
            this.lblFileAssociations.Name = "lblFileAssociations";
            this.lblFileAssociations.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblFileAssociations.Size = new System.Drawing.Size(169, 33);
            this.lblFileAssociations.TabIndex = 4;
            this.lblFileAssociations.Tag = "0";
            this.lblFileAssociations.Text = "File Associations";
            this.lblFileAssociations.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFileAssociations.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblFileAssociations.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblFileAssociations.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblFileAssociations.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblFileAssociations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblGeneral
            // 
            this.lblGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGeneral.BackColor = System.Drawing.Color.Gainsboro;
            this.lblGeneral.Location = new System.Drawing.Point(0, 0);
            this.lblGeneral.Margin = new System.Windows.Forms.Padding(0);
            this.lblGeneral.Name = "lblGeneral";
            this.lblGeneral.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblGeneral.Size = new System.Drawing.Size(169, 33);
            this.lblGeneral.TabIndex = 1;
            this.lblGeneral.Tag = "1";
            this.lblGeneral.Text = "General";
            this.lblGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblGeneral.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblGeneral.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblGeneral.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblGeneral.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblGeneral.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // picBackgroundColor
            // 
            this.picBackgroundColor.BackColor = System.Drawing.Color.White;
            this.picBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBackgroundColor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picBackgroundColor.Location = new System.Drawing.Point(30, 439);
            this.picBackgroundColor.Margin = new System.Windows.Forms.Padding(1);
            this.picBackgroundColor.Name = "picBackgroundColor";
            this.picBackgroundColor.Size = new System.Drawing.Size(67, 27);
            this.picBackgroundColor.TabIndex = 12;
            this.picBackgroundColor.TabStop = false;
            this.tip1.SetToolTip(this.picBackgroundColor, "Change background color");
            this.picBackgroundColor.Click += new System.EventHandler(this.picBackgroundColor_Click);
            // 
            // tabLanguage
            // 
            this.tabLanguage.AutoScroll = true;
            this.tabLanguage.BackColor = System.Drawing.Color.White;
            this.tabLanguage.Controls.Add(this.lblLanguageWarning);
            this.tabLanguage.Controls.Add(this.lnkInstallLanguage);
            this.tabLanguage.Controls.Add(this.lnkRefresh);
            this.tabLanguage.Controls.Add(this.lnkEdit);
            this.tabLanguage.Controls.Add(this.lnkCreateNew);
            this.tabLanguage.Controls.Add(this.lnkGetMoreLanguage);
            this.tabLanguage.Controls.Add(this.cmbLanguage);
            this.tabLanguage.Controls.Add(this.lblLanguageText);
            this.tabLanguage.Location = new System.Drawing.Point(4, 27);
            this.tabLanguage.Margin = new System.Windows.Forms.Padding(0);
            this.tabLanguage.Name = "tabLanguage";
            this.tabLanguage.Size = new System.Drawing.Size(531, 386);
            this.tabLanguage.TabIndex = 2;
            this.tabLanguage.Text = "language";
            // 
            // lblLanguageWarning
            // 
            this.lblLanguageWarning.AutoSize = true;
            this.lblLanguageWarning.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLanguageWarning.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(115)))), ((int)(((byte)(17)))));
            this.lblLanguageWarning.Location = new System.Drawing.Point(15, 59);
            this.lblLanguageWarning.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblLanguageWarning.Name = "lblLanguageWarning";
            this.lblLanguageWarning.Size = new System.Drawing.Size(376, 15);
            this.lblLanguageWarning.TabIndex = 25;
            this.lblLanguageWarning.Text = "[This language pack may be not compatible with ImageGlass 3.2.0.16.]";
            this.lblLanguageWarning.Visible = false;
            // 
            // lnkInstallLanguage
            // 
            this.lnkInstallLanguage.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkInstallLanguage.AutoSize = true;
            this.lnkInstallLanguage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkInstallLanguage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkInstallLanguage.Location = new System.Drawing.Point(15, 125);
            this.lnkInstallLanguage.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkInstallLanguage.Name = "lnkInstallLanguage";
            this.lnkInstallLanguage.Size = new System.Drawing.Size(206, 15);
            this.lnkInstallLanguage.TabIndex = 60;
            this.lnkInstallLanguage.TabStop = true;
            this.lnkInstallLanguage.Text = "> Install new language pack (*.iglang)";
            this.lnkInstallLanguage.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkInstallLanguage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkInstallLanguage_LinkClicked);
            // 
            // lnkRefresh
            // 
            this.lnkRefresh.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkRefresh.AutoSize = true;
            this.lnkRefresh.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkRefresh.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkRefresh.Location = new System.Drawing.Point(199, 33);
            this.lnkRefresh.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkRefresh.Name = "lnkRefresh";
            this.lnkRefresh.Size = new System.Drawing.Size(57, 15);
            this.lnkRefresh.TabIndex = 59;
            this.lnkRefresh.TabStop = true;
            this.lnkRefresh.Text = "> Refresh";
            this.lnkRefresh.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRefresh_LinkClicked);
            // 
            // lnkEdit
            // 
            this.lnkEdit.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkEdit.AutoSize = true;
            this.lnkEdit.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkEdit.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkEdit.Location = new System.Drawing.Point(15, 169);
            this.lnkEdit.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkEdit.Name = "lnkEdit";
            this.lnkEdit.Size = new System.Drawing.Size(164, 15);
            this.lnkEdit.TabIndex = 62;
            this.lnkEdit.TabStop = true;
            this.lnkEdit.Text = "> Edit selected language pack";
            this.lnkEdit.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkEdit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkEdit_LinkClicked);
            // 
            // lnkCreateNew
            // 
            this.lnkCreateNew.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkCreateNew.AutoSize = true;
            this.lnkCreateNew.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkCreateNew.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkCreateNew.Location = new System.Drawing.Point(15, 147);
            this.lnkCreateNew.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkCreateNew.Name = "lnkCreateNew";
            this.lnkCreateNew.Size = new System.Drawing.Size(157, 15);
            this.lnkCreateNew.TabIndex = 61;
            this.lnkCreateNew.TabStop = true;
            this.lnkCreateNew.Text = "> Create new language pack";
            this.lnkCreateNew.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkCreateNew.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCreateNew_LinkClicked);
            // 
            // lnkGetMoreLanguage
            // 
            this.lnkGetMoreLanguage.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkGetMoreLanguage.AutoSize = true;
            this.lnkGetMoreLanguage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkGetMoreLanguage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkGetMoreLanguage.Location = new System.Drawing.Point(15, 191);
            this.lnkGetMoreLanguage.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkGetMoreLanguage.Name = "lnkGetMoreLanguage";
            this.lnkGetMoreLanguage.Size = new System.Drawing.Size(152, 15);
            this.lnkGetMoreLanguage.TabIndex = 63;
            this.lnkGetMoreLanguage.TabStop = true;
            this.lnkGetMoreLanguage.Text = "> Get more language packs";
            this.lnkGetMoreLanguage.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkGetMoreLanguage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGetMoreLanguage_LinkClicked);
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            "English (default)",
            "Vietnamese"});
            this.cmbLanguage.Location = new System.Drawing.Point(18, 31);
            this.cmbLanguage.Margin = new System.Windows.Forms.Padding(1);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(171, 23);
            this.cmbLanguage.TabIndex = 58;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // lblLanguageText
            // 
            this.lblLanguageText.AutoSize = true;
            this.lblLanguageText.Location = new System.Drawing.Point(15, 13);
            this.lblLanguageText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblLanguageText.Name = "lblLanguageText";
            this.lblLanguageText.Size = new System.Drawing.Size(111, 15);
            this.lblLanguageText.TabIndex = 1;
            this.lblLanguageText.Text = "Installed languages:";
            // 
            // tabFileAssociation
            // 
            this.tabFileAssociation.BackColor = System.Drawing.Color.White;
            this.tabFileAssociation.Controls.Add(this.panel2);
            this.tabFileAssociation.Controls.Add(this.lblExtensionsGroupDescription);
            this.tabFileAssociation.Controls.Add(this.lvExtension);
            this.tabFileAssociation.Controls.Add(this.lblSupportedExtension);
            this.tabFileAssociation.Controls.Add(this.lnkOpenFileAssoc);
            this.tabFileAssociation.Location = new System.Drawing.Point(4, 27);
            this.tabFileAssociation.Margin = new System.Windows.Forms.Padding(0);
            this.tabFileAssociation.Name = "tabFileAssociation";
            this.tabFileAssociation.Size = new System.Drawing.Size(531, 386);
            this.tabFileAssociation.TabIndex = 1;
            this.tabFileAssociation.Text = "file association";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.Controls.Add(this.btnRegisterExt);
            this.panel2.Controls.Add(this.btnResetExt);
            this.panel2.Controls.Add(this.btnAddNewExt);
            this.panel2.Controls.Add(this.btnDeleteExt);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 326);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(531, 60);
            this.panel2.TabIndex = 35;
            // 
            // btnRegisterExt
            // 
            this.btnRegisterExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRegisterExt.AutoSize = true;
            this.btnRegisterExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnRegisterExt.Location = new System.Drawing.Point(350, 8);
            this.btnRegisterExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnRegisterExt.Name = "btnRegisterExt";
            this.btnRegisterExt.Size = new System.Drawing.Size(227, 30);
            this.btnRegisterExt.TabIndex = 46;
            this.btnRegisterExt.Text = "Set as Default photo viewer_";
            this.btnRegisterExt.UseVisualStyleBackColor = true;
            this.btnRegisterExt.Click += new System.EventHandler(this.btnRegisterExt_Click);
            // 
            // btnResetExt
            // 
            this.btnResetExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnResetExt.AutoSize = true;
            this.btnResetExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnResetExt.Location = new System.Drawing.Point(199, 8);
            this.btnResetExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnResetExt.Name = "btnResetExt";
            this.btnResetExt.Size = new System.Drawing.Size(147, 30);
            this.btnResetExt.TabIndex = 45;
            this.btnResetExt.Text = "Reset to default";
            this.btnResetExt.UseVisualStyleBackColor = true;
            this.btnResetExt.Click += new System.EventHandler(this.btnResetExt_Click);
            // 
            // btnAddNewExt
            // 
            this.btnAddNewExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddNewExt.AutoSize = true;
            this.btnAddNewExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnAddNewExt.Location = new System.Drawing.Point(18, 8);
            this.btnAddNewExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddNewExt.Name = "btnAddNewExt";
            this.btnAddNewExt.Size = new System.Drawing.Size(87, 30);
            this.btnAddNewExt.TabIndex = 43;
            this.btnAddNewExt.Text = "Add";
            this.btnAddNewExt.UseVisualStyleBackColor = true;
            this.btnAddNewExt.Click += new System.EventHandler(this.btnAddNewExt_Click);
            // 
            // btnDeleteExt
            // 
            this.btnDeleteExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteExt.AutoSize = true;
            this.btnDeleteExt.Enabled = false;
            this.btnDeleteExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnDeleteExt.Location = new System.Drawing.Point(109, 8);
            this.btnDeleteExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnDeleteExt.Name = "btnDeleteExt";
            this.btnDeleteExt.Size = new System.Drawing.Size(87, 30);
            this.btnDeleteExt.TabIndex = 44;
            this.btnDeleteExt.Text = "Delete";
            this.btnDeleteExt.UseVisualStyleBackColor = true;
            this.btnDeleteExt.Click += new System.EventHandler(this.btnDeleteExt_Click);
            // 
            // lblExtensionsGroupDescription
            // 
            this.lblExtensionsGroupDescription.AutoSize = true;
            this.lblExtensionsGroupDescription.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExtensionsGroupDescription.Location = new System.Drawing.Point(15, 13);
            this.lblExtensionsGroupDescription.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblExtensionsGroupDescription.Name = "lblExtensionsGroupDescription";
            this.lblExtensionsGroupDescription.Size = new System.Drawing.Size(382, 15);
            this.lblExtensionsGroupDescription.TabIndex = 34;
            this.lblExtensionsGroupDescription.Text = "*Optional extensions will not be automatically pre-loaded into memory .";
            // 
            // lvExtension
            // 
            this.lvExtension.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvExtension.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvExtension.FullRowSelect = true;
            this.lvExtension.GridLines = true;
            listViewGroup1.Header = "Default extensions";
            listViewGroup1.Name = "Default";
            listViewGroup2.Header = "Optional extensions";
            listViewGroup2.Name = "Optional";
            this.lvExtension.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.lvExtension.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvExtension.Location = new System.Drawing.Point(18, 61);
            this.lvExtension.Margin = new System.Windows.Forms.Padding(2);
            this.lvExtension.Name = "lvExtension";
            this.lvExtension.Size = new System.Drawing.Size(502, 248);
            this.lvExtension.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvExtension.TabIndex = 42;
            this.lvExtension.TileSize = new System.Drawing.Size(100, 30);
            this.lvExtension.UseCompatibleStateImageBehavior = false;
            this.lvExtension.View = System.Windows.Forms.View.Tile;
            this.lvExtension.SelectedIndexChanged += new System.EventHandler(this.lvExtension_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Extensions";
            this.columnHeader1.Width = 150;
            // 
            // lblSupportedExtension
            // 
            this.lblSupportedExtension.AutoSize = true;
            this.lblSupportedExtension.Location = new System.Drawing.Point(15, 40);
            this.lblSupportedExtension.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblSupportedExtension.Name = "lblSupportedExtension";
            this.lblSupportedExtension.Size = new System.Drawing.Size(123, 15);
            this.lblSupportedExtension.TabIndex = 21;
            this.lblSupportedExtension.Text = "Supported extensions:";
            // 
            // lnkOpenFileAssoc
            // 
            this.lnkOpenFileAssoc.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkOpenFileAssoc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkOpenFileAssoc.BackColor = System.Drawing.Color.Transparent;
            this.lnkOpenFileAssoc.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkOpenFileAssoc.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkOpenFileAssoc.Location = new System.Drawing.Point(259, 40);
            this.lnkOpenFileAssoc.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lnkOpenFileAssoc.Name = "lnkOpenFileAssoc";
            this.lnkOpenFileAssoc.Size = new System.Drawing.Size(261, 25);
            this.lnkOpenFileAssoc.TabIndex = 41;
            this.lnkOpenFileAssoc.TabStop = true;
            this.lnkOpenFileAssoc.Text = "Open File Associations";
            this.lnkOpenFileAssoc.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lnkOpenFileAssoc.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkOpenFileAssoc.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkOpenFileAssoc_LinkClicked);
            // 
            // tabGeneral
            // 
            this.tabGeneral.AutoScroll = true;
            this.tabGeneral.BackColor = System.Drawing.Color.White;
            this.tabGeneral.Controls.Add(this.chkShowCheckerboardOnlyImage);
            this.tabGeneral.Controls.Add(this.chkShowNavButtons);
            this.tabGeneral.Controls.Add(this.chkLastSeenImage);
            this.tabGeneral.Controls.Add(this.lnkConfigDir);
            this.tabGeneral.Controls.Add(this.chkDisplayBasename);
            this.tabGeneral.Controls.Add(this.chkShowScrollbar);
            this.tabGeneral.Controls.Add(this.lnkResetBackgroundColor);
            this.tabGeneral.Controls.Add(this.lblHeadOthers);
            this.tabGeneral.Controls.Add(this.lblHeadConfigDir);
            this.tabGeneral.Controls.Add(this.lblHeadStartup);
            this.tabGeneral.Controls.Add(this.panel1);
            this.tabGeneral.Controls.Add(this.chkConfirmationDelete);
            this.tabGeneral.Controls.Add(this.chkAllowMultiInstances);
            this.tabGeneral.Controls.Add(this.chkESCToQuit);
            this.tabGeneral.Controls.Add(this.chkShowToolBar);
            this.tabGeneral.Controls.Add(this.picBackgroundColor);
            this.tabGeneral.Controls.Add(this.lblBackGroundColor);
            this.tabGeneral.Controls.Add(this.chkWelcomePicture);
            this.tabGeneral.Controls.Add(this.chkAutoUpdate);
            this.tabGeneral.Location = new System.Drawing.Point(4, 27);
            this.tabGeneral.Margin = new System.Windows.Forms.Padding(0);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Size = new System.Drawing.Size(531, 386);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "general";
            // 
            // chkShowCheckerboardOnlyImage
            // 
            this.chkShowCheckerboardOnlyImage.AutoSize = true;
            this.chkShowCheckerboardOnlyImage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowCheckerboardOnlyImage.Location = new System.Drawing.Point(30, 385);
            this.chkShowCheckerboardOnlyImage.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowCheckerboardOnlyImage.Name = "chkShowCheckerboardOnlyImage";
            this.chkShowCheckerboardOnlyImage.Size = new System.Drawing.Size(285, 20);
            this.chkShowCheckerboardOnlyImage.TabIndex = 14;
            this.chkShowCheckerboardOnlyImage.Text = "[Display checkerboard only in the image region]";
            this.chkShowCheckerboardOnlyImage.UseVisualStyleBackColor = true;
            // 
            // chkShowNavButtons
            // 
            this.chkShowNavButtons.AutoSize = true;
            this.chkShowNavButtons.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowNavButtons.Location = new System.Drawing.Point(30, 363);
            this.chkShowNavButtons.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowNavButtons.Name = "chkShowNavButtons";
            this.chkShowNavButtons.Size = new System.Drawing.Size(214, 20);
            this.chkShowNavButtons.TabIndex = 13;
            this.chkShowNavButtons.Text = "[Display navigation arrow buttons]";
            this.chkShowNavButtons.UseVisualStyleBackColor = true;
            // 
            // chkLastSeenImage
            // 
            this.chkLastSeenImage.AutoSize = true;
            this.chkLastSeenImage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkLastSeenImage.Location = new System.Drawing.Point(30, 57);
            this.chkLastSeenImage.Margin = new System.Windows.Forms.Padding(1);
            this.chkLastSeenImage.Name = "chkLastSeenImage";
            this.chkLastSeenImage.Size = new System.Drawing.Size(173, 20);
            this.chkLastSeenImage.TabIndex = 4;
            this.chkLastSeenImage.Text = "[Open the last seen image]";
            this.chkLastSeenImage.UseVisualStyleBackColor = true;
            // 
            // lnkConfigDir
            // 
            this.lnkConfigDir.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkConfigDir.AutoSize = true;
            this.lnkConfigDir.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkConfigDir.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkConfigDir.Location = new System.Drawing.Point(30, 153);
            this.lnkConfigDir.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lnkConfigDir.Name = "lnkConfigDir";
            this.lnkConfigDir.Size = new System.Drawing.Size(80, 15);
            this.lnkConfigDir.TabIndex = 6;
            this.lnkConfigDir.TabStop = true;
            this.lnkConfigDir.Text = "[C:\\ABC\\XYZ]";
            this.lnkConfigDir.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkConfigDir.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkConfigDir_LinkClicked);
            // 
            // chkDisplayBasename
            // 
            this.chkDisplayBasename.AutoSize = true;
            this.chkDisplayBasename.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkDisplayBasename.Location = new System.Drawing.Point(30, 340);
            this.chkDisplayBasename.Margin = new System.Windows.Forms.Padding(1);
            this.chkDisplayBasename.Name = "chkDisplayBasename";
            this.chkDisplayBasename.Size = new System.Drawing.Size(312, 20);
            this.chkDisplayBasename.TabIndex = 12;
            this.chkDisplayBasename.Text = "[Display base name of the viewing image on title bar]";
            this.chkDisplayBasename.UseVisualStyleBackColor = true;
            // 
            // chkShowScrollbar
            // 
            this.chkShowScrollbar.AutoSize = true;
            this.chkShowScrollbar.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowScrollbar.Location = new System.Drawing.Point(30, 317);
            this.chkShowScrollbar.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowScrollbar.Name = "chkShowScrollbar";
            this.chkShowScrollbar.Size = new System.Drawing.Size(160, 20);
            this.chkShowScrollbar.TabIndex = 11;
            this.chkShowScrollbar.Text = "Display viewer scrollbars";
            this.chkShowScrollbar.UseVisualStyleBackColor = true;
            // 
            // lnkResetBackgroundColor
            // 
            this.lnkResetBackgroundColor.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkResetBackgroundColor.AutoSize = true;
            this.lnkResetBackgroundColor.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkResetBackgroundColor.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkResetBackgroundColor.Location = new System.Drawing.Point(100, 444);
            this.lnkResetBackgroundColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lnkResetBackgroundColor.Name = "lnkResetBackgroundColor";
            this.lnkResetBackgroundColor.Size = new System.Drawing.Size(35, 15);
            this.lnkResetBackgroundColor.TabIndex = 15;
            this.lnkResetBackgroundColor.TabStop = true;
            this.lnkResetBackgroundColor.Text = "Reset";
            this.lnkResetBackgroundColor.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkResetBackgroundColor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkResetBackgroundColor_LinkClicked);
            // 
            // lblHeadOthers
            // 
            this.lblHeadOthers.AutoSize = true;
            this.lblHeadOthers.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadOthers.Location = new System.Drawing.Point(15, 205);
            this.lblHeadOthers.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeadOthers.Name = "lblHeadOthers";
            this.lblHeadOthers.Size = new System.Drawing.Size(45, 15);
            this.lblHeadOthers.TabIndex = 46;
            this.lblHeadOthers.Text = "Others";
            // 
            // lblHeadConfigDir
            // 
            this.lblHeadConfigDir.AutoSize = true;
            this.lblHeadConfigDir.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadConfigDir.Location = new System.Drawing.Point(15, 132);
            this.lblHeadConfigDir.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeadConfigDir.Name = "lblHeadConfigDir";
            this.lblHeadConfigDir.Size = new System.Drawing.Size(145, 15);
            this.lblHeadConfigDir.TabIndex = 45;
            this.lblHeadConfigDir.Text = "[Configuration directory]";
            // 
            // lblHeadStartup
            // 
            this.lblHeadStartup.AutoSize = true;
            this.lblHeadStartup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadStartup.Location = new System.Drawing.Point(15, 13);
            this.lblHeadStartup.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeadStartup.Name = "lblHeadStartup";
            this.lblHeadStartup.Size = new System.Drawing.Size(52, 15);
            this.lblHeadStartup.TabIndex = 44;
            this.lblHeadStartup.Text = "Start up";
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(181, 497);
            this.panel1.Margin = new System.Windows.Forms.Padding(1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(73, 21);
            this.panel1.TabIndex = 24;
            // 
            // chkConfirmationDelete
            // 
            this.chkConfirmationDelete.AutoSize = true;
            this.chkConfirmationDelete.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkConfirmationDelete.Location = new System.Drawing.Point(30, 295);
            this.chkConfirmationDelete.Margin = new System.Windows.Forms.Padding(1);
            this.chkConfirmationDelete.Name = "chkConfirmationDelete";
            this.chkConfirmationDelete.Size = new System.Drawing.Size(217, 20);
            this.chkConfirmationDelete.TabIndex = 10;
            this.chkConfirmationDelete.Text = "Display Delete confirmation dialog ";
            this.chkConfirmationDelete.UseVisualStyleBackColor = true;
            // 
            // chkAllowMultiInstances
            // 
            this.chkAllowMultiInstances.AutoSize = true;
            this.chkAllowMultiInstances.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkAllowMultiInstances.Location = new System.Drawing.Point(30, 250);
            this.chkAllowMultiInstances.Margin = new System.Windows.Forms.Padding(1);
            this.chkAllowMultiInstances.Name = "chkAllowMultiInstances";
            this.chkAllowMultiInstances.Size = new System.Drawing.Size(244, 20);
            this.chkAllowMultiInstances.TabIndex = 8;
            this.chkAllowMultiInstances.Text = "Allow multiple instances of the program";
            this.chkAllowMultiInstances.UseVisualStyleBackColor = true;
            // 
            // chkESCToQuit
            // 
            this.chkESCToQuit.AutoSize = true;
            this.chkESCToQuit.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkESCToQuit.Location = new System.Drawing.Point(30, 273);
            this.chkESCToQuit.Margin = new System.Windows.Forms.Padding(1);
            this.chkESCToQuit.Name = "chkESCToQuit";
            this.chkESCToQuit.Size = new System.Drawing.Size(229, 20);
            this.chkESCToQuit.TabIndex = 9;
            this.chkESCToQuit.Text = "Allow to press ESC to quit application";
            this.chkESCToQuit.UseVisualStyleBackColor = true;
            // 
            // chkShowToolBar
            // 
            this.chkShowToolBar.AutoSize = true;
            this.chkShowToolBar.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowToolBar.Location = new System.Drawing.Point(30, 80);
            this.chkShowToolBar.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowToolBar.Name = "chkShowToolBar";
            this.chkShowToolBar.Size = new System.Drawing.Size(194, 20);
            this.chkShowToolBar.TabIndex = 5;
            this.chkShowToolBar.Text = "Show toolbar when starting up";
            this.chkShowToolBar.UseVisualStyleBackColor = true;
            // 
            // lblBackGroundColor
            // 
            this.lblBackGroundColor.AutoSize = true;
            this.lblBackGroundColor.Location = new System.Drawing.Point(27, 421);
            this.lblBackGroundColor.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblBackGroundColor.Name = "lblBackGroundColor";
            this.lblBackGroundColor.Size = new System.Drawing.Size(104, 15);
            this.lblBackGroundColor.TabIndex = 11;
            this.lblBackGroundColor.Text = "Background color:";
            // 
            // chkWelcomePicture
            // 
            this.chkWelcomePicture.AutoSize = true;
            this.chkWelcomePicture.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkWelcomePicture.Location = new System.Drawing.Point(30, 35);
            this.chkWelcomePicture.Margin = new System.Windows.Forms.Padding(1);
            this.chkWelcomePicture.Name = "chkWelcomePicture";
            this.chkWelcomePicture.Size = new System.Drawing.Size(152, 20);
            this.chkWelcomePicture.TabIndex = 3;
            this.chkWelcomePicture.Text = "Show welcome picture";
            this.chkWelcomePicture.UseVisualStyleBackColor = true;
            // 
            // chkAutoUpdate
            // 
            this.chkAutoUpdate.AutoSize = true;
            this.chkAutoUpdate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkAutoUpdate.Location = new System.Drawing.Point(30, 228);
            this.chkAutoUpdate.Margin = new System.Windows.Forms.Padding(1);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(198, 20);
            this.chkAutoUpdate.TabIndex = 7;
            this.chkAutoUpdate.Text = "Check for update automatically";
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // tab1
            // 
            this.tab1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tab1.Controls.Add(this.tabGeneral);
            this.tab1.Controls.Add(this.tabImage);
            this.tab1.Controls.Add(this.tabEdit);
            this.tab1.Controls.Add(this.tabFileAssociation);
            this.tab1.Controls.Add(this.tabToolbar);
            this.tab1.Controls.Add(this.tabColorPicker);
            this.tab1.Controls.Add(this.tabLanguage);
            this.tab1.Controls.Add(this.tabTheme);
            this.tab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tab1.Location = new System.Drawing.Point(0, 0);
            this.tab1.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.tab1.Name = "tab1";
            this.tab1.Padding = new System.Drawing.Point(0, 0);
            this.tab1.SelectedIndex = 0;
            this.tab1.Size = new System.Drawing.Size(539, 417);
            this.tab1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tab1.TabIndex = 0;
            this.tab1.SelectedIndexChanged += new System.EventHandler(this.tab1_SelectedIndexChanged);
            // 
            // tabImage
            // 
            this.tabImage.AutoScroll = true;
            this.tabImage.BackColor = System.Drawing.Color.White;
            this.tabImage.Controls.Add(this.chkDisplayLoadMessage);
            this.tabImage.Controls.Add(this.lnkColorProfilePath);
            this.tabImage.Controls.Add(this.lnkColorProfileBrowse);
            this.tabImage.Controls.Add(this.lblColorManagement);
            this.tabImage.Controls.Add(this.chkApplyColorProfile);
            this.tabImage.Controls.Add(this.cmbColorProfile);
            this.tabImage.Controls.Add(this.lblColorProfile);
            this.tabImage.Controls.Add(this.chkShowThumbnailScrollbar);
            this.tabImage.Controls.Add(this.cmbMouseWheelAlt);
            this.tabImage.Controls.Add(this.cmbMouseWheelShift);
            this.tabImage.Controls.Add(this.cmbMouseWheelCtrl);
            this.tabImage.Controls.Add(this.cmbMouseWheel);
            this.tabImage.Controls.Add(this.lblMouseWheelAlt);
            this.tabImage.Controls.Add(this.lblMouseWheelShift);
            this.tabImage.Controls.Add(this.lblMouseWheelCtrl);
            this.tabImage.Controls.Add(this.lblMouseWheel);
            this.tabImage.Controls.Add(this.lblHeadMouseWheelActions);
            this.tabImage.Controls.Add(this.chkShowHiddenImages);
            this.tabImage.Controls.Add(this.panel3);
            this.tabImage.Controls.Add(this.lblHeadZooming);
            this.tabImage.Controls.Add(this.lblHeadSlideshow);
            this.tabImage.Controls.Add(this.lblHeadThumbnailBar);
            this.tabImage.Controls.Add(this.lblHeadImageLoading);
            this.tabImage.Controls.Add(this.chkLoopViewer);
            this.tabImage.Controls.Add(this.lblGeneral_ZoomOptimization);
            this.tabImage.Controls.Add(this.cmbZoomOptimization);
            this.tabImage.Controls.Add(this.chkThumbnailVertical);
            this.tabImage.Controls.Add(this.lblGeneral_ThumbnailSize);
            this.tabImage.Controls.Add(this.cmbThumbnailDimension);
            this.tabImage.Controls.Add(this.chkImageBoosterBack);
            this.tabImage.Controls.Add(this.chkLoopSlideshow);
            this.tabImage.Controls.Add(this.lblImageLoadingOrder);
            this.tabImage.Controls.Add(this.cmbImageOrder);
            this.tabImage.Controls.Add(this.barInterval);
            this.tabImage.Controls.Add(this.lblSlideshowInterval);
            this.tabImage.Controls.Add(this.chkFindChildFolder);
            this.tabImage.Location = new System.Drawing.Point(4, 27);
            this.tabImage.Margin = new System.Windows.Forms.Padding(0);
            this.tabImage.Name = "tabImage";
            this.tabImage.Size = new System.Drawing.Size(531, 386);
            this.tabImage.TabIndex = 3;
            this.tabImage.Text = "Image";
            // 
            // chkDisplayLoadMessage
            // 
            this.chkDisplayLoadMessage.AutoSize = true;
            this.chkDisplayLoadMessage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkDisplayLoadMessage.Location = new System.Drawing.Point(30, 120);
            this.chkDisplayLoadMessage.Margin = new System.Windows.Forms.Padding(1);
            this.chkDisplayLoadMessage.Name = "chkDisplayLoadMessage";
            this.chkDisplayLoadMessage.Size = new System.Drawing.Size(278, 20);
            this.chkDisplayLoadMessage.TabIndex = 60;
            this.chkDisplayLoadMessage.Text = "Display message when an image is still loading";
            this.chkDisplayLoadMessage.UseVisualStyleBackColor = true;
            // 
            // lnkColorProfilePath
            // 
            this.lnkColorProfilePath.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkColorProfilePath.AutoSize = true;
            this.lnkColorProfilePath.BackColor = System.Drawing.Color.Transparent;
            this.lnkColorProfilePath.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkColorProfilePath.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkColorProfilePath.Location = new System.Drawing.Point(27, 328);
            this.lnkColorProfilePath.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lnkColorProfilePath.Name = "lnkColorProfilePath";
            this.lnkColorProfilePath.Size = new System.Drawing.Size(105, 15);
            this.lnkColorProfilePath.TabIndex = 24;
            this.lnkColorProfilePath.TabStop = true;
            this.lnkColorProfilePath.Text = "C:\\abc\\custom.icc";
            this.lnkColorProfilePath.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkColorProfilePath.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkColorProfilePath_LinkClicked);
            // 
            // lnkColorProfileBrowse
            // 
            this.lnkColorProfileBrowse.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkColorProfileBrowse.AutoSize = true;
            this.lnkColorProfileBrowse.BackColor = System.Drawing.Color.Transparent;
            this.lnkColorProfileBrowse.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkColorProfileBrowse.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkColorProfileBrowse.Location = new System.Drawing.Point(219, 305);
            this.lnkColorProfileBrowse.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lnkColorProfileBrowse.Name = "lnkColorProfileBrowse";
            this.lnkColorProfileBrowse.Size = new System.Drawing.Size(53, 15);
            this.lnkColorProfileBrowse.TabIndex = 23;
            this.lnkColorProfileBrowse.TabStop = true;
            this.lnkColorProfileBrowse.Text = "[Browse]";
            this.lnkColorProfileBrowse.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkColorProfileBrowse.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkColorProfileBrowse_LinkClicked);
            // 
            // lblColorManagement
            // 
            this.lblColorManagement.AutoSize = true;
            this.lblColorManagement.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblColorManagement.Location = new System.Drawing.Point(15, 222);
            this.lblColorManagement.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblColorManagement.Name = "lblColorManagement";
            this.lblColorManagement.Size = new System.Drawing.Size(121, 15);
            this.lblColorManagement.TabIndex = 59;
            this.lblColorManagement.Text = "[Color management]";
            // 
            // chkApplyColorProfile
            // 
            this.chkApplyColorProfile.AutoSize = true;
            this.chkApplyColorProfile.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkApplyColorProfile.Location = new System.Drawing.Point(30, 244);
            this.chkApplyColorProfile.Margin = new System.Windows.Forms.Padding(1);
            this.chkApplyColorProfile.Name = "chkApplyColorProfile";
            this.chkApplyColorProfile.Size = new System.Drawing.Size(325, 20);
            this.chkApplyColorProfile.TabIndex = 21;
            this.chkApplyColorProfile.Text = "[Apply also for images without embedded color profile]";
            this.chkApplyColorProfile.UseVisualStyleBackColor = true;
            // 
            // cmbColorProfile
            // 
            this.cmbColorProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColorProfile.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbColorProfile.FormattingEnabled = true;
            this.cmbColorProfile.Items.AddRange(new object[] {
            "[None]",
            "[Custom ICC/ICM profile file:]"});
            this.cmbColorProfile.Location = new System.Drawing.Point(30, 303);
            this.cmbColorProfile.Margin = new System.Windows.Forms.Padding(1);
            this.cmbColorProfile.Name = "cmbColorProfile";
            this.cmbColorProfile.Size = new System.Drawing.Size(187, 23);
            this.cmbColorProfile.TabIndex = 22;
            this.cmbColorProfile.SelectedIndexChanged += new System.EventHandler(this.cmbColorProfile_SelectedIndexChanged);
            // 
            // lblColorProfile
            // 
            this.lblColorProfile.AutoSize = true;
            this.lblColorProfile.BackColor = System.Drawing.Color.Transparent;
            this.lblColorProfile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblColorProfile.Location = new System.Drawing.Point(27, 281);
            this.lblColorProfile.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.lblColorProfile.Name = "lblColorProfile";
            this.lblColorProfile.Size = new System.Drawing.Size(84, 15);
            this.lblColorProfile.TabIndex = 56;
            this.lblColorProfile.Text = "[Color profile:]";
            // 
            // chkShowThumbnailScrollbar
            // 
            this.chkShowThumbnailScrollbar.AutoSize = true;
            this.chkShowThumbnailScrollbar.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowThumbnailScrollbar.Location = new System.Drawing.Point(30, 790);
            this.chkShowThumbnailScrollbar.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowThumbnailScrollbar.Name = "chkShowThumbnailScrollbar";
            this.chkShowThumbnailScrollbar.Size = new System.Drawing.Size(195, 20);
            this.chkShowThumbnailScrollbar.TabIndex = 31;
            this.chkShowThumbnailScrollbar.Text = "[Show thumbnail bar scrollbar]";
            this.chkShowThumbnailScrollbar.UseVisualStyleBackColor = true;
            // 
            // cmbMouseWheelAlt
            // 
            this.cmbMouseWheelAlt.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMouseWheelAlt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbMouseWheelAlt.FormattingEnabled = true;
            this.cmbMouseWheelAlt.Location = new System.Drawing.Point(30, 587);
            this.cmbMouseWheelAlt.Name = "cmbMouseWheelAlt";
            this.cmbMouseWheelAlt.Size = new System.Drawing.Size(187, 23);
            this.cmbMouseWheelAlt.TabIndex = 28;
            // 
            // cmbMouseWheelShift
            // 
            this.cmbMouseWheelShift.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMouseWheelShift.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbMouseWheelShift.FormattingEnabled = true;
            this.cmbMouseWheelShift.Location = new System.Drawing.Point(30, 529);
            this.cmbMouseWheelShift.Name = "cmbMouseWheelShift";
            this.cmbMouseWheelShift.Size = new System.Drawing.Size(187, 23);
            this.cmbMouseWheelShift.TabIndex = 27;
            // 
            // cmbMouseWheelCtrl
            // 
            this.cmbMouseWheelCtrl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMouseWheelCtrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbMouseWheelCtrl.FormattingEnabled = true;
            this.cmbMouseWheelCtrl.Location = new System.Drawing.Point(30, 471);
            this.cmbMouseWheelCtrl.Name = "cmbMouseWheelCtrl";
            this.cmbMouseWheelCtrl.Size = new System.Drawing.Size(187, 23);
            this.cmbMouseWheelCtrl.TabIndex = 26;
            // 
            // cmbMouseWheel
            // 
            this.cmbMouseWheel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMouseWheel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbMouseWheel.FormattingEnabled = true;
            this.cmbMouseWheel.Location = new System.Drawing.Point(30, 414);
            this.cmbMouseWheel.Name = "cmbMouseWheel";
            this.cmbMouseWheel.Size = new System.Drawing.Size(187, 23);
            this.cmbMouseWheel.TabIndex = 25;
            // 
            // lblMouseWheelAlt
            // 
            this.lblMouseWheelAlt.AutoSize = true;
            this.lblMouseWheelAlt.Location = new System.Drawing.Point(27, 568);
            this.lblMouseWheelAlt.Name = "lblMouseWheelAlt";
            this.lblMouseWheelAlt.Size = new System.Drawing.Size(106, 15);
            this.lblMouseWheelAlt.TabIndex = 52;
            this.lblMouseWheelAlt.Text = "Mouse wheel + Alt";
            // 
            // lblMouseWheelShift
            // 
            this.lblMouseWheelShift.AutoSize = true;
            this.lblMouseWheelShift.Location = new System.Drawing.Point(27, 510);
            this.lblMouseWheelShift.Name = "lblMouseWheelShift";
            this.lblMouseWheelShift.Size = new System.Drawing.Size(115, 15);
            this.lblMouseWheelShift.TabIndex = 51;
            this.lblMouseWheelShift.Text = "Mouse wheel + Shift";
            // 
            // lblMouseWheelCtrl
            // 
            this.lblMouseWheelCtrl.AutoSize = true;
            this.lblMouseWheelCtrl.Location = new System.Drawing.Point(27, 451);
            this.lblMouseWheelCtrl.Name = "lblMouseWheelCtrl";
            this.lblMouseWheelCtrl.Size = new System.Drawing.Size(110, 15);
            this.lblMouseWheelCtrl.TabIndex = 50;
            this.lblMouseWheelCtrl.Text = "Mouse wheel + Ctrl";
            // 
            // lblMouseWheel
            // 
            this.lblMouseWheel.AutoSize = true;
            this.lblMouseWheel.Location = new System.Drawing.Point(27, 395);
            this.lblMouseWheel.Name = "lblMouseWheel";
            this.lblMouseWheel.Size = new System.Drawing.Size(77, 15);
            this.lblMouseWheel.TabIndex = 49;
            this.lblMouseWheel.Text = "Mouse wheel";
            // 
            // lblHeadMouseWheelActions
            // 
            this.lblHeadMouseWheelActions.AutoSize = true;
            this.lblHeadMouseWheelActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblHeadMouseWheelActions.Location = new System.Drawing.Point(15, 371);
            this.lblHeadMouseWheelActions.Name = "lblHeadMouseWheelActions";
            this.lblHeadMouseWheelActions.Size = new System.Drawing.Size(123, 15);
            this.lblHeadMouseWheelActions.TabIndex = 48;
            this.lblHeadMouseWheelActions.Text = "Mouse wheel actions";
            // 
            // chkShowHiddenImages
            // 
            this.chkShowHiddenImages.AutoSize = true;
            this.chkShowHiddenImages.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowHiddenImages.Location = new System.Drawing.Point(30, 55);
            this.chkShowHiddenImages.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowHiddenImages.Name = "chkShowHiddenImages";
            this.chkShowHiddenImages.Size = new System.Drawing.Size(142, 20);
            this.chkShowHiddenImages.TabIndex = 17;
            this.chkShowHiddenImages.Text = "Show hidden images";
            this.chkShowHiddenImages.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Location = new System.Drawing.Point(18, 1026);
            this.panel3.Margin = new System.Windows.Forms.Padding(1);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(73, 34);
            this.panel3.TabIndex = 46;
            // 
            // lblHeadZooming
            // 
            this.lblHeadZooming.AutoSize = true;
            this.lblHeadZooming.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadZooming.Location = new System.Drawing.Point(15, 647);
            this.lblHeadZooming.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeadZooming.Name = "lblHeadZooming";
            this.lblHeadZooming.Size = new System.Drawing.Size(56, 15);
            this.lblHeadZooming.TabIndex = 43;
            this.lblHeadZooming.Text = "Zooming";
            // 
            // lblHeadSlideshow
            // 
            this.lblHeadSlideshow.AutoSize = true;
            this.lblHeadSlideshow.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadSlideshow.Location = new System.Drawing.Point(15, 903);
            this.lblHeadSlideshow.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeadSlideshow.Name = "lblHeadSlideshow";
            this.lblHeadSlideshow.Size = new System.Drawing.Size(63, 15);
            this.lblHeadSlideshow.TabIndex = 42;
            this.lblHeadSlideshow.Text = "Slideshow";
            // 
            // lblHeadThumbnailBar
            // 
            this.lblHeadThumbnailBar.AutoSize = true;
            this.lblHeadThumbnailBar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadThumbnailBar.Location = new System.Drawing.Point(15, 745);
            this.lblHeadThumbnailBar.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeadThumbnailBar.Name = "lblHeadThumbnailBar";
            this.lblHeadThumbnailBar.Size = new System.Drawing.Size(86, 15);
            this.lblHeadThumbnailBar.TabIndex = 41;
            this.lblHeadThumbnailBar.Text = "Thumbnail bar";
            // 
            // lblHeadImageLoading
            // 
            this.lblHeadImageLoading.AutoSize = true;
            this.lblHeadImageLoading.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadImageLoading.Location = new System.Drawing.Point(15, 11);
            this.lblHeadImageLoading.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeadImageLoading.Name = "lblHeadImageLoading";
            this.lblHeadImageLoading.Size = new System.Drawing.Size(85, 15);
            this.lblHeadImageLoading.TabIndex = 40;
            this.lblHeadImageLoading.Text = "Image loading";
            // 
            // chkLoopViewer
            // 
            this.chkLoopViewer.AutoSize = true;
            this.chkLoopViewer.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkLoopViewer.Location = new System.Drawing.Point(30, 76);
            this.chkLoopViewer.Margin = new System.Windows.Forms.Padding(1);
            this.chkLoopViewer.Name = "chkLoopViewer";
            this.chkLoopViewer.Size = new System.Drawing.Size(393, 20);
            this.chkLoopViewer.TabIndex = 18;
            this.chkLoopViewer.Text = "Loop back viewer to the first image when reaching the end of the list";
            this.chkLoopViewer.UseVisualStyleBackColor = true;
            // 
            // lblGeneral_ZoomOptimization
            // 
            this.lblGeneral_ZoomOptimization.AutoSize = true;
            this.lblGeneral_ZoomOptimization.Location = new System.Drawing.Point(27, 671);
            this.lblGeneral_ZoomOptimization.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblGeneral_ZoomOptimization.Name = "lblGeneral_ZoomOptimization";
            this.lblGeneral_ZoomOptimization.Size = new System.Drawing.Size(109, 15);
            this.lblGeneral_ZoomOptimization.TabIndex = 36;
            this.lblGeneral_ZoomOptimization.Text = "Zoom optimization";
            // 
            // cmbZoomOptimization
            // 
            this.cmbZoomOptimization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbZoomOptimization.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbZoomOptimization.FormattingEnabled = true;
            this.cmbZoomOptimization.Items.AddRange(new object[] {
            "(loaded from code)"});
            this.cmbZoomOptimization.Location = new System.Drawing.Point(30, 689);
            this.cmbZoomOptimization.Margin = new System.Windows.Forms.Padding(1);
            this.cmbZoomOptimization.Name = "cmbZoomOptimization";
            this.cmbZoomOptimization.Size = new System.Drawing.Size(187, 23);
            this.cmbZoomOptimization.TabIndex = 29;
            // 
            // chkThumbnailVertical
            // 
            this.chkThumbnailVertical.AutoSize = true;
            this.chkThumbnailVertical.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkThumbnailVertical.Location = new System.Drawing.Point(30, 767);
            this.chkThumbnailVertical.Margin = new System.Windows.Forms.Padding(1);
            this.chkThumbnailVertical.Name = "chkThumbnailVertical";
            this.chkThumbnailVertical.Size = new System.Drawing.Size(179, 20);
            this.chkThumbnailVertical.TabIndex = 30;
            this.chkThumbnailVertical.Text = "Thumbnail bar on right side";
            this.chkThumbnailVertical.UseVisualStyleBackColor = true;
            // 
            // lblGeneral_ThumbnailSize
            // 
            this.lblGeneral_ThumbnailSize.AutoSize = true;
            this.lblGeneral_ThumbnailSize.Location = new System.Drawing.Point(27, 829);
            this.lblGeneral_ThumbnailSize.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblGeneral_ThumbnailSize.Name = "lblGeneral_ThumbnailSize";
            this.lblGeneral_ThumbnailSize.Size = new System.Drawing.Size(175, 15);
            this.lblGeneral_ThumbnailSize.TabIndex = 33;
            this.lblGeneral_ThumbnailSize.Text = "Thumbnail dimension (in pixel):";
            // 
            // cmbThumbnailDimension
            // 
            this.cmbThumbnailDimension.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbThumbnailDimension.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbThumbnailDimension.FormattingEnabled = true;
            this.cmbThumbnailDimension.Items.AddRange(new object[] {
            "32",
            "48",
            "64",
            "96",
            "128",
            "256",
            "512",
            "1024"});
            this.cmbThumbnailDimension.Location = new System.Drawing.Point(32, 849);
            this.cmbThumbnailDimension.Margin = new System.Windows.Forms.Padding(1);
            this.cmbThumbnailDimension.Name = "cmbThumbnailDimension";
            this.cmbThumbnailDimension.Size = new System.Drawing.Size(187, 23);
            this.cmbThumbnailDimension.TabIndex = 32;
            // 
            // chkImageBoosterBack
            // 
            this.chkImageBoosterBack.AutoSize = true;
            this.chkImageBoosterBack.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkImageBoosterBack.Location = new System.Drawing.Point(30, 98);
            this.chkImageBoosterBack.Margin = new System.Windows.Forms.Padding(1);
            this.chkImageBoosterBack.Name = "chkImageBoosterBack";
            this.chkImageBoosterBack.Size = new System.Drawing.Size(391, 20);
            this.chkImageBoosterBack.TabIndex = 19;
            this.chkImageBoosterBack.Text = "Turn on Image Booster when navigate back (need more ~20% RAM)";
            this.chkImageBoosterBack.UseVisualStyleBackColor = true;
            // 
            // chkLoopSlideshow
            // 
            this.chkLoopSlideshow.AutoSize = true;
            this.chkLoopSlideshow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkLoopSlideshow.Location = new System.Drawing.Point(30, 925);
            this.chkLoopSlideshow.Margin = new System.Windows.Forms.Padding(1);
            this.chkLoopSlideshow.Name = "chkLoopSlideshow";
            this.chkLoopSlideshow.Size = new System.Drawing.Size(411, 20);
            this.chkLoopSlideshow.TabIndex = 33;
            this.chkLoopSlideshow.Text = "Loop back slideshow to the first image when reaching the end of the list";
            this.chkLoopSlideshow.UseVisualStyleBackColor = true;
            // 
            // lblImageLoadingOrder
            // 
            this.lblImageLoadingOrder.AutoSize = true;
            this.lblImageLoadingOrder.Location = new System.Drawing.Point(27, 152);
            this.lblImageLoadingOrder.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblImageLoadingOrder.Name = "lblImageLoadingOrder";
            this.lblImageLoadingOrder.Size = new System.Drawing.Size(117, 15);
            this.lblImageLoadingOrder.TabIndex = 28;
            this.lblImageLoadingOrder.Text = "Image loading order:";
            // 
            // cmbImageOrder
            // 
            this.cmbImageOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbImageOrder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbImageOrder.FormattingEnabled = true;
            this.cmbImageOrder.Items.AddRange(new object[] {
            "Name (default)",
            "Length",
            "Creation time",
            "Last access time",
            "Last write time",
            "Extension",
            "Random"});
            this.cmbImageOrder.Location = new System.Drawing.Point(30, 171);
            this.cmbImageOrder.Margin = new System.Windows.Forms.Padding(1);
            this.cmbImageOrder.Name = "cmbImageOrder";
            this.cmbImageOrder.Size = new System.Drawing.Size(187, 23);
            this.cmbImageOrder.TabIndex = 20;
            // 
            // barInterval
            // 
            this.barInterval.BackColor = System.Drawing.SystemColors.Window;
            this.barInterval.Location = new System.Drawing.Point(31, 981);
            this.barInterval.Margin = new System.Windows.Forms.Padding(1);
            this.barInterval.Maximum = 60;
            this.barInterval.Minimum = 1;
            this.barInterval.Name = "barInterval";
            this.barInterval.Size = new System.Drawing.Size(195, 45);
            this.barInterval.TabIndex = 34;
            this.barInterval.TickStyle = System.Windows.Forms.TickStyle.None;
            this.barInterval.Value = 5;
            this.barInterval.Scroll += new System.EventHandler(this.barInterval_Scroll);
            // 
            // lblSlideshowInterval
            // 
            this.lblSlideshowInterval.AutoSize = true;
            this.lblSlideshowInterval.Location = new System.Drawing.Point(27, 958);
            this.lblSlideshowInterval.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblSlideshowInterval.Name = "lblSlideshowInterval";
            this.lblSlideshowInterval.Size = new System.Drawing.Size(163, 15);
            this.lblSlideshowInterval.TabIndex = 24;
            this.lblSlideshowInterval.Text = "Slide show interval: 5 seconds";
            // 
            // chkFindChildFolder
            // 
            this.chkFindChildFolder.AutoSize = true;
            this.chkFindChildFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkFindChildFolder.Location = new System.Drawing.Point(30, 33);
            this.chkFindChildFolder.Margin = new System.Windows.Forms.Padding(1);
            this.chkFindChildFolder.Name = "chkFindChildFolder";
            this.chkFindChildFolder.Size = new System.Drawing.Size(172, 20);
            this.chkFindChildFolder.TabIndex = 16;
            this.chkFindChildFolder.Text = "Find images in child folder";
            this.chkFindChildFolder.UseVisualStyleBackColor = true;
            // 
            // tabEdit
            // 
            this.tabEdit.BackColor = System.Drawing.Color.White;
            this.tabEdit.Controls.Add(this.lblSelectAppForEdit);
            this.tabEdit.Controls.Add(this.chkSaveOnRotate);
            this.tabEdit.Controls.Add(this.chkSaveModifyDate);
            this.tabEdit.Controls.Add(this.btnEditEditAllExt);
            this.tabEdit.Controls.Add(this.btnEditResetExt);
            this.tabEdit.Controls.Add(this.btnEditEditExt);
            this.tabEdit.Controls.Add(this.lvImageEditing);
            this.tabEdit.Location = new System.Drawing.Point(4, 27);
            this.tabEdit.Margin = new System.Windows.Forms.Padding(2);
            this.tabEdit.Name = "tabEdit";
            this.tabEdit.Padding = new System.Windows.Forms.Padding(2);
            this.tabEdit.Size = new System.Drawing.Size(531, 386);
            this.tabEdit.TabIndex = 7;
            this.tabEdit.Text = "Edit";
            // 
            // lblSelectAppForEdit
            // 
            this.lblSelectAppForEdit.AutoSize = true;
            this.lblSelectAppForEdit.Location = new System.Drawing.Point(11, 75);
            this.lblSelectAppForEdit.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblSelectAppForEdit.Name = "lblSelectAppForEdit";
            this.lblSelectAppForEdit.Size = new System.Drawing.Size(197, 15);
            this.lblSelectAppForEdit.TabIndex = 59;
            this.lblSelectAppForEdit.Text = "Select application for image editing:";
            // 
            // chkSaveOnRotate
            // 
            this.chkSaveOnRotate.AutoSize = true;
            this.chkSaveOnRotate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkSaveOnRotate.Location = new System.Drawing.Point(15, 13);
            this.chkSaveOnRotate.Margin = new System.Windows.Forms.Padding(1);
            this.chkSaveOnRotate.Name = "chkSaveOnRotate";
            this.chkSaveOnRotate.Size = new System.Drawing.Size(228, 20);
            this.chkSaveOnRotate.TabIndex = 35;
            this.chkSaveOnRotate.Text = "Save the viewing image after rotating";
            this.chkSaveOnRotate.UseVisualStyleBackColor = true;
            // 
            // chkSaveModifyDate
            // 
            this.chkSaveModifyDate.AutoSize = true;
            this.chkSaveModifyDate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkSaveModifyDate.Location = new System.Drawing.Point(15, 39);
            this.chkSaveModifyDate.Margin = new System.Windows.Forms.Padding(1);
            this.chkSaveModifyDate.Name = "chkSaveModifyDate";
            this.chkSaveModifyDate.Size = new System.Drawing.Size(236, 20);
            this.chkSaveModifyDate.TabIndex = 36;
            this.chkSaveModifyDate.Text = "Preserve the modification date on save";
            this.chkSaveModifyDate.UseVisualStyleBackColor = true;
            // 
            // btnEditEditAllExt
            // 
            this.btnEditEditAllExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEditEditAllExt.AutoSize = true;
            this.btnEditEditAllExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnEditEditAllExt.Location = new System.Drawing.Point(107, 324);
            this.btnEditEditAllExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnEditEditAllExt.Name = "btnEditEditAllExt";
            this.btnEditEditAllExt.Size = new System.Drawing.Size(177, 30);
            this.btnEditEditAllExt.TabIndex = 39;
            this.btnEditEditAllExt.Text = "Edit all extensions";
            this.btnEditEditAllExt.UseVisualStyleBackColor = true;
            this.btnEditEditAllExt.Click += new System.EventHandler(this.btnEditEditAllExt_Click);
            // 
            // btnEditResetExt
            // 
            this.btnEditResetExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEditResetExt.AutoSize = true;
            this.btnEditResetExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnEditResetExt.Location = new System.Drawing.Point(288, 324);
            this.btnEditResetExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnEditResetExt.Name = "btnEditResetExt";
            this.btnEditResetExt.Size = new System.Drawing.Size(163, 30);
            this.btnEditResetExt.TabIndex = 40;
            this.btnEditResetExt.Text = "Reset to default";
            this.btnEditResetExt.UseVisualStyleBackColor = true;
            this.btnEditResetExt.Click += new System.EventHandler(this.btnEditResetExt_Click);
            // 
            // btnEditEditExt
            // 
            this.btnEditEditExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEditEditExt.AutoSize = true;
            this.btnEditEditExt.Enabled = false;
            this.btnEditEditExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnEditEditExt.Location = new System.Drawing.Point(15, 324);
            this.btnEditEditExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnEditEditExt.Name = "btnEditEditExt";
            this.btnEditEditExt.Size = new System.Drawing.Size(89, 30);
            this.btnEditEditExt.TabIndex = 38;
            this.btnEditEditExt.Text = "Edit";
            this.btnEditEditExt.UseVisualStyleBackColor = true;
            this.btnEditEditExt.Click += new System.EventHandler(this.btnEditEditExt_Click);
            // 
            // lvImageEditing
            // 
            this.lvImageEditing.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvImageEditing.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clnFileExtension,
            this.clnAppName,
            this.clnAppPath,
            this.clnAppArguments});
            this.lvImageEditing.FullRowSelect = true;
            this.lvImageEditing.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewItem1.StateImageIndex = 0;
            listViewItem2.StateImageIndex = 0;
            listViewItem3.StateImageIndex = 0;
            this.lvImageEditing.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.lvImageEditing.Location = new System.Drawing.Point(15, 94);
            this.lvImageEditing.Margin = new System.Windows.Forms.Padding(2);
            this.lvImageEditing.MultiSelect = false;
            this.lvImageEditing.Name = "lvImageEditing";
            this.lvImageEditing.RightToLeftLayout = true;
            this.lvImageEditing.ShowItemToolTips = true;
            this.lvImageEditing.Size = new System.Drawing.Size(506, 216);
            this.lvImageEditing.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvImageEditing.TabIndex = 37;
            this.lvImageEditing.UseCompatibleStateImageBehavior = false;
            this.lvImageEditing.View = System.Windows.Forms.View.Details;
            this.lvImageEditing.SelectedIndexChanged += new System.EventHandler(this.lvlvImageEditing_SelectedIndexChanged);
            // 
            // clnFileExtension
            // 
            this.clnFileExtension.Text = "File extension";
            this.clnFileExtension.Width = 120;
            // 
            // clnAppName
            // 
            this.clnAppName.Text = "App name";
            this.clnAppName.Width = 200;
            // 
            // clnAppPath
            // 
            this.clnAppPath.Text = "App path";
            this.clnAppPath.Width = 400;
            // 
            // clnAppArguments
            // 
            this.clnAppArguments.Text = "App arguments";
            this.clnAppArguments.Width = 200;
            // 
            // tabToolbar
            // 
            this.tabToolbar.AutoScroll = true;
            this.tabToolbar.BackColor = System.Drawing.Color.White;
            this.tabToolbar.Controls.Add(this.panel5);
            this.tabToolbar.Controls.Add(this.cmbToolbarPosition);
            this.tabToolbar.Controls.Add(this.lblToolbarPosition);
            this.tabToolbar.Controls.Add(this.chkHorzCenterToolbarBtns);
            this.tabToolbar.Location = new System.Drawing.Point(4, 27);
            this.tabToolbar.Margin = new System.Windows.Forms.Padding(0);
            this.tabToolbar.Name = "tabToolbar";
            this.tabToolbar.Size = new System.Drawing.Size(531, 386);
            this.tabToolbar.TabIndex = 4;
            this.tabToolbar.Text = "toolbar";
            this.tabToolbar.UseVisualStyleBackColor = true;
            // 
            // panel5
            // 
            this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel5.Controls.Add(this.lblAvailBtns);
            this.panel5.Controls.Add(this.btnMoveRight);
            this.panel5.Controls.Add(this.btnMoveLeft);
            this.panel5.Controls.Add(this.btnMoveUp);
            this.panel5.Controls.Add(this.lvAvailButtons);
            this.panel5.Controls.Add(this.lblUsedBtns);
            this.panel5.Controls.Add(this.lvUsedButtons);
            this.panel5.Controls.Add(this.btnMoveDown);
            this.panel5.Location = new System.Drawing.Point(15, 96);
            this.panel5.Margin = new System.Windows.Forms.Padding(2);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(507, 276);
            this.panel5.TabIndex = 48;
            // 
            // lblAvailBtns
            // 
            this.lblAvailBtns.AutoSize = true;
            this.lblAvailBtns.BackColor = System.Drawing.Color.Transparent;
            this.lblAvailBtns.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvailBtns.Location = new System.Drawing.Point(1, 3);
            this.lblAvailBtns.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.lblAvailBtns.Name = "lblAvailBtns";
            this.lblAvailBtns.Size = new System.Drawing.Size(110, 15);
            this.lblAvailBtns.TabIndex = 3;
            this.lblAvailBtns.Text = "[Available Buttons:]";
            // 
            // btnMoveRight
            // 
            this.btnMoveRight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnMoveRight.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveRight.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveRight.Location = new System.Drawing.Point(214, 165);
            this.btnMoveRight.Name = "btnMoveRight";
            this.btnMoveRight.Size = new System.Drawing.Size(33, 33);
            this.btnMoveRight.TabIndex = 52;
            this.btnMoveRight.Text = "►";
            this.btnMoveRight.UseVisualStyleBackColor = true;
            this.btnMoveRight.Click += new System.EventHandler(this.btnMoveRight_Click);
            // 
            // btnMoveLeft
            // 
            this.btnMoveLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnMoveLeft.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveLeft.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveLeft.Location = new System.Drawing.Point(214, 119);
            this.btnMoveLeft.Margin = new System.Windows.Forms.Padding(2);
            this.btnMoveLeft.Name = "btnMoveLeft";
            this.btnMoveLeft.Size = new System.Drawing.Size(33, 33);
            this.btnMoveLeft.TabIndex = 51;
            this.btnMoveLeft.Text = "◄";
            this.btnMoveLeft.UseVisualStyleBackColor = true;
            this.btnMoveLeft.Click += new System.EventHandler(this.btnMoveLeft_Click);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnMoveUp.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveUp.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveUp.Location = new System.Drawing.Point(465, 119);
            this.btnMoveUp.Margin = new System.Windows.Forms.Padding(2);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(33, 33);
            this.btnMoveUp.TabIndex = 53;
            this.btnMoveUp.Text = "▲";
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // lvAvailButtons
            // 
            this.lvAvailButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lvAvailButtons.BackColor = System.Drawing.Color.Black;
            this.lvAvailButtons.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvAvailButtons.ForeColor = System.Drawing.SystemColors.Window;
            this.lvAvailButtons.FullRowSelect = true;
            this.lvAvailButtons.GridLines = true;
            this.lvAvailButtons.HideSelection = false;
            this.lvAvailButtons.Location = new System.Drawing.Point(5, 25);
            this.lvAvailButtons.Margin = new System.Windows.Forms.Padding(2);
            this.lvAvailButtons.Name = "lvAvailButtons";
            this.lvAvailButtons.ShowGroups = false;
            this.lvAvailButtons.ShowItemToolTips = true;
            this.lvAvailButtons.Size = new System.Drawing.Size(200, 236);
            this.lvAvailButtons.TabIndex = 49;
            this.lvAvailButtons.UseCompatibleStateImageBehavior = false;
            this.lvAvailButtons.SelectedIndexChanged += new System.EventHandler(this.lvAvailButtons_SelectedIndexChanged);
            this.lvAvailButtons.Resize += new System.EventHandler(this.ButtonsListView_Resize);
            // 
            // lblUsedBtns
            // 
            this.lblUsedBtns.AutoSize = true;
            this.lblUsedBtns.BackColor = System.Drawing.Color.Transparent;
            this.lblUsedBtns.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsedBtns.Location = new System.Drawing.Point(258, 3);
            this.lblUsedBtns.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.lblUsedBtns.Name = "lblUsedBtns";
            this.lblUsedBtns.Size = new System.Drawing.Size(102, 15);
            this.lblUsedBtns.TabIndex = 8;
            this.lblUsedBtns.Text = "[Current Buttons:]";
            // 
            // lvUsedButtons
            // 
            this.lvUsedButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lvUsedButtons.BackColor = System.Drawing.Color.Black;
            this.lvUsedButtons.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvUsedButtons.ForeColor = System.Drawing.SystemColors.Window;
            this.lvUsedButtons.FullRowSelect = true;
            this.lvUsedButtons.HideSelection = false;
            this.lvUsedButtons.Location = new System.Drawing.Point(256, 25);
            this.lvUsedButtons.Margin = new System.Windows.Forms.Padding(2);
            this.lvUsedButtons.Name = "lvUsedButtons";
            this.lvUsedButtons.ShowGroups = false;
            this.lvUsedButtons.ShowItemToolTips = true;
            this.lvUsedButtons.Size = new System.Drawing.Size(200, 236);
            this.lvUsedButtons.TabIndex = 50;
            this.lvUsedButtons.UseCompatibleStateImageBehavior = false;
            this.lvUsedButtons.View = System.Windows.Forms.View.List;
            this.lvUsedButtons.SelectedIndexChanged += new System.EventHandler(this.lvUsedButtons_SelectedIndexChanged);
            this.lvUsedButtons.Resize += new System.EventHandler(this.ButtonsListView_Resize);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnMoveDown.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveDown.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveDown.Location = new System.Drawing.Point(465, 165);
            this.btnMoveDown.Margin = new System.Windows.Forms.Padding(2);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(33, 33);
            this.btnMoveDown.TabIndex = 54;
            this.btnMoveDown.Text = "▼";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // cmbToolbarPosition
            // 
            this.cmbToolbarPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbToolbarPosition.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbToolbarPosition.FormattingEnabled = true;
            this.cmbToolbarPosition.Location = new System.Drawing.Point(18, 35);
            this.cmbToolbarPosition.Margin = new System.Windows.Forms.Padding(1);
            this.cmbToolbarPosition.Name = "cmbToolbarPosition";
            this.cmbToolbarPosition.Size = new System.Drawing.Size(187, 23);
            this.cmbToolbarPosition.TabIndex = 47;
            // 
            // lblToolbarPosition
            // 
            this.lblToolbarPosition.AutoSize = true;
            this.lblToolbarPosition.BackColor = System.Drawing.Color.Transparent;
            this.lblToolbarPosition.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToolbarPosition.Location = new System.Drawing.Point(15, 13);
            this.lblToolbarPosition.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.lblToolbarPosition.Name = "lblToolbarPosition";
            this.lblToolbarPosition.Size = new System.Drawing.Size(104, 15);
            this.lblToolbarPosition.TabIndex = 44;
            this.lblToolbarPosition.Text = "[Toolbar position:]";
            // 
            // chkHorzCenterToolbarBtns
            // 
            this.chkHorzCenterToolbarBtns.AutoSize = true;
            this.chkHorzCenterToolbarBtns.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkHorzCenterToolbarBtns.Location = new System.Drawing.Point(19, 62);
            this.chkHorzCenterToolbarBtns.Margin = new System.Windows.Forms.Padding(1);
            this.chkHorzCenterToolbarBtns.Name = "chkHorzCenterToolbarBtns";
            this.chkHorzCenterToolbarBtns.Size = new System.Drawing.Size(283, 20);
            this.chkHorzCenterToolbarBtns.TabIndex = 48;
            this.chkHorzCenterToolbarBtns.Text = "[Center toolbar buttons horizontally in window]";
            this.chkHorzCenterToolbarBtns.UseVisualStyleBackColor = true;
            // 
            // tabColorPicker
            // 
            this.tabColorPicker.AutoScroll = true;
            this.tabColorPicker.BackColor = System.Drawing.Color.White;
            this.tabColorPicker.Controls.Add(this.chkColorUseHSLA);
            this.tabColorPicker.Controls.Add(this.lblColorCodeFormat);
            this.tabColorPicker.Controls.Add(this.chkColorUseHEXA);
            this.tabColorPicker.Controls.Add(this.chkColorUseRGBA);
            this.tabColorPicker.Location = new System.Drawing.Point(4, 27);
            this.tabColorPicker.Margin = new System.Windows.Forms.Padding(0);
            this.tabColorPicker.Name = "tabColorPicker";
            this.tabColorPicker.Size = new System.Drawing.Size(531, 386);
            this.tabColorPicker.TabIndex = 5;
            this.tabColorPicker.Text = "color picker";
            // 
            // chkColorUseHSLA
            // 
            this.chkColorUseHSLA.AutoSize = true;
            this.chkColorUseHSLA.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkColorUseHSLA.Location = new System.Drawing.Point(30, 80);
            this.chkColorUseHSLA.Margin = new System.Windows.Forms.Padding(1);
            this.chkColorUseHSLA.Name = "chkColorUseHSLA";
            this.chkColorUseHSLA.Size = new System.Drawing.Size(122, 20);
            this.chkColorUseHSLA.TabIndex = 57;
            this.chkColorUseHSLA.Text = "Use HSLA format";
            this.chkColorUseHSLA.UseVisualStyleBackColor = true;
            // 
            // lblColorCodeFormat
            // 
            this.lblColorCodeFormat.AutoSize = true;
            this.lblColorCodeFormat.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblColorCodeFormat.Location = new System.Drawing.Point(15, 13);
            this.lblColorCodeFormat.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblColorCodeFormat.Name = "lblColorCodeFormat";
            this.lblColorCodeFormat.Size = new System.Drawing.Size(108, 15);
            this.lblColorCodeFormat.TabIndex = 47;
            this.lblColorCodeFormat.Text = "Color code format";
            // 
            // chkColorUseHEXA
            // 
            this.chkColorUseHEXA.AutoSize = true;
            this.chkColorUseHEXA.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkColorUseHEXA.Location = new System.Drawing.Point(30, 57);
            this.chkColorUseHEXA.Margin = new System.Windows.Forms.Padding(1);
            this.chkColorUseHEXA.Name = "chkColorUseHEXA";
            this.chkColorUseHEXA.Size = new System.Drawing.Size(173, 20);
            this.chkColorUseHEXA.TabIndex = 56;
            this.chkColorUseHEXA.Text = "Use HEX with alpha format";
            this.chkColorUseHEXA.UseVisualStyleBackColor = true;
            // 
            // chkColorUseRGBA
            // 
            this.chkColorUseRGBA.AutoSize = true;
            this.chkColorUseRGBA.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkColorUseRGBA.Location = new System.Drawing.Point(30, 35);
            this.chkColorUseRGBA.Margin = new System.Windows.Forms.Padding(1);
            this.chkColorUseRGBA.Name = "chkColorUseRGBA";
            this.chkColorUseRGBA.Size = new System.Drawing.Size(123, 20);
            this.chkColorUseRGBA.TabIndex = 55;
            this.chkColorUseRGBA.Text = "Use RGBA format";
            this.chkColorUseRGBA.UseVisualStyleBackColor = true;
            // 
            // tabTheme
            // 
            this.tabTheme.AutoScroll = true;
            this.tabTheme.BackColor = System.Drawing.Color.White;
            this.tabTheme.Controls.Add(this.panelThemeActions);
            this.tabTheme.Controls.Add(this.lvTheme);
            this.tabTheme.Controls.Add(this.btnThemeApply);
            this.tabTheme.Controls.Add(this.lnkThemeDownload);
            this.tabTheme.Controls.Add(this.btnThemeEdit);
            this.tabTheme.Controls.Add(this.lblInstalledThemes);
            this.tabTheme.Location = new System.Drawing.Point(4, 27);
            this.tabTheme.Margin = new System.Windows.Forms.Padding(0);
            this.tabTheme.Name = "tabTheme";
            this.tabTheme.Size = new System.Drawing.Size(531, 386);
            this.tabTheme.TabIndex = 6;
            this.tabTheme.Text = "Theme";
            // 
            // panelThemeActions
            // 
            this.panelThemeActions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelThemeActions.AutoScroll = true;
            this.panelThemeActions.Controls.Add(this.picPreview);
            this.panelThemeActions.Controls.Add(this.btnThemeFolderOpen);
            this.panelThemeActions.Controls.Add(this.btnThemeSaveAs);
            this.panelThemeActions.Controls.Add(this.txtThemeInfo);
            this.panelThemeActions.Controls.Add(this.btnThemeRefresh);
            this.panelThemeActions.Controls.Add(this.btnThemeInstall);
            this.panelThemeActions.Controls.Add(this.btnThemeUninstall);
            this.panelThemeActions.Location = new System.Drawing.Point(356, 32);
            this.panelThemeActions.Margin = new System.Windows.Forms.Padding(2);
            this.panelThemeActions.Name = "panelThemeActions";
            this.panelThemeActions.Size = new System.Drawing.Size(169, 284);
            this.panelThemeActions.TabIndex = 29;
            // 
            // picPreview
            // 
            this.picPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picPreview.BackColor = System.Drawing.Color.Transparent;
            this.picPreview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picPreview.Location = new System.Drawing.Point(2, 2);
            this.picPreview.Margin = new System.Windows.Forms.Padding(2);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(165, 67);
            this.picPreview.TabIndex = 34;
            this.picPreview.TabStop = false;
            // 
            // btnThemeFolderOpen
            // 
            this.btnThemeFolderOpen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThemeFolderOpen.AutoSize = true;
            this.btnThemeFolderOpen.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeFolderOpen.Location = new System.Drawing.Point(2, 204);
            this.btnThemeFolderOpen.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.btnThemeFolderOpen.Name = "btnThemeFolderOpen";
            this.btnThemeFolderOpen.Size = new System.Drawing.Size(165, 29);
            this.btnThemeFolderOpen.TabIndex = 69;
            this.btnThemeFolderOpen.Text = "Open theme folder";
            this.btnThemeFolderOpen.UseVisualStyleBackColor = true;
            this.btnThemeFolderOpen.Click += new System.EventHandler(this.btnThemeFolderOpen_Click);
            // 
            // btnThemeSaveAs
            // 
            this.btnThemeSaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThemeSaveAs.AutoSize = true;
            this.btnThemeSaveAs.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeSaveAs.Location = new System.Drawing.Point(2, 173);
            this.btnThemeSaveAs.Margin = new System.Windows.Forms.Padding(2);
            this.btnThemeSaveAs.Name = "btnThemeSaveAs";
            this.btnThemeSaveAs.Size = new System.Drawing.Size(165, 29);
            this.btnThemeSaveAs.TabIndex = 68;
            this.btnThemeSaveAs.Text = "Save As";
            this.btnThemeSaveAs.UseVisualStyleBackColor = true;
            this.btnThemeSaveAs.Click += new System.EventHandler(this.btnThemeSaveAs_Click);
            // 
            // txtThemeInfo
            // 
            this.txtThemeInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtThemeInfo.BackColor = System.Drawing.SystemColors.Window;
            this.txtThemeInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtThemeInfo.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtThemeInfo.Location = new System.Drawing.Point(2, 242);
            this.txtThemeInfo.Margin = new System.Windows.Forms.Padding(2);
            this.txtThemeInfo.Multiline = true;
            this.txtThemeInfo.Name = "txtThemeInfo";
            this.txtThemeInfo.ReadOnly = true;
            this.txtThemeInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtThemeInfo.Size = new System.Drawing.Size(165, 40);
            this.txtThemeInfo.TabIndex = 59;
            // 
            // btnThemeRefresh
            // 
            this.btnThemeRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThemeRefresh.AutoSize = true;
            this.btnThemeRefresh.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeRefresh.Location = new System.Drawing.Point(2, 80);
            this.btnThemeRefresh.Margin = new System.Windows.Forms.Padding(2);
            this.btnThemeRefresh.Name = "btnThemeRefresh";
            this.btnThemeRefresh.Size = new System.Drawing.Size(165, 29);
            this.btnThemeRefresh.TabIndex = 65;
            this.btnThemeRefresh.Text = "Refresh";
            this.btnThemeRefresh.UseVisualStyleBackColor = true;
            this.btnThemeRefresh.Click += new System.EventHandler(this.btnThemeRefresh_Click);
            // 
            // btnThemeInstall
            // 
            this.btnThemeInstall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThemeInstall.AutoSize = true;
            this.btnThemeInstall.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeInstall.Location = new System.Drawing.Point(2, 111);
            this.btnThemeInstall.Margin = new System.Windows.Forms.Padding(2);
            this.btnThemeInstall.Name = "btnThemeInstall";
            this.btnThemeInstall.Size = new System.Drawing.Size(165, 29);
            this.btnThemeInstall.TabIndex = 66;
            this.btnThemeInstall.Text = "Install";
            this.btnThemeInstall.UseVisualStyleBackColor = true;
            this.btnThemeInstall.Click += new System.EventHandler(this.btnThemeInstall_Click);
            // 
            // btnThemeUninstall
            // 
            this.btnThemeUninstall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThemeUninstall.AutoSize = true;
            this.btnThemeUninstall.Enabled = false;
            this.btnThemeUninstall.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeUninstall.Location = new System.Drawing.Point(2, 141);
            this.btnThemeUninstall.Margin = new System.Windows.Forms.Padding(2);
            this.btnThemeUninstall.Name = "btnThemeUninstall";
            this.btnThemeUninstall.Size = new System.Drawing.Size(165, 29);
            this.btnThemeUninstall.TabIndex = 67;
            this.btnThemeUninstall.Text = "Uninstall";
            this.btnThemeUninstall.UseVisualStyleBackColor = true;
            this.btnThemeUninstall.Click += new System.EventHandler(this.btnThemeUninstall_Click);
            // 
            // lvTheme
            // 
            this.lvTheme.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvTheme.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvTheme.FullRowSelect = true;
            this.lvTheme.LargeImageList = this.imglGeneral;
            this.lvTheme.Location = new System.Drawing.Point(18, 32);
            this.lvTheme.Margin = new System.Windows.Forms.Padding(2);
            this.lvTheme.MultiSelect = false;
            this.lvTheme.Name = "lvTheme";
            this.lvTheme.Size = new System.Drawing.Size(325, 285);
            this.lvTheme.SmallImageList = this.imglGeneral;
            this.lvTheme.StateImageList = this.imglGeneral;
            this.lvTheme.TabIndex = 64;
            this.lvTheme.UseCompatibleStateImageBehavior = false;
            this.lvTheme.View = System.Windows.Forms.View.List;
            this.lvTheme.SelectedIndexChanged += new System.EventHandler(this.lvTheme_SelectedIndexChanged);
            // 
            // imglGeneral
            // 
            this.imglGeneral.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imglGeneral.ImageSize = new System.Drawing.Size(10, 50);
            this.imglGeneral.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // btnThemeApply
            // 
            this.btnThemeApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThemeApply.AutoSize = true;
            this.btnThemeApply.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeApply.Location = new System.Drawing.Point(358, 325);
            this.btnThemeApply.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.btnThemeApply.Name = "btnThemeApply";
            this.btnThemeApply.Size = new System.Drawing.Size(167, 29);
            this.btnThemeApply.TabIndex = 70;
            this.btnThemeApply.Text = "Apply Theme";
            this.btnThemeApply.UseVisualStyleBackColor = true;
            this.btnThemeApply.Click += new System.EventHandler(this.btnThemeApply_Click);
            // 
            // lnkThemeDownload
            // 
            this.lnkThemeDownload.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkThemeDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkThemeDownload.AutoSize = true;
            this.lnkThemeDownload.BackColor = System.Drawing.Color.Transparent;
            this.lnkThemeDownload.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkThemeDownload.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkThemeDownload.Location = new System.Drawing.Point(15, 333);
            this.lnkThemeDownload.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lnkThemeDownload.Name = "lnkThemeDownload";
            this.lnkThemeDownload.Size = new System.Drawing.Size(111, 15);
            this.lnkThemeDownload.TabIndex = 71;
            this.lnkThemeDownload.TabStop = true;
            this.lnkThemeDownload.Text = "[Download themes]";
            this.lnkThemeDownload.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lnkThemeDownload.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkThemeDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkThemeDownload_LinkClicked);
            // 
            // btnThemeEdit
            // 
            this.btnThemeEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnThemeEdit.AutoSize = true;
            this.btnThemeEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeEdit.Location = new System.Drawing.Point(128, 325);
            this.btnThemeEdit.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.btnThemeEdit.Name = "btnThemeEdit";
            this.btnThemeEdit.Size = new System.Drawing.Size(167, 29);
            this.btnThemeEdit.TabIndex = 61;
            this.btnThemeEdit.Text = "Create New Theme";
            this.btnThemeEdit.UseVisualStyleBackColor = true;
            this.btnThemeEdit.Visible = false;
            // 
            // lblInstalledThemes
            // 
            this.lblInstalledThemes.AutoSize = true;
            this.lblInstalledThemes.Location = new System.Drawing.Point(15, 13);
            this.lblInstalledThemes.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblInstalledThemes.Name = "lblInstalledThemes";
            this.lblInstalledThemes.Size = new System.Drawing.Size(96, 15);
            this.lblInstalledThemes.TabIndex = 2;
            this.lblInstalledThemes.Text = "Installed themes:";
            // 
            // imglOpenWith
            // 
            this.imglOpenWith.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imglOpenWith.ImageSize = new System.Drawing.Size(16, 16);
            this.imglOpenWith.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // sp1
            // 
            this.sp1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sp1.Location = new System.Drawing.Point(1, 1);
            this.sp1.Margin = new System.Windows.Forms.Padding(1);
            this.sp1.Name = "sp1";
            // 
            // sp1.Panel1
            // 
            this.sp1.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.sp1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // sp1.Panel2
            // 
            this.sp1.Panel2.Controls.Add(this.tab1);
            this.sp1.Size = new System.Drawing.Size(710, 417);
            this.sp1.SplitterDistance = 168;
            this.sp1.SplitterWidth = 3;
            this.sp1.TabIndex = 17;
            this.sp1.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tableLayoutPanel1.Controls.Add(this.lblGeneral, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblImage, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblToolbar, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblColorPicker, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblEdit, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblFileAssociations, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblLanguage, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.lblTheme, 0, 8);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(168, 417);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // lblImage
            // 
            this.lblImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblImage.Location = new System.Drawing.Point(0, 33);
            this.lblImage.Margin = new System.Windows.Forms.Padding(0);
            this.lblImage.Name = "lblImage";
            this.lblImage.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblImage.Size = new System.Drawing.Size(169, 33);
            this.lblImage.TabIndex = 2;
            this.lblImage.Tag = "0";
            this.lblImage.Text = "Image";
            this.lblImage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblImage.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblImage.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblImage.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblImage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblToolbar
            // 
            this.lblToolbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblToolbar.Location = new System.Drawing.Point(0, 132);
            this.lblToolbar.Margin = new System.Windows.Forms.Padding(0);
            this.lblToolbar.Name = "lblToolbar";
            this.lblToolbar.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblToolbar.Size = new System.Drawing.Size(169, 33);
            this.lblToolbar.TabIndex = 5;
            this.lblToolbar.Tag = "0";
            this.lblToolbar.Text = "Toolbar";
            this.lblToolbar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblToolbar.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblToolbar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblToolbar.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblToolbar.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblToolbar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblColorPicker
            // 
            this.lblColorPicker.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblColorPicker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblColorPicker.Location = new System.Drawing.Point(0, 165);
            this.lblColorPicker.Margin = new System.Windows.Forms.Padding(0);
            this.lblColorPicker.Name = "lblColorPicker";
            this.lblColorPicker.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblColorPicker.Size = new System.Drawing.Size(169, 33);
            this.lblColorPicker.TabIndex = 6;
            this.lblColorPicker.Tag = "0";
            this.lblColorPicker.Text = "Color Picker";
            this.lblColorPicker.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblColorPicker.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblColorPicker.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblColorPicker.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblColorPicker.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblColorPicker.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblEdit
            // 
            this.lblEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblEdit.Location = new System.Drawing.Point(0, 66);
            this.lblEdit.Margin = new System.Windows.Forms.Padding(0);
            this.lblEdit.Name = "lblEdit";
            this.lblEdit.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblEdit.Size = new System.Drawing.Size(169, 33);
            this.lblEdit.TabIndex = 3;
            this.lblEdit.Tag = "0";
            this.lblEdit.Text = "[Edit]";
            this.lblEdit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblEdit.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblEdit.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblEdit.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblEdit.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblEdit.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblTheme
            // 
            this.lblTheme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTheme.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblTheme.Location = new System.Drawing.Point(0, 231);
            this.lblTheme.Margin = new System.Windows.Forms.Padding(0);
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblTheme.Size = new System.Drawing.Size(169, 33);
            this.lblTheme.TabIndex = 8;
            this.lblTheme.Tag = "0";
            this.lblTheme.Text = "Theme";
            this.lblTheme.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTheme.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblTheme.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblTheme.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblTheme.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblTheme.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.AutoSize = true;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSave.Location = new System.Drawing.Point(434, 9);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(83, 30);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.AutoSize = true;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(523, 9);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(83, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.AutoSize = true;
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnApply.Location = new System.Drawing.Point(611, 9);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(83, 30);
            this.btnApply.TabIndex = 2;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // tblayout
            // 
            this.tblayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.tblayout.ColumnCount = 1;
            this.tblayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblayout.Controls.Add(this.sp1, 0, 0);
            this.tblayout.Controls.Add(this.panel4, 0, 1);
            this.tblayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblayout.Location = new System.Drawing.Point(0, 0);
            this.tblayout.Margin = new System.Windows.Forms.Padding(0);
            this.tblayout.Name = "tblayout";
            this.tblayout.RowCount = 2;
            this.tblayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tblayout.Size = new System.Drawing.Size(712, 467);
            this.tblayout.TabIndex = 19;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panel4.Controls.Add(this.btnApply);
            this.panel4.Controls.Add(this.btnSave);
            this.panel4.Controls.Add(this.btnCancel);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 419);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(712, 48);
            this.panel4.TabIndex = 18;
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(712, 467);
            this.Controls.Add(this.tblayout);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(1);
            this.MinimumSize = new System.Drawing.Size(466, 356);
            this.Name = "frmSetting";
            this.RightToLeftLayout = true;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSetting_FormClosing);
            this.Load += new System.EventHandler(this.frmSetting_Load);
            this.SizeChanged += new System.EventHandler(this.frmSetting_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmSetting_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundColor)).EndInit();
            this.tabLanguage.ResumeLayout(false);
            this.tabLanguage.PerformLayout();
            this.tabFileAssociation.ResumeLayout(false);
            this.tabFileAssociation.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tab1.ResumeLayout(false);
            this.tabImage.ResumeLayout(false);
            this.tabImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barInterval)).EndInit();
            this.tabEdit.ResumeLayout(false);
            this.tabEdit.PerformLayout();
            this.tabToolbar.ResumeLayout(false);
            this.tabToolbar.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.tabColorPicker.ResumeLayout(false);
            this.tabColorPicker.PerformLayout();
            this.tabTheme.ResumeLayout(false);
            this.tabTheme.PerformLayout();
            this.panelThemeActions.ResumeLayout(false);
            this.panelThemeActions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.sp1.Panel1.ResumeLayout(false);
            this.sp1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sp1)).EndInit();
            this.sp1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tblayout.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.ImageList imglTheme;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.Label lblFileAssociations;
        private System.Windows.Forms.Label lblGeneral;
        private System.Windows.Forms.ToolTip tip1;
        private System.Windows.Forms.TabPage tabLanguage;
        private System.Windows.Forms.LinkLabel lnkRefresh;
        private System.Windows.Forms.LinkLabel lnkEdit;
        private System.Windows.Forms.LinkLabel lnkCreateNew;
        private System.Windows.Forms.LinkLabel lnkGetMoreLanguage;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.Label lblLanguageText;
        private System.Windows.Forms.TabPage tabFileAssociation;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.CheckBox chkShowToolBar;
        private System.Windows.Forms.PictureBox picBackgroundColor;
        private System.Windows.Forms.Label lblBackGroundColor;
        private System.Windows.Forms.CheckBox chkWelcomePicture;
        private System.Windows.Forms.CheckBox chkAutoUpdate;
        private System.Windows.Forms.TabControl tab1;
        private System.Windows.Forms.SplitContainer sp1;
        private System.Windows.Forms.CheckBox chkESCToQuit;
        private System.Windows.Forms.LinkLabel lnkInstallLanguage;
        private System.Windows.Forms.CheckBox chkAllowMultiInstances;
        private System.Windows.Forms.ImageList imglOpenWith;
        private System.Windows.Forms.Label lblSupportedExtension;
        private System.Windows.Forms.Label lblLanguageWarning;
        private System.Windows.Forms.CheckBox chkConfirmationDelete;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView lvExtension;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.LinkLabel lnkOpenFileAssoc;
        private System.Windows.Forms.Button btnResetExt;
        private System.Windows.Forms.Button btnDeleteExt;
        private System.Windows.Forms.Button btnAddNewExt;
        private System.Windows.Forms.Label lblExtensionsGroupDescription;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnRegisterExt;
        private System.Windows.Forms.Label lblImage;
        private System.Windows.Forms.TabPage tabImage;
        private System.Windows.Forms.Label lblHeadImageLoading;
        private System.Windows.Forms.CheckBox chkLoopViewer;
        //private System.Windows.Forms.CheckBox chkMouseNavigation;
        private System.Windows.Forms.Label lblGeneral_ZoomOptimization;
        private System.Windows.Forms.ComboBox cmbZoomOptimization;
        private System.Windows.Forms.CheckBox chkThumbnailVertical;
        private System.Windows.Forms.Label lblGeneral_ThumbnailSize;
        private System.Windows.Forms.ComboBox cmbThumbnailDimension;
        private System.Windows.Forms.CheckBox chkImageBoosterBack;
        private System.Windows.Forms.CheckBox chkLoopSlideshow;
        private System.Windows.Forms.Label lblImageLoadingOrder;
        private System.Windows.Forms.ComboBox cmbImageOrder;
        private System.Windows.Forms.TrackBar barInterval;
        private System.Windows.Forms.Label lblSlideshowInterval;
        private System.Windows.Forms.CheckBox chkFindChildFolder;
        private System.Windows.Forms.Label lblHeadZooming;
        private System.Windows.Forms.Label lblHeadSlideshow;
        private System.Windows.Forms.Label lblHeadThumbnailBar;
        private System.Windows.Forms.Label lblHeadStartup;
        private System.Windows.Forms.Label lblHeadConfigDir;
        private System.Windows.Forms.Label lblHeadOthers;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.LinkLabel lnkResetBackgroundColor;
        private System.Windows.Forms.CheckBox chkShowScrollbar;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TableLayoutPanel tblayout;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox cmbMouseWheelAlt;
        private System.Windows.Forms.ComboBox cmbMouseWheelShift;
        private System.Windows.Forms.ComboBox cmbMouseWheelCtrl;
        private System.Windows.Forms.ComboBox cmbMouseWheel;
        private System.Windows.Forms.Label lblMouseWheelAlt;
        private System.Windows.Forms.Label lblMouseWheelShift;
        private System.Windows.Forms.Label lblMouseWheelCtrl;
        private System.Windows.Forms.Label lblMouseWheel;
        private System.Windows.Forms.Label lblHeadMouseWheelActions;
        private System.Windows.Forms.CheckBox chkShowHiddenImages;
        private System.Windows.Forms.TabPage tabToolbar;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnMoveLeft;
        private System.Windows.Forms.Button btnMoveRight;
        private System.Windows.Forms.ListView lvAvailButtons;
        private System.Windows.Forms.ListView lvUsedButtons;
        private System.Windows.Forms.Label lblUsedBtns;
        private System.Windows.Forms.Label lblAvailBtns;
        private System.Windows.Forms.Label lblToolbar;
        private System.Windows.Forms.Label lblColorPicker;
        private System.Windows.Forms.TabPage tabColorPicker;
        private System.Windows.Forms.Label lblColorCodeFormat;
        private System.Windows.Forms.CheckBox chkColorUseHEXA;
        private System.Windows.Forms.CheckBox chkColorUseRGBA;
        private System.Windows.Forms.CheckBox chkColorUseHSLA;
        private System.Windows.Forms.Label lblTheme;
        private System.Windows.Forms.TabPage tabTheme;
        private System.Windows.Forms.Label lblInstalledThemes;
        private System.Windows.Forms.ListView lvTheme;
        private System.Windows.Forms.LinkLabel lnkThemeDownload;
        private System.Windows.Forms.Button btnThemeRefresh;
        private System.Windows.Forms.Button btnThemeUninstall;
        private System.Windows.Forms.Button btnThemeEdit;
        private System.Windows.Forms.Button btnThemeInstall;
        private System.Windows.Forms.Panel panelThemeActions;
        private System.Windows.Forms.Button btnThemeSaveAs;
        private System.Windows.Forms.TextBox txtThemeInfo;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.ImageList imglGeneral;
        private System.Windows.Forms.Button btnThemeApply;
        private System.Windows.Forms.Button btnThemeFolderOpen;
        private System.Windows.Forms.CheckBox chkDisplayBasename;
        private System.Windows.Forms.Label lblToolbarPosition;
        private System.Windows.Forms.ComboBox cmbToolbarPosition;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.CheckBox chkShowThumbnailScrollbar;
        private System.Windows.Forms.LinkLabel lnkConfigDir;
        private System.Windows.Forms.CheckBox chkLastSeenImage;
        private System.Windows.Forms.CheckBox chkHorzCenterToolbarBtns;
        private System.Windows.Forms.TabPage tabEdit;
        private System.Windows.Forms.Label lblEdit;
        private System.Windows.Forms.CheckBox chkApplyColorProfile;
        private System.Windows.Forms.ComboBox cmbColorProfile;
        private System.Windows.Forms.Label lblColorProfile;
        private System.Windows.Forms.LinkLabel lnkColorProfileBrowse;
        private System.Windows.Forms.Label lblSelectAppForEdit;
        private System.Windows.Forms.CheckBox chkSaveOnRotate;
        private System.Windows.Forms.CheckBox chkSaveModifyDate;
        private System.Windows.Forms.Button btnEditEditAllExt;
        private System.Windows.Forms.Button btnEditResetExt;
        private System.Windows.Forms.Button btnEditEditExt;
        private System.Windows.Forms.ListView lvImageEditing;
        private System.Windows.Forms.ColumnHeader clnFileExtension;
        private System.Windows.Forms.ColumnHeader clnAppName;
        private System.Windows.Forms.ColumnHeader clnAppPath;
        private System.Windows.Forms.ColumnHeader clnAppArguments;
        private System.Windows.Forms.Label lblColorManagement;
        private System.Windows.Forms.LinkLabel lnkColorProfilePath;
        private System.Windows.Forms.CheckBox chkShowNavButtons;
        private System.Windows.Forms.CheckBox chkShowCheckerboardOnlyImage;
        private System.Windows.Forms.CheckBox chkDisplayLoadMessage;
    }
}