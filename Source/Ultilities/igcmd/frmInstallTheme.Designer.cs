namespace igcmd
{
    partial class frmInstallTheme
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmInstallTheme));
            this.tim = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.picStatus = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // tim
            // 
            this.tim.Enabled = true;
            this.tim.Interval = 1000;
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.Controls.Add(this.btnOpen);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 86);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(531, 65);
            this.panel1.TabIndex = 10;
            // 
            // btnOpen
            // 
            this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpen.AutoSize = true;
            this.btnOpen.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnOpen.Location = new System.Drawing.Point(306, 18);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(134, 35);
            this.btnOpen.TabIndex = 3;
            this.btnOpen.Text = "Open location";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.AutoSize = true;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(446, 18);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(73, 35);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Location = new System.Drawing.Point(46, 24);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(100, 25);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Installing ...";
            // 
            // picStatus
            // 
            this.picStatus.Image = global::igcmd.Properties.Resources.loading;
            this.picStatus.Location = new System.Drawing.Point(12, 18);
            this.picStatus.Name = "picStatus";
            this.picStatus.Size = new System.Drawing.Size(32, 32);
            this.picStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picStatus.TabIndex = 9;
            this.picStatus.TabStop = false;
            // 
            // frmInstallTheme
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(531, 151);
            this.Controls.Add(this.picStatus);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(425, 151);
            this.Name = "frmInstallTheme";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ImageGlass theme installer";
            this.Load += new System.EventHandler(this.frmInstallTheme_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer tim;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.PictureBox picStatus;
        private System.Windows.Forms.Button btnOpen;

    }
}