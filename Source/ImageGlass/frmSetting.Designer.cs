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
            this.btnClose = new System.Windows.Forms.Button();
            this.imglTheme = new System.Windows.Forms.ImageList(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.tab1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbImageOrder = new System.Windows.Forms.ComboBox();
            this.numMaxThumbSize = new System.Windows.Forms.NumericUpDown();
            this.lblGeneral_MaxFileSize = new System.Windows.Forms.Label();
            this.barInterval = new System.Windows.Forms.TrackBar();
            this.lblSlideshowInterval = new System.Windows.Forms.Label();
            this.lblGeneral_ZoomOptimization = new System.Windows.Forms.Label();
            this.cmbZoomOptimization = new System.Windows.Forms.ComboBox();
            this.chkFindChildFolder = new System.Windows.Forms.CheckBox();
            this.chkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.chkLockWorkspace = new System.Windows.Forms.CheckBox();
            this.tabContextMenu = new System.Windows.Forms.TabPage();
            this.lblRemoveContextMenu = new System.Windows.Forms.Label();
            this.lblAddContextMenu = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lbl_ContextMenu_Description = new System.Windows.Forms.Label();
            this.tabLaguage = new System.Windows.Forms.TabPage();
            this.lnkGetMoreLanguage = new System.Windows.Forms.LinkLabel();
            this.lblLanguageEdit = new System.Windows.Forms.Label();
            this.lblLanguageCreateNew = new System.Windows.Forms.Label();
            this.lblLanguageRefresh = new System.Windows.Forms.Label();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.lblLanguage_Text = new System.Windows.Forms.Label();
            this.tabExtension = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblExtension = new System.Windows.Forms.Label();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.lblContextMenu = new System.Windows.Forms.Label();
            this.lblGeneral = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.tab1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThumbSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barInterval)).BeginInit();
            this.tabContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabLaguage.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(501, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 28);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // imglTheme
            // 
            this.imglTheme.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imglTheme.ImageSize = new System.Drawing.Size(32, 32);
            this.imglTheme.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 371);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(595, 42);
            this.panel1.TabIndex = 10;
            // 
            // tab1
            // 
            this.tab1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tab1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tab1.Controls.Add(this.tabGeneral);
            this.tab1.Controls.Add(this.tabContextMenu);
            this.tab1.Controls.Add(this.tabLaguage);
            this.tab1.Controls.Add(this.tabExtension);
            this.tab1.Location = new System.Drawing.Point(-10, 53);
            this.tab1.Multiline = true;
            this.tab1.Name = "tab1";
            this.tab1.SelectedIndex = 0;
            this.tab1.Size = new System.Drawing.Size(613, 357);
            this.tab1.TabIndex = 15;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.label1);
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
            this.tabGeneral.Size = new System.Drawing.Size(605, 331);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "general";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 240);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 15);
            this.label1.TabIndex = 10;
            this.label1.Text = "Image loading order:";
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
            "Extension"});
            this.cmbImageOrder.Location = new System.Drawing.Point(23, 258);
            this.cmbImageOrder.Name = "cmbImageOrder";
            this.cmbImageOrder.Size = new System.Drawing.Size(279, 23);
            this.cmbImageOrder.TabIndex = 9;
            this.cmbImageOrder.SelectedIndexChanged += new System.EventHandler(this.cmbImageOrder_SelectedIndexChanged);
            // 
            // numMaxThumbSize
            // 
            this.numMaxThumbSize.Location = new System.Drawing.Point(218, 206);
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
            this.numMaxThumbSize.TabIndex = 8;
            this.numMaxThumbSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMaxThumbSize.ThousandsSeparator = true;
            this.numMaxThumbSize.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numMaxThumbSize.ValueChanged += new System.EventHandler(this.numMaxThumbSize_ValueChanged);
            // 
            // lblGeneral_MaxFileSize
            // 
            this.lblGeneral_MaxFileSize.AutoSize = true;
            this.lblGeneral_MaxFileSize.Location = new System.Drawing.Point(20, 208);
            this.lblGeneral_MaxFileSize.Name = "lblGeneral_MaxFileSize";
            this.lblGeneral_MaxFileSize.Size = new System.Drawing.Size(192, 15);
            this.lblGeneral_MaxFileSize.TabIndex = 7;
            this.lblGeneral_MaxFileSize.Text = "Maximum thumbnail file size (MB):";
            // 
            // barInterval
            // 
            this.barInterval.BackColor = System.Drawing.Color.White;
            this.barInterval.Location = new System.Drawing.Point(23, 178);
            this.barInterval.Maximum = 30;
            this.barInterval.Minimum = 1;
            this.barInterval.Name = "barInterval";
            this.barInterval.Size = new System.Drawing.Size(279, 45);
            this.barInterval.TabIndex = 6;
            this.barInterval.TickStyle = System.Windows.Forms.TickStyle.None;
            this.barInterval.Value = 5;
            this.barInterval.Scroll += new System.EventHandler(this.barInterval_Scroll);
            // 
            // lblSlideshowInterval
            // 
            this.lblSlideshowInterval.AutoSize = true;
            this.lblSlideshowInterval.Location = new System.Drawing.Point(20, 160);
            this.lblSlideshowInterval.Name = "lblSlideshowInterval";
            this.lblSlideshowInterval.Size = new System.Drawing.Size(117, 15);
            this.lblSlideshowInterval.TabIndex = 5;
            this.lblSlideshowInterval.Text = "Slide show interval: 5";
            // 
            // lblGeneral_ZoomOptimization
            // 
            this.lblGeneral_ZoomOptimization.AutoSize = true;
            this.lblGeneral_ZoomOptimization.Location = new System.Drawing.Point(20, 106);
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
            this.cmbZoomOptimization.Location = new System.Drawing.Point(23, 124);
            this.cmbZoomOptimization.Name = "cmbZoomOptimization";
            this.cmbZoomOptimization.Size = new System.Drawing.Size(279, 23);
            this.cmbZoomOptimization.TabIndex = 3;
            this.cmbZoomOptimization.SelectedIndexChanged += new System.EventHandler(this.cmbZoomOptimization_SelectedIndexChanged);
            // 
            // chkFindChildFolder
            // 
            this.chkFindChildFolder.AutoSize = true;
            this.chkFindChildFolder.Location = new System.Drawing.Point(23, 74);
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
            this.tabContextMenu.Controls.Add(this.lblRemoveContextMenu);
            this.tabContextMenu.Controls.Add(this.lblAddContextMenu);
            this.tabContextMenu.Controls.Add(this.pictureBox1);
            this.tabContextMenu.Controls.Add(this.lbl_ContextMenu_Description);
            this.tabContextMenu.Location = new System.Drawing.Point(4, 4);
            this.tabContextMenu.Name = "tabContextMenu";
            this.tabContextMenu.Padding = new System.Windows.Forms.Padding(3);
            this.tabContextMenu.Size = new System.Drawing.Size(605, 331);
            this.tabContextMenu.TabIndex = 1;
            this.tabContextMenu.Text = "context menu";
            this.tabContextMenu.UseVisualStyleBackColor = true;
            // 
            // lblRemoveContextMenu
            // 
            this.lblRemoveContextMenu.AutoSize = true;
            this.lblRemoveContextMenu.BackColor = System.Drawing.Color.Silver;
            this.lblRemoveContextMenu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblRemoveContextMenu.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRemoveContextMenu.ForeColor = System.Drawing.Color.White;
            this.lblRemoveContextMenu.Location = new System.Drawing.Point(320, 213);
            this.lblRemoveContextMenu.Name = "lblRemoveContextMenu";
            this.lblRemoveContextMenu.Padding = new System.Windows.Forms.Padding(5);
            this.lblRemoveContextMenu.Size = new System.Drawing.Size(60, 25);
            this.lblRemoveContextMenu.TabIndex = 17;
            this.lblRemoveContextMenu.Tag = "1";
            this.lblRemoveContextMenu.Text = "Remove";
            this.lblRemoveContextMenu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRemoveContextMenu.Click += new System.EventHandler(this.lblRemoveContextMenu_Click);
            this.lblRemoveContextMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseDown);
            this.lblRemoveContextMenu.MouseEnter += new System.EventHandler(this.lblButton_MouseEnter);
            this.lblRemoveContextMenu.MouseLeave += new System.EventHandler(this.lblButton_MouseLeave);
            this.lblRemoveContextMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseUp);
            // 
            // lblAddContextMenu
            // 
            this.lblAddContextMenu.AutoSize = true;
            this.lblAddContextMenu.BackColor = System.Drawing.Color.Silver;
            this.lblAddContextMenu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblAddContextMenu.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAddContextMenu.ForeColor = System.Drawing.Color.White;
            this.lblAddContextMenu.Location = new System.Drawing.Point(320, 173);
            this.lblAddContextMenu.Name = "lblAddContextMenu";
            this.lblAddContextMenu.Padding = new System.Windows.Forms.Padding(5);
            this.lblAddContextMenu.Size = new System.Drawing.Size(39, 25);
            this.lblAddContextMenu.TabIndex = 16;
            this.lblAddContextMenu.Tag = "1";
            this.lblAddContextMenu.Text = "Add";
            this.lblAddContextMenu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblAddContextMenu.Click += new System.EventHandler(this.lblAddContextMenu_Click);
            this.lblAddContextMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseDown);
            this.lblAddContextMenu.MouseEnter += new System.EventHandler(this.lblButton_MouseEnter);
            this.lblAddContextMenu.MouseLeave += new System.EventHandler(this.lblButton_MouseLeave);
            this.lblAddContextMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseUp);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(21, 103);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(283, 167);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // lbl_ContextMenu_Description
            // 
            this.lbl_ContextMenu_Description.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_ContextMenu_Description.Location = new System.Drawing.Point(18, 18);
            this.lbl_ContextMenu_Description.Name = "lbl_ContextMenu_Description";
            this.lbl_ContextMenu_Description.Size = new System.Drawing.Size(569, 82);
            this.lbl_ContextMenu_Description.TabIndex = 0;
            this.lbl_ContextMenu_Description.Text = "This feature helps you open an image quickly by context menu. To enable it, press" +
    " Add button. If you want to disable, press Remove button.\r\n\r\nAdd shortcut \'Open " +
    "with ImageGlass\' to context menu";
            // 
            // tabLaguage
            // 
            this.tabLaguage.Controls.Add(this.lnkGetMoreLanguage);
            this.tabLaguage.Controls.Add(this.lblLanguageEdit);
            this.tabLaguage.Controls.Add(this.lblLanguageCreateNew);
            this.tabLaguage.Controls.Add(this.lblLanguageRefresh);
            this.tabLaguage.Controls.Add(this.cmbLanguage);
            this.tabLaguage.Controls.Add(this.lblLanguage_Text);
            this.tabLaguage.Location = new System.Drawing.Point(4, 4);
            this.tabLaguage.Name = "tabLaguage";
            this.tabLaguage.Padding = new System.Windows.Forms.Padding(3);
            this.tabLaguage.Size = new System.Drawing.Size(605, 331);
            this.tabLaguage.TabIndex = 2;
            this.tabLaguage.Text = "language";
            this.tabLaguage.UseVisualStyleBackColor = true;
            // 
            // lnkGetMoreLanguage
            // 
            this.lnkGetMoreLanguage.AutoSize = true;
            this.lnkGetMoreLanguage.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkGetMoreLanguage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkGetMoreLanguage.Location = new System.Drawing.Point(19, 279);
            this.lnkGetMoreLanguage.Name = "lnkGetMoreLanguage";
            this.lnkGetMoreLanguage.Size = new System.Drawing.Size(124, 15);
            this.lnkGetMoreLanguage.TabIndex = 20;
            this.lnkGetMoreLanguage.TabStop = true;
            this.lnkGetMoreLanguage.Text = "> Get more languages";
            this.lnkGetMoreLanguage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGetMoreLanguage_LinkClicked);
            // 
            // lblLanguageEdit
            // 
            this.lblLanguageEdit.AutoSize = true;
            this.lblLanguageEdit.BackColor = System.Drawing.Color.Silver;
            this.lblLanguageEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLanguageEdit.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLanguageEdit.ForeColor = System.Drawing.Color.White;
            this.lblLanguageEdit.Location = new System.Drawing.Point(278, 124);
            this.lblLanguageEdit.Name = "lblLanguageEdit";
            this.lblLanguageEdit.Padding = new System.Windows.Forms.Padding(5);
            this.lblLanguageEdit.Size = new System.Drawing.Size(163, 25);
            this.lblLanguageEdit.TabIndex = 19;
            this.lblLanguageEdit.Tag = "1";
            this.lblLanguageEdit.Text = "Edit selected language pack";
            this.lblLanguageEdit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLanguageEdit.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseDown);
            this.lblLanguageEdit.MouseEnter += new System.EventHandler(this.lblButton_MouseEnter);
            this.lblLanguageEdit.MouseLeave += new System.EventHandler(this.lblButton_MouseLeave);
            this.lblLanguageEdit.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseUp);
            // 
            // lblLanguageCreateNew
            // 
            this.lblLanguageCreateNew.AutoSize = true;
            this.lblLanguageCreateNew.BackColor = System.Drawing.Color.Silver;
            this.lblLanguageCreateNew.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLanguageCreateNew.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLanguageCreateNew.ForeColor = System.Drawing.Color.White;
            this.lblLanguageCreateNew.Location = new System.Drawing.Point(278, 87);
            this.lblLanguageCreateNew.Name = "lblLanguageCreateNew";
            this.lblLanguageCreateNew.Padding = new System.Windows.Forms.Padding(5);
            this.lblLanguageCreateNew.Size = new System.Drawing.Size(156, 25);
            this.lblLanguageCreateNew.TabIndex = 18;
            this.lblLanguageCreateNew.Tag = "1";
            this.lblLanguageCreateNew.Text = "Create new language pack";
            this.lblLanguageCreateNew.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLanguageCreateNew.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseDown);
            this.lblLanguageCreateNew.MouseEnter += new System.EventHandler(this.lblButton_MouseEnter);
            this.lblLanguageCreateNew.MouseLeave += new System.EventHandler(this.lblButton_MouseLeave);
            this.lblLanguageCreateNew.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseUp);
            // 
            // lblLanguageRefresh
            // 
            this.lblLanguageRefresh.AutoSize = true;
            this.lblLanguageRefresh.BackColor = System.Drawing.Color.Silver;
            this.lblLanguageRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLanguageRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLanguageRefresh.ForeColor = System.Drawing.Color.White;
            this.lblLanguageRefresh.Location = new System.Drawing.Point(278, 50);
            this.lblLanguageRefresh.Name = "lblLanguageRefresh";
            this.lblLanguageRefresh.Padding = new System.Windows.Forms.Padding(5);
            this.lblLanguageRefresh.Size = new System.Drawing.Size(56, 25);
            this.lblLanguageRefresh.TabIndex = 17;
            this.lblLanguageRefresh.Tag = "1";
            this.lblLanguageRefresh.Text = "Refresh";
            this.lblLanguageRefresh.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLanguageRefresh.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseDown);
            this.lblLanguageRefresh.MouseEnter += new System.EventHandler(this.lblButton_MouseEnter);
            this.lblLanguageRefresh.MouseLeave += new System.EventHandler(this.lblButton_MouseLeave);
            this.lblLanguageRefresh.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblButton_MouseUp);
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
            this.cmbLanguage.Size = new System.Drawing.Size(249, 23);
            this.cmbLanguage.TabIndex = 2;
            // 
            // lblLanguage_Text
            // 
            this.lblLanguage_Text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLanguage_Text.AutoSize = true;
            this.lblLanguage_Text.Location = new System.Drawing.Point(20, 30);
            this.lblLanguage_Text.Name = "lblLanguage_Text";
            this.lblLanguage_Text.Size = new System.Drawing.Size(114, 15);
            this.lblLanguage_Text.TabIndex = 1;
            this.lblLanguage_Text.Text = "Supported language";
            // 
            // tabExtension
            // 
            this.tabExtension.Location = new System.Drawing.Point(4, 4);
            this.tabExtension.Name = "tabExtension";
            this.tabExtension.Padding = new System.Windows.Forms.Padding(3);
            this.tabExtension.Size = new System.Drawing.Size(605, 331);
            this.tabExtension.TabIndex = 3;
            this.tabExtension.Text = "extension";
            this.tabExtension.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblExtension);
            this.panel2.Controls.Add(this.lblLanguage);
            this.panel2.Controls.Add(this.lblContextMenu);
            this.panel2.Controls.Add(this.lblGeneral);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(595, 62);
            this.panel2.TabIndex = 16;
            // 
            // lblExtension
            // 
            this.lblExtension.BackColor = System.Drawing.Color.Silver;
            this.lblExtension.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblExtension.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblExtension.ForeColor = System.Drawing.Color.White;
            this.lblExtension.Location = new System.Drawing.Point(447, 9);
            this.lblExtension.Name = "lblExtension";
            this.lblExtension.Size = new System.Drawing.Size(136, 40);
            this.lblExtension.TabIndex = 18;
            this.lblExtension.Tag = "0";
            this.lblExtension.Text = "Extension";
            this.lblExtension.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblExtension.Visible = false;
            this.lblExtension.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblExtension.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblExtension.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblExtension.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblExtension.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
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
            this.lblLanguage.Visible = false;
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
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(595, 413);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tab1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(605, 410);
            this.Name = "frmSetting";
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSetting_FormClosing);
            this.Load += new System.EventHandler(this.frmSetting_Load);
            this.SizeChanged += new System.EventHandler(this.frmSetting_SizeChanged);
            this.panel1.ResumeLayout(false);
            this.tab1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxThumbSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barInterval)).EndInit();
            this.tabContextMenu.ResumeLayout(false);
            this.tabContextMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabLaguage.ResumeLayout(false);
            this.tabLaguage.PerformLayout();
            this.panel2.ResumeLayout(false);
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
        private System.Windows.Forms.Label lblExtension;
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
        private System.Windows.Forms.Label lblRemoveContextMenu;
        private System.Windows.Forms.Label lblAddContextMenu;
        private System.Windows.Forms.TabPage tabLaguage;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.Label lblLanguage_Text;
        private System.Windows.Forms.Label lblLanguageRefresh;
        private System.Windows.Forms.Label lblLanguageCreateNew;
        private System.Windows.Forms.Label lblLanguageEdit;
        private System.Windows.Forms.TabPage tabExtension;
        private System.Windows.Forms.NumericUpDown numMaxThumbSize;
        private System.Windows.Forms.Label lblGeneral_MaxFileSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbImageOrder;
        private System.Windows.Forms.LinkLabel lnkGetMoreLanguage;

    }
}