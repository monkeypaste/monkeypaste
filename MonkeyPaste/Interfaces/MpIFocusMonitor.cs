namespace MonkeyPaste {
    public interface MpIFocusMonitor {
        bool IsSelfManagedHistoryControlFocused { get; }
        bool IsTextInputControlFocused { get; }
        object FocusElement { get; }
    }
}
