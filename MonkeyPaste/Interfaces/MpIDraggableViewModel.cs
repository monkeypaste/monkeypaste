namespace MonkeyPaste {
    public interface MpIDraggable {
        bool IsDragging { get; set; }
    }
    public interface MpIDraggableViewModel : MpIDraggable, MpIViewModel {
    }
}
