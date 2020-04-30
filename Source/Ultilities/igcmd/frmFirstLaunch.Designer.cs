namespace igcmd
{
    partial class frmFirstLaunch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFirstLaunch));
            this.panFooter = new System.Windows.Forms.Panel();
            this.lnkSkip = new System.Windows.Forms.LinkLabel();
            this.btnNextStep = new System.Windows.Forms.Button();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.tab1 = new System.Windows.Forms.TabControl();
            this.tabLanguage = new System.Windows.Forms.TabPage();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.tabTheme = new System.Windows.Forms.TabPage();
            this.cmbTheme = new System.Windows.Forms.ComboBox();
            this.lblTheme = new System.Windows.Forms.Label();
            this.tabLayoutMode = new System.Windows.Forms.TabPage();
            this.cmbLayout = new System.Windows.Forms.ComboBox();
            this.lblLayout = new System.Windows.Forms.Label();
            this.tabFileAssociation = new System.Windows.Forms.TabPage();
            this.btnSetDefaultApp = new System.Windows.Forms.Button();
            this.lblDefaultApp = new System.Windows.Forms.Label();
            this.panHeader = new System.Windows.Forms.Panel();
            this.lblStepNumber = new System.Windows.Forms.Label();
            this.panFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.tab1.SuspendLayout();
            this.tabLanguage.SuspendLayout();
            this.tabTheme.SuspendLayout();
            this.tabLayoutMode.SuspendLayout();
            this.tabFileAssociation.SuspendLayout();
            this.panHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // panFooter
            // 
            this.panFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panFooter.Controls.Add(this.lnkSkip);
            this.panFooter.Controls.Add(this.btnNextStep);
            this.panFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panFooter.Location = new System.Drawing.Point(0, 402);
            this.panFooter.Name = "panFooter";
            this.panFooter.Size = new System.Drawing.Size(524, 134);
            this.panFooter.TabIndex = 0;
            // 
            // lnkSkip
            // 
            this.lnkSkip.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkSkip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkSkip.BackColor = System.Drawing.Color.Transparent;
            this.lnkSkip.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.lnkSkip.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(143)))), ((int)(((byte)(183)))));
            this.lnkSkip.Location = new System.Drawing.Point(12, 78);
            this.lnkSkip.Name = "lnkSkip";
            this.lnkSkip.Size = new System.Drawing.Size(500, 34);
            this.lnkSkip.TabIndex = 29;
            this.lnkSkip.TabStop = true;
            this.lnkSkip.Text = "Skip and Launch ImageGlass_";
            this.lnkSkip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkSkip.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(143)))), ((int)(((byte)(183)))));
            this.lnkSkip.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSkip_LinkClicked);
            // 
            // btnNextStep
            // 
            this.btnNextStep.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnNextStep.FlatAppearance.BorderSize = 0;
            this.btnNextStep.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnNextStep.Location = new System.Drawing.Point(189, 32);
            this.btnNextStep.Name = "btnNextStep";
            this.btnNextStep.Size = new System.Drawing.Size(150, 40);
            this.btnNextStep.TabIndex = 0;
            this.btnNextStep.Text = "Next_";
            this.btnNextStep.Click += new System.EventHandler(this.btnNextStep_Click);
            // 
            // picLogo
            // 
            this.picLogo.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.picLogo.BackColor = System.Drawing.Color.Transparent;
            this.picLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picLogo.Image = ((System.Drawing.Image)(resources.GetObject("picLogo.Image")));
            this.picLogo.Location = new System.Drawing.Point(189, 44);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(150, 150);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 1;
            this.picLogo.TabStop = false;
            // 
            // tab1
            // 
            this.tab1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tab1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tab1.Controls.Add(this.tabLanguage);
            this.tab1.Controls.Add(this.tabTheme);
            this.tab1.Controls.Add(this.tabLayoutMode);
            this.tab1.Controls.Add(this.tabFileAssociation);
            this.tab1.Location = new System.Drawing.Point(-22, 213);
            this.tab1.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.tab1.Name = "tab1";
            this.tab1.Padding = new System.Drawing.Point(0, 0);
            this.tab1.SelectedIndex = 0;
            this.tab1.Size = new System.Drawing.Size(562, 214);
            this.tab1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tab1.TabIndex = 2;
            this.tab1.TabStop = false;
            this.tab1.SelectedIndexChanged += new System.EventHandler(this.tab1_SelectedIndexChanged);
            // 
            // tabLanguage
            // 
            this.tabLanguage.BackColor = System.Drawing.Color.White;
            this.tabLanguage.Controls.Add(this.cmbLanguage);
            this.tabLanguage.Controls.Add(this.lblLanguage);
            this.tabLanguage.Location = new System.Drawing.Point(4, 37);
            this.tabLanguage.Name = "tabLanguage";
            this.tabLanguage.Padding = new System.Windows.Forms.Padding(3);
            this.tabLanguage.Size = new System.Drawing.Size(554, 173);
            this.tabLanguage.TabIndex = 7;
            this.tabLanguage.Text = "Language";
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            "English"});
            this.cmbLanguage.Location = new System.Drawing.Point(158, 74);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(240, 33);
            this.cmbLanguage.TabIndex = 1;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // lblLanguage
            // 
            this.lblLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLanguage.BackColor = System.Drawing.Color.Transparent;
            this.lblLanguage.Location = new System.Drawing.Point(30, 34);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(494, 34);
            this.lblLanguage.TabIndex = 0;
            this.lblLanguage.Text = "Select Language_";
            this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabTheme
            // 
            this.tabTheme.BackColor = System.Drawing.Color.White;
            this.tabTheme.Controls.Add(this.cmbTheme);
            this.tabTheme.Controls.Add(this.lblTheme);
            this.tabTheme.Location = new System.Drawing.Point(4, 37);
            this.tabTheme.Name = "tabTheme";
            this.tabTheme.Padding = new System.Windows.Forms.Padding(3);
            this.tabTheme.Size = new System.Drawing.Size(554, 173);
            this.tabTheme.TabIndex = 9;
            this.tabTheme.Text = "Theme";
            // 
            // cmbTheme
            // 
            this.cmbTheme.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTheme.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbTheme.FormattingEnabled = true;
            this.cmbTheme.Items.AddRange(new object[] {
            "2017 (Dark)",
            "2017 (Light Gray)"});
            this.cmbTheme.Location = new System.Drawing.Point(158, 74);
            this.cmbTheme.Name = "cmbTheme";
            this.cmbTheme.Size = new System.Drawing.Size(240, 33);
            this.cmbTheme.TabIndex = 5;
            this.cmbTheme.SelectedIndexChanged += new System.EventHandler(this.cmbTheme_SelectedIndexChanged);
            // 
            // lblTheme
            // 
            this.lblTheme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTheme.BackColor = System.Drawing.Color.Transparent;
            this.lblTheme.Location = new System.Drawing.Point(30, 34);
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Size = new System.Drawing.Size(494, 34);
            this.lblTheme.TabIndex = 4;
            this.lblTheme.Text = "Select Theme_";
            this.lblTheme.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabLayoutMode
            // 
            this.tabLayoutMode.BackColor = System.Drawing.Color.White;
            this.tabLayoutMode.Controls.Add(this.cmbLayout);
            this.tabLayoutMode.Controls.Add(this.lblLayout);
            this.tabLayoutMode.Location = new System.Drawing.Point(4, 37);
            this.tabLayoutMode.Name = "tabLayoutMode";
            this.tabLayoutMode.Padding = new System.Windows.Forms.Padding(3);
            this.tabLayoutMode.Size = new System.Drawing.Size(554, 173);
            this.tabLayoutMode.TabIndex = 8;
            this.tabLayoutMode.Text = "Quick layout mode";
            // 
            // cmbLayout
            // 
            this.cmbLayout.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmbLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLayout.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbLayout.FormattingEnabled = true;
            this.cmbLayout.Items.AddRange(new object[] {
            "Standard",
            "Designer"});
            this.cmbLayout.Location = new System.Drawing.Point(158, 74);
            this.cmbLayout.Name = "cmbLayout";
            this.cmbLayout.Size = new System.Drawing.Size(240, 33);
            this.cmbLayout.TabIndex = 3;
            this.cmbLayout.SelectedIndexChanged += new System.EventHandler(this.cmbLayout_SelectedIndexChanged);
            // 
            // lblLayout
            // 
            this.lblLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLayout.BackColor = System.Drawing.Color.Transparent;
            this.lblLayout.Location = new System.Drawing.Point(30, 34);
            this.lblLayout.Name = "lblLayout";
            this.lblLayout.Size = new System.Drawing.Size(494, 34);
            this.lblLayout.TabIndex = 2;
            this.lblLayout.Text = "Select Layout_";
            this.lblLayout.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabFileAssociation
            // 
            this.tabFileAssociation.BackColor = System.Drawing.Color.White;
            this.tabFileAssociation.Controls.Add(this.btnSetDefaultApp);
            this.tabFileAssociation.Controls.Add(this.lblDefaultApp);
            this.tabFileAssociation.Location = new System.Drawing.Point(4, 37);
            this.tabFileAssociation.Name = "tabFileAssociation";
            this.tabFileAssociation.Padding = new System.Windows.Forms.Padding(3);
            this.tabFileAssociation.Size = new System.Drawing.Size(554, 173);
            this.tabFileAssociation.TabIndex = 10;
            this.tabFileAssociation.Text = "File Association";
            // 
            // btnSetDefaultApp
            // 
            this.btnSetDefaultApp.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnSetDefaultApp.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSetDefaultApp.Location = new System.Drawing.Point(207, 76);
            this.btnSetDefaultApp.Name = "btnSetDefaultApp";
            this.btnSetDefaultApp.Size = new System.Drawing.Size(150, 40);
            this.btnSetDefaultApp.TabIndex = 1;
            this.btnSetDefaultApp.Text = "Yes_";
            this.btnSetDefaultApp.UseVisualStyleBackColor = true;
            this.btnSetDefaultApp.Click += new System.EventHandler(this.btnSetDefaultApp_Click);
            // 
            // lblDefaultApp
            // 
            this.lblDefaultApp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDefaultApp.BackColor = System.Drawing.Color.Transparent;
            this.lblDefaultApp.Location = new System.Drawing.Point(34, 34);
            this.lblDefaultApp.Name = "lblDefaultApp";
            this.lblDefaultApp.Size = new System.Drawing.Size(489, 34);
            this.lblDefaultApp.TabIndex = 0;
            this.lblDefaultApp.Text = "Set ImageGlass as Default Photo Viewer?_";
            this.lblDefaultApp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panHeader
            // 
            this.panHeader.BackColor = System.Drawing.Color.White;
            this.panHeader.Controls.Add(this.lblStepNumber);
            this.panHeader.Controls.Add(this.picLogo);
            this.panHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panHeader.Location = new System.Drawing.Point(0, 0);
            this.panHeader.Name = "panHeader";
            this.panHeader.Size = new System.Drawing.Size(524, 260);
            this.panHeader.TabIndex = 1;
            // 
            // lblStepNumber
            // 
            this.lblStepNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStepNumber.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblStepNumber.Location = new System.Drawing.Point(12, 213);
            this.lblStepNumber.Name = "lblStepNumber";
            this.lblStepNumber.Size = new System.Drawing.Size(500, 34);
            this.lblStepNumber.TabIndex = 2;
            this.lblStepNumber.Text = "Step 1/4_";
            this.lblStepNumber.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmFirstLaunch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(524, 536);
            this.Controls.Add(this.panHeader);
            this.Controls.Add(this.panFooter);
            this.Controls.Add(this.tab1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(536, 566);
            this.Name = "frmFirstLaunch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "First-Launch Configurations_";
            this.Load += new System.EventHandler(this.frmFirstLaunch_Load);
            this.panFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.tab1.ResumeLayout(false);
            this.tabLanguage.ResumeLayout(false);
            this.tabTheme.ResumeLayout(false);
            this.tabLayoutMode.ResumeLayout(false);
            this.tabFileAssociation.ResumeLayout(false);
            this.panHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panFooter;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Button btnNextStep;
        private System.Windows.Forms.TabControl tab1;
        private System.Windows.Forms.TabPage tabLanguage;
        private System.Windows.Forms.TabPage tabLayoutMode;
        private System.Windows.Forms.TabPage tabTheme;
        private System.Windows.Forms.TabPage tabFileAssociation;
        private System.Windows.Forms.Panel panHeader;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.Label lblDefaultApp;
        private System.Windows.Forms.Button btnSetDefaultApp;
        private System.Windows.Forms.Label lblStepNumber;
        private System.Windows.Forms.ComboBox cmbLayout;
        private System.Windows.Forms.Label lblLayout;
        private System.Windows.Forms.ComboBox cmbTheme;
        private System.Windows.Forms.Label lblTheme;
        private System.Windows.Forms.LinkLabel lnkSkip;
    }
}