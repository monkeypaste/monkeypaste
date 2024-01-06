using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpPresetFormat  {
        public string guid { get; set; }

        public bool isDefault { get; set; } = false;
        public string iconUri { get; set; }

        public string label { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;

        public List<MpPluginPresetValueFormat> values { get; set; } = new List<MpPluginPresetValueFormat>();
    }

    public class MpPluginPresetValueFormat  {
        public string paramId { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public MpPluginPresetValueFormat() { }
        public MpPluginPresetValueFormat(string paramId, string value) {
            this.paramId = paramId;
            this.value = value;
        }
    }
}
