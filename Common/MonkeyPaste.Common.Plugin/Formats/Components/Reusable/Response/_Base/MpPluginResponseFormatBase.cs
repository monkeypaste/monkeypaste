using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public abstract class MpPluginResponseFormatBase : MpPluginMessageFormatBase {
        public string errorMessage { get; set; }
        public string retryMessage { get; set; }

        public string otherMessage { get; set; }

        public List<MpPluginUserNotificationFormat> userNotifications { get; set; } = new List<MpPluginUserNotificationFormat>();

    }

}
