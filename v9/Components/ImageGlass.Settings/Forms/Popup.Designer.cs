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
            this.tableMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblHeading = new ImageGlass.UI.ModernLabel();
            this.lblNote = new ImageGlass.UI.ModernLabel();
            this.txtValue = new ImageGlass.UI.ModernTextBox();
            this.lblDescription = new ImageGlass.UI.ModernLabel();
            this.picThumbnail = new System.Windows.Forms.PictureBox();
            this.ChkOption = new ImageGlass.UI.ModernCheckBox();
            this.tableMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).BeginInit();
            this.SuspendLayout();
            // 
            // tableMain
            // 
            this.tableMain.AutoSize = true;
            this.tableMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableMain.BackColor = System.Drawing.Color.Transparent;
            this.tableMain.ColumnCount = 2;
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMain.Controls.Add(this.lblHeading, 1, 1);
            this.tableMain.Controls.Add(this.lblNote, 0, 4);
            this.tableMain.Controls.Add(this.txtValue, 1, 3);
            this.tableMain.Controls.Add(this.lblDescription, 1, 2);
            this.tableMain.Controls.Add(this.picThumbnail, 0, 1);
            this.tableMain.Controls.Add(this.ChkOption, 1, 5);
            this.tableMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableMain.Location = new System.Drawing.Point(0, 0);
            this.tableMain.Margin = new System.Windows.Forms.Padding(0);
            this.tableMain.Name = "tableMain";
            this.tableMain.Padding = new System.Windows.Forms.Padding(0, 40, 0, 40);
            this.tableMain.RowCount = 7;
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.Size = new System.Drawing.Size(1228, 579);
            this.tableMain.TabIndex = 1;
            // 
            // lblHeading
            // 
            this.lblHeading.AutoSize = true;
            this.lblHeading.BackColor = System.Drawing.Color.Transparent;
            this.lblHeading.DarkMode = false;
            this.lblHeading.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblHeading.Location = new System.Drawing.Point(270, 40);
            this.lblHeading.Margin = new System.Windows.Forms.Padding(30, 0, 40, 39);
            this.lblHeading.Name = "lblHeading";
            this.lblHeading.Size = new System.Drawing.Size(197, 54);
            this.lblHeading.TabIndex = 100;
            this.lblHeading.Text = "[Heading]";
            // 
            // lblNote
            // 
            this.lblNote.AutoSize = true;
            this.lblNote.BackColor = System.Drawing.Color.Transparent;
            this.tableMain.SetColumnSpan(this.lblNote, 2);
            this.lblNote.DarkMode = false;
            this.lblNote.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblNote.Location = new System.Drawing.Point(40, 346);
            this.lblNote.Margin = new System.Windows.Forms.Padding(40, 40, 40, 0);
            this.lblNote.Name = "lblNote";
            this.lblNote.Padding = new System.Windows.Forms.Padding(20);
            this.lblNote.Size = new System.Drawing.Size(1148, 85);
            this.lblNote.TabIndex = 5;
            this.lblNote.Text = "[###]";
            // 
            // txtValue
            // 
            this.txtValue.BackColor = System.Drawing.SystemColors.Window;
            this.txtValue.DarkMode = false;
            this.txtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtValue.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtValue.Location = new System.Drawing.Point(280, 217);
            this.txtValue.Margin = new System.Windows.Forms.Padding(40, 0, 40, 39);
            this.txtValue.Name = "txtValue";
            this.txtValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtValue.Size = new System.Drawing.Size(908, 50);
            this.txtValue.TabIndex = 0;
            this.txtValue.TextChanged += new System.EventHandler(this.TxtValue_TextChanged);
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.BackColor = System.Drawing.Color.Transparent;
            this.lblDescription.DarkMode = false;
            this.lblDescription.Location = new System.Drawing.Point(270, 133);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(30, 0, 40, 39);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(154, 45);
            this.lblDescription.TabIndex = 101;
            this.lblDescription.Text = "[Content]";
            // 
            // picThumbnail
            // 
            this.picThumbnail.BackColor = System.Drawing.Color.Transparent;
            this.picThumbnail.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.picThumbnail.Location = new System.Drawing.Point(40, 40);
            this.picThumbnail.Margin = new System.Windows.Forms.Padding(40, 0, 0, 39);
            this.picThumbnail.MaximumSize = new System.Drawing.Size(200, 196);
            this.picThumbnail.Name = "picThumbnail";
            this.tableMain.SetRowSpan(this.picThumbnail, 4);
            this.picThumbnail.Size = new System.Drawing.Size(200, 196);
            this.picThumbnail.TabIndex = 4;
            this.picThumbnail.TabStop = false;
            this.picThumbnail.Visible = false;
            // 
            // ChkOption
            // 
            this.ChkOption.AutoSize = true;
            this.ChkOption.BackColor = System.Drawing.Color.Transparent;
            this.tableMain.SetColumnSpan(this.ChkOption, 2);
            this.ChkOption.DarkMode = true;
            this.ChkOption.Location = new System.Drawing.Point(40, 451);
            this.ChkOption.Margin = new System.Windows.Forms.Padding(40, 20, 40, 39);
            this.ChkOption.Name = "ChkOption";
            this.ChkOption.Size = new System.Drawing.Size(539, 49);
            this.ChkOption.TabIndex = 1;
            this.ChkOption.Text = "[Do not show this message again]";
            this.ChkOption.UseVisualStyleBackColor = false;
            // 
            // Popup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(18F, 45F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1228, 860);
            this.ControlBox = false;
            this.Controls.Add(this.tableMain);
            this.DarkMode = false;
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(12);
            this.Name = "Popup";
            this.Text = "[Title]";
            this.Controls.SetChildIndex(this.tableMain, 0);
            this.tableMain.ResumeLayout(false);
            this.tableMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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