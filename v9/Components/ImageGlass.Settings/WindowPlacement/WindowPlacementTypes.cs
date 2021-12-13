
using System.Runtime.InteropServices;

namespace ImageGlass.Settings;

public enum WindowState
{
    Normal = 1,
    Minimized = 2,
    Maximized = 3,
}


[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct WindowPlacement
{
    public int length;
    public int flags;
    public WindowState showCmd;
    public WpPoint minPosition;
    public WpPoint maxPosition;
    public WpRect normalPosition;

    public WindowPlacement(WpRect normalPos, WindowState state = WindowState.Normal)
    {
        length = Marshal.SizeOf(typeof(WindowPlacement));
        flags = 0;
        showCmd = state;
        minPosition = new WpPoint(-1, -1);
        maxPosition = new WpPoint(-1, -1);

        normalPosition = normalPos;
    }
}


[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct WpPoint
{
    public int X;
    public int Y;

    public WpPoint(int x, int y)
    {
        X = x;
        Y = y;
    }
}


/// <summary>
/// Interaction logic for WindowPlacement
/// </summary>
// RECT structure required by WINDOWPLACEMENT structure
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct WpRect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public WpRect(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
}
