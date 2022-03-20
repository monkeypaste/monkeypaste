namespace MonkeyPaste {
    public interface MpIContentEditorViewModel : MpIViewModel {
        bool GetIsReadOnly();
        void SetIsReadOnly(bool newValue);
    }
}
