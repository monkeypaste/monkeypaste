using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public abstract class MpParameterHostBaseFormat : MpJsonObject {

        public List<MpParameterFormat> parameters { get; set; } = null;
        public List<MpPluginPresetFormat> presets { get; set; } = null;
    }
}
