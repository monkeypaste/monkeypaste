using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;

namespace MonkeyPaste.Avalonia {
    public class MpPluginWrapper : MpPluginFormat {
        [JsonIgnore]
        public AssemblyLoadContext LoadContext { get; set; }
        [JsonIgnore]
        public string ManifestPath { get; set; }

        [JsonIgnore]
        public string ManifestDir =>
            ManifestPath.IsFile() ? Path.GetDirectoryName(ManifestPath) : null;

        [JsonIgnore]
        public string RootDirectory {
            get {
                if (!ManifestDir.IsDirectory()) {
                    return null;
                }
                if (Path.GetDirectoryName(ManifestDir).EndsWith(guid)) {
                    // when plugin is installed it'll be in wrapper dir using guid as its name
                    return Path.GetDirectoryName(ManifestDir);
                }
                return ManifestDir;
            }
        }

        [JsonIgnore]
        public string CachePath {
            get {
                return Path.Combine(
                    MpPluginLoader.PluginCacheDir,
                    $"{guid}.{MpPluginLoader.MANIFEST_FILE_EXT}");
            }
        }
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
