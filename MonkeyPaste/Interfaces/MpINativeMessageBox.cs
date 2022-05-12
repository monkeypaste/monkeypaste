namespace MonkeyPaste {
    public interface MpINativeMessageBox {
        bool ShowOkCancelMessageBox(string title, string message);

        bool? ShowYesNoCancelMessageBox(string title, string message);
    }
}
