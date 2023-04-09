using Newtonsoft.Json;
using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpClipboardHandlerFormats : MpJsonObject {
        public List<MpClipboardHandlerFormat> readers { get; set; }
        public List<MpClipboardHandlerFormat> writers { get; set; }
    }

    public class MpClipboardHandlerFormat : MpParameterHostBaseFormat, MpILabelText {
        [JsonIgnore]
        string MpILabelText.LabelText => displayName;

        public string iconUri { get; set; }
        public string handlerGuid { get; set; }


        public string displayName { get; set; }
        public string clipboardName { get; set; }

        public string description { get; set; }
        public List<MpPluginDependency> dependencies { get; set; }

        public int sortOrderIdx { get; set; }
    }

    public class MpClipboardReaderRequest : MpPluginRequestFormatBase {
        public List<string> readFormats { get; set; }

        public bool ignoreParams { get; set; }

        public object forcedClipboardDataObject { get; set; } // (optional) this is used to convert drag/drop data 
    }

    public class MpClipboardWriterRequest : MpPluginRequestFormatBase {
        public object data { get; set; }
        public List<string> writeFormats { get; set; }
        public bool writeToClipboard { get; set; } = true; // (optional) this is used when creating drag/drop data object

    }

    public class MpClipboardReaderResponse : MpPluginResponseFormatBase { }

    public class MpClipboardWriterResponse : MpPluginResponseFormatBase {
        public object processedDataObject { get; set; }
    }
}
