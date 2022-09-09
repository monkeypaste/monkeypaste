namespace MonkeyPaste.Avalonia {

    public interface MpAvIContentView {
        bool CanDrag { get; }
        MpAvTextSelection Selection { get; }
        MpAvIContentDocument Document { get; }
        void UpdateSelection(int index,int length, bool isFromEditor);
        void SelectAll();
    }
}
