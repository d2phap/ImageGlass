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
            this.lblContextMenu = new System.Windows.Forms.Label();
            this.lblGeneral = new System.Windows.Forms.Label();
            this.tip1 = new System.Windows.Forms.ToolTip(this.components);
            this.picBackgroundColor = new System.Windows.Forms.PictureBox();
            this.tabLanguage = new System.Windows.Forms.TabPage();
            this.lnkRefresh = new System.Windows.Forms.LinkLabel();
            this.lnkEdit = new System.Windows.Forms.LinkLabel();
            this.lnkCreateNew = new System.Windows.Forms.LinkLabel();
            this.lnkGetMoreLanguage = new System.Windows.Forms.LinkLabel();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.lblLanguageText = new System.Windows.Forms.Label();
            this.tabContextMenu = new System.Windows.Forms.TabPage();
            this.btnRemoveAllContextMenu = new System.Windows.Forms.Button();
            this.btnUpdateContextMenu = new System.Windows.Forms.Button();
            this.btnAddDefaultContextMenu = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblExtensions = new System.Windows.Forms.Label();
            this.txtExtensions = new System.Windows.Forms.TextBox();
            this.lbl_ContextMenu_Description = new System.Windows.Forms.Label();
            this.tabGeneral = new System.Windows.Forms.TabPage();
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
            this.lblGeneral_ZoomOptimization = new System.Windows.Forms.Label();
            this.cmbZoomOptimization = new System.Windows.Forms.ComboBox();
            this.chkFindChildFolder = new System.Windows.Forms.CheckBox();
            this.chkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.tab1 = new System.Windows.Forms.TabControl();
            this.sp0 = new System.Windows.Forms.SplitContainer();
            this.chkImageBoosterBack = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundColor)).BeginInit();
            this.tabLanguage.SuspendLayout();
            this.tabContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
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
            this.lblLanguage.BackColor = System.Drawing.SystemColors.Control;
            this.lblLanguage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLanguage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLanguage.ForeColor = System.Drawing.Color.Black;
            this.lblLanguage.Location = new System.Drawing.Point(0, 80);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.lblLanguage.Size = new System.Drawing.Size(155, 40);
            this.lblLanguage.TabIndex = 17;
            this.lblLanguage.Tag = "0";
            this.lblLanguage.Text = "Language";
            this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblLanguage.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblLanguage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblLanguage.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblLanguage.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblLanguage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblContextMenu
            // 
            this.lblContextMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblContextMenu.BackColor = System.Drawing.SystemColors.Control;
            this.lblContextMenu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblContextMenu.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblContextMenu.ForeColor = System.Drawing.Color.Black;
            this.lblContextMenu.Location = new System.Drawing.Point(0, 40);
            this.lblContextMenu.Name = "lblContextMenu";
            this.lblContextMenu.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.lblContextMenu.Size = new System.Drawing.Size(155, 40);
            this.lblContextMenu.TabIndex = 16;
            this.lblContextMenu.Tag = "0";
            this.lblContextMenu.Text = "Context menu";
            this.lblContextMenu.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblContextMenu.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblContextMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblContextMenu.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblContextMenu.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblContextMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblGeneral
            // 
            this.lblGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGeneral.BackColor = System.Drawing.Color.Gainsboro;
            this.lblGeneral.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblGeneral.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGeneral.Location = new System.Drawing.Point(0, 0);
            this.lblGeneral.Name = "lblGeneral";
            this.lblGeneral.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.lblGeneral.Size = new System.Drawing.Size(155, 40);
            this.lblGeneral.TabIndex = 15;
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
            this.picBackgroundColor.Location = new System.Drawing.Point(20, 376);
            this.picBackgroundColor.Name = "picBackgroundColor";
            this.picBackgroundColor.Size = new System.Drawing.Size(100, 19);
            this.picBackgroundColor.TabIndex = 12;
            this.picBackgroundColor.TabStop = false;
            this.tip1.SetToolTip(this.picBackgroundColor, "Change background color");
            this.picBackgroundColor.Click += new System.EventHandler(this.picBackgroundColor_Click);
            // 
            // tabLanguage
            // 
            this.tabLanguage.Controls.Add(this.lnkRefresh);
            this.tabLanguage.Controls.Add(this.lnkEdit);
            this.tabLanguage.Controls.Add(this.lnkCreateNew);
            this.tabLanguage.Controls.Add(this.lnkGetMoreLanguage);
            this.tabLanguage.Controls.Add(this.cmbLanguage);
            this.tabLanguage.Controls.Add(this.lblLanguageText);
            this.tabLanguage.Location = new System.Drawing.Point(4, 4);
            this.tabLanguage.Name = "tabLanguage";
            this.tabLanguage.Padding = new System.Windows.Forms.Padding(3);
            this.tabLanguage.Size = new System.Drawing.Size(551, 456);
            this.tabLanguage.TabIndex = 2;
            this.tabLanguage.Text = "language";
            this.tabLanguage.UseVisualStyleBackColor = true;
            // 
            // lnkRefresh
            // 
            this.lnkRefresh.AutoSize = true;
            this.lnkRefresh.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkRefresh.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkRefresh.Location = new System.Drawing.Point(295, 53);
            this.lnkRefresh.Name = "lnkRefresh";
            this.lnkRefresh.Size = new System.Drawing.Size(57, 15);
            this.lnkRefresh.TabIndex = 23;
            this.lnkRefresh.TabStop = true;
            this.lnkRefresh.Text = "> Refresh";
            this.lnkRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRefresh_LinkClicked);
            // 
            // lnkEdit
            // 
            this.lnkEdit.AutoSize = true;
            this.lnkEdit.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkEdit.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkEdit.Location = new System.Drawing.Point(20, 248);
            this.lnkEdit.Name = "lnkEdit";
            this.lnkEdit.Size = new System.Drawing.Size(164, 15);
            this.lnkEdit.TabIndex = 22;
            this.lnkEdit.TabStop = true;
            this.lnkEdit.Text = "> Edit selected language pack";
            this.lnkEdit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkEdit_LinkClicked);
            // 
            // lnkCreateNew
            // 
            this.lnkCreateNew.AutoSize = true;
            this.lnkCreateNew.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkCreateNew.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkCreateNew.Location = new System.Drawing.Point(20, 217);
            this.lnkCreateNew.Name = "lnkCreateNew";
            this.lnkCreateNew.Size = new System.Drawing.Size(157, 15);
            this.lnkCreateNew.TabIndex = 21;
            this.lnkCreateNew.TabStop = true;
            this.lnkCreateNew.Text = "> Create new language pack";
            this.lnkCreateNew.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCreateNew_LinkClicked);
            // 
            // lnkGetMoreLanguage
            // 
            this.lnkGetMoreLanguage.AutoSize = true;
            this.lnkGetMoreLanguage.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkGetMoreLanguage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkGetMoreLanguage.Location = new System.Drawing.Point(19, 279);
            this.lnkGetMoreLanguage.Name = "lnkGetMoreLanguage";
            this.lnkGetMoreLanguage.Size = new System.Drawing.Size(152, 15);
            this.lnkGetMoreLanguage.TabIndex = 20;
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
            this.cmbLanguage.Location = new System.Drawing.Point(23, 50);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(253, 23);
            this.cmbLanguage.TabIndex = 2;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // lblLanguageText
            // 
            this.lblLanguageText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLanguageText.AutoSize = true;
            this.lblLanguageText.Location = new System.Drawing.Point(20, 30);
            this.lblLanguageText.Name = "lblLanguageText";
            this.lblLanguageText.Size = new System.Drawing.Size(111, 15);
            this.lblLanguageText.TabIndex = 1;
            this.lblLanguageText.Text = "Installed languages:";
            // 
            // tabContextMenu
            // 
            this.tabContextMenu.Controls.Add(this.btnRemoveAllContextMenu);
            this.tabContextMenu.Controls.Add(this.btnUpdateContextMenu);
            this.tabContextMenu.Controls.Add(this.btnAddDefaultContextMenu);
            this.tabContextMenu.Controls.Add(this.pictureBox1);
            this.tabContextMenu.Controls.Add(this.lblExtensions);
            this.tabContextMenu.Controls.Add(this.txtExtensions);
            this.tabContextMenu.Controls.Add(this.lbl_ContextMenu_Description);
            this.tabContextMenu.Location = new System.Drawing.Point(4, 4);
            this.tabContextMenu.Name = "tabContextMenu";
            this.tabContextMenu.Padding = new System.Windows.Forms.Padding(3);
            this.tabContextMenu.Size = new System.Drawing.Size(551, 456);
            this.tabContextMenu.TabIndex = 1;
            this.tabContextMenu.Text = "context menu";
            this.tabContextMenu.UseVisualStyleBackColor = true;
            // 
            // btnRemoveAllContextMenu
            // 
            this.btnRemoveAllContextMenu.Location = new System.Drawing.Point(264, 387);
            this.btnRemoveAllContextMenu.Name = "btnRemoveAllContextMenu";
            this.btnRemoveAllContextMenu.Size = new System.Drawing.Size(115, 29);
            this.btnRemoveAllContextMenu.TabIndex = 23;
            this.btnRemoveAllContextMenu.Text = "Remove all";
            this.btnRemoveAllContextMenu.UseVisualStyleBackColor = true;
            this.btnRemoveAllContextMenu.Click += new System.EventHandler(this.btnRemoveAllContextMenu_Click);
            // 
            // btnUpdateContextMenu
            // 
            this.btnUpdateContextMenu.Location = new System.Drawing.Point(143, 387);
            this.btnUpdateContextMenu.Name = "btnUpdateContextMenu";
            this.btnUpdateContextMenu.Size = new System.Drawing.Size(115, 29);
            this.btnUpdateContextMenu.TabIndex = 22;
            this.btnUpdateContextMenu.Text = "Update";
            this.btnUpdateContextMenu.UseVisualStyleBackColor = true;
            this.btnUpdateContextMenu.Click += new System.EventHandler(this.btnUpdateContextMenu_Click);
            // 
            // btnAddDefaultContextMenu
            // 
            this.btnAddDefaultContextMenu.Location = new System.Drawing.Point(22, 387);
            this.btnAddDefaultContextMenu.Name = "btnAddDefaultContextMenu";
            this.btnAddDefaultContextMenu.Size = new System.Drawing.Size(115, 29);
            this.btnAddDefaultContextMenu.TabIndex = 21;
            this.btnAddDefaultContextMenu.Text = "Add default";
            this.btnAddDefaultContextMenu.UseVisualStyleBackColor = true;
            this.btnAddDefaultContextMenu.Click += new System.EventHandler(this.btnAddDefaultContextMenu_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(21, 158);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(232, 167);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // lblExtensions
            // 
            this.lblExtensions.AutoSize = true;
            this.lblExtensions.Location = new System.Drawing.Point(19, 335);
            this.lblExtensions.Name = "lblExtensions";
            this.lblExtensions.Size = new System.Drawing.Size(65, 15);
            this.lblExtensions.TabIndex = 19;
            this.lblExtensions.Text = "Extensions:";
            // 
            // txtExtensions
            // 
            this.txtExtensions.Location = new System.Drawing.Point(22, 353);
            this.txtExtensions.Name = "txtExtensions";
            this.txtExtensions.Size = new System.Drawing.Size(357, 23);
            this.txtExtensions.TabIndex = 18;
            this.txtExtensions.TabStop = false;
            // 
            // lbl_ContextMenu_Description
            // 
            this.lbl_ContextMenu_Description.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_ContextMenu_Description.Location = new System.Drawing.Point(18, 18);
            this.lbl_ContextMenu_Description.Name = "lbl_ContextMenu_Description";
            this.lbl_ContextMenu_Description.Size = new System.Drawing.Size(516, 137);
            this.lbl_ContextMenu_Description.TabIndex = 0;
            this.lbl_ContextMenu_Description.Text = resources.GetString("lbl_ContextMenu_Description.Text");
            // 
            // tabGeneral
            // 
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
            this.tabGeneral.Controls.Add(this.lblGeneral_ZoomOptimization);
            this.tabGeneral.Controls.Add(this.cmbZoomOptimization);
            this.tabGeneral.Controls.Add(this.chkFindChildFolder);
            this.tabGeneral.Controls.Add(this.chkAutoUpdate);
            this.tabGeneral.Location = new System.Drawing.Point(4, 4);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(551, 454);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "general";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // chkLoopSlideshow
            // 
            this.chkLoopSlideshow.AutoSize = true;
            this.chkLoopSlideshow.Location = new System.Drawing.Point(20, 67);
            this.chkLoopSlideshow.Name = "chkLoopSlideshow";
            this.chkLoopSlideshow.Size = new System.Drawing.Size(405, 19);
            this.chkLoopSlideshow.TabIndex = 16;
            this.chkLoopSlideshow.Text = "Loop back slideshow to the first image when reaching the end of the list";
            this.chkLoopSlideshow.UseVisualStyleBackColor = true;
            this.chkLoopSlideshow.CheckedChanged += new System.EventHandler(this.chkLoopSlideshow_CheckedChanged);
            // 
            // numMaxThumbSize
            // 
            this.numMaxThumbSize.Location = new System.Drawing.Point(20, 262);
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
            this.numMaxThumbSize.Size = new System.Drawing.Size(79, 23);
            this.numMaxThumbSize.TabIndex = 15;
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
            this.lblGeneral_MaxFileSize.Location = new System.Drawing.Point(17, 244);
            this.lblGeneral_MaxFileSize.Name = "lblGeneral_MaxFileSize";
            this.lblGeneral_MaxFileSize.Size = new System.Drawing.Size(186, 15);
            this.lblGeneral_MaxFileSize.TabIndex = 14;
            this.lblGeneral_MaxFileSize.Text = "Maximum thumbnail size (in MB):";
            // 
            // chkHideToolBar
            // 
            this.chkHideToolBar.AutoSize = true;
            this.chkHideToolBar.Location = new System.Drawing.Point(312, 42);
            this.chkHideToolBar.Name = "chkHideToolBar";
            this.chkHideToolBar.Size = new System.Drawing.Size(167, 19);
            this.chkHideToolBar.TabIndex = 13;
            this.chkHideToolBar.Text = "Hide toolbar when start up";
            this.chkHideToolBar.UseVisualStyleBackColor = true;
            this.chkHideToolBar.CheckedChanged += new System.EventHandler(this.chkHideToolBar_CheckedChanged);
            // 
            // lblBackGroundColor
            // 
            this.lblBackGroundColor.AutoSize = true;
            this.lblBackGroundColor.Location = new System.Drawing.Point(17, 355);
            this.lblBackGroundColor.Name = "lblBackGroundColor";
            this.lblBackGroundColor.Size = new System.Drawing.Size(104, 15);
            this.lblBackGroundColor.TabIndex = 11;
            this.lblBackGroundColor.Text = "Background color:";
            // 
            // chkWelcomePicture
            // 
            this.chkWelcomePicture.AutoSize = true;
            this.chkWelcomePicture.Location = new System.Drawing.Point(312, 17);
            this.chkWelcomePicture.Name = "chkWelcomePicture";
            this.chkWelcomePicture.Size = new System.Drawing.Size(146, 19);
            this.chkWelcomePicture.TabIndex = 3;
            this.chkWelcomePicture.Text = "Show welcome picture";
            this.chkWelcomePicture.UseVisualStyleBackColor = true;
            this.chkWelcomePicture.CheckedChanged += new System.EventHandler(this.chkWelcomePicture_CheckedChanged);
            // 
            // lblImageLoadingOrder
            // 
            this.lblImageLoadingOrder.AutoSize = true;
            this.lblImageLoadingOrder.Location = new System.Drawing.Point(17, 299);
            this.lblImageLoadingOrder.Name = "lblImageLoadingOrder";
            this.lblImageLoadingOrder.Size = new System.Drawing.Size(117, 15);
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
            this.cmbImageOrder.Location = new System.Drawing.Point(20, 317);
            this.cmbImageOrder.Name = "cmbImageOrder";
            this.cmbImageOrder.Size = new System.Drawing.Size(279, 23);
            this.cmbImageOrder.TabIndex = 7;
            this.cmbImageOrder.SelectedIndexChanged += new System.EventHandler(this.cmbImageOrder_SelectedIndexChanged);
            // 
            // barInterval
            // 
            this.barInterval.BackColor = System.Drawing.Color.White;
            this.barInterval.Location = new System.Drawing.Point(14, 207);
            this.barInterval.Maximum = 60;
            this.barInterval.Minimum = 1;
            this.barInterval.Name = "barInterval";
            this.barInterval.Size = new System.Drawing.Size(292, 45);
            this.barInterval.TabIndex = 5;
            this.barInterval.TickStyle = System.Windows.Forms.TickStyle.None;
            this.barInterval.Value = 5;
            this.barInterval.Scroll += new System.EventHandler(this.barInterval_Scroll);
            // 
            // lblSlideshowInterval
            // 
            this.lblSlideshowInterval.AutoSize = true;
            this.lblSlideshowInterval.Location = new System.Drawing.Point(17, 189);
            this.lblSlideshowInterval.Name = "lblSlideshowInterval";
            this.lblSlideshowInterval.Size = new System.Drawing.Size(171, 15);
            this.lblSlideshowInterval.TabIndex = 5;
            this.lblSlideshowInterval.Text = "Slide show interval (seconds): 5";
            // 
            // lblGeneral_ZoomOptimization
            // 
            this.lblGeneral_ZoomOptimization.AutoSize = true;
            this.lblGeneral_ZoomOptimization.Location = new System.Drawing.Point(17, 135);
            this.lblGeneral_ZoomOptimization.Name = "lblGeneral_ZoomOptimization";
            this.lblGeneral_ZoomOptimization.Size = new System.Drawing.Size(112, 15);
            this.lblGeneral_ZoomOptimization.TabIndex = 4;
            this.lblGeneral_ZoomOptimization.Text = "Zoom optimization:";
            // 
            // cmbZoomOptimization
            // 
            this.cmbZoomOptimization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbZoomOptimization.FormattingEnabled = true;
            this.cmbZoomOptimization.Items.AddRange(new object[] {
            "Auto",
            "Smooth pixels",
            "Clear pixels"});
            this.cmbZoomOptimization.Location = new System.Drawing.Point(20, 153);
            this.cmbZoomOptimization.Name = "cmbZoomOptimization";
            this.cmbZoomOptimization.Size = new System.Drawing.Size(279, 23);
            this.cmbZoomOptimization.TabIndex = 4;
            this.cmbZoomOptimization.SelectedIndexChanged += new System.EventHandler(this.cmbZoomOptimization_SelectedIndexChanged);
            // 
            // chkFindChildFolder
            // 
            this.chkFindChildFolder.AutoSize = true;
            this.chkFindChildFolder.Location = new System.Drawing.Point(20, 42);
            this.chkFindChildFolder.Name = "chkFindChildFolder";
            this.chkFindChildFolder.Size = new System.Drawing.Size(166, 19);
            this.chkFindChildFolder.TabIndex = 2;
            this.chkFindChildFolder.Text = "Find images in child folder";
            this.chkFindChildFolder.UseVisualStyleBackColor = true;
            this.chkFindChildFolder.CheckedChanged += new System.EventHandler(this.chkFindChildFolder_CheckedChanged);
            // 
            // chkAutoUpdate
            // 
            this.chkAutoUpdate.AutoSize = true;
            this.chkAutoUpdate.Location = new System.Drawing.Point(20, 17);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(192, 19);
            this.chkAutoUpdate.TabIndex = 1;
            this.chkAutoUpdate.Text = "Check for update automatically";
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            this.chkAutoUpdate.CheckedChanged += new System.EventHandler(this.chkAutoUpdate_CheckedChanged);
            // 
            // tab1
            // 
            this.tab1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tab1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tab1.Controls.Add(this.tabGeneral);
            this.tab1.Controls.Add(this.tabContextMenu);
            this.tab1.Controls.Add(this.tabLanguage);
            this.tab1.Location = new System.Drawing.Point(-7, -6);
            this.tab1.Multiline = true;
            this.tab1.Name = "tab1";
            this.tab1.SelectedIndex = 0;
            this.tab1.Size = new System.Drawing.Size(559, 482);
            this.tab1.TabIndex = 15;
            this.tab1.SelectedIndexChanged += new System.EventHandler(this.tab1_SelectedIndexChanged);
            // 
            // sp0
            // 
            this.sp0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sp0.Location = new System.Drawing.Point(0, 0);
            this.sp0.Name = "sp0";
            // 
            // sp0.Panel1
            // 
            this.sp0.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.sp0.Panel1.Controls.Add(this.lblLanguage);
            this.sp0.Panel1.Controls.Add(this.lblGeneral);
            this.sp0.Panel1.Controls.Add(this.lblContextMenu);
            // 
            // sp0.Panel2
            // 
            this.sp0.Panel2.Controls.Add(this.tab1);
            this.sp0.Size = new System.Drawing.Size(704, 441);
            this.sp0.SplitterDistance = 155;
            this.sp0.TabIndex = 17;
            this.sp0.TabStop = false;
            // 
            // chkImageBoosterBack
            // 
            this.chkImageBoosterBack.AutoSize = true;
            this.chkImageBoosterBack.Location = new System.Drawing.Point(20, 92);
            this.chkImageBoosterBack.Name = "chkImageBoosterBack";
            this.chkImageBoosterBack.Size = new System.Drawing.Size(385, 19);
            this.chkImageBoosterBack.TabIndex = 17;
            this.chkImageBoosterBack.Text = "Turn on Image Booster when navigate back (need more ~20% RAM)";
            this.chkImageBoosterBack.UseVisualStyleBackColor = true;
            this.chkImageBoosterBack.CheckedChanged += new System.EventHandler(this.chkImageBoosterBack_CheckedChanged);
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(704, 441);
            this.Controls.Add(this.sp0);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(720, 480);
            this.Name = "frmSetting";
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSetting_FormClosing);
            this.Load += new System.EventHandler(this.frmSetting_Load);
            this.SizeChanged += new System.EventHandler(this.frmSetting_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmSetting_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundColor)).EndInit();
            this.tabLanguage.ResumeLayout(false);
            this.tabLanguage.PerformLayout();
            this.tabContextMenu.ResumeLayout(false);
            this.tabContextMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
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
        private System.Windows.Forms.Label lblContextMenu;
        private System.Windows.Forms.Label lblGeneral;
        private System.Windows.Forms.ToolTip tip1;
        private System.Windows.Forms.TabPage tabLanguage;
        private System.Windows.Forms.LinkLabel lnkRefresh;
        private System.Windows.Forms.LinkLabel lnkEdit;
        private System.Windows.Forms.LinkLabel lnkCreateNew;
        private System.Windows.Forms.LinkLabel lnkGetMoreLanguage;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.Label lblLanguageText;
        private System.Windows.Forms.TabPage tabContextMenu;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblExtensions;
        private System.Windows.Forms.TextBox txtExtensions;
        private System.Windows.Forms.Label lbl_ContextMenu_Description;
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
        private System.Windows.Forms.Label lblGeneral_ZoomOptimization;
        private System.Windows.Forms.ComboBox cmbZoomOptimization;
        private System.Windows.Forms.CheckBox chkFindChildFolder;
        private System.Windows.Forms.CheckBox chkAutoUpdate;
        private System.Windows.Forms.TabControl tab1;
        private System.Windows.Forms.SplitContainer sp0;
        private System.Windows.Forms.Button btnRemoveAllContextMenu;
        private System.Windows.Forms.Button btnUpdateContextMenu;
        private System.Windows.Forms.Button btnAddDefaultContextMenu;
        private System.Windows.Forms.CheckBox chkImageBoosterBack;

    }
}