namespace ImageGlass.FileList
{
    partial class FileListItem
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picAvatar = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.LinkLabel();
            this.lblPath = new System.Windows.Forms.LinkLabel();
            this.lblVersion = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // picAvatar
            // 
            this.picAvatar.BackColor = System.Drawing.Color.Transparent;
            this.picAvatar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picAvatar.Location = new System.Drawing.Point(3, 3);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.Size = new System.Drawing.Size(74, 74);
            this.picAvatar.TabIndex = 0;
            this.picAvatar.TabStop = false;
            this.picAvatar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseUp);
            this.picAvatar.MouseEnter += new System.EventHandler(this.FileListItem_MouseEnter);
            this.picAvatar.MouseLeave += new System.EventHandler(this.FileListItem_MouseLeave);
            this.picAvatar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseUp);
            // 
            // lblTitle
            // 
            this.lblTitle.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F);
            this.lblTitle.LinkArea = new System.Windows.Forms.LinkArea(0, 99);
            this.lblTitle.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lblTitle.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lblTitle.Location = new System.Drawing.Point(83, 3);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(50, 29);
            this.lblTitle.TabIndex = 17;
            this.lblTitle.TabStop = true;
            this.lblTitle.Text = "[Title]";
            this.lblTitle.UseCompatibleTextRendering = true;
            this.lblTitle.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseUp);
            this.lblTitle.MouseEnter += new System.EventHandler(this.FileListItem_MouseEnter);
            this.lblTitle.MouseLeave += new System.EventHandler(this.FileListItem_MouseLeave);
            this.lblTitle.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseUp);
            // 
            // lblPath
            // 
            this.lblPath.ActiveLinkColor = System.Drawing.Color.Black;
            this.lblPath.AutoSize = true;
            this.lblPath.BackColor = System.Drawing.Color.Transparent;
            this.lblPath.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblPath.LinkArea = new System.Windows.Forms.LinkArea(0, 99);
            this.lblPath.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lblPath.LinkColor = System.Drawing.Color.Black;
            this.lblPath.Location = new System.Drawing.Point(83, 29);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(58, 21);
            this.lblPath.TabIndex = 18;
            this.lblPath.TabStop = true;
            this.lblPath.Text = "[File path]";
            this.lblPath.UseCompatibleTextRendering = true;
            this.lblPath.VisitedLinkColor = System.Drawing.Color.Black;
            this.lblPath.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseUp);
            this.lblPath.MouseEnter += new System.EventHandler(this.FileListItem_MouseEnter);
            this.lblPath.MouseLeave += new System.EventHandler(this.FileListItem_MouseLeave);
            this.lblPath.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseUp);
            // 
            // lblVersion
            // 
            this.lblVersion.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lblVersion.AutoSize = true;
            this.lblVersion.BackColor = System.Drawing.Color.Transparent;
            this.lblVersion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblVersion.LinkArea = new System.Windows.Forms.LinkArea(20, 99);
            this.lblVersion.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lblVersion.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lblVersion.Location = new System.Drawing.Point(83, 50);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(189, 21);
            this.lblVersion.TabIndex = 19;
            this.lblVersion.TabStop = true;
            this.lblVersion.Text = "[Current version] - [Latest version]";
            this.lblVersion.UseCompatibleTextRendering = true;
            this.lblVersion.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lblVersion.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseUp);
            this.lblVersion.MouseEnter += new System.EventHandler(this.FileListItem_MouseEnter);
            this.lblVersion.MouseLeave += new System.EventHandler(this.FileListItem_MouseLeave);
            this.lblVersion.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseUp);
            // 
            // FileListItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.picAvatar);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Name = "FileListItem";
            this.Size = new System.Drawing.Size(425, 80);
            this.Tag = "";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseDown);
            this.MouseEnter += new System.EventHandler(this.FileListItem_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.FileListItem_MouseLeave);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picAvatar;
        private System.Windows.Forms.LinkLabel lblTitle;
        private System.Windows.Forms.LinkLabel lblPath;
        private System.Windows.Forms.LinkLabel lblVersion;
    }
}
