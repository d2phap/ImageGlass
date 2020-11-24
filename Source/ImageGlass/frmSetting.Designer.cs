
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(".123");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(".abc");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(".def");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSetting));
            this.imglTheme = new System.Windows.Forms.ImageList(this.components);
            this.lblLanguage = new System.Windows.Forms.Label();
            this.lblFileTypeAssoc = new System.Windows.Forms.Label();
            this.lblGeneral = new System.Windows.Forms.Label();
            this.tip1 = new System.Windows.Forms.ToolTip(this.components);
            this.picBackgroundColor = new System.Windows.Forms.PictureBox();
            this.imglGeneral = new System.Windows.Forms.ImageList(this.components);
            this.imglOpenWith = new System.Windows.Forms.ImageList(this.components);
            this.sp1 = new System.Windows.Forms.SplitContainer();
            this.tableTabHeaders = new System.Windows.Forms.TableLayoutPanel();
            this.lblImage = new System.Windows.Forms.Label();
            this.lblToolbar = new System.Windows.Forms.Label();
            this.lblTools = new System.Windows.Forms.Label();
            this.lblEdit = new System.Windows.Forms.Label();
            this.lblKeyboard = new System.Windows.Forms.Label();
            this.lblTheme = new System.Windows.Forms.Label();
            this.tab1 = new ImageGlass.UI.NakedTabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.chkUseTouchGesture = new System.Windows.Forms.CheckBox();
            this.lblHeadViewer = new System.Windows.Forms.Label();
            this.chkShowToast = new System.Windows.Forms.CheckBox();
            this.chkCenterWindowFit = new System.Windows.Forms.CheckBox();
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
            this.tabImage = new System.Windows.Forms.TabPage();
            this.chkGroupByDirectory = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.numSlideShowInterval = new System.Windows.Forms.NumericUpDown();
            this.numSlideshowIntervalTo = new System.Windows.Forms.NumericUpDown();
            this.lblSlideshowIntervalTo = new System.Windows.Forms.Label();
            this.chkRandomSlideshowInterval = new System.Windows.Forms.CheckBox();
            this.chkShowSlideshowCountdown = new System.Windows.Forms.CheckBox();
            this.chkIsCenterImage = new System.Windows.Forms.CheckBox();
            this.lblImageBoosterCachedCount = new System.Windows.Forms.Label();
            this.cmbImageBoosterCachedCount = new System.Windows.Forms.ComboBox();
            this.txtZoomLevels = new System.Windows.Forms.TextBox();
            this.lblZoomLevels = new System.Windows.Forms.Label();
            this.cmbImageOrderType = new System.Windows.Forms.ComboBox();
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
            this.chkUseFileExplorerSortOrder = new System.Windows.Forms.CheckBox();
            this.chkLoopSlideshow = new System.Windows.Forms.CheckBox();
            this.lblImageLoadingOrder = new System.Windows.Forms.Label();
            this.cmbImageOrder = new System.Windows.Forms.ComboBox();
            this.lblSlideshowInterval = new System.Windows.Forms.Label();
            this.chkFindChildFolder = new System.Windows.Forms.CheckBox();
            this.tabEdit = new System.Windows.Forms.TabPage();
            this.tableEdit = new System.Windows.Forms.TableLayoutPanel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.cmbAfterEditingApp = new System.Windows.Forms.ComboBox();
            this.lblAfterEditingApp = new System.Windows.Forms.Label();
            this.numImageQuality = new System.Windows.Forms.NumericUpDown();
            this.lblImageQuality = new System.Windows.Forms.Label();
            this.chkSaveOnRotate = new System.Windows.Forms.CheckBox();
            this.lblSelectAppForEdit = new System.Windows.Forms.Label();
            this.lvImageEditing = new System.Windows.Forms.ListView();
            this.clnFileExtension = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnAppName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnAppPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnAppArguments = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chkSaveModifyDate = new System.Windows.Forms.CheckBox();
            this.panel7 = new System.Windows.Forms.Panel();
            this.btnEditEditExt = new System.Windows.Forms.Button();
            this.btnEditResetExt = new System.Windows.Forms.Button();
            this.btnEditEditAllExt = new System.Windows.Forms.Button();
            this.tabFileTypeAssoc = new System.Windows.Forms.TabPage();
            this.tableFileAssoc = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnUnregisterExt = new System.Windows.Forms.Button();
            this.btnRegisterExt = new System.Windows.Forms.Button();
            this.btnResetExt = new System.Windows.Forms.Button();
            this.btnAddNewExt = new System.Windows.Forms.Button();
            this.btnDeleteExt = new System.Windows.Forms.Button();
            this.panel8 = new System.Windows.Forms.Panel();
            this.lblSupportedExtension = new System.Windows.Forms.Label();
            this.lvExtension = new System.Windows.Forms.ListView();
            this.clnExt = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lnkOpenFileAssoc = new System.Windows.Forms.LinkLabel();
            this.tabToolbar = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel9 = new System.Windows.Forms.Panel();
            this.lblToolbarIconHeight = new System.Windows.Forms.Label();
            this.numToolbarIconHeight = new System.Windows.Forms.NumericUpDown();
            this.chkHideTooltips = new System.Windows.Forms.CheckBox();
            this.lblToolbarPosition = new System.Windows.Forms.Label();
            this.chkHorzCenterToolbarBtns = new System.Windows.Forms.CheckBox();
            this.cmbToolbarPosition = new System.Windows.Forms.ComboBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.lblAvailBtns = new System.Windows.Forms.Label();
            this.btnMoveRight = new System.Windows.Forms.Button();
            this.btnMoveLeft = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.lvAvailButtons = new System.Windows.Forms.ListView();
            this.lblUsedBtns = new System.Windows.Forms.Label();
            this.lvUsedButtons = new System.Windows.Forms.ListView();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.tabTools = new System.Windows.Forms.TabPage();
            this.chkColorUseHSVA = new System.Windows.Forms.CheckBox();
            this.lnkSelectExifTool = new System.Windows.Forms.LinkLabel();
            this.lblExifToolPath = new System.Windows.Forms.Label();
            this.lblExifTool = new System.Windows.Forms.Label();
            this.chkExifToolAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.lblPageNav = new System.Windows.Forms.Label();
            this.chkShowPageNavAuto = new System.Windows.Forms.CheckBox();
            this.chkColorUseHSLA = new System.Windows.Forms.CheckBox();
            this.lblColorPicker = new System.Windows.Forms.Label();
            this.chkColorUseHEXA = new System.Windows.Forms.CheckBox();
            this.chkColorUseRGBA = new System.Windows.Forms.CheckBox();
            this.tabKeyboard = new System.Windows.Forms.TabPage();
            this.btnKeyReset = new System.Windows.Forms.Button();
            this.cmbKeysSpaceBack = new System.Windows.Forms.ComboBox();
            this.cmbKeysPgUpDown = new System.Windows.Forms.ComboBox();
            this.cmbKeysUpDown = new System.Windows.Forms.ComboBox();
            this.cmbKeysLeftRight = new System.Windows.Forms.ComboBox();
            this.lblKeysSpaceBack = new System.Windows.Forms.Label();
            this.lblKeysPageUpDown = new System.Windows.Forms.Label();
            this.lblKeysUpDown = new System.Windows.Forms.Label();
            this.lblKeysLeftRight = new System.Windows.Forms.Label();
            this.tabTheme = new System.Windows.Forms.TabPage();
            this.tableTheme = new System.Windows.Forms.TableLayoutPanel();
            this.lblInstalledThemes = new System.Windows.Forms.Label();
            this.panelThemeActions = new System.Windows.Forms.Panel();
            this.tb3 = new System.Windows.Forms.TableLayoutPanel();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.txtThemeInfo = new System.Windows.Forms.TextBox();
            this.btnThemeFolderOpen = new System.Windows.Forms.Button();
            this.btnThemeRefresh = new System.Windows.Forms.Button();
            this.btnThemeSaveAs = new System.Windows.Forms.Button();
            this.btnThemeInstall = new System.Windows.Forms.Button();
            this.btnThemeUninstall = new System.Windows.Forms.Button();
            this.lnkThemeDownload = new System.Windows.Forms.LinkLabel();
            this.lvTheme = new System.Windows.Forms.ListView();
            this.btnThemeApply = new System.Windows.Forms.Button();
            this.tabLanguage = new System.Windows.Forms.TabPage();
            this.lblLanguageWarning = new System.Windows.Forms.Label();
            this.lnkInstallLanguage = new System.Windows.Forms.LinkLabel();
            this.lnkRefresh = new System.Windows.Forms.LinkLabel();
            this.lnkEdit = new System.Windows.Forms.LinkLabel();
            this.lnkCreateNew = new System.Windows.Forms.LinkLabel();
            this.lnkGetMoreLanguage = new System.Windows.Forms.LinkLabel();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.lblLanguageText = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.tblayout = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sp1)).BeginInit();
            this.sp1.Panel1.SuspendLayout();
            this.sp1.Panel2.SuspendLayout();
            this.sp1.SuspendLayout();
            this.tableTabHeaders.SuspendLayout();
            this.tab1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabImage.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSlideShowInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSlideshowIntervalTo)).BeginInit();
            this.tabEdit.SuspendLayout();
            this.tableEdit.SuspendLayout();
            this.panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numImageQuality)).BeginInit();
            this.panel7.SuspendLayout();
            this.tabFileTypeAssoc.SuspendLayout();
            this.tableFileAssoc.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel8.SuspendLayout();
            this.tabToolbar.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numToolbarIconHeight)).BeginInit();
            this.panel5.SuspendLayout();
            this.tabTools.SuspendLayout();
            this.tabKeyboard.SuspendLayout();
            this.tabTheme.SuspendLayout();
            this.tableTheme.SuspendLayout();
            this.panelThemeActions.SuspendLayout();
            this.tb3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.tabLanguage.SuspendLayout();
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
            this.lblLanguage.Location = new System.Drawing.Point(0, 376);
            this.lblLanguage.Margin = new System.Windows.Forms.Padding(0);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Padding = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.lblLanguage.Size = new System.Drawing.Size(236, 47);
            this.lblLanguage.TabIndex = 9;
            this.lblLanguage.Tag = "0";
            this.lblLanguage.Text = "Language";
            this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblLanguage.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblLanguage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblLanguage.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblLanguage.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblLanguage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblFileTypeAssoc
            // 
            this.lblFileTypeAssoc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFileTypeAssoc.Location = new System.Drawing.Point(0, 141);
            this.lblFileTypeAssoc.Margin = new System.Windows.Forms.Padding(0);
            this.lblFileTypeAssoc.Name = "lblFileTypeAssoc";
            this.lblFileTypeAssoc.Padding = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.lblFileTypeAssoc.Size = new System.Drawing.Size(236, 47);
            this.lblFileTypeAssoc.TabIndex = 4;
            this.lblFileTypeAssoc.Tag = "0";
            this.lblFileTypeAssoc.Text = "[File Type Associations]";
            this.lblFileTypeAssoc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFileTypeAssoc.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblFileTypeAssoc.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblFileTypeAssoc.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblFileTypeAssoc.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblFileTypeAssoc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblGeneral
            // 
            this.lblGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGeneral.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.lblGeneral.Location = new System.Drawing.Point(0, 0);
            this.lblGeneral.Margin = new System.Windows.Forms.Padding(0);
            this.lblGeneral.Name = "lblGeneral";
            this.lblGeneral.Padding = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.lblGeneral.Size = new System.Drawing.Size(236, 47);
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
            this.picBackgroundColor.Location = new System.Drawing.Point(42, 524);
            this.picBackgroundColor.Margin = new System.Windows.Forms.Padding(1);
            this.picBackgroundColor.Name = "picBackgroundColor";
            this.picBackgroundColor.Size = new System.Drawing.Size(93, 37);
            this.picBackgroundColor.TabIndex = 12;
            this.picBackgroundColor.TabStop = false;
            this.tip1.SetToolTip(this.picBackgroundColor, "Change background color");
            this.picBackgroundColor.Click += new System.EventHandler(this.picBackgroundColor_Click);
            // 
            // imglGeneral
            // 
            this.imglGeneral.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imglGeneral.ImageSize = new System.Drawing.Size(10, 50);
            this.imglGeneral.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // imglOpenWith
            // 
            this.imglOpenWith.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imglOpenWith.ImageSize = new System.Drawing.Size(16, 16);
            this.imglOpenWith.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // sp1
            // 
            this.sp1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.sp1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sp1.Location = new System.Drawing.Point(0, 0);
            this.sp1.Margin = new System.Windows.Forms.Padding(0);
            this.sp1.Name = "sp1";
            // 
            // sp1.Panel1
            // 
            this.sp1.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(165)))), ((int)(((byte)(168)))));
            this.sp1.Panel1.Controls.Add(this.tableTabHeaders);
            // 
            // sp1.Panel2
            // 
            this.sp1.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.sp1.Panel2.Controls.Add(this.tab1);
            this.sp1.Size = new System.Drawing.Size(973, 668);
            this.sp1.SplitterDistance = 225;
            this.sp1.TabIndex = 17;
            this.sp1.TabStop = false;
            // 
            // tableTabHeaders
            // 
            this.tableTabHeaders.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(165)))), ((int)(((byte)(168)))));
            this.tableTabHeaders.ColumnCount = 1;
            this.tableTabHeaders.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 236F));
            this.tableTabHeaders.Controls.Add(this.lblGeneral, 0, 0);
            this.tableTabHeaders.Controls.Add(this.lblImage, 0, 1);
            this.tableTabHeaders.Controls.Add(this.lblToolbar, 0, 4);
            this.tableTabHeaders.Controls.Add(this.lblTools, 0, 5);
            this.tableTabHeaders.Controls.Add(this.lblEdit, 0, 2);
            this.tableTabHeaders.Controls.Add(this.lblFileTypeAssoc, 0, 3);
            this.tableTabHeaders.Controls.Add(this.lblKeyboard, 0, 7);
            this.tableTabHeaders.Controls.Add(this.lblTheme, 0, 8);
            this.tableTabHeaders.Controls.Add(this.lblLanguage, 0, 9);
            this.tableTabHeaders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableTabHeaders.Location = new System.Drawing.Point(0, 0);
            this.tableTabHeaders.Margin = new System.Windows.Forms.Padding(0);
            this.tableTabHeaders.Name = "tableTabHeaders";
            this.tableTabHeaders.RowCount = 9;
            this.tableTabHeaders.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTabHeaders.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTabHeaders.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTabHeaders.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTabHeaders.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTabHeaders.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTabHeaders.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTabHeaders.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTabHeaders.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTabHeaders.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableTabHeaders.Size = new System.Drawing.Size(225, 668);
            this.tableTabHeaders.TabIndex = 5;
            // 
            // lblImage
            // 
            this.lblImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblImage.Location = new System.Drawing.Point(0, 47);
            this.lblImage.Margin = new System.Windows.Forms.Padding(0);
            this.lblImage.Name = "lblImage";
            this.lblImage.Padding = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.lblImage.Size = new System.Drawing.Size(236, 47);
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
            this.lblToolbar.Location = new System.Drawing.Point(0, 188);
            this.lblToolbar.Margin = new System.Windows.Forms.Padding(0);
            this.lblToolbar.Name = "lblToolbar";
            this.lblToolbar.Padding = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.lblToolbar.Size = new System.Drawing.Size(236, 47);
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
            // lblTools
            // 
            this.lblTools.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTools.Location = new System.Drawing.Point(0, 235);
            this.lblTools.Margin = new System.Windows.Forms.Padding(0);
            this.lblTools.Name = "lblTools";
            this.lblTools.Padding = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.lblTools.Size = new System.Drawing.Size(236, 47);
            this.lblTools.TabIndex = 6;
            this.lblTools.Tag = "0";
            this.lblTools.Text = "[Tools]";
            this.lblTools.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTools.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblTools.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblTools.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblTools.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblTools.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblEdit
            // 
            this.lblEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEdit.Location = new System.Drawing.Point(0, 94);
            this.lblEdit.Margin = new System.Windows.Forms.Padding(0);
            this.lblEdit.Name = "lblEdit";
            this.lblEdit.Padding = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.lblEdit.Size = new System.Drawing.Size(236, 47);
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
            // lblKeyboard
            // 
            this.lblKeyboard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKeyboard.Location = new System.Drawing.Point(0, 282);
            this.lblKeyboard.Margin = new System.Windows.Forms.Padding(0);
            this.lblKeyboard.Name = "lblKeyboard";
            this.lblKeyboard.Padding = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.lblKeyboard.Size = new System.Drawing.Size(236, 47);
            this.lblKeyboard.TabIndex = 7;
            this.lblKeyboard.Tag = "0";
            this.lblKeyboard.Text = "[Keyboard]";
            this.lblKeyboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblKeyboard.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblKeyboard.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblKeyboard.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblKeyboard.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblKeyboard.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblTheme
            // 
            this.lblTheme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTheme.Location = new System.Drawing.Point(0, 329);
            this.lblTheme.Margin = new System.Windows.Forms.Padding(0);
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Padding = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.lblTheme.Size = new System.Drawing.Size(236, 47);
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
            // tab1
            // 
            this.tab1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tab1.Controls.Add(this.tabGeneral);
            this.tab1.Controls.Add(this.tabImage);
            this.tab1.Controls.Add(this.tabEdit);
            this.tab1.Controls.Add(this.tabFileTypeAssoc);
            this.tab1.Controls.Add(this.tabToolbar);
            this.tab1.Controls.Add(this.tabTools);
            this.tab1.Controls.Add(this.tabKeyboard);
            this.tab1.Controls.Add(this.tabTheme);
            this.tab1.Controls.Add(this.tabLanguage);
            this.tab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tab1.Location = new System.Drawing.Point(0, 0);
            this.tab1.Margin = new System.Windows.Forms.Padding(0);
            this.tab1.Multiline = true;
            this.tab1.Name = "tab1";
            this.tab1.Padding = new System.Drawing.Point(0, 0);
            this.tab1.SelectedIndex = 0;
            this.tab1.Size = new System.Drawing.Size(744, 668);
            this.tab1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tab1.TabIndex = 0;
            this.tab1.SelectedIndexChanged += new System.EventHandler(this.tab1_SelectedIndexChanged);
            // 
            // tabGeneral
            // 
            this.tabGeneral.AutoScroll = true;
            this.tabGeneral.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tabGeneral.Controls.Add(this.chkUseTouchGesture);
            this.tabGeneral.Controls.Add(this.lblHeadViewer);
            this.tabGeneral.Controls.Add(this.chkShowToast);
            this.tabGeneral.Controls.Add(this.chkCenterWindowFit);
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
            this.tabGeneral.Location = new System.Drawing.Point(4, 69);
            this.tabGeneral.Margin = new System.Windows.Forms.Padding(0);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Size = new System.Drawing.Size(736, 595);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "general";
            // 
            // chkUseTouchGesture
            // 
            this.chkUseTouchGesture.AutoSize = true;
            this.chkUseTouchGesture.BackColor = System.Drawing.Color.Transparent;
            this.chkUseTouchGesture.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkUseTouchGesture.Location = new System.Drawing.Point(42, 453);
            this.chkUseTouchGesture.Margin = new System.Windows.Forms.Padding(1);
            this.chkUseTouchGesture.Name = "chkUseTouchGesture";
            this.chkUseTouchGesture.Size = new System.Drawing.Size(281, 28);
            this.chkUseTouchGesture.TabIndex = 11;
            this.chkUseTouchGesture.Text = "[Enable touch gesture support]";
            this.chkUseTouchGesture.UseVisualStyleBackColor = false;
            // 
            // lblHeadViewer
            // 
            this.lblHeadViewer.AutoSize = true;
            this.lblHeadViewer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadViewer.Location = new System.Drawing.Point(22, 283);
            this.lblHeadViewer.Name = "lblHeadViewer";
            this.lblHeadViewer.Size = new System.Drawing.Size(77, 23);
            this.lblHeadViewer.TabIndex = 47;
            this.lblHeadViewer.Text = "[Viewer]";
            // 
            // chkShowToast
            // 
            this.chkShowToast.AutoSize = true;
            this.chkShowToast.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowToast.Location = new System.Drawing.Point(42, 806);
            this.chkShowToast.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowToast.Name = "chkShowToast";
            this.chkShowToast.Size = new System.Drawing.Size(210, 28);
            this.chkShowToast.TabIndex = 18;
            this.chkShowToast.Text = "[Show toast message]";
            this.chkShowToast.UseVisualStyleBackColor = true;
            // 
            // chkCenterWindowFit
            // 
            this.chkCenterWindowFit.AutoSize = true;
            this.chkCenterWindowFit.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkCenterWindowFit.Location = new System.Drawing.Point(42, 772);
            this.chkCenterWindowFit.Margin = new System.Windows.Forms.Padding(1);
            this.chkCenterWindowFit.Name = "chkCenterWindowFit";
            this.chkCenterWindowFit.Size = new System.Drawing.Size(396, 28);
            this.chkCenterWindowFit.TabIndex = 17;
            this.chkCenterWindowFit.Text = "[Auto center the window in Window Fit mode]";
            this.chkCenterWindowFit.UseVisualStyleBackColor = true;
            // 
            // chkShowCheckerboardOnlyImage
            // 
            this.chkShowCheckerboardOnlyImage.AutoSize = true;
            this.chkShowCheckerboardOnlyImage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowCheckerboardOnlyImage.Location = new System.Drawing.Point(42, 419);
            this.chkShowCheckerboardOnlyImage.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowCheckerboardOnlyImage.Name = "chkShowCheckerboardOnlyImage";
            this.chkShowCheckerboardOnlyImage.Size = new System.Drawing.Size(409, 28);
            this.chkShowCheckerboardOnlyImage.TabIndex = 10;
            this.chkShowCheckerboardOnlyImage.Text = "[Display checkerboard only in the image region]";
            this.chkShowCheckerboardOnlyImage.UseVisualStyleBackColor = true;
            // 
            // chkShowNavButtons
            // 
            this.chkShowNavButtons.AutoSize = true;
            this.chkShowNavButtons.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowNavButtons.Location = new System.Drawing.Point(42, 351);
            this.chkShowNavButtons.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowNavButtons.Name = "chkShowNavButtons";
            this.chkShowNavButtons.Size = new System.Drawing.Size(306, 28);
            this.chkShowNavButtons.TabIndex = 8;
            this.chkShowNavButtons.Text = "[Display navigation arrow buttons]";
            this.chkShowNavButtons.UseVisualStyleBackColor = true;
            // 
            // chkLastSeenImage
            // 
            this.chkLastSeenImage.AutoSize = true;
            this.chkLastSeenImage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkLastSeenImage.Location = new System.Drawing.Point(42, 81);
            this.chkLastSeenImage.Margin = new System.Windows.Forms.Padding(1);
            this.chkLastSeenImage.Name = "chkLastSeenImage";
            this.chkLastSeenImage.Size = new System.Drawing.Size(250, 28);
            this.chkLastSeenImage.TabIndex = 4;
            this.chkLastSeenImage.Text = "[Open the last seen image]";
            this.chkLastSeenImage.UseVisualStyleBackColor = true;
            // 
            // lnkConfigDir
            // 
            this.lnkConfigDir.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkConfigDir.AutoSize = true;
            this.lnkConfigDir.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkConfigDir.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkConfigDir.Location = new System.Drawing.Point(42, 214);
            this.lnkConfigDir.Name = "lnkConfigDir";
            this.lnkConfigDir.Size = new System.Drawing.Size(108, 23);
            this.lnkConfigDir.TabIndex = 6;
            this.lnkConfigDir.TabStop = true;
            this.lnkConfigDir.Text = "[C:\\ABC\\XYZ]";
            this.lnkConfigDir.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkConfigDir.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkConfigDir_LinkClicked);
            // 
            // chkDisplayBasename
            // 
            this.chkDisplayBasename.AutoSize = true;
            this.chkDisplayBasename.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkDisplayBasename.Location = new System.Drawing.Point(42, 385);
            this.chkDisplayBasename.Margin = new System.Windows.Forms.Padding(1);
            this.chkDisplayBasename.Name = "chkDisplayBasename";
            this.chkDisplayBasename.Size = new System.Drawing.Size(450, 28);
            this.chkDisplayBasename.TabIndex = 9;
            this.chkDisplayBasename.Text = "[Display base name of the viewing image on title bar]";
            this.chkDisplayBasename.UseVisualStyleBackColor = true;
            // 
            // chkShowScrollbar
            // 
            this.chkShowScrollbar.AutoSize = true;
            this.chkShowScrollbar.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowScrollbar.Location = new System.Drawing.Point(42, 317);
            this.chkShowScrollbar.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowScrollbar.Name = "chkShowScrollbar";
            this.chkShowScrollbar.Size = new System.Drawing.Size(228, 28);
            this.chkShowScrollbar.TabIndex = 7;
            this.chkShowScrollbar.Text = "Display viewer scrollbars";
            this.chkShowScrollbar.UseVisualStyleBackColor = true;
            // 
            // lnkResetBackgroundColor
            // 
            this.lnkResetBackgroundColor.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkResetBackgroundColor.AutoSize = true;
            this.lnkResetBackgroundColor.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkResetBackgroundColor.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkResetBackgroundColor.Location = new System.Drawing.Point(140, 533);
            this.lnkResetBackgroundColor.Name = "lnkResetBackgroundColor";
            this.lnkResetBackgroundColor.Size = new System.Drawing.Size(51, 23);
            this.lnkResetBackgroundColor.TabIndex = 12;
            this.lnkResetBackgroundColor.TabStop = true;
            this.lnkResetBackgroundColor.Text = "Reset";
            this.lnkResetBackgroundColor.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkResetBackgroundColor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkResetBackgroundColor_LinkClicked);
            // 
            // lblHeadOthers
            // 
            this.lblHeadOthers.AutoSize = true;
            this.lblHeadOthers.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadOthers.Location = new System.Drawing.Point(22, 604);
            this.lblHeadOthers.Name = "lblHeadOthers";
            this.lblHeadOthers.Size = new System.Drawing.Size(63, 23);
            this.lblHeadOthers.TabIndex = 46;
            this.lblHeadOthers.Text = "Others";
            // 
            // lblHeadConfigDir
            // 
            this.lblHeadConfigDir.AutoSize = true;
            this.lblHeadConfigDir.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadConfigDir.Location = new System.Drawing.Point(22, 184);
            this.lblHeadConfigDir.Name = "lblHeadConfigDir";
            this.lblHeadConfigDir.Size = new System.Drawing.Size(213, 23);
            this.lblHeadConfigDir.TabIndex = 45;
            this.lblHeadConfigDir.Text = "[Configuration directory]";
            // 
            // lblHeadStartup
            // 
            this.lblHeadStartup.AutoSize = true;
            this.lblHeadStartup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadStartup.Location = new System.Drawing.Point(22, 20);
            this.lblHeadStartup.Name = "lblHeadStartup";
            this.lblHeadStartup.Size = new System.Drawing.Size(76, 23);
            this.lblHeadStartup.TabIndex = 44;
            this.lblHeadStartup.Text = "Start up";
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(362, 845);
            this.panel1.Margin = new System.Windows.Forms.Padding(1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(102, 29);
            this.panel1.TabIndex = 24;
            // 
            // chkConfirmationDelete
            // 
            this.chkConfirmationDelete.AutoSize = true;
            this.chkConfirmationDelete.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkConfirmationDelete.Location = new System.Drawing.Point(42, 738);
            this.chkConfirmationDelete.Margin = new System.Windows.Forms.Padding(1);
            this.chkConfirmationDelete.Name = "chkConfirmationDelete";
            this.chkConfirmationDelete.Size = new System.Drawing.Size(312, 28);
            this.chkConfirmationDelete.TabIndex = 16;
            this.chkConfirmationDelete.Text = "Display Delete confirmation dialog ";
            this.chkConfirmationDelete.UseVisualStyleBackColor = true;
            // 
            // chkAllowMultiInstances
            // 
            this.chkAllowMultiInstances.AutoSize = true;
            this.chkAllowMultiInstances.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkAllowMultiInstances.Location = new System.Drawing.Point(42, 670);
            this.chkAllowMultiInstances.Margin = new System.Windows.Forms.Padding(1);
            this.chkAllowMultiInstances.Name = "chkAllowMultiInstances";
            this.chkAllowMultiInstances.Size = new System.Drawing.Size(349, 28);
            this.chkAllowMultiInstances.TabIndex = 14;
            this.chkAllowMultiInstances.Text = "Allow multiple instances of the program";
            this.chkAllowMultiInstances.UseVisualStyleBackColor = true;
            // 
            // chkESCToQuit
            // 
            this.chkESCToQuit.AutoSize = true;
            this.chkESCToQuit.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkESCToQuit.Location = new System.Drawing.Point(42, 704);
            this.chkESCToQuit.Margin = new System.Windows.Forms.Padding(1);
            this.chkESCToQuit.Name = "chkESCToQuit";
            this.chkESCToQuit.Size = new System.Drawing.Size(330, 28);
            this.chkESCToQuit.TabIndex = 15;
            this.chkESCToQuit.Text = "Allow to press ESC to quit application";
            this.chkESCToQuit.UseVisualStyleBackColor = true;
            // 
            // chkShowToolBar
            // 
            this.chkShowToolBar.AutoSize = true;
            this.chkShowToolBar.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowToolBar.Location = new System.Drawing.Point(42, 115);
            this.chkShowToolBar.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowToolBar.Name = "chkShowToolBar";
            this.chkShowToolBar.Size = new System.Drawing.Size(280, 28);
            this.chkShowToolBar.TabIndex = 5;
            this.chkShowToolBar.Text = "Show toolbar when starting up";
            this.chkShowToolBar.UseVisualStyleBackColor = true;
            // 
            // lblBackGroundColor
            // 
            this.lblBackGroundColor.AutoSize = true;
            this.lblBackGroundColor.Location = new System.Drawing.Point(38, 501);
            this.lblBackGroundColor.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblBackGroundColor.Name = "lblBackGroundColor";
            this.lblBackGroundColor.Size = new System.Drawing.Size(148, 23);
            this.lblBackGroundColor.TabIndex = 11;
            this.lblBackGroundColor.Text = "Background color:";
            // 
            // chkWelcomePicture
            // 
            this.chkWelcomePicture.AutoSize = true;
            this.chkWelcomePicture.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkWelcomePicture.Location = new System.Drawing.Point(42, 47);
            this.chkWelcomePicture.Margin = new System.Windows.Forms.Padding(1);
            this.chkWelcomePicture.Name = "chkWelcomePicture";
            this.chkWelcomePicture.Size = new System.Drawing.Size(216, 28);
            this.chkWelcomePicture.TabIndex = 3;
            this.chkWelcomePicture.Text = "Show welcome picture";
            this.chkWelcomePicture.UseVisualStyleBackColor = true;
            // 
            // chkAutoUpdate
            // 
            this.chkAutoUpdate.AutoSize = true;
            this.chkAutoUpdate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkAutoUpdate.Location = new System.Drawing.Point(42, 636);
            this.chkAutoUpdate.Margin = new System.Windows.Forms.Padding(1);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(283, 28);
            this.chkAutoUpdate.TabIndex = 13;
            this.chkAutoUpdate.Text = "Check for update automatically";
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // tabImage
            // 
            this.tabImage.AutoScroll = true;
            this.tabImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tabImage.Controls.Add(this.chkGroupByDirectory);
            this.tabImage.Controls.Add(this.tableLayoutPanel2);
            this.tabImage.Controls.Add(this.chkRandomSlideshowInterval);
            this.tabImage.Controls.Add(this.chkShowSlideshowCountdown);
            this.tabImage.Controls.Add(this.chkIsCenterImage);
            this.tabImage.Controls.Add(this.lblImageBoosterCachedCount);
            this.tabImage.Controls.Add(this.cmbImageBoosterCachedCount);
            this.tabImage.Controls.Add(this.txtZoomLevels);
            this.tabImage.Controls.Add(this.lblZoomLevels);
            this.tabImage.Controls.Add(this.cmbImageOrderType);
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
            this.tabImage.Controls.Add(this.chkUseFileExplorerSortOrder);
            this.tabImage.Controls.Add(this.chkLoopSlideshow);
            this.tabImage.Controls.Add(this.lblImageLoadingOrder);
            this.tabImage.Controls.Add(this.cmbImageOrder);
            this.tabImage.Controls.Add(this.lblSlideshowInterval);
            this.tabImage.Controls.Add(this.chkFindChildFolder);
            this.tabImage.Location = new System.Drawing.Point(4, 69);
            this.tabImage.Margin = new System.Windows.Forms.Padding(0);
            this.tabImage.Name = "tabImage";
            this.tabImage.Size = new System.Drawing.Size(736, 595);
            this.tabImage.TabIndex = 3;
            this.tabImage.Text = "Image";
            // 
            // chkGroupByDirectory
            // 
            this.chkGroupByDirectory.AutoSize = true;
            this.chkGroupByDirectory.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkGroupByDirectory.Location = new System.Drawing.Point(42, 81);
            this.chkGroupByDirectory.Margin = new System.Windows.Forms.Padding(1);
            this.chkGroupByDirectory.Name = "chkGroupByDirectory";
            this.chkGroupByDirectory.Size = new System.Drawing.Size(257, 28);
            this.chkGroupByDirectory.TabIndex = 17;
            this.chkGroupByDirectory.Text = "[Group images by directory]";
            this.chkGroupByDirectory.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.numSlideShowInterval, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.numSlideshowIntervalTo, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblSlideshowIntervalTo, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(42, 1683);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(314, 45);
            this.tableLayoutPanel2.TabIndex = 68;
            // 
            // numSlideShowInterval
            // 
            this.numSlideShowInterval.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.numSlideShowInterval.Location = new System.Drawing.Point(3, 3);
            this.numSlideShowInterval.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.numSlideShowInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSlideShowInterval.Name = "numSlideShowInterval";
            this.numSlideShowInterval.Size = new System.Drawing.Size(107, 30);
            this.numSlideShowInterval.TabIndex = 41;
            this.numSlideShowInterval.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSlideShowInterval.ValueChanged += new System.EventHandler(this.numSlideShowInterval_ValueChanged);
            // 
            // numSlideshowIntervalTo
            // 
            this.numSlideshowIntervalTo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.numSlideshowIntervalTo.Location = new System.Drawing.Point(154, 3);
            this.numSlideshowIntervalTo.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.numSlideshowIntervalTo.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSlideshowIntervalTo.Name = "numSlideshowIntervalTo";
            this.numSlideshowIntervalTo.Size = new System.Drawing.Size(107, 30);
            this.numSlideshowIntervalTo.TabIndex = 42;
            this.numSlideshowIntervalTo.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSlideshowIntervalTo.Visible = false;
            this.numSlideshowIntervalTo.ValueChanged += new System.EventHandler(this.numSlideshowIntervalTo_ValueChanged);
            // 
            // lblSlideshowIntervalTo
            // 
            this.lblSlideshowIntervalTo.AutoSize = true;
            this.lblSlideshowIntervalTo.Location = new System.Drawing.Point(114, 4);
            this.lblSlideshowIntervalTo.Margin = new System.Windows.Forms.Padding(1, 4, 1, 0);
            this.lblSlideshowIntervalTo.Name = "lblSlideshowIntervalTo";
            this.lblSlideshowIntervalTo.Size = new System.Drawing.Size(36, 23);
            this.lblSlideshowIntervalTo.TabIndex = 69;
            this.lblSlideshowIntervalTo.Text = "[to]";
            this.lblSlideshowIntervalTo.Visible = false;
            // 
            // chkRandomSlideshowInterval
            // 
            this.chkRandomSlideshowInterval.AutoSize = true;
            this.chkRandomSlideshowInterval.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkRandomSlideshowInterval.Location = new System.Drawing.Point(42, 1603);
            this.chkRandomSlideshowInterval.Margin = new System.Windows.Forms.Padding(1);
            this.chkRandomSlideshowInterval.Name = "chkRandomSlideshowInterval";
            this.chkRandomSlideshowInterval.Size = new System.Drawing.Size(209, 28);
            this.chkRandomSlideshowInterval.TabIndex = 40;
            this.chkRandomSlideshowInterval.Text = "[Use random interval]";
            this.chkRandomSlideshowInterval.UseVisualStyleBackColor = true;
            this.chkRandomSlideshowInterval.CheckedChanged += new System.EventHandler(this.chkRandomSlideshowInterval_CheckedChanged);
            // 
            // chkShowSlideshowCountdown
            // 
            this.chkShowSlideshowCountdown.AutoSize = true;
            this.chkShowSlideshowCountdown.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowSlideshowCountdown.Location = new System.Drawing.Point(42, 1569);
            this.chkShowSlideshowCountdown.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowSlideshowCountdown.Name = "chkShowSlideshowCountdown";
            this.chkShowSlideshowCountdown.Size = new System.Drawing.Size(232, 28);
            this.chkShowSlideshowCountdown.TabIndex = 39;
            this.chkShowSlideshowCountdown.Text = "[Show countdown timer]";
            this.chkShowSlideshowCountdown.UseVisualStyleBackColor = true;
            // 
            // chkIsCenterImage
            // 
            this.chkIsCenterImage.AutoSize = true;
            this.chkIsCenterImage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkIsCenterImage.Location = new System.Drawing.Point(42, 183);
            this.chkIsCenterImage.Margin = new System.Windows.Forms.Padding(1);
            this.chkIsCenterImage.Name = "chkIsCenterImage";
            this.chkIsCenterImage.Size = new System.Drawing.Size(236, 28);
            this.chkIsCenterImage.TabIndex = 20;
            this.chkIsCenterImage.Text = "[Center image on viewer]";
            this.chkIsCenterImage.UseVisualStyleBackColor = true;
            // 
            // lblImageBoosterCachedCount
            // 
            this.lblImageBoosterCachedCount.AutoSize = true;
            this.lblImageBoosterCachedCount.Location = new System.Drawing.Point(38, 348);
            this.lblImageBoosterCachedCount.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblImageBoosterCachedCount.Name = "lblImageBoosterCachedCount";
            this.lblImageBoosterCachedCount.Size = new System.Drawing.Size(471, 23);
            this.lblImageBoosterCachedCount.TabIndex = 64;
            this.lblImageBoosterCachedCount.Text = "[Number of images cached by ImageBooster (one direction)]";
            // 
            // cmbImageBoosterCachedCount
            // 
            this.cmbImageBoosterCachedCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbImageBoosterCachedCount.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbImageBoosterCachedCount.FormattingEnabled = true;
            this.cmbImageBoosterCachedCount.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmbImageBoosterCachedCount.Location = new System.Drawing.Point(42, 374);
            this.cmbImageBoosterCachedCount.Margin = new System.Windows.Forms.Padding(1);
            this.cmbImageBoosterCachedCount.Name = "cmbImageBoosterCachedCount";
            this.cmbImageBoosterCachedCount.Size = new System.Drawing.Size(259, 31);
            this.cmbImageBoosterCachedCount.TabIndex = 24;
            // 
            // txtZoomLevels
            // 
            this.txtZoomLevels.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.txtZoomLevels.Location = new System.Drawing.Point(42, 1172);
            this.txtZoomLevels.Multiline = true;
            this.txtZoomLevels.Name = "txtZoomLevels";
            this.txtZoomLevels.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtZoomLevels.Size = new System.Drawing.Size(529, 81);
            this.txtZoomLevels.TabIndex = 34;
            // 
            // lblZoomLevels
            // 
            this.lblZoomLevels.AutoSize = true;
            this.lblZoomLevels.BackColor = System.Drawing.Color.Transparent;
            this.lblZoomLevels.Location = new System.Drawing.Point(38, 1145);
            this.lblZoomLevels.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblZoomLevels.Name = "lblZoomLevels";
            this.lblZoomLevels.Size = new System.Drawing.Size(111, 23);
            this.lblZoomLevels.TabIndex = 61;
            this.lblZoomLevels.Text = "[Zoom levels]";
            // 
            // cmbImageOrderType
            // 
            this.cmbImageOrderType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbImageOrderType.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbImageOrderType.FormattingEnabled = true;
            this.cmbImageOrderType.Items.AddRange(new object[] {
            "[Ascending]",
            "[Descending]"});
            this.cmbImageOrderType.Location = new System.Drawing.Point(310, 257);
            this.cmbImageOrderType.Margin = new System.Windows.Forms.Padding(1);
            this.cmbImageOrderType.Name = "cmbImageOrderType";
            this.cmbImageOrderType.Size = new System.Drawing.Size(259, 31);
            this.cmbImageOrderType.TabIndex = 22;
            // 
            // lnkColorProfilePath
            // 
            this.lnkColorProfilePath.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkColorProfilePath.AutoSize = true;
            this.lnkColorProfilePath.BackColor = System.Drawing.Color.Transparent;
            this.lnkColorProfilePath.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkColorProfilePath.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkColorProfilePath.Location = new System.Drawing.Point(38, 596);
            this.lnkColorProfilePath.Name = "lnkColorProfilePath";
            this.lnkColorProfilePath.Size = new System.Drawing.Size(144, 23);
            this.lnkColorProfilePath.TabIndex = 28;
            this.lnkColorProfilePath.TabStop = true;
            this.lnkColorProfilePath.Text = "C:\\abc\\custom.icc";
            this.lnkColorProfilePath.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkColorProfilePath.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkColorProfilePath_LinkClicked);
            // 
            // lnkColorProfileBrowse
            // 
            this.lnkColorProfileBrowse.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkColorProfileBrowse.AutoSize = true;
            this.lnkColorProfileBrowse.BackColor = System.Drawing.Color.Transparent;
            this.lnkColorProfileBrowse.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkColorProfileBrowse.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkColorProfileBrowse.Location = new System.Drawing.Point(306, 566);
            this.lnkColorProfileBrowse.Name = "lnkColorProfileBrowse";
            this.lnkColorProfileBrowse.Size = new System.Drawing.Size(74, 23);
            this.lnkColorProfileBrowse.TabIndex = 27;
            this.lnkColorProfileBrowse.TabStop = true;
            this.lnkColorProfileBrowse.Text = "[Browse]";
            this.lnkColorProfileBrowse.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkColorProfileBrowse.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkColorProfileBrowse_LinkClicked);
            // 
            // lblColorManagement
            // 
            this.lblColorManagement.AutoSize = true;
            this.lblColorManagement.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblColorManagement.Location = new System.Drawing.Point(22, 448);
            this.lblColorManagement.Name = "lblColorManagement";
            this.lblColorManagement.Size = new System.Drawing.Size(176, 23);
            this.lblColorManagement.TabIndex = 59;
            this.lblColorManagement.Text = "[Color management]";
            // 
            // chkApplyColorProfile
            // 
            this.chkApplyColorProfile.AutoSize = true;
            this.chkApplyColorProfile.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkApplyColorProfile.Location = new System.Drawing.Point(42, 480);
            this.chkApplyColorProfile.Margin = new System.Windows.Forms.Padding(1);
            this.chkApplyColorProfile.Name = "chkApplyColorProfile";
            this.chkApplyColorProfile.Size = new System.Drawing.Size(464, 28);
            this.chkApplyColorProfile.TabIndex = 25;
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
            this.cmbColorProfile.Location = new System.Drawing.Point(42, 561);
            this.cmbColorProfile.Margin = new System.Windows.Forms.Padding(1);
            this.cmbColorProfile.Name = "cmbColorProfile";
            this.cmbColorProfile.Size = new System.Drawing.Size(259, 31);
            this.cmbColorProfile.TabIndex = 26;
            this.cmbColorProfile.SelectedIndexChanged += new System.EventHandler(this.cmbColorProfile_SelectedIndexChanged);
            // 
            // lblColorProfile
            // 
            this.lblColorProfile.AutoSize = true;
            this.lblColorProfile.BackColor = System.Drawing.Color.Transparent;
            this.lblColorProfile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblColorProfile.Location = new System.Drawing.Point(38, 531);
            this.lblColorProfile.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.lblColorProfile.Name = "lblColorProfile";
            this.lblColorProfile.Size = new System.Drawing.Size(118, 23);
            this.lblColorProfile.TabIndex = 56;
            this.lblColorProfile.Text = "[Color profile:]";
            // 
            // chkShowThumbnailScrollbar
            // 
            this.chkShowThumbnailScrollbar.AutoSize = true;
            this.chkShowThumbnailScrollbar.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowThumbnailScrollbar.Location = new System.Drawing.Point(42, 1348);
            this.chkShowThumbnailScrollbar.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowThumbnailScrollbar.Name = "chkShowThumbnailScrollbar";
            this.chkShowThumbnailScrollbar.Size = new System.Drawing.Size(278, 28);
            this.chkShowThumbnailScrollbar.TabIndex = 36;
            this.chkShowThumbnailScrollbar.Text = "[Show thumbnail bar scrollbar]";
            this.chkShowThumbnailScrollbar.UseVisualStyleBackColor = true;
            // 
            // cmbMouseWheelAlt
            // 
            this.cmbMouseWheelAlt.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMouseWheelAlt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbMouseWheelAlt.FormattingEnabled = true;
            this.cmbMouseWheelAlt.Location = new System.Drawing.Point(42, 958);
            this.cmbMouseWheelAlt.Margin = new System.Windows.Forms.Padding(4);
            this.cmbMouseWheelAlt.Name = "cmbMouseWheelAlt";
            this.cmbMouseWheelAlt.Size = new System.Drawing.Size(259, 31);
            this.cmbMouseWheelAlt.TabIndex = 32;
            // 
            // cmbMouseWheelShift
            // 
            this.cmbMouseWheelShift.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMouseWheelShift.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbMouseWheelShift.FormattingEnabled = true;
            this.cmbMouseWheelShift.Location = new System.Drawing.Point(42, 878);
            this.cmbMouseWheelShift.Margin = new System.Windows.Forms.Padding(4);
            this.cmbMouseWheelShift.Name = "cmbMouseWheelShift";
            this.cmbMouseWheelShift.Size = new System.Drawing.Size(259, 31);
            this.cmbMouseWheelShift.TabIndex = 31;
            // 
            // cmbMouseWheelCtrl
            // 
            this.cmbMouseWheelCtrl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMouseWheelCtrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbMouseWheelCtrl.FormattingEnabled = true;
            this.cmbMouseWheelCtrl.Location = new System.Drawing.Point(42, 796);
            this.cmbMouseWheelCtrl.Margin = new System.Windows.Forms.Padding(4);
            this.cmbMouseWheelCtrl.Name = "cmbMouseWheelCtrl";
            this.cmbMouseWheelCtrl.Size = new System.Drawing.Size(259, 31);
            this.cmbMouseWheelCtrl.TabIndex = 30;
            // 
            // cmbMouseWheel
            // 
            this.cmbMouseWheel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMouseWheel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbMouseWheel.FormattingEnabled = true;
            this.cmbMouseWheel.Location = new System.Drawing.Point(42, 716);
            this.cmbMouseWheel.Margin = new System.Windows.Forms.Padding(4);
            this.cmbMouseWheel.Name = "cmbMouseWheel";
            this.cmbMouseWheel.Size = new System.Drawing.Size(259, 31);
            this.cmbMouseWheel.TabIndex = 29;
            // 
            // lblMouseWheelAlt
            // 
            this.lblMouseWheelAlt.AutoSize = true;
            this.lblMouseWheelAlt.Location = new System.Drawing.Point(38, 931);
            this.lblMouseWheelAlt.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMouseWheelAlt.Name = "lblMouseWheelAlt";
            this.lblMouseWheelAlt.Size = new System.Drawing.Size(153, 23);
            this.lblMouseWheelAlt.TabIndex = 52;
            this.lblMouseWheelAlt.Text = "Mouse wheel + Alt";
            // 
            // lblMouseWheelShift
            // 
            this.lblMouseWheelShift.AutoSize = true;
            this.lblMouseWheelShift.Location = new System.Drawing.Point(38, 850);
            this.lblMouseWheelShift.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMouseWheelShift.Name = "lblMouseWheelShift";
            this.lblMouseWheelShift.Size = new System.Drawing.Size(166, 23);
            this.lblMouseWheelShift.TabIndex = 51;
            this.lblMouseWheelShift.Text = "Mouse wheel + Shift";
            // 
            // lblMouseWheelCtrl
            // 
            this.lblMouseWheelCtrl.AutoSize = true;
            this.lblMouseWheelCtrl.Location = new System.Drawing.Point(38, 768);
            this.lblMouseWheelCtrl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMouseWheelCtrl.Name = "lblMouseWheelCtrl";
            this.lblMouseWheelCtrl.Size = new System.Drawing.Size(159, 23);
            this.lblMouseWheelCtrl.TabIndex = 50;
            this.lblMouseWheelCtrl.Text = "Mouse wheel + Ctrl";
            // 
            // lblMouseWheel
            // 
            this.lblMouseWheel.AutoSize = true;
            this.lblMouseWheel.Location = new System.Drawing.Point(38, 690);
            this.lblMouseWheel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMouseWheel.Name = "lblMouseWheel";
            this.lblMouseWheel.Size = new System.Drawing.Size(110, 23);
            this.lblMouseWheel.TabIndex = 49;
            this.lblMouseWheel.Text = "Mouse wheel";
            // 
            // lblHeadMouseWheelActions
            // 
            this.lblHeadMouseWheelActions.AutoSize = true;
            this.lblHeadMouseWheelActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblHeadMouseWheelActions.Location = new System.Drawing.Point(22, 656);
            this.lblHeadMouseWheelActions.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHeadMouseWheelActions.Name = "lblHeadMouseWheelActions";
            this.lblHeadMouseWheelActions.Size = new System.Drawing.Size(175, 23);
            this.lblHeadMouseWheelActions.TabIndex = 48;
            this.lblHeadMouseWheelActions.Text = "Mouse wheel actions";
            // 
            // chkShowHiddenImages
            // 
            this.chkShowHiddenImages.AutoSize = true;
            this.chkShowHiddenImages.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowHiddenImages.Location = new System.Drawing.Point(42, 115);
            this.chkShowHiddenImages.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowHiddenImages.Name = "chkShowHiddenImages";
            this.chkShowHiddenImages.Size = new System.Drawing.Size(203, 28);
            this.chkShowHiddenImages.TabIndex = 18;
            this.chkShowHiddenImages.Text = "Show hidden images";
            this.chkShowHiddenImages.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Location = new System.Drawing.Point(25, 1753);
            this.panel3.Margin = new System.Windows.Forms.Padding(1);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(102, 29);
            this.panel3.TabIndex = 46;
            // 
            // lblHeadZooming
            // 
            this.lblHeadZooming.AutoSize = true;
            this.lblHeadZooming.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadZooming.Location = new System.Drawing.Point(22, 1041);
            this.lblHeadZooming.Name = "lblHeadZooming";
            this.lblHeadZooming.Size = new System.Drawing.Size(82, 23);
            this.lblHeadZooming.TabIndex = 43;
            this.lblHeadZooming.Text = "Zooming";
            // 
            // lblHeadSlideshow
            // 
            this.lblHeadSlideshow.AutoSize = true;
            this.lblHeadSlideshow.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadSlideshow.Location = new System.Drawing.Point(22, 1504);
            this.lblHeadSlideshow.Name = "lblHeadSlideshow";
            this.lblHeadSlideshow.Size = new System.Drawing.Size(91, 23);
            this.lblHeadSlideshow.TabIndex = 42;
            this.lblHeadSlideshow.Text = "Slideshow";
            // 
            // lblHeadThumbnailBar
            // 
            this.lblHeadThumbnailBar.AutoSize = true;
            this.lblHeadThumbnailBar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadThumbnailBar.Location = new System.Drawing.Point(22, 1284);
            this.lblHeadThumbnailBar.Name = "lblHeadThumbnailBar";
            this.lblHeadThumbnailBar.Size = new System.Drawing.Size(128, 23);
            this.lblHeadThumbnailBar.TabIndex = 41;
            this.lblHeadThumbnailBar.Text = "Thumbnail bar";
            // 
            // lblHeadImageLoading
            // 
            this.lblHeadImageLoading.AutoSize = true;
            this.lblHeadImageLoading.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadImageLoading.Location = new System.Drawing.Point(22, 15);
            this.lblHeadImageLoading.Name = "lblHeadImageLoading";
            this.lblHeadImageLoading.Size = new System.Drawing.Size(126, 23);
            this.lblHeadImageLoading.TabIndex = 40;
            this.lblHeadImageLoading.Text = "Image loading";
            // 
            // chkLoopViewer
            // 
            this.chkLoopViewer.AutoSize = true;
            this.chkLoopViewer.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkLoopViewer.Location = new System.Drawing.Point(42, 149);
            this.chkLoopViewer.Margin = new System.Windows.Forms.Padding(1);
            this.chkLoopViewer.Name = "chkLoopViewer";
            this.chkLoopViewer.Size = new System.Drawing.Size(569, 28);
            this.chkLoopViewer.TabIndex = 19;
            this.chkLoopViewer.Text = "Loop back viewer to the first image when reaching the end of the list";
            this.chkLoopViewer.UseVisualStyleBackColor = true;
            // 
            // lblGeneral_ZoomOptimization
            // 
            this.lblGeneral_ZoomOptimization.AutoSize = true;
            this.lblGeneral_ZoomOptimization.BackColor = System.Drawing.Color.Transparent;
            this.lblGeneral_ZoomOptimization.Location = new System.Drawing.Point(38, 1075);
            this.lblGeneral_ZoomOptimization.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblGeneral_ZoomOptimization.Name = "lblGeneral_ZoomOptimization";
            this.lblGeneral_ZoomOptimization.Size = new System.Drawing.Size(156, 23);
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
            this.cmbZoomOptimization.Location = new System.Drawing.Point(42, 1102);
            this.cmbZoomOptimization.Margin = new System.Windows.Forms.Padding(1);
            this.cmbZoomOptimization.Name = "cmbZoomOptimization";
            this.cmbZoomOptimization.Size = new System.Drawing.Size(259, 31);
            this.cmbZoomOptimization.TabIndex = 33;
            // 
            // chkThumbnailVertical
            // 
            this.chkThumbnailVertical.AutoSize = true;
            this.chkThumbnailVertical.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkThumbnailVertical.Location = new System.Drawing.Point(42, 1314);
            this.chkThumbnailVertical.Margin = new System.Windows.Forms.Padding(1);
            this.chkThumbnailVertical.Name = "chkThumbnailVertical";
            this.chkThumbnailVertical.Size = new System.Drawing.Size(257, 28);
            this.chkThumbnailVertical.TabIndex = 35;
            this.chkThumbnailVertical.Text = "Thumbnail bar on right side";
            this.chkThumbnailVertical.UseVisualStyleBackColor = true;
            // 
            // lblGeneral_ThumbnailSize
            // 
            this.lblGeneral_ThumbnailSize.AutoSize = true;
            this.lblGeneral_ThumbnailSize.Location = new System.Drawing.Point(38, 1401);
            this.lblGeneral_ThumbnailSize.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblGeneral_ThumbnailSize.Name = "lblGeneral_ThumbnailSize";
            this.lblGeneral_ThumbnailSize.Size = new System.Drawing.Size(248, 23);
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
            this.cmbThumbnailDimension.Location = new System.Drawing.Point(42, 1427);
            this.cmbThumbnailDimension.Margin = new System.Windows.Forms.Padding(1);
            this.cmbThumbnailDimension.Name = "cmbThumbnailDimension";
            this.cmbThumbnailDimension.Size = new System.Drawing.Size(259, 31);
            this.cmbThumbnailDimension.TabIndex = 37;
            // 
            // chkUseFileExplorerSortOrder
            // 
            this.chkUseFileExplorerSortOrder.AutoSize = true;
            this.chkUseFileExplorerSortOrder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkUseFileExplorerSortOrder.Location = new System.Drawing.Point(42, 296);
            this.chkUseFileExplorerSortOrder.Margin = new System.Windows.Forms.Padding(1);
            this.chkUseFileExplorerSortOrder.Name = "chkUseFileExplorerSortOrder";
            this.chkUseFileExplorerSortOrder.Size = new System.Drawing.Size(414, 28);
            this.chkUseFileExplorerSortOrder.TabIndex = 23;
            this.chkUseFileExplorerSortOrder.Text = "[Use Windows File Explorer sort order if possible]";
            this.chkUseFileExplorerSortOrder.UseVisualStyleBackColor = true;
            // 
            // chkLoopSlideshow
            // 
            this.chkLoopSlideshow.AutoSize = true;
            this.chkLoopSlideshow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkLoopSlideshow.Location = new System.Drawing.Point(42, 1535);
            this.chkLoopSlideshow.Margin = new System.Windows.Forms.Padding(1);
            this.chkLoopSlideshow.Name = "chkLoopSlideshow";
            this.chkLoopSlideshow.Size = new System.Drawing.Size(604, 28);
            this.chkLoopSlideshow.TabIndex = 38;
            this.chkLoopSlideshow.Text = "[Loop back slideshow to the first image when reaching the end of the list]";
            this.chkLoopSlideshow.UseVisualStyleBackColor = true;
            // 
            // lblImageLoadingOrder
            // 
            this.lblImageLoadingOrder.AutoSize = true;
            this.lblImageLoadingOrder.Location = new System.Drawing.Point(38, 230);
            this.lblImageLoadingOrder.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblImageLoadingOrder.Name = "lblImageLoadingOrder";
            this.lblImageLoadingOrder.Size = new System.Drawing.Size(170, 23);
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
            this.cmbImageOrder.Location = new System.Drawing.Point(42, 257);
            this.cmbImageOrder.Margin = new System.Windows.Forms.Padding(1);
            this.cmbImageOrder.Name = "cmbImageOrder";
            this.cmbImageOrder.Size = new System.Drawing.Size(259, 31);
            this.cmbImageOrder.TabIndex = 21;
            // 
            // lblSlideshowInterval
            // 
            this.lblSlideshowInterval.AutoSize = true;
            this.lblSlideshowInterval.Location = new System.Drawing.Point(38, 1658);
            this.lblSlideshowInterval.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblSlideshowInterval.Name = "lblSlideshowInterval";
            this.lblSlideshowInterval.Size = new System.Drawing.Size(205, 23);
            this.lblSlideshowInterval.TabIndex = 24;
            this.lblSlideshowInterval.Text = "[Slideshow interval: 00:03]";
            // 
            // chkFindChildFolder
            // 
            this.chkFindChildFolder.AutoSize = true;
            this.chkFindChildFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkFindChildFolder.Location = new System.Drawing.Point(42, 47);
            this.chkFindChildFolder.Margin = new System.Windows.Forms.Padding(1);
            this.chkFindChildFolder.Name = "chkFindChildFolder";
            this.chkFindChildFolder.Size = new System.Drawing.Size(245, 28);
            this.chkFindChildFolder.TabIndex = 16;
            this.chkFindChildFolder.Text = "Find images in child folder";
            this.chkFindChildFolder.UseVisualStyleBackColor = true;
            // 
            // tabEdit
            // 
            this.tabEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tabEdit.Controls.Add(this.tableEdit);
            this.tabEdit.Location = new System.Drawing.Point(4, 69);
            this.tabEdit.Name = "tabEdit";
            this.tabEdit.Padding = new System.Windows.Forms.Padding(3);
            this.tabEdit.Size = new System.Drawing.Size(736, 595);
            this.tabEdit.TabIndex = 7;
            this.tabEdit.Text = "Edit";
            // 
            // tableEdit
            // 
            this.tableEdit.ColumnCount = 1;
            this.tableEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableEdit.Controls.Add(this.panel6, 0, 0);
            this.tableEdit.Controls.Add(this.panel7, 0, 1);
            this.tableEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableEdit.Location = new System.Drawing.Point(3, 3);
            this.tableEdit.Margin = new System.Windows.Forms.Padding(0);
            this.tableEdit.Name = "tableEdit";
            this.tableEdit.RowCount = 2;
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableEdit.Size = new System.Drawing.Size(730, 589);
            this.tableEdit.TabIndex = 62;
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.Transparent;
            this.panel6.Controls.Add(this.cmbAfterEditingApp);
            this.panel6.Controls.Add(this.lblAfterEditingApp);
            this.panel6.Controls.Add(this.numImageQuality);
            this.panel6.Controls.Add(this.lblImageQuality);
            this.panel6.Controls.Add(this.chkSaveOnRotate);
            this.panel6.Controls.Add(this.lblSelectAppForEdit);
            this.panel6.Controls.Add(this.lvImageEditing);
            this.panel6.Controls.Add(this.chkSaveModifyDate);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Margin = new System.Windows.Forms.Padding(0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(730, 519);
            this.panel6.TabIndex = 60;
            // 
            // cmbAfterEditingApp
            // 
            this.cmbAfterEditingApp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAfterEditingApp.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbAfterEditingApp.FormattingEnabled = true;
            this.cmbAfterEditingApp.Items.AddRange(new object[] {
            "AAA",
            "BBB",
            "CCC"});
            this.cmbAfterEditingApp.Location = new System.Drawing.Point(22, 127);
            this.cmbAfterEditingApp.Margin = new System.Windows.Forms.Padding(1);
            this.cmbAfterEditingApp.Name = "cmbAfterEditingApp";
            this.cmbAfterEditingApp.Size = new System.Drawing.Size(259, 31);
            this.cmbAfterEditingApp.TabIndex = 7;
            // 
            // lblAfterEditingApp
            // 
            this.lblAfterEditingApp.AutoSize = true;
            this.lblAfterEditingApp.Location = new System.Drawing.Point(18, 99);
            this.lblAfterEditingApp.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblAfterEditingApp.Name = "lblAfterEditingApp";
            this.lblAfterEditingApp.Size = new System.Drawing.Size(221, 23);
            this.lblAfterEditingApp.TabIndex = 63;
            this.lblAfterEditingApp.Text = "[After opening editing app:]";
            // 
            // numImageQuality
            // 
            this.numImageQuality.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.numImageQuality.Location = new System.Drawing.Point(371, 127);
            this.numImageQuality.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numImageQuality.Name = "numImageQuality";
            this.numImageQuality.Size = new System.Drawing.Size(107, 30);
            this.numImageQuality.TabIndex = 8;
            this.numImageQuality.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblImageQuality
            // 
            this.lblImageQuality.AutoSize = true;
            this.lblImageQuality.Location = new System.Drawing.Point(367, 99);
            this.lblImageQuality.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblImageQuality.Name = "lblImageQuality";
            this.lblImageQuality.Size = new System.Drawing.Size(128, 23);
            this.lblImageQuality.TabIndex = 61;
            this.lblImageQuality.Text = "[Image quality:]";
            // 
            // chkSaveOnRotate
            // 
            this.chkSaveOnRotate.AutoSize = true;
            this.chkSaveOnRotate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkSaveOnRotate.Location = new System.Drawing.Point(22, 20);
            this.chkSaveOnRotate.Margin = new System.Windows.Forms.Padding(1);
            this.chkSaveOnRotate.Name = "chkSaveOnRotate";
            this.chkSaveOnRotate.Size = new System.Drawing.Size(330, 28);
            this.chkSaveOnRotate.TabIndex = 5;
            this.chkSaveOnRotate.Text = "Save the viewing image after rotating";
            this.chkSaveOnRotate.UseVisualStyleBackColor = true;
            // 
            // lblSelectAppForEdit
            // 
            this.lblSelectAppForEdit.AutoSize = true;
            this.lblSelectAppForEdit.Location = new System.Drawing.Point(18, 180);
            this.lblSelectAppForEdit.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblSelectAppForEdit.Name = "lblSelectAppForEdit";
            this.lblSelectAppForEdit.Size = new System.Drawing.Size(284, 23);
            this.lblSelectAppForEdit.TabIndex = 59;
            this.lblSelectAppForEdit.Text = "Select application for image editing:";
            // 
            // lvImageEditing
            // 
            this.lvImageEditing.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvImageEditing.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.lvImageEditing.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clnFileExtension,
            this.clnAppName,
            this.clnAppPath,
            this.clnAppArguments});
            this.lvImageEditing.FullRowSelect = true;
            this.lvImageEditing.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvImageEditing.HideSelection = false;
            listViewItem1.StateImageIndex = 0;
            listViewItem2.StateImageIndex = 0;
            listViewItem3.StateImageIndex = 0;
            this.lvImageEditing.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.lvImageEditing.Location = new System.Drawing.Point(22, 206);
            this.lvImageEditing.MultiSelect = false;
            this.lvImageEditing.Name = "lvImageEditing";
            this.lvImageEditing.RightToLeftLayout = true;
            this.lvImageEditing.ShowItemToolTips = true;
            this.lvImageEditing.Size = new System.Drawing.Size(689, 297);
            this.lvImageEditing.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvImageEditing.TabIndex = 9;
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
            // chkSaveModifyDate
            // 
            this.chkSaveModifyDate.AutoSize = true;
            this.chkSaveModifyDate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkSaveModifyDate.Location = new System.Drawing.Point(22, 54);
            this.chkSaveModifyDate.Margin = new System.Windows.Forms.Padding(1);
            this.chkSaveModifyDate.Name = "chkSaveModifyDate";
            this.chkSaveModifyDate.Size = new System.Drawing.Size(341, 28);
            this.chkSaveModifyDate.TabIndex = 6;
            this.chkSaveModifyDate.Text = "Preserve the modification date on save";
            this.chkSaveModifyDate.UseVisualStyleBackColor = true;
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.Color.Transparent;
            this.panel7.Controls.Add(this.btnEditEditExt);
            this.panel7.Controls.Add(this.btnEditResetExt);
            this.panel7.Controls.Add(this.btnEditEditAllExt);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel7.Location = new System.Drawing.Point(0, 519);
            this.panel7.Margin = new System.Windows.Forms.Padding(0);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(730, 70);
            this.panel7.TabIndex = 61;
            // 
            // btnEditEditExt
            // 
            this.btnEditEditExt.AutoSize = true;
            this.btnEditEditExt.Enabled = false;
            this.btnEditEditExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnEditEditExt.Location = new System.Drawing.Point(22, 3);
            this.btnEditEditExt.Name = "btnEditEditExt";
            this.btnEditEditExt.Size = new System.Drawing.Size(183, 42);
            this.btnEditEditExt.TabIndex = 10;
            this.btnEditEditExt.Text = "Edit";
            this.btnEditEditExt.UseVisualStyleBackColor = true;
            this.btnEditEditExt.Click += new System.EventHandler(this.btnEditEditExt_Click);
            // 
            // btnEditResetExt
            // 
            this.btnEditResetExt.AutoSize = true;
            this.btnEditResetExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnEditResetExt.Location = new System.Drawing.Point(465, 3);
            this.btnEditResetExt.Name = "btnEditResetExt";
            this.btnEditResetExt.Size = new System.Drawing.Size(246, 42);
            this.btnEditResetExt.TabIndex = 12;
            this.btnEditResetExt.Text = "Reset to default";
            this.btnEditResetExt.UseVisualStyleBackColor = true;
            this.btnEditResetExt.Click += new System.EventHandler(this.btnEditResetExt_Click);
            // 
            // btnEditEditAllExt
            // 
            this.btnEditEditAllExt.AutoSize = true;
            this.btnEditEditAllExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnEditEditAllExt.Location = new System.Drawing.Point(211, 3);
            this.btnEditEditAllExt.Name = "btnEditEditAllExt";
            this.btnEditEditAllExt.Size = new System.Drawing.Size(248, 42);
            this.btnEditEditAllExt.TabIndex = 11;
            this.btnEditEditAllExt.Text = "Edit all extensions";
            this.btnEditEditAllExt.UseVisualStyleBackColor = true;
            this.btnEditEditAllExt.Click += new System.EventHandler(this.btnEditEditAllExt_Click);
            // 
            // tabFileTypeAssoc
            // 
            this.tabFileTypeAssoc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tabFileTypeAssoc.Controls.Add(this.tableFileAssoc);
            this.tabFileTypeAssoc.Location = new System.Drawing.Point(4, 69);
            this.tabFileTypeAssoc.Margin = new System.Windows.Forms.Padding(0);
            this.tabFileTypeAssoc.Name = "tabFileTypeAssoc";
            this.tabFileTypeAssoc.Size = new System.Drawing.Size(736, 595);
            this.tabFileTypeAssoc.TabIndex = 1;
            this.tabFileTypeAssoc.Text = "file association";
            // 
            // tableFileAssoc
            // 
            this.tableFileAssoc.ColumnCount = 1;
            this.tableFileAssoc.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableFileAssoc.Controls.Add(this.panel2, 0, 1);
            this.tableFileAssoc.Controls.Add(this.panel8, 0, 0);
            this.tableFileAssoc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableFileAssoc.Location = new System.Drawing.Point(0, 0);
            this.tableFileAssoc.Name = "tableFileAssoc";
            this.tableFileAssoc.RowCount = 2;
            this.tableFileAssoc.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableFileAssoc.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableFileAssoc.Size = new System.Drawing.Size(736, 595);
            this.tableFileAssoc.TabIndex = 44;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.Controls.Add(this.btnUnregisterExt);
            this.panel2.Controls.Add(this.btnRegisterExt);
            this.panel2.Controls.Add(this.btnResetExt);
            this.panel2.Controls.Add(this.btnAddNewExt);
            this.panel2.Controls.Add(this.btnDeleteExt);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 475);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(736, 120);
            this.panel2.TabIndex = 35;
            // 
            // btnUnregisterExt
            // 
            this.btnUnregisterExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUnregisterExt.AutoSize = true;
            this.btnUnregisterExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnUnregisterExt.Location = new System.Drawing.Point(336, 51);
            this.btnUnregisterExt.Name = "btnUnregisterExt";
            this.btnUnregisterExt.Size = new System.Drawing.Size(378, 42);
            this.btnUnregisterExt.TabIndex = 47;
            this.btnUnregisterExt.Text = "[Unregister extensions]";
            this.btnUnregisterExt.UseVisualStyleBackColor = true;
            this.btnUnregisterExt.Click += new System.EventHandler(this.BtnUnregisteredExt_Click);
            // 
            // btnRegisterExt
            // 
            this.btnRegisterExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRegisterExt.AutoSize = true;
            this.btnRegisterExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnRegisterExt.Location = new System.Drawing.Point(336, 3);
            this.btnRegisterExt.Name = "btnRegisterExt";
            this.btnRegisterExt.Size = new System.Drawing.Size(378, 42);
            this.btnRegisterExt.TabIndex = 46;
            this.btnRegisterExt.Text = "[Set as Default photo viewer...]";
            this.btnRegisterExt.UseVisualStyleBackColor = true;
            this.btnRegisterExt.Click += new System.EventHandler(this.btnRegisterExt_Click);
            // 
            // btnResetExt
            // 
            this.btnResetExt.AutoSize = true;
            this.btnResetExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnResetExt.Location = new System.Drawing.Point(28, 51);
            this.btnResetExt.Name = "btnResetExt";
            this.btnResetExt.Size = new System.Drawing.Size(302, 42);
            this.btnResetExt.TabIndex = 45;
            this.btnResetExt.Text = "Reset to default";
            this.btnResetExt.UseVisualStyleBackColor = true;
            this.btnResetExt.Click += new System.EventHandler(this.btnResetExt_Click);
            // 
            // btnAddNewExt
            // 
            this.btnAddNewExt.AutoSize = true;
            this.btnAddNewExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnAddNewExt.Location = new System.Drawing.Point(28, 3);
            this.btnAddNewExt.Name = "btnAddNewExt";
            this.btnAddNewExt.Size = new System.Drawing.Size(148, 42);
            this.btnAddNewExt.TabIndex = 43;
            this.btnAddNewExt.Text = "Add";
            this.btnAddNewExt.UseVisualStyleBackColor = true;
            this.btnAddNewExt.Click += new System.EventHandler(this.btnAddNewExt_Click);
            // 
            // btnDeleteExt
            // 
            this.btnDeleteExt.AutoSize = true;
            this.btnDeleteExt.Enabled = false;
            this.btnDeleteExt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnDeleteExt.Location = new System.Drawing.Point(182, 3);
            this.btnDeleteExt.Name = "btnDeleteExt";
            this.btnDeleteExt.Size = new System.Drawing.Size(148, 42);
            this.btnDeleteExt.TabIndex = 44;
            this.btnDeleteExt.Text = "Delete";
            this.btnDeleteExt.UseVisualStyleBackColor = true;
            this.btnDeleteExt.Click += new System.EventHandler(this.btnDeleteExt_Click);
            // 
            // panel8
            // 
            this.panel8.BackColor = System.Drawing.Color.Transparent;
            this.panel8.Controls.Add(this.lblSupportedExtension);
            this.panel8.Controls.Add(this.lvExtension);
            this.panel8.Controls.Add(this.lnkOpenFileAssoc);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel8.Location = new System.Drawing.Point(0, 0);
            this.panel8.Margin = new System.Windows.Forms.Padding(0);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(736, 475);
            this.panel8.TabIndex = 43;
            // 
            // lblSupportedExtension
            // 
            this.lblSupportedExtension.AutoSize = true;
            this.lblSupportedExtension.Location = new System.Drawing.Point(22, 20);
            this.lblSupportedExtension.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblSupportedExtension.Name = "lblSupportedExtension";
            this.lblSupportedExtension.Size = new System.Drawing.Size(179, 23);
            this.lblSupportedExtension.TabIndex = 21;
            this.lblSupportedExtension.Text = "Supported extensions:";
            // 
            // lvExtension
            // 
            this.lvExtension.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvExtension.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.lvExtension.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clnExt,
            this.clnDescription});
            this.lvExtension.FullRowSelect = true;
            this.lvExtension.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvExtension.HideSelection = false;
            this.lvExtension.Location = new System.Drawing.Point(28, 47);
            this.lvExtension.Name = "lvExtension";
            this.lvExtension.ShowItemToolTips = true;
            this.lvExtension.Size = new System.Drawing.Size(686, 412);
            this.lvExtension.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvExtension.TabIndex = 42;
            this.lvExtension.TileSize = new System.Drawing.Size(100, 30);
            this.lvExtension.UseCompatibleStateImageBehavior = false;
            this.lvExtension.View = System.Windows.Forms.View.Details;
            this.lvExtension.SelectedIndexChanged += new System.EventHandler(this.lvExtension_SelectedIndexChanged);
            // 
            // clnExt
            // 
            this.clnExt.Text = "Extension";
            this.clnExt.Width = 150;
            // 
            // clnDescription
            // 
            this.clnDescription.Text = "Description";
            this.clnDescription.Width = 350;
            // 
            // lnkOpenFileAssoc
            // 
            this.lnkOpenFileAssoc.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkOpenFileAssoc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkOpenFileAssoc.BackColor = System.Drawing.Color.Transparent;
            this.lnkOpenFileAssoc.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkOpenFileAssoc.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkOpenFileAssoc.Location = new System.Drawing.Point(348, 14);
            this.lnkOpenFileAssoc.Name = "lnkOpenFileAssoc";
            this.lnkOpenFileAssoc.Size = new System.Drawing.Size(364, 32);
            this.lnkOpenFileAssoc.TabIndex = 41;
            this.lnkOpenFileAssoc.TabStop = true;
            this.lnkOpenFileAssoc.Text = "Open File Associations";
            this.lnkOpenFileAssoc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkOpenFileAssoc.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkOpenFileAssoc.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkOpenFileAssoc_LinkClicked);
            // 
            // tabToolbar
            // 
            this.tabToolbar.AutoScroll = true;
            this.tabToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tabToolbar.Controls.Add(this.tableLayoutPanel1);
            this.tabToolbar.Location = new System.Drawing.Point(4, 69);
            this.tabToolbar.Margin = new System.Windows.Forms.Padding(0);
            this.tabToolbar.Name = "tabToolbar";
            this.tabToolbar.Size = new System.Drawing.Size(736, 595);
            this.tabToolbar.TabIndex = 4;
            this.tabToolbar.Text = "toolbar";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel9, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel5, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(736, 595);
            this.tableLayoutPanel1.TabIndex = 50;
            // 
            // panel9
            // 
            this.panel9.Controls.Add(this.lblToolbarIconHeight);
            this.panel9.Controls.Add(this.numToolbarIconHeight);
            this.panel9.Controls.Add(this.chkHideTooltips);
            this.panel9.Controls.Add(this.lblToolbarPosition);
            this.panel9.Controls.Add(this.chkHorzCenterToolbarBtns);
            this.panel9.Controls.Add(this.cmbToolbarPosition);
            this.panel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel9.Location = new System.Drawing.Point(0, 3);
            this.panel9.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(733, 163);
            this.panel9.TabIndex = 49;
            // 
            // lblToolbarIconHeight
            // 
            this.lblToolbarIconHeight.AutoSize = true;
            this.lblToolbarIconHeight.Location = new System.Drawing.Point(371, 20);
            this.lblToolbarIconHeight.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblToolbarIconHeight.Name = "lblToolbarIconHeight";
            this.lblToolbarIconHeight.Size = new System.Drawing.Size(150, 23);
            this.lblToolbarIconHeight.TabIndex = 51;
            this.lblToolbarIconHeight.Text = "[Toolbar icon size:]";
            // 
            // numToolbarIconHeight
            // 
            this.numToolbarIconHeight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.numToolbarIconHeight.Location = new System.Drawing.Point(375, 46);
            this.numToolbarIconHeight.Margin = new System.Windows.Forms.Padding(1);
            this.numToolbarIconHeight.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numToolbarIconHeight.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numToolbarIconHeight.Name = "numToolbarIconHeight";
            this.numToolbarIconHeight.Size = new System.Drawing.Size(103, 30);
            this.numToolbarIconHeight.TabIndex = 48;
            this.numToolbarIconHeight.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // chkHideTooltips
            // 
            this.chkHideTooltips.AutoSize = true;
            this.chkHideTooltips.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkHideTooltips.Location = new System.Drawing.Point(27, 118);
            this.chkHideTooltips.Margin = new System.Windows.Forms.Padding(1);
            this.chkHideTooltips.Name = "chkHideTooltips";
            this.chkHideTooltips.Size = new System.Drawing.Size(212, 28);
            this.chkHideTooltips.TabIndex = 50;
            this.chkHideTooltips.Text = "[Hide toolbar tooltips]";
            this.chkHideTooltips.UseVisualStyleBackColor = true;
            // 
            // lblToolbarPosition
            // 
            this.lblToolbarPosition.AutoSize = true;
            this.lblToolbarPosition.BackColor = System.Drawing.Color.Transparent;
            this.lblToolbarPosition.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToolbarPosition.Location = new System.Drawing.Point(22, 20);
            this.lblToolbarPosition.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.lblToolbarPosition.Name = "lblToolbarPosition";
            this.lblToolbarPosition.Size = new System.Drawing.Size(146, 23);
            this.lblToolbarPosition.TabIndex = 44;
            this.lblToolbarPosition.Text = "[Toolbar position:]";
            // 
            // chkHorzCenterToolbarBtns
            // 
            this.chkHorzCenterToolbarBtns.AutoSize = true;
            this.chkHorzCenterToolbarBtns.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkHorzCenterToolbarBtns.Location = new System.Drawing.Point(28, 84);
            this.chkHorzCenterToolbarBtns.Margin = new System.Windows.Forms.Padding(1);
            this.chkHorzCenterToolbarBtns.Name = "chkHorzCenterToolbarBtns";
            this.chkHorzCenterToolbarBtns.Size = new System.Drawing.Size(406, 28);
            this.chkHorzCenterToolbarBtns.TabIndex = 49;
            this.chkHorzCenterToolbarBtns.Text = "[Center toolbar buttons horizontally in window]";
            this.chkHorzCenterToolbarBtns.UseVisualStyleBackColor = true;
            // 
            // cmbToolbarPosition
            // 
            this.cmbToolbarPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbToolbarPosition.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbToolbarPosition.FormattingEnabled = true;
            this.cmbToolbarPosition.Location = new System.Drawing.Point(28, 47);
            this.cmbToolbarPosition.Margin = new System.Windows.Forms.Padding(1);
            this.cmbToolbarPosition.Name = "cmbToolbarPosition";
            this.cmbToolbarPosition.Size = new System.Drawing.Size(259, 31);
            this.cmbToolbarPosition.TabIndex = 47;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.lblAvailBtns);
            this.panel5.Controls.Add(this.btnMoveRight);
            this.panel5.Controls.Add(this.btnMoveLeft);
            this.panel5.Controls.Add(this.btnMoveUp);
            this.panel5.Controls.Add(this.lvAvailButtons);
            this.panel5.Controls.Add(this.lblUsedBtns);
            this.panel5.Controls.Add(this.lvUsedButtons);
            this.panel5.Controls.Add(this.btnMoveDown);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 172);
            this.panel5.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(733, 420);
            this.panel5.TabIndex = 48;
            // 
            // lblAvailBtns
            // 
            this.lblAvailBtns.AutoSize = true;
            this.lblAvailBtns.BackColor = System.Drawing.Color.Transparent;
            this.lblAvailBtns.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvailBtns.Location = new System.Drawing.Point(22, 8);
            this.lblAvailBtns.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.lblAvailBtns.Name = "lblAvailBtns";
            this.lblAvailBtns.Size = new System.Drawing.Size(156, 23);
            this.lblAvailBtns.TabIndex = 3;
            this.lblAvailBtns.Text = "[Available Buttons:]";
            // 
            // btnMoveRight
            // 
            this.btnMoveRight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnMoveRight.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveRight.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveRight.Location = new System.Drawing.Point(317, 254);
            this.btnMoveRight.Margin = new System.Windows.Forms.Padding(4);
            this.btnMoveRight.Name = "btnMoveRight";
            this.btnMoveRight.Size = new System.Drawing.Size(47, 47);
            this.btnMoveRight.TabIndex = 53;
            this.btnMoveRight.Text = "►";
            this.btnMoveRight.UseVisualStyleBackColor = true;
            this.btnMoveRight.Click += new System.EventHandler(this.btnMoveRight_Click);
            // 
            // btnMoveLeft
            // 
            this.btnMoveLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnMoveLeft.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveLeft.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveLeft.Location = new System.Drawing.Point(317, 188);
            this.btnMoveLeft.Name = "btnMoveLeft";
            this.btnMoveLeft.Size = new System.Drawing.Size(47, 47);
            this.btnMoveLeft.TabIndex = 52;
            this.btnMoveLeft.Text = "◄";
            this.btnMoveLeft.UseVisualStyleBackColor = true;
            this.btnMoveLeft.Click += new System.EventHandler(this.btnMoveLeft_Click);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnMoveUp.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveUp.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveUp.Location = new System.Drawing.Point(669, 188);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(47, 47);
            this.btnMoveUp.TabIndex = 55;
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
            this.lvAvailButtons.Location = new System.Drawing.Point(28, 40);
            this.lvAvailButtons.Name = "lvAvailButtons";
            this.lvAvailButtons.ShowGroups = false;
            this.lvAvailButtons.ShowItemToolTips = true;
            this.lvAvailButtons.Size = new System.Drawing.Size(276, 364);
            this.lvAvailButtons.TabIndex = 51;
            this.lvAvailButtons.UseCompatibleStateImageBehavior = false;
            this.lvAvailButtons.SelectedIndexChanged += new System.EventHandler(this.lvAvailButtons_SelectedIndexChanged);
            this.lvAvailButtons.Resize += new System.EventHandler(this.ButtonsListView_Resize);
            // 
            // lblUsedBtns
            // 
            this.lblUsedBtns.AutoSize = true;
            this.lblUsedBtns.BackColor = System.Drawing.Color.Transparent;
            this.lblUsedBtns.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsedBtns.Location = new System.Drawing.Point(371, 8);
            this.lblUsedBtns.Margin = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.lblUsedBtns.Name = "lblUsedBtns";
            this.lblUsedBtns.Size = new System.Drawing.Size(146, 23);
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
            this.lvUsedButtons.Location = new System.Drawing.Point(375, 40);
            this.lvUsedButtons.Name = "lvUsedButtons";
            this.lvUsedButtons.ShowGroups = false;
            this.lvUsedButtons.ShowItemToolTips = true;
            this.lvUsedButtons.Size = new System.Drawing.Size(279, 364);
            this.lvUsedButtons.TabIndex = 54;
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
            this.btnMoveDown.Location = new System.Drawing.Point(669, 254);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(47, 47);
            this.btnMoveDown.TabIndex = 56;
            this.btnMoveDown.Text = "▼";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // tabTools
            // 
            this.tabTools.AutoScroll = true;
            this.tabTools.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tabTools.Controls.Add(this.chkColorUseHSVA);
            this.tabTools.Controls.Add(this.lnkSelectExifTool);
            this.tabTools.Controls.Add(this.lblExifToolPath);
            this.tabTools.Controls.Add(this.lblExifTool);
            this.tabTools.Controls.Add(this.chkExifToolAlwaysOnTop);
            this.tabTools.Controls.Add(this.lblPageNav);
            this.tabTools.Controls.Add(this.chkShowPageNavAuto);
            this.tabTools.Controls.Add(this.chkColorUseHSLA);
            this.tabTools.Controls.Add(this.lblColorPicker);
            this.tabTools.Controls.Add(this.chkColorUseHEXA);
            this.tabTools.Controls.Add(this.chkColorUseRGBA);
            this.tabTools.Location = new System.Drawing.Point(4, 69);
            this.tabTools.Margin = new System.Windows.Forms.Padding(0);
            this.tabTools.Name = "tabTools";
            this.tabTools.Size = new System.Drawing.Size(736, 595);
            this.tabTools.TabIndex = 5;
            this.tabTools.Text = "tools";
            // 
            // chkColorUseHSVA
            // 
            this.chkColorUseHSVA.AutoSize = true;
            this.chkColorUseHSVA.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkColorUseHSVA.Location = new System.Drawing.Point(42, 149);
            this.chkColorUseHSVA.Margin = new System.Windows.Forms.Padding(1);
            this.chkColorUseHSVA.Name = "chkColorUseHSVA";
            this.chkColorUseHSVA.Size = new System.Drawing.Size(186, 28);
            this.chkColorUseHSVA.TabIndex = 58;
            this.chkColorUseHSVA.Text = "[Use HSVA format]";
            this.chkColorUseHSVA.UseVisualStyleBackColor = true;
            // 
            // lnkSelectExifTool
            // 
            this.lnkSelectExifTool.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkSelectExifTool.AutoSize = true;
            this.lnkSelectExifTool.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkSelectExifTool.Location = new System.Drawing.Point(38, 386);
            this.lnkSelectExifTool.Name = "lnkSelectExifTool";
            this.lnkSelectExifTool.Size = new System.Drawing.Size(148, 23);
            this.lnkSelectExifTool.TabIndex = 61;
            this.lnkSelectExifTool.TabStop = true;
            this.lnkSelectExifTool.Text = "Select Exif tool file";
            this.lnkSelectExifTool.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkSelectExifTool.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSelectExifTool_LinkClicked);
            // 
            // lblExifToolPath
            // 
            this.lblExifToolPath.AutoSize = true;
            this.lblExifToolPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExifToolPath.Location = new System.Drawing.Point(38, 413);
            this.lblExifToolPath.Name = "lblExifToolPath";
            this.lblExifToolPath.Size = new System.Drawing.Size(186, 23);
            this.lblExifToolPath.TabIndex = 61;
            this.lblExifToolPath.Text = "C:\\aaa\\bbb\\exiftool.exe";
            // 
            // lblExifTool
            // 
            this.lblExifTool.AutoSize = true;
            this.lblExifTool.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExifTool.Location = new System.Drawing.Point(22, 311);
            this.lblExifTool.Name = "lblExifTool";
            this.lblExifTool.Size = new System.Drawing.Size(89, 23);
            this.lblExifTool.TabIndex = 59;
            this.lblExifTool.Text = "[Exif tool]";
            // 
            // chkExifToolAlwaysOnTop
            // 
            this.chkExifToolAlwaysOnTop.AutoSize = true;
            this.chkExifToolAlwaysOnTop.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkExifToolAlwaysOnTop.Location = new System.Drawing.Point(42, 340);
            this.chkExifToolAlwaysOnTop.Margin = new System.Windows.Forms.Padding(1);
            this.chkExifToolAlwaysOnTop.Name = "chkExifToolAlwaysOnTop";
            this.chkExifToolAlwaysOnTop.Size = new System.Drawing.Size(269, 28);
            this.chkExifToolAlwaysOnTop.TabIndex = 60;
            this.chkExifToolAlwaysOnTop.Text = "[Keep Exif tool always on top]";
            this.chkExifToolAlwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // lblPageNav
            // 
            this.lblPageNav.AutoSize = true;
            this.lblPageNav.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageNav.Location = new System.Drawing.Point(22, 212);
            this.lblPageNav.Name = "lblPageNav";
            this.lblPageNav.Size = new System.Drawing.Size(151, 23);
            this.lblPageNav.TabIndex = 58;
            this.lblPageNav.Text = "[Page navigation]";
            // 
            // chkShowPageNavAuto
            // 
            this.chkShowPageNavAuto.AutoSize = true;
            this.chkShowPageNavAuto.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkShowPageNavAuto.Location = new System.Drawing.Point(42, 242);
            this.chkShowPageNavAuto.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowPageNavAuto.Name = "chkShowPageNavAuto";
            this.chkShowPageNavAuto.Size = new System.Drawing.Size(467, 28);
            this.chkShowPageNavAuto.TabIndex = 59;
            this.chkShowPageNavAuto.Text = "[Auto-show Page navigation tool for multi-page image]";
            this.chkShowPageNavAuto.UseVisualStyleBackColor = true;
            // 
            // chkColorUseHSLA
            // 
            this.chkColorUseHSLA.AutoSize = true;
            this.chkColorUseHSLA.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkColorUseHSLA.Location = new System.Drawing.Point(42, 115);
            this.chkColorUseHSLA.Margin = new System.Windows.Forms.Padding(1);
            this.chkColorUseHSLA.Name = "chkColorUseHSLA";
            this.chkColorUseHSLA.Size = new System.Drawing.Size(174, 28);
            this.chkColorUseHSLA.TabIndex = 57;
            this.chkColorUseHSLA.Text = "Use HSLA format";
            this.chkColorUseHSLA.UseVisualStyleBackColor = true;
            // 
            // lblColorPicker
            // 
            this.lblColorPicker.AutoSize = true;
            this.lblColorPicker.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblColorPicker.Location = new System.Drawing.Point(22, 20);
            this.lblColorPicker.Name = "lblColorPicker";
            this.lblColorPicker.Size = new System.Drawing.Size(120, 23);
            this.lblColorPicker.TabIndex = 47;
            this.lblColorPicker.Text = "[Color picker]";
            // 
            // chkColorUseHEXA
            // 
            this.chkColorUseHEXA.AutoSize = true;
            this.chkColorUseHEXA.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkColorUseHEXA.Location = new System.Drawing.Point(42, 81);
            this.chkColorUseHEXA.Margin = new System.Windows.Forms.Padding(1);
            this.chkColorUseHEXA.Name = "chkColorUseHEXA";
            this.chkColorUseHEXA.Size = new System.Drawing.Size(249, 28);
            this.chkColorUseHEXA.TabIndex = 56;
            this.chkColorUseHEXA.Text = "Use HEX with alpha format";
            this.chkColorUseHEXA.UseVisualStyleBackColor = true;
            // 
            // chkColorUseRGBA
            // 
            this.chkColorUseRGBA.AutoSize = true;
            this.chkColorUseRGBA.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkColorUseRGBA.Location = new System.Drawing.Point(42, 47);
            this.chkColorUseRGBA.Margin = new System.Windows.Forms.Padding(1);
            this.chkColorUseRGBA.Name = "chkColorUseRGBA";
            this.chkColorUseRGBA.Size = new System.Drawing.Size(177, 28);
            this.chkColorUseRGBA.TabIndex = 55;
            this.chkColorUseRGBA.Text = "Use RGBA format";
            this.chkColorUseRGBA.UseVisualStyleBackColor = true;
            // 
            // tabKeyboard
            // 
            this.tabKeyboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tabKeyboard.Controls.Add(this.btnKeyReset);
            this.tabKeyboard.Controls.Add(this.cmbKeysSpaceBack);
            this.tabKeyboard.Controls.Add(this.cmbKeysPgUpDown);
            this.tabKeyboard.Controls.Add(this.cmbKeysUpDown);
            this.tabKeyboard.Controls.Add(this.cmbKeysLeftRight);
            this.tabKeyboard.Controls.Add(this.lblKeysSpaceBack);
            this.tabKeyboard.Controls.Add(this.lblKeysPageUpDown);
            this.tabKeyboard.Controls.Add(this.lblKeysUpDown);
            this.tabKeyboard.Controls.Add(this.lblKeysLeftRight);
            this.tabKeyboard.Location = new System.Drawing.Point(4, 69);
            this.tabKeyboard.Name = "tabKeyboard";
            this.tabKeyboard.Padding = new System.Windows.Forms.Padding(3);
            this.tabKeyboard.Size = new System.Drawing.Size(736, 595);
            this.tabKeyboard.TabIndex = 8;
            this.tabKeyboard.Text = "keyboard";
            // 
            // btnKeyReset
            // 
            this.btnKeyReset.Location = new System.Drawing.Point(28, 420);
            this.btnKeyReset.Name = "btnKeyReset";
            this.btnKeyReset.Size = new System.Drawing.Size(260, 42);
            this.btnKeyReset.TabIndex = 77;
            this.btnKeyReset.Text = "[Reset to default]";
            this.btnKeyReset.UseVisualStyleBackColor = true;
            this.btnKeyReset.Click += new System.EventHandler(this.btnKeyReset_Click);
            // 
            // cmbKeysSpaceBack
            // 
            this.cmbKeysSpaceBack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKeysSpaceBack.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbKeysSpaceBack.FormattingEnabled = true;
            this.cmbKeysSpaceBack.Location = new System.Drawing.Point(28, 279);
            this.cmbKeysSpaceBack.Name = "cmbKeysSpaceBack";
            this.cmbKeysSpaceBack.Size = new System.Drawing.Size(259, 31);
            this.cmbKeysSpaceBack.TabIndex = 76;
            // 
            // cmbKeysPgUpDown
            // 
            this.cmbKeysPgUpDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKeysPgUpDown.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbKeysPgUpDown.FormattingEnabled = true;
            this.cmbKeysPgUpDown.Location = new System.Drawing.Point(28, 201);
            this.cmbKeysPgUpDown.Name = "cmbKeysPgUpDown";
            this.cmbKeysPgUpDown.Size = new System.Drawing.Size(259, 31);
            this.cmbKeysPgUpDown.TabIndex = 75;
            // 
            // cmbKeysUpDown
            // 
            this.cmbKeysUpDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKeysUpDown.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbKeysUpDown.FormattingEnabled = true;
            this.cmbKeysUpDown.Location = new System.Drawing.Point(28, 123);
            this.cmbKeysUpDown.Name = "cmbKeysUpDown";
            this.cmbKeysUpDown.Size = new System.Drawing.Size(259, 31);
            this.cmbKeysUpDown.TabIndex = 74;
            // 
            // cmbKeysLeftRight
            // 
            this.cmbKeysLeftRight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKeysLeftRight.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbKeysLeftRight.FormattingEnabled = true;
            this.cmbKeysLeftRight.Location = new System.Drawing.Point(28, 45);
            this.cmbKeysLeftRight.Name = "cmbKeysLeftRight";
            this.cmbKeysLeftRight.Size = new System.Drawing.Size(259, 31);
            this.cmbKeysLeftRight.TabIndex = 73;
            // 
            // lblKeysSpaceBack
            // 
            this.lblKeysSpaceBack.AutoSize = true;
            this.lblKeysSpaceBack.Location = new System.Drawing.Point(22, 253);
            this.lblKeysSpaceBack.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblKeysSpaceBack.Name = "lblKeysSpaceBack";
            this.lblKeysSpaceBack.Size = new System.Drawing.Size(160, 23);
            this.lblKeysSpaceBack.TabIndex = 4;
            this.lblKeysSpaceBack.Text = "[Space / Backspace]";
            // 
            // lblKeysPageUpDown
            // 
            this.lblKeysPageUpDown.AutoSize = true;
            this.lblKeysPageUpDown.Location = new System.Drawing.Point(22, 174);
            this.lblKeysPageUpDown.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblKeysPageUpDown.Name = "lblKeysPageUpDown";
            this.lblKeysPageUpDown.Size = new System.Drawing.Size(177, 23);
            this.lblKeysPageUpDown.TabIndex = 3;
            this.lblKeysPageUpDown.Text = "[PageUp / PageDown]";
            // 
            // lblKeysUpDown
            // 
            this.lblKeysUpDown.AutoSize = true;
            this.lblKeysUpDown.Location = new System.Drawing.Point(22, 96);
            this.lblKeysUpDown.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblKeysUpDown.Name = "lblKeysUpDown";
            this.lblKeysUpDown.Size = new System.Drawing.Size(158, 23);
            this.lblKeysUpDown.TabIndex = 2;
            this.lblKeysUpDown.Text = "[Up / Down arrows]";
            // 
            // lblKeysLeftRight
            // 
            this.lblKeysLeftRight.AutoSize = true;
            this.lblKeysLeftRight.Location = new System.Drawing.Point(22, 20);
            this.lblKeysLeftRight.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblKeysLeftRight.Name = "lblKeysLeftRight";
            this.lblKeysLeftRight.Size = new System.Drawing.Size(160, 23);
            this.lblKeysLeftRight.TabIndex = 1;
            this.lblKeysLeftRight.Text = "[Left / Right arrows]";
            // 
            // tabTheme
            // 
            this.tabTheme.AutoScroll = true;
            this.tabTheme.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tabTheme.Controls.Add(this.tableTheme);
            this.tabTheme.Location = new System.Drawing.Point(4, 69);
            this.tabTheme.Margin = new System.Windows.Forms.Padding(0);
            this.tabTheme.Name = "tabTheme";
            this.tabTheme.Padding = new System.Windows.Forms.Padding(22, 20, 22, 20);
            this.tabTheme.Size = new System.Drawing.Size(736, 595);
            this.tabTheme.TabIndex = 6;
            this.tabTheme.Text = "Theme";
            // 
            // tableTheme
            // 
            this.tableTheme.ColumnCount = 2;
            this.tableTheme.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableTheme.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableTheme.Controls.Add(this.lblInstalledThemes, 0, 0);
            this.tableTheme.Controls.Add(this.panelThemeActions, 1, 1);
            this.tableTheme.Controls.Add(this.lnkThemeDownload, 0, 2);
            this.tableTheme.Controls.Add(this.lvTheme, 0, 1);
            this.tableTheme.Controls.Add(this.btnThemeApply, 1, 2);
            this.tableTheme.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableTheme.Location = new System.Drawing.Point(22, 20);
            this.tableTheme.Name = "tableTheme";
            this.tableTheme.RowCount = 3;
            this.tableTheme.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTheme.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableTheme.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTheme.Size = new System.Drawing.Size(692, 555);
            this.tableTheme.TabIndex = 73;
            // 
            // lblInstalledThemes
            // 
            this.lblInstalledThemes.AutoSize = true;
            this.lblInstalledThemes.Location = new System.Drawing.Point(0, 0);
            this.lblInstalledThemes.Margin = new System.Windows.Forms.Padding(0, 0, 1, 6);
            this.lblInstalledThemes.Name = "lblInstalledThemes";
            this.lblInstalledThemes.Size = new System.Drawing.Size(139, 23);
            this.lblInstalledThemes.TabIndex = 2;
            this.lblInstalledThemes.Text = "Installed themes:";
            // 
            // panelThemeActions
            // 
            this.panelThemeActions.AutoScroll = true;
            this.panelThemeActions.Controls.Add(this.tb3);
            this.panelThemeActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelThemeActions.Location = new System.Drawing.Point(392, 29);
            this.panelThemeActions.Margin = new System.Windows.Forms.Padding(0);
            this.panelThemeActions.Name = "panelThemeActions";
            this.panelThemeActions.Size = new System.Drawing.Size(300, 463);
            this.panelThemeActions.TabIndex = 29;
            // 
            // tb3
            // 
            this.tb3.ColumnCount = 1;
            this.tb3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tb3.Controls.Add(this.picPreview, 0, 0);
            this.tb3.Controls.Add(this.txtThemeInfo, 0, 6);
            this.tb3.Controls.Add(this.btnThemeFolderOpen, 0, 5);
            this.tb3.Controls.Add(this.btnThemeRefresh, 0, 1);
            this.tb3.Controls.Add(this.btnThemeSaveAs, 0, 4);
            this.tb3.Controls.Add(this.btnThemeInstall, 0, 2);
            this.tb3.Controls.Add(this.btnThemeUninstall, 0, 3);
            this.tb3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb3.Location = new System.Drawing.Point(0, 0);
            this.tb3.Name = "tb3";
            this.tb3.RowCount = 7;
            this.tb3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tb3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tb3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tb3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tb3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tb3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tb3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tb3.Size = new System.Drawing.Size(300, 463);
            this.tb3.TabIndex = 71;
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(165)))), ((int)(((byte)(168)))));
            this.picPreview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Location = new System.Drawing.Point(0, 3);
            this.picPreview.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(300, 150);
            this.picPreview.TabIndex = 34;
            this.picPreview.TabStop = false;
            // 
            // txtThemeInfo
            // 
            this.txtThemeInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.txtThemeInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtThemeInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtThemeInfo.Location = new System.Drawing.Point(0, 413);
            this.txtThemeInfo.Margin = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.txtThemeInfo.Multiline = true;
            this.txtThemeInfo.Name = "txtThemeInfo";
            this.txtThemeInfo.ReadOnly = true;
            this.txtThemeInfo.Size = new System.Drawing.Size(300, 64);
            this.txtThemeInfo.TabIndex = 70;
            // 
            // btnThemeFolderOpen
            // 
            this.btnThemeFolderOpen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThemeFolderOpen.AutoSize = true;
            this.btnThemeFolderOpen.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeFolderOpen.Location = new System.Drawing.Point(0, 353);
            this.btnThemeFolderOpen.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.btnThemeFolderOpen.Name = "btnThemeFolderOpen";
            this.btnThemeFolderOpen.Size = new System.Drawing.Size(300, 40);
            this.btnThemeFolderOpen.TabIndex = 69;
            this.btnThemeFolderOpen.Text = "Open theme folder";
            this.btnThemeFolderOpen.UseVisualStyleBackColor = true;
            this.btnThemeFolderOpen.Click += new System.EventHandler(this.btnThemeFolderOpen_Click);
            // 
            // btnThemeRefresh
            // 
            this.btnThemeRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThemeRefresh.AutoSize = true;
            this.btnThemeRefresh.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeRefresh.Location = new System.Drawing.Point(0, 173);
            this.btnThemeRefresh.Margin = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.btnThemeRefresh.Name = "btnThemeRefresh";
            this.btnThemeRefresh.Size = new System.Drawing.Size(300, 40);
            this.btnThemeRefresh.TabIndex = 65;
            this.btnThemeRefresh.Text = "Refresh";
            this.btnThemeRefresh.UseVisualStyleBackColor = true;
            this.btnThemeRefresh.Click += new System.EventHandler(this.btnThemeRefresh_Click);
            // 
            // btnThemeSaveAs
            // 
            this.btnThemeSaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThemeSaveAs.AutoSize = true;
            this.btnThemeSaveAs.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeSaveAs.Location = new System.Drawing.Point(0, 308);
            this.btnThemeSaveAs.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.btnThemeSaveAs.Name = "btnThemeSaveAs";
            this.btnThemeSaveAs.Size = new System.Drawing.Size(300, 40);
            this.btnThemeSaveAs.TabIndex = 68;
            this.btnThemeSaveAs.Text = "Save As";
            this.btnThemeSaveAs.UseVisualStyleBackColor = true;
            this.btnThemeSaveAs.Click += new System.EventHandler(this.btnThemeSaveAs_Click);
            // 
            // btnThemeInstall
            // 
            this.btnThemeInstall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThemeInstall.AutoSize = true;
            this.btnThemeInstall.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeInstall.Location = new System.Drawing.Point(0, 218);
            this.btnThemeInstall.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.btnThemeInstall.Name = "btnThemeInstall";
            this.btnThemeInstall.Size = new System.Drawing.Size(300, 40);
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
            this.btnThemeUninstall.Location = new System.Drawing.Point(0, 263);
            this.btnThemeUninstall.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.btnThemeUninstall.Name = "btnThemeUninstall";
            this.btnThemeUninstall.Size = new System.Drawing.Size(300, 40);
            this.btnThemeUninstall.TabIndex = 67;
            this.btnThemeUninstall.Text = "Uninstall";
            this.btnThemeUninstall.UseVisualStyleBackColor = true;
            this.btnThemeUninstall.Click += new System.EventHandler(this.btnThemeUninstall_Click);
            // 
            // lnkThemeDownload
            // 
            this.lnkThemeDownload.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkThemeDownload.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lnkThemeDownload.AutoSize = true;
            this.lnkThemeDownload.BackColor = System.Drawing.Color.Transparent;
            this.lnkThemeDownload.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkThemeDownload.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkThemeDownload.Location = new System.Drawing.Point(3, 522);
            this.lnkThemeDownload.Margin = new System.Windows.Forms.Padding(3, 20, 3, 0);
            this.lnkThemeDownload.Name = "lnkThemeDownload";
            this.lnkThemeDownload.Size = new System.Drawing.Size(158, 23);
            this.lnkThemeDownload.TabIndex = 72;
            this.lnkThemeDownload.TabStop = true;
            this.lnkThemeDownload.Text = "[Download themes]";
            this.lnkThemeDownload.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lnkThemeDownload.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkThemeDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkThemeDownload_LinkClicked);
            // 
            // lvTheme
            // 
            this.lvTheme.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvTheme.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvTheme.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.lvTheme.FullRowSelect = true;
            this.lvTheme.HideSelection = false;
            this.lvTheme.LargeImageList = this.imglGeneral;
            this.lvTheme.Location = new System.Drawing.Point(3, 32);
            this.lvTheme.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.lvTheme.MultiSelect = false;
            this.lvTheme.Name = "lvTheme";
            this.lvTheme.ShowItemToolTips = true;
            this.lvTheme.Size = new System.Drawing.Size(379, 457);
            this.lvTheme.SmallImageList = this.imglGeneral;
            this.lvTheme.StateImageList = this.imglGeneral;
            this.lvTheme.TabIndex = 64;
            this.lvTheme.UseCompatibleStateImageBehavior = false;
            this.lvTheme.View = System.Windows.Forms.View.List;
            this.lvTheme.SelectedIndexChanged += new System.EventHandler(this.lvTheme_SelectedIndexChanged);
            // 
            // btnThemeApply
            // 
            this.btnThemeApply.AutoSize = true;
            this.btnThemeApply.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnThemeApply.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeApply.Location = new System.Drawing.Point(392, 512);
            this.btnThemeApply.Margin = new System.Windows.Forms.Padding(0, 20, 0, 3);
            this.btnThemeApply.Name = "btnThemeApply";
            this.btnThemeApply.Size = new System.Drawing.Size(300, 40);
            this.btnThemeApply.TabIndex = 71;
            this.btnThemeApply.Text = "Apply Theme";
            this.btnThemeApply.UseVisualStyleBackColor = true;
            this.btnThemeApply.Click += new System.EventHandler(this.btnThemeApply_Click);
            // 
            // tabLanguage
            // 
            this.tabLanguage.AutoScroll = true;
            this.tabLanguage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tabLanguage.Controls.Add(this.lblLanguageWarning);
            this.tabLanguage.Controls.Add(this.lnkInstallLanguage);
            this.tabLanguage.Controls.Add(this.lnkRefresh);
            this.tabLanguage.Controls.Add(this.lnkEdit);
            this.tabLanguage.Controls.Add(this.lnkCreateNew);
            this.tabLanguage.Controls.Add(this.lnkGetMoreLanguage);
            this.tabLanguage.Controls.Add(this.cmbLanguage);
            this.tabLanguage.Controls.Add(this.lblLanguageText);
            this.tabLanguage.Location = new System.Drawing.Point(4, 69);
            this.tabLanguage.Margin = new System.Windows.Forms.Padding(0);
            this.tabLanguage.Name = "tabLanguage";
            this.tabLanguage.Size = new System.Drawing.Size(736, 595);
            this.tabLanguage.TabIndex = 2;
            this.tabLanguage.Text = "Lang";
            // 
            // lblLanguageWarning
            // 
            this.lblLanguageWarning.AutoSize = true;
            this.lblLanguageWarning.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLanguageWarning.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(115)))), ((int)(((byte)(17)))));
            this.lblLanguageWarning.Location = new System.Drawing.Point(22, 82);
            this.lblLanguageWarning.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblLanguageWarning.Name = "lblLanguageWarning";
            this.lblLanguageWarning.Size = new System.Drawing.Size(525, 23);
            this.lblLanguageWarning.TabIndex = 25;
            this.lblLanguageWarning.Text = "[This language pack may be not compatible with ImageGlass 3.2.0.16.]";
            this.lblLanguageWarning.Visible = false;
            // 
            // lnkInstallLanguage
            // 
            this.lnkInstallLanguage.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkInstallLanguage.AutoSize = true;
            this.lnkInstallLanguage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkInstallLanguage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkInstallLanguage.Location = new System.Drawing.Point(22, 174);
            this.lnkInstallLanguage.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkInstallLanguage.Name = "lnkInstallLanguage";
            this.lnkInstallLanguage.Size = new System.Drawing.Size(297, 23);
            this.lnkInstallLanguage.TabIndex = 60;
            this.lnkInstallLanguage.TabStop = true;
            this.lnkInstallLanguage.Text = "> Install new language pack (*.iglang)";
            this.lnkInstallLanguage.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkInstallLanguage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkInstallLanguage_LinkClicked);
            // 
            // lnkRefresh
            // 
            this.lnkRefresh.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkRefresh.AutoSize = true;
            this.lnkRefresh.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkRefresh.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkRefresh.Location = new System.Drawing.Point(282, 47);
            this.lnkRefresh.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkRefresh.Name = "lnkRefresh";
            this.lnkRefresh.Size = new System.Drawing.Size(83, 23);
            this.lnkRefresh.TabIndex = 59;
            this.lnkRefresh.TabStop = true;
            this.lnkRefresh.Text = "> Refresh";
            this.lnkRefresh.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRefresh_LinkClicked);
            // 
            // lnkEdit
            // 
            this.lnkEdit.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkEdit.AutoSize = true;
            this.lnkEdit.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkEdit.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkEdit.Location = new System.Drawing.Point(22, 236);
            this.lnkEdit.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkEdit.Name = "lnkEdit";
            this.lnkEdit.Size = new System.Drawing.Size(239, 23);
            this.lnkEdit.TabIndex = 62;
            this.lnkEdit.TabStop = true;
            this.lnkEdit.Text = "> Edit selected language pack";
            this.lnkEdit.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkEdit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkEdit_LinkClicked);
            // 
            // lnkCreateNew
            // 
            this.lnkCreateNew.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkCreateNew.AutoSize = true;
            this.lnkCreateNew.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkCreateNew.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkCreateNew.Location = new System.Drawing.Point(22, 205);
            this.lnkCreateNew.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkCreateNew.Name = "lnkCreateNew";
            this.lnkCreateNew.Size = new System.Drawing.Size(229, 23);
            this.lnkCreateNew.TabIndex = 61;
            this.lnkCreateNew.TabStop = true;
            this.lnkCreateNew.Text = "> Create new language pack";
            this.lnkCreateNew.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkCreateNew.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCreateNew_LinkClicked);
            // 
            // lnkGetMoreLanguage
            // 
            this.lnkGetMoreLanguage.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkGetMoreLanguage.AutoSize = true;
            this.lnkGetMoreLanguage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkGetMoreLanguage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkGetMoreLanguage.Location = new System.Drawing.Point(22, 267);
            this.lnkGetMoreLanguage.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkGetMoreLanguage.Name = "lnkGetMoreLanguage";
            this.lnkGetMoreLanguage.Size = new System.Drawing.Size(222, 23);
            this.lnkGetMoreLanguage.TabIndex = 63;
            this.lnkGetMoreLanguage.TabStop = true;
            this.lnkGetMoreLanguage.Text = "> Get more language packs";
            this.lnkGetMoreLanguage.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
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
            this.cmbLanguage.Location = new System.Drawing.Point(28, 45);
            this.cmbLanguage.Margin = new System.Windows.Forms.Padding(1);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(237, 31);
            this.cmbLanguage.TabIndex = 58;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // lblLanguageText
            // 
            this.lblLanguageText.AutoSize = true;
            this.lblLanguageText.Location = new System.Drawing.Point(22, 20);
            this.lblLanguageText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblLanguageText.Name = "lblLanguageText";
            this.lblLanguageText.Size = new System.Drawing.Size(161, 23);
            this.lblLanguageText.TabIndex = 1;
            this.lblLanguageText.Text = "Installed languages:";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.AutoSize = true;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSave.Location = new System.Drawing.Point(541, 12);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(130, 42);
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
            this.btnCancel.Location = new System.Drawing.Point(679, 12);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(130, 42);
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
            this.btnApply.Location = new System.Drawing.Point(817, 12);
            this.btnApply.Margin = new System.Windows.Forms.Padding(4);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(130, 42);
            this.btnApply.TabIndex = 2;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // tblayout
            // 
            this.tblayout.BackColor = System.Drawing.Color.Transparent;
            this.tblayout.ColumnCount = 1;
            this.tblayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayout.Controls.Add(this.sp1, 0, 0);
            this.tblayout.Controls.Add(this.panel4, 0, 1);
            this.tblayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblayout.Location = new System.Drawing.Point(0, 0);
            this.tblayout.Margin = new System.Windows.Forms.Padding(0);
            this.tblayout.Name = "tblayout";
            this.tblayout.RowCount = 2;
            this.tblayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            this.tblayout.Size = new System.Drawing.Size(973, 735);
            this.tblayout.TabIndex = 19;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.Transparent;
            this.panel4.Controls.Add(this.btnApply);
            this.panel4.Controls.Add(this.btnSave);
            this.panel4.Controls.Add(this.btnCancel);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 668);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(973, 67);
            this.panel4.TabIndex = 18;
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(134F, 134F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(165)))), ((int)(((byte)(168)))));
            this.ClientSize = new System.Drawing.Size(973, 735);
            this.Controls.Add(this.tblayout);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(1);
            this.MinimumSize = new System.Drawing.Size(641, 471);
            this.Name = "frmSetting";
            this.RightToLeftLayout = true;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSetting_FormClosing);
            this.Load += new System.EventHandler(this.frmSetting_Load);
            this.SizeChanged += new System.EventHandler(this.frmSetting_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmSetting_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundColor)).EndInit();
            this.sp1.Panel1.ResumeLayout(false);
            this.sp1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sp1)).EndInit();
            this.sp1.ResumeLayout(false);
            this.tableTabHeaders.ResumeLayout(false);
            this.tab1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabImage.ResumeLayout(false);
            this.tabImage.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSlideShowInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSlideshowIntervalTo)).EndInit();
            this.tabEdit.ResumeLayout(false);
            this.tableEdit.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numImageQuality)).EndInit();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.tabFileTypeAssoc.ResumeLayout(false);
            this.tableFileAssoc.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel8.ResumeLayout(false);
            this.panel8.PerformLayout();
            this.tabToolbar.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel9.ResumeLayout(false);
            this.panel9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numToolbarIconHeight)).EndInit();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.tabTools.ResumeLayout(false);
            this.tabTools.PerformLayout();
            this.tabKeyboard.ResumeLayout(false);
            this.tabKeyboard.PerformLayout();
            this.tabTheme.ResumeLayout(false);
            this.tableTheme.ResumeLayout(false);
            this.tableTheme.PerformLayout();
            this.panelThemeActions.ResumeLayout(false);
            this.tb3.ResumeLayout(false);
            this.tb3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.tabLanguage.ResumeLayout(false);
            this.tabLanguage.PerformLayout();
            this.tblayout.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.ImageList imglTheme;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.Label lblFileTypeAssoc;
        private System.Windows.Forms.Label lblGeneral;
        private System.Windows.Forms.ToolTip tip1;
        private System.Windows.Forms.TabPage tabLanguage;
        private System.Windows.Forms.LinkLabel lnkRefresh;
        private System.Windows.Forms.LinkLabel lnkEdit;
        private System.Windows.Forms.LinkLabel lnkCreateNew;
        private System.Windows.Forms.LinkLabel lnkGetMoreLanguage;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.Label lblLanguageText;
        private System.Windows.Forms.TabPage tabFileTypeAssoc;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.CheckBox chkShowToolBar;
        private System.Windows.Forms.PictureBox picBackgroundColor;
        private System.Windows.Forms.Label lblBackGroundColor;
        private System.Windows.Forms.CheckBox chkWelcomePicture;
        private System.Windows.Forms.CheckBox chkAutoUpdate;
        private UI.NakedTabControl tab1;
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
        private System.Windows.Forms.ColumnHeader clnExt;
        private System.Windows.Forms.LinkLabel lnkOpenFileAssoc;
        private System.Windows.Forms.Button btnResetExt;
        private System.Windows.Forms.Button btnDeleteExt;
        private System.Windows.Forms.Button btnAddNewExt;
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
        private System.Windows.Forms.CheckBox chkUseFileExplorerSortOrder;
        private System.Windows.Forms.CheckBox chkLoopSlideshow;
        private System.Windows.Forms.Label lblImageLoadingOrder;
        private System.Windows.Forms.ComboBox cmbImageOrder;
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
        private System.Windows.Forms.TableLayoutPanel tableTabHeaders;
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
        private System.Windows.Forms.Label lblTools;
        private System.Windows.Forms.TabPage tabTools;
        private System.Windows.Forms.Label lblColorPicker;
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
        private System.Windows.Forms.TabPage tabKeyboard;
        private System.Windows.Forms.Button btnKeyReset;
        private System.Windows.Forms.ComboBox cmbKeysSpaceBack;
        private System.Windows.Forms.ComboBox cmbKeysPgUpDown;
        private System.Windows.Forms.ComboBox cmbKeysUpDown;
        private System.Windows.Forms.ComboBox cmbKeysLeftRight;
        private System.Windows.Forms.Label lblKeysSpaceBack;
        private System.Windows.Forms.Label lblKeysPageUpDown;
        private System.Windows.Forms.Label lblKeysUpDown;
        private System.Windows.Forms.Label lblKeysLeftRight;
        private System.Windows.Forms.Label lblKeyboard;
        private System.Windows.Forms.ComboBox cmbImageOrderType;
        private System.Windows.Forms.TextBox txtZoomLevels;
        private System.Windows.Forms.Label lblZoomLevels;
        private System.Windows.Forms.Label lblImageBoosterCachedCount;
        private System.Windows.Forms.ComboBox cmbImageBoosterCachedCount;
        private System.Windows.Forms.CheckBox chkIsCenterImage;
        private System.Windows.Forms.CheckBox chkCenterWindowFit;
        private System.Windows.Forms.CheckBox chkShowToast;
        private System.Windows.Forms.Label lblPageNav;
        private System.Windows.Forms.CheckBox chkShowPageNavAuto;
        private System.Windows.Forms.CheckBox chkShowSlideshowCountdown;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.NumericUpDown numSlideShowInterval;
        private System.Windows.Forms.NumericUpDown numSlideshowIntervalTo;
        private System.Windows.Forms.Label lblSlideshowIntervalTo;
        private System.Windows.Forms.CheckBox chkRandomSlideshowInterval;
        private System.Windows.Forms.Label lblHeadViewer;
        private System.Windows.Forms.TableLayoutPanel tableEdit;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.TableLayoutPanel tableFileAssoc;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel9;
        private System.Windows.Forms.TableLayoutPanel tableTheme;
        private System.Windows.Forms.CheckBox chkUseTouchGesture;
        private System.Windows.Forms.Label lblExifTool;
        private System.Windows.Forms.CheckBox chkExifToolAlwaysOnTop;
        private System.Windows.Forms.Label lblExifToolPath;
        private System.Windows.Forms.LinkLabel lnkSelectExifTool;
        private System.Windows.Forms.CheckBox chkHideTooltips;
        private System.Windows.Forms.CheckBox chkColorUseHSVA;
        private System.Windows.Forms.Label lblToolbarIconHeight;
        private System.Windows.Forms.NumericUpDown numToolbarIconHeight;
        private System.Windows.Forms.TableLayoutPanel tb3;
        private System.Windows.Forms.CheckBox chkGroupByDirectory;
        private System.Windows.Forms.Label lblImageQuality;
        private System.Windows.Forms.NumericUpDown numImageQuality;
        private System.Windows.Forms.Label lblAfterEditingApp;
        private System.Windows.Forms.ComboBox cmbAfterEditingApp;
        private System.Windows.Forms.Button btnUnregisterExt;
        private System.Windows.Forms.ColumnHeader clnDescription;
    }
}
