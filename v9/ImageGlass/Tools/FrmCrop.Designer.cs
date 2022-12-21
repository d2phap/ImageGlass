using ImageGlass.UI;

namespace ImageGlass
{
    partial class FrmCrop
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCrop));
            this.TableTop = new System.Windows.Forms.TableLayoutPanel();
            this.LblLocation = new ImageGlass.UI.ModernLabel();
            this.LblSize = new ImageGlass.UI.ModernLabel();
            this.BtnSettings = new ImageGlass.UI.ModernButton();
            this.LblAspectRatio = new ImageGlass.UI.ModernLabel();
            this.NumX = new ImageGlass.UI.ModernNumericUpDown();
            this.NumY = new ImageGlass.UI.ModernNumericUpDown();
            this.NumWidth = new ImageGlass.UI.ModernNumericUpDown();
            this.NumHeight = new ImageGlass.UI.ModernNumericUpDown();
            this.CmbAspectRatio = new ImageGlass.UI.ModernComboBox();
            this.NumRatioFrom = new ImageGlass.UI.ModernNumericUpDown();
            this.NumRatioTo = new ImageGlass.UI.ModernNumericUpDown();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.BtnReset = new ImageGlass.UI.ModernButton();
            this.BtnQuickSelect = new ImageGlass.UI.ModernButton();
            this.TableBottom = new System.Windows.Forms.TableLayoutPanel();
            this.BtnSave = new ImageGlass.UI.ModernButton();
            this.BtnSaveAs = new ImageGlass.UI.ModernButton();
            this.BtnCopy = new ImageGlass.UI.ModernButton();
            this.BtnCrop = new ImageGlass.UI.ModernButton();
            this.TooltipMain = new System.Windows.Forms.ToolTip(this.components);
            this.TableTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumRatioFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumRatioTo)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.TableBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // TableTop
            // 
            this.TableTop.AutoSize = true;
            this.TableTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableTop.ColumnCount = 3;
            this.TableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableTop.Controls.Add(this.LblLocation, 0, 2);
            this.TableTop.Controls.Add(this.LblSize, 0, 3);
            this.TableTop.Controls.Add(this.BtnSettings, 2, 4);
            this.TableTop.Controls.Add(this.LblAspectRatio, 0, 0);
            this.TableTop.Controls.Add(this.NumX, 1, 2);
            this.TableTop.Controls.Add(this.NumY, 2, 2);
            this.TableTop.Controls.Add(this.NumWidth, 1, 3);
            this.TableTop.Controls.Add(this.NumHeight, 2, 3);
            this.TableTop.Controls.Add(this.CmbAspectRatio, 1, 0);
            this.TableTop.Controls.Add(this.NumRatioFrom, 1, 1);
            this.TableTop.Controls.Add(this.NumRatioTo, 2, 1);
            this.TableTop.Controls.Add(this.flowLayoutPanel1, 0, 4);
            this.TableTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.TableTop.Location = new System.Drawing.Point(0, 0);
            this.TableTop.Margin = new System.Windows.Forms.Padding(0);
            this.TableTop.Name = "TableTop";
            this.TableTop.Padding = new System.Windows.Forms.Padding(40);
            this.TableTop.RowCount = 6;
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTop.Size = new System.Drawing.Size(660, 439);
            this.TableTop.TabIndex = 0;
            // 
            // LblLocation
            // 
            this.LblLocation.AutoSize = true;
            this.LblLocation.BackColor = System.Drawing.Color.Transparent;
            this.LblLocation.DarkMode = true;
            this.LblLocation.Location = new System.Drawing.Point(40, 171);
            this.LblLocation.Margin = new System.Windows.Forms.Padding(0, 10, 20, 10);
            this.LblLocation.Name = "LblLocation";
            this.LblLocation.Size = new System.Drawing.Size(168, 45);
            this.LblLocation.TabIndex = 0;
            this.LblLocation.Text = "[Location:]";
            // 
            // LblSize
            // 
            this.LblSize.AutoSize = true;
            this.LblSize.BackColor = System.Drawing.Color.Transparent;
            this.LblSize.DarkMode = true;
            this.LblSize.Location = new System.Drawing.Point(40, 231);
            this.LblSize.Margin = new System.Windows.Forms.Padding(0, 0, 20, 10);
            this.LblSize.Name = "LblSize";
            this.LblSize.Size = new System.Drawing.Size(103, 45);
            this.LblSize.TabIndex = 1;
            this.LblSize.Text = "[Size:]";
            // 
            // BtnSettings
            // 
            this.BtnSettings.DarkMode = true;
            this.BtnSettings.Dock = System.Windows.Forms.DockStyle.Right;
            this.BtnSettings.Image = ((System.Drawing.Image)(resources.GetObject("BtnSettings.Image")));
            this.BtnSettings.ImagePadding = 0;
            this.BtnSettings.Location = new System.Drawing.Point(500, 311);
            this.BtnSettings.Margin = new System.Windows.Forms.Padding(0, 20, 0, 10);
            this.BtnSettings.Name = "BtnSettings";
            this.BtnSettings.Padding = new System.Windows.Forms.Padding(10);
            this.BtnSettings.Size = new System.Drawing.Size(120, 78);
            this.BtnSettings.SvgIcon = ImageGlass.Base.IconName.Setting;
            this.BtnSettings.SystemIcon = null;
            this.BtnSettings.TabIndex = 8;
            this.BtnSettings.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnSettings.Click += new System.EventHandler(this.BtnSettings_Click);
            // 
            // LblAspectRatio
            // 
            this.LblAspectRatio.AutoSize = true;
            this.LblAspectRatio.BackColor = System.Drawing.Color.Transparent;
            this.LblAspectRatio.DarkMode = true;
            this.LblAspectRatio.Location = new System.Drawing.Point(40, 44);
            this.LblAspectRatio.Margin = new System.Windows.Forms.Padding(0, 4, 4, 10);
            this.LblAspectRatio.Name = "LblAspectRatio";
            this.LblAspectRatio.Size = new System.Drawing.Size(218, 45);
            this.LblAspectRatio.TabIndex = 4;
            this.LblAspectRatio.Text = "[Aspect ratio:]";
            // 
            // NumX
            // 
            this.NumX.DarkMode = true;
            this.NumX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NumX.Location = new System.Drawing.Point(262, 171);
            this.NumX.Margin = new System.Windows.Forms.Padding(0, 10, 10, 10);
            this.NumX.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumX.Name = "NumX";
            this.NumX.SelectAllTextOnFocus = true;
            this.NumX.Size = new System.Drawing.Size(169, 50);
            this.NumX.TabIndex = 3;
            this.NumX.ThousandsSeparator = true;
            // 
            // NumY
            // 
            this.NumY.DarkMode = true;
            this.NumY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NumY.Location = new System.Drawing.Point(451, 171);
            this.NumY.Margin = new System.Windows.Forms.Padding(10, 10, 0, 10);
            this.NumY.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumY.Name = "NumY";
            this.NumY.SelectAllTextOnFocus = true;
            this.NumY.Size = new System.Drawing.Size(169, 50);
            this.NumY.TabIndex = 4;
            this.NumY.ThousandsSeparator = true;
            // 
            // NumWidth
            // 
            this.NumWidth.DarkMode = true;
            this.NumWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NumWidth.Location = new System.Drawing.Point(262, 231);
            this.NumWidth.Margin = new System.Windows.Forms.Padding(0, 0, 10, 10);
            this.NumWidth.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumWidth.Name = "NumWidth";
            this.NumWidth.SelectAllTextOnFocus = true;
            this.NumWidth.Size = new System.Drawing.Size(169, 50);
            this.NumWidth.TabIndex = 5;
            this.NumWidth.ThousandsSeparator = true;
            // 
            // NumHeight
            // 
            this.NumHeight.DarkMode = true;
            this.NumHeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NumHeight.Location = new System.Drawing.Point(451, 231);
            this.NumHeight.Margin = new System.Windows.Forms.Padding(10, 0, 0, 10);
            this.NumHeight.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumHeight.Name = "NumHeight";
            this.NumHeight.SelectAllTextOnFocus = true;
            this.NumHeight.Size = new System.Drawing.Size(169, 50);
            this.NumHeight.TabIndex = 6;
            this.NumHeight.ThousandsSeparator = true;
            // 
            // CmbAspectRatio
            // 
            this.TableTop.SetColumnSpan(this.CmbAspectRatio, 2);
            this.CmbAspectRatio.DarkMode = true;
            this.CmbAspectRatio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CmbAspectRatio.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbAspectRatio.FormattingEnabled = true;
            this.CmbAspectRatio.Items.AddRange(new object[] {
            "Free ratio",
            "Custom...",
            "Original",
            "1:1",
            "1:2",
            "3:2",
            "4:3",
            "16:9"});
            this.CmbAspectRatio.Location = new System.Drawing.Point(262, 40);
            this.CmbAspectRatio.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.CmbAspectRatio.Name = "CmbAspectRatio";
            this.CmbAspectRatio.Size = new System.Drawing.Size(358, 51);
            this.CmbAspectRatio.TabIndex = 0;
            this.CmbAspectRatio.SelectedIndexChanged += new System.EventHandler(this.CmbAspectRatio_SelectedIndexChanged);
            // 
            // NumRatioFrom
            // 
            this.NumRatioFrom.DarkMode = true;
            this.NumRatioFrom.Dock = System.Windows.Forms.DockStyle.Top;
            this.NumRatioFrom.Location = new System.Drawing.Point(262, 101);
            this.NumRatioFrom.Margin = new System.Windows.Forms.Padding(0, 0, 10, 10);
            this.NumRatioFrom.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumRatioFrom.Name = "NumRatioFrom";
            this.NumRatioFrom.SelectAllTextOnFocus = true;
            this.NumRatioFrom.Size = new System.Drawing.Size(169, 50);
            this.NumRatioFrom.TabIndex = 1;
            this.NumRatioFrom.ValueChanged += new System.EventHandler(this.NumRatio_ValueChanged);
            // 
            // NumRatioTo
            // 
            this.NumRatioTo.DarkMode = true;
            this.NumRatioTo.Dock = System.Windows.Forms.DockStyle.Top;
            this.NumRatioTo.Location = new System.Drawing.Point(451, 101);
            this.NumRatioTo.Margin = new System.Windows.Forms.Padding(10, 0, 0, 10);
            this.NumRatioTo.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumRatioTo.Name = "NumRatioTo";
            this.NumRatioTo.SelectAllTextOnFocus = true;
            this.NumRatioTo.Size = new System.Drawing.Size(169, 50);
            this.NumRatioTo.TabIndex = 2;
            this.NumRatioTo.ValueChanged += new System.EventHandler(this.NumRatio_ValueChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableTop.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.BtnReset);
            this.flowLayoutPanel1.Controls.Add(this.BtnQuickSelect);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(40, 291);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 20, 12, 10);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(401, 108);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // BtnReset
            // 
            this.BtnReset.DarkMode = true;
            this.BtnReset.Image = ((System.Drawing.Image)(resources.GetObject("BtnReset.Image")));
            this.BtnReset.ImagePadding = 0;
            this.BtnReset.Location = new System.Drawing.Point(269, 20);
            this.BtnReset.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.BtnReset.Name = "BtnReset";
            this.BtnReset.Padding = new System.Windows.Forms.Padding(10);
            this.BtnReset.Size = new System.Drawing.Size(120, 78);
            this.BtnReset.SvgIcon = ImageGlass.Base.IconName.ResetSelection;
            this.BtnReset.SystemIcon = null;
            this.BtnReset.TabIndex = 1;
            this.BtnReset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnReset.Click += new System.EventHandler(this.BtnReset_Click);
            // 
            // BtnQuickSelect
            // 
            this.BtnQuickSelect.DarkMode = true;
            this.BtnQuickSelect.Image = ((System.Drawing.Image)(resources.GetObject("BtnQuickSelect.Image")));
            this.BtnQuickSelect.ImagePadding = 0;
            this.BtnQuickSelect.Location = new System.Drawing.Point(137, 20);
            this.BtnQuickSelect.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.BtnQuickSelect.Name = "BtnQuickSelect";
            this.BtnQuickSelect.Padding = new System.Windows.Forms.Padding(10);
            this.BtnQuickSelect.Size = new System.Drawing.Size(120, 78);
            this.BtnQuickSelect.SvgIcon = ImageGlass.Base.IconName.Selection;
            this.BtnQuickSelect.SystemIcon = null;
            this.BtnQuickSelect.TabIndex = 0;
            this.BtnQuickSelect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnQuickSelect.Visible = false;
            this.BtnQuickSelect.Click += new System.EventHandler(this.BtnQuickSelect_Click);
            // 
            // TableBottom
            // 
            this.TableBottom.AutoSize = true;
            this.TableBottom.ColumnCount = 2;
            this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableBottom.Controls.Add(this.BtnSave, 0, 0);
            this.TableBottom.Controls.Add(this.BtnSaveAs, 0, 1);
            this.TableBottom.Controls.Add(this.BtnCopy, 1, 1);
            this.TableBottom.Controls.Add(this.BtnCrop, 1, 0);
            this.TableBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.TableBottom.Location = new System.Drawing.Point(0, 504);
            this.TableBottom.Margin = new System.Windows.Forms.Padding(6);
            this.TableBottom.Name = "TableBottom";
            this.TableBottom.Padding = new System.Windows.Forms.Padding(40, 30, 40, 30);
            this.TableBottom.RowCount = 2;
            this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableBottom.Size = new System.Drawing.Size(660, 228);
            this.TableBottom.TabIndex = 1;
            // 
            // BtnSave
            // 
            this.BtnSave.ButtonStyle = ImageGlass.UI.ModernButtonStyle.CTA;
            this.BtnSave.DarkMode = true;
            this.BtnSave.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BtnSave.ImagePadding = 2;
            this.BtnSave.Location = new System.Drawing.Point(40, 30);
            this.BtnSave.Margin = new System.Windows.Forms.Padding(0, 0, 6, 6);
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Padding = new System.Windows.Forms.Padding(10);
            this.BtnSave.Size = new System.Drawing.Size(284, 78);
            this.BtnSave.SystemIcon = null;
            this.BtnSave.TabIndex = 0;
            this.BtnSave.Text = "[Save]";
            this.BtnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // BtnSaveAs
            // 
            this.BtnSaveAs.ButtonStyle = ImageGlass.UI.ModernButtonStyle.CTA;
            this.BtnSaveAs.DarkMode = true;
            this.BtnSaveAs.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BtnSaveAs.ImagePadding = 2;
            this.BtnSaveAs.Location = new System.Drawing.Point(40, 120);
            this.BtnSaveAs.Margin = new System.Windows.Forms.Padding(0, 6, 6, 0);
            this.BtnSaveAs.Name = "BtnSaveAs";
            this.BtnSaveAs.Padding = new System.Windows.Forms.Padding(10);
            this.BtnSaveAs.Size = new System.Drawing.Size(284, 78);
            this.BtnSaveAs.SystemIcon = null;
            this.BtnSaveAs.TabIndex = 1;
            this.BtnSaveAs.Text = "[Save as...]";
            this.BtnSaveAs.Click += new System.EventHandler(this.BtnSaveAs_Click);
            // 
            // BtnCopy
            // 
            this.BtnCopy.DarkMode = true;
            this.BtnCopy.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BtnCopy.Image = ((System.Drawing.Image)(resources.GetObject("BtnCopy.Image")));
            this.BtnCopy.ImagePadding = 2;
            this.BtnCopy.Location = new System.Drawing.Point(336, 120);
            this.BtnCopy.Margin = new System.Windows.Forms.Padding(6, 6, 0, 0);
            this.BtnCopy.Name = "BtnCopy";
            this.BtnCopy.Padding = new System.Windows.Forms.Padding(10);
            this.BtnCopy.Size = new System.Drawing.Size(284, 78);
            this.BtnCopy.SvgIcon = ImageGlass.Base.IconName.Copy;
            this.BtnCopy.SystemIcon = null;
            this.BtnCopy.TabIndex = 3;
            this.BtnCopy.Text = "[Copy]";
            this.BtnCopy.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnCopy.Click += new System.EventHandler(this.BtnCopy_Click);
            // 
            // BtnCrop
            // 
            this.BtnCrop.DarkMode = true;
            this.BtnCrop.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BtnCrop.ImagePadding = 2;
            this.BtnCrop.Location = new System.Drawing.Point(336, 30);
            this.BtnCrop.Margin = new System.Windows.Forms.Padding(6, 0, 0, 6);
            this.BtnCrop.Name = "BtnCrop";
            this.BtnCrop.Padding = new System.Windows.Forms.Padding(10);
            this.BtnCrop.Size = new System.Drawing.Size(284, 78);
            this.BtnCrop.SystemIcon = null;
            this.BtnCrop.TabIndex = 2;
            this.BtnCrop.Text = "[Crop only]";
            this.BtnCrop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnCrop.Click += new System.EventHandler(this.BtnCrop_Click);
            // 
            // FrmCrop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(18F, 45F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 732);
            this.Controls.Add(this.TableBottom);
            this.Controls.Add(this.TableTop);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(0, 0);
            this.Margin = new System.Windows.Forms.Padding(12);
            this.Name = "FrmCrop";
            this.Opacity = 0.85D;
            this.Text = "[Crop tool]";
            this.TableTop.ResumeLayout(false);
            this.TableTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumRatioFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumRatioTo)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.TableBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TableLayoutPanel TableTop;
        private ModernLabel LblLocation;
        private ModernLabel LblSize;
        private ModernLabel LblAspectRatio;
        private ModernNumericUpDown NumX;
        private ModernNumericUpDown NumY;
        private ModernNumericUpDown NumWidth;
        private ModernNumericUpDown NumHeight;
        private ModernComboBox CmbAspectRatio;
        private TableLayoutPanel TableBottom;
        private ModernButton BtnSave;
        private ModernButton BtnSaveAs;
        private ModernButton BtnCopy;
        private ModernButton BtnCrop;
        private ModernNumericUpDown NumRatioFrom;
        private ModernNumericUpDown NumRatioTo;
        private ToolTip TooltipMain;
        private FlowLayoutPanel flowLayoutPanel1;
        private ModernButton BtnReset;
        private ModernButton BtnQuickSelect;
        private ModernButton BtnSettings;
    }
}