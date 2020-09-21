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
            this.listExif = new System.Windows.Forms.ListView();
            this.clnProp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clnNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(165)))), ((int)(((byte)(168)))));
            this.panel1.Controls.Add(this.btnCopyValue);
            this.panel1.Controls.Add(this.btnExport);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 488);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(873, 90);
            this.panel1.TabIndex = 17;
            // 
            // btnCopyValue
            // 
            this.btnCopyValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCopyValue.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCopyValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCopyValue.ForeColor = System.Drawing.Color.Black;
            this.btnCopyValue.Location = new System.Drawing.Point(26, 26);
            this.btnCopyValue.Name = "btnCopyValue";
            this.btnCopyValue.Size = new System.Drawing.Size(179, 42);
            this.btnCopyValue.TabIndex = 1;
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
            this.btnExport.Location = new System.Drawing.Point(531, 26);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(155, 42);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "[Export...]";
            this.btnExport.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnClose.ForeColor = System.Drawing.Color.Black;
            this.btnClose.Location = new System.Drawing.Point(692, 26);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(155, 42);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "[Close]";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // listExif
            // 
            this.listExif.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listExif.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.listExif.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clnNo,
            this.clnProp,
            this.clnValue});
            this.listExif.FullRowSelect = true;
            this.listExif.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listExif.HideSelection = false;
            this.listExif.Location = new System.Drawing.Point(26, 29);
            this.listExif.MultiSelect = false;
            this.listExif.Name = "listExif";
            this.listExif.ShowItemToolTips = true;
            this.listExif.Size = new System.Drawing.Size(821, 430);
            this.listExif.TabIndex = 0;
            this.listExif.UseCompatibleStateImageBehavior = false;
            this.listExif.View = System.Windows.Forms.View.Details;
            // 
            // clnProp
            // 
            this.clnProp.Text = "Property";
            this.clnProp.Width = 300;
            // 
            // clnValue
            // 
            this.clnValue.Text = "Value";
            this.clnValue.Width = 480;
            // 
            // clnNo
            // 
            this.clnNo.Text = "";
            // 
            // FrmExifTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(134F, 134F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.ClientSize = new System.Drawing.Size(873, 578);
            this.Controls.Add(this.listExif);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.KeyPreview = true;
            this.Name = "FrmExifTool";
            this.Text = "Exif tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmExif_FormClosing);
            this.Load += new System.EventHandler(this.FrmExif_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmExif_KeyDown);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ListView listExif;
        private System.Windows.Forms.ColumnHeader clnProp;
        private System.Windows.Forms.ColumnHeader clnValue;
        private System.Windows.Forms.Button btnCopyValue;
        private System.Windows.Forms.ColumnHeader clnNo;
    }
}
