using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIKeyGestureViewModel : MpIViewModel {
        ObservableCollection<MpAvShortcutKeyGroupViewModel> KeyGroups { get; }
    }
}
