namespace ImageGlass
{
    partial class frmAddNewFormat
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblImageExtension = new System.Windows.Forms.Label();
            this.txtImageExtension = new System.Windows.Forms.TextBox();
            this.lblExtGroup = new System.Windows.Forms.Label();
            this.cmbExtGroup = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 203);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(457, 68);
            this.panel1.TabIndex = 15;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnOK.ForeColor = System.Drawing.Color.Black;
            this.btnOK.Location = new System.Drawing.Point(259, 16);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(86, 35);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.ForeColor = System.Drawing.Color.Black;
            this.btnClose.Location = new System.Drawing.Point(351, 16);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 35);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblImageExtension
            // 
            this.lblImageExtension.AutoSize = true;
            this.lblImageExtension.Location = new System.Drawing.Point(13, 25);
            this.lblImageExtension.Name = "lblImageExtension";
            this.lblImageExtension.Size = new System.Drawing.Size(142, 25);
            this.lblImageExtension.TabIndex = 16;
            this.lblImageExtension.Text = "Image extension";
            // 
            // txtImageExtension
            // 
            this.txtImageExtension.Location = new System.Drawing.Point(18, 53);
            this.txtImageExtension.Name = "txtImageExtension";
            this.txtImageExtension.Size = new System.Drawing.Size(419, 31);
            this.txtImageExtension.TabIndex = 0;
            this.txtImageExtension.Text = ".svg";
            // 
            // lblExtGroup
            // 
            this.lblExtGroup.AutoSize = true;
            this.lblExtGroup.Location = new System.Drawing.Point(13, 106);
            this.lblExtGroup.Name = "lblExtGroup";
            this.lblExtGroup.Size = new System.Drawing.Size(141, 25);
            this.lblExtGroup.TabIndex = 18;
            this.lblExtGroup.Text = "Extension group";
            // 
            // cmbExtGroup
            // 
            this.cmbExtGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExtGroup.FormattingEnabled = true;
            this.cmbExtGroup.Location = new System.Drawing.Point(18, 134);
            this.cmbExtGroup.Name = "cmbExtGroup";
            this.cmbExtGroup.Size = new System.Drawing.Size(419, 33);
            this.cmbExtGroup.TabIndex = 1;
            // 
            // frmAddNewFormat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(457, 271);
            this.ControlBox = false;
            this.Controls.Add(this.cmbExtGroup);
            this.Controls.Add(this.lblExtGroup);
            this.Controls.Add(this.txtImageExtension);
            this.Controls.Add(this.lblImageExtension);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAddNewFormat";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add new image extension";
            this.Load += new System.EventHandler(this.frmAddNewFormat_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmAddNewFormat_KeyDown);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblImageExtension;
        private System.Windows.Forms.TextBox txtImageExtension;
        private System.Windows.Forms.Label lblExtGroup;
        private System.Windows.Forms.ComboBox cmbExtGroup;
    }
}