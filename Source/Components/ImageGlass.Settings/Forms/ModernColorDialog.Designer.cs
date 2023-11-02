namespace ImageGlass.Settings
{
    partial class ModernColorDialog
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
            ColorPicker = new UI.ModernColorPicker();
            SuspendLayout();
            // 
            // ColorPicker
            // 
            ColorPicker.BackColor = Color.Transparent;
            ColorPicker.ColorMode = UI.ColorMode.Luminance;
            ColorPicker.ColorValue = Color.White;
            ColorPicker.DarkMode = false;
            ColorPicker.Dock = DockStyle.Top;
            ColorPicker.Location = new Point(0, 0);
            ColorPicker.Margin = new Padding(0);
            ColorPicker.Name = "ColorPicker";
            ColorPicker.Padding = new Padding(40, 40, 20, 20);
            ColorPicker.Size = new Size(1434, 836);
            ColorPicker.TabIndex = 3;
            // 
            // ModernColorDialog
            // 
            AutoScaleDimensions = new SizeF(18F, 45F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1434, 1132);
            Controls.Add(ColorPicker);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "ModernColorDialog";
            Text = "[Color picker]";
            Controls.SetChildIndex(ColorPicker, 0);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private UI.ModernColorPicker ColorPicker;
    }
}