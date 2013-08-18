/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
Project homepage: http://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace ImageGlass.ThumbBar
{
    public partial class ImageViewer : UserControl
    {
        private Image m_Image;
        private string m_ImageLocation;

        private bool m_IsThumbnail;
        private bool m_IsActive;
        private Color m_activeBorderColor;
        private int m_indexImage;

        

        public ImageViewer()
        {
            m_IsThumbnail = false;
            m_IsActive = false;
            m_activeBorderColor = Color.FromArgb(255, 0, 0, 0);
            m_indexImage = 0;

            InitializeComponent();
        }

        public Image Image
        {
            set { m_Image = value; }
            get { return m_Image; }
        }

        public int IndexImage
        {
            get { return m_indexImage; }
            set { m_indexImage = value; }
        }

        public Color ActiveBorderColor
        {
            get { return m_activeBorderColor; }
            set
            {
                m_activeBorderColor = value;
                this.Invalidate();
            }
        }

        public string ImageLocation
        {
            set { m_ImageLocation = value; }
            get { return m_ImageLocation; }
        }

        public bool IsActive
        {
            set 
            { 
                m_IsActive = value;
                this.Invalidate();
            }
            get { return m_IsActive; }
        }

        public bool IsThumbnail
        {
            set { m_IsThumbnail = value; }
            get { return m_IsThumbnail; }
        }

        public void ImageSizeChanged(object sender, ThumbnailImageEventArgs e)
        {
            this.Width = e.Size;
            this.Height = e.Size;
            this.Invalidate();
        }

        public void LoadImage(string imageFilename, int width, int height)
        {
            Image tempImage = Image.FromFile(imageFilename);
            m_ImageLocation = imageFilename;

            int dw = tempImage.Width;
            int dh = tempImage.Height;
            int tw = width;
            int th = height;
            double zw = (tw / (double)dw);
            double zh = (th / (double)dh);
            double z = (zw <= zh) ? zw : zh;
            dw = (int)(dw * z);
            dh = (int)(dh * z);

            if (dw < 10) dw = 10;
            if (dh < 10) dh = 10;

            m_Image = new Bitmap(dw, dh);
            Graphics g = Graphics.FromImage(m_Image);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(tempImage, 0, 0, dw, dh);
            g.Dispose();

            tempImage.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (g == null) return;
            if (m_Image == null) return;

            int dw = m_Image.Width;
            int dh = m_Image.Height;
            int tw = this.Width - 8; // remove border, 4*4 
            int th = this.Height - 8; // remove border, 4*4 
            double zw = (tw / (double)dw);
            double zh = (th / (double)dh);
            double z = (zw <= zh) ? zw : zh;

            dw = (int)(dw * z);
            dh = (int)(dh * z);
            int dl = 4 + (tw - dw) / 2; // add border 2*2
            int dt = 4 + (th - dh) / 2; // add border 2*2

            g.DrawRectangle(new Pen(Color.Gray), dl, dt, dw, dh);

            if (m_IsThumbnail)
            for (int j = 0; j < 3; j++)
            {
                g.DrawLine(new Pen(Color.DarkGray),
                    new Point(dl + 3, dt + dh + 1 + j),
                    new Point(dl + dw + 3, dt + dh + 1 + j));
                g.DrawLine(new Pen(Color.DarkGray),
                    new Point(dl + dw + 1 + j, dt + 3),
                    new Point(dl + dw + 1 + j, dt + dh + 3));
            }

            g.DrawImage(m_Image, dl, dt, dw, dh);

            if (m_IsActive)
            {
                g.DrawRectangle(new Pen(Color.White, 1), dl, dt, dw, dh);
                g.DrawRectangle(new Pen(m_activeBorderColor, 2), dl - 2, dt - 2, dw + 4, dh + 4);
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            this.Invalidate();
        }
    }
}
