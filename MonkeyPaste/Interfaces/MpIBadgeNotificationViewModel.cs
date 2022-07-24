using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIBadgeNotificationViewModel : MpIViewModel {
        bool HasBadgeNotification { get; set; }
    }
}
