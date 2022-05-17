using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public class MpClipboardHandlerFormats : MpJsonObject {
        public List<MpClipboardHandlerFormat> handledFormats { get; set; }
    }

    public class MpClipboardHandlerFormat : MpJsonObject {
        public string displayName { get; set; }
        public string clipboardName { get; set; }

        public List<MpPluginParameterFormat> parameters { get; set; } = null;

        public List<MpPluginPresetFormat> presets { get; set; } = null;
    }
}
