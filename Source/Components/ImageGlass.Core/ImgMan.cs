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
        private List<Img> image; //luu danh sach img chua load
        private List<Img> queue; //load 1 so img vao bo nho
        private bool isErrorImage = false;

        public bool IsErrorImage
        {
            get { return isErrorImage; }
            set { isErrorImage = value; }
        }

        public ImgMan()
        {
            image = new List<Img>();
            queue = new List<Img>();
        }
		
        public ImgMan(string[] filenames)
        {
            image = new List<Img>();
            queue = new List<Img>();

            foreach (string name in filenames)
			{
                image.Add(new Img(name));
			}

            Thread tLoader = new Thread(new ThreadStart(Loader));
            tLoader.Priority = ThreadPriority.BelowNormal;
            tLoader.IsBackground = true;
            tLoader.Start();
        }


        /// <summary>
        /// Returns image i, applying all configured enhancements
        /// </summary>
        /// <param name="i">The image to return</param>
        /// <returns>Image i</returns>
        public Image GetImage(int i)
        {
            // Start off with unloading excessive images
            for (int a = 0; a < i - 2; a++)
            {
                image[a].Dispose();
            }
            for (int a = i + 2; a < image.Count; a++)
            {
                image[a].Dispose();
            }

            queue.Clear();
            queue.Add(image[i]);
            Enqueue(i + 1);
            Enqueue(i - 1);

            while (!image[i].IsFinished)
                Thread.Sleep(1);

			if (image[i].IsFailed)
            {
                isErrorImage = true;
                return new Bitmap(1, 1);
            }
            else
            {
                isErrorImage = false;                
                return (Image)image[i].Get();
            }
        }

        /// <summary>
        /// Enqueue image i at a lower priority (caching)
        /// </summary>
        /// <param name="i"></param>
        private void Enqueue(int i)
        {
            if (i < 0 || i >= image.Count) return;
            if (!image[i].IsFinished)
            {
                queue.Add(image[i]);
            }
        }

        /// <summary>
        /// Worker thread; loads images.
        /// </summary>
        private void Loader()
        {
            while (true)
            {
                if (queue.Count > 0)
                {
                    Img i = queue[0];
                    queue.RemoveAt(0);

                    if (!i.IsFinished)
                    {
                        i.Load();
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        public int Length
        {
            get { return image.Count; }
            set { }
        }
        public string GetFileName(int i)
        {
            if (i < 0 || i > image.Count)
                return "";

            return image[i].GetFileName();
        }

        public void SetFileName(int i, string s)
        {
            image[i].SetFileName(s);
        }

        public void Unload(int i)
        {
            if (image[i] != null)
                image[i].Dispose();
        }

        public void Remove(int i)
        {
            Unload(i);
            image.RemoveAt(i);
        }

		public void Dispose()
        {
            for (int i = 0; i < Length; i++)
            {
                Remove(i);
            }
            image.Clear();
            queue.Clear();
        }
    }
}
