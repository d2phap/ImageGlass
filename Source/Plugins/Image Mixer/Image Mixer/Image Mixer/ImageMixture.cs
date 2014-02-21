using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace Image_Mixer
{
    public partial class ImageMixture: UserControl
    {
        public ImageMixture()
        {
            InitializeComponent();
            pic.Image = new Bitmap(1, 1);
        }

        private void btnAddImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Supported formats (*.jpg;*.jpe;*.jfif;*.jpeg;*.png;*.gif;*.ico;*.bmp;*.dib;*.tif;*.tiff;*.exif;*.wmf;*.emf) | *.jpg;*.jpe;*.jfif;*.jpeg;*.png;*.gif;*.ico;*.bmp;*.dib;*.tif;*.tiff;*.exif;*.wmf;*.emf;";

            if(o.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = new FileStream(o.FileName, FileMode.Open, FileAccess.Read))
                {
                    Image imgs = pic.Image;
                    Image img = new Bitmap(fs);

                    int w = pic.Image.Width;
                    int h = pic.Image.Height;

                    if (img.Width > pic.Width)
                    {
                        w = img.Width;
                    }
                    if (img.Height > pic.Height)
                    {
                        h = img.Height;
                    }

                    Image newimg = new Bitmap(w, h);
                    Graphics gPic = Graphics.FromImage(newimg);
                    gPic.DrawImage(pic.Image, 0, 0);
                    pic.Image = newimg;

                    Graphics g = Graphics.FromImage(pic.Image);

                    // Place a image
                    g.DrawImage(img, new Point(0, 0));

                    
                    g.Dispose();
                    img.Dispose();
                }
            }
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Filter = "BMP|*.BMP|EMF|*.EMF|EXIF|*.EXIF|GIF|*.GIF|ICO|*.ICO|JPG|*.JPG|PNG|*.PNG|TIFF|*.TIFF|WMF|*.WMF";;

            if (s.ShowDialog() == DialogResult.OK)
            {
                switch (s.FilterIndex)
                {
                    case 1:
                        pic.Image.Save(s.FileName, ImageFormat.Bmp);
                        break;
                    case 2:
                        pic.Image.Save(s.FileName, ImageFormat.Emf);
                        break;
                    case 3:
                        pic.Image.Save(s.FileName, ImageFormat.Exif);
                        break;
                    case 4:
                        pic.Image.Save(s.FileName, ImageFormat.Gif);
                        break;
                    case 5:
                        pic.Image.Save(s.FileName, ImageFormat.Icon);
                        break;
                    case 6:
                        pic.Image.Save(s.FileName, ImageFormat.Jpeg);
                        break;
                    case 7:
                        pic.Image.Save(s.FileName, ImageFormat.Png);
                        break;
                    case 8:
                        pic.Image.Save(s.FileName, ImageFormat.Tiff);
                        break;
                    case 9:
                        pic.Image.Save(s.FileName, ImageFormat.Wmf);
                        break;
                }
            }

        }

        private void lnkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Config conf = new Config();
            MessageBox.Show(string.Format(@"{0}
{1}

Version: {2}
Author: {3}
", conf.Name.ToUpper(), conf.Description, conf.Version, conf.Author), "About",
 MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
