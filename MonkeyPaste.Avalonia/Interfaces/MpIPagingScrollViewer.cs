using Avalonia;
using Avalonia.Layout;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    public interface MpIPagingScrollViewerViewModel : MpIViewModel {
        bool CanScroll { get; }
        bool IsScrollingIntoView { get; set; }
        bool IsThumbDraggingX { get; set; }
        bool IsThumbDraggingY { get; set; }
        bool IsThumbDragging { get; }
        MpAvClipTrayLayoutType LayoutType { get; set; }
        Orientation ListOrientation { get; }
        double MaxScrollOffsetX { get; }
        double MaxScrollOffsetY { get; }
        double ScrollOffsetX { get; set; }
        double ScrollOffsetY { get; set; }
        double ScrollVelocityX { get; set; }
        double ScrollVelocityY { get; set; }
    }
}