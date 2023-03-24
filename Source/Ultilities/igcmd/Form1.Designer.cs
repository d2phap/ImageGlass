namespace igcmd
{
    partial class frmCheckForUpdate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCheckForUpdate));
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.picStoreApp = new System.Windows.Forms.PictureBox();
            this.web1 = new System.Windows.Forms.WebBrowser();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picStoreApp)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDownload
            // 
            this.btnDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownload.AutoSize = true;
            this.btnDownload.Location = new System.Drawing.Point(551, 29);
            this.btnDownload.Margin = new System.Windows.Forms.Padding(5);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(270, 75);
            this.btnDownload.TabIndex = 1;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.AutoSize = true;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(831, 29);
            this.btnClose.Margin = new System.Windows.Forms.Padding(5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(174, 75);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(165)))), ((int)(((byte)(168)))));
            this.panel1.Controls.Add(this.picStoreApp);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.btnDownload);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 881);
            this.panel1.Margin = new System.Windows.Forms.Padding(5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1026, 133);
            this.panel1.TabIndex = 4;
            // 
            // picStoreApp
            // 
            this.picStoreApp.BackColor = System.Drawing.Color.Black;
            this.picStoreApp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picStoreApp.Dock = System.Windows.Forms.DockStyle.Left;
            this.picStoreApp.Image = ((System.Drawing.Image)(resources.GetObject("picStoreApp.Image")));
            this.picStoreApp.Location = new System.Drawing.Point(0, 0);
            this.picStoreApp.Margin = new System.Windows.Forms.Padding(5);
            this.picStoreApp.Name = "picStoreApp";
            this.picStoreApp.Size = new System.Drawing.Size(385, 133);
            this.picStoreApp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picStoreApp.TabIndex = 24;
            this.picStoreApp.TabStop = false;
            this.picStoreApp.Click += new System.EventHandler(this.picStoreApp_Click);
            // 
            // web1
            // 
            this.web1.AllowNavigation = false;
            this.web1.AllowWebBrowserDrop = false;
            this.web1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.web1.IsWebBrowserContextMenuEnabled = false;
            this.web1.Location = new System.Drawing.Point(0, 0);
            this.web1.Margin = new System.Windows.Forms.Padding(0);
            this.web1.MinimumSize = new System.Drawing.Size(20, 20);
            this.web1.Name = "web1";
            this.web1.Size = new System.Drawing.Size(1026, 882);
            this.web1.TabIndex = 18;
            this.web1.TabStop = false;
            // 
            // frmCheckForUpdate
            // 
            this.AcceptButton = this.btnDownload;
            this.AutoScaleDimensions = new System.Drawing.SizeF(240F, 240F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1026, 1014);
            this.Controls.Add(this.web1);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(861, 357);
            this.Name = "frmCheckForUpdate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Check for update";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCheckForUpdate_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picStoreApp)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox picStoreApp;
        private System.Windows.Forms.WebBrowser web1;
    }
}

