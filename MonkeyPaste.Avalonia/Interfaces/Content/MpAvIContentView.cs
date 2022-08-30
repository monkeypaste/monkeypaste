namespace MonkeyPaste.Avalonia {

    public interface MpAvIContentView {
        MpAvTextSelection Selection { get; }
        MpAvIContentDocument Document { get; }
        void UpdateSelection(int index,int length, bool isFromEditor);
        void SelectAll();
    }
}
