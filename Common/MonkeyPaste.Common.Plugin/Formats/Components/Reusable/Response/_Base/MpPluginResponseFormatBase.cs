using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginResponseFormatBase : MpJsonObject {
        public string errorMessage { get; set; }
        public string retryMessage { get; set; }

        public string otherMessage { get; set; }

        public List<MpPluginUserNotificationFormat> userNotifications { get; set; } = new List<MpPluginUserNotificationFormat>();

        //public MpPluginResponseNewContentFormat newContentItem { get; set; }
        //public List<MpPluginResponseAnnotationFormat> annotations { get; set; } = new List<MpPluginResponseAnnotationFormat>();

        //public MpPortableDataObject dataObject { get; set; }
        public Dictionary<string, object> dataObject { get; set; }
    }

}
