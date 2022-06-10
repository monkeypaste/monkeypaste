namespace MonkeyPaste.Common {
    public interface MpITextRange {
        int Offset { get; }
        int Length { get; }
    }

    public interface MpISizeViewModel {
        double Width { get; }
        double Height { get; }
    }
}
