
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public enum MpPluginComponentType {
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

        public MpClipboardHandlerFormats clipboardHandler { get; set; }

        public MpContactFetcherFormat contactFetcher { get; set; }

        [JsonIgnore]
        public string RootDirectory { get; set; }
        [JsonIgnore]
        public DateTime manifestLastModifiedDateTime { get; set; }

        [JsonIgnore]
        public object Component { get; set; } = null;


        [JsonIgnore]
        public MpPluginFormat backupCheckPluginFormat { get; set; }

        [JsonIgnore]
        public IEnumerable<MpParameterHostBaseFormat> componentFormats {
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
                if (clipboardHandler != null) {
                    if (clipboardHandler.readers != null) {
                        foreach (var cr in clipboardHandler.readers) {
                            yield return cr;
                        }
                    }
                    if (clipboardHandler.writers != null) {
                        foreach (var cw in clipboardHandler.writers) {
                            yield return cw;
                        }
                    }
                }
            }
        }

    }

    public class MpPluginIoTypeFormat : MpJsonObject {
        public bool isNuget { get; set; } = false;
        public bool isDll { get; set; } = false;
        public bool isCli { get; set; } = false;
        public bool isHttp { get; set; } = false;
    }
    public class MpPluginDependency {
        public MpPluginDependencyType type { get; set; }
        public string name { get; set; }
        public string version { get; set; }
    }


}
