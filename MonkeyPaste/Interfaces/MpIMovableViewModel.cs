namespace MonkeyPaste {
    public interface MpIMovableViewModel : MpIBoxViewModel {
        int MovableId { get; }
        bool IsMoving { get; set; }
        bool CanMove { get; set; }
    }
}
