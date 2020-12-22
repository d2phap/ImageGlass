namespace ImageGlass
{
    partial class frmColorPicker
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
            this.lblPixel = new System.Windows.Forms.Label();
            this.panelColor = new System.Windows.Forms.Panel();
            this.lblRGB = new System.Windows.Forms.Label();
            this.lblHEX = new System.Windows.Forms.Label();
            this.txtRGB = new System.Windows.Forms.TextBox();
            this.txtHEX = new System.Windows.Forms.TextBox();
            this.txtCMYK = new System.Windows.Forms.TextBox();
            this.lblCMYK = new System.Windows.Forms.Label();
            this.txtHSL = new System.Windows.Forms.TextBox();
            this.lblHSL = new System.Windows.Forms.Label();
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.lblLocation = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSnapTo = new System.Windows.Forms.Button();
            this.lblFormTitle = new System.Windows.Forms.Label();
            this.txtHSV = new System.Windows.Forms.TextBox();
            this.lblHSV = new System.Windows.Forms.Label();
            this.panelColor.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPixel
            // 
            this.lblPixel.BackColor = System.Drawing.Color.Transparent;
            this.lblPixel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPixel.ForeColor = System.Drawing.Color.White;
            this.lblPixel.Location = new System.Drawing.Point(0, 0);
            this.lblPixel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPixel.Name = "lblPixel";
            this.lblPixel.Size = new System.Drawing.Size(255, 35);
            this.lblPixel.TabIndex = 0;
            this.lblPixel.Text = "(255, 1000)";
            this.lblPixel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelColor
            // 
            this.panelColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Controls.Add(this.lblPixel);
            this.panelColor.Location = new System.Drawing.Point(19, 60);
            this.panelColor.Margin = new System.Windows.Forms.Padding(4);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(257, 37);
            this.panelColor.TabIndex = 1;
            // 
            // lblRGB
            // 
            this.lblRGB.AutoSize = true;
            this.lblRGB.BackColor = System.Drawing.Color.Transparent;
            this.lblRGB.ForeColor = System.Drawing.Color.White;
            this.lblRGB.Location = new System.Drawing.Point(14, 141);
            this.lblRGB.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRGB.Name = "lblRGB";
            this.lblRGB.Size = new System.Drawing.Size(57, 23);
            this.lblRGB.TabIndex = 2;
            this.lblRGB.Text = "RGBA:";
            // 
            // lblHEX
            // 
            this.lblHEX.AutoSize = true;
            this.lblHEX.BackColor = System.Drawing.Color.Transparent;
            this.lblHEX.ForeColor = System.Drawing.Color.White;
            this.lblHEX.Location = new System.Drawing.Point(14, 175);
            this.lblHEX.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHEX.Name = "lblHEX";
            this.lblHEX.Size = new System.Drawing.Size(56, 23);
            this.lblHEX.TabIndex = 3;
            this.lblHEX.Text = "HEXA:";
            // 
            // txtRGB
            // 
            this.txtRGB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRGB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtRGB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRGB.Location = new System.Drawing.Point(76, 138);
            this.txtRGB.Name = "txtRGB";
            this.txtRGB.ReadOnly = true;
            this.txtRGB.Size = new System.Drawing.Size(199, 30);
            this.txtRGB.TabIndex = 2;
            this.txtRGB.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // txtHEX
            // 
            this.txtHEX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHEX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtHEX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHEX.Location = new System.Drawing.Point(76, 171);
            this.txtHEX.Name = "txtHEX";
            this.txtHEX.ReadOnly = true;
            this.txtHEX.Size = new System.Drawing.Size(199, 30);
            this.txtHEX.TabIndex = 3;
            this.txtHEX.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // txtCMYK
            // 
            this.txtCMYK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCMYK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtCMYK.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCMYK.Location = new System.Drawing.Point(76, 207);
            this.txtCMYK.Name = "txtCMYK";
            this.txtCMYK.ReadOnly = true;
            this.txtCMYK.Size = new System.Drawing.Size(199, 30);
            this.txtCMYK.TabIndex = 4;
            this.txtCMYK.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // lblCMYK
            // 
            this.lblCMYK.AutoSize = true;
            this.lblCMYK.BackColor = System.Drawing.Color.Transparent;
            this.lblCMYK.ForeColor = System.Drawing.Color.White;
            this.lblCMYK.Location = new System.Drawing.Point(14, 209);
            this.lblCMYK.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCMYK.Name = "lblCMYK";
            this.lblCMYK.Size = new System.Drawing.Size(59, 23);
            this.lblCMYK.TabIndex = 6;
            this.lblCMYK.Text = "CMYK:";
            // 
            // txtHSL
            // 
            this.txtHSL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHSL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtHSL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHSL.Location = new System.Drawing.Point(76, 242);
            this.txtHSL.Name = "txtHSL";
            this.txtHSL.ReadOnly = true;
            this.txtHSL.Size = new System.Drawing.Size(199, 30);
            this.txtHSL.TabIndex = 5;
            this.txtHSL.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // lblHSL
            // 
            this.lblHSL.AutoSize = true;
            this.lblHSL.BackColor = System.Drawing.Color.Transparent;
            this.lblHSL.ForeColor = System.Drawing.Color.White;
            this.lblHSL.Location = new System.Drawing.Point(14, 244);
            this.lblHSL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHSL.Name = "lblHSL";
            this.lblHSL.Size = new System.Drawing.Size(54, 23);
            this.lblHSL.TabIndex = 8;
            this.lblHSL.Text = "HSLA:";
            // 
            // txtLocation
            // 
            this.txtLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLocation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtLocation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLocation.Location = new System.Drawing.Point(76, 103);
            this.txtLocation.Name = "txtLocation";
            this.txtLocation.ReadOnly = true;
            this.txtLocation.Size = new System.Drawing.Size(199, 30);
            this.txtLocation.TabIndex = 1;
            this.txtLocation.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // lblLocation
            // 
            this.lblLocation.AutoSize = true;
            this.lblLocation.BackColor = System.Drawing.Color.Transparent;
            this.lblLocation.ForeColor = System.Drawing.Color.White;
            this.lblLocation.Location = new System.Drawing.Point(14, 106);
            this.lblLocation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLocation.Name = "lblLocation";
            this.lblLocation.Size = new System.Drawing.Size(42, 23);
            this.lblLocation.TabIndex = 11;
            this.lblLocation.Text = "X, Y:";
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
            // btnSnapTo
            // 
            this.btnSnapTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSnapTo.AutoSize = true;
            this.btnSnapTo.BackColor = System.Drawing.Color.Teal;
            this.btnSnapTo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.btnSnapTo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(62)))), ((int)(((byte)(74)))));
            this.btnSnapTo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(17)))), ((int)(((byte)(35)))));
            this.btnSnapTo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSnapTo.ForeColor = System.Drawing.Color.White;
            this.btnSnapTo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSnapTo.Location = new System.Drawing.Point(154, 2);
            this.btnSnapTo.Margin = new System.Windows.Forms.Padding(4);
            this.btnSnapTo.Name = "btnSnapTo";
            this.btnSnapTo.Size = new System.Drawing.Size(69, 37);
            this.btnSnapTo.TabIndex = 12;
            this.btnSnapTo.Text = "^";
            this.btnSnapTo.UseVisualStyleBackColor = false;
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
            this.lblFormTitle.TabIndex = 29;
            this.lblFormTitle.Text = "[Color picker]";
            this.lblFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtHSV
            // 
            this.txtHSV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHSV.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtHSV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHSV.Location = new System.Drawing.Point(76, 278);
            this.txtHSV.Name = "txtHSV";
            this.txtHSV.ReadOnly = true;
            this.txtHSV.Size = new System.Drawing.Size(199, 30);
            this.txtHSV.TabIndex = 30;
            // 
            // lblHSV
            // 
            this.lblHSV.AutoSize = true;
            this.lblHSV.BackColor = System.Drawing.Color.Transparent;
            this.lblHSV.ForeColor = System.Drawing.Color.White;
            this.lblHSV.Location = new System.Drawing.Point(14, 280);
            this.lblHSV.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHSV.Name = "lblHSV";
            this.lblHSV.Size = new System.Drawing.Size(56, 23);
            this.lblHSV.TabIndex = 31;
            this.lblHSV.Text = "HSVA:";
            // 
            // frmColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(134F, 134F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.ClientSize = new System.Drawing.Size(294, 323);
            this.Controls.Add(this.txtHSV);
            this.Controls.Add(this.lblHSV);
            this.Controls.Add(this.lblFormTitle);
            this.Controls.Add(this.btnSnapTo);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.txtLocation);
            this.Controls.Add(this.lblLocation);
            this.Controls.Add(this.txtHSL);
            this.Controls.Add(this.lblHSL);
            this.Controls.Add(this.txtCMYK);
            this.Controls.Add(this.lblCMYK);
            this.Controls.Add(this.txtHEX);
            this.Controls.Add(this.txtRGB);
            this.Controls.Add(this.lblHEX);
            this.Controls.Add(this.lblRGB);
            this.Controls.Add(this.panelColor);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmColorPicker";
            this.RightToLeftLayout = true;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Color Picker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmColorPicker_FormClosing);
            this.Load += new System.EventHandler(this.frmColorPicker_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmColorPicker_KeyDown);
            this.panelColor.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPixel;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Label lblRGB;
        private System.Windows.Forms.Label lblHEX;
        private System.Windows.Forms.TextBox txtRGB;
        private System.Windows.Forms.TextBox txtHEX;
        private System.Windows.Forms.TextBox txtCMYK;
        private System.Windows.Forms.Label lblCMYK;
        private System.Windows.Forms.TextBox txtHSL;
        private System.Windows.Forms.Label lblHSL;
        private System.Windows.Forms.TextBox txtLocation;
        private System.Windows.Forms.Label lblLocation;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSnapTo;
        private System.Windows.Forms.Label lblFormTitle;
        private System.Windows.Forms.TextBox txtHSV;
        private System.Windows.Forms.Label lblHSV;
    }
}