using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageGlass.Core
{
    public class BitmapBooster
    {

        Bitmap src;
        BitmapData bd;
        IntPtr dst;
        int str;

        public BitmapBooster(Bitmap src)
        {
            this.src = src;
            bd = src.LockBits(new Rectangle(Point.Empty, src.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            dst = bd.Scan0;
            str = bd.Stride;
        }

        public void Dispose()
        {
            src.UnlockBits(bd);
        }

        public Color Get(int x, int y)
        {
            unsafe
            {
                byte* o = (byte*)dst;
                int ofs = str * y + x * 4;

                return Color.FromArgb(
                    o[ofs + 3],
                    o[ofs + 2],
                    o[ofs + 1],
                    o[ofs + 0]);
            }
        }

        public void Set(int x, int y, Color c)
        {
            unsafe
            {
                byte* o = (byte*)dst;
                int ofs = str * y + x * 4;
                o[ofs + 3] = c.A;
                o[ofs + 2] = c.R;
                o[ofs + 1] = c.G;
                o[ofs + 0] = c.B;
            }
        }

        public void Set(int x, int y, byte alpha)
        {
            unsafe
            {
                ((byte*)dst)[str * y + x * 4 + 3] = alpha;
            }
        }

        public static int Min(params int[] values)
        {
            int ret = values[0];
            for (int a = 1; a < values.Length; a++)
            {
                ret = Math.Min(ret, values[a]);
            }
            return ret;
        }
        public static int Max(params int[] values)
        {
            int ret = values[0];
            for (int a = 1; a < values.Length; a++)
            {
                ret = Math.Max(ret, values[a]);
            }
            return ret;
        }
    }
}
