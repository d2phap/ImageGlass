namespace ImageGlass
{
    partial class FrmColorPickerSettings
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
            ChkShowCIELabA = new UI.ModernCheckBox();
            ChkShowHsvA = new UI.ModernCheckBox();
            ChkShowRgbA = new UI.ModernCheckBox();
            ChkShowHexA = new UI.ModernCheckBox();
            ChkShowHslA = new UI.ModernCheckBox();
            TableTop.SuspendLayout();
            SuspendLayout();
            // 
            // TableTop
            // 
            TableTop.AutoSize = true;
            TableTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TableTop.ColumnCount = 1;
            TableTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TableTop.Controls.Add(ChkShowCIELabA, 0, 4);
            TableTop.Controls.Add(ChkShowHsvA, 0, 3);
            TableTop.Controls.Add(ChkShowRgbA, 0, 0);
            TableTop.Controls.Add(ChkShowHexA, 0, 1);
            TableTop.Controls.Add(ChkShowHslA, 0, 2);
            TableTop.Dock = DockStyle.Top;
            TableTop.Location = new Point(0, 0);
            TableTop.Margin = new Padding(0);
            TableTop.Name = "TableTop";
            TableTop.Padding = new Padding(16, 15, 16, 30);
            TableTop.RowCount = 5;
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.Size = new Size(353, 182);
            TableTop.TabIndex = 3;
            // 
            // ChkShowCIELabA
            // 
            ChkShowCIELabA.AutoSize = true;
            ChkShowCIELabA.BackColor = Color.Transparent;
            ChkShowCIELabA.DarkMode = false;
            ChkShowCIELabA.Dock = DockStyle.Top;
            ChkShowCIELabA.Location = new Point(16, 131);
            ChkShowCIELabA.Margin = new Padding(0);
            ChkShowCIELabA.Name = "ChkShowCIELabA";
            ChkShowCIELabA.Size = new Size(321, 21);
            ChkShowCIELabA.TabIndex = 5;
            ChkShowCIELabA.Text = "[Use CIELAB format with alpha value]";
            ChkShowCIELabA.UseVisualStyleBackColor = false;
            // 
            // ChkShowHsvA
            // 
            ChkShowHsvA.AutoSize = true;
            ChkShowHsvA.BackColor = Color.Transparent;
            ChkShowHsvA.DarkMode = false;
            ChkShowHsvA.Dock = DockStyle.Top;
            ChkShowHsvA.Location = new Point(16, 102);
            ChkShowHsvA.Margin = new Padding(0, 0, 0, 8);
            ChkShowHsvA.Name = "ChkShowHsvA";
            ChkShowHsvA.Size = new Size(321, 21);
            ChkShowHsvA.TabIndex = 4;
            ChkShowHsvA.Text = "[Use HSV format with alpha value]";
            ChkShowHsvA.UseVisualStyleBackColor = false;
            // 
            // ChkShowRgbA
            // 
            ChkShowRgbA.AutoSize = true;
            ChkShowRgbA.BackColor = Color.Transparent;
            ChkShowRgbA.DarkMode = false;
            ChkShowRgbA.Dock = DockStyle.Top;
            ChkShowRgbA.Location = new Point(16, 15);
            ChkShowRgbA.Margin = new Padding(0, 0, 0, 8);
            ChkShowRgbA.Name = "ChkShowRgbA";
            ChkShowRgbA.Size = new Size(321, 21);
            ChkShowRgbA.TabIndex = 0;
            ChkShowRgbA.Text = "[Use RGB format with alpha value]";
            ChkShowRgbA.UseVisualStyleBackColor = false;
            // 
            // ChkShowHexA
            // 
            ChkShowHexA.AutoSize = true;
            ChkShowHexA.BackColor = Color.Transparent;
            ChkShowHexA.DarkMode = false;
            ChkShowHexA.Dock = DockStyle.Top;
            ChkShowHexA.Location = new Point(16, 44);
            ChkShowHexA.Margin = new Padding(0, 0, 0, 8);
            ChkShowHexA.Name = "ChkShowHexA";
            ChkShowHexA.Size = new Size(321, 21);
            ChkShowHexA.TabIndex = 1;
            ChkShowHexA.Text = "[Use HEX format with alpha value]";
            ChkShowHexA.UseVisualStyleBackColor = false;
            // 
            // ChkShowHslA
            // 
            ChkShowHslA.AutoSize = true;
            ChkShowHslA.BackColor = Color.Transparent;
            ChkShowHslA.DarkMode = false;
            ChkShowHslA.Dock = DockStyle.Top;
            ChkShowHslA.Location = new Point(16, 73);
            ChkShowHslA.Margin = new Padding(0, 0, 0, 8);
            ChkShowHslA.Name = "ChkShowHslA";
            ChkShowHslA.Size = new Size(321, 21);
            ChkShowHslA.TabIndex = 2;
            ChkShowHslA.Text = "[Use HSL format with alpha value]";
            ChkShowHslA.UseVisualStyleBackColor = false;
            // 
            // FrmColorPickerSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(353, 267);
            ControlBox = false;
            Controls.Add(TableTop);
            Margin = new Padding(1);
            Name = "FrmColorPickerSettings";
            Text = "[Color picker settings]";
            Controls.SetChildIndex(TableTop, 0);
            TableTop.ResumeLayout(false);
            TableTop.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel TableTop;
        private UI.ModernCheckBox ChkShowRgbA;
        private UI.ModernCheckBox ChkShowHexA;
        private UI.ModernCheckBox ChkShowHslA;
        private UI.ModernCheckBox ChkShowHsvA;
        private UI.ModernCheckBox ChkShowCIELabA;
    }
}