namespace MonkeyPaste {
    public interface MpITextSelectionRange {
        int SelectionStart { get; set; }
        int SelectionLength { get; set; }
        bool IsAllSelected { get; set; }
    }
}
