namespace MonkeyPaste {
    public interface MpIPlatformToastNotification {
        void ShowToast(string title, string text, object icon, string accentHexColor);
    }
}
