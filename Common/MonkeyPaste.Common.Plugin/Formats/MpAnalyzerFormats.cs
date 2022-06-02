
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {

    public class MpAnalyzerPluginRequestFormat : MpJsonObject {
        public List<MpAnalyzerPluginRequestItemFormat> items { get; set; } = new List<MpAnalyzerPluginRequestItemFormat>();

        public MpPortableDataObject data { get; set; }
    }

    public class MpAnalyzerPluginRequestItemFormat : MpJsonObject {
        public int paramId { get; set; } = 0;
        public string value { get; set; } = string.Empty;
    }
    public class MpAnalyzerPluginFormat : MpPluginComponentBaseFormat {
        public MpHttpTransactionFormat http { get; set; }

        public MpAnalyzerPluginInputFormat inputType { get; set; } = null;
        public MpAnalyzerPluginOutputFormat outputType { get; set; } = null;
    }

    public class MpAnalyzerPluginInputFormat : MpJsonObject {
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool file { get; set; } = false;
    }

    public class MpAnalyzerPluginOutputFormat : MpJsonObject {
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool file { get; set; } = false;
        public bool imageToken { get; set; } = false;
        public bool textToken { get; set; } = false;
    }
}
