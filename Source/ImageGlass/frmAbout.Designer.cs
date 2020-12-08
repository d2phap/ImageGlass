namespace ImageGlass
{
    partial class frmAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAbout));
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblAppName = new System.Windows.Forms.Label();
            this.lblSlogant = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblSoftwareUpdate = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.sp0 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblReferences = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblComponent = new System.Windows.Forms.Label();
            this.tab1 = new ImageGlass.UI.NakedTabControl();
            this.tpInfo = new System.Windows.Forms.TabPage();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lnkCollaborator = new System.Windows.Forms.LinkLabel();
            this.lnkLogoDesigner = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lnkCheckUpdate = new System.Windows.Forms.LinkLabel();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.lblUpdate = new System.Windows.Forms.Label();
            this.lblInfoContact = new System.Windows.Forms.Label();
            this.lnkFacebook = new System.Windows.Forms.LinkLabel();
            this.lnkEmail = new System.Windows.Forms.LinkLabel();
            this.lnkProjectPage = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.lnkIGHomepage = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.tpComponents = new System.Windows.Forms.TabPage();
            this.txtComponents = new System.Windows.Forms.RichTextBox();
            this.tpReferences = new System.Windows.Forms.TabPage();
            this.txtReferences = new System.Windows.Forms.RichTextBox();
            this.btnDonation = new System.Windows.Forms.PictureBox();
            this.panHeader = new System.Windows.Forms.Panel();
            this.lblCodeName = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sp0)).BeginInit();
            this.sp0.Panel1.SuspendLayout();
            this.sp0.Panel2.SuspendLayout();
            this.sp0.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tab1.SuspendLayout();
            this.tpInfo.SuspendLayout();
            this.tpComponents.SuspendLayout();
            this.tpReferences.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnDonation)).BeginInit();
            this.panHeader.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // picLogo
            // 
            this.picLogo.BackColor = System.Drawing.Color.Transparent;
            this.picLogo.Image = ((System.Drawing.Image)(resources.GetObject("picLogo.Image")));
            this.picLogo.Location = new System.Drawing.Point(23, 22);
            this.picLogo.Margin = new System.Windows.Forms.Padding(4);
            this.picLogo.MaximumSize = new System.Drawing.Size(89, 89);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(89, 89);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            // 
            // lblAppName
            // 
            this.lblAppName.AutoSize = true;
            this.lblAppName.BackColor = System.Drawing.Color.Transparent;
            this.lblAppName.Font = new System.Drawing.Font("Segoe UI", 20F);
            this.lblAppName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(111)))), ((int)(((byte)(185)))));
            this.lblAppName.Location = new System.Drawing.Point(115, 22);
            this.lblAppName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAppName.Name = "lblAppName";
            this.lblAppName.Size = new System.Drawing.Size(212, 51);
            this.lblAppName.TabIndex = 7;
            this.lblAppName.Text = "ImageGlass";
            // 
            // lblSlogant
            // 
            this.lblSlogant.AutoSize = true;
            this.lblSlogant.BackColor = System.Drawing.Color.Transparent;
            this.lblSlogant.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblSlogant.Location = new System.Drawing.Point(120, 74);
            this.lblSlogant.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSlogant.Name = "lblSlogant";
            this.lblSlogant.Size = new System.Drawing.Size(351, 30);
            this.lblSlogant.TabIndex = 8;
            this.lblSlogant.Text = "A lightweight, versatile image viewer";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Italic);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(117)))), ((int)(((byte)(117)))));
            this.label5.Location = new System.Drawing.Point(82, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(232, 20);
            this.label5.TabIndex = 8;
            this.label5.Text = "Free and open source image viewer";
            // 
            // lblSoftwareUpdate
            // 
            this.lblSoftwareUpdate.AutoSize = true;
            this.lblSoftwareUpdate.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Underline);
            this.lblSoftwareUpdate.Location = new System.Drawing.Point(19, 310);
            this.lblSoftwareUpdate.Name = "lblSoftwareUpdate";
            this.lblSoftwareUpdate.Size = new System.Drawing.Size(118, 19);
            this.lblSoftwareUpdate.TabIndex = 13;
            this.lblSoftwareUpdate.Text = "Software updates:";
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(551, -41);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(92, 34);
            this.btnClose.TabIndex = 17;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // sp0
            // 
            this.sp0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.sp0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sp0.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.sp0.Location = new System.Drawing.Point(0, 140);
            this.sp0.Margin = new System.Windows.Forms.Padding(0);
            this.sp0.Name = "sp0";
            // 
            // sp0.Panel1
            // 
            this.sp0.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(165)))), ((int)(((byte)(168)))));
            this.sp0.Panel1.Controls.Add(this.tableLayoutPanel1);
            this.sp0.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // sp0.Panel2
            // 
            this.sp0.Panel2.Controls.Add(this.tab1);
            this.sp0.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.sp0.Size = new System.Drawing.Size(910, 508);
            this.sp0.SplitterDistance = 250;
            this.sp0.TabIndex = 19;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblReferences, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblInfo, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblComponent, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(250, 508);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lblReferences
            // 
            this.lblReferences.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblReferences.BackColor = System.Drawing.Color.Transparent;
            this.lblReferences.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblReferences.ForeColor = System.Drawing.Color.Black;
            this.lblReferences.Location = new System.Drawing.Point(0, 94);
            this.lblReferences.Margin = new System.Windows.Forms.Padding(0);
            this.lblReferences.Name = "lblReferences";
            this.lblReferences.Padding = new System.Windows.Forms.Padding(14, 0, 14, 0);
            this.lblReferences.Size = new System.Drawing.Size(250, 47);
            this.lblReferences.TabIndex = 20;
            this.lblReferences.Tag = "3";
            this.lblReferences.Text = "References";
            this.lblReferences.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblReferences.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblReferences.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblReferences.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblReferences.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblReferences.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.lblInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblInfo.ForeColor = System.Drawing.Color.Black;
            this.lblInfo.Location = new System.Drawing.Point(0, 0);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Padding = new System.Windows.Forms.Padding(14, 0, 14, 0);
            this.lblInfo.Size = new System.Drawing.Size(250, 47);
            this.lblInfo.TabIndex = 18;
            this.lblInfo.Tag = "1";
            this.lblInfo.Text = "Info";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblInfo.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblInfo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblInfo.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblInfo.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblInfo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // lblComponent
            // 
            this.lblComponent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblComponent.BackColor = System.Drawing.Color.Transparent;
            this.lblComponent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblComponent.ForeColor = System.Drawing.Color.Black;
            this.lblComponent.Location = new System.Drawing.Point(0, 47);
            this.lblComponent.Margin = new System.Windows.Forms.Padding(0);
            this.lblComponent.Name = "lblComponent";
            this.lblComponent.Padding = new System.Windows.Forms.Padding(14, 0, 14, 0);
            this.lblComponent.Size = new System.Drawing.Size(250, 47);
            this.lblComponent.TabIndex = 19;
            this.lblComponent.Tag = "2";
            this.lblComponent.Text = "Components";
            this.lblComponent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblComponent.Click += new System.EventHandler(this.lblMenu_Click);
            this.lblComponent.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseDown);
            this.lblComponent.MouseEnter += new System.EventHandler(this.lblMenu_MouseEnter);
            this.lblComponent.MouseLeave += new System.EventHandler(this.lblMenu_MouseLeave);
            this.lblComponent.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblMenu_MouseUp);
            // 
            // tab1
            // 
            this.tab1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tab1.Controls.Add(this.tpInfo);
            this.tab1.Controls.Add(this.tpComponents);
            this.tab1.Controls.Add(this.tpReferences);
            this.tab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tab1.Location = new System.Drawing.Point(0, 0);
            this.tab1.Margin = new System.Windows.Forms.Padding(0);
            this.tab1.Multiline = true;
            this.tab1.Name = "tab1";
            this.tab1.Padding = new System.Drawing.Point(0, 0);
            this.tab1.SelectedIndex = 0;
            this.tab1.Size = new System.Drawing.Size(656, 508);
            this.tab1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tab1.TabIndex = 0;
            this.tab1.SelectedIndexChanged += new System.EventHandler(this.tab1_SelectedIndexChanged);
            // 
            // tpInfo
            // 
            this.tpInfo.AutoScroll = true;
            this.tpInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tpInfo.Controls.Add(this.linkLabel1);
            this.tpInfo.Controls.Add(this.panel2);
            this.tpInfo.Controls.Add(this.lnkCollaborator);
            this.tpInfo.Controls.Add(this.lnkLogoDesigner);
            this.tpInfo.Controls.Add(this.lblVersion);
            this.tpInfo.Controls.Add(this.lnkCheckUpdate);
            this.tpInfo.Controls.Add(this.lblCopyright);
            this.tpInfo.Controls.Add(this.lblUpdate);
            this.tpInfo.Controls.Add(this.lblInfoContact);
            this.tpInfo.Controls.Add(this.lnkFacebook);
            this.tpInfo.Controls.Add(this.lnkEmail);
            this.tpInfo.Controls.Add(this.lnkProjectPage);
            this.tpInfo.Controls.Add(this.linkLabel2);
            this.tpInfo.Controls.Add(this.lnkIGHomepage);
            this.tpInfo.Controls.Add(this.label2);
            this.tpInfo.Location = new System.Drawing.Point(4, 35);
            this.tpInfo.Margin = new System.Windows.Forms.Padding(0);
            this.tpInfo.Name = "tpInfo";
            this.tpInfo.Padding = new System.Windows.Forms.Padding(0, 19, 0, 0);
            this.tpInfo.Size = new System.Drawing.Size(648, 469);
            this.tpInfo.TabIndex = 0;
            this.tpInfo.Text = "tpInfo";
            // 
            // linkLabel1
            // 
            this.linkLabel1.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.linkLabel1.ForeColor = System.Drawing.Color.Black;
            this.linkLabel1.LinkArea = new System.Windows.Forms.LinkArea(9, 99);
            this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabel1.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.linkLabel1.Location = new System.Drawing.Point(54, 394);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(291, 28);
            this.linkLabel1.TabIndex = 8;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Patreon: https://patreon.com/d2phap";
            this.linkLabel1.UseCompatibleTextRendering = true;
            this.linkLabel1.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(365, 636);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(103, 35);
            this.panel2.TabIndex = 17;
            // 
            // lnkCollaborator
            // 
            this.lnkCollaborator.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkCollaborator.AutoSize = true;
            this.lnkCollaborator.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lnkCollaborator.ForeColor = System.Drawing.Color.Black;
            this.lnkCollaborator.LinkArea = new System.Windows.Forms.LinkArea(30, 9);
            this.lnkCollaborator.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkCollaborator.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkCollaborator.Location = new System.Drawing.Point(26, 159);
            this.lnkCollaborator.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkCollaborator.Name = "lnkCollaborator";
            this.lnkCollaborator.Size = new System.Drawing.Size(312, 28);
            this.lnkCollaborator.TabIndex = 4;
            this.lnkCollaborator.TabStop = true;
            this.lnkCollaborator.Text = "Collaborator: Kevin Routley (@fire-eggs)";
            this.lnkCollaborator.UseCompatibleTextRendering = true;
            this.lnkCollaborator.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(151)))), ((int)(((byte)(255)))));
            this.lnkCollaborator.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCollaborator_LinkClicked);
            // 
            // lnkLogoDesigner
            // 
            this.lnkLogoDesigner.AutoSize = true;
            this.lnkLogoDesigner.ForeColor = System.Drawing.Color.Black;
            this.lnkLogoDesigner.Location = new System.Drawing.Point(26, 129);
            this.lnkLogoDesigner.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkLogoDesigner.Name = "lnkLogoDesigner";
            this.lnkLogoDesigner.Size = new System.Drawing.Size(276, 23);
            this.lnkLogoDesigner.TabIndex = 16;
            this.lnkLogoDesigner.Text = "Logo designer: Nguyễn Quốc Tuấn";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblVersion.ForeColor = System.Drawing.Color.Black;
            this.lblVersion.Location = new System.Drawing.Point(24, 22);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(117, 23);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "Version: [xxxx]";
            // 
            // lnkCheckUpdate
            // 
            this.lnkCheckUpdate.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkCheckUpdate.AutoSize = true;
            this.lnkCheckUpdate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lnkCheckUpdate.ForeColor = System.Drawing.Color.Black;
            this.lnkCheckUpdate.LinkArea = new System.Windows.Forms.LinkArea(0, 99);
            this.lnkCheckUpdate.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkCheckUpdate.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkCheckUpdate.Location = new System.Drawing.Point(54, 563);
            this.lnkCheckUpdate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkCheckUpdate.Name = "lnkCheckUpdate";
            this.lnkCheckUpdate.Size = new System.Drawing.Size(150, 28);
            this.lnkCheckUpdate.TabIndex = 11;
            this.lnkCheckUpdate.TabStop = true;
            this.lnkCheckUpdate.Text = "» Check for update";
            this.lnkCheckUpdate.UseCompatibleTextRendering = true;
            this.lnkCheckUpdate.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkCheckUpdate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCheckUpdate_LinkClicked);
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCopyright.ForeColor = System.Drawing.Color.Black;
            this.lblCopyright.Location = new System.Drawing.Point(24, 60);
            this.lblCopyright.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(359, 46);
            this.lblCopyright.TabIndex = 1;
            this.lblCopyright.Text = "Copyright © 2010-[xxxx] by Dương Diệu Pháp\r\nAll rights reserved.";
            // 
            // lblUpdate
            // 
            this.lblUpdate.AutoSize = true;
            this.lblUpdate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUpdate.ForeColor = System.Drawing.Color.Black;
            this.lblUpdate.Location = new System.Drawing.Point(24, 524);
            this.lblUpdate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUpdate.Name = "lblUpdate";
            this.lblUpdate.Size = new System.Drawing.Size(157, 23);
            this.lblUpdate.TabIndex = 13;
            this.lblUpdate.Text = "Software updates:";
            // 
            // lblInfoContact
            // 
            this.lblInfoContact.AutoSize = true;
            this.lblInfoContact.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblInfoContact.ForeColor = System.Drawing.Color.Black;
            this.lblInfoContact.Location = new System.Drawing.Point(24, 205);
            this.lblInfoContact.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblInfoContact.Name = "lblInfoContact";
            this.lblInfoContact.Size = new System.Drawing.Size(77, 23);
            this.lblInfoContact.TabIndex = 2;
            this.lblInfoContact.Text = "Contact:";
            // 
            // lnkFacebook
            // 
            this.lnkFacebook.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkFacebook.AutoSize = true;
            this.lnkFacebook.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lnkFacebook.ForeColor = System.Drawing.Color.Black;
            this.lnkFacebook.LinkArea = new System.Windows.Forms.LinkArea(10, 99);
            this.lnkFacebook.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkFacebook.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkFacebook.Location = new System.Drawing.Point(54, 464);
            this.lnkFacebook.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkFacebook.Name = "lnkFacebook";
            this.lnkFacebook.Size = new System.Drawing.Size(385, 28);
            this.lnkFacebook.TabIndex = 10;
            this.lnkFacebook.TabStop = true;
            this.lnkFacebook.Text = "Facebook: https://www.facebook.com/ImageGlass";
            this.lnkFacebook.UseCompatibleTextRendering = true;
            this.lnkFacebook.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkFacebook.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFacebook_LinkClicked);
            // 
            // lnkEmail
            // 
            this.lnkEmail.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkEmail.AutoSize = true;
            this.lnkEmail.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lnkEmail.ForeColor = System.Drawing.Color.Black;
            this.lnkEmail.LinkArea = new System.Windows.Forms.LinkArea(7, 23);
            this.lnkEmail.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkEmail.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkEmail.Location = new System.Drawing.Point(54, 244);
            this.lnkEmail.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkEmail.Name = "lnkEmail";
            this.lnkEmail.Size = new System.Drawing.Size(225, 28);
            this.lnkEmail.TabIndex = 5;
            this.lnkEmail.TabStop = true;
            this.lnkEmail.Text = "Email: phap@imageglass.org";
            this.lnkEmail.UseCompatibleTextRendering = true;
            this.lnkEmail.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkEmail.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkEmail_LinkClicked);
            // 
            // lnkProjectPage
            // 
            this.lnkProjectPage.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkProjectPage.AutoSize = true;
            this.lnkProjectPage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lnkProjectPage.ForeColor = System.Drawing.Color.Black;
            this.lnkProjectPage.LinkArea = new System.Windows.Forms.LinkArea(8, 99);
            this.lnkProjectPage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkProjectPage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkProjectPage.Location = new System.Drawing.Point(54, 431);
            this.lnkProjectPage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkProjectPage.Name = "lnkProjectPage";
            this.lnkProjectPage.Size = new System.Drawing.Size(294, 28);
            this.lnkProjectPage.TabIndex = 9;
            this.lnkProjectPage.TabStop = true;
            this.lnkProjectPage.Text = "Source: https://imageglass.org/source";
            this.lnkProjectPage.UseCompatibleTextRendering = true;
            this.lnkProjectPage.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkProjectPage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkProjectPage_LinkClicked);
            // 
            // linkLabel2
            // 
            this.linkLabel2.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.linkLabel2.ForeColor = System.Drawing.Color.Black;
            this.linkLabel2.LinkArea = new System.Windows.Forms.LinkArea(8, 23);
            this.linkLabel2.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabel2.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.linkLabel2.Location = new System.Drawing.Point(54, 275);
            this.linkLabel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(188, 28);
            this.linkLabel2.TabIndex = 6;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Twitter: duongdieuphap";
            this.linkLabel2.UseCompatibleTextRendering = true;
            this.linkLabel2.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // lnkIGHomepage
            // 
            this.lnkIGHomepage.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkIGHomepage.AutoSize = true;
            this.lnkIGHomepage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lnkIGHomepage.ForeColor = System.Drawing.Color.Black;
            this.lnkIGHomepage.LinkArea = new System.Windows.Forms.LinkArea(10, 99);
            this.lnkIGHomepage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkIGHomepage.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkIGHomepage.Location = new System.Drawing.Point(54, 361);
            this.lnkIGHomepage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkIGHomepage.Name = "lnkIGHomepage";
            this.lnkIGHomepage.Size = new System.Drawing.Size(274, 28);
            this.lnkIGHomepage.TabIndex = 7;
            this.lnkIGHomepage.TabStop = true;
            this.lnkIGHomepage.Text = "Home page: https://imageglass.org";
            this.lnkIGHomepage.UseCompatibleTextRendering = true;
            this.lnkIGHomepage.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkIGHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkIGHomepage_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(24, 322);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 23);
            this.label2.TabIndex = 8;
            this.label2.Text = "Website: ";
            // 
            // tpComponents
            // 
            this.tpComponents.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tpComponents.Controls.Add(this.txtComponents);
            this.tpComponents.ForeColor = System.Drawing.Color.Black;
            this.tpComponents.Location = new System.Drawing.Point(4, 35);
            this.tpComponents.Margin = new System.Windows.Forms.Padding(0);
            this.tpComponents.Name = "tpComponents";
            this.tpComponents.Padding = new System.Windows.Forms.Padding(28, 0, 0, 0);
            this.tpComponents.Size = new System.Drawing.Size(648, 469);
            this.tpComponents.TabIndex = 1;
            this.tpComponents.Text = "tpComponents";
            // 
            // txtComponents
            // 
            this.txtComponents.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.txtComponents.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtComponents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtComponents.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtComponents.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.txtComponents.Location = new System.Drawing.Point(28, 0);
            this.txtComponents.Margin = new System.Windows.Forms.Padding(9);
            this.txtComponents.Name = "txtComponents";
            this.txtComponents.ReadOnly = true;
            this.txtComponents.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtComponents.ShortcutsEnabled = false;
            this.txtComponents.Size = new System.Drawing.Size(620, 469);
            this.txtComponents.TabIndex = 4;
            this.txtComponents.Text = "List of components here...";
            // 
            // tpReferences
            // 
            this.tpReferences.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.tpReferences.Controls.Add(this.txtReferences);
            this.tpReferences.Location = new System.Drawing.Point(4, 35);
            this.tpReferences.Margin = new System.Windows.Forms.Padding(0);
            this.tpReferences.Name = "tpReferences";
            this.tpReferences.Padding = new System.Windows.Forms.Padding(28, 0, 0, 0);
            this.tpReferences.Size = new System.Drawing.Size(648, 469);
            this.tpReferences.TabIndex = 2;
            this.tpReferences.Text = "tpReferences";
            // 
            // txtReferences
            // 
            this.txtReferences.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.txtReferences.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtReferences.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtReferences.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtReferences.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.txtReferences.Location = new System.Drawing.Point(28, 0);
            this.txtReferences.Margin = new System.Windows.Forms.Padding(9);
            this.txtReferences.Name = "txtReferences";
            this.txtReferences.ReadOnly = true;
            this.txtReferences.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtReferences.ShortcutsEnabled = false;
            this.txtReferences.Size = new System.Drawing.Size(620, 469);
            this.txtReferences.TabIndex = 3;
            this.txtReferences.Text = resources.GetString("txtReferences.Text");
            // 
            // btnDonation
            // 
            this.btnDonation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDonation.Cursor = System.Windows.Forms.Cursors.Help;
            this.btnDonation.Image = ((System.Drawing.Image)(resources.GetObject("btnDonation.Image")));
            this.btnDonation.Location = new System.Drawing.Point(832, 22);
            this.btnDonation.Margin = new System.Windows.Forms.Padding(4);
            this.btnDonation.Name = "btnDonation";
            this.btnDonation.Size = new System.Drawing.Size(56, 56);
            this.btnDonation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnDonation.TabIndex = 20;
            this.btnDonation.TabStop = false;
            this.btnDonation.Click += new System.EventHandler(this.btnDonation_Click);
            // 
            // panHeader
            // 
            this.panHeader.BackColor = System.Drawing.Color.Transparent;
            this.panHeader.Controls.Add(this.lblCodeName);
            this.panHeader.Controls.Add(this.picLogo);
            this.panHeader.Controls.Add(this.lblAppName);
            this.panHeader.Controls.Add(this.btnDonation);
            this.panHeader.Controls.Add(this.lblSlogant);
            this.panHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panHeader.Location = new System.Drawing.Point(0, 0);
            this.panHeader.Margin = new System.Windows.Forms.Padding(0);
            this.panHeader.MinimumSize = new System.Drawing.Size(0, 140);
            this.panHeader.Name = "panHeader";
            this.panHeader.Padding = new System.Windows.Forms.Padding(19);
            this.panHeader.Size = new System.Drawing.Size(910, 140);
            this.panHeader.TabIndex = 22;
            // 
            // lblCodeName
            // 
            this.lblCodeName.AutoSize = true;
            this.lblCodeName.BackColor = System.Drawing.Color.Transparent;
            this.lblCodeName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblCodeName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblCodeName.Location = new System.Drawing.Point(313, 29);
            this.lblCodeName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCodeName.Name = "lblCodeName";
            this.lblCodeName.Size = new System.Drawing.Size(59, 30);
            this.lblCodeName.TabIndex = 21;
            this.lblCodeName.Text = "Kobe";
            this.toolTip1.SetToolTip(this.lblCodeName, "Kobe likes maroon!");
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.sp0, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.panHeader, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(910, 648);
            this.tableLayoutPanel2.TabIndex = 23;
            // 
            // frmAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(134F, 134F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(165)))), ((int)(((byte)(168)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(910, 648);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.tableLayoutPanel2);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(930, 700);
            this.Name = "frmAbout";
            this.RightToLeftLayout = true;
            this.Text = "About";
            this.Load += new System.EventHandler(this.frmAbout_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.sp0.Panel1.ResumeLayout(false);
            this.sp0.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sp0)).EndInit();
            this.sp0.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tab1.ResumeLayout(false);
            this.tpInfo.ResumeLayout(false);
            this.tpInfo.PerformLayout();
            this.tpComponents.ResumeLayout(false);
            this.tpReferences.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnDonation)).EndInit();
            this.panHeader.ResumeLayout(false);
            this.panHeader.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel lnkEmail;
        private System.Windows.Forms.Label lblInfoContact;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel lnkIGHomepage;
        private System.Windows.Forms.LinkLabel lnkProjectPage;
        private System.Windows.Forms.LinkLabel lnkFacebook;
        private System.Windows.Forms.Label lblUpdate;
        private System.Windows.Forms.LinkLabel lnkCheckUpdate;
        private System.Windows.Forms.Label lblAppName;
        private System.Windows.Forms.Label lblSlogant;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblSoftwareUpdate;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lnkLogoDesigner;
        private System.Windows.Forms.SplitContainer sp0;
        private System.Windows.Forms.Label lblReferences;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblComponent;
        private UI.NakedTabControl tab1;
        private System.Windows.Forms.TabPage tpInfo;
        private System.Windows.Forms.TabPage tpComponents;
        private System.Windows.Forms.TabPage tpReferences;
        private System.Windows.Forms.RichTextBox txtReferences;
        private System.Windows.Forms.PictureBox btnDonation;
        private System.Windows.Forms.RichTextBox txtComponents;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.LinkLabel lnkCollaborator;
        private System.Windows.Forms.Panel panHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblCodeName;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}
