namespace MonkeyPaste.Common.Plugin {
    #region Request

    public class MpHttpBodyFormat : MpJsonObject {
        public string mode { get; set; }
        public string raw { get; set; }
        public string encoding { get; set; } = "UTF8";
        public string mediaType { get; set; }
    }

    #endregion

    #region Response 

    //public class MpHttpResponseFormat {


    //    public MpPluginResponseContentMap responseToContentMap { get; set; }
    //    public List<MpAnalyzerPluginTextResponseFormat> text { get; set; }
    //    public List<MpAnalyzerPluginTextTokenResponseValueFormat> textTokens { get; set; }
    //    public List<MpAnalyzerPluginImageTokenResponseValueFormat> imageTokens { get; set; }
    //}

    #endregion
}
