using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public enum MpPluginComponentType {
        None = 0,
        Clipboard,
        Input,
        Restful,
        Gui,
        Composite
    }

    public class MpPlugin : MpJsonMessage {
        public string title { get; set; }
        public string description { get; set; }
        public string version { get; set; }
        public string credits { get; set; }
        public string iconUrl { get; set; }

        public List<MpPluginFormatTypes> types { get; set; }


        public List<object> Components { get; set; } = new List<object>();
    }

    public class MpPluginFormatTypes : MpJsonMessage {
        public List<MpAnalyzerPluginFormat> analyzer { get; set; }
    }
    public class MpAnalyzerPluginFormat : MpJsonMessage {
        public string guid { get; set; }
        public List<MpAnalyzerPluginInputFormat> inputTypes { get; set; }
        public List<MpAnalyzerPluginOutputFormat> outputTypes { get; set; }

        public string parametersResourcePath { get; set; }

        public List<MpAnalyzerPresetFormat> presets { get; set; }

        public List<MpAnalyzerPluginRequestFormat> request { get; set; }
        public List<MpAnalyzerPluginResponseFormat> response { get; set; }
    }

    public class MpAnalyzerPluginRequestFormat : MpJsonMessage {
        public List<MpAnalyzerPluginTransactionValueFormat> values { get; set; }
    }

    public class MpAnalyzerPluginResponseFormat : MpJsonMessage {
        public List<MpAnalyzerPluginTransactionValueFormat> values { get; set; }
    }

    public class MpAnalyzerPluginTransactionValueFormat : MpJsonMessage {
        public int enumId { get; set; }
        public string valueType { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
    public class MpAnalyzerPluginInputFormat : MpJsonMessage {
        public bool plaintext { get; set; }
        public bool richtext { get; set; }
        public bool html { get; set; }
        public bool image { get; set; }
        public bool file { get; set; }
    }

    public class MpAnalyzerPluginOutputFormat : MpJsonMessage {
        public bool text { get; set; }
        public bool boundingbox { get; set; }
    }

    public class MpAnalyzerPresetFormat : MpJsonMessage {
        public string label { get; set; }
        public List<MpAnalyzerPresetValueFormat> values { get; set; }
    }

    public class MpAnalyzerPresetValueFormat : MpJsonMessage {
        public int enumId { get; set; }
        public string label { get; set; }
        public string value { get; set; }
    }
}
