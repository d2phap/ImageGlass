namespace ImageGlass {
    partial class FrmExif {
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
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.gridExif = new System.Windows.Forms.DataGridView();
            this.clnProperty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clnValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.chkExifToolTopMost = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridExif)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(165)))), ((int)(((byte)(168)))));
            this.panel1.Controls.Add(this.chkExifToolTopMost);
            this.panel1.Controls.Add(this.btnExport);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 488);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(785, 90);
            this.panel1.TabIndex = 17;
            // 
            // btnExport
            // 
            this.btnExport.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnExport.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnExport.ForeColor = System.Drawing.Color.Black;
            this.btnExport.Location = new System.Drawing.Point(443, 26);
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
            this.btnClose.Location = new System.Drawing.Point(604, 26);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(155, 42);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "[Close]";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // gridExif
            // 
            this.gridExif.AllowUserToAddRows = false;
            this.gridExif.AllowUserToDeleteRows = false;
            this.gridExif.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridExif.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.gridExif.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridExif.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.gridExif.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridExif.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clnProperty,
            this.clnValue});
            this.gridExif.Location = new System.Drawing.Point(23, 28);
            this.gridExif.MultiSelect = false;
            this.gridExif.Name = "gridExif";
            this.gridExif.ReadOnly = true;
            this.gridExif.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.gridExif.RowHeadersVisible = false;
            this.gridExif.RowHeadersWidth = 57;
            this.gridExif.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.gridExif.RowTemplate.Height = 24;
            this.gridExif.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.gridExif.Size = new System.Drawing.Size(736, 436);
            this.gridExif.TabIndex = 19;
            // 
            // clnProperty
            // 
            this.clnProperty.Frozen = true;
            this.clnProperty.HeaderText = "Property";
            this.clnProperty.MinimumWidth = 7;
            this.clnProperty.Name = "clnProperty";
            this.clnProperty.ReadOnly = true;
            this.clnProperty.Width = 200;
            // 
            // clnValue
            // 
            this.clnValue.HeaderText = "Value";
            this.clnValue.MinimumWidth = 7;
            this.clnValue.Name = "clnValue";
            this.clnValue.ReadOnly = true;
            this.clnValue.Width = 500;
            // 
            // chkExifToolTopMost
            // 
            this.chkExifToolTopMost.AutoSize = true;
            this.chkExifToolTopMost.Location = new System.Drawing.Point(24, 35);
            this.chkExifToolTopMost.Name = "chkExifToolTopMost";
            this.chkExifToolTopMost.Size = new System.Drawing.Size(199, 27);
            this.chkExifToolTopMost.TabIndex = 21;
            this.chkExifToolTopMost.Text = "[Keep window on top]";
            this.chkExifToolTopMost.UseVisualStyleBackColor = true;
            this.chkExifToolTopMost.CheckedChanged += new System.EventHandler(this.chkExifToolTopMost_CheckedChanged);
            // 
            // FrmExif
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(134F, 134F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(203)))), ((int)(((byte)(204)))));
            this.ClientSize = new System.Drawing.Size(785, 578);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.gridExif);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmExif";
            this.Text = "Exif tool";
            this.Load += new System.EventHandler(this.FrmExif_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridExif)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.DataGridView gridExif;
        private System.Windows.Forms.DataGridViewTextBoxColumn clnProperty;
        private System.Windows.Forms.DataGridViewTextBoxColumn clnValue;
        private System.Windows.Forms.CheckBox chkExifToolTopMost;
    }
}
