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
            this.lblX = new ImageGlass.UI.ModernLabel();
            this.lblY = new ImageGlass.UI.ModernLabel();
            this.lblWidth = new ImageGlass.UI.ModernLabel();
            this.lblHeight = new ImageGlass.UI.ModernLabel();
            this.lblAspectRatio = new ImageGlass.UI.ModernLabel();
            this.label6 = new ImageGlass.UI.ModernLabel();
            this.numX = new ImageGlass.UI.ModernNumericUpDown();
            this.numY = new ImageGlass.UI.ModernNumericUpDown();
            this.numWidth = new ImageGlass.UI.ModernNumericUpDown();
            this.numHeight = new ImageGlass.UI.ModernNumericUpDown();
            this.cmbAspectRatio = new ImageGlass.UI.ModernComboBox();
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
            this.tableBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableTop
            // 
            this.tableTop.AutoSize = true;
            this.tableTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableTop.ColumnCount = 2;
            this.tableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableTop.Controls.Add(this.lblX, 0, 0);
            this.tableTop.Controls.Add(this.lblY, 0, 1);
            this.tableTop.Controls.Add(this.lblWidth, 0, 2);
            this.tableTop.Controls.Add(this.lblHeight, 0, 3);
            this.tableTop.Controls.Add(this.lblAspectRatio, 0, 4);
            this.tableTop.Controls.Add(this.label6, 0, 5);
            this.tableTop.Controls.Add(this.numX, 1, 0);
            this.tableTop.Controls.Add(this.numY, 1, 1);
            this.tableTop.Controls.Add(this.numWidth, 1, 2);
            this.tableTop.Controls.Add(this.numHeight, 1, 3);
            this.tableTop.Controls.Add(this.cmbAspectRatio, 1, 4);
            this.tableTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableTop.Location = new System.Drawing.Point(0, 0);
            this.tableTop.Margin = new System.Windows.Forms.Padding(0);
            this.tableTop.Name = "tableTop";
            this.tableTop.RowCount = 6;
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableTop.Size = new System.Drawing.Size(318, 244);
            this.tableTop.TabIndex = 0;
            // 
            // lblX
            // 
            this.lblX.AutoSize = true;
            this.lblX.BackColor = System.Drawing.Color.Transparent;
            this.lblX.DarkMode = true;
            this.lblX.Location = new System.Drawing.Point(15, 20);
            this.lblX.Margin = new System.Windows.Forms.Padding(15, 20, 10, 5);
            this.lblX.Name = "lblX";
            this.lblX.Size = new System.Drawing.Size(24, 23);
            this.lblX.TabIndex = 0;
            this.lblX.Text = "X:";
            // 
            // lblY
            // 
            this.lblY.AutoSize = true;
            this.lblY.BackColor = System.Drawing.Color.Transparent;
            this.lblY.DarkMode = true;
            this.lblY.Location = new System.Drawing.Point(15, 60);
            this.lblY.Margin = new System.Windows.Forms.Padding(15, 5, 10, 5);
            this.lblY.Name = "lblY";
            this.lblY.Size = new System.Drawing.Size(23, 23);
            this.lblY.TabIndex = 1;
            this.lblY.Text = "Y:";
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.BackColor = System.Drawing.Color.Transparent;
            this.lblWidth.DarkMode = true;
            this.lblWidth.Location = new System.Drawing.Point(15, 95);
            this.lblWidth.Margin = new System.Windows.Forms.Padding(15, 5, 10, 5);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(60, 23);
            this.lblWidth.TabIndex = 2;
            this.lblWidth.Text = "Width:";
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.BackColor = System.Drawing.Color.Transparent;
            this.lblHeight.DarkMode = true;
            this.lblHeight.Location = new System.Drawing.Point(15, 130);
            this.lblHeight.Margin = new System.Windows.Forms.Padding(15, 5, 10, 5);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(65, 23);
            this.lblHeight.TabIndex = 3;
            this.lblHeight.Text = "Height:";
            // 
            // lblAspectRatio
            // 
            this.lblAspectRatio.AutoSize = true;
            this.lblAspectRatio.BackColor = System.Drawing.Color.Transparent;
            this.lblAspectRatio.DarkMode = true;
            this.lblAspectRatio.Location = new System.Drawing.Point(15, 172);
            this.lblAspectRatio.Margin = new System.Windows.Forms.Padding(15, 12, 10, 5);
            this.lblAspectRatio.Name = "lblAspectRatio";
            this.lblAspectRatio.Size = new System.Drawing.Size(105, 23);
            this.lblAspectRatio.TabIndex = 4;
            this.lblAspectRatio.Text = "Aspect ratio:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.DarkMode = true;
            this.label6.Location = new System.Drawing.Point(15, 216);
            this.label6.Margin = new System.Windows.Forms.Padding(15, 10, 10, 5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 23);
            this.label6.TabIndex = 5;
            this.label6.Text = "label6";
            // 
            // numX
            // 
            this.numX.DarkMode = true;
            this.numX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numX.Location = new System.Drawing.Point(135, 20);
            this.numX.Margin = new System.Windows.Forms.Padding(5, 20, 20, 5);
            this.numX.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numX.Name = "numX";
            this.numX.Size = new System.Drawing.Size(163, 30);
            this.numX.TabIndex = 6;
            this.numX.ThousandsSeparator = true;
            // 
            // numY
            // 
            this.numY.DarkMode = true;
            this.numY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numY.Location = new System.Drawing.Point(135, 55);
            this.numY.Margin = new System.Windows.Forms.Padding(5, 0, 20, 5);
            this.numY.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numY.Name = "numY";
            this.numY.Size = new System.Drawing.Size(163, 30);
            this.numY.TabIndex = 7;
            this.numY.ThousandsSeparator = true;
            // 
            // numWidth
            // 
            this.numWidth.DarkMode = true;
            this.numWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numWidth.Location = new System.Drawing.Point(135, 90);
            this.numWidth.Margin = new System.Windows.Forms.Padding(5, 0, 20, 5);
            this.numWidth.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numWidth.Name = "numWidth";
            this.numWidth.Size = new System.Drawing.Size(163, 30);
            this.numWidth.TabIndex = 8;
            this.numWidth.ThousandsSeparator = true;
            // 
            // numHeight
            // 
            this.numHeight.DarkMode = true;
            this.numHeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numHeight.Location = new System.Drawing.Point(135, 125);
            this.numHeight.Margin = new System.Windows.Forms.Padding(5, 0, 20, 5);
            this.numHeight.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numHeight.Name = "numHeight";
            this.numHeight.Size = new System.Drawing.Size(163, 30);
            this.numHeight.TabIndex = 9;
            this.numHeight.ThousandsSeparator = true;
            // 
            // cmbAspectRatio
            // 
            this.cmbAspectRatio.DarkMode = true;
            this.cmbAspectRatio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbAspectRatio.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmbAspectRatio.FormattingEnabled = true;
            this.cmbAspectRatio.Items.AddRange(new object[] {
            "Original",
            "Square",
            "1:2",
            "3:2",
            "4:3",
            "16:9"});
            this.cmbAspectRatio.Location = new System.Drawing.Point(135, 170);
            this.cmbAspectRatio.Margin = new System.Windows.Forms.Padding(5, 10, 20, 5);
            this.cmbAspectRatio.Name = "cmbAspectRatio";
            this.cmbAspectRatio.Size = new System.Drawing.Size(163, 31);
            this.cmbAspectRatio.TabIndex = 10;
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
            this.tableBottom.Location = new System.Drawing.Point(0, 266);
            this.tableBottom.Name = "tableBottom";
            this.tableBottom.RowCount = 2;
            this.tableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableBottom.Size = new System.Drawing.Size(318, 130);
            this.tableBottom.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.DarkMode = true;
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnSave.ImagePadding = 2;
            this.btnSave.Location = new System.Drawing.Point(20, 20);
            this.btnSave.Margin = new System.Windows.Forms.Padding(20, 20, 5, 5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Padding = new System.Windows.Forms.Padding(5);
            this.btnSave.Size = new System.Drawing.Size(134, 40);
            this.btnSave.SystemIcon = null;
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "[Save]";
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.DarkMode = true;
            this.btnSaveAs.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnSaveAs.ImagePadding = 2;
            this.btnSaveAs.Location = new System.Drawing.Point(164, 20);
            this.btnSaveAs.Margin = new System.Windows.Forms.Padding(5, 20, 20, 5);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Padding = new System.Windows.Forms.Padding(5);
            this.btnSaveAs.Size = new System.Drawing.Size(134, 40);
            this.btnSaveAs.SystemIcon = null;
            this.btnSaveAs.TabIndex = 1;
            this.btnSaveAs.Text = "[Save as...]";
            // 
            // btnCopy
            // 
            this.btnCopy.DarkMode = true;
            this.btnCopy.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCopy.ImagePadding = 2;
            this.btnCopy.Location = new System.Drawing.Point(20, 70);
            this.btnCopy.Margin = new System.Windows.Forms.Padding(20, 5, 5, 20);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Padding = new System.Windows.Forms.Padding(5);
            this.btnCopy.Size = new System.Drawing.Size(134, 40);
            this.btnCopy.SystemIcon = null;
            this.btnCopy.TabIndex = 2;
            this.btnCopy.Text = "[Copy]";
            // 
            // btnReset
            // 
            this.btnReset.DarkMode = true;
            this.btnReset.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnReset.ImagePadding = 2;
            this.btnReset.Location = new System.Drawing.Point(164, 70);
            this.btnReset.Margin = new System.Windows.Forms.Padding(5, 5, 20, 20);
            this.btnReset.Name = "btnReset";
            this.btnReset.Padding = new System.Windows.Forms.Padding(5);
            this.btnReset.Size = new System.Drawing.Size(134, 40);
            this.btnReset.SystemIcon = null;
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "[Reset]";
            // 
            // FrmCrop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(318, 396);
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
            this.tableBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TableLayoutPanel tableTop;
        private ModernLabel lblX;
        private ModernLabel lblY;
        private ModernLabel lblWidth;
        private ModernLabel lblHeight;
        private ModernLabel lblAspectRatio;
        private ModernLabel label6;
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
    }
}