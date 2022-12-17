namespace ImageGlass
{
    partial class FrmCropSettings
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
            this.ChkCloseToolAfterSaving = new ImageGlass.UI.ModernCheckBox();
            this.LblDefaultSelectionType = new ImageGlass.UI.ModernLabel();
            this.CmbSelectionType = new ImageGlass.UI.ModernComboBox();
            this.LblDefaultSelectionArea = new ImageGlass.UI.ModernLabel();
            this.LblLocation = new ImageGlass.UI.ModernLabel();
            this.NumX = new ImageGlass.UI.ModernNumericUpDown();
            this.NumY = new ImageGlass.UI.ModernNumericUpDown();
            this.NumWidth = new ImageGlass.UI.ModernNumericUpDown();
            this.NumHeight = new ImageGlass.UI.ModernNumericUpDown();
            this.LblSize = new ImageGlass.UI.ModernLabel();
            this.ChkAutoCenterSelection = new ImageGlass.UI.ModernCheckBox();
            this.LblDefaultSelection = new ImageGlass.UI.ModernLabel();
            this.tableTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // tableTop
            // 
            this.tableTop.AutoSize = true;
            this.tableTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableTop.ColumnCount = 3;
            this.tableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableTop.Controls.Add(this.ChkCloseToolAfterSaving, 0, 0);
            this.tableTop.Controls.Add(this.LblDefaultSelectionType, 0, 2);
            this.tableTop.Controls.Add(this.CmbSelectionType, 0, 3);
            this.tableTop.Controls.Add(this.LblDefaultSelectionArea, 0, 4);
            this.tableTop.Controls.Add(this.LblLocation, 0, 5);
            this.tableTop.Controls.Add(this.NumX, 1, 5);
            this.tableTop.Controls.Add(this.NumY, 2, 5);
            this.tableTop.Controls.Add(this.NumWidth, 1, 7);
            this.tableTop.Controls.Add(this.NumHeight, 2, 7);
            this.tableTop.Controls.Add(this.LblSize, 0, 7);
            this.tableTop.Controls.Add(this.ChkAutoCenterSelection, 1, 6);
            this.tableTop.Controls.Add(this.LblDefaultSelection, 0, 1);
            this.tableTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableTop.Location = new System.Drawing.Point(0, 0);
            this.tableTop.Margin = new System.Windows.Forms.Padding(0);
            this.tableTop.Name = "tableTop";
            this.tableTop.Padding = new System.Windows.Forms.Padding(40, 39, 40, 39);
            this.tableTop.RowCount = 8;
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.Size = new System.Drawing.Size(967, 612);
            this.tableTop.TabIndex = 0;
            // 
            // ChkCloseToolAfterSaving
            // 
            this.ChkCloseToolAfterSaving.AutoSize = true;
            this.ChkCloseToolAfterSaving.BackColor = System.Drawing.Color.Transparent;
            this.tableTop.SetColumnSpan(this.ChkCloseToolAfterSaving, 3);
            this.ChkCloseToolAfterSaving.DarkMode = false;
            this.ChkCloseToolAfterSaving.Location = new System.Drawing.Point(40, 39);
            this.ChkCloseToolAfterSaving.Margin = new System.Windows.Forms.Padding(0);
            this.ChkCloseToolAfterSaving.Name = "ChkCloseToolAfterSaving";
            this.ChkCloseToolAfterSaving.Size = new System.Drawing.Size(473, 49);
            this.ChkCloseToolAfterSaving.TabIndex = 2;
            this.ChkCloseToolAfterSaving.Text = "[Close Crop tool after saving]";
            this.ChkCloseToolAfterSaving.UseVisualStyleBackColor = false;
            // 
            // LblDefaultSelectionType
            // 
            this.LblDefaultSelectionType.AutoSize = true;
            this.LblDefaultSelectionType.BackColor = System.Drawing.Color.Transparent;
            this.tableTop.SetColumnSpan(this.LblDefaultSelectionType, 3);
            this.LblDefaultSelectionType.DarkMode = false;
            this.LblDefaultSelectionType.Location = new System.Drawing.Point(70, 203);
            this.LblDefaultSelectionType.Margin = new System.Windows.Forms.Padding(30, 10, 0, 0);
            this.LblDefaultSelectionType.Name = "LblDefaultSelectionType";
            this.LblDefaultSelectionType.Size = new System.Drawing.Size(356, 45);
            this.LblDefaultSelectionType.TabIndex = 1;
            this.LblDefaultSelectionType.Text = "[Default selection type:]";
            // 
            // CmbSelectionType
            // 
            this.tableTop.SetColumnSpan(this.CmbSelectionType, 3);
            this.CmbSelectionType.DarkMode = false;
            this.CmbSelectionType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbSelectionType.FormattingEnabled = true;
            this.CmbSelectionType.Location = new System.Drawing.Point(75, 258);
            this.CmbSelectionType.Margin = new System.Windows.Forms.Padding(35, 10, 0, 0);
            this.CmbSelectionType.Name = "CmbSelectionType";
            this.CmbSelectionType.Size = new System.Drawing.Size(573, 51);
            this.CmbSelectionType.TabIndex = 3;
            this.CmbSelectionType.SelectedIndexChanged += new System.EventHandler(this.CmbSelectionType_SelectedIndexChanged);
            // 
            // LblDefaultSelectionArea
            // 
            this.LblDefaultSelectionArea.AutoSize = true;
            this.LblDefaultSelectionArea.BackColor = System.Drawing.Color.Transparent;
            this.tableTop.SetColumnSpan(this.LblDefaultSelectionArea, 3);
            this.LblDefaultSelectionArea.DarkMode = false;
            this.LblDefaultSelectionArea.Location = new System.Drawing.Point(70, 339);
            this.LblDefaultSelectionArea.Margin = new System.Windows.Forms.Padding(30, 30, 0, 0);
            this.LblDefaultSelectionArea.Name = "LblDefaultSelectionArea";
            this.LblDefaultSelectionArea.Size = new System.Drawing.Size(354, 45);
            this.LblDefaultSelectionArea.TabIndex = 4;
            this.LblDefaultSelectionArea.Text = "[Default selection area:]";
            // 
            // LblLocation
            // 
            this.LblLocation.AutoSize = true;
            this.LblLocation.BackColor = System.Drawing.Color.Transparent;
            this.LblLocation.DarkMode = false;
            this.LblLocation.Location = new System.Drawing.Point(100, 394);
            this.LblLocation.Margin = new System.Windows.Forms.Padding(60, 10, 0, 0);
            this.LblLocation.Name = "LblLocation";
            this.LblLocation.Size = new System.Drawing.Size(168, 45);
            this.LblLocation.TabIndex = 5;
            this.LblLocation.Text = "[Location:]";
            // 
            // NumX
            // 
            this.NumX.DarkMode = false;
            this.NumX.Location = new System.Drawing.Point(268, 394);
            this.NumX.Margin = new System.Windows.Forms.Padding(0, 10, 10, 0);
            this.NumX.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumX.Name = "NumX";
            this.NumX.SelectAllTextOnFocus = true;
            this.NumX.Size = new System.Drawing.Size(180, 50);
            this.NumX.TabIndex = 6;
            this.NumX.ThousandsSeparator = true;
            // 
            // NumY
            // 
            this.NumY.DarkMode = false;
            this.NumY.Location = new System.Drawing.Point(468, 394);
            this.NumY.Margin = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.NumY.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumY.Name = "NumY";
            this.NumY.SelectAllTextOnFocus = true;
            this.NumY.Size = new System.Drawing.Size(180, 50);
            this.NumY.TabIndex = 7;
            this.NumY.ThousandsSeparator = true;
            // 
            // NumWidth
            // 
            this.NumWidth.DarkMode = false;
            this.NumWidth.Location = new System.Drawing.Point(268, 523);
            this.NumWidth.Margin = new System.Windows.Forms.Padding(0, 20, 10, 0);
            this.NumWidth.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumWidth.Name = "NumWidth";
            this.NumWidth.SelectAllTextOnFocus = true;
            this.NumWidth.Size = new System.Drawing.Size(180, 50);
            this.NumWidth.TabIndex = 9;
            this.NumWidth.ThousandsSeparator = true;
            // 
            // NumHeight
            // 
            this.NumHeight.DarkMode = false;
            this.NumHeight.Location = new System.Drawing.Point(468, 523);
            this.NumHeight.Margin = new System.Windows.Forms.Padding(10, 20, 0, 0);
            this.NumHeight.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NumHeight.Name = "NumHeight";
            this.NumHeight.SelectAllTextOnFocus = true;
            this.NumHeight.Size = new System.Drawing.Size(180, 50);
            this.NumHeight.TabIndex = 10;
            this.NumHeight.ThousandsSeparator = true;
            // 
            // LblSize
            // 
            this.LblSize.AutoSize = true;
            this.LblSize.BackColor = System.Drawing.Color.Transparent;
            this.LblSize.DarkMode = false;
            this.LblSize.Location = new System.Drawing.Point(100, 523);
            this.LblSize.Margin = new System.Windows.Forms.Padding(60, 20, 0, 0);
            this.LblSize.Name = "LblSize";
            this.LblSize.Size = new System.Drawing.Size(103, 45);
            this.LblSize.TabIndex = 11;
            this.LblSize.Text = "[Size:]";
            // 
            // ChkAutocenterSelection
            // 
            this.ChkAutoCenterSelection.AutoSize = true;
            this.ChkAutoCenterSelection.BackColor = System.Drawing.Color.Transparent;
            this.tableTop.SetColumnSpan(this.ChkAutoCenterSelection, 2);
            this.ChkAutoCenterSelection.DarkMode = false;
            this.ChkAutoCenterSelection.Location = new System.Drawing.Point(268, 454);
            this.ChkAutoCenterSelection.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.ChkAutoCenterSelection.Name = "ChkAutocenterSelection";
            this.ChkAutoCenterSelection.Size = new System.Drawing.Size(440, 49);
            this.ChkAutoCenterSelection.TabIndex = 12;
            this.ChkAutoCenterSelection.Text = "[Auto-center the selection]";
            this.ChkAutoCenterSelection.UseVisualStyleBackColor = false;
            // 
            // LblDefaultSelection
            // 
            this.LblDefaultSelection.AutoSize = true;
            this.LblDefaultSelection.BackColor = System.Drawing.Color.Transparent;
            this.tableTop.SetColumnSpan(this.LblDefaultSelection, 3);
            this.LblDefaultSelection.DarkMode = false;
            this.LblDefaultSelection.Font = new System.Drawing.Font("Segoe UI", 9.6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LblDefaultSelection.Location = new System.Drawing.Point(40, 148);
            this.LblDefaultSelection.Margin = new System.Windows.Forms.Padding(0, 60, 0, 0);
            this.LblDefaultSelection.Name = "LblDefaultSelection";
            this.LblDefaultSelection.Size = new System.Drawing.Size(295, 45);
            this.LblDefaultSelection.TabIndex = 13;
            this.LblDefaultSelection.Text = "[Default selection]";
            // 
            // FrmCropSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(18F, 45F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(967, 855);
            this.ControlBox = false;
            this.Controls.Add(this.tableTop);
            this.Margin = new System.Windows.Forms.Padding(12);
            this.Name = "FrmCropSettings";
            this.ShowAcceptButton = true;
            this.ShowApplyButton = true;
            this.ShowCancelButton = true;
            this.Text = "[Crop settings]";
            this.Controls.SetChildIndex(this.tableTop, 0);
            this.tableTop.ResumeLayout(false);
            this.tableTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TableLayoutPanel tableTop;
        private UI.ModernLabel LblDefaultSelectionType;
        private UI.ModernCheckBox ChkCloseToolAfterSaving;
        private UI.ModernComboBox CmbSelectionType;
        private UI.ModernLabel LblDefaultSelectionArea;
        private UI.ModernLabel LblLocation;
        private UI.ModernNumericUpDown NumX;
        private UI.ModernNumericUpDown NumY;
        private UI.ModernLabel LblSize;
        private UI.ModernNumericUpDown NumWidth;
        private UI.ModernNumericUpDown NumHeight;
        private UI.ModernCheckBox ChkAutoCenterSelection;
        private UI.ModernLabel LblDefaultSelection;
    }
}