using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public enum MpNotifierStateType {
        None = 0,
        Startup,
        Warning
    }

    public enum MpNotificationType {
        None = 0,
        InvalidPlugin,
        InvalidAction,
        BadHttpRequest,
        DbError,
        LoadComplete
    }

    public enum MpNotificationExceptionSeverityType {
        None = 0,
        Warning, //confirm
        WarningWithOption, //retry/ignore/quit
        Error, //confirm
        ErrorWithOption, //retry/ignore/quit
        ErrorAndShutdown //confirm
    }

    public enum MpNotificationUserActionType {
        None = 0,
        Ignore,
        Retry,
        Shutdown
    }
    
    public class MpNotification {
        public MpNotificationType NotificationType { get; set; } = MpNotificationType.None;
        public MpNotificationExceptionSeverityType SeverityType { get; set; } = MpNotificationExceptionSeverityType.None;
        public MpNotificationUserActionType ResultType { get; set; } = MpNotificationUserActionType.None;

        public string Title { get; set; }
        public string Body { get; set; }

        public double MaxShowMs { get; set; } = -1;
    }
}
