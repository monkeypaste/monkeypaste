using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginDeferredParameterValueResponseFormat : MpPluginResponseFormatBase {
        public List<MpPluginParameterValueFormat> Values { get; set; }
    }
    public class MpPluginContactFetchRequestFormat : MpPluginRequestFormatBase { }
    public class MpPluginContactFetchResponseFormat : MpPluginResponseFormatBase {
        public IEnumerable<MpIContact> Contacts { get; set; }
    }
}
