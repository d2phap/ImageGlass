namespace ImageGlass
{
    partial class FrmColorPicker
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmColorPicker));
            TableLayout = new TableLayoutPanel();
            BtnSettings = new UI.ModernButton();
            BtnCopyHsv = new UI.ModernButton();
            BtnCopyHsl = new UI.ModernButton();
            BtnCopyCmyk = new UI.ModernButton();
            BtnCopyHex = new UI.ModernButton();
            BtnCopyRgb = new UI.ModernButton();
            TxtHsv = new UI.ModernTextBox();
            TxtHsl = new UI.ModernTextBox();
            TxtCmyk = new UI.ModernTextBox();
            TxtHex = new UI.ModernTextBox();
            TxtRgb = new UI.ModernTextBox();
            LblHsv = new UI.ModernLabel();
            LblLocation = new UI.ModernLabel();
            PanColor = new Panel();
            LblCursorLocation = new UI.ModernLabel();
            LblRgb = new UI.ModernLabel();
            LblHex = new UI.ModernLabel();
            LblCmyk = new UI.ModernLabel();
            LblHsl = new UI.ModernLabel();
            TxtLocation = new UI.ModernTextBox();
            BtnCopyLocation = new UI.ModernButton();
            TooltipMain = new ToolTip(components);
            TableLayout.SuspendLayout();
            PanColor.SuspendLayout();
            SuspendLayout();
            // 
            // TableLayout
            // 
            TableLayout.AutoSize = true;
            TableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TableLayout.ColumnCount = 3;
            TableLayout.ColumnStyles.Add(new ColumnStyle());
            TableLayout.ColumnStyles.Add(new ColumnStyle());
            TableLayout.ColumnStyles.Add(new ColumnStyle());
            TableLayout.Controls.Add(BtnSettings, 2, 0);
            TableLayout.Controls.Add(BtnCopyHsv, 2, 6);
            TableLayout.Controls.Add(BtnCopyHsl, 2, 5);
            TableLayout.Controls.Add(BtnCopyCmyk, 2, 4);
            TableLayout.Controls.Add(BtnCopyHex, 2, 3);
            TableLayout.Controls.Add(BtnCopyRgb, 2, 2);
            TableLayout.Controls.Add(TxtHsv, 1, 6);
            TableLayout.Controls.Add(TxtHsl, 1, 5);
            TableLayout.Controls.Add(TxtCmyk, 1, 4);
            TableLayout.Controls.Add(TxtHex, 1, 3);
            TableLayout.Controls.Add(TxtRgb, 1, 2);
            TableLayout.Controls.Add(LblHsv, 0, 6);
            TableLayout.Controls.Add(LblLocation, 0, 1);
            TableLayout.Controls.Add(PanColor, 0, 0);
            TableLayout.Controls.Add(LblRgb, 0, 2);
            TableLayout.Controls.Add(LblHex, 0, 3);
            TableLayout.Controls.Add(LblCmyk, 0, 4);
            TableLayout.Controls.Add(LblHsl, 0, 5);
            TableLayout.Controls.Add(TxtLocation, 1, 1);
            TableLayout.Controls.Add(BtnCopyLocation, 2, 1);
            TableLayout.Dock = DockStyle.Top;
            TableLayout.Location = new Point(0, 0);
            TableLayout.Margin = new Padding(0);
            TableLayout.Name = "TableLayout";
            TableLayout.Padding = new Padding(40);
            TableLayout.RowCount = 7;
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TableLayout.Size = new Size(660, 570);
            TableLayout.TabIndex = 0;
            // 
            // BtnSettings
            // 
            BtnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnSettings.DarkMode = true;
            BtnSettings.Image = (Image)resources.GetObject("BtnSettings.Image");
            BtnSettings.ImagePadding = 0;
            BtnSettings.Location = new Point(540, 40);
            BtnSettings.Margin = new Padding(10, 0, 0, 10);
            BtnSettings.Name = "BtnSettings";
            BtnSettings.Padding = new Padding(8);
            BtnSettings.Size = new Size(80, 60);
            BtnSettings.SvgIcon = Base.IconName.Setting;
            BtnSettings.SystemIcon = null;
            BtnSettings.TabIndex = 12;
            BtnSettings.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnSettings.Click += BtnSettings_Click;
            // 
            // BtnCopyHsv
            // 
            BtnCopyHsv.Anchor = AnchorStyles.Right;
            BtnCopyHsv.DarkMode = true;
            BtnCopyHsv.Image = (Image)resources.GetObject("BtnCopyHsv.Image");
            BtnCopyHsv.ImagePadding = 0;
            BtnCopyHsv.Location = new Point(540, 465);
            BtnCopyHsv.Margin = new Padding(10, 0, 0, 0);
            BtnCopyHsv.Name = "BtnCopyHsv";
            BtnCopyHsv.Padding = new Padding(8);
            BtnCopyHsv.Size = new Size(80, 60);
            BtnCopyHsv.SvgIcon = Base.IconName.Copy;
            BtnCopyHsv.SystemIcon = null;
            BtnCopyHsv.TabIndex = 11;
            BtnCopyHsv.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnCopyHsv.Click += BtnCopyHsv_Click;
            // 
            // BtnCopyHsl
            // 
            BtnCopyHsl.Anchor = AnchorStyles.Right;
            BtnCopyHsl.DarkMode = true;
            BtnCopyHsl.Image = (Image)resources.GetObject("BtnCopyHsl.Image");
            BtnCopyHsl.ImagePadding = 0;
            BtnCopyHsl.Location = new Point(540, 395);
            BtnCopyHsl.Margin = new Padding(10, 0, 0, 0);
            BtnCopyHsl.Name = "BtnCopyHsl";
            BtnCopyHsl.Padding = new Padding(8);
            BtnCopyHsl.Size = new Size(80, 60);
            BtnCopyHsl.SvgIcon = Base.IconName.Copy;
            BtnCopyHsl.SystemIcon = null;
            BtnCopyHsl.TabIndex = 9;
            BtnCopyHsl.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnCopyHsl.Click += BtnCopyHsl_Click;
            // 
            // BtnCopyCmyk
            // 
            BtnCopyCmyk.Anchor = AnchorStyles.Right;
            BtnCopyCmyk.DarkMode = true;
            BtnCopyCmyk.Image = (Image)resources.GetObject("BtnCopyCmyk.Image");
            BtnCopyCmyk.ImagePadding = 0;
            BtnCopyCmyk.Location = new Point(540, 325);
            BtnCopyCmyk.Margin = new Padding(10, 0, 0, 0);
            BtnCopyCmyk.Name = "BtnCopyCmyk";
            BtnCopyCmyk.Padding = new Padding(8);
            BtnCopyCmyk.Size = new Size(80, 60);
            BtnCopyCmyk.SvgIcon = Base.IconName.Copy;
            BtnCopyCmyk.SystemIcon = null;
            BtnCopyCmyk.TabIndex = 7;
            BtnCopyCmyk.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnCopyCmyk.Click += BtnCopyCmyk_Click;
            // 
            // BtnCopyHex
            // 
            BtnCopyHex.Anchor = AnchorStyles.Right;
            BtnCopyHex.DarkMode = true;
            BtnCopyHex.Image = (Image)resources.GetObject("BtnCopyHex.Image");
            BtnCopyHex.ImagePadding = 0;
            BtnCopyHex.Location = new Point(540, 255);
            BtnCopyHex.Margin = new Padding(10, 0, 0, 0);
            BtnCopyHex.Name = "BtnCopyHex";
            BtnCopyHex.Padding = new Padding(8);
            BtnCopyHex.Size = new Size(80, 60);
            BtnCopyHex.SvgIcon = Base.IconName.Copy;
            BtnCopyHex.SystemIcon = null;
            BtnCopyHex.TabIndex = 5;
            BtnCopyHex.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnCopyHex.Click += BtnCopyHex_Click;
            // 
            // BtnCopyRgb
            // 
            BtnCopyRgb.Anchor = AnchorStyles.Right;
            BtnCopyRgb.DarkMode = true;
            BtnCopyRgb.Image = (Image)resources.GetObject("BtnCopyRgb.Image");
            BtnCopyRgb.ImagePadding = 0;
            BtnCopyRgb.Location = new Point(540, 185);
            BtnCopyRgb.Margin = new Padding(10, 0, 0, 0);
            BtnCopyRgb.Name = "BtnCopyRgb";
            BtnCopyRgb.Padding = new Padding(8);
            BtnCopyRgb.Size = new Size(80, 60);
            BtnCopyRgb.SvgIcon = Base.IconName.Copy;
            BtnCopyRgb.SystemIcon = null;
            BtnCopyRgb.TabIndex = 3;
            BtnCopyRgb.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnCopyRgb.Click += BtnCopyRgb_Click;
            // 
            // TxtHsv
            // 
            TxtHsv.BackColor = Color.FromArgb(69, 73, 74);
            TxtHsv.BorderStyle = BorderStyle.FixedSingle;
            TxtHsv.DarkMode = true;
            TxtHsv.Dock = DockStyle.Fill;
            TxtHsv.ForeColor = Color.FromArgb(210, 210, 210);
            TxtHsv.Location = new Point(173, 470);
            TxtHsv.Margin = new Padding(0, 10, 0, 10);
            TxtHsv.Name = "TxtHsv";
            TxtHsv.ReadOnly = true;
            TxtHsv.Size = new Size(342, 50);
            TxtHsv.TabIndex = 10;
            // 
            // TxtHsl
            // 
            TxtHsl.BackColor = Color.FromArgb(69, 73, 74);
            TxtHsl.BorderStyle = BorderStyle.FixedSingle;
            TxtHsl.DarkMode = true;
            TxtHsl.Dock = DockStyle.Fill;
            TxtHsl.ForeColor = Color.FromArgb(210, 210, 210);
            TxtHsl.Location = new Point(173, 400);
            TxtHsl.Margin = new Padding(0, 10, 0, 10);
            TxtHsl.Name = "TxtHsl";
            TxtHsl.ReadOnly = true;
            TxtHsl.Size = new Size(342, 50);
            TxtHsl.TabIndex = 8;
            // 
            // TxtCmyk
            // 
            TxtCmyk.BackColor = Color.FromArgb(69, 73, 74);
            TxtCmyk.BorderStyle = BorderStyle.FixedSingle;
            TxtCmyk.DarkMode = true;
            TxtCmyk.Dock = DockStyle.Fill;
            TxtCmyk.ForeColor = Color.FromArgb(210, 210, 210);
            TxtCmyk.Location = new Point(173, 330);
            TxtCmyk.Margin = new Padding(0, 10, 0, 10);
            TxtCmyk.Name = "TxtCmyk";
            TxtCmyk.ReadOnly = true;
            TxtCmyk.Size = new Size(342, 50);
            TxtCmyk.TabIndex = 6;
            // 
            // TxtHex
            // 
            TxtHex.BackColor = Color.FromArgb(69, 73, 74);
            TxtHex.BorderStyle = BorderStyle.FixedSingle;
            TxtHex.DarkMode = true;
            TxtHex.Dock = DockStyle.Fill;
            TxtHex.ForeColor = Color.FromArgb(210, 210, 210);
            TxtHex.Location = new Point(173, 260);
            TxtHex.Margin = new Padding(0, 10, 0, 10);
            TxtHex.Name = "TxtHex";
            TxtHex.ReadOnly = true;
            TxtHex.Size = new Size(342, 50);
            TxtHex.TabIndex = 4;
            // 
            // TxtRgb
            // 
            TxtRgb.BackColor = Color.FromArgb(69, 73, 74);
            TxtRgb.BorderStyle = BorderStyle.FixedSingle;
            TxtRgb.DarkMode = true;
            TxtRgb.Dock = DockStyle.Fill;
            TxtRgb.ForeColor = Color.FromArgb(210, 210, 210);
            TxtRgb.Location = new Point(173, 190);
            TxtRgb.Margin = new Padding(0, 10, 0, 10);
            TxtRgb.Name = "TxtRgb";
            TxtRgb.ReadOnly = true;
            TxtRgb.Size = new Size(342, 50);
            TxtRgb.TabIndex = 2;
            // 
            // LblHsv
            // 
            LblHsv.Anchor = AnchorStyles.Left;
            LblHsv.AutoSize = true;
            LblHsv.BackColor = Color.Transparent;
            LblHsv.DarkMode = true;
            LblHsv.Location = new Point(43, 472);
            LblHsv.Name = "LblHsv";
            LblHsv.Size = new Size(126, 45);
            LblHsv.TabIndex = 6;
            LblHsv.Text = "[HSVA:]";
            // 
            // LblLocation
            // 
            LblLocation.Anchor = AnchorStyles.Left;
            LblLocation.AutoSize = true;
            LblLocation.BackColor = Color.Transparent;
            LblLocation.DarkMode = true;
            LblLocation.Location = new Point(40, 122);
            LblLocation.Margin = new Padding(0);
            LblLocation.Name = "LblLocation";
            LblLocation.Size = new Size(100, 45);
            LblLocation.TabIndex = 1;
            LblLocation.Text = "[X, Y:]";
            // 
            // PanColor
            // 
            PanColor.BackColor = Color.Transparent;
            PanColor.BorderStyle = BorderStyle.FixedSingle;
            TableLayout.SetColumnSpan(PanColor, 2);
            PanColor.Controls.Add(LblCursorLocation);
            PanColor.Dock = DockStyle.Fill;
            PanColor.Location = new Point(40, 40);
            PanColor.Margin = new Padding(0, 0, 0, 10);
            PanColor.Name = "PanColor";
            PanColor.Size = new Size(475, 60);
            PanColor.TabIndex = 0;
            // 
            // LblCursorLocation
            // 
            LblCursorLocation.BackColor = Color.Transparent;
            LblCursorLocation.DarkMode = false;
            LblCursorLocation.Dock = DockStyle.Fill;
            LblCursorLocation.ForeColor = Color.White;
            LblCursorLocation.Location = new Point(0, 0);
            LblCursorLocation.Name = "LblCursorLocation";
            LblCursorLocation.Size = new Size(473, 58);
            LblCursorLocation.TabIndex = 0;
            LblCursorLocation.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // LblRgb
            // 
            LblRgb.Anchor = AnchorStyles.Left;
            LblRgb.AutoSize = true;
            LblRgb.BackColor = Color.Transparent;
            LblRgb.DarkMode = true;
            LblRgb.Location = new Point(40, 192);
            LblRgb.Margin = new Padding(0);
            LblRgb.Name = "LblRgb";
            LblRgb.Size = new Size(127, 45);
            LblRgb.TabIndex = 2;
            LblRgb.Text = "[RGBA:]";
            // 
            // LblHex
            // 
            LblHex.Anchor = AnchorStyles.Left;
            LblHex.AutoSize = true;
            LblHex.BackColor = Color.Transparent;
            LblHex.DarkMode = true;
            LblHex.Location = new Point(43, 262);
            LblHex.Name = "LblHex";
            LblHex.Size = new Size(126, 45);
            LblHex.TabIndex = 3;
            LblHex.Text = "[HEXA:]";
            // 
            // LblCmyk
            // 
            LblCmyk.Anchor = AnchorStyles.Left;
            LblCmyk.AutoSize = true;
            LblCmyk.BackColor = Color.Transparent;
            LblCmyk.DarkMode = true;
            LblCmyk.Location = new Point(40, 332);
            LblCmyk.Margin = new Padding(0);
            LblCmyk.Name = "LblCmyk";
            LblCmyk.Size = new Size(133, 45);
            LblCmyk.TabIndex = 4;
            LblCmyk.Text = "[CMYK:]";
            // 
            // LblHsl
            // 
            LblHsl.Anchor = AnchorStyles.Left;
            LblHsl.AutoSize = true;
            LblHsl.BackColor = Color.Transparent;
            LblHsl.DarkMode = true;
            LblHsl.Location = new Point(40, 402);
            LblHsl.Margin = new Padding(0);
            LblHsl.Name = "LblHsl";
            LblHsl.Size = new Size(124, 45);
            LblHsl.TabIndex = 5;
            LblHsl.Text = "[HSLA:]";
            // 
            // TxtLocation
            // 
            TxtLocation.BackColor = Color.FromArgb(69, 73, 74);
            TxtLocation.BorderStyle = BorderStyle.FixedSingle;
            TxtLocation.DarkMode = true;
            TxtLocation.Dock = DockStyle.Fill;
            TxtLocation.ForeColor = Color.FromArgb(210, 210, 210);
            TxtLocation.Location = new Point(173, 120);
            TxtLocation.Margin = new Padding(0, 10, 0, 10);
            TxtLocation.Name = "TxtLocation";
            TxtLocation.ReadOnly = true;
            TxtLocation.Size = new Size(342, 50);
            TxtLocation.TabIndex = 0;
            // 
            // BtnCopyLocation
            // 
            BtnCopyLocation.Anchor = AnchorStyles.Right;
            BtnCopyLocation.DarkMode = true;
            BtnCopyLocation.Image = (Image)resources.GetObject("BtnCopyLocation.Image");
            BtnCopyLocation.ImagePadding = 0;
            BtnCopyLocation.Location = new Point(540, 115);
            BtnCopyLocation.Margin = new Padding(10, 0, 0, 0);
            BtnCopyLocation.Name = "BtnCopyLocation";
            BtnCopyLocation.Padding = new Padding(8);
            BtnCopyLocation.Size = new Size(80, 60);
            BtnCopyLocation.SvgIcon = Base.IconName.Copy;
            BtnCopyLocation.SystemIcon = null;
            BtnCopyLocation.TabIndex = 1;
            BtnCopyLocation.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnCopyLocation.Click += BtnCopyLocation_Click;
            // 
            // FrmColorPicker
            // 
            AutoScaleDimensions = new SizeF(18F, 45F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(660, 732);
            Controls.Add(TableLayout);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Location = new Point(0, 0);
            Name = "FrmColorPicker";
            Text = "[Color picker]";
            TableLayout.ResumeLayout(false);
            TableLayout.PerformLayout();
            PanColor.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel TableLayout;
        private Panel PanColor;
        private UI.ModernLabel LblHsv;
        private UI.ModernLabel LblLocation;
        private UI.ModernLabel LblRgb;
        private UI.ModernLabel LblHex;
        private UI.ModernLabel LblCmyk;
        private UI.ModernLabel LblHsl;
        private UI.ModernTextBox TxtLocation;
        private UI.ModernTextBox TxtHsv;
        private UI.ModernTextBox TxtHsl;
        private UI.ModernTextBox TxtCmyk;
        private UI.ModernTextBox TxtHex;
        private UI.ModernTextBox TxtRgb;
        private UI.ModernButton BtnCopyLocation;
        private UI.ModernButton BtnCopyHsv;
        private UI.ModernButton BtnCopyHsl;
        private UI.ModernButton BtnCopyCmyk;
        private UI.ModernButton BtnCopyHex;
        private UI.ModernButton BtnCopyRgb;
        private UI.ModernLabel LblCursorLocation;
        private UI.ModernButton BtnSettings;
        private ToolTip TooltipMain;
    }
}