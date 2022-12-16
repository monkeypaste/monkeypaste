namespace MonkeyPaste {
    public interface MpILabelText {
        string LabelText { get; }
    }
    public interface MpILabelTextViewModel : MpILabelText, MpIViewModel { }
}
