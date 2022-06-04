using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common {
    public class MpClipboardHandlerFormats : MpJsonObject {
        public List<MpClipboardHandlerFormat> handledFormats { get; set; }
    }

    public class MpClipboardHandlerFormat : MpPluginComponentBaseFormat {
        
        public string iconUrl { get; set; }
        public string handlerGuid { get; set; }
        public string displayName { get; set; }
        public string clipboardName { get; set; }

    }
}
