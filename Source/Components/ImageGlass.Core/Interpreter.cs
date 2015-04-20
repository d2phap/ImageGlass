using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageGlass.Core
{
    public class Interpreter
    {

        public static Bitmap load(string path)
        {
            if (path.ToLower().EndsWith(".tga")) return Targa(path);
            if (path.ToLower().EndsWith(".gif")) return new Bitmap(path);

            Bitmap bmp = null;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                bmp = new Bitmap(fs, true);
            }

            return bmp;
        }

        // Tested on files created by:
        // - photoshop
        // - mspaint
        // - TF2
        public static Bitmap Targa(string path)
        {
            byte[] src = System.IO.File.ReadAllBytes(path);
            int w = src[0x0D] * 256 + src[0x0C];
            int h = src[0x0F] * 256 + src[0x0E];
            bool flip = src[0x11] == 0x20;
            Bitmap bm = new Bitmap(w, h);
            BitmapData bd = bm.LockBits(new Rectangle(0, 0, w, h),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int sp;
            unsafe
            {
                for (int y = 0; y < h; y++)
                {
                    if (flip) sp = 0x12 + 3 * w * y;
                    else sp = 0x12 + 3 * w * (h - y - 1);
                    byte* dst = (byte*)bd.Scan0 + (bd.Stride * y);
                    for (int n = 0; n < 3 * w; n++)
                    {
                        dst[n] = src[n + sp];
                    }
                }
            }
            bm.UnlockBits(bd);
            return bm;
        }

        public static Bitmap TargaWhatTheFuck(string path)
        {
            byte[] src = System.IO.File.ReadAllBytes(path);
            int w = src[0x0D] * 256 + src[0x0C];
            int h = src[0x0F] * 256 + src[0x0E];
            Bitmap bm = new Bitmap(w, h);
            BitmapData bd = bm.LockBits(new Rectangle(0, 0, w, h),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            unsafe
            {
                int dstptr = 0;
                byte* dst = (byte*)bd.Scan0;
                for (int a = 0x12; a < src.Length; a += 4)
                {
                    //for (int b = 0; b < src[a]; b++)
                    //{
                    dst[dstptr + 0] = src[a + 1];
                    dst[dstptr + 1] = src[a + 2];
                    dst[dstptr + 2] = src[a + 3];
                    dstptr += 3;
                    //}
                }
            }
            bm.UnlockBits(bd);
            return bm;
        }
    }
}
