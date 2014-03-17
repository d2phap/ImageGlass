using System;
using System.Runtime.InteropServices;

// ReSharper disable NotAccessedField.Global
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace ImageGlass
{
  // Cyotek ImageBox
  // Copyright (c) 2010-2014 Cyotek.
  // http://cyotek.com
  // http://cyotek.com/blog/tag/imagebox

  // Licensed under the MIT License. See imagebox-license.txt for the full text.

  // If you use this control in your applications, attribution, donations or contributions are welcome.

  // ReSharper disable ClassNeverInstantiated.Global
  // ReSharper disable PartialTypeWithSinglePart
  internal partial class NativeMethods // partial for when linking this file into other assemblies
    // ReSharper restore PartialTypeWithSinglePart
    // ReSharper restore ClassNeverInstantiated.Global
  {
    #region Enums

    [Flags]
    public enum SIF
    {
      SIF_RANGE = 0x0001,

      SIF_PAGE = 0x0002,

      SIF_POS = 0x0004,

      SIF_DISABLENOSCROLL = 0x0008,

      SIF_TRACKPOS = 0x0010,

      SIF_ALL = SIF_PAGE | SIF_POS | SIF_RANGE | SIF_TRACKPOS
    }

    #endregion

    #region Constants

    public const int GWL_STYLE = (-16);

    public const int SB_BOTH = 3;

    public const int SB_BOTTOM = 7;

    public const int SB_CTL = 2;

    public const int SB_ENDSCROLL = 8;

    public const int SB_HORZ = 0;

    public const int SB_LEFT = 6;

    public const int SB_LINEDOWN = 1;

    public const int SB_LINELEFT = 0;

    public const int SB_LINERIGHT = 1;

    public const int SB_LINEUP = 0;

    public const int SB_PAGEDOWN = 3;

    public const int SB_PAGELEFT = 2;

    public const int SB_PAGERIGHT = 3;

    public const int SB_PAGEUP = 2;

    public const int SB_RIGHT = 7;

    public const int SB_THUMBPOSITION = 4;

    public const int SB_THUMBTRACK = 5;

    public const int SB_TOP = 6;

    public const int SB_VERT = 1;

    public const int WM_HSCROLL = 0x00000114;

    public const int WM_VSCROLL = 0x00000115;

    public const int WS_BORDER = 0x00800000;

    public const int WS_EX_CLIENTEDGE = 0x200;

    public const int WS_HSCROLL = 0x00100000;

    public const int WS_VSCROLL = 0x00200000;

    #endregion

    #region Private Constructors

    private NativeMethods()
    { }

    #endregion

    #region Class Members

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetScrollInfo(IntPtr hwnd, int bar, [MarshalAs(UnmanagedType.LPStruct)] SCROLLINFO scrollInfo);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    public static extern int SetScrollInfo(IntPtr hwnd, int bar, [MarshalAs(UnmanagedType.LPStruct)] SCROLLINFO scrollInfo, bool redraw);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hwnd, int index, UInt32 newLong);

    #endregion

    #region Nested Types

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SCROLLINFO
    {
      public int cbSize;

      public SIF fMask;

      public int nMin;

      public int nMax;

      public int nPage;

      public int nPos;

      public int nTrackPos;

      public SCROLLINFO()
      {
        cbSize = Marshal.SizeOf(this);
        nPage = 0;
        nMin = 0;
        nMax = 0;
        nPos = 0;
        nTrackPos = 0;
        fMask = 0;
      }
    }

    #endregion
  }

  // ReSharper restore NotAccessedField.Global
  // ReSharper restore FieldCanBeMadeReadOnly.Global
  // ReSharper restore InconsistentNaming
}
