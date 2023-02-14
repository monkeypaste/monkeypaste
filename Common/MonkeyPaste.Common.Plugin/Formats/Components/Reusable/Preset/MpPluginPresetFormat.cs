using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginPresetFormat : MpJsonObject {
        public string guid { get; set; }

        public bool isDefault { get; set; } = false;
        public string iconUri { get; set; }

        public string label { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;

        public List<MpPluginPresetValueFormat> values { get; set; } = new List<MpPluginPresetValueFormat>();
    }

    public class MpPluginPresetValueFormat : MpJsonObject {
        public string paramId { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }
}
