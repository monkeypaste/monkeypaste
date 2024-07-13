namespace MonkeyPaste {
    public interface MpIShortcutGestureLocator {
        string LocateByType(MpShortcutType sct);
        string LocateByCommand(object scvm);
        object LocateSourceByType(MpShortcutType sct);
    }
}
