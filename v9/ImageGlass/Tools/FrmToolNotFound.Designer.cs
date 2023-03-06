namespace ImageGlass
{
    partial class FrmToolNotFound
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
            LblDownloadToolText = new UI.ModernLabel();
            LblDescription = new UI.ModernLabel();
            LblHeading = new UI.ModernLabel();
            LnkGetTools = new LinkLabel();
            TableTop.SuspendLayout();
            SuspendLayout();
            // 
            // TableTop
            // 
            TableTop.AutoSize = true;
            TableTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TableTop.ColumnCount = 1;
            TableTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TableTop.Controls.Add(LblDownloadToolText, 0, 2);
            TableTop.Controls.Add(LblDescription, 1, 1);
            TableTop.Controls.Add(LblHeading, 1, 0);
            TableTop.Controls.Add(LnkGetTools, 0, 3);
            TableTop.Dock = DockStyle.Top;
            TableTop.Location = new Point(0, 0);
            TableTop.Margin = new Padding(0);
            TableTop.Name = "TableTop";
            TableTop.Padding = new Padding(35, 40, 40, 40);
            TableTop.RowCount = 4;
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.RowStyles.Add(new RowStyle());
            TableTop.Size = new Size(1139, 444);
            TableTop.TabIndex = 3;
            // 
            // LblDownloadToolText
            // 
            LblDownloadToolText.AutoSize = true;
            LblDownloadToolText.BackColor = Color.Transparent;
            LblDownloadToolText.DarkMode = false;
            LblDownloadToolText.Location = new Point(40, 264);
            LblDownloadToolText.Margin = new Padding(5, 0, 0, 10);
            LblDownloadToolText.Name = "LblDownloadToolText";
            LblDownloadToolText.Size = new Size(728, 45);
            LblDownloadToolText.TabIndex = 3;
            LblDownloadToolText.Text = "[You can download more tools for ImageGlass at:]";
            // 
            // LblDescription
            // 
            LblDescription.AutoSize = true;
            LblDescription.BackColor = Color.Transparent;
            LblDescription.DarkMode = false;
            LblDescription.Location = new Point(40, 134);
            LblDescription.Margin = new Padding(5, 0, 0, 40);
            LblDescription.Name = "LblDescription";
            LblDescription.Size = new Size(1029, 90);
            LblDescription.TabIndex = 1;
            LblDescription.Text = "[ImageGlass could not find executable file of XXX tool. Do you want to locate it?]";
            // 
            // LblHeading
            // 
            LblHeading.AutoSize = true;
            LblHeading.BackColor = Color.Transparent;
            LblHeading.DarkMode = false;
            LblHeading.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            LblHeading.Location = new Point(40, 40);
            LblHeading.Margin = new Padding(5, 0, 0, 40);
            LblHeading.Name = "LblHeading";
            LblHeading.Size = new Size(426, 54);
            LblHeading.TabIndex = 0;
            LblHeading.Text = "[XXX tool is not found]";
            // 
            // LnkGetTools
            // 
            LnkGetTools.AutoSize = true;
            LnkGetTools.BackColor = Color.Transparent;
            LnkGetTools.Location = new Point(35, 319);
            LnkGetTools.Margin = new Padding(0, 0, 0, 40);
            LnkGetTools.Name = "LnkGetTools";
            LnkGetTools.Size = new Size(422, 45);
            LnkGetTools.TabIndex = 2;
            LnkGetTools.TabStop = true;
            LnkGetTools.Text = "https://imageglass.org/tools";
            LnkGetTools.LinkClicked += LnkGetTools_LinkClicked;
            // 
            // FrmToolNotFound
            // 
            AutoScaleDimensions = new SizeF(18F, 45F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1139, 739);
            Controls.Add(TableTop);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FrmToolNotFound";
            Text = "[Tool not found]";
            Controls.SetChildIndex(TableTop, 0);
            TableTop.ResumeLayout(false);
            TableTop.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel TableTop;
        private UI.ModernLabel LblDescription;
        private UI.ModernLabel LblHeading;
        private LinkLabel LnkGetTools;
        private UI.ModernLabel LblDownloadToolText;
    }
}