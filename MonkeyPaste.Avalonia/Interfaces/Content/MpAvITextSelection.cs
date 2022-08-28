namespace MonkeyPaste.Avalonia {
    public interface MpAvITextSelection : MpAvITextRange {
        void Select(MpAvITextPointer start, MpAvITextPointer end);
    }
}
