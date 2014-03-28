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
using System.Drawing.Imaging;

namespace ImageGlass.Core
{
    public class ImgFilter
    {
        int rotate;
        bool changed;
        float _brigh, _contr, _gamma;
        public const float DEFAULT_BRIGH = 0f; // [-1..+1] where higher = brighter
        public const float DEFAULT_CONTR = 1f; // [ 0..n ] where higher = stronger
        public const float DEFAULT_GAMMA = 1f; // [ 0..n ] where higher = darker

        public ImgFilter()
        {
            rotate = 0;
            _brigh = DEFAULT_BRIGH;
            _contr = DEFAULT_CONTR;
            _gamma = DEFAULT_GAMMA;
            //Program.dbg("ImgFilter instance created");
        }

        public bool hasChanged()
        {
            //Program.dbg("Query filter changed (" + (changed ? "yes" : "no") + ")");
            bool ret = changed;
            changed = false;
            return ret;
        }

        public void incRotate(int i)
        {
            int oldrot = rotate;
            rotate += i;
            if (rotate < 0) rotate = 3;
            if (rotate > 3) rotate = 0;
            if (oldrot != rotate)
                changed = true;
            //Program.dbg("Set rotation to " + rotate);
        }

        public void ResetRotate()
        {
            rotate = 0;
            changed = false;
        }

        public Image apply(Image img)
        {
            //Program.dbg(System.Threading.Thread.CurrentThread.ManagedThreadId + "");
            //Image im = (Image)img.Clone();
            Image im = img;
            if (rotate == 1) im.RotateFlip(RotateFlipType.Rotate90FlipNone);
            if (rotate == 2) im.RotateFlip(RotateFlipType.Rotate180FlipNone);
            if (rotate == 3) im.RotateFlip(RotateFlipType.Rotate270FlipNone);

            if (brigh != DEFAULT_BRIGH ||
                contr != DEFAULT_CONTR ||
                gamma != DEFAULT_GAMMA)
            {
                using (Graphics g = Graphics.FromImage(im))
                {
                    float b = _brigh;
                    float c = _contr;
                    ImageAttributes derp = new ImageAttributes();
                    derp.SetColorMatrix(new ColorMatrix(new float[][]{
                            new float[]{c, 0, 0, 0, 0},
                            new float[]{0, c, 0, 0, 0},
                            new float[]{0, 0, c, 0, 0},
                            new float[]{0, 0, 0, 1, 0},
                            new float[]{b, b, b, 0, 1}}));
                    derp.SetGamma(_gamma);
                    g.DrawImage(img, new Rectangle(Point.Empty, img.Size),
                        0, 0, img.Width, img.Height, GraphicsUnit.Pixel, derp);
                }
            }
            //Program.dbg("Applied filters");
            return im; // == null? (Image)img.Clone() : im;
        }

        public double brigh
        {
            get { return _brigh; }
            set
            {
                float oldvar = _brigh;
                if (value == 9001) _brigh = DEFAULT_BRIGH;
                else _brigh = (float)Math.Min(Math.Max(value, -0.99), 0.99);
                if (oldvar != _brigh) changed = true;
            }
        }
        public double contr
        {
            get { return _contr; }
            set
            {
                float oldvar = _contr;
                if (value == 9001) _contr = DEFAULT_CONTR;
                else _contr = (float)Math.Min(Math.Max(value, 0.01), 10);
                if (oldvar != _contr) changed = true;
            }
        }
        public double gamma
        {
            get { return _gamma; }
            set
            {
                float oldvar = _gamma;
                if (value == 9001) _gamma = DEFAULT_GAMMA;
                else _gamma = (float)Math.Min(Math.Max(value, 0.01), 10);
                if (oldvar != _gamma) changed = true;
            }
        }

        public void guiTweek()
        {
            try
            {
                /*
                string b = InputBox.Derp(
                    "Please select viewer brightness.\n" +
                    "-100 to 100, higher is brighter.\n" +
                    "Default: " + (DEFAULT_BRIGH * 100),
                    (brigh * 100) + "");

                string c = InputBox.Derp(
                    "Please select viewer contrast.\n" +
                    "0 to 9001, higher is stronger.\n" +
                    "Default: " + (DEFAULT_CONTR * 100),
                    (contr * 100) + "");

                string g = InputBox.Derp(
                    "Input viewer gamma.\n" +
                    "0 to 9001, higher is darker.\n" +
                    "Default: " + (DEFAULT_GAMMA * 100),
                    (gamma * 100) + "");

                if (b != null && c != null && g != null)
                {
                    brigh = (float)(Convert.ToDouble(b) / 100);
                    contr = (float)(Convert.ToDouble(c) / 100);
                    gamma = (float)(Convert.ToDouble(g) / 100);
                }
                 * */
            }
            catch { }
        }
    }
}
