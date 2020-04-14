namespace ImageGlass
{
    partial class frmEditApp
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblAppName = new System.Windows.Forms.Label();
            this.txtFileExtension = new System.Windows.Forms.TextBox();
            this.lblFileExtension = new System.Windows.Forms.Label();
            this.txtAppName = new System.Windows.Forms.TextBox();
            this.txtAppPath = new System.Windows.Forms.TextBox();
            this.lblAppPath = new System.Windows.Forms.Label();
            this.txtAppArguments = new System.Windows.Forms.TextBox();
            this.lblAppArguments = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblPreviewLabel = new System.Windows.Forms.Label();
            this.txtCommandPreview = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(165)))), ((int)(((byte)(168)))));
            this.panel1.Controls.Add(this.btnReset);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 494);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(510, 80);
            this.panel1.TabIndex = 21;
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnReset.ForeColor = System.Drawing.Color.Black;
            this.btnReset.Location = new System.Drawing.Point(18, 20);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(132, 40);
            this.btnReset.TabIndex = 6;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.ForeColor = System.Drawing.Color.Black;
            this.btnOK.Location = new System.Drawing.Point(261, 20);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(112, 40);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnClose.ForeColor = System.Drawing.Color.Black;
            this.btnClose.Location = new System.Drawing.Point(380, 20);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(112, 40);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblAppName
            // 
            this.lblAppName.AutoSize = true;
            this.lblAppName.Location = new System.Drawing.Point(14, 106);
            this.lblAppName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAppName.Name = "lblAppName";
            this.lblAppName.Size = new System.Drawing.Size(95, 25);
            this.lblAppName.TabIndex = 23;
            this.lblAppName.Text = "App name";
            // 
            // txtFileExtension
            // 
            this.txtFileExtension.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFileExtension.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.txtFileExtension.Location = new System.Drawing.Point(18, 52);
            this.txtFileExtension.Margin = new System.Windows.Forms.Padding(4);
            this.txtFileExtension.Name = "txtFileExtension";
            this.txtFileExtension.ReadOnly = true;
            this.txtFileExtension.Size = new System.Drawing.Size(472, 31);
            this.txtFileExtension.TabIndex = 0;
            // 
            // lblFileExtension
            // 
            this.lblFileExtension.AutoSize = true;
            this.lblFileExtension.Location = new System.Drawing.Point(14, 26);
            this.lblFileExtension.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFileExtension.Name = "lblFileExtension";
            this.lblFileExtension.Size = new System.Drawing.Size(118, 25);
            this.lblFileExtension.TabIndex = 22;
            this.lblFileExtension.Text = "File extension";
            // 
            // txtAppName
            // 
            this.txtAppName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAppName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.txtAppName.Location = new System.Drawing.Point(18, 134);
            this.txtAppName.Margin = new System.Windows.Forms.Padding(4);
            this.txtAppName.Name = "txtAppName";
            this.txtAppName.Size = new System.Drawing.Size(472, 31);
            this.txtAppName.TabIndex = 1;
            // 
            // txtAppPath
            // 
            this.txtAppPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAppPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.txtAppPath.Location = new System.Drawing.Point(18, 214);
            this.txtAppPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtAppPath.Name = "txtAppPath";
            this.txtAppPath.Size = new System.Drawing.Size(380, 31);
            this.txtAppPath.TabIndex = 2;
            // 
            // lblAppPath
            // 
            this.lblAppPath.AutoSize = true;
            this.lblAppPath.Location = new System.Drawing.Point(14, 188);
            this.lblAppPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAppPath.Name = "lblAppPath";
            this.lblAppPath.Size = new System.Drawing.Size(87, 25);
            this.lblAppPath.TabIndex = 25;
            this.lblAppPath.Text = "App path";
            // 
            // txtAppArguments
            // 
            this.txtAppArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAppArguments.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.txtAppArguments.Location = new System.Drawing.Point(18, 296);
            this.txtAppArguments.Margin = new System.Windows.Forms.Padding(4);
            this.txtAppArguments.Name = "txtAppArguments";
            this.txtAppArguments.Size = new System.Drawing.Size(472, 31);
            this.txtAppArguments.TabIndex = 4;
            // 
            // lblAppArguments
            // 
            this.lblAppArguments.AutoSize = true;
            this.lblAppArguments.Location = new System.Drawing.Point(14, 268);
            this.lblAppArguments.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAppArguments.Name = "lblAppArguments";
            this.lblAppArguments.Size = new System.Drawing.Size(136, 25);
            this.lblAppArguments.TabIndex = 27;
            this.lblAppArguments.Text = "App arguments";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.AutoSize = true;
            this.btnBrowse.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnBrowse.ForeColor = System.Drawing.Color.Black;
            this.btnBrowse.Location = new System.Drawing.Point(410, 212);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(82, 38);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblPreviewLabel
            // 
            this.lblPreviewLabel.AutoSize = true;
            this.lblPreviewLabel.Location = new System.Drawing.Point(14, 352);
            this.lblPreviewLabel.Name = "lblPreviewLabel";
            this.lblPreviewLabel.Size = new System.Drawing.Size(72, 25);
            this.lblPreviewLabel.TabIndex = 29;
            this.lblPreviewLabel.Text = "Preview";
            // 
            // txtCommandPreview
            // 
            this.txtCommandPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommandPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.txtCommandPreview.Location = new System.Drawing.Point(18, 381);
            this.txtCommandPreview.Margin = new System.Windows.Forms.Padding(4);
            this.txtCommandPreview.Multiline = true;
            this.txtCommandPreview.Name = "txtCommandPreview";
            this.txtCommandPreview.ReadOnly = true;
            this.txtCommandPreview.Size = new System.Drawing.Size(472, 93);
            this.txtCommandPreview.TabIndex = 5;
            // 
            // frmEditApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(510, 574);
            this.Controls.Add(this.txtCommandPreview);
            this.Controls.Add(this.lblPreviewLabel);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtAppArguments);
            this.Controls.Add(this.lblAppArguments);
            this.Controls.Add(this.txtAppPath);
            this.Controls.Add(this.lblAppPath);
            this.Controls.Add(this.txtAppName);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblAppName);
            this.Controls.Add(this.txtFileExtension);
            this.Controls.Add(this.lblFileExtension);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmEditApp";
            this.RightToLeftLayout = true;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmEditApp_FormClosing);
            this.Load += new System.EventHandler(this.frmEditApp_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmEditApp_KeyDown);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblAppName;
        private System.Windows.Forms.TextBox txtFileExtension;
        private System.Windows.Forms.Label lblFileExtension;
        private System.Windows.Forms.TextBox txtAppName;
        private System.Windows.Forms.TextBox txtAppPath;
        private System.Windows.Forms.Label lblAppPath;
        private System.Windows.Forms.TextBox txtAppArguments;
        private System.Windows.Forms.Label lblAppArguments;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Label lblPreviewLabel;
        private System.Windows.Forms.TextBox txtCommandPreview;
    }
}