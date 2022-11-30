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
            this.TableTop = new System.Windows.Forms.TableLayoutPanel();
            this.LblLocation = new ImageGlass.UI.ModernLabel();
            this.LblSize = new ImageGlass.UI.ModernLabel();
            this.LblAspectRatio = new ImageGlass.UI.ModernLabel();
            this.NumX = new ImageGlass.UI.ModernNumericUpDown();
            this.NumY = new ImageGlass.UI.ModernNumericUpDown();
            this.NumWidth = new ImageGlass.UI.ModernNumericUpDown();
            this.NumHeight = new ImageGlass.UI.ModernNumericUpDown();
            this.CmbAspectRatio = new ImageGlass.UI.ModernComboBox();
            this.NumRatioFrom = new ImageGlass.UI.ModernNumericUpDown();
            this.NumRatioTo = new ImageGlass.UI.ModernNumericUpDown();
            this.TableBottom = new System.Windows.Forms.TableLayoutPanel();
            this.BtnSave = new ImageGlass.UI.ModernButton();
            this.BtnSaveAs = new ImageGlass.UI.ModernButton();
            this.BtnCopy = new ImageGlass.UI.ModernButton();
            this.BtnReset = new ImageGlass.UI.ModernButton();
            this.TooltipMain = new System.Windows.Forms.ToolTip(this.components);
            this.TableTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumRatioFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumRatioTo)).BeginInit();
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
            this.TableTop.Controls.Add(this.LblLocation, 0, 0);
            this.TableTop.Controls.Add(this.LblSize, 0, 1);
            this.TableTop.Controls.Add(this.LblAspectRatio, 0, 2);
            this.TableTop.Controls.Add(this.NumX, 1, 0);
            this.TableTop.Controls.Add(this.NumY, 2, 0);
            this.TableTop.Controls.Add(this.NumWidth, 1, 1);
            this.TableTop.Controls.Add(this.NumHeight, 2, 1);
            this.TableTop.Controls.Add(this.CmbAspectRatio, 1, 2);
            this.TableTop.Controls.Add(this.NumRatioFrom, 1, 3);
            this.TableTop.Controls.Add(this.NumRatioTo, 2, 3);
            this.TableTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.TableTop.Location = new System.Drawing.Point(0, 0);
            this.TableTop.Margin = new System.Windows.Forms.Padding(0);
            this.TableTop.Name = "TableTop";
            this.TableTop.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.TableTop.RowCount = 4;
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableTop.Size = new System.Drawing.Size(330, 171);
            this.TableTop.TabIndex = 0;
            // 
            // LblLocation
            // 
            this.LblLocation.AutoSize = true;
            this.LblLocation.BackColor = System.Drawing.Color.Transparent;
            this.LblLocation.DarkMode = true;
            this.LblLocation.Location = new System.Drawing.Point(20, 10);
            this.LblLocation.Margin = new System.Windows.Forms.Padding(0, 0, 10, 5);
            this.LblLocation.Name = "LblLocation";
            this.LblLocation.Size = new System.Drawing.Size(89, 23);
            this.LblLocation.TabIndex = 0;
            this.LblLocation.Text = "[Location:]";
            // 
            // LblSize
            // 
            this.LblSize.AutoSize = true;
            this.LblSize.BackColor = System.Drawing.Color.Transparent;
            this.LblSize.DarkMode = true;
            this.LblSize.Location = new System.Drawing.Point(20, 45);
            this.LblSize.Margin = new System.Windows.Forms.Padding(0, 0, 10, 5);
            this.LblSize.Name = "LblSize";
            this.LblSize.Size = new System.Drawing.Size(54, 23);
            this.LblSize.TabIndex = 1;
            this.LblSize.Text = "[Size:]";
            // 
            // LblAspectRatio
            // 
            this.LblAspectRatio.AutoSize = true;
            this.LblAspectRatio.BackColor = System.Drawing.Color.Transparent;
            this.LblAspectRatio.DarkMode = true;
            this.LblAspectRatio.Location = new System.Drawing.Point(20, 92);
            this.LblAspectRatio.Margin = new System.Windows.Forms.Padding(0, 12, 10, 5);
            this.LblAspectRatio.Name = "LblAspectRatio";
            this.LblAspectRatio.Size = new System.Drawing.Size(115, 23);
            this.LblAspectRatio.TabIndex = 4;
            this.LblAspectRatio.Text = "[Aspect ratio:]";
            // 
            // NumX
            // 
            this.NumX.DarkMode = true;
            this.NumX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NumX.Location = new System.Drawing.Point(145, 10);
            this.NumX.Margin = new System.Windows.Forms.Padding(0, 0, 5, 5);
            this.NumX.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumX.Name = "NumX";
            this.NumX.SelectAllTextOnFocus = true;
            this.NumX.Size = new System.Drawing.Size(77, 30);
            this.NumX.TabIndex = 6;
            this.NumX.ThousandsSeparator = true;
            // 
            // NumY
            // 
            this.NumY.DarkMode = true;
            this.NumY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NumY.Location = new System.Drawing.Point(232, 10);
            this.NumY.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
            this.NumY.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumY.Name = "NumY";
            this.NumY.SelectAllTextOnFocus = true;
            this.NumY.Size = new System.Drawing.Size(78, 30);
            this.NumY.TabIndex = 7;
            this.NumY.ThousandsSeparator = true;
            // 
            // NumWidth
            // 
            this.NumWidth.DarkMode = true;
            this.NumWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NumWidth.Location = new System.Drawing.Point(145, 45);
            this.NumWidth.Margin = new System.Windows.Forms.Padding(0, 0, 5, 5);
            this.NumWidth.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumWidth.Name = "NumWidth";
            this.NumWidth.SelectAllTextOnFocus = true;
            this.NumWidth.Size = new System.Drawing.Size(77, 30);
            this.NumWidth.TabIndex = 8;
            this.NumWidth.ThousandsSeparator = true;
            // 
            // NumHeight
            // 
            this.NumHeight.DarkMode = true;
            this.NumHeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NumHeight.Location = new System.Drawing.Point(232, 45);
            this.NumHeight.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
            this.NumHeight.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumHeight.Name = "NumHeight";
            this.NumHeight.SelectAllTextOnFocus = true;
            this.NumHeight.Size = new System.Drawing.Size(78, 30);
            this.NumHeight.TabIndex = 9;
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
            this.CmbAspectRatio.Location = new System.Drawing.Point(145, 90);
            this.CmbAspectRatio.Margin = new System.Windows.Forms.Padding(0, 10, 0, 5);
            this.CmbAspectRatio.Name = "CmbAspectRatio";
            this.CmbAspectRatio.Size = new System.Drawing.Size(165, 31);
            this.CmbAspectRatio.TabIndex = 10;
            this.CmbAspectRatio.SelectedIndexChanged += new System.EventHandler(this.CmbAspectRatio_SelectedIndexChanged);
            // 
            // NumRatioFrom
            // 
            this.NumRatioFrom.DarkMode = true;
            this.NumRatioFrom.Dock = System.Windows.Forms.DockStyle.Top;
            this.NumRatioFrom.Location = new System.Drawing.Point(145, 126);
            this.NumRatioFrom.Margin = new System.Windows.Forms.Padding(0, 0, 5, 5);
            this.NumRatioFrom.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumRatioFrom.Name = "NumRatioFrom";
            this.NumRatioFrom.SelectAllTextOnFocus = true;
            this.NumRatioFrom.Size = new System.Drawing.Size(77, 30);
            this.NumRatioFrom.TabIndex = 11;
            this.NumRatioFrom.ValueChanged += new System.EventHandler(this.NumRatio_ValueChanged);
            // 
            // NumRatioTo
            // 
            this.NumRatioTo.DarkMode = true;
            this.NumRatioTo.Dock = System.Windows.Forms.DockStyle.Top;
            this.NumRatioTo.Location = new System.Drawing.Point(232, 126);
            this.NumRatioTo.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
            this.NumRatioTo.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumRatioTo.Name = "NumRatioTo";
            this.NumRatioTo.SelectAllTextOnFocus = true;
            this.NumRatioTo.Size = new System.Drawing.Size(78, 30);
            this.NumRatioTo.TabIndex = 12;
            this.NumRatioTo.ValueChanged += new System.EventHandler(this.NumRatio_ValueChanged);
            // 
            // TableBottom
            // 
            this.TableBottom.AutoSize = true;
            this.TableBottom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableBottom.ColumnCount = 2;
            this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableBottom.Controls.Add(this.BtnSave, 0, 0);
            this.TableBottom.Controls.Add(this.BtnSaveAs, 1, 0);
            this.TableBottom.Controls.Add(this.BtnCopy, 0, 1);
            this.TableBottom.Controls.Add(this.BtnReset, 1, 1);
            this.TableBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.TableBottom.Location = new System.Drawing.Point(0, 192);
            this.TableBottom.Name = "TableBottom";
            this.TableBottom.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.TableBottom.RowCount = 2;
            this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableBottom.Size = new System.Drawing.Size(330, 106);
            this.TableBottom.TabIndex = 1;
            // 
            // BtnSave
            // 
            this.BtnSave.DarkMode = true;
            this.BtnSave.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BtnSave.ImagePadding = 2;
            this.BtnSave.Location = new System.Drawing.Point(20, 10);
            this.BtnSave.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Padding = new System.Windows.Forms.Padding(5);
            this.BtnSave.Size = new System.Drawing.Size(142, 40);
            this.BtnSave.SystemIcon = null;
            this.BtnSave.TabIndex = 0;
            this.BtnSave.Text = "[Save]";
            this.BtnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // BtnSaveAs
            // 
            this.BtnSaveAs.DarkMode = true;
            this.BtnSaveAs.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BtnSaveAs.ImagePadding = 2;
            this.BtnSaveAs.Location = new System.Drawing.Point(168, 10);
            this.BtnSaveAs.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.BtnSaveAs.Name = "BtnSaveAs";
            this.BtnSaveAs.Padding = new System.Windows.Forms.Padding(5);
            this.BtnSaveAs.Size = new System.Drawing.Size(142, 40);
            this.BtnSaveAs.SystemIcon = null;
            this.BtnSaveAs.TabIndex = 1;
            this.BtnSaveAs.Text = "[Save as...]";
            this.BtnSaveAs.Click += new System.EventHandler(this.BtnSaveAs_Click);
            // 
            // BtnCopy
            // 
            this.BtnCopy.DarkMode = true;
            this.BtnCopy.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BtnCopy.ImagePadding = 2;
            this.BtnCopy.Location = new System.Drawing.Point(20, 56);
            this.BtnCopy.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.BtnCopy.Name = "BtnCopy";
            this.BtnCopy.Padding = new System.Windows.Forms.Padding(5);
            this.BtnCopy.Size = new System.Drawing.Size(142, 40);
            this.BtnCopy.SystemIcon = null;
            this.BtnCopy.TabIndex = 2;
            this.BtnCopy.Text = "[Copy]";
            this.BtnCopy.Click += new System.EventHandler(this.BtnCopy_Click);
            // 
            // BtnReset
            // 
            this.BtnReset.DarkMode = true;
            this.BtnReset.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BtnReset.ImagePadding = 2;
            this.BtnReset.Location = new System.Drawing.Point(168, 56);
            this.BtnReset.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            this.BtnReset.Name = "BtnReset";
            this.BtnReset.Padding = new System.Windows.Forms.Padding(5);
            this.BtnReset.Size = new System.Drawing.Size(142, 40);
            this.BtnReset.SystemIcon = null;
            this.BtnReset.TabIndex = 3;
            this.BtnReset.Text = "[Reset]";
            this.BtnReset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnReset.Click += new System.EventHandler(this.BtnReset_Click);
            // 
            // FrmCrop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(330, 298);
            this.Controls.Add(this.TableBottom);
            this.Controls.Add(this.TableTop);
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "FrmCrop";
            this.Text = "[Crop tool]";
            this.TableTop.ResumeLayout(false);
            this.TableTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumRatioFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumRatioTo)).EndInit();
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
        private ModernButton BtnReset;
        private ModernNumericUpDown NumRatioFrom;
        private ModernNumericUpDown NumRatioTo;
        private ToolTip TooltipMain;
    }
}