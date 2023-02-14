using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIKeyGestureViewModel : MpIViewModel {
        ObservableCollection<MpAvShortcutKeyGroupViewModel> KeyGroups { get; }
    }

    public interface MpAvIShortcutCommandViewModel : MpIViewModel {
        ICommand AssignCommand { get; }
        MpShortcutType ShortcutType { get; }
        MpAvShortcutViewModel ShortcutViewModel { get; }
        string ShortcutKeyString { get; }
    }

    public interface MpIShortcutCommandViewModel : MpIViewModel {
        string ShortcutLabel { get; }
        MpShortcutType ShortcutType { get; }

        int ModelId { get; }

        ICommand ShortcutCommand { get; }
        object ShortcutCommandParameter { get; }
    }

    public interface MpIShortcutCommandCollectionViewModel : MpIViewModel {
        IEnumerable<MpAvIShortcutCommandViewModel> ShortcutCommands { get; }
    }
}
