
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public enum MpPluginType {
        None = 0,
        Clipboard,
        Input,
        Restful,
        Gui,
        Composite
    }
    public class MpPluginFormat : MpManifestFormat {
        public MpHeadlessPluginFormat headless { get; set; } = null;
        public MpAnalyzerPluginFormat analyzer { get; set; } = null;
        public MpClipboardHandlerFormats oleHandler { get; set; }

        public MpContactFetcherFormat contactFetcher { get; set; }
    }


}
