using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public class MpClipboardHandlerFormats : MpJsonObject {
        public List<MpClipboardHandlerFormat> handledFormats { get; set; }
    }

    public class MpClipboardHandlerFormat : MpPluginComponentBaseFormat {
        public string displayName { get; set; }
        public string clipboardName { get; set; }

    }
}
