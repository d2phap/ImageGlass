namespace ImageGlass.UI.BuiltInForms
{
    partial class InputForm
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
            this.txtValue = new System.Windows.Forms.MaskedTextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblContent = new System.Windows.Forms.Label();
            this.picThumbnail = new System.Windows.Forms.PictureBox();
            this.panBottom = new System.Windows.Forms.Panel();
            this.tableBottom = new System.Windows.Forms.TableLayoutPanel();
            this.btnOK = new ImageGlass.UI.ModernButton();
            this.btnCancel = new ImageGlass.UI.ModernButton();
            this.tableMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).BeginInit();
            this.panBottom.SuspendLayout();
            this.tableBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableMain
            // 
            this.tableMain.BackColor = System.Drawing.Color.Transparent;
            this.tableMain.ColumnCount = 2;
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMain.Controls.Add(this.txtValue, 1, 2);
            this.tableMain.Controls.Add(this.lblTitle, 0, 0);
            this.tableMain.Controls.Add(this.lblContent, 1, 1);
            this.tableMain.Controls.Add(this.picThumbnail, 0, 1);
            this.tableMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableMain.Location = new System.Drawing.Point(1, 1);
            this.tableMain.Margin = new System.Windows.Forms.Padding(0);
            this.tableMain.Name = "tableMain";
            this.tableMain.RowCount = 3;
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.Size = new System.Drawing.Size(578, 898);
            this.tableMain.TabIndex = 0;
            // 
            // txtValue
            // 
            this.txtValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtValue.Location = new System.Drawing.Point(140, 113);
            this.txtValue.Margin = new System.Windows.Forms.Padding(20, 10, 20, 20);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(418, 30);
            this.txtValue.TabIndex = 1;
            this.txtValue.TextChanged += new System.EventHandler(this.TxtValue_TextChanged);
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.tableMain.SetColumnSpan(this.lblTitle, 2);
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 9.134328F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(15, 2, 0, 2);
            this.lblTitle.Size = new System.Drawing.Size(578, 40);
            this.lblTitle.TabIndex = 2;
            this.lblTitle.Text = "[Title]";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblContent
            // 
            this.lblContent.AutoSize = true;
            this.lblContent.Location = new System.Drawing.Point(135, 70);
            this.lblContent.Margin = new System.Windows.Forms.Padding(15, 30, 20, 10);
            this.lblContent.Name = "lblContent";
            this.lblContent.Size = new System.Drawing.Size(82, 23);
            this.lblContent.TabIndex = 3;
            this.lblContent.Text = "[Content]";
            // 
            // picThumbnail
            // 
            this.picThumbnail.Location = new System.Drawing.Point(20, 70);
            this.picThumbnail.Margin = new System.Windows.Forms.Padding(20, 30, 0, 10);
            this.picThumbnail.MaximumSize = new System.Drawing.Size(100, 100);
            this.picThumbnail.Name = "picThumbnail";
            this.tableMain.SetRowSpan(this.picThumbnail, 2);
            this.picThumbnail.Size = new System.Drawing.Size(100, 100);
            this.picThumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picThumbnail.TabIndex = 4;
            this.picThumbnail.TabStop = false;
            this.picThumbnail.Visible = false;
            // 
            // panBottom
            // 
            this.panBottom.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panBottom.Controls.Add(this.tableBottom);
            this.panBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panBottom.Location = new System.Drawing.Point(1, 188);
            this.panBottom.Name = "panBottom";
            this.panBottom.Padding = new System.Windows.Forms.Padding(20);
            this.panBottom.Size = new System.Drawing.Size(578, 81);
            this.panBottom.TabIndex = 1;
            // 
            // tableBottom
            // 
            this.tableBottom.BackColor = System.Drawing.Color.Transparent;
            this.tableBottom.ColumnCount = 3;
            this.tableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableBottom.Controls.Add(this.btnOK, 1, 0);
            this.tableBottom.Controls.Add(this.btnCancel, 2, 0);
            this.tableBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableBottom.Location = new System.Drawing.Point(20, 20);
            this.tableBottom.Name = "tableBottom";
            this.tableBottom.RowCount = 1;
            this.tableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableBottom.Size = new System.Drawing.Size(538, 41);
            this.tableBottom.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.AutoSize = true;
            this.btnOK.DarkMode = false;
            this.btnOK.Location = new System.Drawing.Point(277, 0);
            this.btnOK.Margin = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnOK.MinimumSize = new System.Drawing.Size(120, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnOK.Size = new System.Drawing.Size(120, 40);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "[OK]";
            this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.DarkMode = false;
            this.btnCancel.Location = new System.Drawing.Point(417, 0);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnCancel.MinimumSize = new System.Drawing.Size(120, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnCancel.Size = new System.Drawing.Size(121, 40);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "[Cancel]";
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // InputForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(580, 270);
            this.Controls.Add(this.panBottom);
            this.Controls.Add(this.tableMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(580, 240);
            this.Name = "InputForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "InputBox";
            this.Load += new System.EventHandler(this.InputForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputForm_KeyDown);
            this.tableMain.ResumeLayout(false);
            this.tableMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).EndInit();
            this.panBottom.ResumeLayout(false);
            this.tableBottom.ResumeLayout(false);
            this.tableBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableMain;
        private Panel panBottom;
        private TableLayoutPanel tableBottom;
        private ModernButton btnOK;
        private ModernButton btnCancel;
        private Label lblTitle;
        private Label lblContent;
        private MaskedTextBox txtValue;
        private PictureBox picThumbnail;
    }
}