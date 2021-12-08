
using System.Runtime.InteropServices;

namespace ImageGlass.UI.WinApi
{
    public class WinColors
    {
        [DllImport("dwmapi.dll", EntryPoint = "#127", PreserveSig = false)]
        private static extern void DwmGetColorizationParameters(out DWM_COLORIZATION_PARAMS parameters);

        private struct DWM_COLORIZATION_PARAMS
        {
            public uint clrColor;
            public uint clrAfterGlow;
            public uint nIntensity;
            public uint clrAfterGlowBalance;
            public uint clrBlurBalance;
            public uint clrGlassReflectionIntensity;
            public bool fOpaque;
        }

        private static Color GetAccentColor(bool includeAlpha = false)
        {
            var temp = new DWM_COLORIZATION_PARAMS();
            DwmGetColorizationParameters(out temp);

            var bytes = BitConverter.GetBytes(temp.clrAfterGlow);
            var alpha = includeAlpha ? bytes[3] : (byte)255;
            var color = Color.FromArgb(alpha, bytes[2], bytes[1], bytes[0]);

            return color;
        }


        public static Color AccentBrush => GetAccentColor();

        public static Color AccentAlphaBrush => GetAccentColor(true);


    }
}
