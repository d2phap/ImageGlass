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
            this.tab1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.chkHideToolBar = new System.Windows.Forms.CheckBox();
            this.picBackgroundColor = new System.Windows.Forms.PictureBox();
            this.lblBackGroundColor = new System.Windows.Forms.Label();
            this.chkWelcomePicture = new System.Windows.Forms.CheckBox();
            this.lblImageLoadingOrder = new System.Windows.Forms.Label();
            this.cmbImageOrder = new System.Windows.Forms.ComboBox();
            this.numMaxThumbSize = new System.Windows.Forms.NumericUpDown();
            this.barInterval = new System.Windows.Forms.TrackBar();
            this.lblSlideshowInterval = new System.Windows.Forms.Label();
            this.lblGeneral_ZoomOptimization = new System.Windows.Forms.Label();
            this.cmbZoomOptimization = new System.Windows.Forms.ComboBox();
            this.chkFindChildFolder = new System.Windows.Forms.CheckBox();
            this.chkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.chkLockWorkspace = new System.Windows.Forms.CheckBox();
            this.tabContextMenu = new System.Windows.Forms.TabPage();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblUpdateContextMenu = new System.Windows.Forms.Label();
            this.lblExtensions = new System.Windows.Forms.Label();
            this.txtExtensions = new System.Windows.Forms.TextBox();
            this.lblRemoveAllContextMenu = new System.Windows.Forms.Label();
            this.lblAddDefaultContextMenu = new System.Windows.Forms.Label();
            this.lbl_ContextMenu_Description = new System.Windows.Forms.Label();
            this.tabLanguage = new System.Windows.Forms.TabPage();
            this.lnkRefresh = new System.Windows.Forms.LinkLabel();
            this.lnkEdit = new System.Windows.Forms.LinkLabel();
            this.lnkCreateNew = new System.Windows.Forms.LinkLabel();
            this.lnkGetMoreLanguage = new System.Windows.Forms.LinkLabel();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.lblLanguageText = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.lblContextMenu = new System.Windows.Forms.Label();
            this.lblGeneral = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblGeneral_MaxFileSize = new System.Windows.Forms.Label();
            this.tab1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThumbSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barInterval)).BeginInit();
            this.tabContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabLanguage.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imglTheme
            // 
            this.imglTheme.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imglTheme.ImageSize = new System.Drawing.Size(32, 32);
            this.imglTheme.TransparentColor = System.Drawing.Color.Transparent;
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
            this.tab1.Location = new System.Drawing.Point(-10, 53);
            this.tab1.Multiline = true;
            this.tab1.Name = "tab1";
            this.tab1.SelectedIndex = 0;
            this.tab1.Size = new System.Drawing.Size(612, 476);
            this.tab1.TabIndex = 15;
            this.tab1.SelectedIndexChanged += new System.EventHandler(this.tab1_SelectedIndexChanged);
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.chkHideToolBar);
            this.tabGeneral.Controls.Add(this.picBackgroundColor);
            this.tabGeneral.Controls.Add(this.lblBackGroundColor);
            this.tabGeneral.Controls.Add(this.chkWelcomePicture);
            this.tabGeneral.Controls.Add(this.lblImageLoadingOrder);
            this.tabGeneral.Controls.Add(this.cmbImageOrder);
            this.tabGeneral.Controls.Add(this.numMaxThumbSize);
            this.tabGeneral.Controls.Add(this.lblGeneral_MaxFileSize);
            this.tabGeneral.Controls.Add(this.barInterval);
            this.tabGeneral.Controls.Add(this.lblSlideshowInterval);
            this.tabGeneral.Controls.Add(this.lblGeneral_ZoomOptimization);
            this.tabGeneral.Controls.Add(this.cmbZoomOptimization);
            this.tabGeneral.Controls.Add(this.chkFindChildFolder);
            this.tabGeneral.Controls.Add(this.chkAutoUpdate);
            this.tabGeneral.Controls.Add(this.chkLockWorkspace);
            this.tabGeneral.Location = new System.Drawing.Point(4, 4);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(604, 448);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "general";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // chkHideToolBar
            // 
            this.chkHideToolBar.AutoSize = true;
            this.chkHideToolBar.Location = new System.Drawing.Point(22, 124);
            this.chkHideToolBar.Name = "chkHideToolBar";
            this.chkHideToolBar.Size = new System.Drawing.Size(167, 19);
            this.chkHideToolBar.TabIndex = 13;
            this.chkHideToolBar.Text = "Hide toolbar when start up";
            this.chkHideToolBar.UseVisualStyleBackColor = true;
            this.chkHideToolBar.CheckedChanged += new System.EventHandler(this.chkHideToolBar_CheckedChanged);
            // 
            // picBackgroundColor
            // 
            this.picBackgroundColor.BackColor = System.Drawing.Color.White;
            this.picBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBackgroundColor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picBackgroundColor.Location = new System.Drawing.Point(24, 395);
            this.picBackgroundColor.Name = "picBackgroundColor";
            this.picBackgroundColor.Size = new System.Drawing.Size(100, 19);
            this.picBackgroundColor.TabIndex = 12;
            this.picBackgroundColor.TabStop = false;
            this.tip1.SetToolTip(this.picBackgroundColor, "Change background color");
            this.picBackgroundColor.Click += new System.EventHandler(this.picBackgroundColor_Click);
            // 
            // lblBackGroundColor
            // 
            this.lblBackGroundColor.AutoSize = true;
            this.lblBackGroundColor.Location = new System.Drawing.Point(20, 374);
            this.lblBackGroundColor.Name = "lblBackGroundColor";
            this.lblBackGroundColor.Size = new System.Drawing.Size(104, 15);
            this.lblBackGroundColor.TabIndex = 11;
            this.lblBackGroundColor.Text = "Background color:";
            // 
            // chkWelcomePicture
            // 
            this.chkWelcomePicture.AutoSize = true;
            this.chkWelcomePicture.Location = new System.Drawing.Point(22, 99);
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
            this.lblImageLoadingOrder.Location = new System.Drawing.Point(20, 318);
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
            this.cmbImageOrder.Location = new System.Drawing.Point(23, 336);
            this.cmbImageOrder.Name = "cmbImageOrder";
            this.cmbImageOrder.Size = new System.Drawing.Size(279, 23);
            this.cmbImageOrder.TabIndex = 7;
            this.cmbImageOrder.SelectedIndexChanged += new System.EventHandler(this.cmbImageOrder_SelectedIndexChanged);
            // 
            // numMaxThumbSize
            // 
            this.numMaxThumbSize.Location = new System.Drawing.Point(24, 279);
            this.numMaxThumbSize.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numMaxThumbSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxThumbSize.Name = "numMaxThumbSize";
            this.numMaxThumbSize.Size = new System.Drawing.Size(84, 23);
            this.numMaxThumbSize.TabIndex = 6;
            this.numMaxThumbSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMaxThumbSize.ThousandsSeparator = true;
            this.numMaxThumbSize.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numMaxThumbSize.ValueChanged += new System.EventHandler(this.numMaxThumbSize_ValueChanged);
            // 
            // barInterval
            // 
            this.barInterval.BackColor = System.Drawing.Color.White;
            this.barInterval.Location = new System.Drawing.Point(24, 228);
            this.barInterval.Maximum = 30;
            this.barInterval.Minimum = 1;
            this.barInterval.Name = "barInterval";
            this.barInterval.Size = new System.Drawing.Size(279, 45);
            this.barInterval.TabIndex = 5;
            this.barInterval.TickStyle = System.Windows.Forms.TickStyle.None;
            this.barInterval.Value = 5;
            this.barInterval.Scroll += new System.EventHandler(this.barInterval_Scroll);
            // 
            // lblSlideshowInterval
            // 
            this.lblSlideshowInterval.AutoSize = true;
            this.lblSlideshowInterval.Location = new System.Drawing.Point(21, 210);
            this.lblSlideshowInterval.Name = "lblSlideshowInterval";
            this.lblSlideshowInterval.Size = new System.Drawing.Size(171, 15);
            this.lblSlideshowInterval.TabIndex = 5;
            this.lblSlideshowInterval.Text = "Slide show interval (seconds): 5";
            // 
            // lblGeneral_ZoomOptimization
            // 
            this.lblGeneral_ZoomOptimization.AutoSize = true;
            this.lblGeneral_ZoomOptimization.Location = new System.Drawing.Point(21, 156);
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
            this.cmbZoomOptimization.Location = new System.Drawing.Point(24, 174);
            this.cmbZoomOptimization.Name = "cmbZoomOptimization";
            this.cmbZoomOptimization.Size = new System.Drawing.Size(279, 23);
            this.cmbZoomOptimization.TabIndex = 4;
            this.cmbZoomOptimization.SelectedIndexChanged += new System.EventHandler(this.cmbZoomOptimization_SelectedIndexChanged);
            // 
            // chkFindChildFolder
            // 
            this.chkFindChildFolder.AutoSize = true;
            this.chkFindChildFolder.Location = new System.Drawing.Point(22, 74);
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
            this.chkAutoUpdate.Location = new System.Drawing.Point(23, 49);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(192, 19);
            this.chkAutoUpdate.TabIndex = 1;
            this.chkAutoUpdate.Text = "Check for update automatically";
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            this.chkAutoUpdate.CheckedChanged += new System.EventHandler(this.chkAutoUpdate_CheckedChanged);
            // 
            // chkLockWorkspace
            // 
            this.chkLockWorkspace.AutoSize = true;
            this.chkLockWorkspace.Location = new System.Drawing.Point(23, 24);
            this.chkLockWorkspace.Name = "chkLockWorkspace";
            this.chkLockWorkspace.Size = new System.Drawing.Size(153, 19);
            this.chkLockWorkspace.TabIndex = 0;
            this.chkLockWorkspace.Text = "Lock to workspace edge";
            this.chkLockWorkspace.UseVisualStyleBackColor = true;
            this.chkLockWorkspace.CheckedChanged += new System.EventHandler(this.chkLockWorkspace_CheckedChanged);
            // 
            // tabContextMenu
            // 
            this.tabContextMenu.Controls.Add(this.pictureBox1);
            this.tabContextMenu.Controls.Add(this.lblUpdateContextMenu);
            this.tabContextMenu.Controls.Add(this.lblExtensions);
            this.tabContextMenu.Controls.Add(this.txtExtensions);
            this.tabContextMenu.Controls.Add(this.lblRemoveAllContextMenu);
            this.tabContextMenu.Controls.Add(this.lblAddDefaultContextMenu);
            this.tabContextMenu.Controls.Add(this.lbl_ContextMenu_Description);
            this.tabContextMenu.Location = new System.Drawing.Point(4, 4);
            this.tabContextMenu.Name = "tabContextMenu";
            this.tabContextMenu.Padding = new System.Windows.Forms.Padding(3);
            this.tabContextMenu.Size = new System.Drawing.Size(604, 448);
            this.tabContextMenu.TabIndex = 1;
            this.tabContextMenu.Text = "context menu";
            this.tabContextMenu.UseVisualStyleBackColor = true;
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
            // lblUpdateContextMenu
            // 
            this.lblUpdateContextMenu.BackColor = System.Drawing.Color.Silver;
            this.lblUpdateContextMenu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblUpdateContextMenu.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblUpdateContextMenu.ForeColor = System.Drawing.Color.White;
            this.lblUpdateContextMenu.Location = new System.Drawing.Point(143, 387);
            this.lblUpdateContextMenu.Name = "lblUpdateContextMenu";
            this.lblUpdateContextMenu.Padding = new System.Windows.Forms.Padding(5);
            this.lblUpdateContextMenu.Size = new System.Drawing.Size(115, 25);
            this.lblUpdateContextMenu.TabIndex = 20;
            this.lblUpdateContextMenu.Tag = "1";
            this.lblUpdateContextMenu.Text = "Update";
            this.lblUpdateContextMenu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUpdateContextMenu.Click += new System.EventHandler(this.lblUpdateContextMenu_Click);
            this.lblUpdateContextMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseDown);
            this.lblUpdateContextMenu.MouseEnter += new System.EventHandler(this.lblButton_MouseEnter);
            this.lblUpdateContextMenu.MouseLeave += new System.EventHandler(this.lblButton_MouseLeave);
            this.lblUpdateContextMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseUp);
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
            this.txtExtensions.Size = new System.Drawing.Size(565, 23);
            this.txtExtensions.TabIndex = 18;
            // 
            // lblRemoveAllContextMenu
            // 
            this.lblRemoveAllContextMenu.BackColor = System.Drawing.Color.Silver;
            this.lblRemoveAllContextMenu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblRemoveAllContextMenu.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRemoveAllContextMenu.ForeColor = System.Drawing.Color.White;
            this.lblRemoveAllContextMenu.Location = new System.Drawing.Point(264, 387);
            this.lblRemoveAllContextMenu.Name = "lblRemoveAllContextMenu";
            this.lblRemoveAllContextMenu.Padding = new System.Windows.Forms.Padding(5);
            this.lblRemoveAllContextMenu.Size = new System.Drawing.Size(115, 25);
            this.lblRemoveAllContextMenu.TabIndex = 17;
            this.lblRemoveAllContextMenu.Tag = "1";
            this.lblRemoveAllContextMenu.Text = "Remove all";
            this.lblRemoveAllContextMenu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRemoveAllContextMenu.Click += new System.EventHandler(this.lblRemoveAllContextMenu_Click);
            this.lblRemoveAllContextMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseDown);
            this.lblRemoveAllContextMenu.MouseEnter += new System.EventHandler(this.lblButton_MouseEnter);
            this.lblRemoveAllContextMenu.MouseLeave += new System.EventHandler(this.lblButton_MouseLeave);
            this.lblRemoveAllContextMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseUp);
            // 
            // lblAddDefaultContextMenu
            // 
            this.lblAddDefaultContextMenu.BackColor = System.Drawing.Color.Silver;
            this.lblAddDefaultContextMenu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblAddDefaultContextMenu.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAddDefaultContextMenu.ForeColor = System.Drawing.Color.White;
            this.lblAddDefaultContextMenu.Location = new System.Drawing.Point(22, 387);
            this.lblAddDefaultContextMenu.Name = "lblAddDefaultContextMenu";
            this.lblAddDefaultContextMenu.Padding = new System.Windows.Forms.Padding(5);
            this.lblAddDefaultContextMenu.Size = new System.Drawing.Size(115, 25);
            this.lblAddDefaultContextMenu.TabIndex = 16;
            this.lblAddDefaultContextMenu.Tag = "1";
            this.lblAddDefaultContextMenu.Text = "Add default";
            this.lblAddDefaultContextMenu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblAddDefaultContextMenu.Click += new System.EventHandler(this.lblAddDefaultContextMenu_Click);
            this.lblAddDefaultContextMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseDown);
            this.lblAddDefaultContextMenu.MouseEnter += new System.EventHandler(this.lblButton_MouseEnter);
            this.lblAddDefaultContextMenu.MouseLeave += new System.EventHandler(this.lblButton_MouseLeave);
            this.lblAddDefaultContextMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseUp);
            // 
            // lbl_ContextMenu_Description
            // 
            this.lbl_ContextMenu_Description.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_ContextMenu_Description.Location = new System.Drawing.Point(18, 18);
            this.lbl_ContextMenu_Description.Name = "lbl_ContextMenu_Description";
            this.lbl_ContextMenu_Description.Size = new System.Drawing.Size(569, 137);
            this.lbl_ContextMenu_Description.TabIndex = 0;
            this.lbl_ContextMenu_Description.Text = resources.GetString("lbl_ContextMenu_Description.Text");
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
            this.tabLanguage.Size = new System.Drawing.Size(604, 448);
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
            // panel2
            // 
            this.panel2.Controls.Add(this.lblLanguage);
            this.panel2.Controls.Add(this.lblContextMenu);
            this.panel2.Controls.Add(this.lblGeneral);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(594, 62);
            this.panel2.TabIndex = 16;
            // 
            // lblLanguage
            // 
            this.lblLanguage.BackColor = System.Drawing.Color.Silver;
            this.lblLanguage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLanguage.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblLanguage.ForeColor = System.Drawing.Color.White;
            this.lblLanguage.Location = new System.Drawing.Point(302, 9);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(139, 40);
            this.lblLanguage.TabIndex = 17;
            this.lblLanguage.Tag = "0";
            this.lblLanguage.Text = "Language";
            this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLanguage.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblLanguage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblLanguage.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblLanguage.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblLanguage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblContextMenu
            // 
            this.lblContextMenu.BackColor = System.Drawing.Color.Silver;
            this.lblContextMenu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblContextMenu.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblContextMenu.ForeColor = System.Drawing.Color.White;
            this.lblContextMenu.Location = new System.Drawing.Point(157, 9);
            this.lblContextMenu.Name = "lblContextMenu";
            this.lblContextMenu.Size = new System.Drawing.Size(139, 40);
            this.lblContextMenu.TabIndex = 16;
            this.lblContextMenu.Tag = "0";
            this.lblContextMenu.Text = "Context menu";
            this.lblContextMenu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblContextMenu.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblContextMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblContextMenu.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblContextMenu.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblContextMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblGeneral
            // 
            this.lblGeneral.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(176)))));
            this.lblGeneral.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblGeneral.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblGeneral.ForeColor = System.Drawing.Color.White;
            this.lblGeneral.Location = new System.Drawing.Point(12, 9);
            this.lblGeneral.Name = "lblGeneral";
            this.lblGeneral.Size = new System.Drawing.Size(139, 40);
            this.lblGeneral.TabIndex = 15;
            this.lblGeneral.Tag = "1";
            this.lblGeneral.Text = "General";
            this.lblGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblGeneral.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblGeneral.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblGeneral.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblGeneral.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblGeneral.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(500, 7);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 28);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 490);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(594, 42);
            this.panel1.TabIndex = 10;
            // 
            // lblGeneral_MaxFileSize
            // 
            this.lblGeneral_MaxFileSize.AutoSize = true;
            this.lblGeneral_MaxFileSize.Location = new System.Drawing.Point(21, 258);
            this.lblGeneral_MaxFileSize.Name = "lblGeneral_MaxFileSize";
            this.lblGeneral_MaxFileSize.Size = new System.Drawing.Size(192, 15);
            this.lblGeneral_MaxFileSize.TabIndex = 7;
            this.lblGeneral_MaxFileSize.Text = "Maximum thumbnail file size (MB):";
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(594, 532);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tab1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(610, 570);
            this.Name = "frmSetting";
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSetting_FormClosing);
            this.Load += new System.EventHandler(this.frmSetting_Load);
            this.SizeChanged += new System.EventHandler(this.frmSetting_SizeChanged);
            this.tab1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThumbSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barInterval)).EndInit();
            this.tabContextMenu.ResumeLayout(false);
            this.tabContextMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabLanguage.ResumeLayout(false);
            this.tabLanguage.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ImageList imglTheme;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabControl tab1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabContextMenu;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.Label lblContextMenu;
        private System.Windows.Forms.Label lblGeneral;
        private System.Windows.Forms.CheckBox chkLockWorkspace;
        private System.Windows.Forms.TrackBar barInterval;
        private System.Windows.Forms.Label lblSlideshowInterval;
        private System.Windows.Forms.Label lblGeneral_ZoomOptimization;
        private System.Windows.Forms.ComboBox cmbZoomOptimization;
        private System.Windows.Forms.CheckBox chkFindChildFolder;
        private System.Windows.Forms.CheckBox chkAutoUpdate;
        private System.Windows.Forms.Label lbl_ContextMenu_Description;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblRemoveAllContextMenu;
        private System.Windows.Forms.Label lblAddDefaultContextMenu;
        private System.Windows.Forms.TabPage tabLanguage;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.Label lblLanguageText;
        private System.Windows.Forms.NumericUpDown numMaxThumbSize;
        private System.Windows.Forms.Label lblImageLoadingOrder;
        private System.Windows.Forms.ComboBox cmbImageOrder;
        private System.Windows.Forms.LinkLabel lnkGetMoreLanguage;
        private System.Windows.Forms.CheckBox chkWelcomePicture;
        private System.Windows.Forms.PictureBox picBackgroundColor;
        private System.Windows.Forms.Label lblBackGroundColor;
        private System.Windows.Forms.ToolTip tip1;
        private System.Windows.Forms.Label lblExtensions;
        private System.Windows.Forms.TextBox txtExtensions;
        private System.Windows.Forms.Label lblUpdateContextMenu;
        private System.Windows.Forms.CheckBox chkHideToolBar;
        private System.Windows.Forms.LinkLabel lnkEdit;
        private System.Windows.Forms.LinkLabel lnkCreateNew;
        private System.Windows.Forms.LinkLabel lnkRefresh;
        private System.Windows.Forms.Label lblGeneral_MaxFileSize;

    }
}