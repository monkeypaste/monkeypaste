using Newtonsoft.Json;
using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpClipboardHandlerFormats : MpJsonObject {
        public List<MpClipboardHandlerFormat> readers { get; set; } = new List<MpClipboardHandlerFormat>();
        public List<MpClipboardHandlerFormat> writers { get; set; } = new List<MpClipboardHandlerFormat>();
    }

    public class MpClipboardHandlerFormat : MpParameterHostBaseFormat, MpILabelText {
        [JsonIgnore]
        string MpILabelText.LabelText => displayName;

        public string iconUri { get; set; } = string.Empty;
        public string formatGuid { get; set; } = string.Empty;


        private string _displayName = string.Empty;
        public string displayName {
            get {
                if (string.IsNullOrEmpty(_displayName)) {
                    return formatName;
                }
                return _displayName;
            }
            set => _displayName = value;
        }

        public string formatName { get; set; } = string.Empty;

        public string description { get; set; } = string.Empty;
        public List<MpPluginDependency> dependencies { get; set; }

        public int sortOrderIdx { get; set; }
    }

    public class MpClipboardReaderRequest : MpPluginParameterRequestFormat {
        public List<string> readFormats { get; set; }

        public bool ignoreParams { get; set; }

        public object forcedClipboardDataObject { get; set; } // (optional) this is used to convert drag/drop data 
    }

    public class MpClipboardWriterRequest : MpPluginParameterRequestFormat {
        public object data { get; set; }
        public List<string> writeFormats { get; set; }
        public bool writeToClipboard { get; set; } = true; // (optional) this is used when creating drag/drop data object

    }

    public class MpClipboardReaderResponse : MpPluginResponseFormatBase { }

    public class MpClipboardWriterResponse : MpPluginResponseFormatBase {
        public object processedDataObject { get; set; }
    }
}
