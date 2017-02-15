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
            this.panExtraExts = new System.Windows.Forms.Panel();
            this.chkExtraExtsPSD = new System.Windows.Forms.CheckBox();
            this.chkExtraExtsHDR = new System.Windows.Forms.CheckBox();
            this.chkExtraExtsEXR = new System.Windows.Forms.CheckBox();
            this.chkExtraExtsTGA = new System.Windows.Forms.CheckBox();
            this.txtSupportedExtensionDefault = new System.Windows.Forms.TextBox();
            this.lblSupportedExtension = new System.Windows.Forms.Label();
            this.btnOpenFileAssociations = new System.Windows.Forms.Button();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkLoopViewer = new System.Windows.Forms.CheckBox();
            this.chkConfirmationDelete = new System.Windows.Forms.CheckBox();
            this.chkMouseNavigation = new System.Windows.Forms.CheckBox();
            this.lblGeneral_ZoomOptimization = new System.Windows.Forms.Label();
            this.cmbZoomOptimization = new System.Windows.Forms.ComboBox();
            this.chkThumbnailVertical = new System.Windows.Forms.CheckBox();
            this.chkAllowMultiInstances = new System.Windows.Forms.CheckBox();
            this.lblGeneral_ThumbnailSize = new System.Windows.Forms.Label();
            this.cmbThumbnailDimension = new System.Windows.Forms.ComboBox();
            this.chkESCToQuit = new System.Windows.Forms.CheckBox();
            this.chkImageBoosterBack = new System.Windows.Forms.CheckBox();
            this.chkLoopSlideshow = new System.Windows.Forms.CheckBox();
            this.numMaxThumbSize = new System.Windows.Forms.NumericUpDown();
            this.lblGeneral_MaxFileSize = new System.Windows.Forms.Label();
            this.chkHideToolBar = new System.Windows.Forms.CheckBox();
            this.lblBackGroundColor = new System.Windows.Forms.Label();
            this.chkWelcomePicture = new System.Windows.Forms.CheckBox();
            this.lblImageLoadingOrder = new System.Windows.Forms.Label();
            this.cmbImageOrder = new System.Windows.Forms.ComboBox();
            this.barInterval = new System.Windows.Forms.TrackBar();
            this.lblSlideshowInterval = new System.Windows.Forms.Label();
            this.chkFindChildFolder = new System.Windows.Forms.CheckBox();
            this.chkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.tab1 = new System.Windows.Forms.TabControl();
            this.imglOpenWith = new System.Windows.Forms.ImageList(this.components);
            this.sp0 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundColor)).BeginInit();
            this.tabLanguage.SuspendLayout();
            this.tabFileAssociation.SuspendLayout();
            this.panExtraExts.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThumbSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barInterval)).BeginInit();
            this.tab1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sp0)).BeginInit();
            this.sp0.Panel1.SuspendLayout();
            this.sp0.Panel2.SuspendLayout();
            this.sp0.SuspendLayout();
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
            this.lblLanguage.AutoEllipsis = true;
            this.lblLanguage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblLanguage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLanguage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLanguage.ForeColor = System.Drawing.Color.Black;
            this.lblLanguage.Location = new System.Drawing.Point(0, 100);
            this.lblLanguage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Padding = new System.Windows.Forms.Padding(15, 0, 15, 0);
            this.lblLanguage.Size = new System.Drawing.Size(180, 50);
            this.lblLanguage.TabIndex = 3;
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
            this.lblFileAssociations.AutoEllipsis = true;
            this.lblFileAssociations.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblFileAssociations.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblFileAssociations.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFileAssociations.ForeColor = System.Drawing.Color.Black;
            this.lblFileAssociations.Location = new System.Drawing.Point(0, 50);
            this.lblFileAssociations.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileAssociations.Name = "lblFileAssociations";
            this.lblFileAssociations.Padding = new System.Windows.Forms.Padding(15, 0, 15, 0);
            this.lblFileAssociations.Size = new System.Drawing.Size(180, 50);
            this.lblFileAssociations.TabIndex = 2;
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
            this.lblGeneral.AutoEllipsis = true;
            this.lblGeneral.BackColor = System.Drawing.Color.Gainsboro;
            this.lblGeneral.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblGeneral.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGeneral.ForeColor = System.Drawing.Color.Black;
            this.lblGeneral.Location = new System.Drawing.Point(0, 0);
            this.lblGeneral.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblGeneral.Name = "lblGeneral";
            this.lblGeneral.Padding = new System.Windows.Forms.Padding(15, 0, 15, 0);
            this.lblGeneral.Size = new System.Drawing.Size(180, 50);
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
            this.picBackgroundColor.Location = new System.Drawing.Point(20, 949);
            this.picBackgroundColor.Name = "picBackgroundColor";
            this.picBackgroundColor.Size = new System.Drawing.Size(100, 30);
            this.picBackgroundColor.TabIndex = 12;
            this.picBackgroundColor.TabStop = false;
            this.tip1.SetToolTip(this.picBackgroundColor, "Change background color");
            this.picBackgroundColor.Click += new System.EventHandler(this.picBackgroundColor_Click);
            // 
            // tabLanguage
            // 
            this.tabLanguage.Controls.Add(this.lblLanguageWarning);
            this.tabLanguage.Controls.Add(this.lnkInstallLanguage);
            this.tabLanguage.Controls.Add(this.lnkRefresh);
            this.tabLanguage.Controls.Add(this.lnkEdit);
            this.tabLanguage.Controls.Add(this.lnkCreateNew);
            this.tabLanguage.Controls.Add(this.lnkGetMoreLanguage);
            this.tabLanguage.Controls.Add(this.cmbLanguage);
            this.tabLanguage.Controls.Add(this.lblLanguageText);
            this.tabLanguage.Location = new System.Drawing.Point(4, 4);
            this.tabLanguage.Name = "tabLanguage";
            this.tabLanguage.Padding = new System.Windows.Forms.Padding(3);
            this.tabLanguage.Size = new System.Drawing.Size(524, 564);
            this.tabLanguage.TabIndex = 2;
            this.tabLanguage.Text = "language";
            this.tabLanguage.UseVisualStyleBackColor = true;
            // 
            // lblLanguageWarning
            // 
            this.lblLanguageWarning.AutoSize = true;
            this.lblLanguageWarning.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLanguageWarning.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(160)))), ((int)(((byte)(31)))));
            this.lblLanguageWarning.Location = new System.Drawing.Point(20, 94);
            this.lblLanguageWarning.Name = "lblLanguageWarning";
            this.lblLanguageWarning.Size = new System.Drawing.Size(556, 25);
            this.lblLanguageWarning.TabIndex = 25;
            this.lblLanguageWarning.Text = "This language pack may be not compatible with ImageGlass 3.2.0.16.";
            // 
            // lnkInstallLanguage
            // 
            this.lnkInstallLanguage.AutoSize = true;
            this.lnkInstallLanguage.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkInstallLanguage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkInstallLanguage.Location = new System.Drawing.Point(20, 189);
            this.lnkInstallLanguage.Name = "lnkInstallLanguage";
            this.lnkInstallLanguage.Size = new System.Drawing.Size(308, 25);
            this.lnkInstallLanguage.TabIndex = 21;
            this.lnkInstallLanguage.TabStop = true;
            this.lnkInstallLanguage.Text = "> Install new language pack (*.iglang)";
            this.lnkInstallLanguage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkInstallLanguage_LinkClicked);
            // 
            // lnkRefresh
            // 
            this.lnkRefresh.AutoSize = true;
            this.lnkRefresh.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkRefresh.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkRefresh.Location = new System.Drawing.Point(296, 61);
            this.lnkRefresh.Name = "lnkRefresh";
            this.lnkRefresh.Size = new System.Drawing.Size(87, 25);
            this.lnkRefresh.TabIndex = 20;
            this.lnkRefresh.TabStop = true;
            this.lnkRefresh.Text = "> Refresh";
            this.lnkRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRefresh_LinkClicked);
            // 
            // lnkEdit
            // 
            this.lnkEdit.AutoSize = true;
            this.lnkEdit.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkEdit.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkEdit.Location = new System.Drawing.Point(19, 249);
            this.lnkEdit.Name = "lnkEdit";
            this.lnkEdit.Size = new System.Drawing.Size(248, 25);
            this.lnkEdit.TabIndex = 23;
            this.lnkEdit.TabStop = true;
            this.lnkEdit.Text = "> Edit selected language pack";
            this.lnkEdit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkEdit_LinkClicked);
            // 
            // lnkCreateNew
            // 
            this.lnkCreateNew.AutoSize = true;
            this.lnkCreateNew.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkCreateNew.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkCreateNew.Location = new System.Drawing.Point(20, 219);
            this.lnkCreateNew.Name = "lnkCreateNew";
            this.lnkCreateNew.Size = new System.Drawing.Size(236, 25);
            this.lnkCreateNew.TabIndex = 22;
            this.lnkCreateNew.TabStop = true;
            this.lnkCreateNew.Text = "> Create new language pack";
            this.lnkCreateNew.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCreateNew_LinkClicked);
            // 
            // lnkGetMoreLanguage
            // 
            this.lnkGetMoreLanguage.AutoSize = true;
            this.lnkGetMoreLanguage.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkGetMoreLanguage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkGetMoreLanguage.Location = new System.Drawing.Point(20, 279);
            this.lnkGetMoreLanguage.Name = "lnkGetMoreLanguage";
            this.lnkGetMoreLanguage.Size = new System.Drawing.Size(231, 25);
            this.lnkGetMoreLanguage.TabIndex = 24;
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
            this.cmbLanguage.Location = new System.Drawing.Point(24, 58);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(253, 33);
            this.cmbLanguage.TabIndex = 19;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // lblLanguageText
            // 
            this.lblLanguageText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLanguageText.AutoSize = true;
            this.lblLanguageText.Location = new System.Drawing.Point(20, 30);
            this.lblLanguageText.Name = "lblLanguageText";
            this.lblLanguageText.Size = new System.Drawing.Size(168, 25);
            this.lblLanguageText.TabIndex = 1;
            this.lblLanguageText.Text = "Installed languages:";
            // 
            // tabFileAssociation
            // 
            this.tabFileAssociation.Controls.Add(this.panExtraExts);
            this.tabFileAssociation.Controls.Add(this.txtSupportedExtensionDefault);
            this.tabFileAssociation.Controls.Add(this.lblSupportedExtension);
            this.tabFileAssociation.Controls.Add(this.btnOpenFileAssociations);
            this.tabFileAssociation.Location = new System.Drawing.Point(4, 4);
            this.tabFileAssociation.Name = "tabFileAssociation";
            this.tabFileAssociation.Padding = new System.Windows.Forms.Padding(3);
            this.tabFileAssociation.Size = new System.Drawing.Size(524, 564);
            this.tabFileAssociation.TabIndex = 1;
            this.tabFileAssociation.Text = "file association";
            this.tabFileAssociation.UseVisualStyleBackColor = true;
            // 
            // panExtraExts
            // 
            this.panExtraExts.Controls.Add(this.chkExtraExtsPSD);
            this.panExtraExts.Controls.Add(this.chkExtraExtsHDR);
            this.panExtraExts.Controls.Add(this.chkExtraExtsEXR);
            this.panExtraExts.Controls.Add(this.chkExtraExtsTGA);
            this.panExtraExts.Location = new System.Drawing.Point(23, 142);
            this.panExtraExts.Name = "panExtraExts";
            this.panExtraExts.Size = new System.Drawing.Size(502, 150);
            this.panExtraExts.TabIndex = 28;
            // 
            // chkExtraExtsPSD
            // 
            this.chkExtraExtsPSD.AutoSize = true;
            this.chkExtraExtsPSD.Location = new System.Drawing.Point(3, 3);
            this.chkExtraExtsPSD.Name = "chkExtraExtsPSD";
            this.chkExtraExtsPSD.Size = new System.Drawing.Size(83, 29);
            this.chkExtraExtsPSD.TabIndex = 24;
            this.chkExtraExtsPSD.Tag = "*.psd";
            this.chkExtraExtsPSD.Text = "*.PSD";
            this.chkExtraExtsPSD.UseVisualStyleBackColor = true;
            // 
            // chkExtraExtsHDR
            // 
            this.chkExtraExtsHDR.AutoSize = true;
            this.chkExtraExtsHDR.Location = new System.Drawing.Point(3, 108);
            this.chkExtraExtsHDR.Name = "chkExtraExtsHDR";
            this.chkExtraExtsHDR.Size = new System.Drawing.Size(87, 29);
            this.chkExtraExtsHDR.TabIndex = 27;
            this.chkExtraExtsHDR.Tag = "*.hdr";
            this.chkExtraExtsHDR.Text = "*.HDR";
            this.chkExtraExtsHDR.UseVisualStyleBackColor = true;
            // 
            // chkExtraExtsEXR
            // 
            this.chkExtraExtsEXR.AutoSize = true;
            this.chkExtraExtsEXR.Location = new System.Drawing.Point(3, 38);
            this.chkExtraExtsEXR.Name = "chkExtraExtsEXR";
            this.chkExtraExtsEXR.Size = new System.Drawing.Size(81, 29);
            this.chkExtraExtsEXR.TabIndex = 25;
            this.chkExtraExtsEXR.Tag = "*.exr";
            this.chkExtraExtsEXR.Text = "*.EXR";
            this.chkExtraExtsEXR.UseVisualStyleBackColor = true;
            // 
            // chkExtraExtsTGA
            // 
            this.chkExtraExtsTGA.AutoSize = true;
            this.chkExtraExtsTGA.Location = new System.Drawing.Point(2, 73);
            this.chkExtraExtsTGA.Name = "chkExtraExtsTGA";
            this.chkExtraExtsTGA.Size = new System.Drawing.Size(82, 29);
            this.chkExtraExtsTGA.TabIndex = 26;
            this.chkExtraExtsTGA.Tag = "*.tga";
            this.chkExtraExtsTGA.Text = "*.TGA";
            this.chkExtraExtsTGA.UseVisualStyleBackColor = true;
            // 
            // txtSupportedExtensionDefault
            // 
            this.txtSupportedExtensionDefault.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSupportedExtensionDefault.BackColor = System.Drawing.Color.White;
            this.txtSupportedExtensionDefault.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSupportedExtensionDefault.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSupportedExtensionDefault.ForeColor = System.Drawing.Color.Black;
            this.txtSupportedExtensionDefault.Location = new System.Drawing.Point(23, 55);
            this.txtSupportedExtensionDefault.Multiline = true;
            this.txtSupportedExtensionDefault.Name = "txtSupportedExtensionDefault";
            this.txtSupportedExtensionDefault.ReadOnly = true;
            this.txtSupportedExtensionDefault.Size = new System.Drawing.Size(485, 81);
            this.txtSupportedExtensionDefault.TabIndex = 22;
            this.txtSupportedExtensionDefault.Text = "jpg\r\npng\r\n";
            // 
            // lblSupportedExtension
            // 
            this.lblSupportedExtension.AutoSize = true;
            this.lblSupportedExtension.Location = new System.Drawing.Point(20, 30);
            this.lblSupportedExtension.Name = "lblSupportedExtension";
            this.lblSupportedExtension.Size = new System.Drawing.Size(189, 25);
            this.lblSupportedExtension.TabIndex = 21;
            this.lblSupportedExtension.Text = "Supported extensions:";
            // 
            // btnOpenFileAssociations
            // 
            this.btnOpenFileAssociations.AutoSize = true;
            this.btnOpenFileAssociations.Location = new System.Drawing.Point(23, 298);
            this.btnOpenFileAssociations.Name = "btnOpenFileAssociations";
            this.btnOpenFileAssociations.Size = new System.Drawing.Size(258, 35);
            this.btnOpenFileAssociations.TabIndex = 20;
            this.btnOpenFileAssociations.Text = "Open File Associations";
            this.btnOpenFileAssociations.UseVisualStyleBackColor = true;
            this.btnOpenFileAssociations.Click += new System.EventHandler(this.btnOpenFileAssociations_Click);
            // 
            // tabGeneral
            // 
            this.tabGeneral.AutoScroll = true;
            this.tabGeneral.Controls.Add(this.panel1);
            this.tabGeneral.Controls.Add(this.chkLoopViewer);
            this.tabGeneral.Controls.Add(this.chkConfirmationDelete);
            this.tabGeneral.Controls.Add(this.chkMouseNavigation);
            this.tabGeneral.Controls.Add(this.lblGeneral_ZoomOptimization);
            this.tabGeneral.Controls.Add(this.cmbZoomOptimization);
            this.tabGeneral.Controls.Add(this.chkThumbnailVertical);
            this.tabGeneral.Controls.Add(this.chkAllowMultiInstances);
            this.tabGeneral.Controls.Add(this.lblGeneral_ThumbnailSize);
            this.tabGeneral.Controls.Add(this.cmbThumbnailDimension);
            this.tabGeneral.Controls.Add(this.chkESCToQuit);
            this.tabGeneral.Controls.Add(this.chkImageBoosterBack);
            this.tabGeneral.Controls.Add(this.chkLoopSlideshow);
            this.tabGeneral.Controls.Add(this.numMaxThumbSize);
            this.tabGeneral.Controls.Add(this.lblGeneral_MaxFileSize);
            this.tabGeneral.Controls.Add(this.chkHideToolBar);
            this.tabGeneral.Controls.Add(this.picBackgroundColor);
            this.tabGeneral.Controls.Add(this.lblBackGroundColor);
            this.tabGeneral.Controls.Add(this.chkWelcomePicture);
            this.tabGeneral.Controls.Add(this.lblImageLoadingOrder);
            this.tabGeneral.Controls.Add(this.cmbImageOrder);
            this.tabGeneral.Controls.Add(this.barInterval);
            this.tabGeneral.Controls.Add(this.lblSlideshowInterval);
            this.tabGeneral.Controls.Add(this.chkFindChildFolder);
            this.tabGeneral.Controls.Add(this.chkAutoUpdate);
            this.tabGeneral.Location = new System.Drawing.Point(4, 4);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(524, 564);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "general";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(20, 1033);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(109, 32);
            this.panel1.TabIndex = 24;
            // 
            // chkLoopViewer
            // 
            this.chkLoopViewer.AutoSize = true;
            this.chkLoopViewer.Location = new System.Drawing.Point(20, 159);
            this.chkLoopViewer.Name = "chkLoopViewer";
            this.chkLoopViewer.Size = new System.Drawing.Size(580, 29);
            this.chkLoopViewer.TabIndex = 23;
            this.chkLoopViewer.Text = "Loop back viewer to the first image when reaching the end of the list";
            this.chkLoopViewer.UseVisualStyleBackColor = true;
            this.chkLoopViewer.CheckedChanged += new System.EventHandler(this.chkLoopViewer_CheckedChanged);
            // 
            // chkConfirmationDelete
            // 
            this.chkConfirmationDelete.AutoSize = true;
            this.chkConfirmationDelete.Location = new System.Drawing.Point(20, 334);
            this.chkConfirmationDelete.Name = "chkConfirmationDelete";
            this.chkConfirmationDelete.Size = new System.Drawing.Size(317, 29);
            this.chkConfirmationDelete.TabIndex = 22;
            this.chkConfirmationDelete.Text = "Display Delete confirmation dialog ";
            this.chkConfirmationDelete.UseVisualStyleBackColor = true;
            this.chkConfirmationDelete.CheckedChanged += new System.EventHandler(this.chkConfirmationDelete_CheckedChanged);
            // 
            // chkMouseNavigation
            // 
            this.chkMouseNavigation.AutoSize = true;
            this.chkMouseNavigation.Location = new System.Drawing.Point(20, 758);
            this.chkMouseNavigation.Name = "chkMouseNavigation";
            this.chkMouseNavigation.Size = new System.Drawing.Size(550, 29);
            this.chkMouseNavigation.TabIndex = 21;
            this.chkMouseNavigation.Text = "Use the mouse wheel to browse images, hold CTRL for zooming.";
            this.chkMouseNavigation.UseVisualStyleBackColor = true;
            this.chkMouseNavigation.CheckedChanged += new System.EventHandler(this.chkMouseNavigation_CheckedChanged);
            // 
            // lblGeneral_ZoomOptimization
            // 
            this.lblGeneral_ZoomOptimization.AutoSize = true;
            this.lblGeneral_ZoomOptimization.Location = new System.Drawing.Point(15, 691);
            this.lblGeneral_ZoomOptimization.Name = "lblGeneral_ZoomOptimization";
            this.lblGeneral_ZoomOptimization.Size = new System.Drawing.Size(165, 25);
            this.lblGeneral_ZoomOptimization.TabIndex = 19;
            this.lblGeneral_ZoomOptimization.Text = "Zoom optimization";
            // 
            // cmbZoomOptimization
            // 
            this.cmbZoomOptimization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbZoomOptimization.FormattingEnabled = true;
            this.cmbZoomOptimization.Items.AddRange(new object[] {
            "(loaded from code)"});
            this.cmbZoomOptimization.Location = new System.Drawing.Point(20, 719);
            this.cmbZoomOptimization.Name = "cmbZoomOptimization";
            this.cmbZoomOptimization.Size = new System.Drawing.Size(279, 33);
            this.cmbZoomOptimization.TabIndex = 20;
            this.cmbZoomOptimization.SelectedIndexChanged += new System.EventHandler(this.cmbZoomOptimization_SelectedIndexChanged);
            // 
            // chkThumbnailVertical
            // 
            this.chkThumbnailVertical.AutoSize = true;
            this.chkThumbnailVertical.Location = new System.Drawing.Point(20, 369);
            this.chkThumbnailVertical.Name = "chkThumbnailVertical";
            this.chkThumbnailVertical.Size = new System.Drawing.Size(257, 29);
            this.chkThumbnailVertical.TabIndex = 18;
            this.chkThumbnailVertical.Text = "Thumbnail bar on right side";
            this.chkThumbnailVertical.UseVisualStyleBackColor = true;
            this.chkThumbnailVertical.CheckedChanged += new System.EventHandler(this.chkThumbnailVertical_CheckedChanged);
            // 
            // chkAllowMultiInstances
            // 
            this.chkAllowMultiInstances.AutoSize = true;
            this.chkAllowMultiInstances.Location = new System.Drawing.Point(20, 299);
            this.chkAllowMultiInstances.Name = "chkAllowMultiInstances";
            this.chkAllowMultiInstances.Size = new System.Drawing.Size(355, 29);
            this.chkAllowMultiInstances.TabIndex = 17;
            this.chkAllowMultiInstances.Text = "Allow multiple instances of the program";
            this.chkAllowMultiInstances.UseVisualStyleBackColor = true;
            this.chkAllowMultiInstances.CheckedChanged += new System.EventHandler(this.chkAllowMultiInstances_CheckedChanged);
            // 
            // lblGeneral_ThumbnailSize
            // 
            this.lblGeneral_ThumbnailSize.AutoSize = true;
            this.lblGeneral_ThumbnailSize.Location = new System.Drawing.Point(15, 501);
            this.lblGeneral_ThumbnailSize.Name = "lblGeneral_ThumbnailSize";
            this.lblGeneral_ThumbnailSize.Size = new System.Drawing.Size(257, 25);
            this.lblGeneral_ThumbnailSize.TabIndex = 15;
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
            "128"});
            this.cmbThumbnailDimension.Location = new System.Drawing.Point(20, 529);
            this.cmbThumbnailDimension.Name = "cmbThumbnailDimension";
            this.cmbThumbnailDimension.Size = new System.Drawing.Size(279, 33);
            this.cmbThumbnailDimension.TabIndex = 16;
            this.cmbThumbnailDimension.SelectedIndexChanged += new System.EventHandler(this.cmbThumbnailDimension_SelectedIndexChanged);
            // 
            // chkESCToQuit
            // 
            this.chkESCToQuit.AutoSize = true;
            this.chkESCToQuit.Location = new System.Drawing.Point(20, 264);
            this.chkESCToQuit.Name = "chkESCToQuit";
            this.chkESCToQuit.Size = new System.Drawing.Size(336, 29);
            this.chkESCToQuit.TabIndex = 10;
            this.chkESCToQuit.Text = "Allow to press ESC to quit application";
            this.chkESCToQuit.UseVisualStyleBackColor = true;
            this.chkESCToQuit.CheckedChanged += new System.EventHandler(this.chkESCToQuit_CheckedChanged);
            // 
            // chkImageBoosterBack
            // 
            this.chkImageBoosterBack.AutoSize = true;
            this.chkImageBoosterBack.Location = new System.Drawing.Point(20, 229);
            this.chkImageBoosterBack.Name = "chkImageBoosterBack";
            this.chkImageBoosterBack.Size = new System.Drawing.Size(578, 29);
            this.chkImageBoosterBack.TabIndex = 9;
            this.chkImageBoosterBack.Text = "Turn on Image Booster when navigate back (need more ~20% RAM)";
            this.chkImageBoosterBack.UseVisualStyleBackColor = true;
            this.chkImageBoosterBack.CheckedChanged += new System.EventHandler(this.chkImageBoosterBack_CheckedChanged);
            // 
            // chkLoopSlideshow
            // 
            this.chkLoopSlideshow.AutoSize = true;
            this.chkLoopSlideshow.Location = new System.Drawing.Point(20, 194);
            this.chkLoopSlideshow.Name = "chkLoopSlideshow";
            this.chkLoopSlideshow.Size = new System.Drawing.Size(608, 29);
            this.chkLoopSlideshow.TabIndex = 8;
            this.chkLoopSlideshow.Text = "Loop back slideshow to the first image when reaching the end of the list";
            this.chkLoopSlideshow.UseVisualStyleBackColor = true;
            this.chkLoopSlideshow.CheckedChanged += new System.EventHandler(this.chkLoopSlideshow_CheckedChanged);
            // 
            // numMaxThumbSize
            // 
            this.numMaxThumbSize.Location = new System.Drawing.Point(20, 447);
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
            this.numMaxThumbSize.Size = new System.Drawing.Size(79, 31);
            this.numMaxThumbSize.TabIndex = 13;
            this.numMaxThumbSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMaxThumbSize.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numMaxThumbSize.ValueChanged += new System.EventHandler(this.numMaxThumbSize_ValueChanged);
            // 
            // lblGeneral_MaxFileSize
            // 
            this.lblGeneral_MaxFileSize.AutoSize = true;
            this.lblGeneral_MaxFileSize.Location = new System.Drawing.Point(15, 419);
            this.lblGeneral_MaxFileSize.Name = "lblGeneral_MaxFileSize";
            this.lblGeneral_MaxFileSize.Size = new System.Drawing.Size(274, 25);
            this.lblGeneral_MaxFileSize.TabIndex = 14;
            this.lblGeneral_MaxFileSize.Text = "Maximum thumbnail size (in MB):";
            // 
            // chkHideToolBar
            // 
            this.chkHideToolBar.AutoSize = true;
            this.chkHideToolBar.Location = new System.Drawing.Point(20, 124);
            this.chkHideToolBar.Name = "chkHideToolBar";
            this.chkHideToolBar.Size = new System.Drawing.Size(276, 29);
            this.chkHideToolBar.TabIndex = 7;
            this.chkHideToolBar.Text = "Hide toolbar when starting up";
            this.chkHideToolBar.UseVisualStyleBackColor = true;
            this.chkHideToolBar.CheckedChanged += new System.EventHandler(this.chkHideToolBar_CheckedChanged);
            // 
            // lblBackGroundColor
            // 
            this.lblBackGroundColor.AutoSize = true;
            this.lblBackGroundColor.Location = new System.Drawing.Point(15, 921);
            this.lblBackGroundColor.Name = "lblBackGroundColor";
            this.lblBackGroundColor.Size = new System.Drawing.Size(156, 25);
            this.lblBackGroundColor.TabIndex = 11;
            this.lblBackGroundColor.Text = "Background color:";
            // 
            // chkWelcomePicture
            // 
            this.chkWelcomePicture.AutoSize = true;
            this.chkWelcomePicture.Location = new System.Drawing.Point(20, 89);
            this.chkWelcomePicture.Name = "chkWelcomePicture";
            this.chkWelcomePicture.Size = new System.Drawing.Size(216, 29);
            this.chkWelcomePicture.TabIndex = 6;
            this.chkWelcomePicture.Text = "Show welcome picture";
            this.chkWelcomePicture.UseVisualStyleBackColor = true;
            this.chkWelcomePicture.CheckedChanged += new System.EventHandler(this.chkWelcomePicture_CheckedChanged);
            // 
            // lblImageLoadingOrder
            // 
            this.lblImageLoadingOrder.AutoSize = true;
            this.lblImageLoadingOrder.Location = new System.Drawing.Point(15, 827);
            this.lblImageLoadingOrder.Name = "lblImageLoadingOrder";
            this.lblImageLoadingOrder.Size = new System.Drawing.Size(179, 25);
            this.lblImageLoadingOrder.TabIndex = 10;
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
            this.cmbImageOrder.Location = new System.Drawing.Point(20, 855);
            this.cmbImageOrder.Name = "cmbImageOrder";
            this.cmbImageOrder.Size = new System.Drawing.Size(279, 33);
            this.cmbImageOrder.TabIndex = 14;
            this.cmbImageOrder.SelectedIndexChanged += new System.EventHandler(this.cmbImageOrder_SelectedIndexChanged);
            // 
            // barInterval
            // 
            this.barInterval.BackColor = System.Drawing.SystemColors.Window;
            this.barInterval.Location = new System.Drawing.Point(20, 619);
            this.barInterval.Maximum = 60;
            this.barInterval.Minimum = 1;
            this.barInterval.Name = "barInterval";
            this.barInterval.Size = new System.Drawing.Size(292, 69);
            this.barInterval.TabIndex = 12;
            this.barInterval.TickStyle = System.Windows.Forms.TickStyle.None;
            this.barInterval.Value = 5;
            this.barInterval.Scroll += new System.EventHandler(this.barInterval_Scroll);
            // 
            // lblSlideshowInterval
            // 
            this.lblSlideshowInterval.AutoSize = true;
            this.lblSlideshowInterval.Location = new System.Drawing.Point(15, 591);
            this.lblSlideshowInterval.Name = "lblSlideshowInterval";
            this.lblSlideshowInterval.Size = new System.Drawing.Size(248, 25);
            this.lblSlideshowInterval.TabIndex = 5;
            this.lblSlideshowInterval.Text = "Slide show interval: 5 seconds";
            // 
            // chkFindChildFolder
            // 
            this.chkFindChildFolder.AutoSize = true;
            this.chkFindChildFolder.Location = new System.Drawing.Point(20, 53);
            this.chkFindChildFolder.Name = "chkFindChildFolder";
            this.chkFindChildFolder.Size = new System.Drawing.Size(247, 29);
            this.chkFindChildFolder.TabIndex = 5;
            this.chkFindChildFolder.Text = "Find images in child folder";
            this.chkFindChildFolder.UseVisualStyleBackColor = true;
            this.chkFindChildFolder.CheckedChanged += new System.EventHandler(this.chkFindChildFolder_CheckedChanged);
            // 
            // chkAutoUpdate
            // 
            this.chkAutoUpdate.AutoSize = true;
            this.chkAutoUpdate.Location = new System.Drawing.Point(20, 17);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(284, 29);
            this.chkAutoUpdate.TabIndex = 4;
            this.chkAutoUpdate.Text = "Check for update automatically";
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            this.chkAutoUpdate.CheckedChanged += new System.EventHandler(this.chkAutoUpdate_CheckedChanged);
            // 
            // tab1
            // 
            this.tab1.Alignment = System.Windows.Forms.TabAlignment.Right;
            this.tab1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tab1.Controls.Add(this.tabGeneral);
            this.tab1.Controls.Add(this.tabFileAssociation);
            this.tab1.Controls.Add(this.tabLanguage);
            this.tab1.Location = new System.Drawing.Point(-7, -6);
            this.tab1.Multiline = true;
            this.tab1.Name = "tab1";
            this.tab1.SelectedIndex = 0;
            this.tab1.Size = new System.Drawing.Size(562, 572);
            this.tab1.TabIndex = 0;
            this.tab1.SelectedIndexChanged += new System.EventHandler(this.tab1_SelectedIndexChanged);
            // 
            // imglOpenWith
            // 
            this.imglOpenWith.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imglOpenWith.ImageSize = new System.Drawing.Size(16, 16);
            this.imglOpenWith.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // sp0
            // 
            this.sp0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sp0.Location = new System.Drawing.Point(0, 0);
            this.sp0.Name = "sp0";
            // 
            // sp0.Panel1
            // 
            this.sp0.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.sp0.Panel1.Controls.Add(this.lblLanguage);
            this.sp0.Panel1.Controls.Add(this.lblGeneral);
            this.sp0.Panel1.Controls.Add(this.lblFileAssociations);
            // 
            // sp0.Panel2
            // 
            this.sp0.Panel2.Controls.Add(this.tab1);
            this.sp0.Size = new System.Drawing.Size(704, 561);
            this.sp0.SplitterDistance = 180;
            this.sp0.TabIndex = 17;
            this.sp0.TabStop = false;
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(704, 561);
            this.Controls.Add(this.sp0);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(720, 600);
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
            this.panExtraExts.ResumeLayout(false);
            this.panExtraExts.PerformLayout();
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThumbSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barInterval)).EndInit();
            this.tab1.ResumeLayout(false);
            this.sp0.Panel1.ResumeLayout(false);
            this.sp0.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sp0)).EndInit();
            this.sp0.ResumeLayout(false);
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
        private System.Windows.Forms.CheckBox chkLoopSlideshow;
        private System.Windows.Forms.NumericUpDown numMaxThumbSize;
        private System.Windows.Forms.Label lblGeneral_MaxFileSize;
        private System.Windows.Forms.CheckBox chkHideToolBar;
        private System.Windows.Forms.PictureBox picBackgroundColor;
        private System.Windows.Forms.Label lblBackGroundColor;
        private System.Windows.Forms.CheckBox chkWelcomePicture;
        private System.Windows.Forms.Label lblImageLoadingOrder;
        private System.Windows.Forms.ComboBox cmbImageOrder;
        private System.Windows.Forms.TrackBar barInterval;
        private System.Windows.Forms.Label lblSlideshowInterval;
        private System.Windows.Forms.CheckBox chkFindChildFolder;
        private System.Windows.Forms.CheckBox chkAutoUpdate;
        private System.Windows.Forms.TabControl tab1;
        private System.Windows.Forms.SplitContainer sp0;
        private System.Windows.Forms.CheckBox chkImageBoosterBack;
        private System.Windows.Forms.CheckBox chkESCToQuit;
        private System.Windows.Forms.LinkLabel lnkInstallLanguage;
        private System.Windows.Forms.Button btnOpenFileAssociations;
        private System.Windows.Forms.Label lblGeneral_ThumbnailSize;
        private System.Windows.Forms.ComboBox cmbThumbnailDimension;
        private System.Windows.Forms.CheckBox chkAllowMultiInstances;
        private System.Windows.Forms.ImageList imglOpenWith;
        private System.Windows.Forms.TextBox txtSupportedExtensionDefault;
        private System.Windows.Forms.Label lblSupportedExtension;
        private System.Windows.Forms.CheckBox chkExtraExtsPSD;
        private System.Windows.Forms.CheckBox chkExtraExtsHDR;
        private System.Windows.Forms.CheckBox chkExtraExtsTGA;
        private System.Windows.Forms.CheckBox chkExtraExtsEXR;
        private System.Windows.Forms.Panel panExtraExts;
        private System.Windows.Forms.Label lblLanguageWarning;
        private System.Windows.Forms.CheckBox chkThumbnailVertical;
        private System.Windows.Forms.Label lblGeneral_ZoomOptimization;
        private System.Windows.Forms.ComboBox cmbZoomOptimization;
        private System.Windows.Forms.CheckBox chkMouseNavigation;
        private System.Windows.Forms.CheckBox chkConfirmationDelete;
        private System.Windows.Forms.CheckBox chkLoopViewer;
        private System.Windows.Forms.Panel panel1;
    }
}