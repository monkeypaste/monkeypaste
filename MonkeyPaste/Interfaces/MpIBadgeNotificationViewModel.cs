using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIBadgeNotifier {
        //bool HasBadgeNotification { get; set; }
        int NotificationCount { get; }
    }
}
