namespace MonkeyPaste {

    public interface MpITextSelectionRange {
        int SelectionStart { get; }
        int SelectionLength { get; }
        string SelectedPlainText { get; set; }
    }
}
