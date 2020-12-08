/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageGlass.Heart {
    public class BitmapBooster {
        private readonly Bitmap src;
        private readonly BitmapData bd;
        private readonly IntPtr dst;
        private readonly int str;

        public BitmapBooster(Bitmap src) {
            this.src = src;
            bd = src.LockBits(new Rectangle(Point.Empty, src.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            dst = bd.Scan0;
            str = bd.Stride;
        }

        public void Dispose() {
            src.UnlockBits(bd);
        }

        public Color Get(int x, int y) {
            unsafe {
                var o = (byte*)dst;
                var ofs = (str * y) + (x * 4);

                return Color.FromArgb(
                    o[ofs + 3],
                    o[ofs + 2],
                    o[ofs + 1],
                    o[ofs + 0]);
            }
        }

        public void Set(int x, int y, Color c) {
            unsafe {
                var o = (byte*)dst;
                var ofs = (str * y) + (x * 4);
                o[ofs + 3] = c.A;
                o[ofs + 2] = c.R;
                o[ofs + 1] = c.G;
                o[ofs + 0] = c.B;
            }
        }

        public void Set(int x, int y, byte alpha) {
            unsafe {
                ((byte*)dst)[(str * y) + (x * 4) + 3] = alpha;
            }
        }

        public static int Min(params int[] values) {
            var ret = values[0];
            for (var a = 1; a < values.Length; a++) {
                ret = Math.Min(ret, values[a]);
            }
            return ret;
        }
        public static int Max(params int[] values) {
            var ret = values[0];
            for (var a = 1; a < values.Length; a++) {
                ret = Math.Max(ret, values[a]);
            }
            return ret;
        }
    }
}
