using MonkeyPaste.Common;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIDesignerSettingsViewModel {

        double MinScale { get; }
        double MaxScale { get; }
        double Scale { get; set; }

        double TranslateOffsetX { get; set; }
        double TranslateOffsetY { get; set; }

        bool IsGridVisible { get; set; }
        ICommand ResetDesignerViewCommand { get; }
    }
}
