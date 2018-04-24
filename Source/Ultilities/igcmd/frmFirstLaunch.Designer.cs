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
            this.panel1 = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.btnNextStep = new System.Windows.Forms.Button();
            this.tab1 = new System.Windows.Forms.TabControl();
            this.tabLanguage = new System.Windows.Forms.TabPage();
            this.tabLayoutMode = new System.Windows.Forms.TabPage();
            this.tabTheme = new System.Windows.Forms.TabPage();
            this.tabFileAssociation = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.lblDefaultApp = new System.Windows.Forms.Label();
            this.btnSetDefaultApp = new System.Windows.Forms.Button();
            this.lblStepNumber = new System.Windows.Forms.Label();
            this.cmbLayout = new System.Windows.Forms.ComboBox();
            this.lblLayout = new System.Windows.Forms.Label();
            this.cmbTheme = new System.Windows.Forms.ComboBox();
            this.lblTheme = new System.Windows.Forms.Label();
            this.lnkSkip = new System.Windows.Forms.LinkLabel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.tab1.SuspendLayout();
            this.tabLanguage.SuspendLayout();
            this.tabLayoutMode.SuspendLayout();
            this.tabTheme.SuspendLayout();
            this.tabFileAssociation.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panel1.Controls.Add(this.lnkSkip);
            this.panel1.Controls.Add(this.btnNextStep);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 387);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(581, 134);
            this.panel1.TabIndex = 0;
            // 
            // picLogo
            // 
            this.picLogo.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.picLogo.BackColor = System.Drawing.Color.Transparent;
            this.picLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picLogo.Image = ((System.Drawing.Image)(resources.GetObject("picLogo.Image")));
            this.picLogo.Location = new System.Drawing.Point(217, 43);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(150, 150);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 1;
            this.picLogo.TabStop = false;
            // 
            // btnNextStep
            // 
            this.btnNextStep.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnNextStep.Location = new System.Drawing.Point(217, 32);
            this.btnNextStep.Name = "btnNextStep";
            this.btnNextStep.Size = new System.Drawing.Size(150, 40);
            this.btnNextStep.TabIndex = 0;
            this.btnNextStep.Text = "Next";
            this.btnNextStep.UseVisualStyleBackColor = true;
            this.btnNextStep.Click += new System.EventHandler(this.btnNextStep_Click);
            // 
            // tab1
            // 
            this.tab1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tab1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tab1.Controls.Add(this.tabLanguage);
            this.tab1.Controls.Add(this.tabLayoutMode);
            this.tab1.Controls.Add(this.tabTheme);
            this.tab1.Controls.Add(this.tabFileAssociation);
            this.tab1.Location = new System.Drawing.Point(-22, 213);
            this.tab1.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.tab1.Name = "tab1";
            this.tab1.Padding = new System.Drawing.Point(0, 0);
            this.tab1.SelectedIndex = 0;
            this.tab1.Size = new System.Drawing.Size(625, 217);
            this.tab1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tab1.TabIndex = 2;
            this.tab1.TabStop = false;
            // 
            // tabLanguage
            // 
            this.tabLanguage.BackColor = System.Drawing.Color.White;
            this.tabLanguage.Controls.Add(this.cmbLanguage);
            this.tabLanguage.Controls.Add(this.lblLanguage);
            this.tabLanguage.Location = new System.Drawing.Point(4, 37);
            this.tabLanguage.Name = "tabLanguage";
            this.tabLanguage.Padding = new System.Windows.Forms.Padding(3);
            this.tabLanguage.Size = new System.Drawing.Size(617, 152);
            this.tabLanguage.TabIndex = 7;
            this.tabLanguage.Text = "Language";
            // 
            // tabLayoutMode
            // 
            this.tabLayoutMode.BackColor = System.Drawing.Color.White;
            this.tabLayoutMode.Controls.Add(this.cmbLayout);
            this.tabLayoutMode.Controls.Add(this.lblLayout);
            this.tabLayoutMode.Location = new System.Drawing.Point(4, 37);
            this.tabLayoutMode.Name = "tabLayoutMode";
            this.tabLayoutMode.Padding = new System.Windows.Forms.Padding(3);
            this.tabLayoutMode.Size = new System.Drawing.Size(617, 152);
            this.tabLayoutMode.TabIndex = 8;
            this.tabLayoutMode.Text = "Quick layout mode";
            // 
            // tabTheme
            // 
            this.tabTheme.BackColor = System.Drawing.Color.White;
            this.tabTheme.Controls.Add(this.cmbTheme);
            this.tabTheme.Controls.Add(this.lblTheme);
            this.tabTheme.Location = new System.Drawing.Point(4, 37);
            this.tabTheme.Name = "tabTheme";
            this.tabTheme.Padding = new System.Windows.Forms.Padding(3);
            this.tabTheme.Size = new System.Drawing.Size(617, 176);
            this.tabTheme.TabIndex = 9;
            this.tabTheme.Text = "Theme";
            // 
            // tabFileAssociation
            // 
            this.tabFileAssociation.BackColor = System.Drawing.Color.White;
            this.tabFileAssociation.Controls.Add(this.btnSetDefaultApp);
            this.tabFileAssociation.Controls.Add(this.lblDefaultApp);
            this.tabFileAssociation.Location = new System.Drawing.Point(4, 37);
            this.tabFileAssociation.Name = "tabFileAssociation";
            this.tabFileAssociation.Padding = new System.Windows.Forms.Padding(3);
            this.tabFileAssociation.Size = new System.Drawing.Size(617, 176);
            this.tabFileAssociation.TabIndex = 10;
            this.tabFileAssociation.Text = "File Association";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.lblStepNumber);
            this.panel2.Controls.Add(this.picLogo);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(581, 259);
            this.panel2.TabIndex = 1;
            // 
            // lblLanguage
            // 
            this.lblLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLanguage.BackColor = System.Drawing.Color.Transparent;
            this.lblLanguage.Location = new System.Drawing.Point(30, 35);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(557, 35);
            this.lblLanguage.TabIndex = 0;
            this.lblLanguage.Text = "Select Language";
            this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            "English"});
            this.cmbLanguage.Location = new System.Drawing.Point(186, 73);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(243, 33);
            this.cmbLanguage.TabIndex = 1;
            // 
            // lblDefaultApp
            // 
            this.lblDefaultApp.BackColor = System.Drawing.Color.Transparent;
            this.lblDefaultApp.Location = new System.Drawing.Point(30, 35);
            this.lblDefaultApp.Name = "lblDefaultApp";
            this.lblDefaultApp.Size = new System.Drawing.Size(557, 38);
            this.lblDefaultApp.TabIndex = 0;
            this.lblDefaultApp.Text = "Set ImageGlass as Default Photo Viewer?";
            this.lblDefaultApp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSetDefaultApp
            // 
            this.btnSetDefaultApp.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnSetDefaultApp.Location = new System.Drawing.Point(235, 73);
            this.btnSetDefaultApp.Name = "btnSetDefaultApp";
            this.btnSetDefaultApp.Size = new System.Drawing.Size(150, 40);
            this.btnSetDefaultApp.TabIndex = 1;
            this.btnSetDefaultApp.Text = "Yes";
            this.btnSetDefaultApp.UseVisualStyleBackColor = true;
            this.btnSetDefaultApp.Click += new System.EventHandler(this.btnSetDefaultApp_Click);
            // 
            // lblStepNumber
            // 
            this.lblStepNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStepNumber.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblStepNumber.Location = new System.Drawing.Point(12, 213);
            this.lblStepNumber.Name = "lblStepNumber";
            this.lblStepNumber.Size = new System.Drawing.Size(557, 35);
            this.lblStepNumber.TabIndex = 2;
            this.lblStepNumber.Text = "Step 1/4";
            this.lblStepNumber.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbLayout
            // 
            this.cmbLayout.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmbLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLayout.FormattingEnabled = true;
            this.cmbLayout.Items.AddRange(new object[] {
            "Standard",
            "Designer"});
            this.cmbLayout.Location = new System.Drawing.Point(186, 73);
            this.cmbLayout.Name = "cmbLayout";
            this.cmbLayout.Size = new System.Drawing.Size(243, 33);
            this.cmbLayout.TabIndex = 3;
            // 
            // lblLayout
            // 
            this.lblLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLayout.BackColor = System.Drawing.Color.Transparent;
            this.lblLayout.Location = new System.Drawing.Point(30, 35);
            this.lblLayout.Name = "lblLayout";
            this.lblLayout.Size = new System.Drawing.Size(557, 35);
            this.lblLayout.TabIndex = 2;
            this.lblLayout.Text = "Select Layout";
            this.lblLayout.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbTheme
            // 
            this.cmbTheme.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTheme.FormattingEnabled = true;
            this.cmbTheme.Items.AddRange(new object[] {
            "2017 (Dark)",
            "2017 (Light Gray)"});
            this.cmbTheme.Location = new System.Drawing.Point(186, 73);
            this.cmbTheme.Name = "cmbTheme";
            this.cmbTheme.Size = new System.Drawing.Size(243, 33);
            this.cmbTheme.TabIndex = 5;
            // 
            // lblTheme
            // 
            this.lblTheme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTheme.BackColor = System.Drawing.Color.Transparent;
            this.lblTheme.Location = new System.Drawing.Point(30, 35);
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Size = new System.Drawing.Size(557, 35);
            this.lblTheme.TabIndex = 4;
            this.lblTheme.Text = "Select Theme";
            this.lblTheme.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lnkSkip
            // 
            this.lnkSkip.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(104)))), ((int)(((byte)(199)))));
            this.lnkSkip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkSkip.BackColor = System.Drawing.Color.Transparent;
            this.lnkSkip.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.lnkSkip.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkSkip.Location = new System.Drawing.Point(12, 78);
            this.lnkSkip.Name = "lnkSkip";
            this.lnkSkip.Size = new System.Drawing.Size(557, 35);
            this.lnkSkip.TabIndex = 29;
            this.lnkSkip.TabStop = true;
            this.lnkSkip.Text = "Skip and Launch ImageGlass";
            this.lnkSkip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkSkip.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkSkip.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSkip_LinkClicked);
            // 
            // frmFirstLaunch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(581, 521);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tab1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmFirstLaunch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "First-Launch Configurations";
            this.Load += new System.EventHandler(this.frmFirstLaunch_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.tab1.ResumeLayout(false);
            this.tabLanguage.ResumeLayout(false);
            this.tabLayoutMode.ResumeLayout(false);
            this.tabTheme.ResumeLayout(false);
            this.tabFileAssociation.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Button btnNextStep;
        private System.Windows.Forms.TabControl tab1;
        private System.Windows.Forms.TabPage tabLanguage;
        private System.Windows.Forms.TabPage tabLayoutMode;
        private System.Windows.Forms.TabPage tabTheme;
        private System.Windows.Forms.TabPage tabFileAssociation;
        private System.Windows.Forms.Panel panel2;
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