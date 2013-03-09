using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace Language
{
	/// <summary>
	/// Summary description for ctlMain.
	/// </summary>
	public class ctlMain : System.Windows.Forms.UserControl
    {
        private Label lblSlideshow;
        private TrackBar trbSlideShow;
        private Label label1;
        private CheckBox chkLocktoEdge;
        private CheckBox chkAutoUpdate;
        private ComboBox cmbZoom;
        private CheckBox chkRecursive;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ctlMain()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.lblSlideshow = new System.Windows.Forms.Label();
            this.trbSlideShow = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.chkLocktoEdge = new System.Windows.Forms.CheckBox();
            this.chkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.cmbZoom = new System.Windows.Forms.ComboBox();
            this.chkRecursive = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.trbSlideShow)).BeginInit();
            this.SuspendLayout();
            // 
            // lblSlideshow
            // 
            this.lblSlideshow.AutoSize = true;
            this.lblSlideshow.Location = new System.Drawing.Point(26, 150);
            this.lblSlideshow.Name = "lblSlideshow";
            this.lblSlideshow.Size = new System.Drawing.Size(117, 15);
            this.lblSlideshow.TabIndex = 5;
            this.lblSlideshow.Text = "Slide show interval: 5";
            // 
            // trbSlideShow
            // 
            this.trbSlideShow.Location = new System.Drawing.Point(149, 150);
            this.trbSlideShow.Maximum = 60;
            this.trbSlideShow.Minimum = 1;
            this.trbSlideShow.Name = "trbSlideShow";
            this.trbSlideShow.Size = new System.Drawing.Size(343, 45);
            this.trbSlideShow.TabIndex = 6;
            this.trbSlideShow.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trbSlideShow.Value = 5;
            this.trbSlideShow.Scroll += new System.EventHandler(this.trbSlideShow_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "Zoom optimization:";
            // 
            // chkLocktoEdge
            // 
            this.chkLocktoEdge.AutoSize = true;
            this.chkLocktoEdge.Checked = true;
            this.chkLocktoEdge.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLocktoEdge.Location = new System.Drawing.Point(27, 23);
            this.chkLocktoEdge.Name = "chkLocktoEdge";
            this.chkLocktoEdge.Size = new System.Drawing.Size(153, 19);
            this.chkLocktoEdge.TabIndex = 8;
            this.chkLocktoEdge.Text = "Lock to workspace edge";
            this.chkLocktoEdge.UseVisualStyleBackColor = true;
            this.chkLocktoEdge.CheckedChanged += new System.EventHandler(this.chkLocktoEdge_CheckedChanged);
            // 
            // chkAutoUpdate
            // 
            this.chkAutoUpdate.AutoSize = true;
            this.chkAutoUpdate.Checked = true;
            this.chkAutoUpdate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoUpdate.Location = new System.Drawing.Point(27, 48);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(192, 19);
            this.chkAutoUpdate.TabIndex = 9;
            this.chkAutoUpdate.Text = "Check for update automatically";
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            this.chkAutoUpdate.CheckedChanged += new System.EventHandler(this.chkAutoUpdate_CheckedChanged);
            // 
            // cmbZoom
            // 
            this.cmbZoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbZoom.FormattingEnabled = true;
            this.cmbZoom.Items.AddRange(new object[] {
            "Auto",
            "Smooth pixels",
            "Clear pixels"});
            this.cmbZoom.Location = new System.Drawing.Point(149, 106);
            this.cmbZoom.Name = "cmbZoom";
            this.cmbZoom.Size = new System.Drawing.Size(165, 23);
            this.cmbZoom.TabIndex = 11;
            this.cmbZoom.SelectedIndexChanged += new System.EventHandler(this.cmbZoom_SelectedIndexChanged);
            // 
            // chkRecursive
            // 
            this.chkRecursive.AutoSize = true;
            this.chkRecursive.Location = new System.Drawing.Point(27, 73);
            this.chkRecursive.Name = "chkRecursive";
            this.chkRecursive.Size = new System.Drawing.Size(161, 19);
            this.chkRecursive.TabIndex = 12;
            this.chkRecursive.Text = "Find image in child folder";
            this.chkRecursive.UseVisualStyleBackColor = true;
            this.chkRecursive.CheckedChanged += new System.EventHandler(this.chkRecursive_CheckedChanged);
            // 
            // ctlMain
            // 
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.chkRecursive);
            this.Controls.Add(this.cmbZoom);
            this.Controls.Add(this.chkAutoUpdate);
            this.Controls.Add(this.chkLocktoEdge);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trbSlideShow);
            this.Controls.Add(this.lblSlideshow);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ctlMain";
            this.Size = new System.Drawing.Size(517, 328);
            this.Load += new System.EventHandler(this.ctlMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trbSlideShow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion


        private void chkLocktoEdge_CheckedChanged(object sender, EventArgs e)
        {
            string hkey = "HKEY_CURRENT_USER\\Software\\PhapSoftware\\ImageGlass\\";
            Microsoft.Win32.Registry.SetValue(hkey, "LockToEdge", chkLocktoEdge.Checked.ToString());
        }

        private void chkAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            string hkey = "HKEY_CURRENT_USER\\Software\\PhapSoftware\\ImageGlass\\";            
            if (chkAutoUpdate.Checked)
            {
                Microsoft.Win32.Registry.SetValue(hkey, "AutoUpdate", chkAutoUpdate.Checked.ToString());
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(hkey, "AutoUpdate", "0");
            }
        }

        private void cmbZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            string hkey = "HKEY_CURRENT_USER\\Software\\PhapSoftware\\ImageGlass\\";
            Microsoft.Win32.Registry.SetValue(hkey, "ZoomOptimize", cmbZoom.Text);
        }

        private void trbSlideShow_Scroll(object sender, EventArgs e)
        {
            string hkey = "HKEY_CURRENT_USER\\Software\\PhapSoftware\\ImageGlass\\";
            Microsoft.Win32.Registry.SetValue(hkey, "Interval", trbSlideShow.Value);
            lblSlideshow.Text = "Slide show interval: " + trbSlideShow.Value.ToString();
        }

        private void chkRecursive_CheckedChanged(object sender, EventArgs e)
        {
            string hkey = "HKEY_CURRENT_USER\\Software\\PhapSoftware\\ImageGlass\\";
            Microsoft.Win32.Registry.SetValue(hkey, "Recursive", chkRecursive.Checked.ToString());
        }

        private void ctlMain_Load(object sender, EventArgs e)
        {
            string hkey = "HKEY_CURRENT_USER\\Software\\PhapSoftware\\ImageGlass\\";

            chkLocktoEdge.Checked = bool.Parse(Microsoft.Win32.Registry.GetValue(hkey, 
                "LockToEdge", "true").ToString());            
            chkRecursive.Checked = bool.Parse(Microsoft.Win32.Registry.GetValue(hkey,
                "Recursive", "false").ToString());
            string s = Microsoft.Win32.Registry.GetValue(hkey, "AutoUpdate", "true").ToString();
            if (s != "0")
            {
                chkAutoUpdate.Checked = true;
            }
            else
            {
                chkAutoUpdate.Checked = false;
            }
            
            s = Microsoft.Win32.Registry.GetValue(hkey, "ZoomOptimize", "Auto").ToString();
            int i = cmbZoom.Items.IndexOf(s);
            if (-1 < i && i < 3)
            {
                cmbZoom.SelectedIndex = i;
            }
            else
            {
                cmbZoom.SelectedIndex = 0;
            }

            i = int.Parse(Microsoft.Win32.Registry.GetValue(hkey, "Interval", "5").ToString());
            if (0 < i && i < 61)
            {
                trbSlideShow.Value = i;
            }
            else
            {
                trbSlideShow.Value = 5;
            }
            lblSlideshow.Text = "Slide show interval: " + trbSlideShow.Value.ToString();
        }
	}
}
