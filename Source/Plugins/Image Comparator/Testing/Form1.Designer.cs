namespace Testing
{
    partial class Form1
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
            this.imageComparator1 = new Image_Comparator.ImageComparator();
            this.SuspendLayout();
            // 
            // imageComparator1
            // 
            this.imageComparator1.BackColor = System.Drawing.Color.White;
            this.imageComparator1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageComparator1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.imageComparator1.Location = new System.Drawing.Point(0, 0);
            this.imageComparator1.MinimumSize = new System.Drawing.Size(645, 415);
            this.imageComparator1.Name = "imageComparator1";
            this.imageComparator1.Size = new System.Drawing.Size(819, 420);
            this.imageComparator1.TabIndex = 0;
            this.imageComparator1.Load += new System.EventHandler(this.imageComparator1_Load);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(819, 420);
            this.Controls.Add(this.imageComparator1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private Image_Comparator.ImageComparator imageMixture1;
        private Image_Comparator.ImageComparator imageComparator1;
    }
}

