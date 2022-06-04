using System.Collections.Generic;
using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Plugin {
    public abstract class MpPluginComponentBaseFormat : MpJsonObject {
        public List<MpPluginParameterFormat> parameters { get; set; } = null;
        public List<MpPluginPresetFormat> presets { get; set; } = null;
    }
}
