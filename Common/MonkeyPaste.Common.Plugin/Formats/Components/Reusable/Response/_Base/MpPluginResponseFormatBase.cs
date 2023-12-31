using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public abstract class MpPluginResponseFormatBase : MpPluginMessageFormatBase {
        public string errorMessage { get; set; }
        public string retryMessage { get; set; }

        public string otherMessage { get; set; }

        public Dictionary<object, string> invalidParams { get; set; } = new();
        public List<MpPluginUserNotificationFormat> userNotifications { get; set; } = new();

    }

}
