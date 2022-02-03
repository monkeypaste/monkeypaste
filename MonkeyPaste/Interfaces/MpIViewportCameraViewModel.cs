namespace MonkeyPaste {
    public interface MpIViewportCameraViewModel {
        bool IsPanning { get; set; }
        bool CanPan { get; }

        bool IsZooming { get; set; }
        bool CanZoom { get; }

        double CameraX { get; set; }
        double CameraY { get; set; }

        double MaxCameraZoomFactor { get; }
        double MinCameraZoomFactor { get; }
        double CameraZoomFactor { get; set; }

        double DesignerWidth { get; set; }
        double DesignerHeight { get; set; }

        double ViewportWidth { get; set; }
        double ViewportHeight { get; set; }
    }
}
