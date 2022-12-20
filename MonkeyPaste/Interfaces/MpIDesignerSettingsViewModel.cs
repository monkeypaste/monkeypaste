using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpIDesignerSettingsViewModel {

        double Scale { get; set; }

        double TranslateOffsetX { get; set; }
        double TranslateOffsetY { get; set; }
    }
}
