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
            this.panelColor.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPixel
            // 
            this.lblPixel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPixel.BackColor = System.Drawing.Color.Transparent;
            this.lblPixel.ForeColor = System.Drawing.Color.White;
            this.lblPixel.Location = new System.Drawing.Point(4, 0);
            this.lblPixel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPixel.Name = "lblPixel";
            this.lblPixel.Size = new System.Drawing.Size(250, 34);
            this.lblPixel.TabIndex = 0;
            this.lblPixel.Text = "(255, 1000)";
            this.lblPixel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPixel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // panelColor
            // 
            this.panelColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Controls.Add(this.lblPixel);
            this.panelColor.Location = new System.Drawing.Point(20, 20);
            this.panelColor.Margin = new System.Windows.Forms.Padding(4);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(260, 36);
            this.panelColor.TabIndex = 1;
            this.panelColor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // lblRGB
            // 
            this.lblRGB.AutoSize = true;
            this.lblRGB.BackColor = System.Drawing.Color.Transparent;
            this.lblRGB.ForeColor = System.Drawing.Color.White;
            this.lblRGB.Location = new System.Drawing.Point(15, 75);
            this.lblRGB.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRGB.Name = "lblRGB";
            this.lblRGB.Size = new System.Drawing.Size(61, 25);
            this.lblRGB.TabIndex = 2;
            this.lblRGB.Text = "RGBA:";
            this.lblRGB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // lblHEX
            // 
            this.lblHEX.AutoSize = true;
            this.lblHEX.BackColor = System.Drawing.Color.Transparent;
            this.lblHEX.ForeColor = System.Drawing.Color.White;
            this.lblHEX.Location = new System.Drawing.Point(15, 112);
            this.lblHEX.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHEX.Name = "lblHEX";
            this.lblHEX.Size = new System.Drawing.Size(61, 25);
            this.lblHEX.TabIndex = 3;
            this.lblHEX.Text = "HEXA:";
            this.lblHEX.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // txtRGB
            // 
            this.txtRGB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRGB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtRGB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRGB.Location = new System.Drawing.Point(82, 72);
            this.txtRGB.Name = "txtRGB";
            this.txtRGB.ReadOnly = true;
            this.txtRGB.Size = new System.Drawing.Size(198, 31);
            this.txtRGB.TabIndex = 1;
            this.txtRGB.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // txtHEX
            // 
            this.txtHEX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHEX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtHEX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHEX.Location = new System.Drawing.Point(82, 109);
            this.txtHEX.Name = "txtHEX";
            this.txtHEX.ReadOnly = true;
            this.txtHEX.Size = new System.Drawing.Size(198, 31);
            this.txtHEX.TabIndex = 2;
            this.txtHEX.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // txtCMYK
            // 
            this.txtCMYK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCMYK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtCMYK.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCMYK.Location = new System.Drawing.Point(82, 146);
            this.txtCMYK.Name = "txtCMYK";
            this.txtCMYK.ReadOnly = true;
            this.txtCMYK.Size = new System.Drawing.Size(198, 31);
            this.txtCMYK.TabIndex = 3;
            this.txtCMYK.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // lblCMYK
            // 
            this.lblCMYK.AutoSize = true;
            this.lblCMYK.BackColor = System.Drawing.Color.Transparent;
            this.lblCMYK.ForeColor = System.Drawing.Color.White;
            this.lblCMYK.Location = new System.Drawing.Point(15, 149);
            this.lblCMYK.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCMYK.Name = "lblCMYK";
            this.lblCMYK.Size = new System.Drawing.Size(63, 25);
            this.lblCMYK.TabIndex = 6;
            this.lblCMYK.Text = "CMYK:";
            this.lblCMYK.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // txtHSL
            // 
            this.txtHSL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHSL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtHSL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHSL.Location = new System.Drawing.Point(82, 183);
            this.txtHSL.Name = "txtHSL";
            this.txtHSL.ReadOnly = true;
            this.txtHSL.Size = new System.Drawing.Size(198, 31);
            this.txtHSL.TabIndex = 7;
            this.txtHSL.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // lblHSL
            // 
            this.lblHSL.AutoSize = true;
            this.lblHSL.BackColor = System.Drawing.Color.Transparent;
            this.lblHSL.ForeColor = System.Drawing.Color.White;
            this.lblHSL.Location = new System.Drawing.Point(15, 186);
            this.lblHSL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHSL.Name = "lblHSL";
            this.lblHSL.Size = new System.Drawing.Size(60, 25);
            this.lblHSL.TabIndex = 8;
            this.lblHSL.Text = "HSLA:";
            this.lblHSL.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // frmColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.ClientSize = new System.Drawing.Size(300, 235);
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
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmColorPicker_FormClosing);
            this.Load += new System.EventHandler(this.frmColorPicker_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmColorPicker_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            this.Move += new System.EventHandler(this.frmColorPicker_Move);
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
    }
}