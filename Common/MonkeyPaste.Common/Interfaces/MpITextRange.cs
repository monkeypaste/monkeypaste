namespace MonkeyPaste.Common {
    public interface MpITextRange {
        int Offset { get; }
        int Length { get; }
    }
    public interface MpIDocumentComponent {
        object Document { get; }
        bool IsInSameDocument(MpIDocumentComponent dtr);
    }
}
