namespace MonkeyPaste {

    public interface MpITextSelectionRange : MpIViewModel {
        int SelectionStart { get; set; }
        int SelectionEnd { get; set; }
        string SelectedPlainText { get; set; }
        int SelectionLength { get; }

        string Text { get; set; }
    }

    public interface MpIRtfSelectionRange : MpITextSelectionRange {
        string SelectedRichText { get; }
    }
}
