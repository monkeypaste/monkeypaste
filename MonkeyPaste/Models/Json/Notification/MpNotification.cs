using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpNotificationFormat : MpJsonObject {
        #region Properties

        public MpNotificationType NotificationType { get; set; }

        public int MaxShowTimeMs { get; set; } = -1;
        public string Title { get; set; }
        public string Body { get; set; }
        public string Detail { get; set; }

        public string IconSourceStr { get; set; }

        #endregion
    }
}
