namespace MonkeyPaste {
    public interface MpINotificationBalloonView : MpIUserControl {        
        void ShowWindow(object dc);
        void HideWindow(object dc);
    }
}
