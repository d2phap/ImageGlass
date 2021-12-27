

namespace ImageGlass.PhotoBox;


[Flags]
internal enum AnimationSource
{
    Default = 0,

    PanLeft = 1 << 1,
    PanRight = 1 << 2,
    PanUp = 1 << 3,
    PanDown = 1 << 4,

    ZoomIn = 1 << 5,
    ZoomOut = 1 << 6,
}
