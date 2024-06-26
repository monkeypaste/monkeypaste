﻿using Avalonia.Layout;

namespace MonkeyPaste.Avalonia {
    public interface MpIPagingScrollViewerViewModel : MpIViewModel {
        bool CanScroll { get; }
        bool IsScrollingIntoView { get; set; }
        bool IsThumbDraggingX { get; set; }
        bool IsThumbDraggingY { get; set; }
        bool IsThumbDragging { get; }
        MpClipTrayLayoutType LayoutType { get; set; }
        Orientation ListOrientation { get; }
        double MaxScrollOffsetX { get; }
        double MaxScrollOffsetY { get; }
        double ScrollOffsetX { get; set; }
        double ScrollOffsetY { get; set; }
        double ScrollVelocityX { get; set; }
        double ScrollVelocityY { get; set; }
        bool IsTouchScrolling { get; set; }
        bool CanTouchScroll { get; }
    }
}