namespace MonkeyPaste.Common.Plugin {
    public class MpPluginFormat : MpManifestFormat {
        public MpHeadlessPluginFormat headless { get; set; } = null;
        public MpAnalyzerPluginFormat analyzer { get; set; } = null;
        public MpClipboardHandlerFormats oleHandler { get; set; }

        public MpContactFetcherFormat contactFetcher { get; set; }
    }


}
