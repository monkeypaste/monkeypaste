namespace MonkeyPaste.Common.Plugin {
    public class MpHttpTransactionFormatBase : MpJsonObject {
        public string name { get; set; }

        public MpHttpRequestFormat request { get; set; }

        public MpPluginResponseFormatBase response { get; set; }
    }
}
