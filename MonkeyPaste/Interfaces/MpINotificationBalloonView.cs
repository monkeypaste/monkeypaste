namespace MonkeyPaste {
    public interface MpINotificationBalloonView : MpIUserControl {        
        void ShowWindow();
        void ShowWindow(object dc);
        void HideWindow();
        void HideWindow(object dc);
    }
}
