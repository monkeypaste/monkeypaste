using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Common {
    public class MpHttpTransactionFormatBase {
        public string name { get; set; }

        public MpHttpRequestFormat request { get; set; }

        public MpPluginResponseFormatBase response { get; set; }
    }
    public class MpHttpAnalyzerTransactionFormat : MpHttpTransactionFormatBase {
        public new MpAnalyzerPluginResponseFormat response { get; set; }
    }
}
