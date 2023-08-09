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
            ((System.ComponentModel.ISupportInitialize)Web2).BeginInit();
            SuspendLayout();
            // 
            // Web2
            // 
            Web2.DefaultBackgroundColor = Color.FromArgb(0, 255, 255, 255);
            Web2.Size = new Size(2277, 1425);
            // 
            // FrmSettings
            // 
            AutoScaleDimensions = new SizeF(18F, 45F);
            AutoScaleMode = AutoScaleMode.Font;
            BackdropStyle = Base.BackdropStyle.Mica;
            ClientSize = new Size(2277, 1425);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(6);
            MaximizeBox = true;
            MinimumSize = new Size(500, 300);
            Name = "FrmSettings";
            StartPosition = FormStartPosition.Manual;
            Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)Web2).EndInit();
            ResumeLayout(false);
        }

        #endregion
    }
}