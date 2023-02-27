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
            TableTop = new TableLayoutPanel();
            ChkCloseToolAfterSaving = new UI.ModernCheckBox();
            CmbSelectionType = new UI.ModernComboBox();
            LblLocation = new UI.ModernLabel();
            NumX = new UI.ModernNumericUpDown();
            NumY = new UI.ModernNumericUpDown();
            NumWidth = new UI.ModernNumericUpDown();
            NumHeight = new UI.ModernNumericUpDown();
            LblSize = new UI.ModernLabel();
            ChkAutoCenterSelection = new UI.ModernCheckBox();
            LblDefaultSelection = new UI.ModernLabel();
            TableTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NumX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumHeight).BeginInit();
            SuspendLayout();
            // 
            // TableTop
            // 
            TableTop.AutoSize = true;
            TableTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TableTop.ColumnCount = 3;
            TableTop.ColumnStyles.Add(new ColumnStyle());
            TableTop.ColumnStyles.Add(new ColumnStyle());
            TableTop.ColumnStyles.Add(new ColumnStyle());
            TableTop.Controls.Add(ChkCloseToolAfterSaving, 0, 0);
            TableTop.Controls.Add(CmbSelectionType, 0, 2);
            TableTop.Controls.Add(LblLocation, 0, 4);
            TableTop.Controls.Add(NumX, 1, 4);
            TableTop.Controls.Add(NumY, 2, 4);
            TableTop.Controls.Add(NumWidth, 1, 5);
            TableTop.Controls.Add(NumHeight, 2, 5);
            TableTop.Controls.Add(LblSize, 0, 5);
            TableTop.Controls.Add(ChkAutoCenterSelection, 0, 3);
            TableTop.Controls.Add(LblDefaultSelection, 0, 1);
            TableTop.Dock = DockStyle.Top;
            TableTop.Location = new Point(0, 0);
            TableTop.Margin = new Padding(0);
            TableTop.Name = "TableTop";
            TableTop.Padding = new Padding(40, 40, 40, 80);
            TableTop.RowCount = 7;
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TableTop.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TableTop.Size = new Size(750, 554);
            TableTop.TabIndex = 0;
            // 
            // ChkCloseToolAfterSaving
            // 
            ChkCloseToolAfterSaving.AutoSize = true;
            ChkCloseToolAfterSaving.BackColor = Color.Transparent;
            TableTop.SetColumnSpan(ChkCloseToolAfterSaving, 3);
            ChkCloseToolAfterSaving.DarkMode = false;
            ChkCloseToolAfterSaving.Location = new Point(40, 40);
            ChkCloseToolAfterSaving.Margin = new Padding(0);
            ChkCloseToolAfterSaving.Name = "ChkCloseToolAfterSaving";
            ChkCloseToolAfterSaving.Size = new Size(473, 49);
            ChkCloseToolAfterSaving.TabIndex = 2;
            ChkCloseToolAfterSaving.Text = "[Close Crop tool after saving]";
            ChkCloseToolAfterSaving.UseVisualStyleBackColor = false;
            // 
            // CmbSelectionType
            // 
            TableTop.SetColumnSpan(CmbSelectionType, 3);
            CmbSelectionType.DarkMode = false;
            CmbSelectionType.DrawMode = DrawMode.OwnerDrawVariable;
            CmbSelectionType.FormattingEnabled = true;
            CmbSelectionType.Location = new Point(40, 204);
            CmbSelectionType.Margin = new Padding(0, 10, 0, 0);
            CmbSelectionType.Name = "CmbSelectionType";
            CmbSelectionType.Size = new Size(578, 51);
            CmbSelectionType.TabIndex = 3;
            CmbSelectionType.SelectedIndexChanged += CmbSelectionType_SelectedIndexChanged;
            // 
            // LblLocation
            // 
            LblLocation.AutoSize = true;
            LblLocation.BackColor = Color.Transparent;
            LblLocation.DarkMode = false;
            LblLocation.Location = new Point(70, 364);
            LblLocation.Margin = new Padding(30, 30, 0, 0);
            LblLocation.Name = "LblLocation";
            LblLocation.Size = new Size(168, 45);
            LblLocation.TabIndex = 5;
            LblLocation.Text = "[Location:]";
            // 
            // NumX
            // 
            NumX.DarkMode = false;
            NumX.Location = new Point(238, 364);
            NumX.Margin = new Padding(0, 30, 10, 0);
            NumX.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            NumX.Name = "NumX";
            NumX.SelectAllTextOnFocus = true;
            NumX.Size = new Size(180, 50);
            NumX.TabIndex = 6;
            NumX.ThousandsSeparator = true;
            // 
            // NumY
            // 
            NumY.DarkMode = false;
            NumY.Location = new Point(438, 364);
            NumY.Margin = new Padding(10, 30, 0, 0);
            NumY.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            NumY.Name = "NumY";
            NumY.SelectAllTextOnFocus = true;
            NumY.Size = new Size(180, 50);
            NumY.TabIndex = 7;
            NumY.ThousandsSeparator = true;
            // 
            // NumWidth
            // 
            NumWidth.DarkMode = false;
            NumWidth.Location = new Point(238, 424);
            NumWidth.Margin = new Padding(0, 10, 10, 0);
            NumWidth.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            NumWidth.Name = "NumWidth";
            NumWidth.SelectAllTextOnFocus = true;
            NumWidth.Size = new Size(180, 50);
            NumWidth.TabIndex = 9;
            NumWidth.ThousandsSeparator = true;
            // 
            // NumHeight
            // 
            NumHeight.DarkMode = false;
            NumHeight.Location = new Point(438, 424);
            NumHeight.Margin = new Padding(10, 10, 0, 0);
            NumHeight.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            NumHeight.Name = "NumHeight";
            NumHeight.SelectAllTextOnFocus = true;
            NumHeight.Size = new Size(180, 50);
            NumHeight.TabIndex = 10;
            NumHeight.ThousandsSeparator = true;
            // 
            // LblSize
            // 
            LblSize.AutoSize = true;
            LblSize.BackColor = Color.Transparent;
            LblSize.DarkMode = false;
            LblSize.Location = new Point(70, 424);
            LblSize.Margin = new Padding(30, 10, 0, 0);
            LblSize.Name = "LblSize";
            LblSize.Size = new Size(103, 45);
            LblSize.TabIndex = 11;
            LblSize.Text = "[Size:]";
            // 
            // ChkAutoCenterSelection
            // 
            ChkAutoCenterSelection.AutoSize = true;
            ChkAutoCenterSelection.BackColor = Color.Transparent;
            TableTop.SetColumnSpan(ChkAutoCenterSelection, 3);
            ChkAutoCenterSelection.DarkMode = false;
            ChkAutoCenterSelection.Location = new Point(70, 285);
            ChkAutoCenterSelection.Margin = new Padding(30, 30, 0, 0);
            ChkAutoCenterSelection.Name = "ChkAutoCenterSelection";
            ChkAutoCenterSelection.Size = new Size(440, 49);
            ChkAutoCenterSelection.TabIndex = 12;
            ChkAutoCenterSelection.Text = "[Auto-center the selection]";
            ChkAutoCenterSelection.UseVisualStyleBackColor = false;
            ChkAutoCenterSelection.CheckedChanged += ChkAutoCenterSelection_CheckedChanged;
            // 
            // LblDefaultSelection
            // 
            LblDefaultSelection.AutoSize = true;
            LblDefaultSelection.BackColor = Color.Transparent;
            TableTop.SetColumnSpan(LblDefaultSelection, 3);
            LblDefaultSelection.DarkMode = false;
            LblDefaultSelection.Font = new Font("Segoe UI", 9.6F, FontStyle.Regular, GraphicsUnit.Point);
            LblDefaultSelection.Location = new Point(40, 149);
            LblDefaultSelection.Margin = new Padding(0, 60, 0, 0);
            LblDefaultSelection.Name = "LblDefaultSelection";
            LblDefaultSelection.Size = new Size(278, 45);
            LblDefaultSelection.TabIndex = 13;
            LblDefaultSelection.Text = "[Default selection]";
            // 
            // FrmCropSettings
            // 
            AutoScaleDimensions = new SizeF(18F, 45F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(750, 882);
            ControlBox = false;
            Controls.Add(TableTop);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(12);
            Name = "FrmCropSettings";
            Text = "[Crop settings]";
            Controls.SetChildIndex(TableTop, 0);
            TableTop.ResumeLayout(false);
            TableTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NumX).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumY).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumHeight).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel TableTop;
        private UI.ModernCheckBox ChkCloseToolAfterSaving;
        private UI.ModernComboBox CmbSelectionType;
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