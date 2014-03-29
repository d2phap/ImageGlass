/*
 * imaeg - generic image utility in C#
 * Copyright (C) 2010  ed <tripflag@gmail.com>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License v2
 * (version 2) as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, refer to the following URL:
 * http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace ImageGlass.Core
{
    public class ImgMan
    {
        const int MAXQUE = 5;

        string root;
        List<Img> image; //luu danh sach img chua load
        List<Img> queue; //load 1 so img vao bo nho
        public ImgFilter filter;
		//public int loadNext = 0;//load truoc vao hanh doi
        public bool imgError = false;
		
		public Bitmap ErrorImage()
        {
            return ImageGlass.Core.Properties.Resources.Image_Error;            
        }

        public ImgMan()
        {
            image = new List<Img>();
            queue = new List<Img>();
        }
		
        public ImgMan(string root, string[] names)
        {
            //debug();
            this.root = root;
            image = new List<Img>();
            queue = new List<Img>();
            foreach (string name in names)
			{
                image.Add(new Img(name));
			}

            filter = new ImgFilter();
            Thread loada = new Thread(new ThreadStart(loader));
            loada.Priority = ThreadPriority.BelowNormal;
            loada.IsBackground = true;
            loada.Start();

            //Program.dbg("ImgMan instance created");
        }

        Label lb;
        void debug()
        {
            lb = new Label();
            lb.Visible = true;
            lb.Dock = DockStyle.Fill;
            Form fm = new Form();
            fm.Controls.Add(lb);
            fm.Size = new Size(320, 900);
            fm.TopMost = true;
            fm.Show();
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Tick += new EventHandler(t_Tick);
            t.Interval = 100;
            t.Start();
        }
        void t_Tick(object sender, EventArgs e)
        {
            StringBuilder a = new StringBuilder();
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < image.Count; i++)
            {
                if (image[i].finished)
                {
                    a.AppendLine(i + " - " + image[i].getName());
                }
            }
            for (int i = 0; i < queue.Count; i++)
            {
                b.AppendLine(i + " - " + queue[i].getName());
            }
            lb.Text = System.DateTime.Now.Ticks +
                "\n\n" + a.ToString() +
                "\n\n" + b.ToString();
        }

        /// <summary>
        /// Returns image i, applying all configured enhancements
        /// </summary>
        /// <param name="i">The image to return</param>
        /// <returns>Image i</returns>
        public Image get(int i)
        {

            // Start off with unloading excessive images
            for (int a = 0; a < i - 3; a++)
            {
                image[a].dispose();
            }
            for (int a = i + 3; a < image.Count; a++)
            {
                image[a].dispose();
            }

            // New filter settings?
            if (filter.hasChanged())
            { //ALTERNATIVE_CODE
                foreach (Img im in image)
                {
                    im.dispose();
                }
            }

            queue.Clear();
            queue.Add(image[i]);
            enqueue(i + 1);
            //enqueue(i + 2);
            enqueue(i - 1);
            //enqueue(i - 2);

            while (!image[i].finished)
                Thread.Sleep(1);
			
			if (image[i].failed)
            {
                imgError = true;
                return new Bitmap(1, 1);//ImageGlass.Core.Properties.Resources.Image_Error);
            }
            else
            {
                imgError = false;                
                return (Image)image[i].get();
            }
        }

        /// <summary>
        /// Enqueue image i at a lower priority (caching)
        /// </summary>
        /// <param name="i"></param>
        void enqueue(int i)
        {
            if (i < 0 || i >= image.Count) return;
            if (!image[i].finished)
            {
                //foreach (Img j in queue)
                //if (image[i] == j) return;
                queue.Add(image[i]);
                //queue.Insert(1, image[i]);
            }
        }

        /// <summary>
        /// Worker thread; loads images.
        /// </summary>
        void loader()
        {
            while (true)
            {
                if (queue.Count > 0)
                {
                    Img i = queue[0];
                    queue.RemoveAt(0);
                    //Program.dbg("Loader requested on " + i.getName());
                    if (!i.finished)
                    {
                        //i.load(root);
                        //Program.dbg("Loader executing on " + i.getName());
                        i.load(root, filter); //ALTERNATIVE_CODE
                        //i.set(filter.apply(i.get())); //ALTERNATIVE_CODE
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        public int length
        {
            get { return image.Count; }
            set { }
        }
        public string getName(int i)
        {
            return image[i].getName();
        }
        public string getPath(int i)
        {
            if (i < 0 || i > image.Count)
                return "";

            return root + image[i].getName();
        }
        public void setName(int i, string s)
        {
            image[i].setName(s);
        }
        public void unload(int i)
        {
            if (image[i] != null)
                image[i].dispose();
        }
        public void remove(int i)
        {
            unload(i);
            image.RemoveAt(i);
        }
		public void Dispose()
        {
            for (int i = 0; i < length; i++)
            {
                remove(i);
            }
            image.Clear();
            queue.Clear();
        }
    }
}
