using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ImageGlass.Theme
{
    public static class DPIScaling
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        public const int WM_DPICHANGED = 0x02E0;

        public static short LOWORD(int number)
        {
            return (short)number;
        }

        public static int CalculateCurrentDPI(Form f)
        {
            float dx, dy;
            Graphics g = f.CreateGraphics();
            try
            {
                dx = g.DpiX;
                dy = g.DpiY;
            }
            finally
            {
                g.Dispose();
            }

            return (int)dx;
        }

        public static void HandleDpiChanged(int oldDpi, int currentDpi, Form f)
        {
            if (oldDpi != 0)
            {
                float scaleFactor = (float)currentDpi / oldDpi;

                //the default scaling method of the framework
                f.Scale(new SizeF(scaleFactor, scaleFactor));
                
                //fonts are not scaled automatically so we need to handle this manually
                ScaleFontForControl(f, scaleFactor);
            }
        }

        private static void ScaleFontForControl(Control control, float factor)
        {
            control.Font = new Font(control.Font.FontFamily,
                   control.Font.Size * factor,
                   control.Font.Style);

            foreach (Control child in control.Controls)
            {
                ScaleFontForControl(child, factor);
            }
        }

    }
}
