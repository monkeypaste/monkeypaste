using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpISupportHeadlessClipboardComponentFormat : MpIHeadlessComponentFormatBase {
        MpClipboardHandlerFormats GetFormats();
    }

    public interface MpIOlePluginComponent : MpIPluginComponentBase {
        Task<MpOlePluginResponse> ProcessOleRequestAsync(MpOlePluginRequest request);
    }

    public interface MpIOleReaderComponent : MpIOlePluginComponent {
    }

    public interface MpIOleWriterComponent : MpIOlePluginComponent {
    }

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

    public class MpOlePluginRequest : MpPluginParameterRequestFormat {
        public bool isDnd { get; set; }
        //public IDataObject oleData { get; set; }
        public List<string> formats { get; set; }
        public bool ignoreParams { get; set; }
    }
    public class MpOlePluginResponse : MpPluginResponseFormatBase {
        //public IDataObject oleData { get; set; }
    }
}
