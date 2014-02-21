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
            this.imageMixture1 = new Image_Mixer.ImageMixture();
            this.SuspendLayout();
            // 
            // imageMixture1
            // 
            this.imageMixture1.BackColor = System.Drawing.Color.Transparent;
            this.imageMixture1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageMixture1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.imageMixture1.Location = new System.Drawing.Point(0, 0);
            this.imageMixture1.Name = "imageMixture1";
            this.imageMixture1.Size = new System.Drawing.Size(819, 420);
            this.imageMixture1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(819, 420);
            this.Controls.Add(this.imageMixture1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private Image_Mixer.ImageMixture imageMixture1;
    }
}

