using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public abstract class MpParameterHostBaseFormat : MpPluginComponentFormatBase {

        public List<MpParameterFormat> parameters { get; set; } = null;
        public List<MpPresetFormat> presets { get; set; } = null;
    }
}
