using MonkeyPaste.Common;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public interface MpIUserColorViewModel : MpIViewModel {
        string UserHexColor { get; set; }
    }

    public interface MpIColorPalettePickerViewModel : MpIUserColorViewModel {
        MpIAsyncCommand<object> PaletteColorPickedCommand { get; }
    }


}
