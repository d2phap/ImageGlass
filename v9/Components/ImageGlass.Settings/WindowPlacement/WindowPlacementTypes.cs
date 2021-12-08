
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
    public Point minPosition;
    public Point maxPosition;
    public Rect normalPosition;

    public WindowPlacement(Rect normalPos, WindowState state = WindowState.Normal)
    {
        this.length = Marshal.SizeOf(typeof(WindowPlacement));
        this.flags = 0;
        this.showCmd = state;
        this.minPosition = new Point(-1, -1);
        this.maxPosition = new Point(-1, -1);

        this.normalPosition = normalPos;
    }
}


[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct Point
{
    public int X;
    public int Y;

    public Point(int x, int y)
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
public struct Rect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public Rect(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
}
