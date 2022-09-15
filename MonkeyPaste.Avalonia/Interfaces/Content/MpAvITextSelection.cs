namespace MonkeyPaste.Avalonia {
    public interface MpAvITextSelection : MpAvITextRange {

        string Text { get; set; }
        void Select(MpAvITextPointer start, MpAvITextPointer end);
    }
}
