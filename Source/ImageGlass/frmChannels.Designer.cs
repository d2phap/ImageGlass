namespace ImageGlass
{
    partial class FrmChannels
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
            this.btnClose = new System.Windows.Forms.Button();
            this.radDefault = new System.Windows.Forms.RadioButton();
            this.radRed = new System.Windows.Forms.RadioButton();
            this.radBlue = new System.Windows.Forms.RadioButton();
            this.radGreen = new System.Windows.Forms.RadioButton();
            this.lblViewChannels = new System.Windows.Forms.Label();
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
            this.btnClose.Location = new System.Drawing.Point(242, 1);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(73, 40);
            this.btnClose.TabIndex = 999;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // radDefault
            // 
            this.radDefault.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radDefault.Appearance = System.Windows.Forms.Appearance.Button;
            this.radDefault.Checked = true;
            this.radDefault.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radDefault.ForeColor = System.Drawing.Color.White;
            this.radDefault.Location = new System.Drawing.Point(24, 62);
            this.radDefault.Name = "radDefault";
            this.radDefault.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.radDefault.Size = new System.Drawing.Size(270, 50);
            this.radDefault.TabIndex = 1;
            this.radDefault.TabStop = true;
            this.radDefault.Tag = "134217727";
            this.radDefault.Text = "[Default]";
            this.radDefault.UseVisualStyleBackColor = true;
            this.radDefault.Click += new System.EventHandler(this.RadChannels_Clicked);
            // 
            // radRed
            // 
            this.radRed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radRed.Appearance = System.Windows.Forms.Appearance.Button;
            this.radRed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radRed.ForeColor = System.Drawing.Color.White;
            this.radRed.Location = new System.Drawing.Point(24, 118);
            this.radRed.Name = "radRed";
            this.radRed.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.radRed.Size = new System.Drawing.Size(270, 50);
            this.radRed.TabIndex = 2;
            this.radRed.Tag = "1";
            this.radRed.Text = "[Red]";
            this.radRed.UseVisualStyleBackColor = true;
            this.radRed.Click += new System.EventHandler(this.RadChannels_Clicked);
            // 
            // radBlue
            // 
            this.radBlue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radBlue.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBlue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radBlue.ForeColor = System.Drawing.Color.White;
            this.radBlue.Location = new System.Drawing.Point(24, 230);
            this.radBlue.Name = "radBlue";
            this.radBlue.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.radBlue.Size = new System.Drawing.Size(270, 50);
            this.radBlue.TabIndex = 4;
            this.radBlue.Tag = "4";
            this.radBlue.Text = "[Blue]";
            this.radBlue.UseVisualStyleBackColor = true;
            this.radBlue.Click += new System.EventHandler(this.RadChannels_Clicked);
            // 
            // radGreen
            // 
            this.radGreen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radGreen.Appearance = System.Windows.Forms.Appearance.Button;
            this.radGreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radGreen.ForeColor = System.Drawing.Color.White;
            this.radGreen.Location = new System.Drawing.Point(24, 174);
            this.radGreen.Name = "radGreen";
            this.radGreen.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.radGreen.Size = new System.Drawing.Size(270, 50);
            this.radGreen.TabIndex = 3;
            this.radGreen.Tag = "2";
            this.radGreen.Text = "[Green]";
            this.radGreen.UseVisualStyleBackColor = true;
            this.radGreen.Click += new System.EventHandler(this.RadChannels_Clicked);
            // 
            // lblViewChannels
            // 
            this.lblViewChannels.AutoSize = true;
            this.lblViewChannels.ForeColor = System.Drawing.Color.White;
            this.lblViewChannels.Location = new System.Drawing.Point(19, 9);
            this.lblViewChannels.Name = "lblViewChannels";
            this.lblViewChannels.Size = new System.Drawing.Size(132, 25);
            this.lblViewChannels.TabIndex = 1000;
            this.lblViewChannels.Text = "[View channels]";
            // 
            // FrmChannels
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.ClientSize = new System.Drawing.Size(316, 306);
            this.Controls.Add(this.lblViewChannels);
            this.Controls.Add(this.radBlue);
            this.Controls.Add(this.radGreen);
            this.Controls.Add(this.radRed);
            this.Controls.Add(this.radDefault);
            this.Controls.Add(this.btnClose);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmChannels";
            this.RightToLeftLayout = true;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Color Picker";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmChannels_FormClosing);
            this.Load += new System.EventHandler(this.FrmChannels_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmChannels_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FrmChannels_MouseDown);
            this.Move += new System.EventHandler(this.FrmChannels_Move);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.RadioButton radDefault;
        private System.Windows.Forms.RadioButton radRed;
        private System.Windows.Forms.RadioButton radBlue;
        private System.Windows.Forms.RadioButton radGreen;
        private System.Windows.Forms.Label lblViewChannels;
    }
}