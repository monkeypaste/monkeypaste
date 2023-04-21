using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIShortcutCommandViewModel : MpIViewModel {
        MpShortcutType ShortcutType { get; }
        string KeyString { get; }
        ICommand ShortcutCommand { get; }
        object ShortcutCommandParameter { get; }
    }

    public interface MpIShortcutGestureLocator {
        string LocateByType(MpShortcutType sct);
        string LocateByCommand(MpIShortcutCommandViewModel scvm);
    }
}
