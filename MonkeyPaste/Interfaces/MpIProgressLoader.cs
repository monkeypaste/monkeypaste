namespace MonkeyPaste {
    public interface MpINotification {
        string Title { get; set; }
        string Body { get; set; }
        string Detail { get; set; }

        string IconResourceKey { get; }

        MpNotificationDialogType DialogType { get; }
    }

    public interface MpIUserActionNotification : MpINotification {
        MpNotificationExceptionSeverityType ExceptionType { get; set; }
    }

    public interface MpIProgressLoader : MpINotification {        
        double PercentLoaded { get; }
    }
}
