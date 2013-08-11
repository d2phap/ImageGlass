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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Get more extensions");
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.panExtension = new System.Windows.Forms.Panel();
            this.lnkGetMoreExt = new System.Windows.Forms.LinkLabel();
            this.sp0 = new System.Windows.Forms.SplitContainer();
            this.tvExtension = new System.Windows.Forms.TreeView();
            this.panel1.SuspendLayout();
            this.panExtension.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sp0)).BeginInit();
            this.sp0.Panel1.SuspendLayout();
            this.sp0.Panel2.SuspendLayout();
            this.sp0.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 422);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(822, 42);
            this.panel1.TabIndex = 14;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(728, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 28);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // panExtension
            // 
            this.panExtension.Controls.Add(this.lnkGetMoreExt);
            this.panExtension.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panExtension.Location = new System.Drawing.Point(4, 0);
            this.panExtension.Margin = new System.Windows.Forms.Padding(0);
            this.panExtension.Name = "panExtension";
            this.panExtension.Size = new System.Drawing.Size(600, 404);
            this.panExtension.TabIndex = 0;
            // 
            // lnkGetMoreExt
            // 
            this.lnkGetMoreExt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lnkGetMoreExt.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkGetMoreExt.Location = new System.Drawing.Point(0, 0);
            this.lnkGetMoreExt.Name = "lnkGetMoreExt";
            this.lnkGetMoreExt.Size = new System.Drawing.Size(600, 404);
            this.lnkGetMoreExt.TabIndex = 0;
            this.lnkGetMoreExt.TabStop = true;
            this.lnkGetMoreExt.Text = "Get more extensions";
            this.lnkGetMoreExt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkGetMoreExt.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGetMoreExt_LinkClicked);
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
            this.sp0.Size = new System.Drawing.Size(804, 404);
            this.sp0.SplitterDistance = 196;
            this.sp0.TabIndex = 13;
            // 
            // tvExtension
            // 
            this.tvExtension.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvExtension.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvExtension.ImageKey = "empty.png";
            this.tvExtension.ItemHeight = 25;
            this.tvExtension.Location = new System.Drawing.Point(0, 50);
            this.tvExtension.Name = "tvExtension";
            treeNode1.Name = "Node0";
            treeNode1.Text = "Get more extensions";
            this.tvExtension.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.tvExtension.ShowRootLines = false;
            this.tvExtension.Size = new System.Drawing.Size(193, 354);
            this.tvExtension.TabIndex = 8;
            this.tvExtension.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvExtension_AfterSelect);
            // 
            // frmExtension
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(822, 464);
            this.Controls.Add(this.sp0);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmExtension";
            this.Text = "Extension";
            this.Load += new System.EventHandler(this.frmExtension_Load);
            this.panel1.ResumeLayout(false);
            this.panExtension.ResumeLayout(false);
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
        private System.Windows.Forms.LinkLabel lnkGetMoreExt;
    }
}