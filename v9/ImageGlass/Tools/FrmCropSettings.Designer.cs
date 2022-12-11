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
            this.LblDefaultSelection = new ImageGlass.UI.ModernLabel();
            this.LblLocation = new ImageGlass.UI.ModernLabel();
            this.NumX = new ImageGlass.UI.ModernNumericUpDown();
            this.NumY = new ImageGlass.UI.ModernNumericUpDown();
            this.NumWidth = new ImageGlass.UI.ModernNumericUpDown();
            this.NumHeight = new ImageGlass.UI.ModernNumericUpDown();
            this.LblSize = new ImageGlass.UI.ModernLabel();
            this.ChkAutocenterSelection = new ImageGlass.UI.ModernCheckBox();
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
            this.tableTop.Controls.Add(this.LblDefaultSelectionType, 0, 1);
            this.tableTop.Controls.Add(this.CmbSelectionType, 0, 2);
            this.tableTop.Controls.Add(this.LblDefaultSelection, 0, 3);
            this.tableTop.Controls.Add(this.LblLocation, 0, 4);
            this.tableTop.Controls.Add(this.NumX, 1, 4);
            this.tableTop.Controls.Add(this.NumY, 2, 4);
            this.tableTop.Controls.Add(this.NumWidth, 1, 6);
            this.tableTop.Controls.Add(this.NumHeight, 2, 6);
            this.tableTop.Controls.Add(this.LblSize, 0, 6);
            this.tableTop.Controls.Add(this.ChkAutocenterSelection, 1, 5);
            this.tableTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableTop.Location = new System.Drawing.Point(0, 0);
            this.tableTop.Margin = new System.Windows.Forms.Padding(0);
            this.tableTop.Name = "tableTop";
            this.tableTop.Padding = new System.Windows.Forms.Padding(20);
            this.tableTop.RowCount = 7;
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.Size = new System.Drawing.Size(608, 261);
            this.tableTop.TabIndex = 0;
            // 
            // ChkCloseToolAfterSaving
            // 
            this.ChkCloseToolAfterSaving.AutoSize = true;
            this.ChkCloseToolAfterSaving.BackColor = System.Drawing.Color.Transparent;
            this.tableTop.SetColumnSpan(this.ChkCloseToolAfterSaving, 3);
            this.ChkCloseToolAfterSaving.DarkMode = false;
            this.ChkCloseToolAfterSaving.Location = new System.Drawing.Point(23, 23);
            this.ChkCloseToolAfterSaving.Name = "ChkCloseToolAfterSaving";
            this.ChkCloseToolAfterSaving.Size = new System.Drawing.Size(253, 27);
            this.ChkCloseToolAfterSaving.TabIndex = 2;
            this.ChkCloseToolAfterSaving.Text = "[Close Crop tool after saving]";
            this.ChkCloseToolAfterSaving.UseVisualStyleBackColor = false;
            // 
            // LblDefaultSelectionType
            // 
            this.LblDefaultSelectionType.AutoSize = true;
            this.LblDefaultSelectionType.DarkMode = false;
            this.LblDefaultSelectionType.Location = new System.Drawing.Point(23, 53);
            this.LblDefaultSelectionType.Name = "LblDefaultSelectionType";
            this.LblDefaultSelectionType.Size = new System.Drawing.Size(189, 23);
            this.LblDefaultSelectionType.TabIndex = 1;
            this.LblDefaultSelectionType.Text = "[Default selection type:]";
            // 
            // CmbSelectionType
            // 
            this.tableTop.SetColumnSpan(this.CmbSelectionType, 3);
            this.CmbSelectionType.DarkMode = false;
            this.CmbSelectionType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbSelectionType.FormattingEnabled = true;
            this.CmbSelectionType.Location = new System.Drawing.Point(23, 79);
            this.CmbSelectionType.Name = "CmbSelectionType";
            this.CmbSelectionType.Size = new System.Drawing.Size(169, 31);
            this.CmbSelectionType.TabIndex = 3;
            // 
            // LblDefaultSelection
            // 
            this.LblDefaultSelection.AutoSize = true;
            this.LblDefaultSelection.DarkMode = false;
            this.LblDefaultSelection.Location = new System.Drawing.Point(23, 113);
            this.LblDefaultSelection.Name = "LblDefaultSelection";
            this.LblDefaultSelection.Size = new System.Drawing.Size(151, 23);
            this.LblDefaultSelection.TabIndex = 4;
            this.LblDefaultSelection.Text = "[Default selection:]";
            // 
            // LblLocation
            // 
            this.LblLocation.AutoSize = true;
            this.LblLocation.DarkMode = false;
            this.LblLocation.Location = new System.Drawing.Point(23, 136);
            this.LblLocation.Name = "LblLocation";
            this.LblLocation.Size = new System.Drawing.Size(89, 23);
            this.LblLocation.TabIndex = 5;
            this.LblLocation.Text = "[Location:]";
            // 
            // NumX
            // 
            this.NumX.DarkMode = false;
            this.NumX.Location = new System.Drawing.Point(218, 139);
            this.NumX.Name = "NumX";
            this.NumX.SelectAllTextOnFocus = true;
            this.NumX.Size = new System.Drawing.Size(168, 30);
            this.NumX.TabIndex = 6;
            // 
            // NumY
            // 
            this.NumY.DarkMode = false;
            this.NumY.Location = new System.Drawing.Point(392, 139);
            this.NumY.Name = "NumY";
            this.NumY.SelectAllTextOnFocus = true;
            this.NumY.Size = new System.Drawing.Size(168, 30);
            this.NumY.TabIndex = 7;
            // 
            // NumWidth
            // 
            this.NumWidth.DarkMode = false;
            this.NumWidth.Location = new System.Drawing.Point(218, 208);
            this.NumWidth.Name = "NumWidth";
            this.NumWidth.SelectAllTextOnFocus = true;
            this.NumWidth.Size = new System.Drawing.Size(168, 30);
            this.NumWidth.TabIndex = 9;
            // 
            // NumHeight
            // 
            this.NumHeight.DarkMode = false;
            this.NumHeight.Location = new System.Drawing.Point(392, 208);
            this.NumHeight.Name = "NumHeight";
            this.NumHeight.SelectAllTextOnFocus = true;
            this.NumHeight.Size = new System.Drawing.Size(168, 30);
            this.NumHeight.TabIndex = 10;
            // 
            // LblSize
            // 
            this.LblSize.AutoSize = true;
            this.LblSize.DarkMode = false;
            this.LblSize.Location = new System.Drawing.Point(23, 205);
            this.LblSize.Name = "LblSize";
            this.LblSize.Size = new System.Drawing.Size(54, 23);
            this.LblSize.TabIndex = 11;
            this.LblSize.Text = "[Size:]";
            // 
            // ChkAutocenterSelection
            // 
            this.ChkAutocenterSelection.AutoSize = true;
            this.ChkAutocenterSelection.BackColor = System.Drawing.Color.Transparent;
            this.tableTop.SetColumnSpan(this.ChkAutocenterSelection, 2);
            this.ChkAutocenterSelection.DarkMode = false;
            this.ChkAutocenterSelection.Location = new System.Drawing.Point(218, 175);
            this.ChkAutocenterSelection.Name = "ChkAutocenterSelection";
            this.ChkAutocenterSelection.Size = new System.Drawing.Size(236, 27);
            this.ChkAutocenterSelection.TabIndex = 12;
            this.ChkAutocenterSelection.Text = "[Auto-center the selection]";
            this.ChkAutocenterSelection.UseVisualStyleBackColor = false;
            // 
            // FrmCropSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 381);
            this.ControlBox = false;
            this.Controls.Add(this.tableTop);
            this.Name = "FrmCropSettings";
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
        private UI.ModernLabel LblDefaultSelection;
        private UI.ModernLabel LblLocation;
        private UI.ModernNumericUpDown NumX;
        private UI.ModernNumericUpDown NumY;
        private UI.ModernLabel LblSize;
        private UI.ModernNumericUpDown NumWidth;
        private UI.ModernNumericUpDown NumHeight;
        private UI.ModernCheckBox ChkAutocenterSelection;
    }
}