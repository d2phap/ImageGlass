namespace ImageGlass.UI
{
    partial class ModernColorPicker
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TableLayout = new TableLayoutPanel();
            BoxGradient = new ColorBox2D();
            SliderRgb = new RgbColorSlider();
            SliderAlpha = new AlphaColorSlider();
            ViewColors = new ColorView();
            TxtHex = new ModernTextBox();
            RadR = new RadioButton();
            RadG = new RadioButton();
            RadB = new RadioButton();
            RadH = new RadioButton();
            RadS = new RadioButton();
            RadV = new RadioButton();
            NumR = new ModernNumericUpDown();
            NumG = new ModernNumericUpDown();
            NumB = new ModernNumericUpDown();
            NumA = new ModernNumericUpDown();
            NumH = new ModernNumericUpDown();
            NumS = new ModernNumericUpDown();
            NumV = new ModernNumericUpDown();
            modernLabel1 = new ModernLabel();
            modernLabel2 = new ModernLabel();
            modernLabel3 = new ModernLabel();
            modernLabel4 = new ModernLabel();
            modernLabel5 = new ModernLabel();
            modernLabel6 = new ModernLabel();
            modernLabel7 = new ModernLabel();
            modernLabel8 = new ModernLabel();
            modernLabel9 = new ModernLabel();
            modernLabel10 = new ModernLabel();
            modernLabel11 = new ModernLabel();
            TableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NumR).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumG).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumA).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumH).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumS).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumV).BeginInit();
            SuspendLayout();
            // 
            // TableLayout
            // 
            TableLayout.ColumnCount = 7;
            TableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TableLayout.ColumnStyles.Add(new ColumnStyle());
            TableLayout.ColumnStyles.Add(new ColumnStyle());
            TableLayout.ColumnStyles.Add(new ColumnStyle());
            TableLayout.ColumnStyles.Add(new ColumnStyle());
            TableLayout.ColumnStyles.Add(new ColumnStyle());
            TableLayout.ColumnStyles.Add(new ColumnStyle());
            TableLayout.Controls.Add(BoxGradient, 0, 0);
            TableLayout.Controls.Add(SliderRgb, 1, 0);
            TableLayout.Controls.Add(SliderAlpha, 2, 0);
            TableLayout.Controls.Add(TxtHex, 5, 1);
            TableLayout.Controls.Add(RadR, 3, 2);
            TableLayout.Controls.Add(RadG, 3, 3);
            TableLayout.Controls.Add(RadB, 3, 4);
            TableLayout.Controls.Add(RadH, 3, 6);
            TableLayout.Controls.Add(RadS, 3, 7);
            TableLayout.Controls.Add(RadV, 3, 8);
            TableLayout.Controls.Add(NumR, 5, 2);
            TableLayout.Controls.Add(NumG, 5, 3);
            TableLayout.Controls.Add(NumB, 5, 4);
            TableLayout.Controls.Add(NumA, 5, 5);
            TableLayout.Controls.Add(NumH, 5, 6);
            TableLayout.Controls.Add(NumS, 5, 7);
            TableLayout.Controls.Add(NumV, 5, 8);
            TableLayout.Controls.Add(modernLabel1, 6, 6);
            TableLayout.Controls.Add(modernLabel2, 6, 7);
            TableLayout.Controls.Add(modernLabel3, 4, 5);
            TableLayout.Controls.Add(modernLabel4, 3, 1);
            TableLayout.Controls.Add(modernLabel5, 4, 6);
            TableLayout.Controls.Add(modernLabel6, 4, 7);
            TableLayout.Controls.Add(modernLabel7, 4, 8);
            TableLayout.Controls.Add(modernLabel8, 4, 2);
            TableLayout.Controls.Add(modernLabel9, 4, 3);
            TableLayout.Controls.Add(modernLabel10, 4, 4);
            TableLayout.Controls.Add(modernLabel11, 6, 8);
            TableLayout.Controls.Add(ViewColors, 3, 0);
            TableLayout.Dock = DockStyle.Fill;
            TableLayout.Location = new Point(0, 0);
            TableLayout.Margin = new Padding(0);
            TableLayout.Name = "TableLayout";
            TableLayout.RowCount = 10;
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TableLayout.Size = new Size(1439, 915);
            TableLayout.TabIndex = 23;
            // 
            // BoxGradient
            // 
            BoxGradient.ColorMode = ColorMode.Luminance;
            BoxGradient.ColorRgb = Color.FromArgb(255, 0, 0);
            BoxGradient.DarkMode = false;
            BoxGradient.Dock = DockStyle.Top;
            BoxGradient.Location = new Point(0, 0);
            BoxGradient.Margin = new Padding(0, 0, 10, 0);
            BoxGradient.Name = "BoxGradient";
            TableLayout.SetRowSpan(BoxGradient, 9);
            BoxGradient.Size = new Size(810, 710);
            BoxGradient.TabIndex = 0;
            BoxGradient.TabStop = false;
            BoxGradient.ColorChanged += BoxGradient_ColorChanged;
            // 
            // SliderRgb
            // 
            SliderRgb.ColorMode = ColorMode.Luminance;
            SliderRgb.ColorRGB = Color.FromArgb(255, 0, 0);
            SliderRgb.DarkMode = false;
            SliderRgb.Location = new Point(830, 0);
            SliderRgb.Margin = new Padding(10, 0, 10, 0);
            SliderRgb.Name = "SliderRgb";
            TableLayout.SetRowSpan(SliderRgb, 9);
            SliderRgb.Size = new Size(100, 710);
            SliderRgb.TabIndex = 0;
            SliderRgb.TabStop = false;
            SliderRgb.Value = 0F;
            SliderRgb.ValueChanged += SliderRgb_ValueChanged;
            // 
            // SliderAlpha
            // 
            SliderAlpha.ColorValue = Color.FromArgb(0, 0, 0);
            SliderAlpha.DarkMode = false;
            SliderAlpha.Location = new Point(950, 0);
            SliderAlpha.Margin = new Padding(10, 0, 10, 0);
            SliderAlpha.Name = "SliderAlpha";
            TableLayout.SetRowSpan(SliderAlpha, 9);
            SliderAlpha.Size = new Size(100, 710);
            SliderAlpha.TabIndex = 0;
            SliderAlpha.TabStop = false;
            SliderAlpha.Value = 0F;
            SliderAlpha.ValueChanged += SliderAlpha_ValueChanged;
            // 
            // ViewColors
            // 
            ViewColors.Color1 = Color.FromArgb(100, 50, 100, 100);
            ViewColors.Color2 = Color.Empty;
            TableLayout.SetColumnSpan(ViewColors, 3);
            ViewColors.DarkMode = false;
            ViewColors.Dock = DockStyle.Left;
            ViewColors.Location = new Point(1070, 0);
            ViewColors.Margin = new Padding(10, 0, 0, 40);
            ViewColors.Name = "ViewColors";
            ViewColors.Size = new Size(313, 80);
            ViewColors.TabIndex = 0;
            ViewColors.TabStop = false;
            // 
            // TxtHex
            // 
            TxtHex.BackColor = SystemColors.Window;
            TxtHex.DarkMode = false;
            TxtHex.ForeColor = SystemColors.WindowText;
            TxtHex.Location = new Point(1183, 130);
            TxtHex.Margin = new Padding(10);
            TxtHex.Name = "TxtHex";
            TxtHex.SelectAllTextOnFocus = true;
            TxtHex.Size = new Size(200, 50);
            TxtHex.TabIndex = 11;
            TxtHex.Text = "#FF0000FF";
            TxtHex.LostFocus += TxtHex_LostFocus;
            // 
            // RadR
            // 
            RadR.Anchor = AnchorStyles.Left;
            RadR.AutoSize = true;
            RadR.Location = new Point(1070, 209);
            RadR.Margin = new Padding(10);
            RadR.Name = "RadR";
            RadR.Size = new Size(33, 32);
            RadR.TabIndex = 10;
            RadR.TabStop = true;
            RadR.UseVisualStyleBackColor = true;
            RadR.CheckedChanged += ChkColorMode_CheckedChanged;
            // 
            // RadG
            // 
            RadG.Anchor = AnchorStyles.Left;
            RadG.AutoSize = true;
            RadG.Location = new Point(1070, 279);
            RadG.Margin = new Padding(10);
            RadG.Name = "RadG";
            RadG.Size = new Size(33, 32);
            RadG.TabIndex = 10;
            RadG.TabStop = true;
            RadG.UseVisualStyleBackColor = true;
            RadG.CheckedChanged += ChkColorMode_CheckedChanged;
            // 
            // RadB
            // 
            RadB.Anchor = AnchorStyles.Left;
            RadB.AutoSize = true;
            RadB.Location = new Point(1070, 349);
            RadB.Margin = new Padding(10);
            RadB.Name = "RadB";
            RadB.Size = new Size(33, 32);
            RadB.TabIndex = 10;
            RadB.TabStop = true;
            RadB.UseVisualStyleBackColor = true;
            RadB.CheckedChanged += ChkColorMode_CheckedChanged;
            // 
            // RadH
            // 
            RadH.Anchor = AnchorStyles.Left;
            RadH.AutoSize = true;
            RadH.Location = new Point(1070, 519);
            RadH.Margin = new Padding(10, 40, 10, 10);
            RadH.Name = "RadH";
            RadH.Size = new Size(33, 32);
            RadH.TabIndex = 10;
            RadH.TabStop = true;
            RadH.UseVisualStyleBackColor = true;
            RadH.CheckedChanged += ChkColorMode_CheckedChanged;
            // 
            // RadS
            // 
            RadS.Anchor = AnchorStyles.Left;
            RadS.AutoSize = true;
            RadS.Location = new Point(1070, 589);
            RadS.Margin = new Padding(10);
            RadS.Name = "RadS";
            RadS.Size = new Size(33, 32);
            RadS.TabIndex = 10;
            RadS.TabStop = true;
            RadS.UseVisualStyleBackColor = true;
            RadS.CheckedChanged += ChkColorMode_CheckedChanged;
            // 
            // RadV
            // 
            RadV.Anchor = AnchorStyles.Left;
            RadV.AutoSize = true;
            RadV.Location = new Point(1070, 659);
            RadV.Margin = new Padding(10);
            RadV.Name = "RadV";
            RadV.Size = new Size(33, 32);
            RadV.TabIndex = 10;
            RadV.TabStop = true;
            RadV.UseVisualStyleBackColor = true;
            RadV.CheckedChanged += ChkColorMode_CheckedChanged;
            // 
            // NumR
            // 
            NumR.DarkMode = false;
            NumR.Location = new Point(1183, 200);
            NumR.Margin = new Padding(10);
            NumR.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            NumR.Name = "NumR";
            NumR.Size = new Size(200, 50);
            NumR.TabIndex = 12;
            NumR.ValueChanged += NumR_ValueChanged;
            // 
            // NumG
            // 
            NumG.DarkMode = false;
            NumG.Location = new Point(1183, 270);
            NumG.Margin = new Padding(10);
            NumG.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            NumG.Name = "NumG";
            NumG.Size = new Size(200, 50);
            NumG.TabIndex = 13;
            NumG.ValueChanged += NumG_ValueChanged;
            // 
            // NumB
            // 
            NumB.DarkMode = false;
            NumB.Location = new Point(1183, 340);
            NumB.Margin = new Padding(10);
            NumB.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            NumB.Name = "NumB";
            NumB.Size = new Size(200, 50);
            NumB.TabIndex = 14;
            NumB.ValueChanged += NumB_ValueChanged;
            // 
            // NumA
            // 
            NumA.DarkMode = false;
            NumA.Location = new Point(1183, 410);
            NumA.Margin = new Padding(10);
            NumA.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            NumA.Name = "NumA";
            NumA.Size = new Size(200, 50);
            NumA.TabIndex = 15;
            NumA.ValueChanged += NumA_ValueChanged;
            // 
            // NumH
            // 
            NumH.DarkMode = false;
            NumH.Location = new Point(1183, 510);
            NumH.Margin = new Padding(10, 40, 10, 10);
            NumH.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            NumH.Name = "NumH";
            NumH.Size = new Size(200, 50);
            NumH.TabIndex = 16;
            NumH.ValueChanged += NumH_ValueChanged;
            // 
            // NumS
            // 
            NumS.DarkMode = false;
            NumS.Location = new Point(1183, 580);
            NumS.Margin = new Padding(10);
            NumS.Name = "NumS";
            NumS.Size = new Size(200, 50);
            NumS.TabIndex = 17;
            NumS.ValueChanged += NumS_ValueChanged;
            // 
            // NumV
            // 
            NumV.DarkMode = false;
            NumV.Location = new Point(1183, 650);
            NumV.Margin = new Padding(10);
            NumV.Name = "NumV";
            NumV.Size = new Size(200, 50);
            NumV.TabIndex = 18;
            NumV.ValueChanged += NumL_ValueChanged;
            // 
            // modernLabel1
            // 
            modernLabel1.AutoSize = true;
            modernLabel1.BackColor = Color.Transparent;
            modernLabel1.DarkMode = false;
            modernLabel1.Location = new Point(1393, 510);
            modernLabel1.Margin = new Padding(0, 40, 0, 10);
            modernLabel1.Name = "modernLabel1";
            modernLabel1.Size = new Size(32, 45);
            modernLabel1.TabIndex = 19;
            modernLabel1.Text = "°";
            // 
            // modernLabel2
            // 
            modernLabel2.AutoSize = true;
            modernLabel2.BackColor = Color.Transparent;
            modernLabel2.DarkMode = false;
            modernLabel2.Location = new Point(1393, 580);
            modernLabel2.Margin = new Padding(0, 10, 0, 10);
            modernLabel2.Name = "modernLabel2";
            modernLabel2.Size = new Size(46, 45);
            modernLabel2.TabIndex = 20;
            modernLabel2.Text = "%";
            // 
            // modernLabel3
            // 
            modernLabel3.AutoSize = true;
            modernLabel3.BackColor = Color.Transparent;
            modernLabel3.DarkMode = false;
            modernLabel3.Location = new Point(1113, 410);
            modernLabel3.Margin = new Padding(0, 10, 10, 10);
            modernLabel3.Name = "modernLabel3";
            modernLabel3.Size = new Size(48, 45);
            modernLabel3.TabIndex = 0;
            modernLabel3.Text = "A:";
            // 
            // modernLabel4
            // 
            modernLabel4.AutoSize = true;
            modernLabel4.BackColor = Color.Transparent;
            TableLayout.SetColumnSpan(modernLabel4, 2);
            modernLabel4.DarkMode = false;
            modernLabel4.Location = new Point(1070, 130);
            modernLabel4.Margin = new Padding(10);
            modernLabel4.Name = "modernLabel4";
            modernLabel4.Size = new Size(85, 45);
            modernLabel4.TabIndex = 0;
            modernLabel4.Text = "HEX:";
            // 
            // modernLabel5
            // 
            modernLabel5.AutoSize = true;
            modernLabel5.BackColor = Color.Transparent;
            modernLabel5.DarkMode = false;
            modernLabel5.Location = new Point(1113, 510);
            modernLabel5.Margin = new Padding(0, 40, 10, 10);
            modernLabel5.Name = "modernLabel5";
            modernLabel5.Size = new Size(50, 45);
            modernLabel5.TabIndex = 0;
            modernLabel5.Text = "H:";
            // 
            // modernLabel6
            // 
            modernLabel6.AutoSize = true;
            modernLabel6.BackColor = Color.Transparent;
            modernLabel6.DarkMode = false;
            modernLabel6.Location = new Point(1113, 580);
            modernLabel6.Margin = new Padding(0, 10, 10, 10);
            modernLabel6.Name = "modernLabel6";
            modernLabel6.Size = new Size(44, 45);
            modernLabel6.TabIndex = 0;
            modernLabel6.Text = "S:";
            // 
            // modernLabel7
            // 
            modernLabel7.AutoSize = true;
            modernLabel7.BackColor = Color.Transparent;
            modernLabel7.DarkMode = false;
            modernLabel7.Location = new Point(1113, 650);
            modernLabel7.Margin = new Padding(0, 10, 10, 10);
            modernLabel7.Name = "modernLabel7";
            modernLabel7.Size = new Size(47, 45);
            modernLabel7.TabIndex = 0;
            modernLabel7.Text = "V:";
            // 
            // modernLabel8
            // 
            modernLabel8.AutoSize = true;
            modernLabel8.BackColor = Color.Transparent;
            modernLabel8.DarkMode = false;
            modernLabel8.Location = new Point(1113, 200);
            modernLabel8.Margin = new Padding(0, 10, 10, 10);
            modernLabel8.Name = "modernLabel8";
            modernLabel8.Size = new Size(46, 45);
            modernLabel8.TabIndex = 0;
            modernLabel8.Text = "R:";
            // 
            // modernLabel9
            // 
            modernLabel9.AutoSize = true;
            modernLabel9.BackColor = Color.Transparent;
            modernLabel9.DarkMode = false;
            modernLabel9.Location = new Point(1113, 270);
            modernLabel9.Margin = new Padding(0, 10, 10, 10);
            modernLabel9.Name = "modernLabel9";
            modernLabel9.Size = new Size(49, 45);
            modernLabel9.TabIndex = 0;
            modernLabel9.Text = "G:";
            // 
            // modernLabel10
            // 
            modernLabel10.AutoSize = true;
            modernLabel10.BackColor = Color.Transparent;
            modernLabel10.DarkMode = false;
            modernLabel10.Location = new Point(1113, 340);
            modernLabel10.Margin = new Padding(0, 10, 10, 10);
            modernLabel10.Name = "modernLabel10";
            modernLabel10.Size = new Size(45, 45);
            modernLabel10.TabIndex = 0;
            modernLabel10.Text = "B:";
            // 
            // modernLabel11
            // 
            modernLabel11.AutoSize = true;
            modernLabel11.BackColor = Color.Transparent;
            modernLabel11.DarkMode = false;
            modernLabel11.Location = new Point(1393, 650);
            modernLabel11.Margin = new Padding(0, 10, 0, 10);
            modernLabel11.Name = "modernLabel11";
            modernLabel11.Size = new Size(46, 45);
            modernLabel11.TabIndex = 21;
            modernLabel11.Text = "%";
            // 
            // ModernColorPicker
            // 
            AutoScaleDimensions = new SizeF(18F, 45F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(TableLayout);
            Margin = new Padding(0);
            Name = "ModernColorPicker";
            Size = new Size(1439, 915);
            TableLayout.ResumeLayout(false);
            TableLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NumR).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumG).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumB).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumA).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumH).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumS).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumV).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private TableLayoutPanel TableLayout;
        private ColorBox2D BoxGradient;
        private RgbColorSlider SliderRgb;
        private AlphaColorSlider SliderAlpha;
        private ColorView ViewColors;
        private ModernTextBox TxtHex;
        private ModernLabel modernLabel4;
        private ModernNumericUpDown NumA;
        private ModernLabel modernLabel3;
        private ModernNumericUpDown NumB;
        private RadioButton RadB;
        private ModernNumericUpDown NumG;
        private RadioButton RadG;
        private ModernNumericUpDown NumR;
        private RadioButton RadR;
        private ModernNumericUpDown NumV;
        private RadioButton RadV;
        private ModernNumericUpDown NumS;
        private RadioButton RadS;
        private ModernNumericUpDown NumH;
        private RadioButton RadH;
        private ModernLabel modernLabel8;
        private ModernLabel modernLabel7;
        private ModernLabel modernLabel6;
        private ModernLabel modernLabel5;
        private ModernLabel modernLabel9;
        private ModernLabel modernLabel10;
        private ModernLabel modernLabel1;
        private ModernLabel modernLabel2;
        private ModernLabel modernLabel11;
    }
}
