namespace MonkeyPaste {
    public interface MpIValidatableViewModel : MpIViewModel {
        string ValidationText { get; set; }
        bool IsValid { get; }
    }
}
