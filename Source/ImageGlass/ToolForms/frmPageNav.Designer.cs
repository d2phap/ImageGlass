namespace ImageGlass
{
    partial class frmPageNav
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
            this.btnClose = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnSnapTo = new System.Windows.Forms.Button();
            this.toolPageNav = new ImageGlass.UI.ToolStripToolTip();
            this.btnFirstPage = new System.Windows.Forms.ToolStripButton();
            this.btnPreviousPage = new System.Windows.Forms.ToolStripButton();
            this.btnNextPage = new System.Windows.Forms.ToolStripButton();
            this.btnLastPage = new System.Windows.Forms.ToolStripButton();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.lblFormTitle = new System.Windows.Forms.Label();
            this.toolPageNav.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.AutoSize = true;
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(15)))), ((int)(((byte)(29)))));
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(62)))), ((int)(((byte)(74)))));
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(17)))), ((int)(((byte)(35)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.Location = new System.Drawing.Point(224, 2);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(69, 37);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.ShowAlways = true;
            // 
            // btnSnapTo
            // 
            this.btnSnapTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSnapTo.AutoSize = true;
            this.btnSnapTo.BackColor = System.Drawing.Color.Teal;
            this.btnSnapTo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.btnSnapTo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSnapTo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnSnapTo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSnapTo.ForeColor = System.Drawing.Color.White;
            this.btnSnapTo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSnapTo.Location = new System.Drawing.Point(153, 2);
            this.btnSnapTo.Margin = new System.Windows.Forms.Padding(4);
            this.btnSnapTo.Name = "btnSnapTo";
            this.btnSnapTo.Size = new System.Drawing.Size(69, 37);
            this.btnSnapTo.TabIndex = 6;
            this.btnSnapTo.Text = "^";
            this.btnSnapTo.UseVisualStyleBackColor = false;
            // 
            // toolPageNav
            // 
            this.toolPageNav.Alignment = ImageGlass.UI.ToolbarAlignment.LEFT;
            this.toolPageNav.AllowMerge = false;
            this.toolPageNav.AutoSize = false;
            this.toolPageNav.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.toolPageNav.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolPageNav.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolPageNav.HideTooltips = false;
            this.toolPageNav.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolPageNav.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnFirstPage,
            this.btnPreviousPage,
            this.btnNextPage,
            this.btnLastPage});
            this.toolPageNav.Location = new System.Drawing.Point(0, 117);
            this.toolPageNav.Name = "toolPageNav";
            this.toolPageNav.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolPageNav.ShowItemToolTips = false;
            this.toolPageNav.Size = new System.Drawing.Size(294, 60);
            this.toolPageNav.TabIndex = 8;
            this.toolPageNav.ToolTipShowUp = false;
            // 
            // btnFirstPage
            // 
            this.btnFirstPage.AutoSize = false;
            this.btnFirstPage.BackColor = System.Drawing.Color.Transparent;
            this.btnFirstPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFirstPage.Image = global::ImageGlass.Properties.Resources.info;
            this.btnFirstPage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnFirstPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFirstPage.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnFirstPage.Name = "btnFirstPage";
            this.btnFirstPage.Size = new System.Drawing.Size(33, 33);
            this.btnFirstPage.ToolTipText = "Go to previous image (Left arrow / PageUp)";
            // 
            // btnPreviousPage
            // 
            this.btnPreviousPage.AutoSize = false;
            this.btnPreviousPage.BackColor = System.Drawing.Color.Transparent;
            this.btnPreviousPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPreviousPage.Image = global::ImageGlass.Properties.Resources.info;
            this.btnPreviousPage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnPreviousPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPreviousPage.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnPreviousPage.Name = "btnPreviousPage";
            this.btnPreviousPage.Size = new System.Drawing.Size(33, 33);
            this.btnPreviousPage.ToolTipText = "Go to next image (Right arrow / PageDown)";
            // 
            // btnNextPage
            // 
            this.btnNextPage.AutoSize = false;
            this.btnNextPage.BackColor = System.Drawing.Color.Transparent;
            this.btnNextPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNextPage.Image = global::ImageGlass.Properties.Resources.info;
            this.btnNextPage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnNextPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNextPage.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(33, 33);
            this.btnNextPage.ToolTipText = "Print image (Ctrl + P)";
            // 
            // btnLastPage
            // 
            this.btnLastPage.AutoSize = false;
            this.btnLastPage.BackColor = System.Drawing.Color.Transparent;
            this.btnLastPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLastPage.Image = global::ImageGlass.Properties.Resources.info;
            this.btnLastPage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnLastPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLastPage.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnLastPage.Name = "btnLastPage";
            this.btnLastPage.Size = new System.Drawing.Size(33, 33);
            this.btnLastPage.ToolTipText = "Send to recycle bin";
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPageInfo.Font = new System.Drawing.Font("Segoe UI", 13F);
            this.lblPageInfo.Location = new System.Drawing.Point(11, 57);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(272, 44);
            this.lblPageInfo.TabIndex = 9;
            this.lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFormTitle
            // 
            this.lblFormTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFormTitle.AutoEllipsis = true;
            this.lblFormTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblFormTitle.ForeColor = System.Drawing.Color.White;
            this.lblFormTitle.Location = new System.Drawing.Point(0, 5);
            this.lblFormTitle.Margin = new System.Windows.Forms.Padding(14, 0, 4, 0);
            this.lblFormTitle.Name = "lblFormTitle";
            this.lblFormTitle.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblFormTitle.Size = new System.Drawing.Size(145, 30);
            this.lblFormTitle.TabIndex = 28;
            this.lblFormTitle.Text = "[Page navigation]";
            this.lblFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmPageNav
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(134F, 134F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.ClientSize = new System.Drawing.Size(294, 177);
            this.Controls.Add(this.lblFormTitle);
            this.Controls.Add(this.toolPageNav);
            this.Controls.Add(this.lblPageInfo);
            this.Controls.Add(this.btnSnapTo);
            this.Controls.Add(this.btnClose);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(294, 112);
            this.Name = "frmPageNav";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Page Navigation";
            this.Activated += new System.EventHandler(this.frmPageNav_Activated);
            this.Load += new System.EventHandler(this.frmPageNav_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmPageNav_KeyDown);
            this.toolPageNav.ResumeLayout(false);
            this.toolPageNav.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnSnapTo;
        private ImageGlass.UI.ToolStripToolTip toolPageNav;
        private System.Windows.Forms.ToolStripButton btnFirstPage;
        private System.Windows.Forms.ToolStripButton btnPreviousPage;
        private System.Windows.Forms.ToolStripButton btnNextPage;
        private System.Windows.Forms.ToolStripButton btnLastPage;
        public System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.Label lblFormTitle;
    }
}