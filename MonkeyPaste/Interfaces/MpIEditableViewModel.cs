namespace MonkeyPaste {
    public interface MpIEditableViewModel : MpIViewModel {
        bool IsReadOnly { get; set; }
    }
}
