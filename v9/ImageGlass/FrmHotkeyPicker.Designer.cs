namespace ImageGlass
{
    partial class FrmHotkeyPicker
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
            TableLayout = new TableLayoutPanel();
            LblHotkey = new UI.ModernLabel();
            TxtHotkey = new UI.ModernTextBox();
            TableLayout.SuspendLayout();
            SuspendLayout();
            // 
            // TableLayout
            // 
            TableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TableLayout.BackColor = Color.Transparent;
            TableLayout.ColumnCount = 1;
            TableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TableLayout.Controls.Add(LblHotkey, 0, 0);
            TableLayout.Controls.Add(TxtHotkey, 0, 1);
            TableLayout.Dock = DockStyle.Top;
            TableLayout.Location = new Point(0, 0);
            TableLayout.Name = "TableLayout";
            TableLayout.Padding = new Padding(40, 40, 40, 80);
            TableLayout.RowCount = 2;
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.RowStyles.Add(new RowStyle());
            TableLayout.Size = new Size(768, 281);
            TableLayout.TabIndex = 3;
            // 
            // LblHotkey
            // 
            LblHotkey.AutoSize = true;
            LblHotkey.BackColor = Color.Transparent;
            LblHotkey.DarkMode = false;
            LblHotkey.Location = new Point(43, 40);
            LblHotkey.Name = "LblHotkey";
            LblHotkey.Padding = new Padding(0, 0, 0, 20);
            LblHotkey.Size = new Size(555, 65);
            LblHotkey.TabIndex = 0;
            LblHotkey.Text = "[Press any keys to create your hotkey]";
            // 
            // TxtHotkey
            // 
            TxtHotkey.BackColor = SystemColors.Window;
            TxtHotkey.DarkMode = false;
            TxtHotkey.Dock = DockStyle.Top;
            TxtHotkey.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
            TxtHotkey.ForeColor = SystemColors.WindowText;
            TxtHotkey.Location = new Point(43, 108);
            TxtHotkey.Name = "TxtHotkey";
            TxtHotkey.ReadOnly = true;
            TxtHotkey.Size = new Size(682, 70);
            TxtHotkey.TabIndex = 1;
            TxtHotkey.TextAlign = HorizontalAlignment.Center;
            TxtHotkey.KeyDown += TxtHotkey_KeyDown;
            // 
            // FrmHotkeyPicker
            // 
            AutoScaleDimensions = new SizeF(18F, 45F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(768, 576);
            ControlBox = false;
            Controls.Add(TableLayout);
            Name = "FrmHotkeyPicker";
            Text = " ";
            Controls.SetChildIndex(TableLayout, 0);
            TableLayout.ResumeLayout(false);
            TableLayout.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel TableLayout;
        private UI.ModernLabel LblHotkey;
        private UI.ModernTextBox TxtHotkey;
    }
}