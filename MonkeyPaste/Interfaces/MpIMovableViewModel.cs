namespace MonkeyPaste {
    public interface MpIMovableViewModel : MpIBoxViewModel {
        bool IsMoving { get; set; }
        bool CanMove { get; set; }
    }
}
