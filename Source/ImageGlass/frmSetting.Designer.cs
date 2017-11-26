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
            this.lnkOpenFileAssoc = new System.Windows.Forms.LinkLabel();
            this.lvExtension = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblSupportedExtension = new System.Windows.Forms.Label();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.chkShowScrollbar = new System.Windows.Forms.CheckBox();
            this.lnkResetBackgroundColor = new System.Windows.Forms.LinkLabel();
            this.lblHeadOthers = new System.Windows.Forms.Label();
            this.lblHeadPortableMode = new System.Windows.Forms.Label();
            this.lblHeadStartup = new System.Windows.Forms.Label();
            this.chkPortableMode = new System.Windows.Forms.CheckBox();
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
            this.chkShowHiddenImages = new System.Windows.Forms.CheckBox();
            this.btnEditEditAllExt = new System.Windows.Forms.Button();
            this.btnEditResetExt = new System.Windows.Forms.Button();
            this.btnEditEditExt = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lvImageEditing = new System.Windows.Forms.ListView();
            this.clnFileExtension = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnAppName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnAppPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnAppArguments = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblHeadImageEditing = new System.Windows.Forms.Label();
            this.lblHeadZooming = new System.Windows.Forms.Label();
            this.lblHeadSlideshow = new System.Windows.Forms.Label();
            this.lblHeadThumbnailBar = new System.Windows.Forms.Label();
            this.lblHeadImageLoading = new System.Windows.Forms.Label();
            this.chkLoopViewer = new System.Windows.Forms.CheckBox();
            this.chkMouseNavigation = new System.Windows.Forms.CheckBox();
            this.lblGeneral_ZoomOptimization = new System.Windows.Forms.Label();
            this.cmbZoomOptimization = new System.Windows.Forms.ComboBox();
            this.chkThumbnailVertical = new System.Windows.Forms.CheckBox();
            this.lblGeneral_ThumbnailSize = new System.Windows.Forms.Label();
            this.cmbThumbnailDimension = new System.Windows.Forms.ComboBox();
            this.chkImageBoosterBack = new System.Windows.Forms.CheckBox();
            this.chkLoopSlideshow = new System.Windows.Forms.CheckBox();
            this.numMaxThumbSize = new System.Windows.Forms.NumericUpDown();
            this.lblGeneral_MaxFileSize = new System.Windows.Forms.Label();
            this.lblImageLoadingOrder = new System.Windows.Forms.Label();
            this.cmbImageOrder = new System.Windows.Forms.ComboBox();
            this.barInterval = new System.Windows.Forms.TrackBar();
            this.lblSlideshowInterval = new System.Windows.Forms.Label();
            this.chkFindChildFolder = new System.Windows.Forms.CheckBox();
            this.imglOpenWith = new System.Windows.Forms.ImageList(this.components);
            this.sp1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblImage = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.tblayout = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.chkSaveOnRotate = new System.Windows.Forms.CheckBox();
            this.lblSelectAppForEdit = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundColor)).BeginInit();
            this.tabLanguage.SuspendLayout();
            this.tabFileAssociation.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tab1.SuspendLayout();
            this.tabImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThumbSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barInterval)).BeginInit();
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
            this.lblLanguage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLanguage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLanguage.ForeColor = System.Drawing.Color.Black;
            this.lblLanguage.Location = new System.Drawing.Point(0, 99);
            this.lblLanguage.Margin = new System.Windows.Forms.Padding(0);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblLanguage.Size = new System.Drawing.Size(153, 33);
            this.lblLanguage.TabIndex = 4;
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
            this.lblFileAssociations.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblFileAssociations.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFileAssociations.ForeColor = System.Drawing.Color.Black;
            this.lblFileAssociations.Location = new System.Drawing.Point(0, 66);
            this.lblFileAssociations.Margin = new System.Windows.Forms.Padding(0);
            this.lblFileAssociations.Name = "lblFileAssociations";
            this.lblFileAssociations.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblFileAssociations.Size = new System.Drawing.Size(153, 33);
            this.lblFileAssociations.TabIndex = 3;
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
            this.lblGeneral.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblGeneral.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGeneral.ForeColor = System.Drawing.Color.Black;
            this.lblGeneral.Location = new System.Drawing.Point(0, 0);
            this.lblGeneral.Margin = new System.Windows.Forms.Padding(0);
            this.lblGeneral.Name = "lblGeneral";
            this.lblGeneral.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblGeneral.Size = new System.Drawing.Size(153, 33);
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
            this.picBackgroundColor.Location = new System.Drawing.Point(31, 335);
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
            this.tabLanguage.Size = new System.Drawing.Size(463, 368);
            this.tabLanguage.TabIndex = 2;
            this.tabLanguage.Text = "language";
            // 
            // lblLanguageWarning
            // 
            this.lblLanguageWarning.AutoSize = true;
            this.lblLanguageWarning.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLanguageWarning.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(160)))), ((int)(((byte)(31)))));
            this.lblLanguageWarning.Location = new System.Drawing.Point(7, 63);
            this.lblLanguageWarning.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblLanguageWarning.Name = "lblLanguageWarning";
            this.lblLanguageWarning.Size = new System.Drawing.Size(368, 15);
            this.lblLanguageWarning.TabIndex = 25;
            this.lblLanguageWarning.Text = "This language pack may be not compatible with ImageGlass 3.2.0.16.";
            // 
            // lnkInstallLanguage
            // 
            this.lnkInstallLanguage.AutoSize = true;
            this.lnkInstallLanguage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkInstallLanguage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkInstallLanguage.Location = new System.Drawing.Point(8, 124);
            this.lnkInstallLanguage.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkInstallLanguage.Name = "lnkInstallLanguage";
            this.lnkInstallLanguage.Size = new System.Drawing.Size(206, 15);
            this.lnkInstallLanguage.TabIndex = 35;
            this.lnkInstallLanguage.TabStop = true;
            this.lnkInstallLanguage.Text = "> Install new language pack (*.iglang)";
            this.lnkInstallLanguage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkInstallLanguage_LinkClicked);
            // 
            // lnkRefresh
            // 
            this.lnkRefresh.AutoSize = true;
            this.lnkRefresh.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkRefresh.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkRefresh.Location = new System.Drawing.Point(195, 39);
            this.lnkRefresh.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkRefresh.Name = "lnkRefresh";
            this.lnkRefresh.Size = new System.Drawing.Size(57, 15);
            this.lnkRefresh.TabIndex = 34;
            this.lnkRefresh.TabStop = true;
            this.lnkRefresh.Text = "> Refresh";
            this.lnkRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRefresh_LinkClicked);
            // 
            // lnkEdit
            // 
            this.lnkEdit.AutoSize = true;
            this.lnkEdit.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkEdit.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkEdit.Location = new System.Drawing.Point(8, 164);
            this.lnkEdit.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkEdit.Name = "lnkEdit";
            this.lnkEdit.Size = new System.Drawing.Size(164, 15);
            this.lnkEdit.TabIndex = 37;
            this.lnkEdit.TabStop = true;
            this.lnkEdit.Text = "> Edit selected language pack";
            this.lnkEdit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkEdit_LinkClicked);
            // 
            // lnkCreateNew
            // 
            this.lnkCreateNew.AutoSize = true;
            this.lnkCreateNew.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkCreateNew.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkCreateNew.Location = new System.Drawing.Point(8, 144);
            this.lnkCreateNew.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkCreateNew.Name = "lnkCreateNew";
            this.lnkCreateNew.Size = new System.Drawing.Size(157, 15);
            this.lnkCreateNew.TabIndex = 36;
            this.lnkCreateNew.TabStop = true;
            this.lnkCreateNew.Text = "> Create new language pack";
            this.lnkCreateNew.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCreateNew_LinkClicked);
            // 
            // lnkGetMoreLanguage
            // 
            this.lnkGetMoreLanguage.AutoSize = true;
            this.lnkGetMoreLanguage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkGetMoreLanguage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkGetMoreLanguage.Location = new System.Drawing.Point(8, 184);
            this.lnkGetMoreLanguage.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lnkGetMoreLanguage.Name = "lnkGetMoreLanguage";
            this.lnkGetMoreLanguage.Size = new System.Drawing.Size(152, 15);
            this.lnkGetMoreLanguage.TabIndex = 38;
            this.lnkGetMoreLanguage.TabStop = true;
            this.lnkGetMoreLanguage.Text = "> Get more language packs";
            this.lnkGetMoreLanguage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGetMoreLanguage_LinkClicked);
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            "English (default)",
            "Vietnamese"});
            this.cmbLanguage.Location = new System.Drawing.Point(13, 39);
            this.cmbLanguage.Margin = new System.Windows.Forms.Padding(1);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(171, 23);
            this.cmbLanguage.TabIndex = 33;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // lblLanguageText
            // 
            this.lblLanguageText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLanguageText.AutoSize = true;
            this.lblLanguageText.Location = new System.Drawing.Point(7, 19);
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
            this.tabFileAssociation.Controls.Add(this.lnkOpenFileAssoc);
            this.tabFileAssociation.Controls.Add(this.lvExtension);
            this.tabFileAssociation.Controls.Add(this.lblSupportedExtension);
            this.tabFileAssociation.Location = new System.Drawing.Point(4, 27);
            this.tabFileAssociation.Margin = new System.Windows.Forms.Padding(0);
            this.tabFileAssociation.Name = "tabFileAssociation";
            this.tabFileAssociation.Size = new System.Drawing.Size(463, 368);
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
            this.panel2.Location = new System.Drawing.Point(0, 314);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(463, 54);
            this.panel2.TabIndex = 35;
            // 
            // btnRegisterExt
            // 
            this.btnRegisterExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRegisterExt.AutoSize = true;
            this.btnRegisterExt.Location = new System.Drawing.Point(159, 7);
            this.btnRegisterExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnRegisterExt.Name = "btnRegisterExt";
            this.btnRegisterExt.Size = new System.Drawing.Size(159, 25);
            this.btnRegisterExt.TabIndex = 31;
            this.btnRegisterExt.Text = "Register extensions";
            this.btnRegisterExt.UseVisualStyleBackColor = true;
            this.btnRegisterExt.Click += new System.EventHandler(this.btnRegisterExt_Click);
            // 
            // btnResetExt
            // 
            this.btnResetExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetExt.AutoSize = true;
            this.btnResetExt.Location = new System.Drawing.Point(322, 7);
            this.btnResetExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnResetExt.Name = "btnResetExt";
            this.btnResetExt.Size = new System.Drawing.Size(133, 25);
            this.btnResetExt.TabIndex = 32;
            this.btnResetExt.Text = "Reset to default";
            this.btnResetExt.UseVisualStyleBackColor = true;
            this.btnResetExt.Click += new System.EventHandler(this.btnResetExt_Click);
            // 
            // btnAddNewExt
            // 
            this.btnAddNewExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddNewExt.AutoSize = true;
            this.btnAddNewExt.Location = new System.Drawing.Point(14, 7);
            this.btnAddNewExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddNewExt.Name = "btnAddNewExt";
            this.btnAddNewExt.Size = new System.Drawing.Size(50, 25);
            this.btnAddNewExt.TabIndex = 29;
            this.btnAddNewExt.Text = "Add";
            this.btnAddNewExt.UseVisualStyleBackColor = true;
            this.btnAddNewExt.Click += new System.EventHandler(this.btnAddNewExt_Click);
            // 
            // btnDeleteExt
            // 
            this.btnDeleteExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteExt.AutoSize = true;
            this.btnDeleteExt.Enabled = false;
            this.btnDeleteExt.Location = new System.Drawing.Point(68, 7);
            this.btnDeleteExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnDeleteExt.Name = "btnDeleteExt";
            this.btnDeleteExt.Size = new System.Drawing.Size(66, 25);
            this.btnDeleteExt.TabIndex = 30;
            this.btnDeleteExt.Text = "Delete";
            this.btnDeleteExt.UseVisualStyleBackColor = true;
            this.btnDeleteExt.Click += new System.EventHandler(this.btnDeleteExt_Click);
            // 
            // lblExtensionsGroupDescription
            // 
            this.lblExtensionsGroupDescription.AutoSize = true;
            this.lblExtensionsGroupDescription.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExtensionsGroupDescription.Location = new System.Drawing.Point(11, 19);
            this.lblExtensionsGroupDescription.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblExtensionsGroupDescription.Name = "lblExtensionsGroupDescription";
            this.lblExtensionsGroupDescription.Size = new System.Drawing.Size(382, 15);
            this.lblExtensionsGroupDescription.TabIndex = 34;
            this.lblExtensionsGroupDescription.Text = "*Optional extensions will not be automatically pre-loaded into memory .";
            // 
            // lnkOpenFileAssoc
            // 
            this.lnkOpenFileAssoc.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkOpenFileAssoc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkOpenFileAssoc.AutoSize = true;
            this.lnkOpenFileAssoc.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkOpenFileAssoc.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkOpenFileAssoc.Location = new System.Drawing.Point(325, 43);
            this.lnkOpenFileAssoc.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lnkOpenFileAssoc.Name = "lnkOpenFileAssoc";
            this.lnkOpenFileAssoc.Size = new System.Drawing.Size(126, 15);
            this.lnkOpenFileAssoc.TabIndex = 27;
            this.lnkOpenFileAssoc.TabStop = true;
            this.lnkOpenFileAssoc.Text = "Open File Associations";
            this.lnkOpenFileAssoc.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkOpenFileAssoc_LinkClicked);
            // 
            // lvExtension
            // 
            this.lvExtension.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvExtension.CheckBoxes = true;
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
            this.lvExtension.Location = new System.Drawing.Point(15, 60);
            this.lvExtension.Margin = new System.Windows.Forms.Padding(2);
            this.lvExtension.Name = "lvExtension";
            this.lvExtension.Size = new System.Drawing.Size(439, 249);
            this.lvExtension.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvExtension.TabIndex = 28;
            this.lvExtension.UseCompatibleStateImageBehavior = false;
            this.lvExtension.View = System.Windows.Forms.View.Details;
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
            this.lblSupportedExtension.Location = new System.Drawing.Point(11, 43);
            this.lblSupportedExtension.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblSupportedExtension.Name = "lblSupportedExtension";
            this.lblSupportedExtension.Size = new System.Drawing.Size(123, 15);
            this.lblSupportedExtension.TabIndex = 21;
            this.lblSupportedExtension.Text = "Supported extensions:";
            // 
            // tabGeneral
            // 
            this.tabGeneral.AutoScroll = true;
            this.tabGeneral.BackColor = System.Drawing.Color.White;
            this.tabGeneral.Controls.Add(this.chkShowScrollbar);
            this.tabGeneral.Controls.Add(this.lnkResetBackgroundColor);
            this.tabGeneral.Controls.Add(this.lblHeadOthers);
            this.tabGeneral.Controls.Add(this.lblHeadPortableMode);
            this.tabGeneral.Controls.Add(this.lblHeadStartup);
            this.tabGeneral.Controls.Add(this.chkPortableMode);
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
            this.tabGeneral.Size = new System.Drawing.Size(463, 368);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "general";
            // 
            // chkShowScrollbar
            // 
            this.chkShowScrollbar.AutoSize = true;
            this.chkShowScrollbar.Location = new System.Drawing.Point(30, 285);
            this.chkShowScrollbar.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowScrollbar.Name = "chkShowScrollbar";
            this.chkShowScrollbar.Size = new System.Drawing.Size(154, 19);
            this.chkShowScrollbar.TabIndex = 12;
            this.chkShowScrollbar.Text = "Display viewer scrollbars";
            this.chkShowScrollbar.UseVisualStyleBackColor = true;
            // 
            // lnkResetBackgroundColor
            // 
            this.lnkResetBackgroundColor.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkResetBackgroundColor.AutoSize = true;
            this.lnkResetBackgroundColor.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkResetBackgroundColor.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkResetBackgroundColor.Location = new System.Drawing.Point(101, 339);
            this.lnkResetBackgroundColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lnkResetBackgroundColor.Name = "lnkResetBackgroundColor";
            this.lnkResetBackgroundColor.Size = new System.Drawing.Size(35, 15);
            this.lnkResetBackgroundColor.TabIndex = 13;
            this.lnkResetBackgroundColor.TabStop = true;
            this.lnkResetBackgroundColor.Text = "Reset";
            this.lnkResetBackgroundColor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkResetBackgroundColor_LinkClicked);
            // 
            // lblHeadOthers
            // 
            this.lblHeadOthers.AutoSize = true;
            this.lblHeadOthers.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadOthers.Location = new System.Drawing.Point(15, 175);
            this.lblHeadOthers.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeadOthers.Name = "lblHeadOthers";
            this.lblHeadOthers.Size = new System.Drawing.Size(45, 15);
            this.lblHeadOthers.TabIndex = 46;
            this.lblHeadOthers.Text = "Others";
            // 
            // lblHeadPortableMode
            // 
            this.lblHeadPortableMode.AutoSize = true;
            this.lblHeadPortableMode.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadPortableMode.Location = new System.Drawing.Point(15, 109);
            this.lblHeadPortableMode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeadPortableMode.Name = "lblHeadPortableMode";
            this.lblHeadPortableMode.Size = new System.Drawing.Size(89, 15);
            this.lblHeadPortableMode.TabIndex = 45;
            this.lblHeadPortableMode.Text = "Portable mode";
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
            // chkPortableMode
            // 
            this.chkPortableMode.AutoSize = true;
            this.chkPortableMode.Enabled = false;
            this.chkPortableMode.Location = new System.Drawing.Point(30, 131);
            this.chkPortableMode.Margin = new System.Windows.Forms.Padding(1);
            this.chkPortableMode.Name = "chkPortableMode";
            this.chkPortableMode.Size = new System.Drawing.Size(142, 19);
            this.chkPortableMode.TabIndex = 7;
            this.chkPortableMode.Text = "Enable Portable mode";
            this.chkPortableMode.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(29, 387);
            this.panel1.Margin = new System.Windows.Forms.Padding(1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(73, 21);
            this.panel1.TabIndex = 24;
            // 
            // chkConfirmationDelete
            // 
            this.chkConfirmationDelete.AutoSize = true;
            this.chkConfirmationDelete.Location = new System.Drawing.Point(30, 263);
            this.chkConfirmationDelete.Margin = new System.Windows.Forms.Padding(1);
            this.chkConfirmationDelete.Name = "chkConfirmationDelete";
            this.chkConfirmationDelete.Size = new System.Drawing.Size(211, 19);
            this.chkConfirmationDelete.TabIndex = 11;
            this.chkConfirmationDelete.Text = "Display Delete confirmation dialog ";
            this.chkConfirmationDelete.UseVisualStyleBackColor = true;
            // 
            // chkAllowMultiInstances
            // 
            this.chkAllowMultiInstances.AutoSize = true;
            this.chkAllowMultiInstances.Location = new System.Drawing.Point(30, 219);
            this.chkAllowMultiInstances.Margin = new System.Windows.Forms.Padding(1);
            this.chkAllowMultiInstances.Name = "chkAllowMultiInstances";
            this.chkAllowMultiInstances.Size = new System.Drawing.Size(238, 19);
            this.chkAllowMultiInstances.TabIndex = 9;
            this.chkAllowMultiInstances.Text = "Allow multiple instances of the program";
            this.chkAllowMultiInstances.UseVisualStyleBackColor = true;
            // 
            // chkESCToQuit
            // 
            this.chkESCToQuit.AutoSize = true;
            this.chkESCToQuit.Location = new System.Drawing.Point(30, 241);
            this.chkESCToQuit.Margin = new System.Windows.Forms.Padding(1);
            this.chkESCToQuit.Name = "chkESCToQuit";
            this.chkESCToQuit.Size = new System.Drawing.Size(223, 19);
            this.chkESCToQuit.TabIndex = 10;
            this.chkESCToQuit.Text = "Allow to press ESC to quit application";
            this.chkESCToQuit.UseVisualStyleBackColor = true;
            // 
            // chkShowToolBar
            // 
            this.chkShowToolBar.AutoSize = true;
            this.chkShowToolBar.Location = new System.Drawing.Point(30, 57);
            this.chkShowToolBar.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowToolBar.Name = "chkShowToolBar";
            this.chkShowToolBar.Size = new System.Drawing.Size(188, 19);
            this.chkShowToolBar.TabIndex = 6;
            this.chkShowToolBar.Text = "Show toolbar when starting up";
            this.chkShowToolBar.UseVisualStyleBackColor = true;
            // 
            // lblBackGroundColor
            // 
            this.lblBackGroundColor.AutoSize = true;
            this.lblBackGroundColor.Location = new System.Drawing.Point(27, 317);
            this.lblBackGroundColor.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblBackGroundColor.Name = "lblBackGroundColor";
            this.lblBackGroundColor.Size = new System.Drawing.Size(104, 15);
            this.lblBackGroundColor.TabIndex = 11;
            this.lblBackGroundColor.Text = "Background color:";
            // 
            // chkWelcomePicture
            // 
            this.chkWelcomePicture.AutoSize = true;
            this.chkWelcomePicture.Location = new System.Drawing.Point(30, 35);
            this.chkWelcomePicture.Margin = new System.Windows.Forms.Padding(1);
            this.chkWelcomePicture.Name = "chkWelcomePicture";
            this.chkWelcomePicture.Size = new System.Drawing.Size(146, 19);
            this.chkWelcomePicture.TabIndex = 5;
            this.chkWelcomePicture.Text = "Show welcome picture";
            this.chkWelcomePicture.UseVisualStyleBackColor = true;
            // 
            // chkAutoUpdate
            // 
            this.chkAutoUpdate.AutoSize = true;
            this.chkAutoUpdate.Location = new System.Drawing.Point(30, 197);
            this.chkAutoUpdate.Margin = new System.Windows.Forms.Padding(1);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(192, 19);
            this.chkAutoUpdate.TabIndex = 8;
            this.chkAutoUpdate.Text = "Check for update automatically";
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // tab1
            // 
            this.tab1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tab1.Controls.Add(this.tabGeneral);
            this.tab1.Controls.Add(this.tabImage);
            this.tab1.Controls.Add(this.tabFileAssociation);
            this.tab1.Controls.Add(this.tabLanguage);
            this.tab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tab1.Location = new System.Drawing.Point(0, 0);
            this.tab1.Margin = new System.Windows.Forms.Padding(0);
            this.tab1.Name = "tab1";
            this.tab1.Padding = new System.Drawing.Point(0, 0);
            this.tab1.SelectedIndex = 0;
            this.tab1.Size = new System.Drawing.Size(471, 399);
            this.tab1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tab1.TabIndex = 0;
            this.tab1.SelectedIndexChanged += new System.EventHandler(this.tab1_SelectedIndexChanged);
            // 
            // tabImage
            // 
            this.tabImage.AutoScroll = true;
            this.tabImage.BackColor = System.Drawing.Color.White;
            this.tabImage.Controls.Add(this.lblSelectAppForEdit);
            this.tabImage.Controls.Add(this.chkSaveOnRotate);
            this.tabImage.Controls.Add(this.chkShowHiddenImages);
            this.tabImage.Controls.Add(this.btnEditEditAllExt);
            this.tabImage.Controls.Add(this.btnEditResetExt);
            this.tabImage.Controls.Add(this.btnEditEditExt);
            this.tabImage.Controls.Add(this.panel3);
            this.tabImage.Controls.Add(this.lvImageEditing);
            this.tabImage.Controls.Add(this.lblHeadImageEditing);
            this.tabImage.Controls.Add(this.lblHeadZooming);
            this.tabImage.Controls.Add(this.lblHeadSlideshow);
            this.tabImage.Controls.Add(this.lblHeadThumbnailBar);
            this.tabImage.Controls.Add(this.lblHeadImageLoading);
            this.tabImage.Controls.Add(this.chkLoopViewer);
            this.tabImage.Controls.Add(this.chkMouseNavigation);
            this.tabImage.Controls.Add(this.lblGeneral_ZoomOptimization);
            this.tabImage.Controls.Add(this.cmbZoomOptimization);
            this.tabImage.Controls.Add(this.chkThumbnailVertical);
            this.tabImage.Controls.Add(this.lblGeneral_ThumbnailSize);
            this.tabImage.Controls.Add(this.cmbThumbnailDimension);
            this.tabImage.Controls.Add(this.chkImageBoosterBack);
            this.tabImage.Controls.Add(this.chkLoopSlideshow);
            this.tabImage.Controls.Add(this.numMaxThumbSize);
            this.tabImage.Controls.Add(this.lblGeneral_MaxFileSize);
            this.tabImage.Controls.Add(this.lblImageLoadingOrder);
            this.tabImage.Controls.Add(this.cmbImageOrder);
            this.tabImage.Controls.Add(this.barInterval);
            this.tabImage.Controls.Add(this.lblSlideshowInterval);
            this.tabImage.Controls.Add(this.chkFindChildFolder);
            this.tabImage.Location = new System.Drawing.Point(4, 27);
            this.tabImage.Margin = new System.Windows.Forms.Padding(2);
            this.tabImage.Name = "tabImage";
            this.tabImage.Padding = new System.Windows.Forms.Padding(2);
            this.tabImage.Size = new System.Drawing.Size(463, 368);
            this.tabImage.TabIndex = 3;
            this.tabImage.Text = "Image";
            // 
            // chkShowHiddenImages
            // 
            this.chkShowHiddenImages.AutoSize = true;
            this.chkShowHiddenImages.Location = new System.Drawing.Point(30, 57);
            this.chkShowHiddenImages.Margin = new System.Windows.Forms.Padding(1);
            this.chkShowHiddenImages.Name = "chkShowHiddenImages";
            this.chkShowHiddenImages.Size = new System.Drawing.Size(136, 19);
            this.chkShowHiddenImages.TabIndex = 48;
            this.chkShowHiddenImages.Text = "Show hidden images";
            this.chkShowHiddenImages.UseVisualStyleBackColor = true;
            // 
            // btnEditEditAllExt
            // 
            this.btnEditEditAllExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditEditAllExt.AutoSize = true;
            this.btnEditEditAllExt.Location = new System.Drawing.Point(289, 858);
            this.btnEditEditAllExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnEditEditAllExt.Name = "btnEditEditAllExt";
            this.btnEditEditAllExt.Size = new System.Drawing.Size(146, 25);
            this.btnEditEditAllExt.TabIndex = 47;
            this.btnEditEditAllExt.Text = "Edit all extensions";
            this.btnEditEditAllExt.UseVisualStyleBackColor = true;
            this.btnEditEditAllExt.Click += new System.EventHandler(this.btnEditEditAllExt_Click);
            // 
            // btnEditResetExt
            // 
            this.btnEditResetExt.AutoSize = true;
            this.btnEditResetExt.Location = new System.Drawing.Point(84, 858);
            this.btnEditResetExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnEditResetExt.Name = "btnEditResetExt";
            this.btnEditResetExt.Size = new System.Drawing.Size(133, 25);
            this.btnEditResetExt.TabIndex = 26;
            this.btnEditResetExt.Text = "Reset to default";
            this.btnEditResetExt.UseVisualStyleBackColor = true;
            this.btnEditResetExt.Click += new System.EventHandler(this.btnEditResetExt_Click);
            // 
            // btnEditEditExt
            // 
            this.btnEditEditExt.AutoSize = true;
            this.btnEditEditExt.Enabled = false;
            this.btnEditEditExt.Location = new System.Drawing.Point(30, 858);
            this.btnEditEditExt.Margin = new System.Windows.Forms.Padding(2);
            this.btnEditEditExt.Name = "btnEditEditExt";
            this.btnEditEditExt.Size = new System.Drawing.Size(50, 25);
            this.btnEditEditExt.TabIndex = 25;
            this.btnEditEditExt.Text = "Edit";
            this.btnEditEditExt.UseVisualStyleBackColor = true;
            this.btnEditEditExt.Click += new System.EventHandler(this.btnEditEditExt_Click);
            // 
            // panel3
            // 
            this.panel3.Location = new System.Drawing.Point(18, 869);
            this.panel3.Margin = new System.Windows.Forms.Padding(1);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(73, 21);
            this.panel3.TabIndex = 46;
            // 
            // lvImageEditing
            // 
            this.lvImageEditing.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvImageEditing.CheckBoxes = true;
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
            this.lvImageEditing.Location = new System.Drawing.Point(30, 705);
            this.lvImageEditing.Margin = new System.Windows.Forms.Padding(2);
            this.lvImageEditing.MultiSelect = false;
            this.lvImageEditing.Name = "lvImageEditing";
            this.lvImageEditing.RightToLeftLayout = true;
            this.lvImageEditing.ShowItemToolTips = true;
            this.lvImageEditing.Size = new System.Drawing.Size(405, 144);
            this.lvImageEditing.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvImageEditing.TabIndex = 24;
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
            // lblHeadImageEditing
            // 
            this.lblHeadImageEditing.AutoSize = true;
            this.lblHeadImageEditing.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadImageEditing.Location = new System.Drawing.Point(15, 636);
            this.lblHeadImageEditing.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeadImageEditing.Name = "lblHeadImageEditing";
            this.lblHeadImageEditing.Size = new System.Drawing.Size(84, 15);
            this.lblHeadImageEditing.TabIndex = 44;
            this.lblHeadImageEditing.Text = "Image editing";
            // 
            // lblHeadZooming
            // 
            this.lblHeadZooming.AutoSize = true;
            this.lblHeadZooming.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadZooming.Location = new System.Drawing.Point(15, 202);
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
            this.lblHeadSlideshow.Location = new System.Drawing.Point(15, 514);
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
            this.lblHeadThumbnailBar.Location = new System.Drawing.Point(15, 330);
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
            this.lblHeadImageLoading.Location = new System.Drawing.Point(15, 13);
            this.lblHeadImageLoading.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeadImageLoading.Name = "lblHeadImageLoading";
            this.lblHeadImageLoading.Size = new System.Drawing.Size(85, 15);
            this.lblHeadImageLoading.TabIndex = 40;
            this.lblHeadImageLoading.Text = "Image loading";
            // 
            // chkLoopViewer
            // 
            this.chkLoopViewer.AutoSize = true;
            this.chkLoopViewer.Location = new System.Drawing.Point(30, 78);
            this.chkLoopViewer.Margin = new System.Windows.Forms.Padding(1);
            this.chkLoopViewer.Name = "chkLoopViewer";
            this.chkLoopViewer.Size = new System.Drawing.Size(387, 19);
            this.chkLoopViewer.TabIndex = 14;
            this.chkLoopViewer.Text = "Loop back viewer to the first image when reaching the end of the list";
            this.chkLoopViewer.UseVisualStyleBackColor = true;
            // 
            // chkMouseNavigation
            // 
            this.chkMouseNavigation.AutoSize = true;
            this.chkMouseNavigation.Location = new System.Drawing.Point(30, 224);
            this.chkMouseNavigation.Margin = new System.Windows.Forms.Padding(1);
            this.chkMouseNavigation.Name = "chkMouseNavigation";
            this.chkMouseNavigation.Size = new System.Drawing.Size(366, 19);
            this.chkMouseNavigation.TabIndex = 17;
            this.chkMouseNavigation.Text = "Use the mouse wheel to browse images, hold CTRL for zooming.";
            this.chkMouseNavigation.UseVisualStyleBackColor = true;
            // 
            // lblGeneral_ZoomOptimization
            // 
            this.lblGeneral_ZoomOptimization.AutoSize = true;
            this.lblGeneral_ZoomOptimization.Location = new System.Drawing.Point(27, 257);
            this.lblGeneral_ZoomOptimization.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblGeneral_ZoomOptimization.Name = "lblGeneral_ZoomOptimization";
            this.lblGeneral_ZoomOptimization.Size = new System.Drawing.Size(109, 15);
            this.lblGeneral_ZoomOptimization.TabIndex = 36;
            this.lblGeneral_ZoomOptimization.Text = "Zoom optimization";
            // 
            // cmbZoomOptimization
            // 
            this.cmbZoomOptimization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbZoomOptimization.FormattingEnabled = true;
            this.cmbZoomOptimization.Items.AddRange(new object[] {
            "(loaded from code)"});
            this.cmbZoomOptimization.Location = new System.Drawing.Point(30, 275);
            this.cmbZoomOptimization.Margin = new System.Windows.Forms.Padding(1);
            this.cmbZoomOptimization.Name = "cmbZoomOptimization";
            this.cmbZoomOptimization.Size = new System.Drawing.Size(187, 23);
            this.cmbZoomOptimization.TabIndex = 18;
            // 
            // chkThumbnailVertical
            // 
            this.chkThumbnailVertical.AutoSize = true;
            this.chkThumbnailVertical.Location = new System.Drawing.Point(30, 352);
            this.chkThumbnailVertical.Margin = new System.Windows.Forms.Padding(1);
            this.chkThumbnailVertical.Name = "chkThumbnailVertical";
            this.chkThumbnailVertical.Size = new System.Drawing.Size(173, 19);
            this.chkThumbnailVertical.TabIndex = 19;
            this.chkThumbnailVertical.Text = "Thumbnail bar on right side";
            this.chkThumbnailVertical.UseVisualStyleBackColor = true;
            // 
            // lblGeneral_ThumbnailSize
            // 
            this.lblGeneral_ThumbnailSize.AutoSize = true;
            this.lblGeneral_ThumbnailSize.Location = new System.Drawing.Point(27, 440);
            this.lblGeneral_ThumbnailSize.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblGeneral_ThumbnailSize.Name = "lblGeneral_ThumbnailSize";
            this.lblGeneral_ThumbnailSize.Size = new System.Drawing.Size(175, 15);
            this.lblGeneral_ThumbnailSize.TabIndex = 33;
            this.lblGeneral_ThumbnailSize.Text = "Thumbnail dimension (in pixel):";
            // 
            // cmbThumbnailDimension
            // 
            this.cmbThumbnailDimension.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            this.cmbThumbnailDimension.Location = new System.Drawing.Point(30, 458);
            this.cmbThumbnailDimension.Margin = new System.Windows.Forms.Padding(1);
            this.cmbThumbnailDimension.Name = "cmbThumbnailDimension";
            this.cmbThumbnailDimension.Size = new System.Drawing.Size(187, 23);
            this.cmbThumbnailDimension.TabIndex = 21;
            // 
            // chkImageBoosterBack
            // 
            this.chkImageBoosterBack.AutoSize = true;
            this.chkImageBoosterBack.Location = new System.Drawing.Point(30, 100);
            this.chkImageBoosterBack.Margin = new System.Windows.Forms.Padding(1);
            this.chkImageBoosterBack.Name = "chkImageBoosterBack";
            this.chkImageBoosterBack.Size = new System.Drawing.Size(385, 19);
            this.chkImageBoosterBack.TabIndex = 15;
            this.chkImageBoosterBack.Text = "Turn on Image Booster when navigate back (need more ~20% RAM)";
            this.chkImageBoosterBack.UseVisualStyleBackColor = true;
            // 
            // chkLoopSlideshow
            // 
            this.chkLoopSlideshow.AutoSize = true;
            this.chkLoopSlideshow.Location = new System.Drawing.Point(30, 536);
            this.chkLoopSlideshow.Margin = new System.Windows.Forms.Padding(1);
            this.chkLoopSlideshow.Name = "chkLoopSlideshow";
            this.chkLoopSlideshow.Size = new System.Drawing.Size(405, 19);
            this.chkLoopSlideshow.TabIndex = 22;
            this.chkLoopSlideshow.Text = "Loop back slideshow to the first image when reaching the end of the list";
            this.chkLoopSlideshow.UseVisualStyleBackColor = true;
            // 
            // numMaxThumbSize
            // 
            this.numMaxThumbSize.Location = new System.Drawing.Point(30, 404);
            this.numMaxThumbSize.Margin = new System.Windows.Forms.Padding(1);
            this.numMaxThumbSize.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numMaxThumbSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxThumbSize.Name = "numMaxThumbSize";
            this.numMaxThumbSize.Size = new System.Drawing.Size(53, 23);
            this.numMaxThumbSize.TabIndex = 20;
            this.numMaxThumbSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMaxThumbSize.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // lblGeneral_MaxFileSize
            // 
            this.lblGeneral_MaxFileSize.AutoSize = true;
            this.lblGeneral_MaxFileSize.Location = new System.Drawing.Point(27, 385);
            this.lblGeneral_MaxFileSize.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblGeneral_MaxFileSize.Name = "lblGeneral_MaxFileSize";
            this.lblGeneral_MaxFileSize.Size = new System.Drawing.Size(186, 15);
            this.lblGeneral_MaxFileSize.TabIndex = 31;
            this.lblGeneral_MaxFileSize.Text = "Maximum thumbnail size (in MB):";
            // 
            // lblImageLoadingOrder
            // 
            this.lblImageLoadingOrder.AutoSize = true;
            this.lblImageLoadingOrder.Location = new System.Drawing.Point(27, 131);
            this.lblImageLoadingOrder.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblImageLoadingOrder.Name = "lblImageLoadingOrder";
            this.lblImageLoadingOrder.Size = new System.Drawing.Size(117, 15);
            this.lblImageLoadingOrder.TabIndex = 28;
            this.lblImageLoadingOrder.Text = "Image loading order:";
            // 
            // cmbImageOrder
            // 
            this.cmbImageOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbImageOrder.FormattingEnabled = true;
            this.cmbImageOrder.Items.AddRange(new object[] {
            "Name (default)",
            "Length",
            "Creation time",
            "Last access time",
            "Last write time",
            "Extension",
            "Random"});
            this.cmbImageOrder.Location = new System.Drawing.Point(30, 148);
            this.cmbImageOrder.Margin = new System.Windows.Forms.Padding(1);
            this.cmbImageOrder.Name = "cmbImageOrder";
            this.cmbImageOrder.Size = new System.Drawing.Size(187, 23);
            this.cmbImageOrder.TabIndex = 16;
            // 
            // barInterval
            // 
            this.barInterval.BackColor = System.Drawing.SystemColors.Window;
            this.barInterval.Location = new System.Drawing.Point(30, 589);
            this.barInterval.Margin = new System.Windows.Forms.Padding(1);
            this.barInterval.Maximum = 60;
            this.barInterval.Minimum = 1;
            this.barInterval.Name = "barInterval";
            this.barInterval.Size = new System.Drawing.Size(195, 45);
            this.barInterval.TabIndex = 23;
            this.barInterval.TickStyle = System.Windows.Forms.TickStyle.None;
            this.barInterval.Value = 5;
            this.barInterval.Scroll += new System.EventHandler(this.barInterval_Scroll);
            // 
            // lblSlideshowInterval
            // 
            this.lblSlideshowInterval.AutoSize = true;
            this.lblSlideshowInterval.Location = new System.Drawing.Point(27, 568);
            this.lblSlideshowInterval.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblSlideshowInterval.Name = "lblSlideshowInterval";
            this.lblSlideshowInterval.Size = new System.Drawing.Size(163, 15);
            this.lblSlideshowInterval.TabIndex = 24;
            this.lblSlideshowInterval.Text = "Slide show interval: 5 seconds";
            // 
            // chkFindChildFolder
            // 
            this.chkFindChildFolder.AutoSize = true;
            this.chkFindChildFolder.Location = new System.Drawing.Point(30, 35);
            this.chkFindChildFolder.Margin = new System.Windows.Forms.Padding(1);
            this.chkFindChildFolder.Name = "chkFindChildFolder";
            this.chkFindChildFolder.Size = new System.Drawing.Size(166, 19);
            this.chkFindChildFolder.TabIndex = 13;
            this.chkFindChildFolder.Text = "Find images in child folder";
            this.chkFindChildFolder.UseVisualStyleBackColor = true;
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
            this.sp1.Size = new System.Drawing.Size(623, 399);
            this.sp1.SplitterDistance = 149;
            this.sp1.SplitterWidth = 3;
            this.sp1.TabIndex = 17;
            this.sp1.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 153F));
            this.tableLayoutPanel1.Controls.Add(this.lblGeneral, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblImage, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblFileAssociations, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblLanguage, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(149, 399);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // lblImage
            // 
            this.lblImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblImage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblImage.ForeColor = System.Drawing.Color.Black;
            this.lblImage.Location = new System.Drawing.Point(0, 33);
            this.lblImage.Margin = new System.Windows.Forms.Padding(0);
            this.lblImage.Name = "lblImage";
            this.lblImage.Padding = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblImage.Size = new System.Drawing.Size(153, 33);
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
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.AutoSize = true;
            this.btnSave.Location = new System.Drawing.Point(376, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 25);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.AutoSize = true;
            this.btnCancel.Location = new System.Drawing.Point(457, 8);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.AutoSize = true;
            this.btnApply.Location = new System.Drawing.Point(538, 8);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 25);
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
            this.tblayout.Size = new System.Drawing.Size(625, 449);
            this.tblayout.TabIndex = 19;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panel4.Controls.Add(this.btnApply);
            this.panel4.Controls.Add(this.btnSave);
            this.panel4.Controls.Add(this.btnCancel);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 401);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(625, 48);
            this.panel4.TabIndex = 18;
            // 
            // chkSaveOnRotate
            // 
            this.chkSaveOnRotate.AutoSize = true;
            this.chkSaveOnRotate.Location = new System.Drawing.Point(30, 660);
            this.chkSaveOnRotate.Margin = new System.Windows.Forms.Padding(1);
            this.chkSaveOnRotate.Name = "chkSaveOnRotate";
            this.chkSaveOnRotate.Size = new System.Drawing.Size(222, 19);
            this.chkSaveOnRotate.TabIndex = 49;
            this.chkSaveOnRotate.Text = "Save the viewing image after rotating";
            this.chkSaveOnRotate.UseVisualStyleBackColor = true;
            // 
            // lblSelectAppForEdit
            // 
            this.lblSelectAppForEdit.AutoSize = true;
            this.lblSelectAppForEdit.Location = new System.Drawing.Point(27, 688);
            this.lblSelectAppForEdit.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblSelectAppForEdit.Name = "lblSelectAppForEdit";
            this.lblSelectAppForEdit.Size = new System.Drawing.Size(197, 15);
            this.lblSelectAppForEdit.TabIndex = 50;
            this.lblSelectAppForEdit.Text = "Select application for image editing:";
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(625, 449);
            this.Controls.Add(this.tblayout);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(1);
            this.MinimumSize = new System.Drawing.Size(470, 368);
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
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThumbSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barInterval)).EndInit();
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
        private System.Windows.Forms.CheckBox chkPortableMode;
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
        private System.Windows.Forms.CheckBox chkMouseNavigation;
        private System.Windows.Forms.Label lblGeneral_ZoomOptimization;
        private System.Windows.Forms.ComboBox cmbZoomOptimization;
        private System.Windows.Forms.CheckBox chkThumbnailVertical;
        private System.Windows.Forms.Label lblGeneral_ThumbnailSize;
        private System.Windows.Forms.ComboBox cmbThumbnailDimension;
        private System.Windows.Forms.CheckBox chkImageBoosterBack;
        private System.Windows.Forms.CheckBox chkLoopSlideshow;
        private System.Windows.Forms.NumericUpDown numMaxThumbSize;
        private System.Windows.Forms.Label lblGeneral_MaxFileSize;
        private System.Windows.Forms.Label lblImageLoadingOrder;
        private System.Windows.Forms.ComboBox cmbImageOrder;
        private System.Windows.Forms.TrackBar barInterval;
        private System.Windows.Forms.Label lblSlideshowInterval;
        private System.Windows.Forms.CheckBox chkFindChildFolder;
        private System.Windows.Forms.Label lblHeadZooming;
        private System.Windows.Forms.Label lblHeadSlideshow;
        private System.Windows.Forms.Label lblHeadThumbnailBar;
        private System.Windows.Forms.Label lblHeadStartup;
        private System.Windows.Forms.Label lblHeadPortableMode;
        private System.Windows.Forms.Label lblHeadOthers;
        private System.Windows.Forms.Label lblHeadImageEditing;
        private System.Windows.Forms.ListView lvImageEditing;
        private System.Windows.Forms.ColumnHeader clnFileExtension;
        private System.Windows.Forms.ColumnHeader clnAppName;
        private System.Windows.Forms.ColumnHeader clnAppPath;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnEditResetExt;
        private System.Windows.Forms.Button btnEditEditExt;
        private System.Windows.Forms.ColumnHeader clnAppArguments;
        private System.Windows.Forms.LinkLabel lnkResetBackgroundColor;
        private System.Windows.Forms.Button btnEditEditAllExt;
        private System.Windows.Forms.CheckBox chkShowScrollbar;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TableLayoutPanel tblayout;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chkShowHiddenImages;
        private System.Windows.Forms.Label lblSelectAppForEdit;
        private System.Windows.Forms.CheckBox chkSaveOnRotate;
    }
}