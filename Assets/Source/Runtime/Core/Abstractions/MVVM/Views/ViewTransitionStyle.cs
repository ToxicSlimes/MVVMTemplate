using System;

namespace Core.Abstractions.MVVM.Views
{
    [Flags]
    public enum ViewTransitionStyle
    {
        None = 0,
        Fade = 1 << 0,
        SlideUp = 1 << 1,
        SlideDown = 1 << 2,
        SlideLeft = 1 << 3,
        SlideRight = 1 << 4,
        Scale = 1 << 5,
        Bounce = 1 << 6
    }
}
