using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpIDraggableViewModel : MpIViewModel {
        bool IsDragging { get; set; }
    }
}
