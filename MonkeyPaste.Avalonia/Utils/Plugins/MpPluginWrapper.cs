using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpPluginWrapper : MpPluginFormat {
        [JsonIgnore]
        public MpPluginAssemblyLoadContext LoadContext { get; set; }

        [JsonIgnore]
        public string RootDirectory { get; set; }
        [JsonIgnore]
        public DateTime manifestLastModifiedDateTime { get; set; }

        [JsonIgnore]
        public IEnumerable<MpIPluginComponentBase> Components { get; set; } = null;

        [JsonIgnore]
        public bool IsManifestChangedFromBackup { get; set; }

        [JsonIgnore]
        public virtual MpPluginWrapper backupCheckPluginFormat { get; set; }


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
