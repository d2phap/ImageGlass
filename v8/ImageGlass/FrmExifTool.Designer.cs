namespace ImageGlass {
    partial class FrmExifTool {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCopyValue = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lvExifItems = new System.Windows.Forms.ListView();
            this.clnNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnProperty = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panNotFound = new System.Windows.Forms.Panel();
            this.lnkSelectExifTool = new System.Windows.Forms.LinkLabel();
            this.lblNotFound = new System.Windows.Forms.Label();
            this.txtExifToolCommandPreview = new System.Windows.Forms.TextBox();
            this.clnTagId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1.SuspendLayout();
            this.panNotFound.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(165)))), ((int)(((byte)(168)))));
            this.panel1.Controls.Add(this.btnCopyValue);
            this.panel1.Controls.Add(this.btnExport);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 874);
            this.panel1.Margin = new System.Windows.Forms.Padding(5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1526, 161);
            this.panel1.TabIndex = 17;
            // 
            // btnCopyValue
            // 
            this.btnCopyValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCopyValue.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCopyValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCopyValue.ForeColor = System.Drawing.Color.Black;
            this.btnCopyValue.Location = new System.Drawing.Point(47, 47);
            this.btnCopyValue.Margin = new System.Windows.Forms.Padding(5);
            this.btnCopyValue.Name = "btnCopyValue";
            this.btnCopyValue.Size = new System.Drawing.Size(321, 75);
            this.btnCopyValue.TabIndex = 2;
            this.btnCopyValue.Text = "[Copy value]";
            this.btnCopyValue.UseVisualStyleBackColor = true;
            this.btnCopyValue.Click += new System.EventHandler(this.btnCopyValue_Click);
            // 
            // btnExport
            // 
            this.btnExport.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnExport.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnExport.ForeColor = System.Drawing.Color.Black;
            this.btnExport.Location = new System.Drawing.Point(913, 47);
            this.btnExport.Margin = new System.Windows.Forms.Padding(5);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(278, 75);
            this.btnExport.TabIndex = 3;
            this.btnExport.Text = "[Export...]";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnClose.ForeColor = System.Drawing.Color.Black;
            this.btnClose.Location = new System.Drawing.Point(1202, 47);
            this.btnClose.Margin = new System.Windows.Forms.Padding(5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(278, 75);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "[Close]";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lvExifItems
            // 
            this.lvExifItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvExifItems.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.lvExifItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clnNo,
            this.clnTagId,
            this.clnProperty,
            this.clnValue});
            this.lvExifItems.FullRowSelect = true;
            this.lvExifItems.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvExifItems.HideSelection = false;
            this.lvExifItems.Location = new System.Drawing.Point(47, 41);
            this.lvExifItems.Margin = new System.Windows.Forms.Padding(5);
            this.lvExifItems.MultiSelect = false;
            this.lvExifItems.Name = "lvExifItems";
            this.lvExifItems.ShowItemToolTips = true;
            this.lvExifItems.Size = new System.Drawing.Size(1430, 651);
            this.lvExifItems.TabIndex = 0;
            this.lvExifItems.UseCompatibleStateImageBehavior = false;
            this.lvExifItems.View = System.Windows.Forms.View.Details;
            // 
            // clnNo
            // 
            this.clnNo.Text = "";
            this.clnNo.Width = 100;
            // 
            // clnProperty
            // 
            this.clnProperty.Text = "[Property]";
            this.clnProperty.Width = 300;
            // 
            // clnValue
            // 
            this.clnValue.Text = "[Value]";
            this.clnValue.Width = 800;
            // 
            // panNotFound
            // 
            this.panNotFound.Controls.Add(this.lnkSelectExifTool);
            this.panNotFound.Controls.Add(this.lblNotFound);
            this.panNotFound.Dock = System.Windows.Forms.DockStyle.Top;
            this.panNotFound.Location = new System.Drawing.Point(0, 0);
            this.panNotFound.Margin = new System.Windows.Forms.Padding(5);
            this.panNotFound.Name = "panNotFound";
            this.panNotFound.Size = new System.Drawing.Size(1526, 296);
            this.panNotFound.TabIndex = 18;
            // 
            // lnkSelectExifTool
            // 
            this.lnkSelectExifTool.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(108)))), ((int)(((byte)(177)))));
            this.lnkSelectExifTool.AutoSize = true;
            this.lnkSelectExifTool.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkSelectExifTool.Location = new System.Drawing.Point(39, 204);
            this.lnkSelectExifTool.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lnkSelectExifTool.Name = "lnkSelectExifTool";
            this.lnkSelectExifTool.Size = new System.Drawing.Size(258, 41);
            this.lnkSelectExifTool.TabIndex = 1;
            this.lnkSelectExifTool.TabStop = true;
            this.lnkSelectExifTool.Text = "Select Exif tool file";
            this.lnkSelectExifTool.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(208)))));
            this.lnkSelectExifTool.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSelectExifTool_LinkClicked);
            // 
            // lblNotFound
            // 
            this.lblNotFound.Location = new System.Drawing.Point(39, 54);
            this.lblNotFound.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblNotFound.Name = "lblNotFound";
            this.lblNotFound.Size = new System.Drawing.Size(1440, 150);
            this.lblNotFound.TabIndex = 0;
            this.lblNotFound.Text = "[The Exif tool does not exist or invalid\r\nC:\\aaa\\bbb\\xxx.exe]";
            // 
            // txtExifToolCommandPreview
            // 
            this.txtExifToolCommandPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExifToolCommandPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.txtExifToolCommandPreview.Location = new System.Drawing.Point(47, 729);
            this.txtExifToolCommandPreview.Margin = new System.Windows.Forms.Padding(5);
            this.txtExifToolCommandPreview.Multiline = true;
            this.txtExifToolCommandPreview.Name = "txtExifToolCommandPreview";
            this.txtExifToolCommandPreview.ReadOnly = true;
            this.txtExifToolCommandPreview.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtExifToolCommandPreview.Size = new System.Drawing.Size(1430, 113);
            this.txtExifToolCommandPreview.TabIndex = 1;
            // 
            // clnTagId
            // 
            this.clnTagId.Text = "Tag ID";
            this.clnTagId.Width = 130;
            // 
            // FrmExifTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(240F, 240F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.ClientSize = new System.Drawing.Size(1526, 1035);
            this.Controls.Add(this.txtExifToolCommandPreview);
            this.Controls.Add(this.panNotFound);
            this.Controls.Add(this.lvExifItems);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "FrmExifTool";
            this.Text = "[Exif tool]";
            this.Activated += new System.EventHandler(this.FrmExifTool_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmExif_FormClosing);
            this.Load += new System.EventHandler(this.FrmExif_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmExif_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panNotFound.ResumeLayout(false);
            this.panNotFound.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ListView lvExifItems;
        private System.Windows.Forms.ColumnHeader clnProperty;
        private System.Windows.Forms.ColumnHeader clnValue;
        private System.Windows.Forms.Button btnCopyValue;
        private System.Windows.Forms.ColumnHeader clnNo;
        private System.Windows.Forms.Panel panNotFound;
        private System.Windows.Forms.Label lblNotFound;
        private System.Windows.Forms.LinkLabel lnkSelectExifTool;
        private System.Windows.Forms.TextBox txtExifToolCommandPreview;
        private System.Windows.Forms.ColumnHeader clnTagId;
    }
}
