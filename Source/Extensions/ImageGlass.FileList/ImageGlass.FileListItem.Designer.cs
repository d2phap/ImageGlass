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
            this.components = new System.ComponentModel.Container();
            this.picAvatar = new System.Windows.Forms.PictureBox();
            this.tip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // picAvatar
            // 
            this.picAvatar.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.picAvatar.BackColor = System.Drawing.Color.Transparent;
            this.picAvatar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.picAvatar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picAvatar.Location = new System.Drawing.Point(6, 5);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.Size = new System.Drawing.Size(37, 37);
            this.picAvatar.TabIndex = 0;
            this.picAvatar.TabStop = false;
            this.picAvatar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseDown);
            this.picAvatar.MouseEnter += new System.EventHandler(this.FileListItem_MouseEnter);
            this.picAvatar.MouseLeave += new System.EventHandler(this.FileListItem_MouseLeave);
            this.picAvatar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseUp);
            // 
            // FileListItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.picAvatar);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Name = "FileListItem";
            this.Size = new System.Drawing.Size(415, 45);
            this.Tag = "";
            this.Load += new System.EventHandler(this.FileListItem_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FileListItem_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseDown);
            this.MouseEnter += new System.EventHandler(this.FileListItem_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.FileListItem_MouseLeave);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FileListItem_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picAvatar;
        private System.Windows.Forms.ToolTip tip1;
    }
}
