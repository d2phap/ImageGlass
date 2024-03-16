using ImageGlass.UI;

namespace ImageGlass.Settings
{
    partial class Popup
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
            tableMain = new TableLayoutPanel();
            lblHeading = new ModernLabel();
            lblNote = new ModernLabel();
            txtValue = new ModernTextBox();
            lblDescription = new ModernLabel();
            picThumbnail = new PictureBox();
            ChkOption = new ModernCheckBox();
            tableMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picThumbnail).BeginInit();
            SuspendLayout();
            // 
            // tableMain
            // 
            tableMain.AutoSize = true;
            tableMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableMain.BackColor = Color.Transparent;
            tableMain.ColumnCount = 2;
            tableMain.ColumnStyles.Add(new ColumnStyle());
            tableMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableMain.Controls.Add(lblHeading, 1, 1);
            tableMain.Controls.Add(lblNote, 0, 4);
            tableMain.Controls.Add(txtValue, 1, 3);
            tableMain.Controls.Add(lblDescription, 1, 2);
            tableMain.Controls.Add(picThumbnail, 0, 1);
            tableMain.Controls.Add(ChkOption, 1, 5);
            tableMain.Dock = DockStyle.Top;
            tableMain.Location = new Point(0, 0);
            tableMain.Margin = new Padding(0);
            tableMain.Name = "tableMain";
            tableMain.Padding = new Padding(0, 15, 0, 15);
            tableMain.RowCount = 7;
            tableMain.RowStyles.Add(new RowStyle());
            tableMain.RowStyles.Add(new RowStyle());
            tableMain.RowStyles.Add(new RowStyle());
            tableMain.RowStyles.Add(new RowStyle());
            tableMain.RowStyles.Add(new RowStyle());
            tableMain.RowStyles.Add(new RowStyle());
            tableMain.RowStyles.Add(new RowStyle());
            tableMain.Size = new Size(478, 230);
            tableMain.TabIndex = 1;
            // 
            // lblHeading
            // 
            lblHeading.AutoSize = true;
            lblHeading.BackColor = Color.Transparent;
            lblHeading.DarkMode = false;
            lblHeading.Font = new Font("Segoe UI", 12F);
            lblHeading.Location = new Point(106, 15);
            lblHeading.Margin = new Padding(12, 0, 16, 15);
            lblHeading.Name = "lblHeading";
            lblHeading.Size = new Size(78, 21);
            lblHeading.TabIndex = 100;
            lblHeading.Text = "[Heading]";
            // 
            // lblNote
            // 
            lblNote.AutoSize = true;
            lblNote.BackColor = Color.Transparent;
            tableMain.SetColumnSpan(lblNote, 2);
            lblNote.DarkMode = false;
            lblNote.Dock = DockStyle.Top;
            lblNote.Location = new Point(16, 138);
            lblNote.Margin = new Padding(16, 15, 16, 0);
            lblNote.Name = "lblNote";
            lblNote.Padding = new Padding(8);
            lblNote.Size = new Size(446, 33);
            lblNote.TabIndex = 5;
            // 
            // txtValue
            // 
            txtValue.BackColor = SystemColors.Window;
            txtValue.DarkMode = false;
            txtValue.Dock = DockStyle.Fill;
            txtValue.ForeColor = SystemColors.WindowText;
            txtValue.Location = new Point(110, 83);
            txtValue.Margin = new Padding(16, 0, 16, 15);
            txtValue.Name = "txtValue";
            txtValue.ScrollBars = ScrollBars.Vertical;
            txtValue.Size = new Size(352, 25);
            txtValue.TabIndex = 0;
            txtValue.TextChanged += TxtValue_TextChanged;
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.BackColor = Color.Transparent;
            lblDescription.DarkMode = false;
            lblDescription.Location = new Point(106, 51);
            lblDescription.Margin = new Padding(12, 0, 16, 15);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(61, 17);
            lblDescription.TabIndex = 101;
            lblDescription.Text = "[Content]";
            // 
            // picThumbnail
            // 
            picThumbnail.BackColor = Color.Transparent;
            picThumbnail.BackgroundImageLayout = ImageLayout.Center;
            picThumbnail.Location = new Point(16, 15);
            picThumbnail.Margin = new Padding(16, 0, 0, 15);
            picThumbnail.MaximumSize = new Size(78, 74);
            picThumbnail.Name = "picThumbnail";
            tableMain.SetRowSpan(picThumbnail, 4);
            picThumbnail.Size = new Size(78, 74);
            picThumbnail.TabIndex = 4;
            picThumbnail.TabStop = false;
            picThumbnail.Visible = false;
            // 
            // ChkOption
            // 
            ChkOption.AutoSize = true;
            ChkOption.BackColor = Color.Transparent;
            tableMain.SetColumnSpan(ChkOption, 2);
            ChkOption.DarkMode = true;
            ChkOption.Location = new Point(16, 179);
            ChkOption.Margin = new Padding(16, 8, 16, 15);
            ChkOption.Name = "ChkOption";
            ChkOption.Size = new Size(225, 21);
            ChkOption.TabIndex = 1;
            ChkOption.Text = "[Do not show this message again]";
            ChkOption.UseVisualStyleBackColor = false;
            // 
            // Popup
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(478, 325);
            ControlBox = false;
            Controls.Add(tableMain);
            DarkMode = false;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(5);
            Name = "Popup";
            Text = "[Title]";
            Controls.SetChildIndex(tableMain, 0);
            tableMain.ResumeLayout(false);
            tableMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picThumbnail).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableMain;
        private ModernLabel lblDescription;
        private ModernTextBox txtValue;
        private PictureBox picThumbnail;
        private ModernLabel lblHeading;
        private ModernCheckBox ChkOption;
        private ModernLabel lblNote;
    }
}