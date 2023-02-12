using ImageGlass.UI;

namespace ImageGlass
{
    partial class FrmSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSettings));
            this.lblSettingsFilePath = new ImageGlass.UI.ModernLabel();
            this.btnOpenSettingsFile = new ImageGlass.UI.ModernButton();
            this.SuspendLayout();
            // 
            // lblSettingsFilePath
            // 
            this.lblSettingsFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSettingsFilePath.BackColor = System.Drawing.Color.Transparent;
            this.lblSettingsFilePath.DarkMode = false;
            this.lblSettingsFilePath.Location = new System.Drawing.Point(0, 0);
            this.lblSettingsFilePath.Name = "lblSettingsFilePath";
            this.lblSettingsFilePath.Padding = new System.Windows.Forms.Padding(40);
            this.lblSettingsFilePath.Size = new System.Drawing.Size(1174, 257);
            this.lblSettingsFilePath.TabIndex = 0;
            this.lblSettingsFilePath.Text = "[Setting file]";
            this.lblSettingsFilePath.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnOpenSettingsFile
            // 
            this.btnOpenSettingsFile.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOpenSettingsFile.AutoSize = true;
            this.btnOpenSettingsFile.DarkMode = false;
            this.btnOpenSettingsFile.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOpenSettingsFile.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenSettingsFile.Image")));
            this.btnOpenSettingsFile.ImagePadding = 2;
            this.btnOpenSettingsFile.Location = new System.Drawing.Point(393, 304);
            this.btnOpenSettingsFile.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.btnOpenSettingsFile.Name = "btnOpenSettingsFile";
            this.btnOpenSettingsFile.Padding = new System.Windows.Forms.Padding(4);
            this.btnOpenSettingsFile.Size = new System.Drawing.Size(384, 90);
            this.btnOpenSettingsFile.SystemIcon = ImageGlass.Base.WinApi.SHSTOCKICONID.SIID_DOCNOASSOC;
            this.btnOpenSettingsFile.TabIndex = 1;
            this.btnOpenSettingsFile.Text = "Open settings file";
            this.btnOpenSettingsFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnOpenSettingsFile.Click += new System.EventHandler(this.btnOpenSettingsFile_Click);
            // 
            // FrmSettings
            // 
            this.AcceptButton = this.btnOpenSettingsFile;
            this.AutoScaleDimensions = new System.Drawing.SizeF(18F, 45F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1175, 443);
            this.Controls.Add(this.btnOpenSettingsFile);
            this.Controls.Add(this.lblSettingsFilePath);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSettings";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ImageGlass settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ModernLabel lblSettingsFilePath;
        private ModernButton btnOpenSettingsFile;
    }
}