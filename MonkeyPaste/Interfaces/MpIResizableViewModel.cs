namespace MonkeyPaste {
    public interface MpIResizableViewModel {
        bool IsResizing { get; set; }
        bool CanResize { get; set; }
    }

    public enum MpResizeEdgeType {
        None = 0,
        Left,
        Top,
        Right,
        Bottom
    }
    public interface MpIResizableRectViewModel : MpIResizableViewModel {
        double MinWidth { get; }
        double MinHeight { get; }
        double MaxWidth { get; }
        double MaxHeight { get; }
        double BoundWidth { get; set; }
        double BoundHeight { get; set; }
        double DefaultWidth { get; }
        double DefaultHeight { get; }

        bool AllowResizeWidth { get; }

        bool AllowResizeHeight { get; }

    }
}
