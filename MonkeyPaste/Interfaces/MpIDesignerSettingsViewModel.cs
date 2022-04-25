namespace MonkeyPaste {
    public interface MpIDesignerSettingsViewModel {

        double ScaleX { get; set; }
        double ScaleY { get; set; }

        double DesignerWidth { get; set; }
        double DesignerHeight { get; set; }

        double ViewportWidth { get; }
        double ViewportHeight { get; }
    }
}
