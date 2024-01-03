using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public abstract class MpPluginRequestFormatBase : MpPluginMessageFormatBase { }
    public class MpPluginParameterRequestFormat : MpPluginRequestFormatBase {

        public List<MpParameterRequestItemFormat> items { get; set; }
    }
}
