namespace MonkeyPaste {
    public interface MpIDesignerItemSettingsViewModel : MpIViewModel {
        double ScaleX { get; set; }
        double ScaleY { get; set; }

        double TranslateOffsetX { get; set; }
        double TranslateOffsetY { get; set; }
    }
}
