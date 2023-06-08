namespace MonkeyPaste {
    public interface MpIFocusMonitor {
        bool IsInputControlFocused { get; }
        bool IsSelfManagedHistoryControlFocused { get; }

        object FocusElement { get; }
    }
}
