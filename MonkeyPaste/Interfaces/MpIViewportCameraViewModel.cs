namespace MonkeyPaste {
    public interface MpIViewportCameraViewModel {
        double ScaleX { get; set; }
        double ScaleY { get; set; }

        double DesignerWidth { get; set; }
        double DesignerHeight { get; set; }

        double ViewportWidth { get;  }
        double ViewportHeight { get; }
    }
}
