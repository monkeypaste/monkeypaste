namespace MonkeyPaste.Avalonia {

    public interface MpAvIContentView {
        bool IsViewLoaded { get; }
        bool IsContentUnloaded { get; set; }
        MpAvTextSelection Selection { get; }
        MpAvIContentDocument Document { get; }
        void UpdateSelection(int index,int length,string text, bool isFromEditor, bool isChangeBegin);
        void SelectAll();
        void DeselectAll();
    }
}
