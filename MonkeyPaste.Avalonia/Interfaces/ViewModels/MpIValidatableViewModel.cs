namespace MonkeyPaste.Avalonia {
    public interface MpIValidatableViewModel : MpIViewModel {
        string ValidationText { get; set; }
        bool IsValid { get; }
    }
}
