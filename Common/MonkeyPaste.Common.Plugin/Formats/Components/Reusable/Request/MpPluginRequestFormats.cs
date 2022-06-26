using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginRequestItemFormat : MpJsonObject, MpIParameterKeyValuePair {
        public int paramId { get; set; } = 0;
        public string value { get; set; } = string.Empty;
    }

    public class MpPluginRequestFormatBase : MpJsonObject {
        public List<MpPluginRequestItemFormat> items { get; set; }
    }
}
