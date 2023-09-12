
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

        [JsonIgnore]
        public string RootDirectory { get; set; }
        [JsonIgnore]
        public DateTime manifestLastModifiedDateTime { get; set; }

        [JsonIgnore]
        public object[] Components { get; set; } = null;


        [JsonIgnore]
        public virtual MpPluginFormat backupCheckPluginFormat { get; set; }

        [JsonIgnore]
        public virtual IEnumerable<MpParameterHostBaseFormat> componentFormats {
            get {
                if (headless != null) {
                    yield return headless;
                }
                if (analyzer != null) {
                    yield return analyzer;
                }
                if (contactFetcher != null) {
                    yield return contactFetcher;
                }
                if (oleHandler != null) {
                    if (oleHandler.readers != null) {
                        foreach (var cr in oleHandler.readers) {
                            yield return cr;
                        }
                    }
                    if (oleHandler.writers != null) {
                        foreach (var cw in oleHandler.writers) {
                            yield return cw;
                        }
                    }
                }
            }
        }

    }


}
