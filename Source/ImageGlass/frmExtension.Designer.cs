namespace ImageGlass
{
    partial class frmExtension
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmExtension));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnInstallExt = new System.Windows.Forms.Button();
            this.btnGetMoreExt = new System.Windows.Forms.Button();
            this.btnRefreshAllExt = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.panExtension = new System.Windows.Forms.Panel();
            this.sp0 = new System.Windows.Forms.SplitContainer();
            this.tvExtension = new System.Windows.Forms.TreeView();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sp0)).BeginInit();
            this.sp0.Panel1.SuspendLayout();
            this.sp0.Panel2.SuspendLayout();
            this.sp0.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.panel1.Controls.Add(this.btnInstallExt);
            this.panel1.Controls.Add(this.btnGetMoreExt);
            this.panel1.Controls.Add(this.btnRefreshAllExt);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 394);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(824, 68);
            this.panel1.TabIndex = 14;
            // 
            // btnInstallExt
            // 
            this.btnInstallExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnInstallExt.AutoSize = true;
            this.btnInstallExt.ForeColor = System.Drawing.Color.Black;
            this.btnInstallExt.Image = ((System.Drawing.Image)(resources.GetObject("btnInstallExt.Image")));
            this.btnInstallExt.Location = new System.Drawing.Point(133, 16);
            this.btnInstallExt.Name = "btnInstallExt";
            this.btnInstallExt.Size = new System.Drawing.Size(115, 35);
            this.btnInstallExt.TabIndex = 2;
            this.btnInstallExt.Text = "Install";
            this.btnInstallExt.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnInstallExt.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnInstallExt.UseVisualStyleBackColor = true;
            this.btnInstallExt.Click += new System.EventHandler(this.btnInstallExt_Click);
            // 
            // btnGetMoreExt
            // 
            this.btnGetMoreExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGetMoreExt.AutoSize = true;
            this.btnGetMoreExt.ForeColor = System.Drawing.Color.Black;
            this.btnGetMoreExt.Image = ((System.Drawing.Image)(resources.GetObject("btnGetMoreExt.Image")));
            this.btnGetMoreExt.Location = new System.Drawing.Point(254, 16);
            this.btnGetMoreExt.Name = "btnGetMoreExt";
            this.btnGetMoreExt.Size = new System.Drawing.Size(238, 35);
            this.btnGetMoreExt.TabIndex = 3;
            this.btnGetMoreExt.Text = "Get more extensions";
            this.btnGetMoreExt.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnGetMoreExt.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnGetMoreExt.UseVisualStyleBackColor = true;
            this.btnGetMoreExt.Click += new System.EventHandler(this.btnGetMoreExt_Click);
            // 
            // btnRefreshAllExt
            // 
            this.btnRefreshAllExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRefreshAllExt.AutoSize = true;
            this.btnRefreshAllExt.ForeColor = System.Drawing.Color.Black;
            this.btnRefreshAllExt.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshAllExt.Image")));
            this.btnRefreshAllExt.Location = new System.Drawing.Point(12, 16);
            this.btnRefreshAllExt.Name = "btnRefreshAllExt";
            this.btnRefreshAllExt.Size = new System.Drawing.Size(115, 35);
            this.btnRefreshAllExt.TabIndex = 1;
            this.btnRefreshAllExt.Text = "Refresh";
            this.btnRefreshAllExt.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnRefreshAllExt.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnRefreshAllExt.UseVisualStyleBackColor = true;
            this.btnRefreshAllExt.Click += new System.EventHandler(this.btnRefreshAllExt_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.AutoSize = true;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.ForeColor = System.Drawing.Color.Black;
            this.btnClose.Location = new System.Drawing.Point(727, 16);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 35);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // panExtension
            // 
            this.panExtension.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panExtension.ForeColor = System.Drawing.Color.Black;
            this.panExtension.Location = new System.Drawing.Point(4, 0);
            this.panExtension.Margin = new System.Windows.Forms.Padding(0);
            this.panExtension.Name = "panExtension";
            this.panExtension.Size = new System.Drawing.Size(602, 376);
            this.panExtension.TabIndex = 0;
            // 
            // sp0
            // 
            this.sp0.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sp0.BackColor = System.Drawing.Color.Transparent;
            this.sp0.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.sp0.Location = new System.Drawing.Point(7, 12);
            this.sp0.Name = "sp0";
            // 
            // sp0.Panel1
            // 
            this.sp0.Panel1.Controls.Add(this.tvExtension);
            this.sp0.Panel1.Padding = new System.Windows.Forms.Padding(0, 50, 3, 0);
            // 
            // sp0.Panel2
            // 
            this.sp0.Panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sp0.Panel2.BackgroundImage")));
            this.sp0.Panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.sp0.Panel2.Controls.Add(this.panExtension);
            this.sp0.Panel2.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.sp0.Size = new System.Drawing.Size(806, 376);
            this.sp0.SplitterDistance = 196;
            this.sp0.TabIndex = 13;
            // 
            // tvExtension
            // 
            this.tvExtension.BackColor = System.Drawing.Color.White;
            this.tvExtension.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvExtension.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvExtension.ForeColor = System.Drawing.Color.Black;
            this.tvExtension.FullRowSelect = true;
            this.tvExtension.ImageKey = "empty.png";
            this.tvExtension.ItemHeight = 40;
            this.tvExtension.Location = new System.Drawing.Point(0, 50);
            this.tvExtension.Name = "tvExtension";
            this.tvExtension.ShowNodeToolTips = true;
            this.tvExtension.ShowRootLines = false;
            this.tvExtension.Size = new System.Drawing.Size(193, 326);
            this.tvExtension.TabIndex = 0;
            this.tvExtension.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvExtension_AfterSelect);
            // 
            // frmExtension
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(824, 462);
            this.Controls.Add(this.sp0);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(440, 200);
            this.Name = "frmExtension";
            this.RightToLeftLayout = true;
            this.Text = "Extension Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmExtension_FormClosing);
            this.Load += new System.EventHandler(this.frmExtension_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.sp0.Panel1.ResumeLayout(false);
            this.sp0.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sp0)).EndInit();
            this.sp0.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panExtension;
        private System.Windows.Forms.SplitContainer sp0;
        private System.Windows.Forms.TreeView tvExtension;
        private System.Windows.Forms.Button btnRefreshAllExt;
        private System.Windows.Forms.Button btnInstallExt;
        private System.Windows.Forms.Button btnGetMoreExt;
    }
}