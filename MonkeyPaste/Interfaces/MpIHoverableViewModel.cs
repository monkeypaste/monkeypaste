
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIHoverableViewModel : MpIViewModel {
        bool IsHovering { get; set; }
    }
}
