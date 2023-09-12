using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public abstract class MpPluginMessageFormatBase : MpJsonObject {
        public Dictionary<string, object> dataObjectLookup { get; set; }
    }
}
