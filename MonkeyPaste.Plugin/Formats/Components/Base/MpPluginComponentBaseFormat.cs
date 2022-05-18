using System.Collections.Generic;

namespace MonkeyPaste.Plugin {
    public abstract class MpPluginComponentBaseFormat : MpJsonObject {
        public List<MpPluginParameterFormat> parameters { get; set; } = null;
        public List<MpPluginPresetFormat> presets { get; set; } = null;
    }
}
