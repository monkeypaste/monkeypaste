
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public enum MpPluginComponentType {
        None = 0,
        Clipboard,
        Input,
        Restful,
        Gui,
        Composite
    }

    public class MpPlugin  {
        public string title { get; set; }
        public string description { get; set; }
        public string version { get; set; }
        public string credits { get; set; }
        public string iconUrl { get; set; }

        public List<MpPluginFormatTypes> types { get; set; }

        public List<object> Components { get; set; } = new List<object>();
    }

    public class MpPluginFormatTypes  {
        public List<MpAnalyzerPluginFormat> analyzers { get; set; }
    }
    
}
