using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIDesignerSettingsViewModel {

        double MinZoomFactor { get; }
        double MaxZoomFactor { get; }
        double ZoomFactor { get; set; }

        double TranslateOffsetX { get; set; }
        double TranslateOffsetY { get; set; }

        bool IsGridVisible { get; set; }
        ICommand ResetDesignerViewCommand { get; }
    }
}
