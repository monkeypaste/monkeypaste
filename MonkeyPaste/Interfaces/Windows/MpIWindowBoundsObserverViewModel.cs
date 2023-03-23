using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpIWindowBoundsObserverViewModel : MpIViewModel {
        MpRect Bounds { get; set; }
        MpRect LastBounds { get; set; }
    }
}
