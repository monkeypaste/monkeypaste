namespace MonkeyPaste.Common.Plugin {
    /// <summary>
    /// The extended runtime subclass of <see cref="MpManifestFormat"/> containing
    /// </summary>
    public class MpPluginFormat : MpManifestFormat {
        public MpHeadlessPluginFormat headless { get; set; } = null;
        public MpAnalyzerPluginFormat analyzer { get; set; } = null;
        public MpClipboardHandlerFormats oleHandler { get; set; }
        public MpContactFetcherFormat contactFetcher { get; set; }
    }
}
