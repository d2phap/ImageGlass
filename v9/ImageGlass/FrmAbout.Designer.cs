namespace ImageGlass
{
    partial class FrmAbout
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
            Web2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)Web2).BeginInit();
            SuspendLayout();
            // 
            // Web2
            // 
            Web2.AllowExternalDrop = true;
            Web2.CreationProperties = null;
            Web2.DefaultBackgroundColor = Color.White;
            Web2.Dock = DockStyle.Fill;
            Web2.Location = new Point(0, 0);
            Web2.Name = "Web2";
            Web2.Size = new Size(1468, 1300);
            Web2.TabIndex = 0;
            Web2.ZoomFactor = 1D;
            // 
            // FrmAbout
            // 
            AutoScaleDimensions = new SizeF(18F, 45F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1468, 1300);
            Controls.Add(Web2);
            Name = "FrmAbout";
            Text = "[About]";
            ((System.ComponentModel.ISupportInitialize)Web2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 Web2;
    }
}