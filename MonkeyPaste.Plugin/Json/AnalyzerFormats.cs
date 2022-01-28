using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public class MpAnalyzerPluginRequestFormat {
        public string param1Type { get; set; }
        public string param2Type { get; set; }
    }

    public class MpAnalyzerPluginResponseFormat {
        public List<MpAnalyzerPluginResponseValueFormat> values { get; set; }
    }

    public class MpAnalyzerPluginResponseValueFormat {
        public string valueType { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }

    public class MpAnalyzerPluginTransactionValueFormat {
        public int enumId { get; set; }
        public string valueType { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
}
