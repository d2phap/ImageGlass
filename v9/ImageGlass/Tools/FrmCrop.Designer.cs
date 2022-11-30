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
            this.tableTop = new System.Windows.Forms.TableLayoutPanel();
            this.lblLocation = new ImageGlass.UI.ModernLabel();
            this.lblSize = new ImageGlass.UI.ModernLabel();
            this.lblAspectRatio = new ImageGlass.UI.ModernLabel();
            this.numX = new ImageGlass.UI.ModernNumericUpDown();
            this.numY = new ImageGlass.UI.ModernNumericUpDown();
            this.numWidth = new ImageGlass.UI.ModernNumericUpDown();
            this.numHeight = new ImageGlass.UI.ModernNumericUpDown();
            this.cmbAspectRatio = new ImageGlass.UI.ModernComboBox();
            this.numRatioFrom = new ImageGlass.UI.ModernNumericUpDown();
            this.numRatioTo = new ImageGlass.UI.ModernNumericUpDown();
            this.tableBottom = new System.Windows.Forms.TableLayoutPanel();
            this.btnSave = new ImageGlass.UI.ModernButton();
            this.btnSaveAs = new ImageGlass.UI.ModernButton();
            this.btnCopy = new ImageGlass.UI.ModernButton();
            this.btnReset = new ImageGlass.UI.ModernButton();
            this.tableTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRatioFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRatioTo)).BeginInit();
            this.tableBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableTop
            // 
            this.tableTop.AutoSize = true;
            this.tableTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableTop.ColumnCount = 3;
            this.tableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableTop.Controls.Add(this.lblLocation, 0, 0);
            this.tableTop.Controls.Add(this.lblSize, 0, 1);
            this.tableTop.Controls.Add(this.lblAspectRatio, 0, 2);
            this.tableTop.Controls.Add(this.numX, 1, 0);
            this.tableTop.Controls.Add(this.numY, 2, 0);
            this.tableTop.Controls.Add(this.numWidth, 1, 1);
            this.tableTop.Controls.Add(this.numHeight, 2, 1);
            this.tableTop.Controls.Add(this.cmbAspectRatio, 1, 2);
            this.tableTop.Controls.Add(this.numRatioFrom, 1, 3);
            this.tableTop.Controls.Add(this.numRatioTo, 2, 3);
            this.tableTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableTop.Location = new System.Drawing.Point(0, 0);
            this.tableTop.Margin = new System.Windows.Forms.Padding(0);
            this.tableTop.Name = "tableTop";
            this.tableTop.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.tableTop.RowCount = 4;
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableTop.Size = new System.Drawing.Size(330, 171);
            this.tableTop.TabIndex = 0;
            // 
            // lblLocation
            // 
            this.lblLocation.AutoSize = true;
            this.lblLocation.BackColor = System.Drawing.Color.Transparent;
            this.lblLocation.DarkMode = true;
            this.lblLocation.Location = new System.Drawing.Point(20, 10);
            this.lblLocation.Margin = new System.Windows.Forms.Padding(0, 0, 10, 5);
            this.lblLocation.Name = "lblLocation";
            this.lblLocation.Size = new System.Drawing.Size(89, 23);
            this.lblLocation.TabIndex = 0;
            this.lblLocation.Text = "[Location:]";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.BackColor = System.Drawing.Color.Transparent;
            this.lblSize.DarkMode = true;
            this.lblSize.Location = new System.Drawing.Point(20, 45);
            this.lblSize.Margin = new System.Windows.Forms.Padding(0, 0, 10, 5);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(54, 23);
            this.lblSize.TabIndex = 1;
            this.lblSize.Text = "[Size:]";
            // 
            // lblAspectRatio
            // 
            this.lblAspectRatio.AutoSize = true;
            this.lblAspectRatio.BackColor = System.Drawing.Color.Transparent;
            this.lblAspectRatio.DarkMode = true;
            this.lblAspectRatio.Location = new System.Drawing.Point(20, 92);
            this.lblAspectRatio.Margin = new System.Windows.Forms.Padding(0, 12, 10, 5);
            this.lblAspectRatio.Name = "lblAspectRatio";
            this.lblAspectRatio.Size = new System.Drawing.Size(115, 23);
            this.lblAspectRatio.TabIndex = 4;
            this.lblAspectRatio.Text = "[Aspect ratio:]";
            // 
            // numX
            // 
            this.numX.DarkMode = true;
            this.numX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numX.Location = new System.Drawing.Point(145, 10);
            this.numX.Margin = new System.Windows.Forms.Padding(0, 0, 5, 5);
            this.numX.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numX.Name = "numX";
            this.numX.SelectAllTextOnFocus = true;
            this.numX.Size = new System.Drawing.Size(77, 30);
            this.numX.TabIndex = 6;
            this.numX.ThousandsSeparator = true;
            // 
            // numY
            // 
            this.numY.DarkMode = true;
            this.numY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numY.Location = new System.Drawing.Point(232, 10);
            this.numY.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
            this.numY.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numY.Name = "numY";
            this.numY.SelectAllTextOnFocus = true;
            this.numY.Size = new System.Drawing.Size(78, 30);
            this.numY.TabIndex = 7;
            this.numY.ThousandsSeparator = true;
            // 
            // numWidth
            // 
            this.numWidth.DarkMode = true;
            this.numWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numWidth.Location = new System.Drawing.Point(145, 45);
            this.numWidth.Margin = new System.Windows.Forms.Padding(0, 0, 5, 5);
            this.numWidth.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numWidth.Name = "numWidth";
            this.numWidth.SelectAllTextOnFocus = true;
            this.numWidth.Size = new System.Drawing.Size(77, 30);
            this.numWidth.TabIndex = 8;
            this.numWidth.ThousandsSeparator = true;
            // 
            // numHeight
            // 
            this.numHeight.DarkMode = true;
            this.numHeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numHeight.Location = new System.Drawing.Point(232, 45);
            this.numHeight.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
            this.numHeight.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numHeight.Name = "numHeight";
            this.numHeight.SelectAllTextOnFocus = true;
            this.numHeight.Size = new System.Drawing.Size(78, 30);
            this.numHeight.TabIndex = 9;
            this.numHeight.ThousandsSeparator = true;
            // 
            // cmbAspectRatio
            // 
            this.tableTop.SetColumnSpan(this.cmbAspectRatio, 2);
            this.cmbAspectRatio.DarkMode = true;
            this.cmbAspectRatio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbAspectRatio.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmbAspectRatio.FormattingEnabled = true;
            this.cmbAspectRatio.Items.AddRange(new object[] {
            "Free ratio",
            "Custom...",
            "Original",
            "1:1",
            "1:2",
            "3:2",
            "4:3",
            "16:9"});
            this.cmbAspectRatio.Location = new System.Drawing.Point(145, 90);
            this.cmbAspectRatio.Margin = new System.Windows.Forms.Padding(0, 10, 0, 5);
            this.cmbAspectRatio.Name = "cmbAspectRatio";
            this.cmbAspectRatio.Size = new System.Drawing.Size(165, 31);
            this.cmbAspectRatio.TabIndex = 10;
            this.cmbAspectRatio.SelectedIndexChanged += new System.EventHandler(this.cmbAspectRatio_SelectedIndexChanged);
            // 
            // numRatioFrom
            // 
            this.numRatioFrom.DarkMode = true;
            this.numRatioFrom.Dock = System.Windows.Forms.DockStyle.Top;
            this.numRatioFrom.Location = new System.Drawing.Point(145, 126);
            this.numRatioFrom.Margin = new System.Windows.Forms.Padding(0, 0, 5, 5);
            this.numRatioFrom.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numRatioFrom.Name = "numRatioFrom";
            this.numRatioFrom.SelectAllTextOnFocus = true;
            this.numRatioFrom.Size = new System.Drawing.Size(77, 30);
            this.numRatioFrom.TabIndex = 11;
            // 
            // numRatioTo
            // 
            this.numRatioTo.DarkMode = true;
            this.numRatioTo.Dock = System.Windows.Forms.DockStyle.Top;
            this.numRatioTo.Location = new System.Drawing.Point(232, 126);
            this.numRatioTo.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
            this.numRatioTo.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numRatioTo.Name = "numRatioTo";
            this.numRatioTo.SelectAllTextOnFocus = true;
            this.numRatioTo.Size = new System.Drawing.Size(78, 30);
            this.numRatioTo.TabIndex = 12;
            // 
            // tableBottom
            // 
            this.tableBottom.AutoSize = true;
            this.tableBottom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableBottom.ColumnCount = 2;
            this.tableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableBottom.Controls.Add(this.btnSave, 0, 0);
            this.tableBottom.Controls.Add(this.btnSaveAs, 1, 0);
            this.tableBottom.Controls.Add(this.btnCopy, 0, 1);
            this.tableBottom.Controls.Add(this.btnReset, 1, 1);
            this.tableBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableBottom.Location = new System.Drawing.Point(0, 192);
            this.tableBottom.Name = "tableBottom";
            this.tableBottom.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.tableBottom.RowCount = 2;
            this.tableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableBottom.Size = new System.Drawing.Size(330, 106);
            this.tableBottom.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.DarkMode = true;
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnSave.ImagePadding = 2;
            this.btnSave.Location = new System.Drawing.Point(20, 10);
            this.btnSave.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Padding = new System.Windows.Forms.Padding(5);
            this.btnSave.Size = new System.Drawing.Size(142, 40);
            this.btnSave.SystemIcon = null;
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "[Save]";
            this.btnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.DarkMode = true;
            this.btnSaveAs.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnSaveAs.ImagePadding = 2;
            this.btnSaveAs.Location = new System.Drawing.Point(168, 10);
            this.btnSaveAs.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Padding = new System.Windows.Forms.Padding(5);
            this.btnSaveAs.Size = new System.Drawing.Size(142, 40);
            this.btnSaveAs.SystemIcon = null;
            this.btnSaveAs.TabIndex = 1;
            this.btnSaveAs.Text = "[Save as...]";
            // 
            // btnCopy
            // 
            this.btnCopy.DarkMode = true;
            this.btnCopy.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCopy.ImagePadding = 2;
            this.btnCopy.Location = new System.Drawing.Point(20, 56);
            this.btnCopy.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Padding = new System.Windows.Forms.Padding(5);
            this.btnCopy.Size = new System.Drawing.Size(142, 40);
            this.btnCopy.SystemIcon = null;
            this.btnCopy.TabIndex = 2;
            this.btnCopy.Text = "[Copy]";
            // 
            // btnReset
            // 
            this.btnReset.DarkMode = true;
            this.btnReset.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnReset.ImagePadding = 2;
            this.btnReset.Location = new System.Drawing.Point(168, 56);
            this.btnReset.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            this.btnReset.Name = "btnReset";
            this.btnReset.Padding = new System.Windows.Forms.Padding(5);
            this.btnReset.Size = new System.Drawing.Size(142, 40);
            this.btnReset.SystemIcon = null;
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "[Reset]";
            this.btnReset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // FrmCrop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(330, 298);
            this.Controls.Add(this.tableBottom);
            this.Controls.Add(this.tableTop);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "FrmCrop";
            this.Text = "[Crop tool]";
            this.tableTop.ResumeLayout(false);
            this.tableTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRatioFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRatioTo)).EndInit();
            this.tableBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TableLayoutPanel tableTop;
        private ModernLabel lblLocation;
        private ModernLabel lblSize;
        private ModernLabel lblAspectRatio;
        private ModernNumericUpDown numX;
        private ModernNumericUpDown numY;
        private ModernNumericUpDown numWidth;
        private ModernNumericUpDown numHeight;
        private ModernComboBox cmbAspectRatio;
        private TableLayoutPanel tableBottom;
        private ModernButton btnSave;
        private ModernButton btnSaveAs;
        private ModernButton btnCopy;
        private ModernButton btnReset;
        private ModernNumericUpDown numRatioFrom;
        private ModernNumericUpDown numRatioTo;
    }
}