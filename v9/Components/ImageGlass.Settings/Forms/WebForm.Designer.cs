namespace ImageGlass
{
    partial class WebForm
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
            WebV = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)WebV).BeginInit();
            SuspendLayout();
            // 
            // Web2
            // 
            WebV.AllowExternalDrop = true;
            WebV.CreationProperties = null;
            WebV.DefaultBackgroundColor = Color.White;
            WebV.Dock = DockStyle.Fill;
            WebV.Location = new Point(0, 0);
            WebV.Name = "Web2";
            WebV.Size = new Size(1468, 1312);
            WebV.TabIndex = 0;
            WebV.ZoomFactor = 1D;
            // 
            // FrmAbout
            // 
            AutoScaleDimensions = new SizeF(18F, 45F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1468, 1312);
            Controls.Add(WebV);
            MaximizeBox = false;
            Name = "FrmAbout";
            Text = "[About]";
            ((System.ComponentModel.ISupportInitialize)WebV).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 WebV;
    }
}